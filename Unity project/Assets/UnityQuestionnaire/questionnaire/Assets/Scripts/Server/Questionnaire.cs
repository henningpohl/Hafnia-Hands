[System.Serializable]
public class Questionnaire
{
    //these variables are case sensitive and must match the strings "firstName" and "lastName" in the JSON.
    public string name;
    public Question[] questions;
    public int[] exclude_rounds;
}