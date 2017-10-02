using UnityEngine;
using UnityEngine.UI;
using com.EvolveVR.BonejanglesVR;

public class TestCanvas : MonoBehaviour
{

    private int lastNumConnCount = 0;
    public Text gameText;

    private void Update()
    {
        if (GameManager.Instance.CurNumBonesConnected == lastNumConnCount)
            return;

        lastNumConnCount = GameManager.Instance.CurNumBonesConnected;
        gameText.text = lastNumConnCount.ToString();

        if (lastNumConnCount == GameManager.totalNumConnections) {
            gameText.text = "\n\nWinner\n\n";
        }
    }
}
