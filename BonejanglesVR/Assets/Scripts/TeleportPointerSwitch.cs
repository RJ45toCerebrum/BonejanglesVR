using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class TeleportPointerSwitch : MonoBehaviour 
{
	public Collider[] ignoreColliders;
	private const string rightController = "RightController";
	private const string leftController = "LeftController";

	private VRTK_Pointer leftPointer;
	private VRTK_Pointer rightPointer;


	private void Awake()
	{
		GameObject leftConGO = GameObject.Find (leftController);
		GameObject rightConGO = GameObject.Find (rightController);
		leftPointer = leftConGO.GetComponent<VRTK_Pointer> ();
		rightPointer = rightConGO.GetComponent<VRTK_Pointer> ();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (ignoreColliders.Contains (other))
			return;

		string controllerName = isController (other);
		if (controllerName == leftController || controllerName == rightController)
			PointerSwitch (false);
	}


	private string isController(Collider other)
	{
		Transform otherParent = other.transform.parent;
		if (otherParent) 
		{
			otherParent = otherParent.parent;
			if (otherParent) 
			{
				if (otherParent.name == rightController)
					return rightController;
				else if (otherParent.name == leftController)
					return leftController;
			}
		}

		return null;
	}

	private void PointerSwitch(bool on)
	{
		if (on) {
			leftPointer.enabled = true;
			rightPointer.enabled = true;
		} else {
			leftPointer.enabled = false;
			rightPointer.enabled = false;
		}
	}
}
