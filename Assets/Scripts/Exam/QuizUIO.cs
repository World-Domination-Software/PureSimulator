using UnityEngine;
using WDS.Exams;

public class QuizUIO : MonoBehaviour
{
    private string Answer;
    private QuizManager quizManager;

    public void Init(string a, QuizManager qm)
    {
        quizManager = qm;
        Answer = a;
    }

    public void OnClick()
    {
        quizManager.OnClickAnswer(Answer);
    }
}
