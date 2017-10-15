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
            mousePadCenter = new Vector3(lowerLeft.position.x - width / 2,
                                         lowerLeft.position.y,
                                         lowerLeft.position.z - height / 2);

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
            VRDebug.Log("MouseUsed", 0);
            if(IsCursorOverlap(buttonRectTransform)) {
                VRDebug.Log("Mouse Click", 1);
            }
            else
                VRDebug.Log("", 1);
        }

        private IEnumerator InitGrabMechanic()
        {
            Renderer rend = GetComponent<Renderer>();
            rend.enabled = false;
            yield return new WaitForEndOfFrame();
            Debug.Log("End of frame Mouse Grabbed");
            Vector3 newPos = new Vector3();
            newPos.x = lowerLeft.position.x - width / 2;
            newPos.y = lowerLeft.position.y;
            newPos.z = lowerLeft.position.z - height / 2;
            transform.position = newPos;
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

            Vector2 relativeMouesPosition = GetMousePadPosition();
            uiCursor.localPosition = relativeMouesPosition;

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
    }
}