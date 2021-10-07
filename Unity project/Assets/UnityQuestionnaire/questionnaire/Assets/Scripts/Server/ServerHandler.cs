using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NoMoreQuestions : Exception
{
    //
}

public class RoundDone : Exception
{
    //
}

public class ServerHandler : MonoBehaviour
{
    public TextAsset jsonFile;

    // references to UI
    public GameObject survey;
    private GameObject title;
    private GameObject help;
    private GameObject button1;
    private GameObject button2;
    private GameObject button3;
    private GameObject button4;
    private GameObject button5;
    private GameObject button6;
    private GameObject button7;

    public ServerResponse sr;

    private int round = 0;
    
    // make sure this one is false for deployment
    private bool TESTING = Application.isEditor;

    private Questionnaires qs;
    public string API_URL = "https://...";

    // stores data
    private Log log = new Log();

    // internal counters
    int current_questionnaire_count = 0;
    int current_question_count = 0;

    public int skin_tone = -1;

    void Start()
    {
        Debug.Log("ServerHandler::StartSurvey()");

        this.qs = JsonUtility.FromJson<Questionnaires>(jsonFile.text);

        assignReferences();

        LogQuestions();
    }

    private void assignReferences()
    {
        this.button1 = GameObject.Find("Button1");
        this.button2 = GameObject.Find("Button2");
        this.button3 = GameObject.Find("Button3");
        this.button4 = GameObject.Find("Button4");
        this.button5 = GameObject.Find("Button5");
        this.button6 = GameObject.Find("Button6");
        this.button7 = GameObject.Find("Button7");

        this.title = GameObject.Find("QuestionnaireHeader");
        this.help = GameObject.Find("QuestionnaireHelpText");
    }

    public void SetOrder(string order)
    {
        this.log.SetOrder(order);
    }

    public void Reset()
    {
        this.current_questionnaire_count = 0;
        this.current_question_count = 0;
        this.HideButtons();
        this.SetTitle("");
        this.SetHelp("");
    }

    public void CloseSurvey()
    {
        this.survey.SetActive(false);
        Debug.Log("Closed questionnaire. Logged data:");
        Debug.Log(this.toJSON());
    }

    public async void OpenSurvey()
    {
        // busy waiting until object has been spawned properly
        while(this.button1 == null)
        {
            assignReferences();
            await System.Threading.Tasks.Task.Delay(10); // 10 ms
        }

        Debug.Log("Opened questionnaire.");
        this.survey.SetActive(true);
        Reset();
        UpdateCanvas();    
    }

    

    private Questionnaire GetCurrentQuestionnaire()
    {
        Questionnaire q = this.qs.questionnaires[current_questionnaire_count];

        // skip questionnaires that do not match the exclusion criteria
        // in the json file
        while (Array.IndexOf(q.exclude_rounds, round) > -1)
        {
            this.current_questionnaire_count++;
            q = this.qs.questionnaires[current_questionnaire_count];
        }

        Debug.Log("ServerHandler::GetCurrentQuestionnaire()");
        Debug.Log("Round: " + round);
        Debug.Log("Q: " + q.name);
        Debug.Log("Exclude_rounds: " + String.Join(", ", q.exclude_rounds));

        return this.qs.questionnaires[current_questionnaire_count];
    }

    private Question GetCurrentQuestion()
    {
        return GetCurrentQuestionnaire().questions[current_question_count];
    }

    public void UpdateCanvas()
    {
        //Debug.Log("UpdateCanvas()");

        Questionnaire questionnaire;
        Question question;        

        try { 
            questionnaire = GetCurrentQuestionnaire();
            question = GetCurrentQuestion();
        }
        catch (Exception e) {
            // no more questions
            Debug.Log("No more questions");
            round++;
            throw new RoundDone();
        }

        Debug.Log("Questionnaire: '" + questionnaire.name + "', Q" + this.current_question_count);
        
        SetTitle(question.question);
        SetHelp(question.help);
        string[] button_titles =
        {
            question.b1, question.b2, question.b3, question.b4, question.b5, question.b6, question.b7
        };

        setButtonTitles(
            button_titles
        );

        // color buttons
        if(questionnaire.name == "fitzpatrick")
        {
            setFitzpatrickButtonColors();
        }
        else
        {
            resetButtonColors();
        }
    }

    private void resetButtonColors()
    {
        GameObject[] buttons =
        {
            button1, button2, button3, button4, button5, button6, button7
        };
        for (var i = 0; i < buttons.Length; i++)
        {
            buttons[i].GetComponent<Image>().color = Color.white;
            buttons[i].transform.GetChild(0).GetComponent<Text>().color = Color.black;
        }
    }

    private void setFitzpatrickButtonColors()
    {
        // https://www.elkaclinic.com.au/the-fitzpatrick-skin-type-chart/
        Color[] skintones =
        {
             new Color(244f/255f, 208f/255f, 176f/255f),
             new Color(232f/255f, 180f/255f, 143f/255f),
             new Color(211f/255f, 158f/255f, 124f/255f),
             new Color(187f/255f, 119f/255f, 80f/255f),
             new Color(165f/255f, 93f/255f, 43f/255f),
             new Color(60f/255f, 32f/255f, 29f/255f)
        };
        GameObject[] buttons =
        {
            button1, button2, button3, button4, button5, button6
        };
        for (var i = 0; i < buttons.Length; i++)
        {
            int offset = 0;
            if (i == 2) offset--;
            if (i == 3) offset++;

            buttons[i].GetComponent<Image>().color = skintones[i];
            buttons[i].transform.GetChild(0).GetComponent<Text>().color = skintones[buttons.Length - 1 - i - offset];
        }
    }

