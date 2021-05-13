using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class raycastShoot : MonoBehaviourPun
{

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

    void Start()
    {
        if (!photonView.IsMine)
        {
            return;
        }
       // laserLine = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire") ) {
            gunFlash.Play();
        //    nextFire = Time.deltaTime + fireRate;
            //StartCoroutine(ShotEffect());
            Shoot(); 
        
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
        if (Physics.Raycast(rayOrigin, playerCamera.transform.forward, out hit, weaponRange))
        {
         //   laserLine.SetPosition(1, hit.point);

            EnemyHealth enemyHealth = hit.collider.GetComponent<EnemyHealth>();

            if (enemyHealth != null)
            {
                Debug.Log("enemy health deducted");
                enemyHealth.DeductHealth(gunDamage);
            }
            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForce(-hit.normal * hitforce);
            }
        }
       // else {
           // laserLine.SetPosition(1, rayOrigin + (playerCamera.transform.forward * weaponRange));
       // }
            

    }
}
