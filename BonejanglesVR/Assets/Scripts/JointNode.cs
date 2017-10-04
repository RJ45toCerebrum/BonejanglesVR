using VRTK;
using UnityEngine;
using System.Collections;

namespace com.EvolveVR.BonejanglesVR
{
    public class JointNode : MonoBehaviour
    {
        private static VRTK_ControllerReference leftControllerReference;
        private static VRTK_ControllerReference rightControllerReference;
        private VRTK_SnapDropZone snapDropZone;
        public string validObjectName;


        private void Awake()
        {
            snapDropZone = GetComponent<VRTK_SnapDropZone>();

            if (snapDropZone != null) {
                snapDropZone.ObjectSnappedToDropZone += ObjectSnapped;
                snapDropZone.ObjectUnsnappedFromDropZone += ObjectUnsnapped;
            }

            StartCoroutine(LateStart());
        }

        private IEnumerator LateStart()
        {
            yield return new WaitUntil(() =>
            {
                leftControllerReference = VRTK_ControllerReference.GetControllerReference(SDK_BaseController.ControllerHand.Left);
                return leftControllerReference.hand == SDK_BaseController.ControllerHand.Left;
            });

            rightControllerReference = VRTK_ControllerReference.GetControllerReference(SDK_BaseController.ControllerHand.Right);
        }


        private void ObjectUnsnapped(object sender, SnapDropZoneEventArgs e)
        {
            Rigidbody rb = e.snappedObject.GetComponent<Rigidbody>();
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.drag = 1.5f;
            rb.angularDrag = 0.2f;
            e.snappedObject.GetComponent<VRTK_InteractableObject>().enabled = true;

            // we only want to remove the connection IF the object was a valid one
            // this is because only valid objects add connections
            if (e.snappedObject.name == validObjectName) {
                GameManager gm = GameManager.Instance;
                if (gm)
                    gm.RemoveConnection();
            }
            else
                StartCoroutine(PingPongPulses());
        }

        private void ObjectSnapped(object sender, SnapDropZoneEventArgs e)
        {
            if (e.snappedObject.name != validObjectName) {
                snapDropZone.ForceUnsnap();
            }
            else {
                Rigidbody otherRB = e.snappedObject.GetComponent<Rigidbody>();
                otherRB.drag = 12.0f;
                otherRB.angularDrag = 20.0f;

                GameManager gm = GameManager.Instance;
                if (gm)
                    gm.AddConnection();

                VRTK_ControllerHaptics.TriggerHapticPulse(leftControllerReference, 1.0f, 0.035f, 0.02f);
                VRTK_ControllerHaptics.TriggerHapticPulse(rightControllerReference, 1.0f, 0.035f, 0.02f);
            }

        }

        private IEnumerator PingPongPulses()
        {
            for(int i = 0; i < 4; i++) {
                if(i % 2 == 0)
                    VRTK_ControllerHaptics.TriggerHapticPulse(leftControllerReference, 0.5f, 0.07f, 0.02f);
                else
                    VRTK_ControllerHaptics.TriggerHapticPulse(rightControllerReference, 0.5f, 0.07f, 0.02f);

                yield return new WaitForSeconds(0.15f);
            }  
        }
    }
}