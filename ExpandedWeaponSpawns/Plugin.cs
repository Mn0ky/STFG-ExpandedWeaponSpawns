using System;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using System.Linq;
using ExpandedWeaponSpawns.Patches;

namespace ExpandedWeaponSpawns;

[BepInPlugin(Guid, "ExpandedWeaponSpawns", VersionNumber)]
[BepInDependency("monky.plugins.SimpleAntiCheat")]
[BepInProcess("StickFight.exe")]
public class Plugin : BaseUnityPlugin
{
    private const string VersionNumber = "1.0.0";
    private const string Guid = "monky.plugins.ExpandedWeaponSpawns";

    private static ConfigEntry<string> ?_configBowDrawKeybind;
    private static ConfigEntry<KeyboardShortcut> ?_configMenuKeybind;

    private void Awake()
    {
        // Plugin startup logic
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        try
        {
            Harmony harmony = new("monky.ExpandedWeaponSpawns"); // Creates harmony instance with identifier

            Logger.LogInfo("Applying BowHandler patches...");
            BowHandlerPatches.Patch(harmony);
            Logger.LogInfo("Applying P2PPackageHandler patches...");
            P2PPackageHandlerPatches.Patch(harmony);
            Logger.LogInfo("Applying Weapon patches...");
            WeaponPatch.Patch(harmony);
            Logger.LogInfo("Applying StickyObject patches...");
            StickyObjectPatches.Patch(harmony);
            Logger.LogInfo("Applying GameManager patch...");
            GameManagerPatch.Patch(harmony);
            Logger.LogInfo("Applying WeaponSelectionHandler patches...");
            WeaponSelectionHandlerPatches.Patch(harmony);
            Logger.LogInfo("Applying Fighting patches...");
            FightingPatch.Patch(harmony);
            Logger.LogInfo("Apply MultiplayerManager patches...");
            MultiplayerManagerPatches.Patch(harmony);
        }
        catch (Exception ex)
        {
            Logger.LogError("Exception on applying patches: " + ex.InnerException + ex.Message + ex.Source);
        }

        try
        {
            Logger.LogInfo("Loading configuration options from config file...");
                    
            _configBowDrawKeybind = Config.Bind("Bow Options",
                "Bow Keybind",
                "LeftShift",
                "Change the bow draw key? (https://docs.unity3d.com/ScriptReference/KeyCode.html)");

            _configMenuKeybind = Config.Bind("Weapon Selector Options",
                "Bow Keybind",
                new KeyboardShortcut(KeyCode.LeftShift, KeyCode.F3),
                "Change the weapon selector menu keybind? (https://docs.unity3d.com/ScriptReference/KeyCode.html)");
            
            // Bow drawkey defaults to LeftShift if enum parsing fails
            BowHandlerPatches.drawKey = _configBowDrawKeybind.Value.ToEnum(KeyCode.LeftShift);

            ExpandedWeaponsMenu.MenuKey1 = _configMenuKeybind.Value.MainKey;
            // If menu has only a single key for its keybind then this is true
            ExpandedWeaponsMenu.SingleKey = !_configMenuKeybind.Value.Modifiers.Any(); 
            if (!ExpandedWeaponsMenu.SingleKey) ExpandedWeaponsMenu.MenuKey2 = _configMenuKeybind.Value.Modifiers.Last();

            ExpandedWeaponsMenu.LoadWeaponStates();
        }
        catch (Exception ex)
        {
            Logger.LogError("Exception on loading config: " + ex.InnerException + ex.Message + ex.Source);
        }
    }
}