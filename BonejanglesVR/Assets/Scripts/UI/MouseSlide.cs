using UnityEngine;
using VRTK;
using VRTK.GrabAttachMechanics;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace com.EvolveVR.BonejanglesVR
{
    public class MouseSlide : MonoBehaviour
    {
        private VRTK_InteractableObject mouseInteractable;

        public Transform lowerLeft;
        public Transform lowerRight;
        public Transform upperRight;
        public Transform upperLeft;
        private Vector3 mousePadCenter;
        public bool flipCursorDirections = false;
        public bool invertY = false;
        public bool invertX = false;

        private float width;
        private float height;
        private Vector3 monitorDiagnal;

        public AudioSource audioSource;
        public RectTransform uiCursor;
        public RectTransform[] buttonRectTransforms;
        private Vector3 lastPosition;

        [Range(0.0f, 1.0f)]
        public float timeBetweenPulses;
        protected float currentHapticTime;
        private VRTK_ControllerReference handRef;
        private VRTK_Pointer pointer; // pointer needed for the main scene

		public float maxHandDistanceFromMouse;


		public bool InUse
		{
			get  
			{
				return mouseInteractable.IsUsing ();
			}	
		}


        private void Awake() {
            mouseInteractable = GetComponent<VRTK_InteractableObject>();
            audioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            Vector3 widthVector = (lowerRight.position - lowerLeft.position);
            width = widthVector.magnitude;
            Vector3 heightVector = (upperLeft.position - lowerLeft.position);
            height = heightVector.magnitude;
            mousePadCenter = lowerLeft.position + widthVector / 2 + heightVector / 2;

            uiCursor.localPosition = new Vector3(0, 0, -0.01f);

            mouseInteractable.InteractableObjectGrabbed += MouseGrabbed;
            mouseInteractable.InteractableObjectUsed += OnMouseClick;
            mouseInteractable.InteractableObjectUngrabbed += MouseUngrabbed;
        }

        private void MouseGrabbed(object sender, InteractableObjectEventArgs e)
        {
            handRef = VRTK_ControllerReference.GetControllerReference(mouseInteractable.GetGrabbingObject());

            GameObject handGO = null;
            if (handRef.hand == SDK_BaseController.ControllerHand.Left)
                handGO = GameObject.Find("LeftController");
            else
                handGO = GameObject.Find("RightController");

            pointer = handGO.GetComponent<VRTK_Pointer>();

            StartCoroutine(InitGrabMechanic());
        }

        private void MouseUngrabbed(object sender, InteractableObjectEventArgs e) {
            ControllerPointerSwitch(true);
        }

        private void OnMouseClick(object sender, InteractableObjectEventArgs e)
        {
            RectTransform rt = GetOverlappingRT();
            if (rt) {
                Button button = rt.GetComponent<Button>();
                button.onClick.Invoke();
				if (audioSource) {
					audioSource.time = 3.3f;
					audioSource.Play ();
				} else
					Debug.LogWarning ("No Audio source for mouse click sound");
            }
        }

        private IEnumerator InitGrabMechanic()
        {
            ControllerPointerSwitch(false);

            Renderer[] rends = GetComponentsInChildren<Renderer>();
            foreach(Renderer r in rends)
                r.enabled = false;

            yield return new WaitForEndOfFrame();

            transform.position = mousePadCenter;
            foreach (Renderer r in rends)
                r.enabled = true;
        }

        private Vector2 GetMousePadPosition()
        {
            Vector2 mousePosRel = new Vector2();
            Vector3 pos = (transform.position - mousePadCenter);
            mousePosRel.x = -pos.x / width;
            mousePosRel.y = -pos.z / height;
            return mousePosRel;
        }


        private void Update()
        {
            if (!mouseInteractable.IsGrabbed())
                return;

			CursorMovement ();

            RectTransform rt = GetOverlappingRT();
            if (rt) {
                Button b = rt.GetComponent<Button>();
                if (b) {
                    EventSystem.current.SetSelectedGameObject(b.gameObject);
                    b.Select();
                }
            }
            else
                EventSystem.current.SetSelectedGameObject(null);


			// allow the payer to pull dettach from the mouse
			float offsetFromHand = (transform.position - handRef.actual.transform.position).magnitude;
			if (offsetFromHand > maxHandDistanceFromMouse)
				mouseInteractable.ForceStopInteracting ();

			Haptics();
        }

		private void CursorMovement()
		{
			Vector2 relativeMousePosition = GetMousePadPosition();
			Vector2 cursorPosition = new Vector2(relativeMousePosition.x, relativeMousePosition.y);
			if (flipCursorDirections)
				cursorPosition = new Vector2(relativeMousePosition.y, relativeMousePosition.x);
			if (invertX)
				cursorPosition.x *= -1;
			if (invertY)
				cursorPosition.y *= -1;

			uiCursor.localPosition = cursorPosition;
		}

        private bool IsCursorOverlap(RectTransform rectT)
        {
            float localWidth = rectT.rect.height * rectT.localScale.y;
            float xMin = rectT.localPosition.x - localWidth / 2;
            float xMax = rectT.localPosition.x + localWidth / 2;

            float localHeight = rectT.rect.height * rectT.localScale.y;
            float yMin = rectT.localPosition.y - localHeight / 2;
            float yMax = rectT.localPosition.y + localHeight / 2;

            bool xInRange = uiCursor.localPosition.x > xMin && uiCursor.localPosition.x < xMax;
            bool yInRange = uiCursor.localPosition.y > yMin && uiCursor.localPosition.y < yMax;

            if (xInRange && yInRange)
                return true;

            return false;
        }

        private RectTransform GetOverlappingRT()
        {
            foreach (RectTransform rt in buttonRectTransforms)  {
                if (IsCursorOverlap(rt))
                    return rt;
            }

            return null;
        }

        private void Haptics()
        {
            currentHapticTime += Time.deltaTime;
            if (currentHapticTime > timeBetweenPulses) {
                VRTK_ControllerHaptics.TriggerHapticPulse(handRef, 0.3f);
                currentHapticTime = Time.deltaTime;
            }
        }

        private void ControllerPointerSwitch(bool state)
        {
            if (!pointer)
                return;

            pointer.enableTeleport = state;
            VRTK_StraightPointerRenderer spr = pointer.GetComponent<VRTK_StraightPointerRenderer>();
            spr.enabled = state;
        }

		// I still have to fix thing in here
        public void OnGameStart() {
            mouseInteractable.ForceStopInteracting();
            mouseInteractable.grabAttachMechanicScript = null;
            Destroy(GetComponent<VRTK_CustomJointGrabAttach>());
            Destroy(GetComponent<SpringJoint>());
            Destroy(mouseInteractable);
            Destroy(this);
        }
    }
}