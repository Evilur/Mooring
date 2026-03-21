using UnityEngine;
using System.Linq;

namespace Burlak.Interface {
    internal class TowingHandle : MonoBehaviour, Hoverable, Interactable {
        /* The rope width */
        private const float _ropeWidth = 0.075f;

        /* Set interactible text */
        public string GetHoverText() =>
            Main.LocaleShip() +
            "\n[<color=yellow><b>" +
            ZInput.instance.GetBoundKeyString("Use") +
            "</b></color>] " +
            Main.LocalePull();

        /* Set interactible name */
        public string GetHoverName() => "Towing Handle";

        /* Set interact action */
        public bool Interact(Humanoid user, bool hold, bool alt) {
            /* Do not support holding */
            if (hold) return false;

            /* Remove old components (if exist) */
            {
                if (gameObject
                        .TryGetComponent<LineRenderer>(out LineRenderer line))
                    Destroy(line);
            } {
                if (gameObject
                        .TryGetComponent<LineConnect>(out LineConnect line))
                    Destroy(line);
            }

            /* Setting the LineRenderer component */
            {
                /* Create the component */
                LineRenderer line = gameObject.AddComponent<LineRenderer>();

                /* Set the material from the harpoon projectile's rope */
                line.material = Resources
                    .FindObjectsOfTypeAll<Material>()
                    .FirstOrDefault(e => e.name.Contains("harpoon_wire"));

                /* Use global coordinates instead of local ones */
                line.useWorldSpace = false;

                /* Render light on the material */
                line.generateLightingData = true;

                /* Set the width */
                line.startWidth = line.endWidth = _ropeWidth;
            }

            /* Setting the LineConnect component */
            {
                /* Create the component */
                LineConnect line = gameObject.AddComponent<LineConnect>();

                /* Connect the second rope's end to the player */
                line.SetPeer(Player.m_localPlayer.gameObject
                        .GetComponent<ZNetView>());

                /* Attach a rope to the player's center */
                line.m_centerOfCharacter = true;

                /* Disable the dynamic thickness */
                line.m_dynamicThickness = false;
            }

            /* Return the success */
            return true;
        }

        /* Do not use inventory items */
        public bool UseItem(Humanoid user, ItemDrop.ItemData item) => false;
    }
}
