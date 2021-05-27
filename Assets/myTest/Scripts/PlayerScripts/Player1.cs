using UnityEngine;

using Photon.Pun;
using System.Collections;

namespace myTest
{
   
    public class Player1 : MonoBehaviourPun
    {
        public static GameObject LocalPlayerInstance;
       
   
        
        public new Camera camera;
        public GameObject CameraManager;
        public GameObject graphics;
        public GameObject survivorGraphics;
        public GameObject zombieGraphics;
        public GameObject hud;

        private Animator playerAnim; 

        [Header("Health")]
        [SerializeField] public int health = 10;
        [SerializeField] private int maxHealth = 10;
         public bool isDead = false;


        //public override void OnStartLocalPlayer()
        //{
        //    base.OnStartLocalPlayer();

        //    cameraContainer.SetActive(true);
        //    graphics.SetActive(false);
        //    zombieGraphics.SetActive(false);
        //    hud.SetActive(true);

        //    Cursor.lockState = CursorLockMode.Locked;
        //    Cursor.visible = false;
        //}

        public void Awake()
        {

            if (photonView.IsMine)
            {
                Player1.LocalPlayerInstance = this.gameObject;
            }

            DontDestroyOnLoad(this.gameObject);
        }
        void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadingMode)
        {
            this.CalledOnLevelWasLoaded(scene.buildIndex);
        }
        void CalledOnLevelWasLoaded(int level)
        {
            // check if we are outside the Arena and if it's the case, spawn around the center of the arena in a safe zone
            if (!Physics.Raycast(transform.position, -Vector3.up, 5f))
            {
                transform.position = new Vector3(0f, 5f, 0f);
            }
        }


       
        public void Start()
        {
            SetDefaults();
            PlayerCamera playerCamera = this.gameObject.GetComponent<PlayerCamera>();
            playerAnim = this.gameObject.GetComponent<Animator>();
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public void Update()
        {
            if (photonView.IsMine)
            {
                if (health <= 0f)
                {
                    GameManager.Instance.LeaveRoom();
                }
            }
           
        }

        public void OnDestroy()
        {
  

            if (photonView.IsMine)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }


        private void SetDefaults()
        {
            health = maxHealth;
            isDead = false;
        }

        [PunRPC]
        public void TakeDamage(int amount, string enemyName)
        {
            if (isDead) return;
            if (photonView.IsMine)
            {
                playerAnim.SetTrigger("hit");
                health -= amount;
                Debug.Log("HEALTH" + health);
                if (health <= 0)
                {
                    
                    this.photonView.RPC("Death", RpcTarget.All, enemyName);
                }
              
            }
        
        }

        /// <summary>
        /// RPC function to declare death of player.
        /// </summary>
        /// <param name="enemyName">Enemy's name who cause this player's death.</param>
        [PunRPC]
        void Death(string enemyName)
        {
            isDead = true;

            if (photonView.IsMine)
            {
                playerAnim.SetBool("Death_b", true);
                StartCoroutine("DestroyPlayer");
            }
            
        }

        IEnumerator DestroyPlayer(float delayTime)
        {
            Debug.Log("destroying player");
            yield return new WaitForSeconds(delayTime);
            playerAnim.SetInteger("DeathType_int", 2);
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(health);
            }
            else
            {
                health = (int)stream.ReceiveNext();
            }
        }
    }
}
