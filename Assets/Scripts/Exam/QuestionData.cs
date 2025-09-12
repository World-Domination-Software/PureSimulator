using System.Collections.Generic;

[System.Serializable]
public class QuestionData
{
    public string question;
    public Dictionary<string, string> options = new Dictionary<string, string>();
    public string correct;
}

[System.Serializable]
public class QuestionCompleteData
{
    public string question;
    public string answer = "None";
    public string correct;
}