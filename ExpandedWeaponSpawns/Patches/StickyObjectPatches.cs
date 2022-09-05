using HarmonyLib;
using UnityEngine;

namespace ExpandedWeaponSpawns.Patches
{
    class StickyObjectPatches
    {

        public static void Patch(Harmony harmonyInstance)
        {
            var startMethod = AccessTools.Method(typeof(StickyObject), "Start");
            var startMethodPostfix = new HarmonyMethod(typeof(StickyObjectPatches).GetMethod(nameof(StartMethodPostfix)));  
            harmonyInstance.Patch(startMethod, postfix: startMethodPostfix);

            var stickMethod = AccessTools.Method(typeof(StickyObject), "Stick");
            var stickMethodPrefix = new HarmonyMethod(typeof(StickyObjectPatches).GetMethod(nameof(StickMethodPrefix)));  
            harmonyInstance.Patch(stickMethod, prefix: stickMethodPrefix);
        }

        public static void StartMethodPostfix(StickyObject __instance)
        {
            // Only modify collider layer immediately for 0 charged arrows as otherwise they'll go out of bounds
            if (__instance.gameObject.name != "BulletArrow(Clone)") return;
            
            foreach (var bowData in UnityEngine.Object.FindObjectsOfType<BowData>())
            {
                if (bowData.ShootCharge <= 2.05f && bowData.PlayerID == __instance.gameObject.GetComponent<TeamHolder>().team)
                {
                    __instance.gameObject.GetComponentInChildren<BoxCollider>().gameObject.layer = 29;
                    break;
                }
            }
        }

        // TODO: Figure out why original postfix was causing arrow-in-middle-of-screen bug
        public static bool StickMethodPrefix(ref Rigidbody hitRig, ref Quaternion rot, ref Controller c, StickyObject __instance)
        {
            bool isDone = Traverse.Create(__instance).Field("done").GetValue<bool>();
            if (isDone) return false;

            __instance.stickObject = new GameObject().transform;

            if (__instance.gameObject.name == "BulletArrow(Clone)")
            {
                Debug.Log("Readding collider");
                GameObject origCollider = __instance.gameObject.GetComponentInChildren<BoxCollider>().gameObject;
                GameObject newCollider = UnityEngine.Object.Instantiate(origCollider, __instance.stickObject, true);

                newCollider.layer = 29;

                UnityEngine.Object.Destroy(origCollider);
            }

            __instance.stickObject.position = __instance.gameObject.transform.position;
            __instance.stickObject.rotation = rot;

			if (hitRig) __instance.stickObject.SetParent(hitRig.transform, true);
			if (hitRig)	__instance.hitR = hitRig;
			if (c)	__instance.controller = c;

			TargetHolder component = __instance.gameObject.GetComponent<TargetHolder>();
            if (component && __instance.controller && __instance.hitR) component.Set(__instance.hitR, __instance.controller);

            return false;
        }
    }
}
