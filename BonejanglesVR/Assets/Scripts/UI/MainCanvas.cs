using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.EvolveVR.BonejanglesVR
{
	// controller for the computer canvas
	public class MainCanvas : MonoBehaviour
	{
		public MouseSlide mouse;
		public RectTransform contentRect;
		public RectTransform cursorTransform;
		public float scrollMultiplier = 500;
		// min max scroll values used for scrolling the list and clamping
		public float minScrollValue;
		public float maxScrollValue;
		private float previousY = 0;
		private bool startedSlide = false;
		public float timeToShowMessage = 5.0f;
		public Text timeText;


		private void Update() 
		{
			float secondsLeft = GameManager.Instance.AllowedSeconds;
			timeText.text = secondsLeft.ToString ();

			if (GameManager.Instance.GameOver)
				timeText.enabled = false;

			if (mouse.InUse)
			{
				if (startedSlide) 
				{
					float dY = (cursorTransform.localPosition.y - previousY) * scrollMultiplier;
					float y = cursorTransform.localPosition.y;
					Vector3 p = contentRect.localPosition;
					p.y = Mathf.Clamp (p.y + dY, minScrollValue, maxScrollValue); 
					contentRect.localPosition = p;
					previousY = cursorTransform.localPosition.y;
				}
				else {
					startedSlide = true;
					previousY = cursorTransform.localPosition.y;
				}
			} 
			else 
				startedSlide = false;
			
		}
	}
}