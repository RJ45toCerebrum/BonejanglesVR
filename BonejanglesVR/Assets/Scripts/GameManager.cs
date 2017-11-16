using UnityEngine;
using UnityEngine.UI;

namespace com.EvolveVR.BonejanglesVR
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager gm;
		private bool gameOver = false;

        private int curNumBonesConnected;
        private JointNode[] joints;

        // delete later
        public GameObject gameOverMessage;
        public AudioSource endGameAudioQueuePlaceholder;

		// timing
		public float allowedSeconds = 1800.0f;
		public string winMessage = "Win";
		public string loseMessage = "Lose";

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
            joints = FindObjectsOfType<JointNode>();
        }

        private void Start() {
            gameOverMessage.SetActive(false);
        }

		private void Update()
		{
			if (gameOver)
				return;

			allowedSeconds -= Time.deltaTime;
			if (allowedSeconds <= 0)
				EndGame (loseMessage);
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