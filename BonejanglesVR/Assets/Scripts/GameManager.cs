using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.EvolveVR.BonejanglesVR
{
	[System.Serializable]
	public class Objective
	{
		public enum ObjectiveStatus
		{
			NotAvailable, Available, Finished
		}

		public string objectiveMessage;
		public string[] jointNames;
		private List<JointNode> jointNodes;
		private int numAdded;
		[Tooltip("The time is in Minutes")]
		public float allowedTime;
		private ObjectiveStatus objectiveCompleted = ObjectiveStatus.NotAvailable;
		public ObjectiveUI objectiveUI;



		public bool HasJoint(string jointName)
		{
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

		public bool ObjectiveCompleted()
		{
			if (objectiveCompleted == ObjectiveStatus.Finished)
				return true;

			foreach (JointNode jn in jointNodes) {
				if (jn.Connection != JointNode.ConnectionType.Correct)
					return false;
			}

			return true;
		}

		public void SetMessage(){
			objectiveUI.ChangeMessage (objectiveMessage);
		}

		public void SetObjectiveStatus(ObjectiveStatus status) {
			objectiveCompleted = status;
			objectiveUI.ChangeStatus (status);
		}

		public void AddJoint(JointNode jn)
		{
			if (jointNodes == null)
				jointNodes = new List<JointNode> (jointNames.Length);

			jointNodes.Add (jn);
		}
	}

    public class GameManager : MonoBehaviour
    {
        private static GameManager gm;
		private bool gameOver = false;
		// this is so we can create 
		public Objective[] objectiveList;
		private int currentObjective = 0;
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
			InitObjectives();
        }

		private void InitObjectives()
		{
			// the point of this is to map joint name --> objective that the joint is apart of
			joints = FindObjectsOfType<JointNode>();
			for(int i = 0; i < joints.Length; i++)
			{
				JointNode jn = joints [i];
				jn.OnBoneSnapped += BoneSnapped;
				// find the objective associated with this joint; linear but not that many bones or objectives so its fine
				Objective objective = null;
				for(int j = 0; j < objectiveList.Length; j++){
					if (objectiveList [j].HasJoint (jn.name)) {
						objective = objectiveList [j];
						objective.AddJoint (jn);
					}
				}
			}

			// init the objectives now
			foreach (Objective obj in objectiveList) {
				obj.SetMessage ();
				obj.SetObjectiveStatus (Objective.ObjectiveStatus.NotAvailable);
			}
			objectiveList [currentObjective].SetObjectiveStatus (Objective.ObjectiveStatus.Available);
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
			if (objectiveList[currentObjective].HasJoint(jn.name) && 
				objectiveList[currentObjective].ObjectiveCompleted ())
			{
				if (AllObjectivesCompleted ()) {
					Debug.LogError ("Game Won");
				} else {
					// play sound, make new objective available
					objectiveList [currentObjective].SetObjectiveStatus(Objective.ObjectiveStatus.Finished);
					currentObjective++;
					objectiveList [currentObjective].SetObjectiveStatus (Objective.ObjectiveStatus.Available);
				}
			}
		}

		private bool AllObjectivesCompleted()
		{
			foreach (Objective obj in objectiveList) {
				if(!obj.ObjectiveCompleted())
					return false;
			}

			return true;
		}

        private bool AllBonesCorrectlyConnected()
        {
            foreach(JointNode joint in joints) {
                if (joint.Connection != JointNode.ConnectionType.Correct)
                    return false;
            }
            return true;   
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