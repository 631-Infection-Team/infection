using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace myTest
{
    public class shotGun : MonoBehaviour
    {
		[SerializeField] Camera cam;

		PhotonView PV;

		void Awake()
		{
			PV = GetComponent<PhotonView>();
		}

		public void Shoot()
		{
			Debug.Log("pew pew");
			//Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
			//ray.origin = cam.transform.position;
			//if (Physics.Raycast(ray, out RaycastHit hit))
			//{
			//	hit.collider.gameObject.GetComponent<IDamageable>()?.TakeDamage(((GunInfo)itemInfo).damage);
			//	PV.RPC("RPC_Shoot", RpcTarget.All, hit.point, hit.normal);
			//}
		}


	}
}