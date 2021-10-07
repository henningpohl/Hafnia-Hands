using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class StudyControllerFrontiers : StudyController {
    private int num_trials = 4; // three + trial

    public FormsManager Forms;
    public ServerHandler sh;
    public StudyBoxController BoxController;
    public MaterialSwitcher HandSkinSwitcher;

    private List<ExperimentalCondition> order;
    public int current_trial = -1; // 0-8
    public ExperimentalCondition condition;

    private int max_blocks = Enum.GetNames(typeof(ExperimentalCondition)).Length - 1;

    public enum ExperimentalCondition
    {
        HandMatch = 0,
        HandMismatch = 1,
        HandAlien = 2,
        Trial = -1
    }

    public List<ExperimentalCondition> GetConditionOrder()
    {
        List<ExperimentalCondition> list = new List<ExperimentalCondition>() {
            ExperimentalCondition.HandMatch,
            ExperimentalCondition.HandMismatch,
            ExperimentalCondition.HandAlien
        };
        list.Shuffle();

        return list;
    }

    void Start() {
        UnityInitializer.AttachToGameObject(this.gameObject);

        this.order = GetConditionOrder();
        sh.SetOrder(String.Join(", ", order));

        PreSurvey();

        Debug.Log("numTrials, " + this.num_trials + ", " + max_blocks);

        sh.SetNumTrials((this.num_trials-1) * this.max_blocks);
    }

    void PositionBoxes()
    {
        //GameObject User = Forms.GetComponent<UserFollower>().User;
        Vector3 newPos = Camera.main.transform.position + Camera.main.transform.forward * 0.43f;

        BoxController.transform.position = newPos;
        BoxController.transform.LookAt(Camera.main.transform.position);

        // a bit down'
        newPos.y *= 0.9f;
        BoxController.transform.position = newPos;
    }

    public void HandleDoneSurvey()
    {
        sh.IncrementRound();
        Debug.Log("StudyController::HandleDoneSurvey()");
        this.InitRound();
    }

    private void RedoTrial()
    {
        current_trial--;
        //sh.DecrementRound();
        StartCoroutine(DismissDialog());
    }

    IEnumerator DismissDialog()
    {
        yield return new WaitForSeconds(10);
        Forms.ShowForm(FormsManager.FormType.Messages, false);
        InitRound();
    }

    private void store_data_to_s3(string data, string postfix="")
    {
        string key = SystemInfo.deviceUniqueIdentifier + "_" + current_trial + postfix + ".txt";

        string awsAccessKeyId = "";
        string awsSecretAccessKey = "";
        string bucket = "";

        AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;

        IAmazonS3 s3client  = new AmazonS3Client(awsAccessKeyId, awsSecretAccessKey, RegionEndpoint.EUCentral1);

        PutObjectRequest request = new PutObjectRequest
        {
            BucketName = bucket,
            Key = key,
            ContentBody = data
        };

        s3client.PutObjectAsync(request, (responseObject) =>
        {
            // done
            Debug.Log("STORED DATA TO S3");
            Debug.Log(responseObject.Response);
            Debug.Log(responseObject.state);
            Debug.Log(responseObject.Exception);
            Debug.Log(responseObject.ToString());
        });
    }

    private async void InitRound()
    {
        Debug.Log("StudyController::InitRound()");

        PositionBoxes();

        var round = await BeginNewRound();

        sh.addTrackingConfidence(round.TrackingConfidence);

        Debug.Log("Tracking confidence: " + round.TrackingConfidence + ", trial: " + current_trial);

        if (round.TrackingConfidence < .6f && current_trial == 0)
        {
            store_data_to_s3(round.MovementData, "_redo_trial");
            // this was the trial round, and tracking was horrible,
            // lets warn the user and try again
            Forms.ShowMessage("Bad tracking", "The hand tracking does not seem to be optimal. Could you place yourself in a space with better lighting and try again? Retrying in 10 seconds.");
            RedoTrial();
            return;
        }
        else if (!round.last)
        {
            // show questionnaire
            store_data_to_s3(round.MovementData);
            EndRound();
        }
        else
        {
            // done now
            store_data_to_s3(round.MovementData);
            ExperimentDone();
        }
    }

    private ExperimentalCondition GetCurrentCondition()
    {
        if(current_trial <= 0)
        {
            return ExperimentalCondition.Trial;
        }

        return this.order[current_trial % this.max_blocks];
    }

    private void PreSurvey()
    {
        Forms.ShowForm(FormsManager.FormType.Survey, true);
        StartSurvey.Invoke();
    }

    IEnumerator ShowSurvey()
    {
        yield return new WaitForSeconds(2);
        Forms.ShowForm(FormsManager.FormType.Survey, true);
        StartSurvey.Invoke();
    }

    public void EndRound()
    {
        StartCoroutine(ShowSurvey());
    }

    private int GetHandMaterialIndex()
    {
        int index = this.sh.skin_tone - 1;

        switch(GetCurrentCondition())
        {
            case ExperimentalCondition.HandAlien:
                return 6;
            case ExperimentalCondition.HandMatch:
                return index;
            case ExperimentalCondition.HandMismatch:
                int antiindex = index < 3 ? index + 3 : index - 3;
                return antiindex;
        }

        // return Skeleton to signal something went wrong (shouldn't happen)
        return HandSkinSwitcher.Materials.Count - 1; 
    }

    private void ExperimentDone()
    {
        Debug.Log("StudyController:ExperimentDone()");

        SendToServer.Invoke();

        Forms.ShowForm(FormsManager.FormType.Final, true);
    }

    public void ClickedFinalDone()
    {
        string token = SystemInfo.deviceUniqueIdentifier;
        string url = Raffle.GetRaffleURL(token);
        Application.OpenURL(url);

        Forms.ShowForm(FormsManager.FormType.Final, false);
    }

    public async Task<TrialData> BeginNewRound()
    {
        // before starting, check if we are actually done
        if(this.current_trial >= this.max_blocks * (this.num_trials-1))
        {
            var t = new TrialData();
            t.last = true;
            return t;
        }

        this.current_trial++;
        condition = GetCurrentCondition();

        Debug.Log(
            "Beginning new trial (" + this.current_trial + ") " +
            "with condition: (" + condition.ToString() + ") " +
            "and skin-tone: (" + GetHandMaterialIndex().ToString() + ")"
        );

        HandSkinSwitcher.ChangeHandSkin(GetHandMaterialIndex());

        var res = await BoxController.StartTrial(20000); // 25000
        res.last = false;

        return res;
    }
    

    void Update() {

    }

}
