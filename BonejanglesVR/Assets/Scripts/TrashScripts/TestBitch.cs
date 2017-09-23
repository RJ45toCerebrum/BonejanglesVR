using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
using VRTK;

namespace com.EvolveVR.BonjanglesVR
{
    public class TestBitch : MonoBehaviour
    {
        public TextAsset skeletonInfo;
        JSONNode node;
        public Text t;

        private void Awake()
        {
            //node = JSON.Parse(skeletonInfo.text);
            //Debug.Log(node["cranium"]["wikiURL"]);
            //node.AsObject.
        }

        public void Shit(object o, DestinationMarkerEventArgs args)
        {
            string n = args.target.name;
            float d = args.distance;
            t.text = string.Format("name: {0}, distance: {1}", n, d);
        }
    }
}