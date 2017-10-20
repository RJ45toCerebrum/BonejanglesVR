using UnityEngine;
using VRTK;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace com.EvolveVR.BonejanglesVR
{
    public class MouseSlide : MonoBehaviour
    {
        [SerializeField]
        private GameManager gameManager;

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

        public RectTransform uiCursor;
        public RectTransform buttonRectTransform;
        private Button button;
        private Vector3 lastPosition;

        [Range(0.0f, 1.0f)]
        public float timeBetweenPulses;
        protected float currentHapticTime;
        private VRTK_ControllerReference handRef;
        private VRTK_Pointer pointer;

        private void Awake() {
            mouseInteractable = GetComponent<VRTK_InteractableObject>();
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

            button = buttonRectTransform.GetComponent<Button>();
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
            if(IsCursorOverlap(buttonRectTransform)) {
                DeleteCode();
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

            Vector2 relativeMousePosition = GetMousePadPosition();
            Vector2 cursorPosition = new Vector2(relativeMousePosition.x, relativeMousePosition.y);
            if (flipCursorDirections)
                cursorPosition = new Vector2(relativeMousePosition.y, relativeMousePosition.x);
            if (invertX)
                cursorPosition.x *= -1;
            if (invertY)
                cursorPosition.y *= -1;

            uiCursor.localPosition = cursorPosition;
            if (IsCursorOverlap(buttonRectTransform))
                EventSystem.current.SetSelectedGameObject(button.gameObject);
            else
                EventSystem.current.SetSelectedGameObject(null);

            Haptics();
        }   

        private bool IsCursorOverlap(RectTransform rectT)
        {
            float localWidth = buttonRectTransform.rect.height * buttonRectTransform.localScale.y;
            float xMin = buttonRectTransform.localPosition.x - localWidth / 2;
            float xMax = buttonRectTransform.localPosition.x + localWidth / 2;

            float localHeight = buttonRectTransform.rect.height * buttonRectTransform.localScale.y;
            float yMin = buttonRectTransform.localPosition.y - localHeight / 2;
            float yMax = buttonRectTransform.localPosition.y + localHeight / 2;

            bool xInRange = uiCursor.localPosition.x > xMin && uiCursor.localPosition.x < xMax;
            bool yInRange = uiCursor.localPosition.y > yMin && uiCursor.localPosition.y < yMax;

            if (xInRange && yInRange)
                return true;

            return false;
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

        private void DeleteCode()
        {
            GameObject textGO= GameObject.Find("MonitorResultsText");
            Text t = textGO.GetComponent<Text>();
            t.text = "Virus Downloaded...";
        }
    }
}