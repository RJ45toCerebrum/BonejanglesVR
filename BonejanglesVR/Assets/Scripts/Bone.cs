using UnityEngine;
using VRTK;

namespace com.EvolveVR.BonejanglesVR
{
    [RequireComponent(typeof(VRTK_InteractableObject), typeof(VRTK_DestinationMarker), typeof(VRTK_DestinationPoint))]
    public class Bone : MonoBehaviour
    {
        private VRTK_DestinationMarker destinationMarker;
        private VRTK_ControllerEvents leftControllerEvents;
        private VRTK_ControllerEvents rightControllerEvents;

        private bool isBoneActive = false;

        void Awake() {
            destinationMarker = GetComponent<VRTK_DestinationMarker>();
            leftControllerEvents = GameObject.FindGameObjectWithTag("LeftController").GetComponent<VRTK_ControllerEvents>();
            rightControllerEvents = GameObject.FindGameObjectWithTag("RightController").GetComponent<VRTK_ControllerEvents>();
        }

        private void Start() {
            destinationMarker.DestinationMarkerEnter += OnPointerEnter;
            destinationMarker.DestinationMarkerExit += OnPointerExit;
        }

        private void Update()
        {
            if ((leftControllerEvents.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.TouchpadPress) ||
                rightControllerEvents.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.TouchpadPress)) &&
                isBoneActive) {
                transform.position = Vector3.Lerp(transform.position, rightControllerEvents.transform.position, 0.04f);
            }
        }

        private void OnPointerEnter(object o, DestinationMarkerEventArgs args) {
            isBoneActive = true;
            
        }

        private void OnPointerExit(object o, DestinationMarkerEventArgs args) {
            isBoneActive = false;
        }
    }
}