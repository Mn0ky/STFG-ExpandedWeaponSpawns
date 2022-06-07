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
    public class ExpandedWeaponsMenu : MonoBehaviour
    {
        public static UnusedWeaponInfo[] UnusedWeapons = new UnusedWeaponInfo[]
        {
            new (8, 5, "Shield"),
            new (9, 5, "Fan"),
            new (11, 8, "Ball"),
            new (13, 10, "BowAndArrow"),
            new (15, 10, "Lightsaber"),
            new (18, 6, "MinigunTiny"),
            new (28, 8, "LaserPlanter"),
            new (29, 8, "HolySword"),
            new (30, 2, "GodMinigun"),
            new (34, 4, "LavaWhip")
        };

        public static readonly int[] specialIndexes = new int[] { 30, 34 };

        public static KeyCode MenuKey1;
        public static KeyCode MenuKey2;
        public static bool SingleKeyKeybind;
        public static readonly string WeaponStatesPath = Path.Combine(Application.persistentDataPath, "WeaponStatesData.json");

        private readonly MethodInfo _generateRaritiesMethod = AccessTools.Method(typeof(WeaponSelectionHandler), "GenerateWeaponRarityArray");
        private bool _showMenu;
        private readonly int _windowID = 99;
        private Rect _menuRect;

        private bool _showRaritiesMenu;
        private readonly int _raritiesWindowID = 98;
        private Rect _raritiesMenuRect;
        private const string rarityMenuDescription = "Change the spawn rates of special weapons. "
                                                     + "While 255 is the true max, 20 is the recommended max, as it's the pistol/sword spawnrate (highest of any weapon). "
                                                     + "To keep in line with other weapon spawns, try not to go above the lower single digits.";

        private void Start()
        {
            float MenuWidth = 600f;
            float MenuHeight = 130f;
            _menuRect = new((Screen.width - MenuWidth) / 2, (Screen.height - MenuHeight) / 2, MenuWidth, MenuHeight);

            float raritiesMenuWidth = Screen.currentResolution.width / 3;
            float raritiesMenuHeight = Screen.currentResolution.height / 3;
            _raritiesMenuRect = new((Screen.width - raritiesMenuWidth) / 2, (Screen.height - raritiesMenuHeight) / 1000, raritiesMenuWidth, raritiesMenuHeight);
        }

        private void Update()
        {
            if (Input.GetKey(MenuKey1) && Input.GetKeyDown(MenuKey2) || Input.GetKeyDown(MenuKey1) && SingleKeyKeybind) _showMenu = !_showMenu;
        }

        private void OnGUI()
        {
            if (_showMenu) _menuRect = GUILayout.Window(_windowID, _menuRect, WeaponSelectorWindow, "Special Weapons");
            if (_showRaritiesMenu) _raritiesMenuRect = GUILayout.Window(_raritiesWindowID, _raritiesMenuRect, WeaponSpawnChanceWindow, "Special Weapon Spawn Chances");
        }

        private void WeaponSelectorWindow(int id)
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

        private void WeaponSpawnChanceWindow(int id)
        {
            if (GUILayout.Button("<color=red>Close</color>", GUILayout.MaxWidth(50))) _showRaritiesMenu = false;
            GUILayout.Label(rarityMenuDescription);

            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical();
            bool valueChanged = false;
            for (int i = 0; i < UnusedWeapons.Length; i++)
            {
                var weapon = UnusedWeapons[i];
                GUILayout.Label("<b>" + weapon.Name + "</b>");

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

            if (GUILayout.Button("Reset Defaults", GUILayout.MaxWidth(100))) ResetSpecialWeaponRarityDefaults();

            GUI.skin.label.alignment = TextAnchor.UpperLeft;
            GUI.skin.button.alignment = TextAnchor.UpperLeft;

            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }

        private void GenerateRarities() => _generateRaritiesMethod.Invoke(GameManager.Instance.GetComponent<WeaponSelectionHandler>(), null);

        // TODO: Eventually try to switch this to simply reading in from a "defaults" JSON
        private void ResetSpecialWeaponRarityDefaults()
        {
            foreach (var unusedWeapon in UnusedWeapons)
            {
                unusedWeapon.Rarity = unusedWeapon.DefaultRarity;
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

        private void SaveWeaponStates()
        {
            JSONArray weaponStatesJSON = CreateWeaponStatesJSON();
            File.WriteAllText(WeaponStatesPath, weaponStatesJSON.ToString());
        }

        // TODO: Abstract json creation to UnusedWeaponInfo
        private JSONArray CreateWeaponStatesJSON()
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
