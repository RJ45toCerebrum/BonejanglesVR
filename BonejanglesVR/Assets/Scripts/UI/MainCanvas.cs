using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.EvolveVR.BonejanglesVR
{
	public class MainCanvas : MonoBehaviour
	{
	    // reference to the message panel to display to player when game starts
		public GameObject messagePanel;
		public GameObject objectivePanel;

		public Transform mouseTransform;
		public float timeToShowMessage = 5.0f;
		public Button objectiveButton;
		public Text timeText;

		private void Start() 
		{
			if (messagePanel == null) {
				Debug.LogError ("There is not message panel to show...");
				enabled = false;
				return;
			}
			else if (mouseTransform == null)
				Debug.LogWarning ("No mouse reference and thus user has to wait entire time to rid the message panel");

			StartCoroutine (ShowPlayerMessage());

			objectiveButton.onClick.AddListener (() => {
				StartCoroutine (ShowPlayerMessage());
			});
		}

		// coroutine for showing a message to the player
		private IEnumerator ShowPlayerMessage()
		{
			float seconds = 0;
			messagePanel.SetActive (true);
			Vector3 lastMousePosition = mouseTransform.position;

			while (seconds < timeToShowMessage) 
			{
				// Has the mouse moved? the mouse has move so make it go away
				if ((lastMousePosition - mouseTransform.position).sqrMagnitude > 0.000005f)
					break;

				if (GameManager.Instance.GameOver)
					break;

				lastMousePosition = mouseTransform.position;
				seconds += Time.deltaTime;
				yield return null;
			}

			if (GameManager.Instance.GameOver) {
				Text message = messagePanel.GetComponentInChildren<Text> ();
				message.text = "You a Scrub...";
				timeText.enabled = false;
			} else {
				messagePanel.SetActive (false);
			}
		}

		private void Update() {
			float secondsLeft = GameManager.Instance.AllowedSeconds;
			timeText.text = secondsLeft.ToString ();

			if (GameManager.Instance.GameOver)
				timeText.enabled = false;
		}
	}
}