using UnityEngine;

using Photon.Pun;


namespace myTest
{
   
    public class Player1 : MonoBehaviourPun
    {
        public static GameObject LocalPlayerInstance;
        private PhotonView photonView;
   
        
        public new Camera camera;
        public GameObject CameraManager;
        public GameObject graphics;
        public GameObject survivorGraphics;
        public GameObject zombieGraphics;
        public GameObject hud;

        [Header("Health")]
        [SerializeField] public int health = 100;
        [SerializeField] private int maxHealth = 100;
         public bool isDead = false;


        //public override void OnStartLocalPlayer()
        //{
        //     base.OnStartLocalPlayer();

        //    cameraContainer.SetActive(true);
        //    graphics.SetActive(false);
        //    zombieGraphics.SetActive(false);
        //    hud.SetActive(true);

        //    Cursor.lockState = CursorLockMode.Locked;
        //    Cursor.visible = false;
        //}
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
          //  GameObject _uiGo = Instantiate(this.PlayerUiPrefab);
            //_uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
        }


        public void Awake()
        {
            if (photonView.IsMine)
            {
                PlayerManager.LocalPlayerInstance = this.gameObject;
            }
            DontDestroyOnLoad(this.gameObject);
        }
        public void Start()
        {
            PlayerCamera playerCamera = this.gameObject.GetComponent<PlayerCamera>();
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public void Update()
        {
            if (photonView.IsMine)
            {
             //   this.ProcessInputs();
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

        //public void Heal(int amount = 100)
        //{
        //    health = Mathf.Clamp(health + amount, 0, maxHealth);
        //}
      //  public void TakeDamage(int amount, uint sourceID)

       public void TakeDamage(int amount)
       {
           health = Mathf.Clamp(health -= amount, 0, maxHealth);
       

           if (health <= 0)
           {
                isDead = true;
           }
       }

        //public void Freeze()
        //{
        //    weapon.enabled = false;
        //    infectedWeapon.enabled = false;
        //    pickupBehavior.enabled = false;
        //}

        private void SetDefaults()
        {
            health = maxHealth;
            isDead = false;
        }

        //private void Death(uint sourceID)
        //{
        //    health = 0;
        //    isDead = true;

        //    RpcOnDeath();
        //}

        [PunRPC]
        void RPC_TakeDamage(int damage)
        {

            if (!photonView.IsMine)
                return;

            health -= damage;
            Debug.Log("Ow ive been shoot" + photonView.name);
       //     healthbarImage.fillAmount = health/ maxHealth;
            
            if (health <= 0)
            {
                isDead = true;
            }
        }


    }
}
