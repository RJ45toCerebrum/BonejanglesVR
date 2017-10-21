using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRTK;

namespace com.EvolveVR.BonejanglesVR
{
    public class LegoBlock : MonoBehaviour
    {
        private class ProngInfoList
        {
            public struct ProngInfo
            {
                public int prongIndex;
                public Transform prongTransform;
            }

            public List<ProngInfo> otherProngsWithinRange;
            public List<ProngInfo> myProngsInRange;

            public ProngInfoList()
            {
                otherProngsWithinRange = new List<ProngInfo>();
                myProngsInRange = new List<ProngInfo>();
            }

            /// <param name="index"> which index in the list does Transform of lego prong occur</param>
            /// <param name="t">The prongs transform</param>
            /// <param name="thisBlock">indicate whether this information is from this block or otherblock</param>
            public void AddNew(int index, Transform t, bool thisBlock = true)
            {
                if (!Contains(t, thisBlock)) 
                {
                    ProngInfo pi = new ProngInfo();
                    pi.prongIndex = index;
                    pi.prongTransform = t;

                    if (thisBlock)
                        myProngsInRange.Add(pi);
                    else
                        otherProngsWithinRange.Add(pi);
                }
            }

            private bool Contains(Transform t, bool thisBlock)
            {
                if (thisBlock) 
                {
                    return myProngsInRange.Any((p) => {
                        return p.prongTransform == t;
                    });
                }

                return otherProngsWithinRange.Any((p) => {
                    return p.prongTransform == t;
                });
            }

            public bool AreProngsAdjacent()
            {
                if (myProngsInRange.Count == minProngNumber)
                    return AdjacencySnakeMethod() || AdjacencyLinearMethod();

                return AdjacencySnakeMethod();
            }

            // uses the snake method to check if adjacent
            private bool AdjacencySnakeMethod()
            {
                int lastNumber = myProngsInRange[0].prongIndex;
                for(int i = 1; i < myProngsInRange.Count; i++)
                {
                    if ((myProngsInRange[i].prongIndex - lastNumber) == 1)
                        lastNumber = myProngsInRange[i].prongIndex;
                    else
                        return false;
                }

                return true;
            }

            // uses the Linear method to check adjacency
            private bool AdjacencyLinearMethod()
            {
                int lastNumber = myProngsInRange[0].prongIndex;
                for (int i = 1; i < myProngsInRange.Count; i++) {
                    if ((myProngsInRange[i].prongIndex - lastNumber) == 2)
                        lastNumber = myProngsInRange[i].prongIndex;
                    else
                        return false;
                }

                return true;
            }

            public void Clear()
            {
                otherProngsWithinRange.Clear();
                myProngsInRange.Clear();
            }

            public bool AllowedNumberOfProngs()
            {
                return myProngsInRange.Count == normalProngNumber ||
                       myProngsInRange.Count == minProngNumber;
            }

            public List<Transform> ProngTransforms(bool thisBlock)
            {
                List<Transform> transforms = new List<Transform>();
                if (thisBlock)
                    myProngsInRange.ForEach((p) => { transforms.Add(p.prongTransform); });
                else
                    otherProngsWithinRange.ForEach((p) => { transforms.Add(p.prongTransform); });

                return transforms;
            }
        }

        public bool debugMode = false;
        private VRTK_InteractableObject thisLegoInteractble;

        // dont forget about the weird trigger problem; this is why im doing this
        public GeneralTriggerEvent blockTriggerEvent;
        private float maxProngDistance = 0.035f;
        public const int normalProngNumber = 4;
        public const int minProngNumber = 2;
        
        public Transform[] topProngs;
        public Transform[] bottomProngs;
        public GameObject centroidPrimitive;

        private LegoBlock otherLegoBlock;
        private ProngInfoList prongInfoList;

        public bool isAnchored = false;
        public float breakForce = 500f;
        public const float floorHeight = 0.01f;
        private BoxCollider boxCollider;


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
            boxCollider = GetComponent<BoxCollider>();
        }

        private void Start() {
            blockTriggerEvent.SubOnTriggerEnterEvent(OnBlockEnter);
            blockTriggerEvent.SubOnTriggerStayEvent(OnBlockStay);
            blockTriggerEvent.SubOnTriggerExitEvent(OnBlockExit);

            prongInfoList = new ProngInfoList();
        }


