using UnityEngine;
using System.Linq;

namespace Burlak.Core {
    internal class TowingHandle : MonoBehaviour, Hoverable, Interactable {
        /* The rope width */
        private const float _ropeWidth = 0.075f;

        /* The player, attached to the handle */
        private Humanoid _player = null;

        /* The ship object */
        private Ship _ship = null;

        /* The ship's rigid body component */
        private Rigidbody _shipRigidBody = null;

        /* The ship's base maxAngularVelocity value */
        private float _shipBaseAngularVelocity = 7f;

        /* Pull values */
        private const float force = 50f;
        private const float maxSpeed = 1f;
        private const float maxRotation = 0.25f;
        private const ForceMode mode = ForceMode.Acceleration;

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
        public bool Interact(Humanoid player, bool hold, bool alt) {
            /* Do not support holding */
            if (hold) return false;

            /* If the player is attached */
            if (_player != null ||
                TryGetComponent<LineRenderer>(out _) ||
                TryGetComponent<LineConnect>(out _)) {
                /* Remove old components (if exist) */
                DetachPlayer();

                /* Exit with the success code */
                return true;
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
                line.SetPeer(player.GetComponent<ZNetView>());

                /* Attach a rope to the player's center */
                line.m_centerOfCharacter = true;

                /* Disable the dynamic thickness */
                line.m_dynamicThickness = false;
            }

            /* Play the sound */
            PlaySound();

            /* Save the attached player */
            _player = player;

            /* Update the ship and ship's rigid body */
            _ship = transform.parent.GetComponent<Ship>();
            _shipRigidBody = _ship.GetComponent<Rigidbody>();

            /* Set the maxAngularVelocity */
            _shipRigidBody.maxAngularVelocity = maxRotation;

            /* Return the success */
            return true;
        }

        /* Do not use inventory items */
        public bool UseItem(Humanoid user, ItemDrop.ItemData item) => false;

        /* Detach the player from the ship */
        private void DetachPlayer() {
            /* Play the sound */
            PlaySound();

            /* Restore the maxAngularVelocity */
            _shipRigidBody.maxAngularVelocity = _shipBaseAngularVelocity;

            {
                if (gameObject.TryGetComponent<LineRenderer>(
                            out LineRenderer line)) Destroy(line);
            } {
                if (gameObject.TryGetComponent<LineConnect>(
                            out LineConnect line)) Destroy(line);
            }
            _player = null;
        }

        /* On awake */
        private void Awake() {
            /* Create a spere trigger */
            SphereCollider sphere = gameObject.AddComponent<SphereCollider>();

            /* Set the radius */
            sphere.radius = 1.0f;

            /* Make this sphere as a trigger */
            sphere.isTrigger = true;
        }

        private void FixedUpdate() {
            /* If the there is not atteched players */
            if (_player == null) return;

            /* If the player wants to detach */
            if (_player.InAttack()) {
                DetachPlayer();
                return;
            }

            /* If the player wants to pull the ship */
            if (!_player.IsBlocking() || _ship.HasPlayerOnboard()) return;

            /* Lock the speed */
            if (_shipRigidBody.linearVelocity.magnitude > maxSpeed)
                _shipRigidBody.linearVelocity =
                    _shipRigidBody.linearVelocity.normalized * maxSpeed;

            /* Lock the rotation */
            if (_shipRigidBody.angularVelocity.magnitude > maxRotation)
                _shipRigidBody.angularVelocity =
                    _shipRigidBody.angularVelocity.normalized * maxRotation;

            /* Get the player's center position */
            Vector3 playerPos = _player.transform.position;
            playerPos.y += 0.9f;

            /* Get the delta between the player and the handle */
            Vector3 delta = playerPos - transform.position;

            /* If the player is too close to she ship, do nothing */
            if (delta.sqrMagnitude < 3f * 3f) return;

            /* Apply the force to the ship */
            _shipRigidBody.AddForceAtPosition(delta.normalized * force,
                                              transform.position,
                                              mode);
        }

        private void PlaySound() {
            GameObject prefab = ZNetScene.instance.GetPrefab("sfx_unarmed_hit");
            Instantiate(prefab, transform.position, Quaternion.identity);
        }
    }
}
