using System.Collections.Generic;
using UnityEngine;
using VRTK;

namespace com.EvolveVR.BonejanglesVR
{
    public class LegoBlock : MonoBehaviour
    {
        public bool debugMode = false;
        private VRTK_InteractableObject thisLegoInteractble;

        // dont forget about the weird trigger problem; this is why im doing this
        public GeneralTriggerEvent blockTriggerEvent;
        private float maxProngDistance = 0.025f;
        public const int minNumAllowedProngs = 4;
        public const int maxNumAllowedProng = 6;

        public Transform[] topProngs;
        public Transform[] bottomProngs;
        public GameObject centroidPrimitive;

        private LegoBlock otherLegoBlock;
        private List<Transform> otherProngsWithinRange;
        private List<Transform> myProngsInRange;

        public bool isAnchored = false;
        public const float floorHeight = 0.01f;


        public Transform[] BottomProngs
        {
            get { return bottomProngs; }
        }

        public VRTK_InteractableObject LegoInteractable
        {
            get { return thisLegoInteractble; }
        }

        public bool IsAnchored
        {
            set { isAnchored = value; }
            get { return isAnchored; }
        }


        private void Awake() {
            thisLegoInteractble = GetComponent<VRTK_InteractableObject>();
        }

        private void Start(){
            blockTriggerEvent.SubOnTriggerEnterEvent(OnBlockEnter);
            blockTriggerEvent.SubOnTriggerStayEvent(OnBlockStay);
            blockTriggerEvent.SubOnTriggerExitEvent(OnBlockExit);

            otherProngsWithinRange = new List<Transform>();
            myProngsInRange = new List<Transform>();
        }

        private void OnBlockEnter(Collider other){
            otherLegoBlock = other.GetComponent<LegoBlock>();
        }

        private void OnBlockStay(Collider other)
        {
            if (otherLegoBlock == null || !isAnchored || otherLegoBlock.IsAnchored)
                return;

            otherProngsWithinRange.Clear();
            myProngsInRange.Clear();

            Transform[] otherBottomProngs = otherLegoBlock.BottomProngs;
            for(int i = 0; i < otherBottomProngs.Length; i++) {
                Transform closestProngTransform = ClosestProng(topProngs[i].position, otherBottomProngs);
                float distance = (topProngs[i].position - closestProngTransform.position).magnitude;
                if (distance < maxProngDistance) 
                {
                    Debug.DrawLine(topProngs[i].position, closestProngTransform.position, Color.red, 0.1f);
                    if (!otherProngsWithinRange.Contains(closestProngTransform)) {
                        otherProngsWithinRange.Add(closestProngTransform);
                        myProngsInRange.Add(topProngs[i]);
                    }
                }
            }

            // after we add all of the prongs within range; we need to see if they are of the correct numbers
            if(otherProngsWithinRange.Count == minNumAllowedProngs)  {
                if (AreProngsAdjacent()) {
                    AnchorLego();
                    VRDebug.Log("pERFORM ANCHOR", 0);
                }
            }
        }

        private void OnBlockExit(Collider other) {
            otherLegoBlock = null;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "Floor")
                AnchorLegoToFloor();
        }

        private bool AreProngsAdjacent()
        {
            bool firstProngEncountered = false;
            int j = 0;
            for(int i = 0; j < minNumAllowedProngs; i++) 
            {
                if (topProngs[i] == myProngsInRange[j]) {
                    firstProngEncountered = true;
                    j++;
                }
                else if (firstProngEncountered)
                    return false;
            }

            return true;
        }

        private Vector3 CreateCentroid(List<Transform> ts)
        {
            Vector3 centroid = new Vector3();
            foreach(Transform t in ts) {
                centroid.x += t.position.x;
                centroid.y += t.position.y;
                centroid.z += t.position.z;
            }

            centroid /= otherProngsWithinRange.Count;
            return centroid;
        }

        private Transform ClosestProng(Vector3 position, Transform[] otherBottomProngs)
        {
            Transform closestTransform = otherBottomProngs[0];
            float closestProngDistance = Vector3.Distance(position, otherBottomProngs[0].position);

            for (int r = 0; r < otherBottomProngs.Length; r++) {
                float distance = Vector3.Distance(position, otherBottomProngs[r].position);
                if (distance < closestProngDistance) {
                    closestProngDistance = distance;
                    closestTransform = otherBottomProngs[r];
                }
            }

            return closestTransform;
        }

        private void AnchorLego()
        {
            // make sure to stop the current interaction
            if (thisLegoInteractble.IsGrabbed())
                thisLegoInteractble.ForceStopInteracting();

            // orient the lego and put it into place; a create a empty game object a proper position
            // and move that into place and then destroy it
            OrientTransform(otherLegoBlock.transform);
            Vector3 otherProngsCentroid = CreateCentroid(otherProngsWithinRange);
            Vector3 myProngsCentroid = CreateCentroid(myProngsInRange);
            GameObject otherCentroidGO = GameObject.Instantiate(centroidPrimitive);
            otherCentroidGO.transform.position = otherProngsCentroid;
            otherCentroidGO.transform.rotation = otherLegoBlock.transform.rotation;
            otherLegoBlock.transform.SetParent(otherCentroidGO.transform);
            Vector3 offset = (myProngsCentroid - otherProngsCentroid);
            otherCentroidGO.transform.position += offset;
            otherLegoBlock.transform.SetParent(null);
            Destroy(otherCentroidGO);

            // After block is oriented, we need to fixed it into place and make it ungrbbable for now
            // later Ill fix this
            otherLegoBlock.gameObject.AddComponent<FixedJoint>();
            otherLegoBlock.LegoInteractable.enabled = false;
            otherLegoBlock.IsAnchored = true;

            blockTriggerEvent.AddIgnoreCollider(otherLegoBlock.GetComponent<Collider>());

            // delete later
            debugMode = false;
        }

        private void AnchorLegoToFloor()
        {
            if(thisLegoInteractble.IsGrabbed())
                thisLegoInteractble.ForceStopInteracting();

            OrientTransform(transform);
            transform.position = new Vector3(transform.position.x, floorHeight, transform.position.z);
            FixedJoint fixedJoint = gameObject.AddComponent<FixedJoint>();
            thisLegoInteractble.enabled = false;
            isAnchored = true;
        }

        private void OrientTransform(Transform t)
        {
            //transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
            //1) zero out the x and the z axis
            float yAxisAngle = t.eulerAngles.y;

            // 2) find the nearest multiple of 90 degrees;
            float a = RoundDown(yAxisAngle, 90);
            float b = RoundUp(yAxisAngle, 90);
            float c = Mathf.Abs(a - yAxisAngle);
            float d = Mathf.Abs(b - yAxisAngle);
            float nearest90DegreeRotation = c <= d ? a : b;

            t.rotation = Quaternion.Euler(0, nearest90DegreeRotation, 0);
        }

        private float RoundDown(float v, int multiple)
        {
            float a = Mathf.Floor(v / multiple);
            float b = a * multiple;
            return b;
        }

        private float RoundUp(float v, int multiple)
        {
            if (multiple == 0)
                return 0;

            var remainder = Mathf.Abs(v) % multiple;
            if (remainder == 0)
                return v;
            if (v < 0)
                return -(Mathf.Abs(v) - remainder);
            else
                return v + multiple - remainder;
        }
    }
}