using UnityEngine;
using VRTK;

namespace com.EvolveVR.BonejanglesVR
{
    public class AutoHighlightOnGrab : MonoBehaviour
    {
        public VRTK_SnapDropZone snapDropZone;
        public GameObject highlightContainer;
        public VRTK_InteractableObject interactable;

        private void Awake()
        {
            if (snapDropZone == null || interactable == null || highlightContainer == null) {
                Debug.LogWarning("The script cannot do anything without ALL references. From " + name);
                return;
            }
        }

        private void Start()
        {
            if (snapDropZone != null && interactable != null && highlightContainer) {
                interactable.InteractableObjectGrabbed += OnInteraction;
                interactable.InteractableObjectUngrabbed += OnInteractionStopped;

                snapDropZone.highlightAlwaysActive = true;
                highlightContainer.SetActive(false);
            }
        }

        private void OnInteractionStopped(object sender, InteractableObjectEventArgs e) {
            highlightContainer.SetActive(false);
        }

        private void OnInteraction(object sender, InteractableObjectEventArgs e) {
            highlightContainer.SetActive(true);
        }
    }
}