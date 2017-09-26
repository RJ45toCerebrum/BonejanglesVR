using UnityEngine;
using UnityEngine.UI;

namespace com.EvolveVR.BonejanglesVR
{
    public class BoneInfoDisplayer : MonoBehaviour
    {
        public Text boneNameText;
        public Text boneFunctionsText;

        private void Start() {
            SkeletonInfo.SubSelectedBoneEvent(OnBoneSelected);
        }

        private void OnBoneSelected(Bone bone)
        {
            if(bone != null) 
            {
                boneNameText.text = bone.BoneInfo.BoneName;
                boneFunctionsText.text = "";
                foreach (string funcStr in bone.BoneInfo.Functions)
                    boneFunctionsText.text += funcStr + "\n\n";
            }
            else {
                boneNameText.text = "";
                boneFunctionsText.text = "";
            }
        }
    }
}