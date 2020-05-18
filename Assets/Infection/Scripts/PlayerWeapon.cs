using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace Infection
{
    [RequireComponent(typeof(Player))]
    [RequireComponent(typeof(PlayerCamera))]
    public class PlayerWeapon : NetworkBehaviour
    {
        public void Update()
        {
            if (!isLocalPlayer) return;

            if (Input.GetAxis("Fire") > 0f)
            {
                CmdFire();
            }
        }

        [Command]
        void CmdFire()
        {
            // We need to calculate the raycast on the server side, because we cannot send gameobjects over the network.
            Transform cameraTransform = GetComponent<Player>().camera.transform;
            Ray ray = new Ray(cameraTransform.position, cameraTransform.forward + new Vector3(0, 0, 0));
            bool raycast = Physics.Raycast(ray, out var hit, 1000f, LayerMask.GetMask("Default"));

            if (raycast)
            {
                //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
                //Debug.Log("Server recognized that the object was hit: " + hit.transform.gameObject.name);

                Player victim = hit.transform.gameObject.GetComponent<Player>();

                if (victim)
                {
                    // Cause damage to the victim, and pass our network ID so we can keep track of who killed who.
                    victim.TakeDamage(30, GetComponent<NetworkIdentity>().netId);
                }
            }

            //GameObject projectile = Instantiate(projectilePrefab, projectileMount.position, transform.rotation);
            //NetworkServer.Spawn(projectile);
            RpcOnFire();
        }

        [ClientRpc]
        void RpcOnFire()
        {
            // Call a method on the PlayerAnimator.cs here. (Set trigger shoot?)
        }
    }
}

