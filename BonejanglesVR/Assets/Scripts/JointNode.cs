using VRTK;
using UnityEngine;
using System.Collections;
using System.IO;
using System;
using SimpleJSON;


namespace com.EvolveVR.BonejanglesVR
{
    /// <summary>
    /// NOTE: Be very careful not to change the names of the Bones because
    /// these are used to validate snappable objects / bones and to get 
    /// references to downstream JointNodes in the Skeleton chain
    /// </summary>

    [RequireComponent(typeof(VRTK_SnapDropZone))]
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
        public enum ConnectionType { NotConnected, Correct, Incorrect };
        private JointType jointType = JointType.Character;
        private ConnectionType connectionType = ConnectionType.NotConnected;
        private JSONNode jointInfo;

        private static VRTK_ControllerReference leftControllerReference;
        private static VRTK_ControllerReference rightControllerReference;
        private VRTK_SnapDropZone snapDropZone;
        public string validObjectName;

        [Tooltip("This is the drag that is modified on the snapping rigidbody")]
        // these are rigidbody settings for dealing with hanging skeleton
        public float dragAmount = 12.0f;
        public float angulerDragAmount = 20.0f;
        private int originalSolverIters;
        private int originalSolverExitVelIters;

        [SerializeField]
        private JointNode parentNode;
        private JointNode[] childJointNodes;
        private bool isHanging = false;
        public bool modifySwingAngles = false;


        public bool IsSnapped
        {
            get { return snapDropZone.GetCurrentSnappedObject() != null; }
        }

        public ConnectionType Connection
        {
            get{return connectionType; }
        }

        private void Awake()
        {
            // initialize joint type
            if (GetComponent<CharacterJoint>() != null)
                jointType = JointType.Character;
            else if (GetComponent<HingeJoint>() != null)
                jointType = JointType.Hinge;
            else
                jointType = JointType.None;

            // initialize snapdrop zone events
            snapDropZone = GetComponent<VRTK_SnapDropZone>();
            if (snapDropZone != null) {
                snapDropZone.ObjectSnappedToDropZone += ObjectSnapped;
                snapDropZone.ObjectUnsnappedFromDropZone += ObjectUnsnapped;
            }

            // get ref to jointNodes of the snappable object; 
            // for setting joint angles with already snapped together bones on the the skeleton hanger
            GameObject snappableObject = GameObject.Find(validObjectName);
            if (snappableObject)
                childJointNodes = snappableObject.GetComponentsInChildren<JointNode>(); 
            else
                Debug.LogWarningFormat("Unable to find the snappable object {0} in the scene", validObjectName);

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
            rb.solverIterations = originalSolverIters;
            rb.solverVelocityIterations = originalSolverExitVelIters;
            rb.maxAngularVelocity = 7;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;

            e.snappedObject.GetComponent<VRTK_InteractableObject>().enabled = true;
            SetIsHanging(false);

            connectionType = ConnectionType.NotConnected;
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
                // set rigibody values
                Rigidbody rb = e.snappedObject.GetComponent<Rigidbody>();
                originalSolverIters = rb.solverIterations;
                originalSolverExitVelIters = rb.solverVelocityIterations;
                rb.drag = dragAmount;
                rb.angularDrag = angulerDragAmount;
                rb.solverIterations = 40;
                rb.solverVelocityIterations = 40;
                rb.maxAngularVelocity = 2.7f;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

                VRTK_ControllerHaptics.TriggerHapticPulse(leftControllerReference, 1.0f, 0.035f, 0.02f);
                VRTK_ControllerHaptics.TriggerHapticPulse(rightControllerReference, 1.0f, 0.035f, 0.02f);

                if (parentNode && parentNode.IsHanging())
                    SetIsHanging(true);

                // velocity manager can be used to mk sure the velocity
                connectionType = ConnectionType.Correct;
                GameManager gm = GameManager.Instance;
                if (gm)
                    gm.AddConnection();
            }
        }

        private void ModifyJointRotationAngles()
        {
            if (!modifySwingAngles)
                return;

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
                        cj.highTwistLimit = jl(jointInfo[htlJSON].AsFloat / 5);
                        cj.lowTwistLimit = jl(jointInfo[ltlJSON].AsFloat / 5);
                        cj.swing1Limit = jl(jointInfo[s1lJSON].AsFloat / 5);
                        cj.swing2Limit = jl(jointInfo[s2lJSON].AsFloat / 5);
                    }
                    else {
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
                        jointLim.min = jointInfo[minJSON].AsFloat / 5;
                        jointLim.max = jointInfo[maxJSON].AsFloat / 5;
                    }
                    else {
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

        private void SetIsHanging(bool value)
        {
            isHanging = value;
            ModifyJointRotationAngles();
            if (childJointNodes != null) 
            {
                foreach (JointNode jn in childJointNodes) {
                    if (jn.IsSnapped)
                        jn.SetIsHanging(value);
                }
            }
        }

        private bool IsHanging()
        {
            return isHanging;
        }
    }
}