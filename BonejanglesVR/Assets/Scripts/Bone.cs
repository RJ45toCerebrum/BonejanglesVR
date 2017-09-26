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
        private VRTK_InteractableObject interactableObject;

        private VRTK_DestinationPoint destinationPoint;
        private VRTK_ControllerEvents leftControllerEvents;
        private VRTK_ControllerEvents rightControllerEvents;

        private bool isBoneActive = false;
        private SkeletonInfo.Side side;
        private BoneInfo boneInfo;

        public Color boneHighlightColor;
        public Color boneSelectedColor;
        private VRTK_OutlineObjectCopyHighlighter boneHighlighter;
        public VRTK_ObjectTooltip nameTooltip;
        public float distanceToDisplayTooltips = 0.7f;


        public BoneInfo BoneInfo
        {
            get { return boneInfo; }
        }


        void Awake() {
            destinationPoint = GetComponent<VRTK_DestinationPoint>();
            leftControllerEvents = GameObject.FindGameObjectWithTag("LeftController").GetComponent<VRTK_ControllerEvents>();
            rightControllerEvents = GameObject.FindGameObjectWithTag("RightController").GetComponent<VRTK_ControllerEvents>();
        }

        private void AddMyComponents()
        {
            MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
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

            boneHighlighter = gameObject.AddComponent<VRTK_OutlineObjectCopyHighlighter>();
            boneHighlighter.enabled = true;
            boneHighlighter.active = true;
            boneHighlighter.unhighlightOnDisable = true;
            boneHighlighter.thickness = 0.1f;
        }

        private void Start()
        { 
            AddMyComponents();
            boneHighlighter.active = true;
            boneHighlighter.unhighlightOnDisable = false;
            boneHighlighter.Highlight(Color.red, float.MaxValue);

            destinationPoint.DestinationMarkerEnter += OnPointerEnter;
            destinationPoint.DestinationMarkerExit += OnPointerExit;

            side = InitSymmetricVariables();
            boneInfo = SkeletonInfo.GetBoneData(name);
            boneInfo.BoneGameObject = gameObject;
            boneInfo.Side = this.side;
            SkeletonInfo.TryAddBone(BoneInfo);
        }

        // this is specifically because VRTK has some very annoying code that 
        // makes my MeshCollider a trugger when I clearly dont want that 
        private IEnumerator LateStart()
        {
            yield return new WaitForSeconds(0.2f);
            MeshCollider mc = GetComponent<MeshCollider>();
            mc.enabled = true;
            mc.isTrigger = false;

            if (nameTooltip != null) {
                nameTooltip.displayText = BoneInfo.BoneName;
                nameTooltip.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (nameTooltip != null)
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
                    if (distance < distanceToDisplayTooltips) {
                        nameTooltip.gameObject.SetActive(true);
                    }
                }
            }
            else
                nameTooltip.gameObject.SetActive(false);
        }

        private void OnPointerEnter(object o, DestinationMarkerEventArgs args) {
            isBoneActive = true;
            SkeletonInfo.CurrentActiveBone = this;
        }
        private void OnPointerExit(object o, DestinationMarkerEventArgs args) {
            isBoneActive = false;
            SkeletonInfo.CurrentActiveBone = null;
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

        public void SetBoneHighlight(bool highlight) {
            if (highlight)
                boneHighlighter.Highlight(boneSelectedColor);
            else
                boneHighlighter.Unhighlight();
        }
    }
}