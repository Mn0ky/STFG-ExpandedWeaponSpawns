using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using HarmonyLib;
using System.IO;
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
            new UnusedWeaponInfo {Index = 34, Rarity = 10, Name = "LavaWhip"}
        };

        public static readonly int[] specialIndexes = new int[] { 30, 34 };

        public static KeyCode MenuKey1;
        public static KeyCode MenuKey2;
        public static bool SingleKeyKeybind;
        public static readonly string WeaponStatesPath = Path.Combine(Application.persistentDataPath, "WeaponStatesData.json");

        private readonly MethodInfo _generateRaritiesMethod = AccessTools.Method(typeof(WeaponSelectionHandler), "GenerateWeaponRarityArray");
        private bool _showMenu;
        private readonly int _windowID = 99;
        private Rect _menuRect = new(300, 300, 600f, 130f);

        private bool _showRaritiesMenu;
        private readonly int _raritiesWindowID = 98;
        private static readonly float _raritiesMenuWidth = Screen.currentResolution.width / 3;
        private static readonly float _raritiesMenuHeight = Screen.currentResolution.height / 3;
        private Rect _raritiesMenuRect = new((Screen.width - _raritiesMenuWidth) / 2, (Screen.height - _raritiesMenuHeight) / 1000, _raritiesMenuWidth, _raritiesMenuHeight);

        void Update()
        {
            if (Input.GetKey(MenuKey1) && Input.GetKeyDown(MenuKey2) || Input.GetKeyDown(MenuKey1) && SingleKeyKeybind) _showMenu = !_showMenu;
        }

        void OnGUI()
        {
            if (_showMenu) _menuRect = GUILayout.Window(_windowID, _menuRect, WeaponSelectorWindow, "Special Weapons");
            if (_showRaritiesMenu) _raritiesMenuRect = GUILayout.Window(_raritiesWindowID, _raritiesMenuRect, WeaponSpawnChanceWindow, "Special Weapon Spawn Chances");
        }

        void WeaponSelectorWindow(int id)
        {
            GUILayout.BeginVertical();
            GUI.skin.button.alignment = TextAnchor.UpperCenter;
            GUILayout.Space(20f);

            GUILayout.BeginHorizontal();
            GUILayout.Space(20f);
            if (GUILayout.Button("Edit Rarities"))
            {
                _showMenu = false;
                _showRaritiesMenu = true;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(20f);
            GUILayout.BeginHorizontal();
            int totalItemsInVerticalGroup = 2;
            int curItemsInVertGroup = 0;

            bool valueChanged = false;
            for (int i = 0; i < UnusedWeapons.Length; i++)
            {
                if (curItemsInVertGroup == totalItemsInVerticalGroup)
                {
                    curItemsInVertGroup = 0;
                    GUILayout.EndVertical();
                    GUILayout.Space(30f);
                }

                if (curItemsInVertGroup == 0) GUILayout.BeginVertical();

                var weapon = UnusedWeapons[i];
                UnusedWeapons[i].IsActive = GUILayout.Toggle(UnusedWeapons[i].IsActive, weapon.Name);
                if (GUI.changed) valueChanged = true;

                GUILayout.Space(30f);
                curItemsInVertGroup++;
            }

            if (valueChanged)
            {
                GenerateRarities();
                SaveWeaponStates();
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }

        void WeaponSpawnChanceWindow(int id)
        {
            GUI.skin.label.alignment = TextAnchor.UpperCenter;
            GUI.skin.button.alignment = TextAnchor.UpperCenter;
            if (GUILayout.Button("<color=red>Close</color>")) _showRaritiesMenu = false;
            GUILayout.Label(
            "Change the spawn rates of special weapons. While 255 is the true max, 20 is the recommended max, as it's the pistol/sword spawnrate, which is the highest of any weapon."
            );

            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical();
            bool valueChanged = false;
            for (int i = 0; i < UnusedWeapons.Length; i++)
            {
                var weapon = UnusedWeapons[i];
                GUILayout.Label(weapon.Name);

                GUILayout.BeginHorizontal();
                weapon.Rarity = byte.Parse(GUILayout.TextField(weapon.Rarity.ToString(), GUILayout.MaxWidth(50)));
                weapon.Rarity = (int) GUILayout.HorizontalSlider(weapon.Rarity, byte.MinValue, byte.MaxValue);
                if (GUI.changed) valueChanged = true;
                GUILayout.EndHorizontal();
                GUILayout.Space(20);
            }

            if (valueChanged)
            {
                GenerateRarities();
                SaveWeaponStates();
            }

            if (GUILayout.Button("Reset defaults")) ResetSpecialWeaponRarityDefaults();

            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }

        void GenerateRarities() => _generateRaritiesMethod.Invoke(GameManager.Instance.GetComponent<WeaponSelectionHandler>(), null);

        // TODO: Eventually try to switch this to simply reading in from a "defaults" JSON
        void ResetSpecialWeaponRarityDefaults()
        {
            UnusedWeaponInfo[] unusedWeaponDefaults = new UnusedWeaponInfo[]
            {
                new UnusedWeaponInfo {Index = 8, Rarity = 15},
                new UnusedWeaponInfo {Index = 9, Rarity = 15},
                new UnusedWeaponInfo {Index = 11, Rarity = 20},
                new UnusedWeaponInfo {Index = 13, Rarity = 20},
                new UnusedWeaponInfo {Index = 15, Rarity = 5},
                new UnusedWeaponInfo {Index = 18, Rarity = 10},
                new UnusedWeaponInfo {Index = 28, Rarity = 10},
                new UnusedWeaponInfo {Index = 29, Rarity = 12},
                new UnusedWeaponInfo {Index = 30, Rarity = 15},
                new UnusedWeaponInfo {Index = 34, Rarity = 10},
            };

            foreach (var defaultUnusedWeapon in unusedWeaponDefaults)
            {
                var unusedWeapon = UnusedWeapons.First(weapon => weapon.Index == defaultUnusedWeapon.Index);
                unusedWeapon.Rarity = defaultUnusedWeapon.Rarity;
            }

            GenerateRarities();
            SaveWeaponStates();
        }

        public static void LoadWeaponStates()
        {
            if (File.Exists(WeaponStatesPath))
            {
                JSONArray weaponStatesJSON = JSONNode.Parse(File.ReadAllText(WeaponStatesPath)).AsArray;
                foreach (var weaponInfoObj in weaponStatesJSON)
                {
                    JSONObject weaponInfoJSON = weaponInfoObj.Value.AsObject;
                    int weaponIndex = weaponInfoJSON[0];
                    int weaponRarity = weaponInfoJSON[1];
                    bool weaponState = weaponInfoJSON[2];  

                    var weapon = UnusedWeapons.First(weapon => weaponIndex == weapon.Index);
                    weapon.IsActive = weaponState;
                    weapon.Rarity = weaponRarity;
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
