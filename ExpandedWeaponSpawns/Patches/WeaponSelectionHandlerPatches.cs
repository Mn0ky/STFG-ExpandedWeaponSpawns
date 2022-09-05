using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace ExpandedWeaponSpawns.Patches
{
    class WeaponSelectionHandlerPatches
    {
        public static void Patch(Harmony harmonyInstance)
        {
            var getWeaponByIndexMethod = AccessTools.Method(typeof(WeaponSelectionHandler), "GetWeaponByIndex");
            var getWeaponByIndexMethodPrefix = 
                new HarmonyMethod(typeof(WeaponSelectionHandlerPatches).GetMethod(nameof(GetWeaponByIndexMethodPrefix)));  
            harmonyInstance.Patch(getWeaponByIndexMethod, prefix: getWeaponByIndexMethodPrefix);

            var getRandomWeaponIndexMethod = AccessTools.Method(typeof(WeaponSelectionHandler), "GetRandomWeaponIndex");
            var getRandomWeaponIndexMethodTranspiler = 
                new HarmonyMethod(typeof(WeaponSelectionHandlerPatches).GetMethod(nameof(GetRandomWeaponIndexMethodTranspiler)));  
            harmonyInstance.Patch(getRandomWeaponIndexMethod, transpiler: getRandomWeaponIndexMethodTranspiler);

            var generateWeaponRarityArrayMethod = AccessTools.Method(typeof(WeaponSelectionHandler), "GenerateWeaponRarityArray");
            var generateWeaponRarityArrayMethodPostfix = 
                new HarmonyMethod(typeof(WeaponSelectionHandlerPatches).GetMethod(nameof(GenerateWeaponRarityArrayMethodPostfix)));  
            harmonyInstance.Patch(generateWeaponRarityArrayMethod, postfix: generateWeaponRarityArrayMethodPostfix);
        }

        public static bool GetWeaponByIndexMethodPrefix(ref int index, ref GameObject? __result, WeaponSelectionHandler __instance)
        {
            var weaponID = index;
            var weaponPickupObjs = Traverse.Create(__instance).Field("m_WeaponObjects").GetValue<GameObject[]>();
            GameObject? weaponPickup = null;

            if (weaponID is not (30 or 34 or 65 or 66 or 67)) // Weapons that do not have their own respective drop
            {
                if (weaponID < weaponPickupObjs.Length) weaponPickup = weaponPickupObjs[weaponID];
                if (weaponPickup)
                {
                    Debug.Log("Weapon is in default list: " + weaponID);
                    __result = weaponPickup;
                    return false;   
                }
            }
            else
            {
                // Get sword weapon (Gun2) to use as placeholder for weapons
                // whose WeaponDrop is not themselves (ex: holyminigun's is the normal minigun)
                weaponPickup = UnityEngine.Object.Instantiate(weaponPickupObjs[2]);

                var weaponPickupComponent = weaponPickup.GetComponent<WeaponPickUp>();
                weaponPickupComponent.id = weaponID; // Switch the weapon id to that of the special weapon (lava whip or holyminigun)
                
                if (!ExpandedWeaponsMenu.IsDefaultCharacter)
                {
                    weaponPickupComponent.id = weaponPickupComponent.id switch
                    {
                        65 => 4, // AI knife index
                        66 => 7, // AI ice revolver index
                        67 => 10,// AI og shotgun index
                        _ => weaponPickupComponent.id
                    };
                }
                
                Debug.Log("Changing weapon to present!");
                weaponPickupComponent.ChangeToPresent();

                // Re-add ConstantForce component as ChangeToPresent() destroys it and so later on an
                // error will occur if the player attempts to throw the special weapon
                if (!weaponPickup.GetComponent<ConstantForce>()) weaponPickup.AddComponent<ConstantForce>();

                __result = weaponPickup;
                return false;
            }    
            
            Debug.Log("Did not find weapon in default list, pulling directly from Weapons transform: " + weaponID);
            __result = UnityEngine.Object.FindObjectOfType<Weapons>().transform.GetChild(weaponID).GetComponent<Weapon>().weaponDrop;
            return false;
        }

        // TODO: Add specific check for instruction at i + 1
        public static IEnumerable<CodeInstruction> GetRandomWeaponIndexMethodTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var instructionList = instructions.ToList();
            var wepSelHandlerType = typeof(WeaponSelectionHandler);
            var weaponObjectsField = AccessTools.Field(wepSelHandlerType, "m_WeaponObjects");

            for (var i = 0; i < instructionList.Count; i++)
            {
                if (!instructionList[i].LoadsField(weaponObjectsField)) continue;
                
                instructionList.RemoveAt(i);
                instructionList[i + 1] = CodeInstruction.Call(wepSelHandlerType, "GetWeaponByIndex");
            }

            return instructionList.AsEnumerable();
        }

        public static void GenerateWeaponRarityArrayMethodPostfix(WeaponSelectionHandler __instance)
        {
            var weaponRaritiesArrayInstance = Traverse.Create(__instance).Field("m_WeaponRaritiesArray");
            var weaponRaritiesList = weaponRaritiesArrayInstance.GetValue<List<byte>>();

            foreach (var weapon in ExpandedWeaponsMenu.UnusedWeapons)
            {
                if (!weapon.IsActive) continue;
                
                for (var i = 0; i < weapon.Rarity; i++) 
                    weaponRaritiesList.Add(weapon.Index);
            }

            weaponRaritiesArrayInstance.SetValue(weaponRaritiesList);
        }
    }
}
    
