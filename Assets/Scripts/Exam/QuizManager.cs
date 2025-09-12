using UnityEngine;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;

namespace WDS.Exams
{
    public class QuizManager : MonoBehaviour
    {
        [System.Serializable]
        public class QuizBank
        {
            public List<QuestionData> QuestionTable = new();
        
            public void Init(string json)
            {
                Dictionary<string, QuestionData> loadedData = JsonConvert.DeserializeObject<Dictionary<string, QuestionData>>(json);
                QuestionTable = loadedData.Values.ToList();
            }
        }
        private QuizBank bank;

        public TextAsset examFile;
        public int questionsPerTest = 15;
        public ToggleGroup toggleGroup;

        [Space]
        public Text questionText;
        public Text submitButtonText; //changes to submit at final question
        public Text progressText, scoreText, timeText;
        public Transform answerParent;
        public GameObject answerPrefab;

        [Space]
        public GameObject reviewPanel;
        public Animator reviewAnimator;
        public Transform reviewParent;
        public GameObject reviewPrefab;
        public Color correctColor, wrongColor;

        private int current = 0;
        private Toggle[] currentToggles = new Toggle[3];
        private List<QuestionCompleteData> questions = new List<QuestionCompleteData>();
        private int seconds, minutes;
        private bool reviewed;

        private void Start()
        {
            bank = new QuizBank();
            bank.Init(examFile.text);

            current = 1;
            progressText.text = "Question: " + current+" / "+questionsPerTest;
            RenderCurrent();
            timeText.text = "00:00";
            StartCoroutine(TimeLoop());
        }

        private IEnumerator TimeLoop()
        {
            while(!reviewed)
            {
                yield return new WaitForSeconds(1f);
                seconds++;
                if(seconds>=60) { seconds = 0; minutes++; }
                timeText.text = $"{minutes:00}:{seconds:00}";
            }
        }

        //goes to next question!
        public void Next()
        {
            current++;

            if(reviewed) { 
                SceneManager.LoadScene("Exam");
                return; 
            }

            if(current == questionsPerTest) { 
                submitButtonText.text = "Submit!"; 
            }
            
            //check answer?
            if(currentToggles[0].isOn) { questions[questions.Count - 1].answer = "A"; }
            else if(currentToggles[1].isOn) { questions[questions.Count - 1].answer = "B"; }
            else if(currentToggles[2].isOn) { questions[questions.Count - 1].answer = "C"; }

            if(current > questionsPerTest)
            {
                ShowScores();
                return;
            }

            progressText.text = "Question: " + current+" / "+questionsPerTest;
            RenderCurrent();
        }

        public void Exit()
        {
            LoadingUI.Instance.LoadScene("Simuulation");
        }

        private void RenderCurrent()
        {
            for(int i = 0; i < answerParent.childCount; i++)
                Destroy(answerParent.GetChild(i).gameObject);

            QuestionData q = bank.QuestionTable[Random.Range(0, bank.QuestionTable.Count)];
            questions.Add(new QuestionCompleteData{
                question = q.question, correct = q.correct, answer = "",
            });

            questionText.text = q.question;
            for(int i = 0; i < q.options.Count; i++)
            {
                //shown as: A: question...
                GameObject g = Instantiate(answerPrefab, answerParent);
                g.transform.GetChild(0).GetComponent<Text>().text = q.options[IndexedAlpha(i)];
                Toggle t = g.transform.GetChild(1).GetComponent<Toggle>();
                t.group = toggleGroup;
                currentToggles[i] = t;
            }
        }

        private void ShowScores()
        {
            questionText.text = "";
            for(int i = 0; i < answerParent.childCount; i++)
                Destroy(answerParent.GetChild(i).gameObject);

            reviewPanel.SetActive(true);
            reviewAnimator.SetBool("Open", true);
            scoreText.text = $"Scores: {CorrectQuestions() * 10} / {questionsPerTest * 10}";

            //show questions, answers and correct answers:
            for(int i = 0; i < questions.Count; i++)
            {
                GameObject g = Instantiate(reviewPrefab, reviewParent);
                g.transform.GetChild(0).GetComponent<Text>().text = questions[i].question;
                g.transform.GetChild(1).GetComponent<Text>().text = (i+1)+". ";
                Text t = g.transform.GetChild(2).GetComponent<Text>();
                t.text = "Your Answer: "+questions[i].answer + " | Correct Answer: " + questions[i].correct;
                t.color = (questions[i].answer == questions[i].correct) ? correctColor : wrongColor;
            }

            reviewed = true;
        }

        public void HideReviewPanel()
        {
            reviewAnimator.SetBool("Open", false);
            Invoke(nameof(DelayedHideReviewPanel), 0.5f);
        }

        private void DelayedHideReviewPanel()
        {
            reviewPanel.SetActive(false);
            submitButtonText.text = "Start Again?";
        }

        private string IndexedAlpha(int i) { 
            if(i == 0) return "A"; 
            if(i == 1) return "B"; 
            if(i == 2) return "C"; 
            return ""; 
        }

        private int CorrectQuestions()
        {
            int correct = 0;
            for(int i = 0; i < questions.Count; i++)
            {
                if(questions[i].answer == questions[i].correct) correct++;
            }
            return correct;
        }
    }
}
