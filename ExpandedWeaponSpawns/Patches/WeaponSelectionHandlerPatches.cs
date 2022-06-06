using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using System.Reflection.Emit;

namespace ExpandedWeaponSpawns
{
    class WeaponSelectionHandlerPatches
    {
        public static void Patch(Harmony harmonyInstance)
        {
            var getWeaponByIndexMethod = AccessTools.Method(typeof(WeaponSelectionHandler), "GetWeaponByIndex");
            var getWeaponByIndexMethodPrefix = new HarmonyMethod(typeof(WeaponSelectionHandlerPatches).GetMethod(nameof(GetWeaponByIndexMethodPrefix)));  
            harmonyInstance.Patch(getWeaponByIndexMethod, prefix: getWeaponByIndexMethodPrefix);

            var getRandomWeaponIndexMethod = AccessTools.Method(typeof(WeaponSelectionHandler), "GetRandomWeaponIndex");
            var getRandomWeaponIndexMethodTranspiler = new HarmonyMethod(typeof(WeaponSelectionHandlerPatches).GetMethod(nameof(GetRandomWeaponIndexMethodTranspiler)));  
            harmonyInstance.Patch(getRandomWeaponIndexMethod, transpiler: getRandomWeaponIndexMethodTranspiler);

            var generateWeaponRarityArrayMethod = AccessTools.Method(typeof(WeaponSelectionHandler), "GenerateWeaponRarityArray");
            var generateWeaponRarityArrayMethodPostfix= new HarmonyMethod(typeof(WeaponSelectionHandlerPatches).GetMethod(nameof(GenerateWeaponRarityArrayMethodPostfix)));  
            harmonyInstance.Patch(generateWeaponRarityArrayMethod, postfix: generateWeaponRarityArrayMethodPostfix);
        }

        public static bool GetWeaponByIndexMethodPrefix(ref int index, ref GameObject __result, WeaponSelectionHandler __instance)
        {
            var weaponID = index;
            var weaponPickupObjs = Traverse.Create(__instance).Field("m_WeaponObjects").GetValue<GameObject[]>();

            GameObject weaponPickup = weaponPickupObjs[weaponID];
            if (!Array.Exists(ExpandedWeaponsMenu.specialIndexes, element => element == weaponID))
            {
                if (weaponPickup)
                {
                    __result = weaponPickup;
                    return false;
                }
            }
            else
            {
                weaponPickup = weaponPickupObjs[2]; // Get sword weapon (Gun2) to use as placeholder for weapons whose WeaponDrop is another weapon (ex: holyminigun's is the normal minigun's)

                var weaponPickupComponent = weaponPickup.GetComponent<WeaponPickUp>();
                weaponPickupComponent.id = weaponID; // Switch the weapon id to that of the special weapon (lava whip or holyminigun)
                weaponPickupComponent.ChangeToPresent();

                // Readd ConstantForce component as ChangeToPresent() destroys it and so later on an error will occur if the player attempts to throw the special weapon
                if (!weaponPickup.GetComponent<ConstantForce>()) weaponPickup.AddComponent<ConstantForce>();

                __result = weaponPickup;
                return false;
            }    

            __result = UnityEngine.Object.FindObjectOfType<Weapons>().transform.GetChild(weaponID).GetComponent<Weapon>().weaponDrop;
            return false;
        }

        // TODO: Add specific check for instruction at i + 1
        public static IEnumerable<CodeInstruction> GetRandomWeaponIndexMethodTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            FieldInfo m_WeaponObjectsField = AccessTools.Field(typeof(WeaponSelectionHandler), "m_WeaponObjects");

            for (int i = 0; i < instructionList.Count; i++)
            {
                if (instructionList[i].LoadsField(m_WeaponObjectsField))
                {
                    instructionList.RemoveAt(i);
                    instructionList[i + 1] = CodeInstruction.Call(typeof(WeaponSelectionHandler), "GetWeaponByIndex");
                }
            }

            return instructionList.AsEnumerable();
        }

        public static void GenerateWeaponRarityArrayMethodPostfix(WeaponSelectionHandler __instance)
        {
            var weaponRaritiesArrayInstance = Traverse.Create(__instance).Field("m_WeaponRaritiesArray");
            var weaponRaritiesList = weaponRaritiesArrayInstance.GetValue<List<byte>>();

            foreach (var weapon in ExpandedWeaponsMenu.UnusedWeapons)
            {
                if (weapon.IsActive) 
                    for (int i = 0; i < weapon.Rarity; i++) 
                        weaponRaritiesList.Add(weapon.Index);
            }

            weaponRaritiesArrayInstance.SetValue(weaponRaritiesList);
        }
    }
}
    
