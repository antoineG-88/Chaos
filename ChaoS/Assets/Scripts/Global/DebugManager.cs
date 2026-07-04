using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class DebugManager : MonoBehaviour
{
    [Header("Debug Monitor")]
    public GameObject debugPanel;
    public GameObject frameContainer;
    public GameObject persisContainer;
    public TMP_Text monitorFrameContent;
    public TMP_Text monitorPersistentContent;
    [Header("Debug Cast")]
    public Color defaultCastTestColor;
    public Color defaultCastHitColor;


    //------------------------------------------------------------<--....______....-->

    private static List<string> frameLines;
    private static List<string> persistentLines;
    private static int currentFrameIndex;

    //------------------------------------------------------------<--....______....-->

    private static List<RaycastDebug> frameRaycastDebugs;
    private static List<RaycastDebug> persistentRaycastDebugs;

    //------------------------------------------------------------<--....______....-->

    static public DebugManager I;

    private void Awake()
    {
        I = this;
        currentFrameIndex = Time.frameCount;
        frameRaycastDebugs = new List<RaycastDebug>();
        persistentRaycastDebugs = new List<RaycastDebug>();
    }

    private void Start()
    {
        frameLines = new List<string>();
        persistentLines = new List<string>();
        I.persisContainer.SetActive(false);
    }

    private void Update()
    {
        UpdateMonitor();
    }

    static public void DebugRaycast(Vector3 origin, Vector3 direction, RaycastHit hitToDebug, float distance, Color testColor, Color hitColor)
    {
        RaycastDebug newRayDebug = new RaycastDebug();
        newRayDebug.start = origin;
        newRayDebug.end = origin + direction.normalized * distance;
        newRayDebug.hasHit = hitToDebug.collider != null;
        if (newRayDebug.hasHit)
            newRayDebug.hitPoint = hitToDebug.point;
        newRayDebug.testColor = testColor;
        newRayDebug.hitColor = hitColor;

        frameRaycastDebugs.Add(newRayDebug);
    }
    static public void DebugRaycast(Vector3 origin, Vector3 direction, RaycastHit hitToDebug, float distance)
    {
        RaycastDebug newRayDebug = new RaycastDebug();
        newRayDebug.start = origin;
        newRayDebug.end = origin + direction.normalized * distance;
        newRayDebug.hasHit = hitToDebug.collider != null;
        if (newRayDebug.hasHit)
            newRayDebug.hitPoint = hitToDebug.point;
        newRayDebug.testColor = I.defaultCastTestColor;
        newRayDebug.hitColor = I.defaultCastHitColor;

        frameRaycastDebugs.Add(newRayDebug);
    }

    static public void DebugBoxcast(Vector3 origin, Vector3 halfExtent,Vector3 direction, RaycastHit hitToDebug, Quaternion boxRotation, float distance, Color testColor, Color hitColor)
    {
        RaycastDebug newRayDebug = new RaycastDebug();
        newRayDebug.start = origin;
        newRayDebug.end = origin + direction.normalized * distance;
        newRayDebug.hasHit = hitToDebug.collider != null;
        if (newRayDebug.hasHit)
            newRayDebug.hitPoint = hitToDebug.point;
        newRayDebug.testColor = I.defaultCastTestColor;
        newRayDebug.hitColor = I.defaultCastHitColor;

        frameRaycastDebugs.Add(newRayDebug);
    }

    static public void DebugBoxcast(Vector3 origin, Vector3 halfExtent, Vector3 direction, RaycastHit hitToDebug, Quaternion boxRotation, float distance)
    {

    }

    static private void ClearFrameCastDebug()
    {
        frameRaycastDebugs.Clear();
    }

    #region Monitor

    private void UpdateMonitor()
    {
        if (currentFrameIndex != Time.frameCount)
        {
            ClearFrameLines();
            frameContainer.SetActive(false);
            currentFrameIndex = Time.frameCount;
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            if (!debugPanel.activeSelf)
            {
                OpenDebugPanel();
            }
            else
            {
                CloseDebugPanel();
            }
        }
    }

    static public void AddFrameLine(string lineText)
    {
        if (currentFrameIndex != Time.frameCount)
        {
            ClearFrameLines();
            currentFrameIndex = Time.frameCount;
        }

        I.frameContainer.SetActive(true);

        I.monitorFrameContent.SetText((frameLines.Count != 0 ? I.monitorFrameContent.text + "\n" : "") + lineText);
        frameLines.Add(lineText);
    }

    static public void AddPersistentLine(string lineText)
    {
        I.persisContainer.SetActive(true);

        I.monitorPersistentContent.SetText((persistentLines.Count != 0 ? I.monitorPersistentContent.text + "\n" : "") + lineText);
        persistentLines.Add(lineText);
    }
    static public void AddPersistentLineForSpecifiedTime(string lineText, float displayTime)
    {
        I.StartCoroutine(I.AddThenRemovePersisLine(lineText, displayTime));
    }

    static public void ClearPersistentLines()
    {
        I.monitorPersistentContent.text = "";
        persistentLines.Clear();
        I.persisContainer.SetActive(false);
    }

    private static void ClearFrameLines()
    {
        I.monitorFrameContent.text = "";
        frameLines.Clear();
    }

    private IEnumerator AddThenRemovePersisLine(string text, float time)
    {
        I.persisContainer.SetActive(true);

        I.monitorPersistentContent.SetText((persistentLines.Count != 0 ? I.monitorPersistentContent.text + "\n" : "") + text);
        persistentLines.Add(text);

        yield return new WaitForSecondsRealtime(time);
        RemovePersistentLine(text);
    }

    public void RemovePersistentLine(string textToRemove)
    {
        persistentLines.Remove(textToRemove);
        ClearPersistentLines();
        for (int i = 0; i < persistentLines.Count; i++)
        {
            I.monitorPersistentContent.SetText((i != 0 ? I.monitorPersistentContent.text + "\n" : "") + persistentLines[i]);
        }
    }

    public void ModifyPersistentLine(string textToReplace, string newText)
    {
        int index = persistentLines.IndexOf(textToReplace);
        persistentLines[index] = newText;
    }

    private void OpenDebugPanel()
    {
        debugPanel.SetActive(true);
    }

    private void CloseDebugPanel()
    {
        debugPanel.SetActive(false);
    }
    #endregion

    private void OnDrawGizmos()
    {
        if(Application.isPlaying)
        {
            foreach (RaycastDebug rayDebug in frameRaycastDebugs)
            {
                Gizmos.color = rayDebug.hasHit ? rayDebug.hitColor : rayDebug.testColor;
                Gizmos.DrawLine(rayDebug.start, rayDebug.end);
                if (rayDebug.hasHit)
                {
                    Gizmos.color = rayDebug.hitColor;
                    Gizmos.DrawSphere(rayDebug.hitPoint, 0.05f);
                }
            }


            foreach (RaycastDebug rayDebug in frameRaycastDebugs)
            {
                Gizmos.color = rayDebug.hasHit ? rayDebug.hitColor : rayDebug.testColor;
                Gizmos.DrawLine(rayDebug.start, rayDebug.end);
                if (rayDebug.hasHit)
                {
                    Gizmos.color = rayDebug.hitColor;
                    Gizmos.DrawSphere(rayDebug.hitPoint, 0.05f);
                }
            }

            ClearFrameCastDebug();
        }
    }

    private struct RaycastDebug
    {
        public Vector3 start;
        public Vector3 end;
        public bool hasHit;
        public Vector3 hitPoint;
        public Color testColor;
        public Color hitColor;
    }
    private struct BoxcastDebug
    {
        public Vector3 start;
        public Vector3 end;
        public bool hasHit;
        public Vector3 hitPoint;
        public Vector3 size;
        public Quaternion rot;
        public Color testColor;
        public Color hitColor;
    }
}
