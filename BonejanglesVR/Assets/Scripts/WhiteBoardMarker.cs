using VRTK;
using VRTK.GrabAttachMechanics;
using UnityEngine;

namespace com.EvolveVR.BonejanglesVR
{
    public class WhiteBoardMarker : MonoBehaviour
    {
        public VRTK_InteractableObject interactableObject;
        public Transform raycastTransform;
        public RaycastTextureDraw whiteBoard;
        public Renderer markerRenderer;
        public GameObject radialMenuLeftHand;
        public GameObject radialMenuRightHand;


        private void Start()
        {
            if (!whiteBoard)
                enabled = false;

            markerRenderer.material.color = whiteBoard.DrawingColor;
            interactableObject.InteractableObjectGrabbed += Grabbed;
            interactableObject.InteractableObjectUngrabbed += Ungrabbed;
        }

        private void Ungrabbed(object sender, InteractableObjectEventArgs e)
        {
            if (radialMenuLeftHand)
                radialMenuLeftHand.SetActive(false);
            if(radialMenuRightHand)
                radialMenuRightHand.SetActive(false);

            VRTK_Pointer pointer = e.interactingObject.GetComponent<VRTK_Pointer>();
            pointer.enabled = false;
        }

        private void Grabbed(object sender, InteractableObjectEventArgs e) {
            VRTK_Pointer pointer = e.interactingObject.GetComponent<VRTK_Pointer>();
            pointer.enabled = false;
            radialMenuLeftHand.SetActive(true);
            radialMenuRightHand.SetActive(true);
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.tag == "WhiteBoard") 
            {
                RaycastHit hit;
                Ray ray = new Ray(raycastTransform.position, raycastTransform.forward);
                if (Physics.Raycast(ray, out hit, 0.5f))
                    whiteBoard.Draw(gameObject, hit);
            }
        }


        public void BlackButtonClicked() {
            markerRenderer.material.color = Color.black;
            whiteBoard.SetColorBlock(Color.black);
        }

        public void GreenButtonClicked() {
            markerRenderer.material.color = Color.green;
            whiteBoard.SetColorBlock(Color.green);
        }

        public void RedButtonClicked() {
            markerRenderer.material.color = Color.red;
            whiteBoard.SetColorBlock(Color.red);
        }

        public void BlueButtonClicked() {
            markerRenderer.material.color = Color.blue;
            whiteBoard.SetColorBlock(Color.blue);
        }
    }
}