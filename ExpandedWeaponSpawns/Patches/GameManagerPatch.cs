using HarmonyLib;

namespace ExpandedWeaponSpawns.Patches
{
    class GameManagerPatch
    {
        public static void Patch(Harmony harmonyInstance)
        {
            var startMethod = AccessTools.Method(typeof(GameManager), "Start");
            var startMethodPostfix = new HarmonyMethod(typeof(GameManagerPatch).GetMethod(nameof(StartMethodPostfix)));
            harmonyInstance.Patch(startMethod, postfix: startMethodPostfix);
        }

        public static void StartMethodPostfix(GameManager __instance) 
            => __instance.gameObject.AddComponent<ExpandedWeaponsMenu>();       
    }
}
