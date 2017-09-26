using UnityEngine;
using UnityEngine.UI;
using VRTK;
using UnityEditor;

/// <summary>
/// In order to log things to an in world console
/// </summary>
public class VRDebug : MonoBehaviour
{
    public int numberOfEntries;

    private static VRDebug vrDebug;
    private static LogEntry[] logEntries;
    private static Text consoleText;
    private static Image backgroundPanel;

    public Color normalColor;
    public Color highlightColor;

    public static VRDebug Instance
    {
        get { return vrDebug; }
    }

    private void Awake()
    {
        if (vrDebug != null)
            Destroy(gameObject);

        vrDebug = this;
        consoleText = GetComponentInChildren<Text>();
        backgroundPanel = GetComponentInChildren<Image>();

        VRTK_DestinationPoint point = GetComponent<VRTK_DestinationPoint>();
        point.DestinationMarkerEnter += OnPointerEnter;
        point.DestinationMarkerExit += OnPointerExit;

        InitLogEntries();
    }

    private void InitLogEntries()
    {
        GameObject logEntryPrefab = Resources.Load<GameObject>("VRDebug/LogEntry");
        float logEntryHeight = logEntryPrefab.GetComponent<RectTransform>().sizeDelta.y;
        RectTransform contentRT = (RectTransform)transform.Find("VRConsoleCanvas/ScrollView/Viewport/Content");
        if (contentRT == null)
            Debug.LogError("There must be a Gameobject named Content under the ViewPort of the ScrollView");

        contentRT.sizeDelta = new Vector2(contentRT.sizeDelta.x, logEntryHeight * numberOfEntries + numberOfEntries);
        
        logEntries = new LogEntry[numberOfEntries];
        // now insert the entries
        for (int i = 0; i < numberOfEntries; i++) {
            GameObject logEntry = Instantiate(logEntryPrefab);
            logEntries[i] = logEntry.GetComponent<LogEntry>();
            logEntries[i].LogNumber = i.ToString();
            logEntries[i].LogContent = "";
            logEntry.transform.SetParent(contentRT, false);
        }
    }

    private void OnPointerExit(object sender, DestinationMarkerEventArgs e) {
        backgroundPanel.color = normalColor;
    }

    private void OnPointerEnter(object sender, DestinationMarkerEventArgs e) {
        backgroundPanel.color = highlightColor;
    }

    public static void Log(string logMessage, int logNumber) {
        logEntries[logNumber].LogContent = logMessage;
    }

    public static void ClearLogs() {
        foreach(LogEntry le in logEntries) {
            le.LogContent = "";
        }
    }
}
