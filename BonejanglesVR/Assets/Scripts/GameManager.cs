using UnityEngine;

namespace com.EvolveVR.BonejanglesVR
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager gm;

        private int curNumBonesConnected;
        public const int totalNumConnections = 25;

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
        }

        public void AddConnection()
        {
            curNumBonesConnected++;
            if(curNumBonesConnected == totalNumConnections) {
                Debug.LogError("Implement end game code");
            }
        }

        public void RemoveConnection()
        {
            curNumBonesConnected--;
        }
    }
}