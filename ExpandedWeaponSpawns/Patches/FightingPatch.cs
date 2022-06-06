using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace ExpandedWeaponSpawns
{
    class FightingPatch
    {
        public static void Patch(Harmony harmonyInstance)
        {
            var networkThrowWeaponMethod = AccessTools.Method(typeof(Fighting), "NetworkThrowWeapon");
            var networkThrowWeaponMethodPrefix = new HarmonyMethod(typeof(FightingPatch).GetMethod(nameof(NetworkThrowWeaponMethodPrefix)));
            harmonyInstance.Patch(networkThrowWeaponMethod, prefix: networkThrowWeaponMethodPrefix);
        }

        public static bool NetworkThrowWeaponMethodPrefix(ref byte weaponIndex, ref Weapons ___weapons, Fighting __instance)
        {
            // Readd ConstantForce component as ChangeToPresent() destroys it and so later on an error will occur if the player attempts to throw the special weapon
            var weaponDropObj = ___weapons.transform.GetChild(weaponIndex - 1).GetComponent<Weapon>().weaponDrop;
            Debug.Log("Wanting to throw: " + weaponDropObj.name);
            if (!weaponDropObj.GetComponent<ConstantForce>()) weaponDropObj.AddComponent<ConstantForce>();
            return true;
        }
    }
}
