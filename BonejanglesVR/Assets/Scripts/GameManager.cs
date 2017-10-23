using UnityEngine;
using UnityEngine.UI;

namespace com.EvolveVR.BonejanglesVR
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager gm;

        private int curNumBonesConnected;
        private JointNode[] joints;

        // delete later
        public GameObject gameOverMessage;
        public AudioSource endGameAudioQueuePlaceholder;

        public static GameManager Instance
        {
            get { return gm; }
        }

        public int CurNumBonesConnected
        {
            get { return curNumBonesConnected; }
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

        public void AddConnection()
        {
            curNumBonesConnected++;
            if(AllBonesCorrectlyConnected())
                EndGame();
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

        private void EndGame()
        {
            gameOverMessage.SetActive(true);
            endGameAudioQueuePlaceholder.Play();
        }
    }
}