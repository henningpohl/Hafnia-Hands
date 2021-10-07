using DG.Tweening;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public struct TrialData {
    public DateTime TrialStart;
    public DateTime TrialEnd;
    public string MovementData;
    public float TrackingConfidence;
    public bool last;
}

public class StudyBoxController : MonoBehaviour {
    public StudyBox LeftBox, RightBox;
    public Color ActiveColor, InactiveColor;
    public TextMesh CountdownText;
    public AudioSource CountdownAudio;
    public Camera MainCamera;

    private MovementData movementData;
    private bool trialActive = false;
    private DateTime trialStart;
    private float handOutsideTime = 0f;

    void Start() {
        LeftBox.SetActive(false);
        RightBox.SetActive(false);

        LeftBox.SetColors(ActiveColor, InactiveColor);
        RightBox.SetColors(ActiveColor, InactiveColor);

        CountdownText.text = "";

        //StartTrial(); // TODO: remove after testing
    }

    void Update() {
        if(trialActive == false) {
            return;
        }

        if(LeftBox.IsActive) {
            if(LeftBox.IsHandInside) {
                handOutsideTime = 0f;
            } else {
                handOutsideTime += Time.deltaTime;
            }
        } else if(RightBox.IsActive) {
            if(RightBox.IsHandInside) {
                handOutsideTime = 0f;
            } else {
                handOutsideTime += Time.deltaTime;
            }
        }

        if(handOutsideTime < 1f) {
            CountdownText.text = "";
        } else {
            CountdownText.text = "Please keep your hand inside the box";
        }

    }

    private void FixedUpdate() {
        if(movementData == null || trialActive == false) {
            return;
        }

        // Overall data
        movementData.Timestamps.Add(DateTime.Now.ToString("O"));
        movementData.RecordingTimeSeconds.Add((float)(DateTime.Now - trialStart).TotalSeconds);
        movementData.CenterEyePositions.Add(MainCamera.transform.position);
        movementData.CenterEyeOrientations.Add(MainCamera.transform.rotation.eulerAngles);
        // Box data
        movementData.LeftBoxPositions.Add(LeftBox.transform.position);
        movementData.LeftBoxActive.Add(LeftBox.IsActive);
        movementData.LeftBoxOrientations.Add(LeftBox.transform.rotation.eulerAngles);
        movementData.LeftBoxHasHandInside.Add(LeftBox.IsHandInside);
        movementData.RightBoxPositions.Add(RightBox.transform.position);
        movementData.RightBoxActive.Add(RightBox.IsActive);
        movementData.RightBoxOrientations.Add(RightBox.transform.rotation.eulerAngles);
        movementData.RightBoxHasHandInside.Add(RightBox.IsHandInside);
        // Hand data
        movementData.LeftHandScale.Add(LeftBox.Hand.HandScale);
        movementData.LeftHandPositions.Add(LeftBox.Hand.transform.position);
        movementData.LeftHandOrientations.Add(LeftBox.Hand.transform.rotation.eulerAngles);
        movementData.LeftHandSkeletonData.Add(MovementData.OVRHandToString(LeftBox.HandBones));
        movementData.LeftHandTracked.Add(LeftBox.Hand.IsTracked);
        movementData.LeftHandTrackingConfidence.Add(LeftBox.Hand.HandConfidence);
        movementData.RightHandScale.Add(RightBox.Hand.HandScale);
        movementData.RightHandPositions.Add(RightBox.Hand.transform.position);
        movementData.RightHandOrientations.Add(RightBox.Hand.transform.rotation.eulerAngles);
        movementData.RightHandSkeletonData.Add(MovementData.OVRHandToString(RightBox.HandBones));
        movementData.RightHandTracked.Add(RightBox.Hand.IsTracked);
        movementData.RightHandTrackingConfidence.Add(RightBox.Hand.HandConfidence);
    }

    public async Task<TrialData> StartTrial(int durationInMs = 60 * 1000) {

        Sequence countdown = DOTween.Sequence();
        CountdownText.fontSize = 500;
        CountdownText.text = "3";
        CountdownAudio.Play();
        countdown.Append(CountdownText.transform.DOPunchScale(CountdownText.transform.localScale * 1.2f, 1f, 0));
        countdown.AppendCallback(() => { CountdownText.text = "2"; CountdownAudio.Play();});
        countdown.Append(CountdownText.transform.DOPunchScale(CountdownText.transform.localScale * 1.2f, 1f, 0));
        countdown.AppendCallback(() => { CountdownText.text = "1"; CountdownAudio.Play(); });
        countdown.Append(CountdownText.transform.DOPunchScale(CountdownText.transform.localScale * 1.2f, 1f, 0));
        countdown.AppendCallback(() => CountdownText.text = "");
        await countdown.AsyncWaitForCompletion();
        CountdownText.fontSize = 50;

        movementData = new MovementData();
        var trialData = new TrialData();
        trialData.TrialStart = DateTime.Now;
        trialActive = true;
        trialStart = trialData.TrialStart;

        if(UnityEngine.Random.value < 0.5f) {
            LeftBox.SetActive(true);
            await LeftBox.StartMovement(durationInMs / 2);
            LeftBox.SetActive(false);
            RightBox.SetActive(true);
            await RightBox.StartMovement(durationInMs / 2);
        } else {
            RightBox.SetActive(true);
            await RightBox.StartMovement(durationInMs / 2);
            RightBox.SetActive(false);
            LeftBox.SetActive(true);
            await LeftBox.StartMovement(durationInMs / 2);
        }
        LeftBox.SetActive(false);
        RightBox.SetActive(false);

        trialActive = false;
        trialData.TrialEnd = DateTime.Now;
        trialData.MovementData = movementData.Compress();
        trialData.TrackingConfidence = movementData.TrackingConfidence;

        return trialData;
    }
}
