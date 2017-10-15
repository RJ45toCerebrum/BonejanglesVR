using UnityEngine;
using VRTK;
using System.Collections.Generic;

namespace com.EvolveVR.BonejanglesVR
{
    public abstract class InteractableSlide : MonoBehaviour
    {
        public string leftControllerTag = "LeftController";
        public string rightControllerTag = "RightController";

        protected VRTK_ControllerEvents controllerEvent;
        protected VRTK_ControllerReference handRef;
        protected VRTK_InteractGrab interactGrab;
        [Range(0.0f, 1.0f)]
        public float timeBetweenPulses;
        protected float currentHapticTime;
        protected GameObject hand;
        protected bool firstClick = true;
        protected bool isInteracting = false;

        [Range(0, 1)]
        public float smoothingFactor = 0.2f;

        public Collider[] ignoreColliders;
        private HashSet<Collider> collidersToIgnore;


        public bool IsInteracting
        {
            get
            {
                if (controllerEvent != null) {
                    if (controllerEvent.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.TriggerClick))
                        return true;
                }
                return false;
            }
        }

        public Vector3 InteractingHandPosition
        {
            get { return hand.transform.position; }
        }

        protected virtual void Start()
        {
            collidersToIgnore = new HashSet<Collider>();
            foreach (Collider ic in ignoreColliders)
                collidersToIgnore.Add(ic);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (collidersToIgnore.Contains(other))
                return;

            Transform parent = other.transform.parent;
            if (parent) 
            {
                parent = parent.parent;
                if (parent) 
                {
                    if (parent.tag == rightControllerTag) {
                        hand = VRTK_DeviceFinder.GetControllerRightHand();
                        controllerEvent = parent.GetComponent<VRTK_ControllerEvents>();
                        handRef = VRTK_ControllerReference.GetControllerReference(hand);
                        interactGrab = parent.GetComponent<VRTK_InteractGrab>();
                    }
                    else if (parent.tag == leftControllerTag) {
                        hand = VRTK_DeviceFinder.GetControllerLeftHand();
                        controllerEvent = parent.GetComponent<VRTK_ControllerEvents>();
                        handRef = VRTK_ControllerReference.GetControllerReference(hand);
                        interactGrab = parent.GetComponent<VRTK_InteractGrab>();
                    }
                    firstClick = true;
                }
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (!hand || collidersToIgnore.Contains(other) || interactGrab.GetGrabbedObject() != null)
                return;

            if (controllerEvent.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.TriggerClick)) {
                if (!firstClick) {
                    OnInteraction(other);
                }
                else {
                    firstClick = false;
                    OnInitialTriggerInteraction(other);
                }
            }
            else {
                firstClick = true;
                OnInteractionStopped(other);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            controllerEvent = null;
            hand = null;
            firstClick = true;
            OnInteractionStopped(other);
        }

        protected virtual void OnInitialTriggerInteraction(Collider other) {}

        protected abstract void OnInteraction(Collider other);

        protected virtual void OnInteractionStopped(Collider other) {}
    }
}