using System;
using UnityEngine;
using VRTK;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace com.EvolveVR.BonejanglesVR
{

    public class MouseSlide : MonoBehaviour
    {
        VRTK_InteractableObject mouseObject;

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

        private void Awake() {
            mouseObject = GetComponent<VRTK_InteractableObject>();
        }

        private void Start()
        {
            Vector3 widthVector = (lowerRight.position - lowerLeft.position);
            width = widthVector.magnitude;
            Vector3 heightVector = (upperLeft.position - lowerLeft.position);
            height = heightVector.magnitude;
            mousePadCenter = lowerLeft.position + widthVector / 2 + heightVector / 2;

            uiCursor.localPosition = new Vector3(0, 0, -0.01f);

            mouseObject.InteractableObjectGrabbed += MouseGrabbed;
            mouseObject.InteractableObjectUsed += OnMouseClick;

            button = buttonRectTransform.GetComponent<Button>();
        }

        private void MouseGrabbed(object sender, InteractableObjectEventArgs e) {
            Debug.Log("On Mouse Grabbed");
            StartCoroutine(InitGrabMechanic());
        }

        private void OnMouseClick(object sender, InteractableObjectEventArgs e)
        {
            if(IsCursorOverlap(buttonRectTransform)) {
                DeleteCode();
            }
        }

        private IEnumerator InitGrabMechanic()
        {
            Renderer rend = GetComponent<Renderer>();
            rend.enabled = false;
            yield return new WaitForEndOfFrame();

            transform.position = mousePadCenter;
            rend.enabled = true;
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
            if (!mouseObject.IsGrabbed())
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

        private void DeleteCode()
        {
            GameObject textGO= GameObject.Find("MonitorResultsText");
            Text t = textGO.GetComponent<Text>();
            t.text = "Virus Downloaded...";
        }
    }
}