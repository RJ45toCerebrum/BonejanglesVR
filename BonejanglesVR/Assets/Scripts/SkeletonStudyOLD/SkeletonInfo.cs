using UnityEngine;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using SimpleJSON;

namespace com.EvolveVR.BonejanglesVR
{
	/// <summary>
	/// Skeleton info. This class is used for obtaining skeleton information
	/// and bone data in a global fashion.
	/// Thus, there should only be one instance of this in the scene at one time
	/// </summary>
	public class SkeletonInfo : MonoBehaviour
	{
        private static SkeletonInfo skeleton;

        public enum Side { None, Left, Right };

		private static TextAsset textBoneInfo;
		private static JSONNode jsonBoneData;
        
        // Dictionary used for obtaining any information on the Skeleton
        private static Dictionary<string, LinkedList<BoneInfo>> boneInfoDictionary = new Dictionary<string, LinkedList<BoneInfo>>();
		private static int numberOfBonesInScene = 0;
		private static int numberOfBonesReported = 0;
		private static bool isInitialized = false;

        // selected bone means the user selected the bone by pressing down of the botton part of the touchpad
        private static Bone selectedBone;
        // the currentActiveBone is the bone that the user is currently manipulating
        private static Bone currentActiveBone;
        public delegate void SelectedBoneChange(Bone bone);
        private static event SelectedBoneChange onBoneSelected;
        private static int numSelectedBoneEventSubs = 0;

        #region Unity Methods
        private void Awake()
		{
            if (skeleton != null)
                Destroy(gameObject);

            skeleton = this;
			textBoneInfo = Resources.Load<TextAsset> ("BoneInfoText/BoneInfo");
			jsonBoneData = InitBoneData (textBoneInfo.text);
            boneInfoDictionary = new Dictionary<string, LinkedList<BoneInfo>>();
		}

		private void Start() {
			numberOfBonesInScene = FindObjectsOfType<Bone> ().Length;
		}
		#endregion


		public static int NumberOfBonesInScene {
			get {
				return numberOfBonesInScene;
			}
		}

        public static int NumberOfBonesRegistered
        {
            get
            {
                return boneInfoDictionary.Count;
            }
        }

        public static bool IsInitialized {
			get {
				return isInitialized;
			}
		}

        public static Bone CurrentActiveBone
        {
            get { return currentActiveBone; }
            set { currentActiveBone = value; }
        }

        public static Bone SelectedBone
        {
            get { return selectedBone; }
            set
            {
                selectedBone = value;
                if (numSelectedBoneEventSubs > 0)
                    onBoneSelected(value);

                if (value != null)
                    value.SetBoneHighlight(true);
            }
        }

        #region METHODS
        private JSONNode InitBoneData(string jsonStringBoneData)
		{
			JSONNode boneData = JSON.Parse (jsonStringBoneData);
			return boneData;
		}

        /// <summary>
        /// data for the bone given its name
        /// </summary>
        /// <param name="boneName">name of the gameobject with a Bone script attached</param>
        /// <returns>information on the bone</returns>
		public static BoneInfo GetBoneData(string boneName) 
		{ 
            JSONArray articulationsJSONArr = jsonBoneData[boneName]["articulations"].AsArray;
            JSONArray functionsJSONArr = jsonBoneData[boneName]["function"].AsArray;
            string[] articulations = GetArrayFromJSON(articulationsJSONArr);
            string[] functions = GetArrayFromJSON(functionsJSONArr);

            BoneInfo bone = new BoneInfo (jsonBoneData[boneName] ["name"], 
                                          jsonBoneData [boneName] ["description"], 
                                          jsonBoneData [boneName] ["wikiURL"],
                                          articulations,
                                          functions);
			return bone;
		}

        private static string[] GetArrayFromJSON(JSONArray jsonArr)
        {
            string[] arr = new string[jsonArr.Count];
            for(int i = 0; i < arr.Length; i++)
                arr[i] = jsonArr[i].Value;
            return arr;
        }

        public static void TryAddBone(BoneInfo bone)
        {
            LinkedList<BoneInfo> boneList;
			if (!boneInfoDictionary.ContainsKey (bone.BoneName)) {
                boneList = new LinkedList<BoneInfo>();
                boneList.AddFirst(bone);
                boneInfoDictionary.Add(bone.BoneName, boneList);
            }
            else 
            {
                boneList = boneInfoDictionary[bone.BoneName];
                if (!boneList.Contains(bone))
                    boneList.AddLast(bone);
            }
            numberOfBonesReported++;
            Debug.Log (numberOfBonesReported);
			//if (numberOfBonesReported == numberOfBonesInScene)
			//	isInitialized = true;
        }
	    
        public static BoneInfo GetBoneInfo(string name, Side side)
        {
            if (boneInfoDictionary.ContainsKey(name)) 
            {
                var boneList = boneInfoDictionary[name];
                foreach(BoneInfo b in boneList) {
                    if (b.Side == side)
                        return b;
                }
            }
            return null;
        }

        public static void SubSelectedBoneEvent(SelectedBoneChange callback)
        {
            if (callback != null) {
                onBoneSelected += callback;
                numSelectedBoneEventSubs++;
            }
        }
		#endregion
    }
		
    /// <summary>
    /// Holds all the information for a Bone
    /// </summary>
	public class BoneInfo
	{
		private string boneName;
		private string description;
        private string wikiURL;
        private string[] articulations;
        private string[] functions;
        private SkeletonInfo.Side side;
        private GameObject boneGO;

		public BoneInfo(string _boneName, string _description, 
                        string _wikiURL, string[] _articulations,
                        string[] _functions)
		{
			boneName = _boneName;
			description = _description;
			wikiURL = _wikiURL;
            articulations = _articulations;
            functions = _functions;
        }

        public string BoneName
        {
            get { return boneName; }
        }

        public string Description
        {
            get{ return description;}
        }

        public string WikiURL
        {
            get { return wikiURL;}
        }

        public string[] Articulations
        {
            get { return articulations; }
        }

        public string[] Functions
        {
            get { return functions; }
        }

        public GameObject BoneGameObject
        {
            get{ return boneGO; }
            set{ boneGO = value; }
        }

        public SkeletonInfo.Side Side
        {
            get { return side; }
            set{ side = value; }
        }
    }
}
