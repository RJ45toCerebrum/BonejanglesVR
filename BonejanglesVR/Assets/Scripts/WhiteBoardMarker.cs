using VRTK;
using UnityEngine;

namespace com.EvolveVR.BonejanglesVR
{
    public class WhiteBoardMarker : MonoBehaviour
    {
        //private VRTK_InteractableObject interactableObject;
        public Transform raycastTransform;
        public RaycastTextureDraw whiteBoard;

        private void Start()
        {
            if (!whiteBoard)
                enabled = false;
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.tag == "WhiteBoard") 
            {
                RaycastHit hit;
                if (Physics.Raycast(raycastTransform.position, raycastTransform.forward, out hit, 0.5f))
                    whiteBoard.Draw(gameObject, hit);
            }
        }
    }
}