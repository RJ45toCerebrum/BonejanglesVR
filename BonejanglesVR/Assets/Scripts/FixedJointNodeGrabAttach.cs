using VRTK.GrabAttachMechanics;
using VRTK;
using UnityEngine;

namespace com.EvolveVR.BonejanglesVR
{
    /// <summary>
    /// This script is to prevent problems with bones falling apart when they are attached together
    /// and player is holding them; The reason is because onGrabCollisionDelay is needed for
    /// smooth grabing, but produces the bone disjoint; this script is to mitigate this problem
    /// </summary>
    public class FixedJointNodeGrabAttach : VRTK_FixedJointGrabAttach
    {
        private VRTK_InteractableObject interactableObject;
        private JointNode[] jointNodes;
        private float ogOnGrabCollisionDelay = 0;

        protected override void Awake() {
            base.Awake();
            interactableObject = GetComponent<VRTK_InteractableObject>();
            jointNodes = GetComponentsInChildren<JointNode>();
        }
        
        private void Start() {
            interactableObject.InteractableObjectGrabbed += InteractableGrabbed;
            interactableObject.InteractableObjectUngrabbed += InteractableUngrabbed;
            ogOnGrabCollisionDelay = onGrabCollisionDelay;
        }

        private void InteractableUngrabbed(object sender, InteractableObjectEventArgs e) {
            onGrabCollisionDelay = ogOnGrabCollisionDelay;
        }

        private void InteractableGrabbed(object sender, InteractableObjectEventArgs e)
        {
            foreach(JointNode joint in jointNodes) {
                if (joint.Connection == JointNode.ConnectionType.Correct) {
                    onGrabCollisionDelay = 0;
                    return;
                }
            }
        }
    }
}