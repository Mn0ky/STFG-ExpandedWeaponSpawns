using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace ExpandedWeaponSpawns.Patches
{
    class WeaponPatch
    {
        public static void Patch(Harmony harmonyInstance)
        {
            var actuallyShootMethod = AccessTools.Method(typeof(Weapon), "ActuallyShoot");
            var actuallyShootMethodPrefix = new HarmonyMethod(typeof(WeaponPatch).GetMethod(nameof(ActuallyShootMethodPrefix)));  
            harmonyInstance.Patch(actuallyShootMethod, prefix: actuallyShootMethodPrefix);

			var actuallyShootMethodTranspiler = new HarmonyMethod(typeof(WeaponPatch).GetMethod(nameof(ActuallyShootMethodTranspiler)));  
			harmonyInstance.Patch(actuallyShootMethod, transpiler: actuallyShootMethodTranspiler);

			var onEnableMethod = AccessTools.Method(typeof(Weapon), "OnEnable");
			var onEnableMethodPostfix = new HarmonyMethod(typeof(WeaponPatch).GetMethod(nameof(OnEnableMethodPostfix)));  
			harmonyInstance.Patch(onEnableMethod, postfix: onEnableMethodPostfix);
        }

        public static void OnEnableMethodPostfix(Weapon __instance)
        {
			switch (__instance.gameObject.name)
            {
				case "13 Bow":
					__instance.currentCharge = 0;
					__instance.startBullets = 5;
					break;
				case "30 MiniHolyGun":
					__instance.startBullets = 200;
                    break;
            }
		}

        public static bool ActuallyShootMethodPrefix(Weapon __instance)
        {
			if (!__instance.isCharged) return true;
			
			BowData bowInfo = __instance.gameObject.GetComponentInChildren<BowData>();

			Debug.Log("bowInfo.PlayerID: " + bowInfo.PlayerID);
			if (bowInfo.PlayerID == GameManager.Instance.mMultiplayerManager.LocalPlayerIndex)
			{
				bowInfo.ShootCharge = __instance.currentCharge;
				Debug.Log("SENDING SHOOT packet: " + bowInfo.ShootCharge);

				NetworkHelper.SyncShootCharge(bowInfo.ShootCharge);	
			}

			__instance.currentCharge = 0f;
			Debug.Log("currentCharge after shooting: " + __instance.currentCharge);
			return true;
		}

        // TODO: Introduce a local variable to load from rather than calling a function every time shootCharge is needed
        public static IEnumerable<CodeInstruction> ActuallyShootMethodTranspiler(IEnumerable<CodeInstruction> instructions)
        {
			List<CodeInstruction> instructionList = instructions.ToList();
            FieldInfo curChargeField = typeof(Weapon).GetField("currentCharge");

			for (var i = 0; i < instructionList.Count; i++)
				if (instructionList[i].LoadsField(curChargeField)) 
					instructionList[i] = CodeInstruction.Call(typeof(NetworkHelper), nameof(NetworkHelper.GetShootCharge));

			return instructionList.AsEnumerable();
        }
    }
}
