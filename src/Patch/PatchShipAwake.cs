using HarmonyLib;
using UnityEngine;
using Mooring.Core;

namespace Mooring.Patch {
    [HarmonyPatch(typeof(Ship), "Awake")]
    internal static class PatchShipAwake {
        private static void Postfix(ref Ship __instance) {
            /* Create two handles */
            GameObject headHandle = new GameObject("head_handle");
            GameObject backHandle = new GameObject("back_handle");

            /* Add them TowingHandle component */
            headHandle.AddComponent<TowingHandle>();
            backHandle.AddComponent<TowingHandle>();

            /* Set them parent */
            Transform parent = __instance.transform;
            headHandle.transform.SetParent(parent);
            backHandle.transform.SetParent(parent);

            /* Set the local position */
            if (__instance.name == "Karve(Clone)") {
                headHandle.transform.localPosition =
                    new Vector3(0, 1.4f, -4.6f);
                backHandle.transform.localPosition =
                    new Vector3(0, 1.4f, 4.6f);
            } else if (__instance.name == "Raft(Clone)") {
                headHandle.transform.localPosition =
                    new Vector3(0, 0, 2.9f);
                backHandle.transform.localPosition =
                    new Vector3(0, 0, -2.9f);
            } else if (__instance.name == "VikingShip(Clone)") {
                headHandle.transform.localPosition =
                    new Vector3(0, 2.6f, -10.2f);
                backHandle.transform.localPosition =
                    new Vector3(0, 2.6f, 10.2f);
            } else if (__instance.name == "VikingShip_Ashlands(Clone)") {
                headHandle.transform.localPosition =
                    new Vector3(0, 6, -16.4f);
                backHandle.transform.localPosition =
                    new Vector3(0, 5.9f, 16.3f);
            }
        }
    }
}
