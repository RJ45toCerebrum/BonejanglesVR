using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This is to see the Logs in VRDebug console in the world
/// </summary>
public class LogEntry : MonoBehaviour
{
    Text logNumberText;
    Text logContentText;

    public string LogNumber
    {
        get{return logNumberText.text;}
        set{logNumberText.text = value;}
    }

    public string LogContent
    {
        get{ return logContentText.text;}
        set{logContentText.text = value.ToString();}
    }

    void Awake () {
        foreach(Transform child in transform) {
            if (child.name == "LogNumber")
                logNumberText = child.GetComponent<Text>();
            else
                logContentText = child.GetComponent<Text>();
        }
	}
}
