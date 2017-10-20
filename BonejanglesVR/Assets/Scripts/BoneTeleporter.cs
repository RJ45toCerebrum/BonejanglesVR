using VRTK;
using UnityEngine;
using System.Collections.Generic;

namespace com.EvolveVR.BonejanglesVR
{
    public class BoneTeleporter : MonoBehaviour
    {
        private VRTK_InteractGrab interactGrab;
        private Vector3 beforeTeleHandPosition;
        private Vector3 afterTeleHandPosition;

        private void Awake() {
            interactGrab = GetComponent<VRTK_InteractGrab>();
            InitTeleportationEvents();
        }

        private void InitTeleportationEvents()
        {
            VRTK_BasicTeleport basicTeleport = FindObjectOfType<VRTK_BasicTeleport>();
            basicTeleport.Teleporting += BeforeTeleport;
            basicTeleport.Teleported += AfterTeleport;
        }

        private void AfterTeleport(object sender, DestinationMarkerEventArgs e)
        {
            GameObject grabbedObj = interactGrab.GetGrabbedObject();
            if (grabbedObj != null) 
            {
                if (grabbedObj.tag == "Bone") 
                {
                    afterTeleHandPosition = interactGrab.transform.position;
                    Vector3 offset = (afterTeleHandPosition - beforeTeleHandPosition);
                    GameObject[] allBones = GetAllBones(grabbedObj);
                    foreach(GameObject go in allBones) {
                        go.transform.position += offset;
                    }
                }
            }
        }

        private void BeforeTeleport(object sender, DestinationMarkerEventArgs e) {
            beforeTeleHandPosition = transform.position;
        }

        private GameObject[] GetAllBones(GameObject rootBone)
        {
            List<GameObject> bones = new List<GameObject>();
            bones.Add(rootBone);

            JointNode[] jointNodes = rootBone.GetComponentsInChildren<JointNode>();
            foreach (JointNode jn in jointNodes) {
                GameObject[] boneGOs = jn.GetConnectedBones(jn);
                if (boneGOs.Length > 0)
                    bones.AddRange(boneGOs);
            }

            return bones.ToArray();
        }
    }
}