using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace myTest
{

public class raycastShoot : MonoBehaviourPunCallbacks, IPunObservable
{
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


    bool IsFiring;
    public int gunDamage = 1;
    public float fireRate = .1f;
    public float weaponRange = 50f;
    public float hitforce = 100f;
    public Transform gunEnd;
    [SerializeField] ParticleSystem gunFlash;
    [SerializeField]  Camera playerCamera;
    private WaitForSeconds shotDuration = new WaitForSeconds(.07f);
  //  private LineRenderer laserLine;
    private float nextFire;
    private Player1 player;

    void Start()
    {
        if (!photonView.IsMine)
        {
            return;
        }
        player = GetComponent<Player1>();
       // laserLine = GetComponent<LineRenderer>();
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
       // laserLine.enabled = true;
        yield return shotDuration;
       // laserLine.enabled = false;
    }

    public void Shoot()
    {

        

        Vector3 forward = playerCamera.transform.TransformDirection(Vector3.forward) * 2;
        Vector3 rayOrigin = playerCamera.ViewportToWorldPoint(new Vector3 (.5f,.5f,0));
        RaycastHit hit;

       // laserLine.SetPosition(0, gunEnd.position);
        if(Physics.Raycast(rayOrigin, playerCamera.transform.forward, out hit, weaponRange))
        {
         //   laserLine.SetPosition(1, hit.point);

            EnemyHealth enemyHealth = hit.collider.GetComponent<EnemyHealth>();
             Player1 enemyPlayer = hit.collider.GetComponent<Player1>();

             if(enemyPlayer != null){

                 enemyPlayer.photonView.RPC("Damage", RpcTarget.All, gunDamage);
             }
            else if (enemyHealth != null)
            {
                Debug.Log("enemy health deducted");
                enemyHealth.DeductHealth(gunDamage);
            }
            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForce(-hit.normal * hitforce);
            }
        }
    }
       // else {
           // laserLine.SetPosition(1, rayOrigin + (playerCamera.transform.forward * weaponRange));
       // }
        [PunRPC]
        void Damage(int gunDamage, PhotonMessageInfo info)
        {

            if(photonView.IsMine){ player.health =-gunDamage;}
            // the photonView.RPC() call is the same as without the info parameter.
            // the info.Sender is the player who called the RPC.
            
        }

    
    }
}
