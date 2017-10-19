using UnityEngine;
using VRTK;
using VRTK.GrabAttachMechanics;
using System.Collections;
using System;

namespace com.EvolveVR.BonejanglesVR
{
    [RequireComponent( typeof(ConfigurableJoint), typeof(VRTK_SpringJointGrabAttach))]
    public class LockableDrawer : VRTK_InteractableObject
    {
        public enum RotationAxis { X, Y, Z};

        private bool isLocked;
        // this means that can you lock or unlock it; this is needed because 
        // I need a period of time where the key cannot interact with it
        private bool isLockable = true;
        [Header("Drawer specific")]
        public bool lockOnStart = true;
        public Material lockMaterial;
        public Color lockColor;
        public Color unlockColor;
        private Rigidbody rb;


        private GameObject hand;
        private VRTK_InteractableObject keyIO;
        private bool keyInLock = false;
        private Quaternion startRotation;
        private Quaternion lastRotation;
        public float startAngle;
        public float endValue;
        public RotationAxis rotationAxis;
        public Transform keyInPlaceTransform;
        
        public bool IsLocked
        {
            get { return isLocked; }
        }

        protected override void Awake() {
            base.Awake();
            rb = GetComponent<Rigidbody>();
            SetLockState(lockOnStart);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Key" && !IsGrabbed() && isLockable) 
            {
                keyIO = other.GetComponent<VRTK_InteractableObject>();
                hand = keyIO.GetGrabbingObject();
                // this means the key likely fell from whne user was using a diff drawer above it
                if(!hand) {
                    keyIO = null;
                    return;
                }
                
                startRotation = hand.transform.rotation;
                lastRotation = startRotation;

                StartCoroutine(SetKeyInLock(other.transform));
            }
        }

        private void OnTriggerStay(Collider other)
        {
            // this checks the hands rotation
            if (other.tag == "Key" && !IsGrabbed() && isLockable && hand != null && keyInLock) 
            {
                // get the relative rotation from the starting rotation; Quaternion magic
                Quaternion relativeRotation = Quaternion.Inverse(startRotation) * hand.transform.rotation;
                bool isBetweenAngles = false;
                float dTheta = 0;
                if (rotationAxis == RotationAxis.X) {
                    isBetweenAngles = relativeRotation.eulerAngles.x > startAngle && relativeRotation.eulerAngles.x < endValue;
                    dTheta = relativeRotation.eulerAngles.x - lastRotation.eulerAngles.x;
                }
                else if (rotationAxis == RotationAxis.Y) {
                    isBetweenAngles = relativeRotation.eulerAngles.y > startAngle && relativeRotation.eulerAngles.y < endValue;
                    dTheta = relativeRotation.eulerAngles.y - lastRotation.eulerAngles.y;
                }
                else { 
                    isBetweenAngles = relativeRotation.eulerAngles.z > startAngle && relativeRotation.eulerAngles.z < endValue;
                    dTheta = relativeRotation.eulerAngles.z - lastRotation.eulerAngles.z;
                }

                // rotate the key
                other.transform.Rotate(keyInPlaceTransform.forward, -dTheta);

                if (isBetweenAngles)
                    SetLockState(!isLocked);

                lastRotation = relativeRotation;
            }
        }

        private void SetLockState(bool state)
        {
            if (!isLockable)
                return;

            isLocked = state;
            if (isLocked) 
            {
                lockMaterial.color = lockColor;
                rb.isKinematic = true;
                isGrabbable = false;
            }
            else {
                lockMaterial.color = unlockColor;
                isGrabbable = true;
                rb.isKinematic = false;
            }

            SetKeyState(true);
            StartCoroutine(LockState(1.0f));
            if(keyIO)
                keyIO.enabled = true;

            hand = null;
            keyIO = null;
            startRotation = Quaternion.identity;
            keyInLock = false;
        }

        private void SetKeyState(bool state)
        {
            if (!keyIO)
                return;

            keyIO.ForceStopInteracting();
            keyIO.ForceStopSecondaryGrabInteraction();
            keyIO.enabled = state;

            Rigidbody keyRB = keyIO.GetComponent<Rigidbody>();
            if (state) {
                keyRB.useGravity = true;
                keyRB.isKinematic = false;
            }
            else {
                keyRB.useGravity = false;
                keyRB.isKinematic = true;
            }
        }

        private IEnumerator LockState(float seconds)
        {
            isLockable = false;
            yield return new WaitForSeconds(seconds);
            isLockable = true;
        }

        private IEnumerator SetKeyInLock(Transform keyTransform)
        {
            SetKeyState(false);
            keyIO.transform.position = keyInPlaceTransform.position;
            keyIO.transform.rotation = keyInPlaceTransform.rotation;
            keyInLock = true;

            isGrabbable = false;
            yield return null;
        }
    }
}