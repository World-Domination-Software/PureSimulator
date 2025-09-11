using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace WDS.Exams
{
    public class QuizManager : MonoBehaviour
    {
        [Space, Header("Data")]
        [Tooltip("File name inside StreamingAssets, e.g. pure_ie_exam.json")]
        public string quizFile = "pure_ie_exam.json";
        [Range(1, 200)]
        public int questionsPerExam = 15;
        [Tooltip("Seed for reproducible draws; leave 0 for random")]
        public int seed = 0;

        [Header("UI")]
        //public Text titleText;
        public Text questionText;
        public Text progressText;
        public Transform optionsParent;     // container with toggle buttons or UI rows
        public GameObject optionRowPrefab;  // prefab with: Toggle + TMP_Text label
        public Button nextButton;
        public Button prevButton;
        public Button submitButton;
        public Button challengeButton;      // visible on review screen per question
        public GameObject reviewPanel;      // panel to show results after submit
        public Text scoreText;
        public Transform reviewListParent;  // container for review rows
        public GameObject reviewRowPrefab;  // shows Q, user answer, correct answer, and a "Challenge" button

        [Header("Discord")]
        //public DiscordChallenge discord;

        QuizBank _bank;
        List<Question> _selected;
        int _index;
        readonly Dictionary<int, string> _answers = new(); // questionId -> "A"/"B"/...

        enum Mode { Taking, Review }
        Mode _mode = Mode.Taking;

        void Start()
        {
            _bank = QuizLoader.LoadFromStreamingAssets(quizFile);

            // pick X
            _selected = new List<Question>(_bank.questions);
            if (_selected.Count == 0) { Debug.LogError("No questions loaded"); return; }
            if (seed != 0) Random.InitState(seed);
            FisherYates(_selected);
            if (questionsPerExam < _selected.Count) _selected.RemoveRange(questionsPerExam, _selected.Count - questionsPerExam);

            _index = 0;
            BuildOptionsUI();
            RenderCurrent();

            Application.targetFrameRate = 30;
        }

        public void Exit()
        {
            LoadingUI.Instance.LoadScene("Simulation");
        }

        void FisherYates<T>(IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        void BuildOptionsUI()
        {
            // Clear existing
            foreach (Transform child in optionsParent) Destroy(child.gameObject);

            // We build 4 slots max; we’ll hide unused.
            for (int i = 0; i < 6; i++)
            {
                var row = Instantiate(optionRowPrefab, optionsParent);
                row.name = $"OptionRow_{i}";
                row.SetActive(false);
            }

            nextButton.onClick.AddListener(Next);
            prevButton.onClick.AddListener(Prev);
            submitButton.onClick.AddListener(SubmitExam);
        }

        void RenderCurrent()
        {
            _mode = Mode.Taking;
            reviewPanel?.SetActive(false);

            var q = _selected[_index];
            questionText.text = q.q;
            progressText.text = $"Question {_index + 1}/{_selected.Count}";

            // Map options (A,B,C,...) deterministically (already ordered in JSON sample)
            var opts = q.options;
            var keys = opts.keys;
            var vals = opts.values;

            for (int i = 0; i < optionsParent.childCount; i++)
            {
                var row = optionsParent.GetChild(i).gameObject;
                if (i < keys.Count)
                {
                    row.SetActive(true);
                    var toggle = row.GetComponentInChildren<Toggle>(true);
                    var label = row.GetComponentInChildren<Text>(true);
                    string key = keys[i];
                    label.text = $"{key}) {vals[i]}";

                    toggle.isOn = _answers.TryGetValue(q.id, out var chosen) && string.Equals(chosen, key, StringComparison.OrdinalIgnoreCase);
                    toggle.onValueChanged.RemoveAllListeners();
                    toggle.onValueChanged.AddListener(on =>
                    {
                        if (on) _answers[q.id] = key;
                        else if (_answers.TryGetValue(q.id, out var k) && k == key) _answers.Remove(q.id);
                    });
                }
                else row.SetActive(false);
            }

            prevButton.gameObject.SetActive(_index > 0);
            nextButton.gameObject.SetActive(_index < _selected.Count - 1);
        }

        void Next()
        {
            if (_index < _selected.Count - 1) { _index++; RenderCurrent(); }
        }

        void Prev()
        {
            if (_index > 0) { _index--; RenderCurrent(); }
        }

        void SubmitExam()
        {
            // Simple validation: require an answer for each visible question
            foreach (var q in _selected)
                if (!_answers.ContainsKey(q.id))
                {
                    Debug.LogWarning("Please answer all questions before submitting.");
                    return;
                }

            int score = 0;
            foreach (var q in _selected)
                if (string.Equals(_answers[q.id], q.correct, StringComparison.OrdinalIgnoreCase)) score++;

            scoreText.text = $"Score: {score} / {_selected.Count}";
            BuildReviewList();
            reviewPanel?.SetActive(true);
            _mode = Mode.Review;

            // Persist simple stats
            var best = PlayerPrefs.GetInt($"{_bank.id}_best", 0);
            if (score > best)
            {
                PlayerPrefs.SetInt($"{_bank.id}_best", score);
                PlayerPrefs.Save();
            }
        }

        void BuildReviewList()
        {
            // Clear
            foreach (Transform child in reviewListParent) Destroy(child.gameObject);

            foreach (var q in _selected)
            {
                var row = Instantiate(reviewRowPrefab, reviewListParent);
                var labels = row.GetComponentsInChildren<Text>(true);
                string user = _answers.TryGetValue(q.id, out var a) ? a : "(no answer)";
                string userText = q.options.Dict.TryGetValue(user, out var ut) ? $"{user}) {ut}" : user;
                string corrText = q.options.Dict.TryGetValue(q.correct, out var ct) ? $"{q.correct}) {ct}" : q.correct;

                // Expect: labels[0]=Q, labels[1]=User, labels[2]=Correct
                if (labels.Length >= 3)
                {
                    labels[0].text = q.q;
                    labels[1].text = $"Your answer: {userText}";
                    labels[2].text = $"Correct: {corrText}";
                    labels[2].color = Color.green;
                    if (!string.Equals(user, q.correct, StringComparison.OrdinalIgnoreCase))
                        labels[1].color = new Color(1f, .4f, .4f);
                }

                /*var challengeBtn = row.GetComponentInChildren<Button>(true);
                if (challengeBtn != null && discord != null)
                {
                    challengeBtn.onClick.RemoveAllListeners();
                    challengeBtn.onClick.AddListener(() =>
                    {
                        discord.SendChallenge(
                            quizId: _bank.id,
                            questionId: q.id,
                            questionText: q.q,
                            userAnswerText: userText,
                            correctAnswerText: corrText
                        );
                    });
                }*/
            }
        }
    }
}
