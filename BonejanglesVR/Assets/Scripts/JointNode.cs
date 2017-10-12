using VRTK;
using UnityEngine;
using System.Collections;
using System.IO;
using System;
using SimpleJSON;


namespace com.EvolveVR.BonejanglesVR
{
    public class JointNode : MonoBehaviour
    {
        public const string ltlJSON = "ltl";
        public const string htlJSON = "htl";
        public const string s1lJSON = "s1l";
        public const string s2lJSON = "s2l";
        public const string minJSON = "min";
        public const string maxJSON = "max";
        public const string jtypeJSON = "jtype";

        public enum JointType { Character, Hinge, None};
        private JointType jointType = JointType.Character;
        private JSONNode jointInfo;

        private static VRTK_ControllerReference leftControllerReference;
        private static VRTK_ControllerReference rightControllerReference;
        private VRTK_SnapDropZone snapDropZone;
        public string validObjectName;

        [Tooltip("This is the drag that is modified on the snapping rigidbody")]
        public float dragAmount = 12.0f;
        public float angulerDragAmount = 20.0f;

        [SerializeField]
        private JointNode parentNode;
        private bool isHanging = false;
        public bool modifySwingAngles = false;

        public bool IsHanging
        {
            get{ return isHanging; }
        }

        private void Awake()
        {
            if (GetComponent<CharacterJoint>() != null)
                jointType = JointType.Character;
            else if (GetComponent<HingeJoint>() != null)
                jointType = JointType.Hinge;
            else
                jointType = JointType.None;

            snapDropZone = GetComponent<VRTK_SnapDropZone>();

            if (snapDropZone != null) {
                snapDropZone.ObjectSnappedToDropZone += ObjectSnapped;
                snapDropZone.ObjectUnsnappedFromDropZone += ObjectUnsnapped;
            }

            StartCoroutine(LateStart());
        }

        private void Start()
        {
            if (parentNode == null)
                isHanging = true;

            if (modifySwingAngles) 
            {
                string filePath = Application.dataPath + "/Resources/BoneInfoText/" + name + ".json";
                if (jointType != JointType.None) {
                    if (File.Exists(filePath))
                        jointInfo = JSONClass.LoadFromFile(filePath);
                    else
                        Debug.LogWarning("Was not able to find joint info file and Not parent joint");
                }
            }
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
            this.isHanging = false;
            if(modifySwingAngles)
                HangingJointAngleModification(false);

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
            else 
            {
                Rigidbody otherRB = e.snappedObject.GetComponent<Rigidbody>();
                otherRB.drag = dragAmount;
                otherRB.angularDrag = angulerDragAmount;

                GameManager gm = GameManager.Instance;
                if (gm)
                    gm.AddConnection();

                VRTK_ControllerHaptics.TriggerHapticPulse(leftControllerReference, 1.0f, 0.035f, 0.02f);
                VRTK_ControllerHaptics.TriggerHapticPulse(rightControllerReference, 1.0f, 0.035f, 0.02f);

                if (parentNode && parentNode.IsHanging) {
                    this.isHanging = true;
                    if (modifySwingAngles)
                        HangingJointAngleModification(true);
                }

                VRDebug.Log("Is hanging: " + this.isHanging, 0);
            }

        }

        private void HangingJointAngleModification(bool isHanging)
        {
            Func<float, SoftJointLimit > jl = (v) =>
            {
                SoftJointLimit sjl = new SoftJointLimit();
                sjl.bounciness = 0;
                sjl.contactDistance = 1.0f;
                sjl.limit = v;
                return sjl;
            };

            switch (jointType) 
            {
                case JointType.Character:
                    CharacterJoint cj = GetComponent<CharacterJoint>();
                    if (isHanging) {
                        VRDebug.Log("Angles values reduced", 1);
                        cj.highTwistLimit = jl(jointInfo[htlJSON].AsFloat / 5);
                        cj.lowTwistLimit = jl(jointInfo[ltlJSON].AsFloat / 5);
                        cj.swing1Limit = jl(jointInfo[s1lJSON].AsFloat / 5);
                        cj.swing2Limit = jl(jointInfo[s2lJSON].AsFloat / 5);
                    }
                    else {
                        VRDebug.Log("Setting values back", 2);
                        cj.highTwistLimit = jl(jointInfo[htlJSON].AsFloat);
                        cj.lowTwistLimit = jl(jointInfo[ltlJSON].AsFloat);
                        cj.swing1Limit = jl(jointInfo[s1lJSON].AsFloat);
                        cj.swing2Limit = jl(jointInfo[s2lJSON].AsFloat);
                    }
                    break;
                case JointType.Hinge:
                    HingeJoint hj = GetComponent<HingeJoint>();
                    JointLimits jointLim = hj.limits;
                    if (isHanging) {
                        VRDebug.Log("Angles values reduced", 1);
                        jointLim.min = jointInfo[minJSON].AsFloat / 5;
                        jointLim.max = jointInfo[maxJSON].AsFloat / 5;
                    }
                    else {
                        VRDebug.Log("Setting values back", 2);
                        jointLim.min = jointInfo[minJSON].AsFloat;
                        jointLim.max = jointInfo[maxJSON].AsFloat;
                    }
                    break;
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