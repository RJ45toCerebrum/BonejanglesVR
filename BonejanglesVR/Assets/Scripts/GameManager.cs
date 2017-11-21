using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.EvolveVR.BonejanglesVR
{
	[System.Serializable]
	public class Objective
	{
		public string objectiveMessage;
		public string[] jointNames;
		[Tooltip("The time is in Minutes")]
		public float allowedTime;

		public bool HasJoint(string jointName){
			foreach (string jName in jointNames) {
				if (jName == jointName)
					return true;
			}

			return false;
		}

		// the amount of time should return number of seconds
		public float GetTime() {
			return allowedTime * 60f;
		}
	}

    public class GameManager : MonoBehaviour
    {
        private static GameManager gm;
		private bool gameOver = false;
		public Objective[] objectiveList;
		private int currentObjective = 0;
		// this maps a joint name to a objective; this allows me to 
		// find whcih objective a JointNode is a part of and thus see if
		// all the other Node in the set are also connected
		private Dictionary<string, Objective> jointToObjective;

        private int curNumBonesConnected;
        private JointNode[] joints;

        // delete later
        public GameObject gameOverMessage;
        public AudioSource endGameAudioQueuePlaceholder;

		// timing
		public float allowedSeconds = 1800.0f;
		public string winMessage = "Win";
		public string loseMessage = "Lose";
		public string playerTip;


        public static GameManager Instance
        {
            get { return gm; }
        }

        public int CurNumBonesConnected
        {
            get { return curNumBonesConnected; }
        }

		public float AllowedSeconds 
		{
			get { return allowedSeconds; }
		}

		public bool GameOver 
		{
			get { return gameOver; }
		}


        private void Awake()
        {
            if (gm != null)
                Destroy(this);
            gm = this;
        }

        private void Start() {
            gameOverMessage.SetActive(false);

			// the point of this is to map joint name --> objective that the joint is apart of
			InitObjectiveMap();
        }

		private void InitObjectiveMap()
		{
			// the point of this is to map joint name --> objective that the joint is apart of
			joints = FindObjectsOfType<JointNode>();
			jointToObjective = new Dictionary<string, Objective> (joints.Length);
			for(int i = 0; i < joints.Length; i++)
			{
				JointNode jn = joints [i];
				jn.OnBoneSnapped += BoneSnapped;
				// find the objective associated with this joint; linear but not that many bones or objectives so its fine
				Objective objective = null;
				for(int j = 0; j < objectiveList.Length; j++){
					if (objectiveList [j].HasJoint (jn.name))
						objective = objectiveList [j];
				}

				if (objective != null)
					jointToObjective.Add (jn.name, objective);
				else
					Debug.LogWarningFormat ("Joint {0} has no objective associated", jn.name);
			}
		}

		private void Update()
		{
			if (gameOver)
				return;

			allowedSeconds -= Time.deltaTime;
			if (allowedSeconds <= 0)
				EndGame (loseMessage);
		}


		private void BoneSnapped(JointNode jn)
		{
			Debug.Log (jn.name);
			Objective obj = jointToObjective [jn.name];
			Debug.Log (obj.objectiveMessage);
		}

        public void AddConnection()
        {
			if (allowedSeconds <= 0)
				return;

            curNumBonesConnected++;
            if(AllBonesCorrectlyConnected())
                EndGame("Win Message");
        }

        private bool AllBonesCorrectlyConnected()
        {
            foreach(JointNode joint in joints) {
                if (joint.Connection != JointNode.ConnectionType.Correct)
                    return false;
            }
            return true;   
        }

        public void RemoveConnection()
        {
            curNumBonesConnected--;
        }

		private void EndGame(string message)
        {
            gameOverMessage.SetActive(true);
			Text t = gameOverMessage.GetComponentInChildren<Text> ();
			t.text = message;

            endGameAudioQueuePlaceholder.Play();
			gameOver = true;
        }
    }
}