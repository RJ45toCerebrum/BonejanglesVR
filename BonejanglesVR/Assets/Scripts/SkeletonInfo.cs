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
        public enum Side { None, Left, Right };

		private static TextAsset textBoneInfo;
		private static JSONNode jsonBoneData;
        
        // Dictionary used for obtaining any information on the Skeleton
        private static Dictionary<string, LinkedList<BoneInfo>> boneInfoDictionary = new Dictionary<string, LinkedList<BoneInfo>>();
		private static int numberOfBonesInScene = 0;
		private static int numberOfBonesReported = 0;
		private static bool isInitialized = false;

		#region Unity Methods
		private void Awake()
		{
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

		public bool IsInitialized {
			get {
				return isInitialized;
			}
		}

		public static int NumberOfBonesRegistered {
			get {
				return boneInfoDictionary.Count;
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
            string[] articulations = new string[articulationsJSONArr.Count];
            for(int i = 0; i < articulations.Length; i++)
                articulations[i] = articulationsJSONArr[i].Value;

            BoneInfo bone = new BoneInfo (jsonBoneData[boneName] ["name"], 
                                          jsonBoneData [boneName] ["description"], 
                                          jsonBoneData [boneName] ["wikiURL"],
                                          articulations);
			return bone;
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

		public LinkedList<BoneInfo> this[string key]
		{
			get 
			{
                LinkedList<BoneInfo> boneList;
				if (boneInfoDictionary.TryGetValue (key, out boneList))
					return boneList;
				return null;
			}
		}
	    
        public BoneInfo GetBoneInfo(string name, Side side)
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
		#endregion
    }
		
    /// <summary>
    /// Holds all the information for a Bone
    /// </summary>
	public class BoneInfo
	{
		private string boneName;
		private string functionDescription;
        private string wikiURL;
        private string[] articulations;
        private SkeletonInfo.Side side;
        private GameObject boneGO;

		public BoneInfo(string _boneName, string _functionDescription, string _wikiURL, string[] _articulations)
		{
			boneName = _boneName;
			functionDescription = _functionDescription;
			wikiURL = _wikiURL;
            articulations = _articulations;
        }

        public string BoneName
        {
            get { return boneName; }
        }

        public string FunctionDescription
        {
            get{ return functionDescription;}
        }

        public string WikiURL
        {
            get { return wikiURL;}
        }

        public string[] Articulations
        {
            get { return articulations; }
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
