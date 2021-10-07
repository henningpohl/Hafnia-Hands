using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FormsManager : MonoBehaviour {
    public GameObject ConsentForm;
    public GameObject SurveyForm;
    public GameObject NoInternetSign;

    public enum FormType
    {
        Consent,
        Survey,
        NoInternet
    }

    void Start() {
        ConsentForm.SetActive(false);

        // check internet
        StartCoroutine(CheckInternet());
    }

    
    void Update() {

    }

    public void ShowForm(FormType form, bool active)
    {
        switch (form)
        {
            case FormType.Consent:
                ConsentForm.SetActive(active);
                break;
            case FormType.Survey:
                SurveyForm.SetActive(active);
                break;
            case FormType.NoInternet:
                NoInternetSign.SetActive(active);
                break;
        }
    }

    void _checkInternet()
    {
        StartCoroutine(checkInternetConnection((isConnected) => {
            has_internet = isConnected;

            // handle connection status here
            if (!has_internet)
            {
                // make internet message visible
                ShowForm(FormType.NoInternet, true);
            }
            else
            {
                ShowForm(FormType.NoInternet, false);
            }
        }));
    }

    IEnumerator checkInternetConnection(Action<bool> action)
    {
        WWW www = new WWW("http://google.com");
        yield return www;
        if (www.error != null)
        {
            action(false);
        }
        else
        {
            action(true);
        }
    }

    bool has_internet = false;
    IEnumerator CheckInternet()
    {
        // check for internet every three seconds
        // stop checking, once connection is established
        while (!has_internet)
        {
            _checkInternet();
            yield return new WaitForSeconds(3.0f);
        }

    }
}
