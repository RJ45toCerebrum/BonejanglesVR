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
        public Transform grabPoint;

        private Rigidbody rb;
        private ConfigurableJoint joint;

        private void Awake() {
            rb = interactableObject.GetComponent<Rigidbody>();
            JointBreakRelay relay = interactableObject.GetComponent<JointBreakRelay>();
        }

        private void Start()
        {
            if (!whiteBoard)
                enabled = false;

            markerRenderer.material.color = whiteBoard.drawingColor;
        }

        //private void OnTriggerEnter(Collider other)
        //{
        //    if (other.tag == "WhiteBoard") 
        //    {
        //        joint = interactableObject.GetComponent<ConfigurableJoint>();
        //        joint.xMotion = ConfigurableJointMotion.Limited;
        //        SoftJointLimit sj = joint.linearLimit;
        //        sj.limit = 0.05f;
        //        JointDrive drive = joint.xDrive;
        //        drive.positionSpring = 100;
        //    }
        //}

        private void OnTriggerStay(Collider other)
        {
            VRDebug.Log("Staying " + other.name, 0);
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