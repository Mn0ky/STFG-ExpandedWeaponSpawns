using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using HarmonyLib;
using System.IO;
using BepInEx;
using SimpleJSON;

namespace ExpandedWeaponSpawns
{
    class ExpandedWeaponsMenu : MonoBehaviour
    {
        public static UnusedWeaponInfo[] UnusedWeapons = new UnusedWeaponInfo[]
        {
            new UnusedWeaponInfo {Index = 8, Rarity = 15, Name = "Shield"},
            new UnusedWeaponInfo {Index = 9, Rarity = 15, Name = "Fan"},
            new UnusedWeaponInfo {Index = 11, Rarity = 20, Name = "Ball"}, 
            new UnusedWeaponInfo {Index = 13, Rarity = 20, Name = "BowAndArrow"},
            new UnusedWeaponInfo {Index = 15, Rarity = 5, Name = "Lightsaber"},
            new UnusedWeaponInfo {Index = 18, Rarity = 10, Name = "MinigunTiny"},  
            new UnusedWeaponInfo {Index = 28, Rarity = 10, Name = "LaserPlanter"},  
            new UnusedWeaponInfo {Index = 29, Rarity = 12, Name = "HolySword"},  
            new UnusedWeaponInfo {Index = 30, Rarity = 15, Name = "GodMinigun"},  
            new UnusedWeaponInfo {Index = 34, Rarity = 10, Name = "LavaWhip"},  
        };

        public static readonly int[] specialIndexes = new int[] { 30, 34 };

        private bool[] activeWeaponComparison = new bool[UnusedWeapons.Length];

        public static KeyCode MenuKey1;
        public static KeyCode MenuKey2;
        public static bool SingleKeyKeybind;
        public static readonly string WeaponStatesPath = Path.Combine(Application.persistentDataPath, "WeaponStatesData.json");

        private readonly MethodInfo _generateRaritiesMethod = AccessTools.Method(typeof(WeaponSelectionHandler), "GenerateWeaponRarityArray");
        private bool _showMenu;
        private readonly int _windowID = 99;
        private Rect _menuRect = new(300, 300, 600f, 130f);

        void Update()
        {
            if (Input.GetKey(MenuKey1) && Input.GetKeyDown(MenuKey2) || Input.GetKeyDown(MenuKey1) && SingleKeyKeybind) _showMenu = !_showMenu;

            for (int i = 0; i < UnusedWeapons.Length; i++)
            {
                UnusedWeaponInfo weapon = UnusedWeapons[i];
                if (weapon.IsActive != activeWeaponComparison[i])
                {
                    activeWeaponComparison[i] = weapon.IsActive;
                    GenerateRarities();
                    SaveWeaponStates();
                    Debug.Log($"Toggled: {weapon.Name} | State: {weapon.IsActive}");
                }
            }
        }

        void OnGUI()
        {
            if (_showMenu) _menuRect = GUILayout.Window(_windowID, _menuRect, WeaponSelectorWindow, "Special Weapons");
        }

        void WeaponSelectorWindow(int id)
        {
            GUILayout.BeginVertical();
            GUILayout.Space(40);
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            int totalItemsInVerticalGroup = 2;
            int curItemsInVertGroup = 0;

            for (int i = 0; i < UnusedWeapons.Length; i++)
            {
                if (curItemsInVertGroup == totalItemsInVerticalGroup)
                {
                    curItemsInVertGroup = 0;
                    GUILayout.EndVertical();
                    GUILayout.Space(30);
                }

                if (curItemsInVertGroup == 0) GUILayout.BeginVertical();

                var weapon = UnusedWeapons[i];
                UnusedWeapons[i].IsActive = GUILayout.Toggle(UnusedWeapons[i].IsActive, weapon.Name);
                GUILayout.Space(30);
                curItemsInVertGroup++;
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }

        void GenerateRarities() => _generateRaritiesMethod.Invoke(GameManager.Instance.GetComponent<WeaponSelectionHandler>(), null);

        public static void LoadWeaponStates()
        {
            if (File.Exists(WeaponStatesPath))
            {
                JSONArray weaponStatesJSON = JSONNode.Parse(File.ReadAllText(WeaponStatesPath)).AsArray;
                foreach (var weaponInfoObj in weaponStatesJSON)
                {
                    JSONObject weaponInfoJSON = weaponInfoObj.Value.AsObject;
                    int weaponIndex = weaponInfoJSON[0];
                    bool weaponState = weaponInfoJSON[2];

                    var weapon = UnusedWeapons.First(weapon => weaponIndex == weapon.Index);
                    weapon.IsActive = weaponState;
                }
            }
        }

        void SaveWeaponStates()
        {
            JSONArray weaponStatesJSON = CreateWeaponStatesJSON();
            File.WriteAllText(WeaponStatesPath, weaponStatesJSON.ToString());
        }

        JSONArray CreateWeaponStatesJSON()
        {
            JSONObject weaponStatesJSONObj = new();
            JSONArray weaponStatesJSON = new();
            foreach (var weaponInfo in UnusedWeapons)
            {
                JSONObject weaponInfoJSON = new();
                weaponInfoJSON.Add("Index", (int)weaponInfo.Index);
                weaponInfoJSON.Add("Rarity", weaponInfo.Rarity);
                weaponInfoJSON.Add("IsActive", weaponInfo.IsActive);

                weaponStatesJSON.Add(weaponInfoJSON);
            }

            return weaponStatesJSON;
        }
    }
}
