using System;
using System.Collections.Generic;

public class Log
{
    private List<Answer> answers = new List<Answer>();
    private long start;
    public string order;

    public Log()
    {
        this.start = new System.DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
    }

    public void SetOrder(string order)
    {
        this.order = order;
    }

    public void NewAnswer(int answer, string name)
    {
        this.answers.Add(
            new Answer(answer, name)
        );
    }

    public DataLog ToDataLog(bool isTest)
    {
        return new DataLog(answers, isTest, start, order);
    }

    public int NumAnswers()
    {
        return answers.Count;
    }
}

