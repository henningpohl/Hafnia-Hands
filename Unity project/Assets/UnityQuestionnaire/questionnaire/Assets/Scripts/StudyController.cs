using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StudyController : MonoBehaviour {

    public UnityEvent StartSurvey = new UnityEvent();
    public UnityEvent SendToServer = new UnityEvent();

    public List<List<int>> LatinSquare = new List<List<int>>() {
        new List<int>() { 0, 1, 3, 4 },
        new List<int>() { 1, 2, 0, 3 },
        new List<int>() { 2, 3, 1, 0 },
        new List<int>() { 3, 0, 2, 1 }
    };

    public List<int> GetOrder()
    {
        return LatinSquare[0];
    }

    void Start() {
        //Forms.ShowForm(FormsManager.FormType.Consent);
        StartStudy();

        // TODO: call this each time you want the survey to appear
        StartSurvey.Invoke();

        // TODO: call this one when experiment is done, and you want to store data
        // check ServerHandler::CheckForServerResponse() if you a callback 
        //SendToServer.Invoke();
    }

    void Update() {

    }

    [ContextMenu("Start Study")]
    public async void StartStudy() {
        NextCondition();
    }

    [ContextMenu("Next Condition")]
    public async void NextCondition() {

    }
}
