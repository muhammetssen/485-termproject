using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Logger : MonoBehaviour
{
 
    string myLog;
    Queue<string> myLogQueue = new Queue<string>();

    void Start()
    {
        Debug.Log("Integrating interceptor for logs");
    }

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {

        string newString = "\n [" + type + "] : " + logString;
        myLogQueue.Enqueue(newString);
        if (myLogQueue.Count > 10)
        {
            myLogQueue.Dequeue();

        }

        myLog = "";
        while (myLogQueue.Count > 0)
        {
            myLog += myLogQueue.Dequeue();
        }
    }

    void OnGUI()
    {
        GUILayout.Label(myLog);
    }
}
