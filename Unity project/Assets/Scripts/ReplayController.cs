using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ReplayController : MonoBehaviour {
    [Header("Selection Screen")]
    public GameObject SelectionGroup;
    
    [Header("Loading Screen")]
    public GameObject ProgressGroup;
    public Slider ProgressSlider; 
    
    [Header("Playback Screen")]
    public GameObject PlaybackGroup;
    public Slider PlaybackSlider;
    public Text PlaybackText;

    [Header("Replay Objects")]
    public GameObject Head;
    public GameObject LeftHand;
    [NamedArrayAttribute(new string[] { "Hand_WristRoot", "Hand_ForearmStub", "Hand_Thumb0", "Hand_Thumb1",
        "Hand_Thumb2", "Hand_Thumb3", "Hand_Index1", "Hand_Index2", "Hand_Index3", "Hand_Middle1", "Hand_Middle2", "Hand_Middle3",
        "Hand_Ring1", "Hand_Ring2", "Hand_Ring3", "Hand_Pinky0", "Hand_Pinky1", "Hand_Pinky2", "Hand_Pinky3", 
        "Hand_ThumbTip", "Hand_IndexTip", "Hand_MiddleTip", "Hand_RingTip", "Hand_PinkyTip", "Hand_End" })]
    public Transform[] LeftHandBones;
    public GameObject RightHand;
    [NamedArrayAttribute(new string[] { "Hand_WristRoot", "Hand_ForearmStub", "Hand_Thumb0", "Hand_Thumb1",
        "Hand_Thumb2", "Hand_Thumb3", "Hand_Index1", "Hand_Index2", "Hand_Index3", "Hand_Middle1", "Hand_Middle2", "Hand_Middle3",
        "Hand_Ring1", "Hand_Ring2", "Hand_Ring3", "Hand_Pinky0", "Hand_Pinky1", "Hand_Pinky2", "Hand_Pinky3", 
        "Hand_ThumbTip", "Hand_IndexTip", "Hand_MiddleTip", "Hand_RingTip", "Hand_PinkyTip", "Hand_End" })]
    public Transform[] RightHandBones;
    public GameObject LeftBox;
    public GameObject RightBox;

    private CancellationTokenSource loadCancelTokenSource;
    private Queue<Action> mainThreadQueue = new Queue<Action>();

    private MovementData data;
    private DateTime replayRecordingStart;
    private float replayStartTime;
    private int replayPosition;


    void Start() {
        loadCancelTokenSource = new CancellationTokenSource();
        ProgressGroup.SetActive(false);
        PlaybackGroup.SetActive(false);
        LeftBox.GetComponentInChildren<Renderer>().material.color = Color.blue;
        RightBox.GetComponentInChildren<Renderer>().material.color = Color.blue;
    }

    private async Task LoadLog(string path, IProgress<int> progress, CancellationToken ct) {
        data = null;
        string logString = "";
        int percentDone = 0;
        if(progress != null) {
            progress.Report(percentDone);
        }

        using(StreamReader sr = new StreamReader(path)) {
            logString = await sr.ReadToEndAsync();
        }

        if(ct.IsCancellationRequested) {
            return;
        }

        percentDone = 10;
        if(progress != null) {
            progress.Report(percentDone);
        }

        var logData = Convert.FromBase64String(logString);
        var output = new StringBuilder();
        using(MemoryStream bufferStream = new MemoryStream(logData)) {
            byte[] buffer = new byte[1024];
            using(GZipStream gs = new GZipStream(bufferStream, CompressionMode.Decompress)) {
                while(true) {
                    if(ct.IsCancellationRequested) {
                        return;
                    }
                    if(progress != null) {
                        int newDone = 10 + (int)((90f * bufferStream.Position) / bufferStream.Length);
                        if(newDone > percentDone) {
                            percentDone = newDone;
                            progress.Report(percentDone);
                        }
                    }
                    var bytesRead = await gs.ReadAsync(buffer, 0, 1024);
                    output.Append(Encoding.ASCII.GetString(buffer, 0, bytesRead));
                    if(bytesRead == 0) {
                        break;
                    }
                } 
            }
        }

        data = JsonUtility.FromJson<MovementData>(output.ToString());
        replayRecordingStart  = DateTime.ParseExact(data.Timestamps[0], "O", CultureInfo.InvariantCulture);
        replayStartTime = Time.time;
        replayPosition = 0;
    }

    public void OnSelectionClick() {
        //var path = EditorUtility.OpenFilePanelWithFilters("Select log file", UnityEngine.Windows.Directory.localFolder, new string[] {"Log files", "txt"});
        var path = "E:\\Projects\\VR Hands\\Analysis\\test3.txt";

        ProgressGroup.SetActive(true);
        SelectionGroup.SetActive(false);
        LoadLog(path, new Progress<int>(x => {
            ProgressSlider.value = 0.01f * x;
        }), loadCancelTokenSource.Token).ContinueWith(x => {
            lock(mainThreadQueue) {
                mainThreadQueue.Enqueue(() => {
                    ProgressGroup.SetActive(false);
                    PlaybackGroup.SetActive(true);
                });
            }
        });
    }


    void Update() {
        lock(mainThreadQueue) {
            while(mainThreadQueue.Count > 0) {
                mainThreadQueue.Dequeue().Invoke();
            }
        }

        if(data == null) {
            return;
        }

        var replayTime = Time.time - replayStartTime;
        for(int i = replayPosition; i < data.Timestamps.Count; ++i) {
            if(data.RecordingTimeSeconds[i] < replayTime) { 
                replayPosition = i;
            } else {
                break;
            }
        }

        var endTime = TimeSpan.FromSeconds(data.RecordingTimeSeconds[data.RecordingTimeSeconds.Count - 1]);
        var curTime = TimeSpan.FromSeconds(data.RecordingTimeSeconds[replayPosition]);
        PlaybackText.text = $"{curTime.Minutes}:{curTime.Seconds:00}/{endTime.Minutes}:{endTime.Seconds:00}";
        PlaybackSlider.value = (float)(curTime.TotalMilliseconds / endTime.TotalMilliseconds);

        Head.transform.SetPositionAndRotation(
            data.CenterEyePositions[replayPosition],
            Quaternion.Euler(data.CenterEyeOrientations[replayPosition]));
        LeftBox.SetActive(data.LeftBoxActive[replayPosition]);
        LeftBox.transform.SetPositionAndRotation(
            data.LeftBoxPositions[replayPosition],
            Quaternion.Euler(data.LeftBoxOrientations[replayPosition])
        );

        RightBox.SetActive(data.RightBoxActive[replayPosition]);
        RightBox.transform.SetPositionAndRotation(
            data.RightBoxPositions[replayPosition],
            Quaternion.Euler(data.RightBoxOrientations[replayPosition])
        );

        LeftHand.SetActive(data.LeftHandTracked[replayPosition]);
        LeftHand.transform.SetPositionAndRotation(
            data.LeftHandPositions[replayPosition],
            Quaternion.Euler(data.LeftHandOrientations[replayPosition])
        );

        RightHand.SetActive(data.RightHandTracked[replayPosition]);
        RightHand.transform.SetPositionAndRotation(
            data.RightHandPositions[replayPosition],
            Quaternion.Euler(data.RightHandOrientations[replayPosition])
        );

        var leftHandBoneData = data.LeftHandSkeletonData[replayPosition].Split('|')
            .Select(x => x.Split(','))
            .Select(x => new { Id=int.Parse(x[0]), X=float.Parse(x[1]), Y=float.Parse(x[2]), Z=float.Parse(x[3]) })
            .ToList();
        foreach(var bd in leftHandBoneData) {
            var transform = LeftHandBones[bd.Id];
            if(transform != null) {
                transform.localRotation = Quaternion.Euler(bd.X, bd.Y, bd.Z);
            }
        }

        var rightHandBoneData = data.RightHandSkeletonData[replayPosition].Split('|')
            .Select(x => x.Split(','))
            .Select(x => new { Id = int.Parse(x[0]), X = float.Parse(x[1]), Y = float.Parse(x[2]), Z = float.Parse(x[3]) })
            .ToList();
        foreach(var bd in rightHandBoneData) {
            var transform = RightHandBones[bd.Id];
            if(transform != null) {
                transform.localRotation = Quaternion.Euler(bd.X, bd.Y, bd.Z);
            }
        }

    }

    private void OnDisable() {
        loadCancelTokenSource.Cancel();
    }
}
