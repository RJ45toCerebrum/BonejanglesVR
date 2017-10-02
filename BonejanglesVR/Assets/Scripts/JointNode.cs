using VRTK;
using UnityEngine;

namespace com.EvolveVR.BonejanglesVR
{
    public class JointNode : MonoBehaviour
    {
        private VRTK_SnapDropZone snapDropZone;
        public string validObjectName;

        private void Awake()
        {
            snapDropZone = GetComponent<VRTK_SnapDropZone>();

            if (snapDropZone != null) {
                snapDropZone.ObjectSnappedToDropZone += ObjectSnapped;
                snapDropZone.ObjectUnsnappedFromDropZone += ObjectUnsnapped;
            }
        }

        private void ObjectUnsnapped(object sender, SnapDropZoneEventArgs e)
        {
            Rigidbody rb = e.snappedObject.GetComponent<Rigidbody>();
            rb.useGravity = true;
            rb.isKinematic = false;
            e.snappedObject.GetComponent<VRTK_InteractableObject>().enabled = true;

            // we only want to remove the connection IF the object was a valid one
            // this is because only valid objects add connections
            if(e.snappedObject.name == validObjectName)
                GameManager.Instance.RemoveConnection();
        }

        private void ObjectSnapped(object sender, SnapDropZoneEventArgs e)
        {
            if (e.snappedObject.name != validObjectName)
                snapDropZone.ForceUnsnap();
            else
                GameManager.Instance.AddConnection();
        }
    }
}