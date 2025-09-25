using System.Collections.Generic;

[System.Serializable]
public class QuestionData
{
    public string question;
    public Dictionary<string, string> options = new Dictionary<string, string>();
    public string correct;
    public string answer;
}