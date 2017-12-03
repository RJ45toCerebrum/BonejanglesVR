using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRTK;

namespace com.EvolveVR.BonejanglesVR
{
	public class Node<T>
	{
		public T data;
		public int depth = 0;
		public List<Node<T>> children;

		public Node() {
			children = new List<Node<T>> ();
		}
	}

	[System.Serializable]
	public class Objective
	{
		public enum ObjectiveStatus
		{
			NotAvailable, Available, Finished
		}

		public string objectiveName;
		public string[] connectedObjectiveNames;

		public string objectiveMessage;
		public string[] jointNames;
		private List<JointNode> jointNodes;
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
		private JointNode[] joints;

		public Objective[] objectiveList;
		private Queue<Node<Objective>> nextObjectives;
		private List<Objective> activeObjectives;

        // delete later
        public GameObject gameOverMessage;
        public AudioSource audioSource;
		public AudioClip winAudio;
		public AudioClip loseAudio;

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
			nextObjectives = new Queue<Node<Objective>> ();
			activeObjectives = new List<Objective> ();
			Dictionary<string, Node<Objective>> graphBuilder = new Dictionary<string, Node<Objective>> ();

			// the point of this is to map joint name --> objective that the joint is apart of
			joints = FindObjectsOfType<JointNode> ();
			for (int i = 0; i < joints.Length; i++) 
			{
				JointNode jn = joints [i];
				jn.OnBoneSnapped += BoneSnapped;

				// FIND the objective associated with this joint; O(n) but not that many bones or objectives so its fine
				Objective objective = null;
				for (int j = 0; j < objectiveList.Length; j++) 
				{
					if (objectiveList [j].HasJoint (jn.name)) 
					{
						objective = objectiveList [j];
						objective.AddJoint (jn);

						if (!graphBuilder.ContainsKey (objective.objectiveName)) 
						{
							Node<Objective> objectiveNode = new Node<Objective> ();
							objectiveNode.data = objective;
							graphBuilder.Add (objective.objectiveName, objectiveNode);
						}
						// joint is never associated with multiple objectives
						break;
					}
				}
			}

			// initiallize objective; currently there are no connections between objective nodes; 
			// make them now that all objective nodes are mapped;
			foreach (Objective obj in objectiveList) 
			{
				obj.SetMessage ();
				obj.SetObjectiveStatus (Objective.ObjectiveStatus.NotAvailable);
				Node<Objective> objectiveNode = graphBuilder [obj.objectiveName];
				foreach (string childObj in obj.connectedObjectiveNames)
					objectiveNode.children.Add (graphBuilder [childObj]);
			}
				
			// this is for algorithm that uses Q and List in order to keep track of which objectives
			// are currently active and which ones are next in the queue

			graphBuilder ["obj1"].depth = 0;
			nextObjectives.Enqueue (graphBuilder ["obj1"]);
			NextObjectives ();
		}

		private void Update()
		{
			if (gameOver)
				return;

			allowedSeconds -= Time.deltaTime;
			if (allowedSeconds <= 0)
				EndGame (false, loseMessage);
		}

		private void NextObjectives()
		{
			if (nextObjectives.Count == 0)
				return;

			activeObjectives.Clear ();
			// algo --> while nodes in the queue have the same depth as front one;
			// 1) pop 2) add its children 3) add to active objective list
			Node<Objective> objectiveNode;
			int currentDepth = nextObjectives.Peek ().depth;

			while (nextObjectives.Count != 0 && currentDepth == nextObjectives.Peek ().depth) 
			{
				objectiveNode = nextObjectives.Dequeue ();
				activeObjectives.Add (objectiveNode.data);
				allowedSeconds = objectiveNode.data.GetTime();
				objectiveNode.data.SetObjectiveStatus (Objective.ObjectiveStatus.Available);
				foreach (var node in objectiveNode.children) {
					node.depth = currentDepth + 1;
					nextObjectives.Enqueue (node);
				}
			}
		}

		private void BoneSnapped(JointNode jn) 
		{
			// if the player completed an objective then Play sound, set objective state
			bool allObjectviesComplete = true;
			foreach (Objective obj in activeObjectives) 
			{
				if (obj.ObjectiveCompleted ())
					obj.SetObjectiveStatus (Objective.ObjectiveStatus.Finished);
				else
					allObjectviesComplete = false;
			}

			if (allObjectviesComplete)
				NextObjectives ();

			// game over check
			if (AllObjectivesCompleted()){
				EndGame (true, winMessage);
			}
		}

		private bool AllObjectivesCompleted(List<Objective> objectives)
		{
			foreach (Objective obj in objectives) {
				if(!obj.ObjectiveCompleted())
					return false;
			}

			return true;
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

		private void EndGame(bool won, string message)
        {
            gameOverMessage.SetActive(true);
			Text t = gameOverMessage.GetComponentInChildren<Text> ();
			t.text = message;

			if (won)
				audioSource.clip = winAudio;
			else
				audioSource.clip = loseAudio;
			
            audioSource.Play();
			gameOver = true;

			//FindObjectOfType<VRTK_Head>
        }
    }
}