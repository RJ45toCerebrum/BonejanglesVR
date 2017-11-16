using UnityEngine;

namespace com.EvolveVR.BonejanglesVR
{
    // Holds information such its "centers" and Pivot
    // A RubikcCenter Never moves relative to its parent; Because of this, 
    // its 'centers' are always the same
    public class RubikCenter : MonoBehaviour
    {
        [SerializeField]
        private Transform[] centers;
        private RubiksCube.FaceColor[] faceColors;

        public Transform[] Centers
        {
            get { return centers; }
        }
        
        private void Start()
        {
            faceColors = new RubiksCube.FaceColor[centers.Length];
            for(int i = 0; i < faceColors.Length; i++)
                faceColors[i] = NameToFaceColor(centers[i].name);
        }

        private RubiksCube.FaceColor NameToFaceColor(string tName)
        {
            switch(tName)
            {
                case "Red":
                    return RubiksCube.FaceColor.Red;
                case "Green":
                    return RubiksCube.FaceColor.Green;
                case "Blue":
                    return RubiksCube.FaceColor.Blue;
                case "Yellow":
                    return RubiksCube.FaceColor.Yellow;
                case "White":
                    return RubiksCube.FaceColor.White;
                default:
                    return RubiksCube.FaceColor.Orange;
            }
        }

        // This is qureied from the RubikUnit and it essentially says which face center
        // is closest and thus which side is this face on; Red, Blue, Green, Yellow, White, or Orange
        // Which side is determined by the Center face of that side; If the center is red then its red side
        public RubiksCube.FaceColor GetFaceSide(Transform t)
        {
            int index = 0;
            float closestDistance = (centers[index].position - t.position).sqrMagnitude;
            if (centers.Length > 1) 
            {
                for(int i = 1; i < centers.Length; i++) 
                {
                    float distance = (centers[i].position - t.position).sqrMagnitude;
                    if(distance < closestDistance) {
                        index = i;
                        closestDistance = distance;
                    }
                }
            }

            return faceColors[index];
        }
    }
}