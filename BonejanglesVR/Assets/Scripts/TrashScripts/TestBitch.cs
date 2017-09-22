using UnityEngine;
using SimpleJSON;

namespace com.EvolveVR.BonjanglesVR
{
    public class TestBitch : MonoBehaviour
    {
        public TextAsset skeletonInfo;
        JSONNode node;

        private void Awake()
        {
            node = JSON.Parse(skeletonInfo.text);
            Debug.Log(node["cranium"]["wikiURL"]);
            //node.AsObject.
        }

    }
}