// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayerUI.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Networking Demos
// </copyright>
// <summary>
//  Used in PUN Basics Tutorial to deal with the networked player instance UI display tha follows a given player to show its health and name
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using TMPro;

using System.Collections;
using myTest.Combat;

namespace myTest
{
	#pragma warning disable 649

	/// <summary>
	/// Player UI. Constraint the UI to follow a PlayerManager GameObject in the world,
	/// Affect a slider and text to display Player's name and health
	/// </summary>
	public class PlayerUI : MonoBehaviour
    {
        #region Private Fields

	    [Tooltip("Pixel offset from the player target")]
        [SerializeField]
        private Vector3 screenOffset = new Vector3(0f, 30f, 0f);

	    [Tooltip("UI Text to display Player's Name")]
	    [SerializeField]
	    private Text playerNameText;

	    [Tooltip("UI Slider to display Player's Health")]
	    [SerializeField]
	    private Slider playerHealthSlider;

		[SerializeField] private Image crosshair = null;
		[SerializeField] private TextMeshProUGUI timerDisplay = null;
		[SerializeField] private TextMeshProUGUI gameState = null;
		[SerializeField] private TextMeshProUGUI roundMessage = null;
		[SerializeField] private TextMeshProUGUI healthValueDisplay = null;
		[SerializeField] private TextMeshProUGUI playerCount = null;
		[SerializeField] private Weapon playerWeapon = null;
		[SerializeField] private PickupBehavior playerPickupBehavior = null;


		private Animation _timerAnim = null;

		PlayerManager target;

		float characterControllerHeight;

		Transform targetTransform;

		Renderer targetRenderer;

	    CanvasGroup _canvasGroup;
	    
		Vector3 targetPosition;

		#endregion

		#region MonoBehaviour Messages
		
		/// <summary>
		/// MonoBehaviour method called on GameObject by Unity during early initialization phase
		/// </summary>
		void Awake()
		{

			_canvasGroup = this.GetComponent<CanvasGroup>();
			
			//this.transform.SetParent(GameObject.Find("Canvas").GetComponent<Transform>(), false);

			_timerAnim = timerDisplay.gameObject.GetComponent<Animation>();
		}

		private void OnEnable()
		{
			timerDisplay.text = "0:59";
			gameState.text = "Pre-Game";
			roundMessage.text = "";
		}

		public void UpdateTimer(float timeLeft)
		{
			int min = Mathf.FloorToInt(timeLeft / 60);
			int sec = Mathf.FloorToInt(timeLeft % 60);

			timerDisplay.color = sec <= 10 && min <= 0 ? new Color(255, 0, 0, 0.8f) : new Color(255, 255, 255, 0.8f);
			timerDisplay.text = min.ToString("00") + ":" + sec.ToString("00");

			// Round Timer animation
			_timerAnim.Play();
		}

		public void UpdateState(string info)
		{
			gameState.text = info;
		}

		public void UpdateRoundMessage(string message)
		{
			roundMessage.text = message;
		}

		public void UpdatePlayerCount(int number)
		{
            if (number == 1)
            {
				playerCount.text = "1 Player Left";
			}
            else
            {
				playerCount.text = number.ToString() + " Players Left";
			}
		}

		/// <summary>
		/// MonoBehaviour method called on GameObject by Unity on every frame.
		/// update the health slider to reflect the Player's health
		/// </summary>
		void Update()
		{
			// Destroy itself if the target is null, It's a fail safe when Photon is destroying Instances of a Player over the network
			/*
			if (target == null) {
				Destroy(this.gameObject);
				return;
			}
			*/


			// Reflect the Player Health
			/*
			if (playerHealthSlider != null) {
				playerHealthSlider.value = target.Health;
			}
			*/
		}

		/// <summary>
		/// MonoBehaviour method called after all Update functions have been called. This is useful to order script execution.
		/// In our case since we are following a moving GameObject, we need to proceed after the player was moved during a particular frame.
		/// </summary>
		void LateUpdate () {

			// Do not show the UI if we are not visible to the camera, thus avoid potential bugs with seeing the UI, but not the player itself.
			if (targetRenderer!=null)
			{
				this._canvasGroup.alpha = targetRenderer.isVisible ? 1f : 0f;
			}
			
			// #Critical
			// Follow the Target GameObject on screen.
			if (targetTransform!=null)
			{
				targetPosition = targetTransform.position;
				targetPosition.y += characterControllerHeight;
				
				this.transform.position = Camera.main.WorldToScreenPoint (targetPosition) + screenOffset;
			}

		}




		#endregion

		#region Public Methods

		/// <summary>
		/// Assigns a Player Target to Follow and represent.
		/// </summary>
		/// <param name="target">Target.</param>
		public void SetTarget(PlayerManager _target){

			if (_target == null) {
				Debug.LogError("<Color=Red><b>Missing</b></Color> PlayMakerManager target for PlayerUI.SetTarget.", this);
				return;
			}

			// Cache references for efficiency because we are going to reuse them.
			this.target = _target;
            targetTransform = this.target.GetComponent<Transform>();
            targetRenderer = this.target.GetComponentInChildren<Renderer>();


            CharacterController _characterController = this.target.GetComponent<CharacterController> ();

			// Get data from the Player that won't change during the lifetime of this Component
			if (_characterController != null){
				characterControllerHeight = _characterController.height;
			}

			if (playerNameText != null) {
                playerNameText.text = this.target.photonView.Owner.NickName;
			}
		}

		#endregion

	}
}