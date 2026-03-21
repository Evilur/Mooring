using UnityEngine;

namespace Burlak {
    internal class TowingHandle : MonoBehaviour, Hoverable, Interactable {
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

            return true;
        }

        /* Do not use inventory items */
        public bool UseItem(Humanoid user, ItemDrop.ItemData item) => false;
    }
}
