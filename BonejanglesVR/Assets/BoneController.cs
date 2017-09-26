using UnityEngine;
using VRTK;

namespace com.EvolveVR.BonejanglesVR
{
    // used to controll the state of bone such as whether they are active pr nots
    public class BoneController : MonoBehaviour
    {
        private VRTK_ControllerEvents controllerEvents;
        private Bone activeBone;

        public float lerpAmount = 0.03f;

        // Use this for initialization
        void Awake() {
            controllerEvents = GetComponent<VRTK_ControllerEvents>();
        }

        private void Update()
        {
            activeBone = SkeletonInfo.CurrentActiveBone;
            if (SkeletonInfo.CurrentActiveBone == null)
                return;

            Vector2 v = controllerEvents.GetTouchpadAxis();
            if (controllerEvents.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.TouchpadPress)) 
            {
                // then they are trying to pull
                if (v.y > 0) {
                    if ((activeBone.transform.position - transform.position).sqrMagnitude > 0.02f)
                        activeBone.transform.position = Vector3.Lerp(activeBone.transform.position, controllerEvents.transform.position, lerpAmount);
                }
                // then they are trying to select
                else if (v.y < -0.4f) {
                    SkeletonInfo.SelectedBone = SkeletonInfo.CurrentActiveBone;
                    VRDebug.Log(SkeletonInfo.SelectedBone.BoneInfo.BoneName, 0);
                }
            }
        }
    }
}