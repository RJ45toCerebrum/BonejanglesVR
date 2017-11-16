using UnityEngine;
using System.Collections.Generic;
using System;

namespace com.EvolveVR.BonejanglesVR
{
    public class Pair<A, B>
    {
        public A first;
        public B second;
    }

    public class RubiksCube : MonoBehaviour
    {
        public enum FaceColor { Red, Green, Blue, Yellow, Orange, White};

        public const int cubeLength = 3;
        public const float errorMargin = 0.025f;

        [SerializeField]
        private Transform[] centers;
        // the for every face color in the faceColors array, its has its matching pivot in the facePivots array
        private FaceColor[] faceColors;
        // used for 3D rotation code and relative location in the 2D maps
        private Transform[] facePivots;
        private Dictionary<FaceColor, Pair<FaceColor, RubiksUnit>[,]> cubeMap;


        public Transform[] Centers
        {
            get { return centers; }
        }


        private void Start()
        {
            InitFaceColorsAndPivots();
            InitCubeMap();
            InitRubiksUnits();
        }

        private void InitCubeMap()
        {
            cubeMap = new Dictionary<FaceColor, Pair<FaceColor,RubiksUnit>[,] >();
            foreach(FaceColor fc in Enum.GetValues(typeof(FaceColor)))
            {
                Pair<FaceColor, RubiksUnit>[,] colorMap = new Pair<FaceColor, RubiksUnit>[cubeLength,cubeLength];
                cubeMap.Add(fc, colorMap);
                for(int i = 0; i < cubeLength; i++) 
                {
                    for(int j = 0; j < cubeLength; j++)
                        colorMap[i, j] = new Pair<FaceColor, RubiksUnit>();
                }
            }
        }

        private void InitFaceColorsAndPivots()
        {
            faceColors = new FaceColor[centers.Length];
            facePivots = new Transform[centers.Length];
            for (int i = 0; i < faceColors.Length; i++) {
                faceColors[i] = NameToFaceColor(centers[i].name);
                facePivots[i] = centers[i].parent.Find("Pivot");
            }
        }

        private void InitRubiksUnits() {
            RubiksUnit[] units = GetComponentsInChildren<RubiksUnit>();
            UpdateRubiksUnitMaps(units);
        }


        // - values = 0, near zero = 1, and positive = 2
        private int[] GetLocationRelativeToPivot(Transform faceTransform, Transform pivotTransform)
        {
            Vector3 relativeFacePosition = pivotTransform.InverseTransformPoint(faceTransform.position);
            var xRow = GetDimensionFromLocalPositionValue(relativeFacePosition.x);
            var yRow = GetDimensionFromLocalPositionValue(relativeFacePosition.y);
            return new int[] { xRow, yRow};
        }

        // The transform cube has 3 rows, 3 columns, and 3 depths
        // local positions with value < -0.02 means its in the Zero slot in that dimension
        // value > 0.02 means its in 2 slot in that dimension and 
        // if niether then close to or at zero so 1 slot of dimension
        private int GetDimensionFromLocalPositionValue(float value)
        {
            if (value < -errorMargin)
                return 0;
            else if (value > errorMargin)
                return 2;

            return 1;
        }

        private FaceColor NameToFaceColor(string tName)
        {
            switch (tName) {
                case "Red":
                    return FaceColor.Red;
                case "Green":
                    return FaceColor.Green;
                case "Blue":
                    return FaceColor.Blue;
                case "Yellow":
                    return FaceColor.Yellow;
                case "White":
                    return FaceColor.White;
                default:
                    return FaceColor.Orange;
            }
        }

        public FaceColor GetFaceSide(Transform t)
        {
            int index = 0;
            float closestDistance = (centers[index].position - t.position).sqrMagnitude;
            if (centers.Length > 1) {
                for (int i = 1; i < centers.Length; i++) {
                    float distance = (centers[i].position - t.position).sqrMagnitude;
                    if (distance < closestDistance) {
                        index = i;
                        closestDistance = distance;
                    }
                }
            }

            return faceColors[index];
        }

        // we only need to update adjacent faces to the center face that was rotated
        private FaceColor[] AdjacentFaces(FaceColor fc)
        {
            FaceColor[] fcolors = new FaceColor[4];
            switch(fc) 
            {
                case FaceColor.Blue:
                    fcolors[0] = FaceColor.White;
                    fcolors[1] = FaceColor.Green;
                    fcolors[2] = FaceColor.Red;
                    fcolors[3] = FaceColor.Orange;
                    break;
                case FaceColor.Green:
                    fcolors[0] = FaceColor.Blue;
                    fcolors[1] = FaceColor.White;
                    fcolors[2] = FaceColor.Red;
                    fcolors[3] = FaceColor.Yellow;
                    break;
                case FaceColor.Red:
                    fcolors[0] = FaceColor.Blue;
                    fcolors[1] = FaceColor.Green;
                    fcolors[2] = FaceColor.Orange;
                    fcolors[3] = FaceColor.Yellow;
                    break;
                case FaceColor.White:
                    fcolors[0] = FaceColor.Blue;
                    fcolors[1] = FaceColor.Orange;
                    fcolors[2] = FaceColor.Green;
                    fcolors[3] = FaceColor.Yellow;
                    break;
                case FaceColor.Yellow:
                    fcolors[0] = FaceColor.Orange;
                    fcolors[1] = FaceColor.White;
                    fcolors[2] = FaceColor.Red;
                    fcolors[3] = FaceColor.Green;
                    break;
                default:
                    break;
            }

            return fcolors;
        }

        private int GetClosestFaceIndex(Transform t)
        {
            int index = 0;
            float smallestSqrDist = (t.position - centers[index].position).sqrMagnitude;

            for(int i = 1; i< centers.Length; i++) 
            {
                float sqrDist = (t.position - centers[i].position).sqrMagnitude;
                if (sqrDist < smallestSqrDist) {
                    index = i;
                    smallestSqrDist = sqrDist;
                }
            }

            return index;
        }

        private void UpdateRubiksUnitMaps(RubiksUnit[] units)
        {
            foreach (RubiksUnit unit in units) 
            {
                foreach (Transform faceTransform in unit.Faces) 
                {
                    // Get Closest Face Index
                    int index = GetClosestFaceIndex(faceTransform);
                    FaceColor centerFaceColor = faceColors[index];
                    Transform pivot = facePivots[index];
                    int[] loc = GetLocationRelativeToPivot(faceTransform, pivot);

                    Pair<FaceColor, RubiksUnit> pair = cubeMap[centerFaceColor][loc[0], loc[1]];
                    pair.first = centerFaceColor;
                    pair.second = unit;
                }
            }
        }

        public bool CubeSolved()
        {
            foreach (FaceColor fc in cubeMap.Keys) 
            {
                Pair<FaceColor, RubiksUnit>[,] faces = cubeMap[fc];
                for(int i = 0; i < cubeLength; i++) 
                {
                    for (int j = 0; j < cubeLength; j++) {
                        if (faces[i, j].first != fc)
                            return false;
                    }
                }
            }

            return true;
        }
    }
}