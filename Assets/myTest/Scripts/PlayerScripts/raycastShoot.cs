using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace myTest
{

public class raycastShoot : MonoBehaviourPunCallbacks, IPunObservable
{


    bool IsFiring;
    public int gunDamage = 1;
    public float fireRate = .1f;
    public float weaponRange = 50f;
    public float hitforce = 100f;
    public Transform gunEnd;
    [SerializeField] ParticleSystem gunFlash;
    [SerializeField]  Camera playerCamera;
    private WaitForSeconds shotDuration = new WaitForSeconds(.07f);
  
    private float nextFire;
    private Player1 player;

    void Start()
    {
        if (!photonView.IsMine)
        {
            return;
        }
        player = GetComponent<Player1>();
    
    }

    void Update()
    {
         if (Input.GetButtonDown("Fire"))
            {
                if (!IsFiring)
                {
                     gunFlash.Play();
                    Shoot(); 
                    IsFiring = true;
                }
            }
            if (Input.GetButtonUp("Fire"))
            {
                if (IsFiring)
                {
                    IsFiring = false;
                }
            }
     
    }

    private IEnumerator ShotEffect()
    {
      
        yield return shotDuration;
      
    }

    public void Shoot()
    {

        

        Vector3 forward = playerCamera.transform.TransformDirection(Vector3.forward) * 2;
        Vector3 rayOrigin = playerCamera.ViewportToWorldPoint(new Vector3 (.5f,.5f,0));
        RaycastHit hit;

      
        if(Physics.Raycast(rayOrigin, playerCamera.transform.forward, out hit, weaponRange))
        {
       
            EnemyHealth enemyHealth = hit.collider.GetComponent<EnemyHealth>();
            Player1 enemyPlayer = hit.collider.GetComponent<Player1>();
            string hitTag = hit.transform.gameObject.tag;
                if (hitTag == "Player"){
                    enemyPlayer.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.All, gunDamage, PhotonNetwork.LocalPlayer.NickName);
             }
            else if (enemyHealth != null)
            {
                enemyHealth.DeductHealth(gunDamage);
            }
           
        }
    }

      
        #region IPunObservable implementation

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting && photonView.IsMine == true)
            {
                // We own this player: send the others our data
                stream.SendNext(IsFiring);
                stream.SendNext(player.health);
            }
            else
            {
                // Network player, receive data
                this.IsFiring = (bool)stream.ReceiveNext();
                this.player.health = (int)stream.ReceiveNext();
            }
        }
        #endregion


    }
}