    public void setButtonTitles(string[] values)
    {
        // first make all invis
        HideButtons();

        int hidden_buttons = 0;

        GameObject[] buttons =
        {
            button1, button2, button3, button4, button5, button6, button7
        };

        for(var i = 0; i < values.Length; i++)
        {
            string val = values[i];
            if (val == "")
            {
                // never mind this button
                hidden_buttons++;
                continue;
            }

            buttons[i].SetActive(true);

            try
            {
                buttons[i].GetComponent<Text>().text = val;
            }
            catch (Exception e)
            {
                // if this errors, the text elem is probably on a child obj
                foreach (Transform child in buttons[i].transform)
                {
                    Text c = child.GetComponent<Text>();
                    if (c != null)
                    {
                        // found
                        c.text = val;
                        break;
                    }
                }
            }
        }

        // now offset everything a little bit to align buttons; because the hidden buttons (only in the end)
        GameObject p = button1.transform.parent.gameObject;
        RectTransform rt = p.GetComponent<RectTransform>();
        float button_width = button1.GetComponent<RectTransform>().rect.width;
        float button_space = Math.Abs(button1.GetComponent<RectTransform>().anchoredPosition.x - button2.GetComponent<RectTransform>().anchoredPosition.x) - button_width;
        rt.anchoredPosition = new Vector2(0.5f * hidden_buttons * (button_width + button_space), 0);
    }

    public void SetTitle(string str_title)
    {
        Text title0 = title.GetComponent<Text>();
        title0.text = str_title;
    }

    public void SetHelp(string str_help)
    {
        Text title0 = help.GetComponent<Text>();
        title0.text = str_help;
    }

    void LogQuestions()
    {
        Debug.Log("LogQuestions()");

        foreach (Questionnaire q in qs.questionnaires)
        {
            foreach (Question question in q.questions)
            {
                Debug.Log("Question: " + question.question);
            }
        }
    }

    private string GetCurrentName()
    {
        Questionnaire questionnaire = GetCurrentQuestionnaire();
        Question question = GetCurrentQuestion();

        string name = questionnaire.name + "-Q" + this.current_question_count;

        return name;
    }

    public bool AnswerQuestion(int button_id)
    {
        string name = GetCurrentName();
        log.NewAnswer(button_id, name);

        Debug.Log("ServerHandler::AnswerQuestion() -> q('" + name + "') = " + button_id);

        if(name == "fitzpatrick-Q0")
        {
            skin_tone = button_id;
        }

        if (current_question_count == GetCurrentQuestionnaire().questions.Length -1)
        {
            current_questionnaire_count++;
            current_question_count = 0;

            if (current_questionnaire_count == this.qs.questionnaires.Length)
            {
                current_questionnaire_count = -1;
                return false;
            }
        }
        else
        {
            current_question_count++;
        }

        return true;
    }

    public void ShowButtonGroup()
    {
        button1.transform.parent.gameObject.SetActive(true);
    }

    public void HideButtonGroup()
    {
        button1.transform.parent.gameObject.SetActive(false);
    }

    public void ShowButtons()
    {
        button1.SetActive(true);
        button2.SetActive(true);
        button3.SetActive(true);
        button4.SetActive(true);
        button5.SetActive(true);
        button6.SetActive(true);
        button7.SetActive(true);
    }

    public void HideButtons()
    {
        button1.SetActive(false);
        button2.SetActive(false);
        button3.SetActive(false);
        button4.SetActive(false);
        button5.SetActive(false);
        button6.SetActive(false);
        button7.SetActive(false);
    }

    public string toJSON()
    {
        return JsonUtility.ToJson(log.ToDataLog(TESTING), TESTING);
    }

    public void SendDataToServer()
    {
        Debug.Log("ServerHandler::SendDataToServer()");
        StartCoroutine(CheckForServerResponse());
        StartCoroutine(SendJSONToServer());
    }

    IEnumerator SendJSONToServer()
    {
        string data = toJSON();

        Debug.Log(data);

        UnityWebRequest www = new UnityWebRequest(API_URL);
        www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(data));
        www.downloadHandler = new DownloadHandlerBuffer();
        www.method = UnityWebRequest.kHttpVerbPOST;

        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            Debug.Log(www.error);
        }
        else
        {
            this.sr = JsonUtility.FromJson<ServerResponse>(www.downloadHandler.text);            
        }
    }

    IEnumerator CheckForServerResponse()
    {
        while (sr == null)
        {
            // do nothing
            yield return new WaitForSeconds(1);
        }

        // now we have a server response
        Debug.Log("Participant ID: " + sr.PID);

        SetTitle("Response from server: " + sr.ToString());
    }
}

public class ServerResponse
{
    public string PID;
    public string status;

    override public string ToString()
    {
        return "ServerResponse(status:" + status + ", PID:" + PID + ")";
    }
}