using UnityEngine;
using System.Linq;
using HarmonyLib;
using System.Reflection;

namespace Mooring.Core {
    internal class TowingHandle : MonoBehaviour, Hoverable, Interactable {
        /* The rope width */
        private const float _ropeWidth = 0.075f;

        /* The player, attached to the handle */
        private Player _player = null;

        /* Player's StopEmote method */
        private MethodInfo _playerStopAnimation =
            AccessTools.Method(typeof(Player), "StopEmote");

        /* The ship object */
        private Ship _ship = null;

        /* The ship's rigid body component */
        private Rigidbody _shipRigidBody = null;

        /* Pull values */
        private static float force = 50;
        private static float maxSpeed = 1;
        private static float maxRotation = 0.1f;
        private static ForceMode mode = ForceMode.Acceleration;

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

                /* Set the number of points */
                line.positionCount = 256;
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

                /* Enable synamic slack */
                line.m_dynamicSlack = true;

                /* Set slack */
                line.SetSlack(0.075f);
            }

            /* Play the sound */
            PlaySound();

            /* Save the attached player */
            _player = player as Player;

            /* Update the ship and ship's rigid body */
            _ship = transform.parent.GetComponent<Ship>();
            _shipRigidBody = _ship.GetComponent<Rigidbody>();

            /* Return the success */
            return true;
        }

        /* Do not use inventory items */
        public bool UseItem(Humanoid user, ItemDrop.ItemData item) => false;

        /* Detach the player from the ship */
        private void DetachPlayer() {
            /* Play the sound */
            PlaySound();

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

            /* Apply the 'vehice' layer */
            gameObject.layer = LayerMask.NameToLayer("vehicle");
        }

        private void FixedUpdate() {
            /* If the there is not attached players */
            if (_player == null) return;

            /* Stop the player animation */
            _playerStopAnimation.Invoke(_player, null);

            /* If the player wants to detach */
            if (_player.IsCrouching()) {
                DetachPlayer();
                return;
            }

            /* If the player wants to pull the ship */
            if (!_player.IsBlocking() || _ship.HasPlayerOnboard()) return;

            /* Animate the player */
            _player.StartEmote("cheer");

            /* Lock the speed */
            _shipRigidBody.linearVelocity =
                Vector3.ClampMagnitude(_shipRigidBody.linearVelocity,
                                       maxSpeed);

            /* Lock the rotation speed */
            _shipRigidBody.angularVelocity =
                Vector3.ClampMagnitude(_shipRigidBody.angularVelocity,
                                       maxRotation);

            /* Get the player's center position */
            Vector3 playerPos = _player.transform.position;
            playerPos.y += 0.9f;

            /* Get the delta between the player and the handle */
            Vector3 delta = playerPos - transform.position;

            /* If the player is too close to she ship, do nothing */
            if (delta.sqrMagnitude < 3 * 3) return;

            /* Get the force verctor */
            Vector3 pullForce = delta.normalized * force;

            /* Get the torque force vector */
            Vector3 torque = Vector3.Cross(transform.position -
                                           _shipRigidBody.worldCenterOfMass,
                                           pullForce);

            /* Apply the forces to the ship */
            _shipRigidBody.AddForce(pullForce, mode);
            _shipRigidBody.AddTorque(torque * 0.01f, mode);
        }

        private void PlaySound() {
            GameObject prefab = ZNetScene.instance.GetPrefab("sfx_unarmed_hit");
            Instantiate(prefab, transform.position, Quaternion.identity);
        }
    }
}