        private void OnBlockEnter(Collider other){
            otherLegoBlock = other.GetComponent<LegoBlock>();
        }

        private void OnBlockStay(Collider other)
        {
            if (otherLegoBlock == null || !isAnchored || otherLegoBlock.IsAnchored)
                return;

            prongInfoList.Clear();

            Transform[] otherBottomProngs = otherLegoBlock.BottomProngs;
            for(int i = 0; i < topProngs.Length; i++) 
            {
                ProngInfoList.ProngInfo closestProng = ClosestProng(topProngs[i].position, otherBottomProngs);
                float distance = (topProngs[i].position - closestProng.prongTransform.position).magnitude;
                if (distance < maxProngDistance) 
                {
                    Debug.DrawLine(topProngs[i].position, closestProng.prongTransform.position, Color.red, 0.1f);
                    prongInfoList.AddNew(i, topProngs[i], thisBlock: true);
                    prongInfoList.AddNew(closestProng.prongIndex, closestProng.prongTransform, thisBlock: false);
                }
            }

            // after we add all of the prongs within range; we need to see if they are of the correct numbers
            if(prongInfoList.AllowedNumberOfProngs())  
            {
                if (prongInfoList.AreProngsAdjacent())
                    AnchorLego();
            }
        }

        private void OnBlockExit(Collider other) {
            otherLegoBlock = null;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "Floor") 
            {
                Collider[] cs = Physics.OverlapBox(transform.position, boxCollider.size, transform.rotation);
                foreach(Collider c in cs)  {
                    if(c.tag == "Lego" && c != boxCollider)
                        return;
                }

                AnchorLegoToFloor();
            }
        }

        private void OnJointBreak(float breakForce){
            isAnchored = false;
        }


        private Vector3 CreateCentroid(List<Transform> ts)
        {
            Vector3 centroid = new Vector3();
            foreach(Transform t in ts) {
                centroid.x += t.position.x;
                centroid.y += t.position.y;
                centroid.z += t.position.z;
            }

            centroid /= ts.Count;
            return centroid;
        }

        private ProngInfoList.ProngInfo ClosestProng(Vector3 position, Transform[] otherBottomProngs)
        {
            int index = 0;
            Transform closestTransform = otherBottomProngs[0];
            float closestProngDistance = Vector3.Distance(position, otherBottomProngs[0].position);

            for (int r = 1; r < otherBottomProngs.Length; r++) {
                float distance = Vector3.Distance(position, otherBottomProngs[r].position);
                if (distance < closestProngDistance) {
                    closestProngDistance = distance;
                    closestTransform = otherBottomProngs[r];
                    index = r;
                }
            }

            ProngInfoList.ProngInfo info = new ProngInfoList.ProngInfo();
            info.prongIndex = index;
            info.prongTransform = closestTransform;
            return info;
        }

        private void AnchorLego()
        {
            // make sure to stop the current interaction
            if (thisLegoInteractble.IsGrabbed())
                thisLegoInteractble.ForceStopInteracting();

            // orient the lego and put it into place; a create a empty game object a proper position
            // and move that into place and then destroy it
            OrientTransform(otherLegoBlock.transform);
            Vector3 otherProngsCentroid = CreateCentroid(prongInfoList.ProngTransforms(thisBlock : false));
            Vector3 myProngsCentroid = CreateCentroid(prongInfoList.ProngTransforms(thisBlock : true));
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
            FixedJoint fj = otherLegoBlock.gameObject.AddComponent<FixedJoint>();
            fj.breakForce = breakForce;
            otherLegoBlock.LegoInteractable.enabled = false;
            otherLegoBlock.IsAnchored = true;

            blockTriggerEvent.AddIgnoreCollider(otherLegoBlock.GetComponent<Collider>());
        }

        private void AnchorLegoToFloor()
        {
            if(thisLegoInteractble.IsGrabbed())
                thisLegoInteractble.ForceStopInteracting();

            OrientTransform(transform);
            transform.position = new Vector3(transform.position.x, floorHeight, transform.position.z);
            FixedJoint fixedJoint = gameObject.AddComponent<FixedJoint>();
            fixedJoint.breakForce = breakForce;
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