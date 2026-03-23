using HarmonyLib;
using UnityEngine;
using Mooring.Core;
using System.Collections.Generic;

namespace Mooring.Patch {

    [HarmonyPatch(typeof(ZNetScene), "Awake")]
    internal static class PatchZNetSceneAwake {
        private readonly static Dictionary<string, Vector3[]> HANDLE_POINTS =
            new Dictionary<string, Vector3[]> {
                {
                    "Raft",
                    new Vector3[] {
                        new Vector3(0, 0, 2.9f),
                        new Vector3(0, 0, -2.9f)
                    }
                }, {
                    "Karve",
                    new Vector3[] {
                        new Vector3(0, 1.4f, 4.6f),
                        new Vector3(0, 1.4f, -4.6f)
                    }
                }, {
                    "VikingShip",
                    new Vector3[] {
                        new Vector3(0, 2.6f, 10.2f),
                        new Vector3(0, 2.6f, -10.2f)
                    }
                }, {
                    "VikingShip_Ashlands",
                    new Vector3[] {
                        new Vector3(0, 5.9f, 16.3f),
                        new Vector3(0, 6, -16.4f)
                    }
                },
        };

        static void Postfix(ZNetScene __instance) {
            /* Loop through all the ships */
            foreach(KeyValuePair<string, Vector3[]> pair in HANDLE_POINTS) {
                /* Get the prefab name */
                string name = pair.Key;

                /* Get handle points */
                Vector3[] points = pair.Value;

                /* Get the prefab */
                GameObject prefab = __instance.GetPrefab(name);
                if (prefab == null) return;

                /* Create a new towing handles */
                GameObject[] handles = new GameObject[] {
                    new GameObject("head_handle", typeof(TowingHandle)),
                    new GameObject("back_handle", typeof(TowingHandle))
                };

                /* Loop through them */
                for (byte i = 0; i < 2; i++) {
                    /* Get the transform */
                    Transform transform = handles[i].transform;

                    /* Set the parent */
                    transform.SetParent(prefab.transform);

                    /* Set the local position */
                    transform.localPosition = points[i];
                }
            }
        }
    }
}
