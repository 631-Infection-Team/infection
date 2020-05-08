using System;
using Mirror;
using UnityEngine;

namespace Infection.Combat
{
    [RequireComponent(typeof(Player))]
    [RequireComponent(typeof(PlayerCamera))]
    public class InfectedWeapon : NetworkBehaviour
    {
        public SphereCollider attackTrigger = null;
        public LayerMask attackMask;
        public Transform weaponHolder = null;
        public GameObject clawPrefab = null;
        public AnimatorOverrideController animatorOverride = null;
        public Sprite crosshair = null;
        public float timeBetweenAttacks = 0.3f;

        [SyncEvent] public event Action EventOnEnable = null;

        private Collider[] hits = new Collider[8];
        private Animator _weaponHolderAnimator = null;

        private float _timeSinceLastAttack = Mathf.Infinity;

        private void Awake()
        {
            _weaponHolderAnimator = weaponHolder.GetComponent<Animator>();
        }

        private void OnEnable()
        {
            if (isLocalPlayer)
            {
                _weaponHolderAnimator.runtimeAnimatorController = animatorOverride;
                CmdUpdateWeaponModel();
                CmdEventOnEnable();
            }
        }

        public void Update()
        {
            if (!isLocalPlayer) return;

            if (Input.GetAxis("Fire") > 0f && _timeSinceLastAttack >= timeBetweenAttacks)
            {
                CmdAttack();
                _timeSinceLastAttack = 0f;
            }

            _timeSinceLastAttack += Time.deltaTime;
        }

        [Command]
        void CmdAttack()
        {
            // We need to calculate the raycast on the server side, because we cannot send gameobjects over the network.
            var hitCount = Physics.OverlapSphereNonAlloc(attackTrigger.bounds.center, attackTrigger.radius, hits, attackMask);

            if (hitCount > 0)
            {
                //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
                //Debug.Log("Server recognized that the object was hit: " + hit.transform.gameObject.name);

                foreach (var hit in hits)
                {
                    if (hit == null)
                    {
                        continue;
                    }

                    Player victim = hit.transform.gameObject.GetComponent<Player>();
                    if (victim && victim.gameObject != gameObject && victim.team == Player.Team.SURVIVOR)
                    {
                        Debug.Log("Infected weapon hit: " + victim);
                        // Cause damage to the victim, and pass our network ID so we can keep track of who killed who.
                        victim.TakeDamage(50, GetComponent<NetworkIdentity>().netId);
                    }
                }
            }

            RpcOnAttack();
        }

        [ClientRpc]
        void RpcOnAttack()
        {
            // Call a method on the PlayerAnimator.cs here. (Set trigger shoot?)
            _weaponHolderAnimator.SetTrigger("Fire");
            _weaponHolderAnimator.SetFloat("FireRate", 1.0f / timeBetweenAttacks);
        }

        /// <summary>
        /// Removes old weapon model and spawns a new weapon model from the currently equipped weapon.
        /// This process destroys all child game objects from the weapon holder and instantiates a new object
        /// from the model prefab in the weapon definition.
        /// </summary>
        [Command]
        private void CmdUpdateWeaponModel()
        {
            // Destroy all children
            foreach (Transform child in weaponHolder)
            {
                Destroy(child.gameObject);
            }

            if (clawPrefab != null)
            {
                // Spawn weapon model
                GameObject clawModel = Instantiate(clawPrefab, weaponHolder);
                var pos = clawModel.transform.localPosition;
                var rot = clawModel.transform.localRotation;
                clawModel.SetActive(isLocalPlayer);
                NetworkServer.Spawn(clawModel, connectionToClient);
                RpcUpdateWeaponModel(clawModel, pos, rot);
            }
        }

        [ClientRpc]
        private void RpcUpdateWeaponModel(GameObject weaponModel, Vector3 pos, Quaternion rot)
        {
            weaponModel.transform.SetParent(weaponHolder);
            weaponModel.transform.localPosition = pos;
            weaponModel.transform.localRotation = rot;
        }

        [Command]
        private void CmdEventOnEnable()
        {
            EventOnEnable?.Invoke();
        }
    }
}
