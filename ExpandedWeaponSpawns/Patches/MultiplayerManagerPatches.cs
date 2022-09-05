using System.Collections.Generic;
using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace ExpandedWeaponSpawns.Patches;

public class MultiplayerManagerPatches
{
    private static readonly GameObject[] AIWeapons = new GameObject[3];
    private const string TargetWeapons = "7 Ice Gun, 10 shotgun, 4 Knife";
    
    public static void Patch(Harmony harmonyInstance)
    {
        var onPlayerSpawnedMethod = AccessTools.Method(typeof(MultiplayerManager), "OnPlayerSpawned");
        var onPlayerSpawnedMethodPostfix = new HarmonyMethod(typeof(MultiplayerManagerPatches)
            .GetMethod(nameof(OnPlayerSpawnedMethodPostfix)));
        harmonyInstance.Patch(onPlayerSpawnedMethod, postfix: onPlayerSpawnedMethodPostfix);
        
        var spawnPlayerDummyMethod = AccessTools.Method(typeof(MultiplayerManager), "SpawnPlayerDummy");
        var spawnPlayerDummyMethodPostfix = new HarmonyMethod(typeof(MultiplayerManagerPatches)
            .GetMethod(nameof(SpawnPlayerDummyMethodPostfix)));
        harmonyInstance.Patch(spawnPlayerDummyMethod, postfix: spawnPlayerDummyMethodPostfix);
    }

    public static void OnPlayerSpawnedMethodPostfix(ref byte[] data, ref GameObject ___m_PlayerPrefab, ref List<Controller> ___m_Players)
    {
        var playerID = data[0];
        
        AssignAIWeapons(___m_PlayerPrefab.name, ___m_Players[playerID].transform.Find("Weapons"));
        ExpandedWeaponsMenu.IsDefaultCharacter = ___m_PlayerPrefab.name == "Character";
    }

    public static void SpawnPlayerDummyMethodPostfix(ref byte i, ref GameObject ___m_PlayerPrefab, ref List<Controller> ___m_Players)
    {
        var playerID = i;
        
        AssignAIWeapons(___m_PlayerPrefab.name, ___m_Players[playerID].transform.Find("Weapons"));
    }

    private static void AssignAIWeapons(string playerPrefabName, Transform weaponsTransform)
    {
        if (playerPrefabName != "Character") return;
        if (AIWeapons[0] == null) FillAIWeapons();

        foreach (var weapon in AIWeapons) 
            Object.Instantiate(weapon, weaponsTransform);
    }

    private static void FillAIWeapons()
    {
        var boltWeapons = Resources.FindObjectsOfTypeAll<HoardHandler>()[0].character
            .GetComponentInChildren<Weapons>().transform;

        var curIndex = 0;
        foreach (Transform weapon in boltWeapons)
        {
            if (!TargetWeapons.Contains(weapon.name)) continue;
            
            AIWeapons[curIndex] = weapon.gameObject;
            curIndex++;
        }
    }
}