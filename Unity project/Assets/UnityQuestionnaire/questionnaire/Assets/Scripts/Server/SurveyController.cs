using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SurveyController : MonoBehaviour
{
    public int COOL_DOWN_SECONDS = 3;
    public UnityEvent SurveyDone = new UnityEvent();

    private ServerHandler sh;

    void Start()
    {
        // get reference to serverhandler component
        this.sh = gameObject.GetComponent<ServerHandler>();
    }

    bool buttonDisabled(string key)
    {
        try
        {
            return !GameObject.Find("Button" + key).activeSelf;
        }
        catch(NullReferenceException e)
        {
            // pressed button that doesn't match 3d button
            return true;
        }
        
    }

    void Update()
    {
        if (Input.anyKeyDown)
        {
            String key = Input.inputString;

            // do not accept press from button if it's not present
            if (buttonDisabled(key)) return;

            switch (key)
            {
                case "1":
                    button1Pressed();
                    break;
                case "2":
                    button2Pressed();
                    break;
                case "3":
                    button3Pressed();
                    break;
                case "4":
                    button4Pressed();
                    break;
                case "5":
                    button5Pressed();
                    break;
                case "6":
                    button6Pressed();
                    break;
                case "7":
                    button7Pressed();
                    break;
                default:
                    break;

            }
        }
    }

    public void button1Pressed()
    {
        _buttonPress(1);
    }
    public void button2Pressed()
    {
        _buttonPress(2);
    }
    public void button3Pressed()
    {
        _buttonPress(3);
    }
    public void button4Pressed()
    {
        _buttonPress(4);
    }
    public void button5Pressed()
    {
        _buttonPress(5);
    }
    public void button6Pressed()
    {
        _buttonPress(6);
    }
    public void button7Pressed()
    {
        _buttonPress(7);
    }

    IEnumerator LiftCoolDown()
    {
        cool_down = true;
        sh.HideButtonGroup();
        yield return new WaitForSeconds(COOL_DOWN_SECONDS);
        cool_down = false;
        sh.ShowButtonGroup();
    }

    bool cool_down = false;
    private void _buttonPress(int button_id)
    {
        Debug.Log("PRESSED BUTTON: " + button_id);

        // don't allow filling out surveys, when form is not present
        // only really possible from debugging
        if(!sh.survey.activeSelf)
        {
            return;
        }

        if(cool_down)
        {
            return;
        }
        

        bool more = false;
        try
        {
            more = sh.AnswerQuestion(button_id);
        }
        catch (Exception e)
        {
            more = false;
        }

        if (!more)
        {
            sh.Reset();
            sh.CloseSurvey();
            SurveyDone.Invoke();
        }
        else
        {
            try
            {
                sh.UpdateCanvas();
            }
            catch (RoundDone e)
            {
                sh.Reset();
                sh.CloseSurvey();
                SurveyDone.Invoke();
            }
        }

        StartCoroutine(LiftCoolDown());
    }
}


