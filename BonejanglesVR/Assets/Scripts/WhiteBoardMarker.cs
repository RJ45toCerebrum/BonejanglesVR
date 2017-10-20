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


        private void Start()
        {
            if (!whiteBoard)
                enabled = false;

            markerRenderer.material.color = whiteBoard.drawingColor;
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

    }
}