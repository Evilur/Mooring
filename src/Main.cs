using BepInEx;
using HarmonyLib;
using BepInEx.Configuration;
using UnityEngine;
using Burlak.Interface;

namespace Burlak {
    [BepInPlugin(ModInfo.GUID, ModInfo.MODNAME, ModInfo.VERSION)]
    internal class Main : BaseUnityPlugin {
        private static ConfigEntry<string> _locale_ship;
        private static ConfigEntry<string> _locale_pull;

        public static string LocaleShip() => _locale_ship.Value;
        public static string LocalePull() => _locale_pull.Value;

        private void Awake() {
            /* Patch all the patches */
            Harmony harmony = new Harmony(ModInfo.GUID);
            harmony.PatchAll();

            /* Setting config entries */
            _locale_ship = Config.Bind(
                "Locale",
                "Ship",
                "Ship",
                "Localication of the 'ship' word"
            );
            _locale_pull = Config.Bind(
                "Locale",
                "Pull",
                "Pull",
                "Localication of the 'pull' word"
            );
        }
    }

    [HarmonyPatch(typeof(Ship), "Awake")]
    internal static class Penis {
        private static void Postfix(ref Ship __instance) {
            GameObject go = __instance.gameObject;
            if (!go.TryGetComponent<TowingHandle>(out _))
                go.AddComponent<TowingHandle>();
        }
    }
}
