using VRTK;
using UnityEngine;
using System;

namespace com.EvolveVR.BonejanglesVR
{
    public class SkeletonHanger : InteractableSlide
    {
        [Range(0, 1)]
        public float rotationSmoothFactor = 0.2f;
        float lastHandRotationY = 0;

        protected override void OnInitialTriggerInteraction(Collider other) {
            lastHandRotationY = hand.transform.eulerAngles.y;
        }

        protected override void OnInteraction(Collider other)
        {
            Vector3 controllerPos = InteractingHandPosition;
            Vector3 newPosition = new Vector3(controllerPos.x, transform.position.y, controllerPos.z);
            transform.position = Vector3.Lerp(transform.position, newPosition, smoothingFactor);

            float dR = hand.transform.eulerAngles.y - lastHandRotationY;
            Quaternion newRotation = Quaternion.Euler(0, transform.eulerAngles.y + dR, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, rotationSmoothFactor);
            lastHandRotationY = hand.transform.eulerAngles.y;

            currentHapticTime += Time.deltaTime;
            if (currentHapticTime > timeBetweenPulses) {
                VRTK_ControllerHaptics.TriggerHapticPulse(handRef, 0.3f);
                currentHapticTime = Time.deltaTime;
            }
        }
    }
}