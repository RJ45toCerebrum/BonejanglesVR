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
        private bool isLocked;
        // this means that can you lock or unlock it; this is needed because 
        // I need a period of time where the key cannot interact with it
        private bool isLockable = true;
        public bool lockOnStart = true;
        public Material lockMaterial;
        public Transform closedPosition;
        public Color lockColor;
        public Color unlockColor;
        private ConfigurableJoint configJoint;
        private Rigidbody rb;

        private Quaternion startRotation;
        private float lastXRotation;
        
        public bool IsLocked
        {
            get { return isLocked; }
        }

        protected override void Awake() {
            base.Awake();
            configJoint = GetComponent<ConfigurableJoint>();
            rb = GetComponent<Rigidbody>();
            SetLockState(lockOnStart);
        }

        private void OnTriggerEnter(Collider other) {
            if (other.tag == "Key" && !IsGrabbed() && isLockable)
                startRotation = other.transform.rotation;
        }

        private void OnTriggerStay(Collider other)
        {
            if(other.tag == "Key" && !IsGrabbed() && isLockable) 
            {
                Quaternion relative = Quaternion.Inverse(startRotation) * other.transform.rotation;
                VRDebug.Log(relative.eulerAngles.y.ToString(), 0);
                if (relative.eulerAngles.y > 50 && relative.eulerAngles.y < 60) {
                    SetLockState(!isLocked);
                    VRDebug.Log("Seting state", 0);
                }
            }
        }

        private void SetLockState(bool state)
        {
            if (!isLockable)
                return;

            isLocked = state;
            if (isLocked) {
                lockMaterial.color = lockColor;
                rb.isKinematic = true;
                isGrabbable = false;
            }
            else {
                lockMaterial.color = unlockColor;
                isGrabbable = true;
                rb.isKinematic = false;
            }

            StartCoroutine(LockState(1.5f));
        }

        private IEnumerator CloseDrawerRoutine()
        {
            Vector3 offset = (closedPosition.position - transform.position);
            while(offset.sqrMagnitude > 0.00f)  {
                transform.position = Vector3.Lerp(transform.position, closedPosition.position, 0.1f);
                offset = (closedPosition.position - transform.position);
                yield return null;
            }
        }

        private IEnumerator LockState(float seconds)
        {
            isLockable = false;
            yield return new WaitForSeconds(seconds);
            isLockable = true;
        }
    }
}