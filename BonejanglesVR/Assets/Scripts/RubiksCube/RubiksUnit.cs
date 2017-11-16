using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace com.EvolveVR.BonejanglesVR
{
    public class RubiksUnit : MonoBehaviour
    {
        private static readonly string[] allowedColorNames = {"Red", "Green", "Blue", "Yellow", "Orange", "White"};
        private RubiksCube rubikCube;
        private List<Transform> faces;

        public List<Transform> Faces
        {
            get { return faces; }
        }

        private void Awake()
        {
            rubikCube = transform.parent.GetComponent<RubiksCube>();
            faces = new List<Transform>(1);
            foreach (Transform t in transform) 
            {
                if (allowedColorNames.Contains(t.name))
                    faces.Add(t);
            }
        }
    }
}