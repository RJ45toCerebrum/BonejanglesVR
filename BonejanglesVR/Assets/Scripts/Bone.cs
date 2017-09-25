using UnityEngine;
using UnityEngine.UI;
using VRTK;
using VRTK.GrabAttachMechanics;
using VRTK.Highlighters;
using System.Collections;

namespace com.EvolveVR.BonejanglesVR
{
    public class Bone : MonoBehaviour
    {
        MeshCollider meshCollider;
        VRTK_InteractableObject interactableObject;

        private VRTK_DestinationPoint destinationPoint;
        private VRTK_ControllerEvents leftControllerEvents;
        private VRTK_ControllerEvents rightControllerEvents;

        private bool isBoneActive = false;
        private SkeletonInfo.Side side;
        private BoneInfo boneInfo;

        public Color boneHighlightColor;

        public VRTK_ObjectTooltip nameTooltip;
        public VRTK_ObjectTooltip functionTooltip;
        public float distanceToDisplayTooltips = 0.7f;


        //delete
        public Text text; 
        // end delete

        void Awake() {
            destinationPoint = GetComponent<VRTK_DestinationPoint>();
            leftControllerEvents = GameObject.FindGameObjectWithTag("LeftController").GetComponent<VRTK_ControllerEvents>();
            rightControllerEvents = GameObject.FindGameObjectWithTag("RightController").GetComponent<VRTK_ControllerEvents>();

            // delete
            text = GameObject.Find("BoneTextInfo").GetComponent<Text>();
        }

        private void AddMyComponents()
        {
            meshCollider = gameObject.AddComponent<MeshCollider>();
            meshCollider.isTrigger = false;
            meshCollider.inflateMesh = true;
            meshCollider.convex = true;
            meshCollider.skinWidth = 0.001f;

            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            destinationPoint = gameObject.AddComponent<VRTK_DestinationPoint>();
            destinationPoint.enabled = true;
            destinationPoint.enableTeleport = true;
            destinationPoint.snapToPoint = true;
            destinationPoint.hidePointerCursorOnHover = true;
            StartCoroutine(LateStart());


            interactableObject = gameObject.AddComponent<VRTK_InteractableObject>();
            interactableObject.allowedGrabControllers = VRTK_InteractableObject.AllowedController.Both;
            interactableObject.allowedTouchControllers= VRTK_InteractableObject.AllowedController.Both;
            interactableObject.disableWhenIdle = true;
            interactableObject.isGrabbable = true;
            interactableObject.holdButtonToGrab = true;
            interactableObject.touchHighlightColor = boneHighlightColor;
            interactableObject.isUsable = true;
            interactableObject.holdButtonToUse = true;
            interactableObject.pointerActivatesUseAction = true;
            interactableObject.grabOverrideButton = VRTK_ControllerEvents.ButtonAlias.GripPress;
            interactableObject.useOverrideButton = VRTK_ControllerEvents.ButtonAlias.GripPress;

            VRTK_FixedJointGrabAttach fixedJoint = gameObject.AddComponent<VRTK_FixedJointGrabAttach>();
            fixedJoint.enabled = true;
            fixedJoint.precisionGrab = true;
            fixedJoint.destroyImmediatelyOnThrow = false;

            VRTK_OutlineObjectCopyHighlighter highlighter = gameObject.AddComponent<VRTK_OutlineObjectCopyHighlighter>();
            highlighter.enabled = true;
            highlighter.active = true;
            highlighter.unhighlightOnDisable = true;
            highlighter.thickness = 0.1f;
        }

        private void Start()
        {
            VRDebug.Instance.Log("Start", 0);

            AddMyComponents();

            destinationPoint.DestinationMarkerEnter += OnPointerEnter;
            destinationPoint.DestinationMarkerExit += OnPointerExit;

            side = InitSymmetricVariables();
            boneInfo = SkeletonInfo.GetBoneData(name);
            boneInfo.BoneGameObject = gameObject;
            boneInfo.Side = this.side;
            SkeletonInfo.TryAddBone(boneInfo);
        }

        // this is specifically because VRTK has some very annoying code that 
        // makes my MeshCollider a trugger when I clearly dont want that 
        private IEnumerator LateStart()
        {
            yield return new WaitForSeconds(0.2f);
            meshCollider.enabled = true;
            meshCollider.isTrigger = false;


            if (nameTooltip != null && functionTooltip != null) {
                nameTooltip.displayText = boneInfo.BoneName;
                nameTooltip.gameObject.SetActive(false);
                functionTooltip.displayText = boneInfo.FunctionDescription;
                functionTooltip.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (rightControllerEvents.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.TouchpadPress) &&
                isBoneActive) {
                transform.position = Vector3.Lerp(transform.position, rightControllerEvents.transform.position, 0.04f);
            }

            if (nameTooltip != null && functionTooltip != null)
                TooltipRoutine();
        }

        private void TooltipRoutine()
        {
            if (isBoneActive) 
            {
                Transform t = VRTK_DeviceFinder.HeadsetTransform();
                if (t != null) 
                {
                    float distance = (t.position - transform.position).magnitude;
                    VRDebug.Instance.Log(distance.ToString(), 1);
                    VRDebug.Instance.Log("Hellow worlds", 2);
                    if (distance < distanceToDisplayTooltips) {
                        nameTooltip.gameObject.SetActive(true);
                        functionTooltip.gameObject.SetActive(true);
                    }
                }
            }
            else {
                nameTooltip.gameObject.SetActive(false);
                functionTooltip.gameObject.SetActive(false);
            }
        }

        private void OnPointerEnter(object o, DestinationMarkerEventArgs args) {
            isBoneActive = true;

            // delete---------------------------------------Later
            if (text != null) {
                text.text = boneInfo.BoneName + "\n\n";
                text.text += boneInfo.FunctionDescription + "\n\n";
                text.text += boneInfo.WikiURL;
            }
        }
        private void OnPointerExit(object o, DestinationMarkerEventArgs args) {
            isBoneActive = false;
        }

        private SkeletonInfo.Side InitSymmetricVariables()
        {
            GameObject lGO = GameObject.Find("L");
            GameObject rGO = GameObject.Find("R");
            bool isLeftBone = transform.IsChildOf(lGO.transform);
            bool isRightBone = transform.IsChildOf(rGO.transform);

            if (isLeftBone)
                return SkeletonInfo.Side.Left;
            else if (isRightBone)
                return SkeletonInfo.Side.Right;

            return SkeletonInfo.Side.None;
        }
    }
}