using UnityEngine;
using VRTK;

namespace com.EvolveVR.BonejanglesVR
{
    public class InteractableSlide : MonoBehaviour
    {
        VRTK_ControllerEvents controllerEvent;
        VRTK_ControllerReference handRef;
        [Range(0.0f, 1.0f)]
        public float timeBetweenPulses;
        private float currentHapticTime;
        GameObject hand;
        float lastHandRotationY = 0;
        bool firstClick = true;
        [Range(0, 1)]
        public float smoothingFactor = 0.2f;

        private void OnTriggerEnter(Collider other)
        {
            Transform parent = other.transform.parent;
            if (parent) 
            {
                parent = parent.parent;
                if (parent) 
                {
                    if (parent.tag == "RightController") {
                        hand = VRTK_DeviceFinder.GetControllerRightHand();
                        controllerEvent = parent.GetComponent<VRTK_ControllerEvents>();
                        handRef = VRTK_ControllerReference.GetControllerReference(hand);
                    }
                    else if (parent.tag == "LeftController") {
                        hand = VRTK_DeviceFinder.GetControllerLeftHand();
                        controllerEvent = parent.GetComponent<VRTK_ControllerEvents>();
                        handRef = VRTK_ControllerReference.GetControllerReference(hand);
                    }
                    firstClick = true;
                }
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (!hand)
                return;

            if (controllerEvent.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.TriggerClick)) {
                if (!firstClick) {
                    Vector3 controllerPos = controllerEvent.transform.position;
                    Vector3 newPosition = new Vector3(controllerPos.x, transform.position.y, controllerPos.z);
                    transform.position = Vector3.Lerp(transform.position, newPosition, smoothingFactor);

                    float dR = hand.transform.eulerAngles.y - lastHandRotationY;
                    transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y + dR, 0);
                    lastHandRotationY = hand.transform.eulerAngles.y;

                    currentHapticTime += Time.deltaTime;
                    if (currentHapticTime > timeBetweenPulses) {
                        VRTK_ControllerHaptics.TriggerHapticPulse(handRef, 0.3f);
                        currentHapticTime = Time.deltaTime;
                    }
                }
                else {
                    lastHandRotationY = hand.transform.eulerAngles.y;
                    firstClick = false;
                }
            }
            else
                firstClick = true;
        }

        private void OnTriggerExit(Collider other)
        {
            controllerEvent = null;
            hand = null;
            firstClick = true;
        }
    }
}