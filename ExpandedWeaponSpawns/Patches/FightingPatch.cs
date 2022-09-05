using HarmonyLib;
using UnityEngine;

namespace ExpandedWeaponSpawns.Patches
{
    class FightingPatch
    {
        public static void Patch(Harmony harmonyInstance)
        {
            var networkThrowWeaponMethod = AccessTools.Method(typeof(Fighting), "NetworkThrowWeapon");
            var networkThrowWeaponMethodPrefix =
                new HarmonyMethod(typeof(FightingPatch).GetMethod(nameof(NetworkThrowWeaponMethodPrefix)));
            harmonyInstance.Patch(networkThrowWeaponMethod, prefix: networkThrowWeaponMethodPrefix);
        }
        
        // Re-add ConstantForce component if non-existent as ChangeToPresent() destroys it and so later on an error
        // will occur if the player attempts to throw the special weapon
        public static bool NetworkThrowWeaponMethodPrefix(ref byte weaponIndex, ref Weapons ___weapons)
        {
            
            var weaponDropObj = ___weapons.transform.GetChild(weaponIndex - 1).GetComponent<Weapon>().weaponDrop;
            if (!weaponDropObj.GetComponent<ConstantForce>()) weaponDropObj.AddComponent<ConstantForce>();
            
            return true;
        }
    }
}
