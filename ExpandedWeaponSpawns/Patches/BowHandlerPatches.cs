using HarmonyLib;
using UnityEngine;

namespace ExpandedWeaponSpawns.Patches
{
    class BowHandlerPatches
    {
        public static KeyCode drawKey;

        public static void Patch(Harmony harmonyInstance)
        {
            var startMethod = AccessTools.Method(typeof(BowHandler), "Start");
            var startMethodPostfix = new HarmonyMethod(typeof(BowHandlerPatches).GetMethod(nameof(StartMethodPostfix)));  
            harmonyInstance.Patch(startMethod, postfix: startMethodPostfix);

            var updateMethod = AccessTools.Method(typeof(BowHandler), "Update");
            var updateMethodPrefix = new HarmonyMethod(typeof(BowHandlerPatches).GetMethod(nameof(UpdateMethodPrefix)));  
            harmonyInstance.Patch(updateMethod, prefix: updateMethodPrefix);
        }

        public static void StartMethodPostfix(BowHandler __instance)
        {
            if (!__instance.GetComponentInParent<BowData>()) __instance.gameObject.AddComponent<BowData>();
        }

        public static bool UpdateMethodPrefix(BowHandler __instance)
        {
            Weapon? bow = __instance.GetComponentInParent<BowData>().WeaponObj;
            int playerID = __instance.GetComponentInParent<BowData>().PlayerID;

            if (bow == null) return false;
            
            if (playerID == GameManager.Instance.mMultiplayerManager.LocalPlayerIndex)
            {
                if (Input.GetKey(drawKey) && bow.currentCharge < 2.8f)
                {
                    bow.currentCharge = Mathf.Clamp(bow.currentCharge + (Time.deltaTime * 5), 0f, 2.8f);
                    NetworkHelper.SyncCharge(bow.currentCharge);
                }
                else if (bow.currentCharge > 0f)
                {
                    bow.currentCharge = Mathf.Clamp(bow.currentCharge - (Time.deltaTime * 5), 0f, 2.8f);
                    NetworkHelper.SyncCharge(bow.currentCharge);
                }
            }

            return true;
        }
    }
}
