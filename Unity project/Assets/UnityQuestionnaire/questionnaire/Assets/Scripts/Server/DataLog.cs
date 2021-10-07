using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

[System.Serializable]
public class DataLog
{
    public Answer[] answers;
    public bool test;
    public long end;
    public string device;
    public string deviceID;
    public string IP;
    public string order;
    public long start;

    public DataLog(List<Answer> answers, bool isTest, long start, string order)
    {
        this.device = SystemInfo.deviceModel;
        this.deviceID = SystemInfo.deviceUniqueIdentifier;
        this.answers = answers.ToArray();
        this.test = isTest;
        this.end = new System.DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
        this.start = start;
        this.order = order;

        if(!isTest)
        {
            this.IP = new WebClient().DownloadString("http://icanhazip.com").Trim();
        }
        else {
            this.IP = "localhost";
        }
    }
}