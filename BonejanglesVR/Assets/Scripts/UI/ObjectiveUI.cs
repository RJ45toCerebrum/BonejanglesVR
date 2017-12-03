using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.EvolveVR.BonejanglesVR
{
	public class ObjectiveUI : MonoBehaviour
	{
		// color --> Green means 
		public Image completedImage;
		public Color finishedColor;
		public Color notAvailableColor;
		public Color notFinishedColor;
		public Text messageText;

		public void ChangeStatus(Objective.ObjectiveStatus status) 
		{
			if (status == Objective.ObjectiveStatus.Available) {
				completedImage.color = notFinishedColor;
				messageText.color = Color.white;
			}
			else if (status == Objective.ObjectiveStatus.NotAvailable) {
				completedImage.color = notAvailableColor;
				messageText.color = Color.grey;
			}
			else
				completedImage.color = finishedColor;
		}

		public void ChangeMessage(string message){
			messageText.text = message;
		}
	}
}