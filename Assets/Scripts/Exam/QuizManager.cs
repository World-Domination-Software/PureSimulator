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
        public Text progressText, scoreText, timeText;
        public GameObject submitButton;
        public Transform answerParent;
        public GameObject answerPrefab;

        [Space]
        public GameObject reviewPanel;
        public Animator reviewAnimator;
        public Transform reviewParent;
        public GameObject reviewPrefab;
        public Color correctColor, wrongColor;

        [Space]
        public GameObject initialPanel;
        public Animator initialAnimator;
        public Dropdown numQeustionPerExamDD;
        public Dropdown timeDropdown;

        private int current = 0;
        //private Toggle[] currentToggles = new Toggle[3];
        private List<QuestionCompleteData> questions = new List<QuestionCompleteData>();
        private int maxTime;
        private bool reviewed;

        private void Start()
        {
            bank = new QuizBank();
            bank.Init(examFile.text);

            initialPanel.SetActive(true);
            initialAnimator.SetBool("Open",true);
            numQeustionPerExamDD.value = 2;
            timeDropdown.value = 2;
            progressText.text = "";
            timeText.text = "";
        }

        public void StartExam()
        {
            questionsPerTest = GetNumQuestion();
            maxTime = GetTime();
            current = 1;
            progressText.text = "Question: " + current+" / "+questionsPerTest;
            RenderCurrent();
            timeText.text = $"Time Remaining: {(maxTime / 60):00}:00";
            StartCoroutine(TimeLoop());

            initialAnimator.SetBool("Open", false);
            StartCoroutine(DelayedClosePanel(initialPanel));
        }

        private IEnumerator TimeLoop()
        {
            while(!reviewed)
            {
                yield return new WaitForSeconds(1f);
                maxTime--;
                if(maxTime <= 0)
                {
                    //lose the exam if time is out!
                    ShowScores();
                    timeText.text = "Timeout!";
                }
                else
                    timeText.text = $"Time Remaining: {(maxTime / 60):00}:{(maxTime % 60):00}";
            }
        }

        //goes to next question!
        public void Next()
        {
            current++;
            
            //check answer?
            /*if(currentToggles[0].isOn) { questions[questions.Count - 1].answer = "A"; }
            else if(currentToggles[1].isOn) { questions[questions.Count - 1].answer = "B"; }
            else if(currentToggles[2].isOn) { questions[questions.Count - 1].answer = "C"; }*/

            if(current > questionsPerTest)
            {
                ShowScores();
                return;
            }

            progressText.text = "Question: " + current+" / "+questionsPerTest;
            RenderCurrent();
        }

        public void StartOver()
        {
            if(reviewed) { 
                SceneManager.LoadScene("Exam");
            }
        }

        public void Exit()
        {
            LoadingUI.Instance.LoadScene("Simulation");
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
                g.transform.GetChild(0).GetComponent<Text>().text = (i+1)+". "+q.options[IndexedAlpha(i)];
                g.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(()=>OnPressed(IndexedAlpha(i)));
                /*Toggle t = g.transform.GetChild(0).GetComponent<Toggle>();
                t.group = toggleGroup;
                currentToggles[i] = t;
                t.onValueChanged.AddListener(OnTogglePressed);*/
            }
        }

        private void OnPressed(string value)
        {
            questions[questions.Count - 1].answer = value;
            Next();
        }

        private void ShowScores()
        {
            questionText.text = "";
            for(int i = 0; i < answerParent.childCount; i++)
                Destroy(answerParent.GetChild(i).gameObject);

            reviewPanel.SetActive(true);
            reviewAnimator.SetBool("Open", true);
            scoreText.text = $"Scores: {(CorrectQuestions() / (float)questionsPerTest) * 100.0f}%"; //as a percentage!

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
            StartCoroutine(DelayedClosePanel(reviewPanel));
        }

        private IEnumerator DelayedClosePanel(GameObject panel)
        {
            yield return new WaitForSeconds(0.5f);
            panel.SetActive(false);

            if(panel == reviewPanel) {
                submitButton.SetActive(true);
            }
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

        private int GetNumQuestion()
        {
            if(numQeustionPerExamDD.value == 1) return 10;
            if(numQeustionPerExamDD.value == 2) return 15;
            if(numQeustionPerExamDD.value == 3) return 20;
            if(numQeustionPerExamDD.value == 4) return 25;

            return 5;
        }

        private int GetTime()
        {
            //in seconds - 1 min = 60 sec
            if(timeDropdown.value == 0) return 300;
            if(timeDropdown.value == 1) return 600;
            if(timeDropdown.value == 2) return 1200;

            return 1800;
        }
    }
}
