using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AI_Assistant : MonoBehaviour
{
    [Header("UI")]
    public InputField inputField;
    public Button sendButton;
    public Text outputText;   // shows only the latest Assistant answer (or a helpful error)

    public string assistantId;
    public string openAIApiKey;
    private string openAIOrganization;

    [Header("Behavior")]
    public float pollIntervalSeconds = 0.5f;
    public int   pollMaxTries = 40; // ~20s
    public float textAnimationSpeed = 0.01f;
    public bool  reuseThreadAcrossRuns = true;

    const string BaseUrl  = "https://api.openai.com/v1";
    const string ThreadPP = "pure_minimal_thread_id";
    string threadId;
    bool busy;

    void Awake()
    {
        if (reuseThreadAcrossRuns)
            threadId = PlayerPrefs.GetString(ThreadPP, string.Empty);

        if (sendButton) sendButton.onClick.AddListener(OnSend);
        SetOutput("Ready. Enter a prompt and press Send.");
    }

    void OnDestroy()
    {
        if (sendButton) sendButton.onClick.RemoveListener(OnSend);
    }

    public void OnSend()
    {
        if (busy) return;
        var prompt = inputField ? inputField.text.Trim() : "";
        if (string.IsNullOrEmpty(prompt))
        {
            SetOutput("Enter a valid prompt.");
            return;
        }

        //select the input field again!
        inputField.text = "";
        inputField.ActivateInputField();

        StartCoroutine(SendFlow(prompt));
    }

    IEnumerator SendFlow(string userText)
    {
        busy = true;
        SetOutput("Thinking …");

        // 1) Ensure thread
        if (string.IsNullOrEmpty(threadId))
        {
            var createPayload = "{}"; // minimal valid JSON
            using (var req = NewPost($"{BaseUrl}/threads", createPayload))
            {
                yield return req.SendWebRequest();
                if (!Is2xx(req.responseCode))
                {
                    ShowFail("Create thread", req);
                    busy = false; yield break;
                }
                threadId = ExtractJsonString(req.downloadHandler.text, "id");
                if (string.IsNullOrEmpty(threadId)) { SetOutput("Error: no thread id in response."); busy = false; yield break; }
                if (reuseThreadAcrossRuns) PlayerPrefs.SetString(ThreadPP, threadId);
            }
        }

        // 2) Add user message
        var msgPayload = "{\"role\":\"user\",\"content\":" + JsonString(userText) + "}";
        using (var msgReq = NewPost($"{BaseUrl}/threads/{threadId}/messages", msgPayload))
        {
            yield return msgReq.SendWebRequest();
            if (!Is2xx(msgReq.responseCode))
            {
                ShowFail("Add message", msgReq);
                busy = false; yield break;
            }
        }

        // 3) Start run
        string runId = null;
        var runPayload = "{\"assistant_id\":" + JsonString(assistantId) + "}";
        using (var runReq = NewPost($"{BaseUrl}/threads/{threadId}/runs", runPayload))
        {
            yield return runReq.SendWebRequest();
            if (!Is2xx(runReq.responseCode))
            {
                ShowFail("Start run", runReq);
                busy = false; yield break;
            }
            runId = ExtractJsonString(runReq.downloadHandler.text, "id");
            if (string.IsNullOrEmpty(runId)) { SetOutput("Error: no run id in response."); busy = false; yield break; }
        }

        // 4) Poll run until terminal
        string status = "";
        for (int i = 0; i < pollMaxTries; i++)
        {
            using (var getRun = NewGet($"{BaseUrl}/threads/{threadId}/runs/{runId}"))
            {
                yield return getRun.SendWebRequest();
                if (!Is2xx(getRun.responseCode))
                {
                    ShowFail("Get run", getRun);
                    busy = false; yield break;
                }
                var body = getRun.downloadHandler.text;
                status = ExtractJsonString(body, "status");
                if (status == "completed" || status == "failed" || status == "requires_action" || status == "cancelled" || status == "expired")
                    break;
            }
            yield return new WaitForSeconds(pollIntervalSeconds);
        }

        if (status != "completed")
        {
            SetOutput($"Run finished with status: {status}");
            busy = false; yield break;
        }

        // 5) Fetch messages (newest first) and display the first assistant answer
        using (var getMsg = NewGet($"{BaseUrl}/threads/{threadId}/messages?order=desc&limit=50"))
        {
            yield return getMsg.SendWebRequest();
            if (!Is2xx(getMsg.responseCode))
            {
                ShowFail("Get messages", getMsg);
                busy = false; yield break;
            }
            var json = getMsg.downloadHandler.text;

            var latestAnswer = ExtractLatestAssistantTextValue(json);
            if (string.IsNullOrEmpty(latestAnswer))
            {
                // Helpful fallback: show a small snippet of the raw JSON so we can see structure
                var snippet = json.Length > 1200 ? json.Substring(0, 1200) + " …(truncated)" : json;
                SetOutput("(Could not find assistant text)\n\nRaw snippet:\n" + snippet);
            }
            else
            {
                latestAnswer = StripCitations(latestAnswer);
                SetOutput(latestAnswer);
            }
        }

        busy = false;
    }

    // -------- HTTP helpers --------

    UnityWebRequest NewPost(string url, string json)
    {
        var req = new UnityWebRequest(url, "POST");
        var body = Encoding.UTF8.GetBytes(json ?? "{}");
        req.uploadHandler = new UploadHandlerRaw(body);
        req.uploadHandler.contentType = "application/json";     // explicit
        req.downloadHandler = new DownloadHandlerBuffer();
        //req.chunkedTransfer = false;                         // avoid chunked
        AttachHeaders(req);
        return req;
    }

    UnityWebRequest NewGet(string url)
    {
        var req = UnityWebRequest.Get(url);
        req.downloadHandler = new DownloadHandlerBuffer();
        //req.chunkedTransfer = false;
        AttachHeaders(req);
        return req;
    }

    void AttachHeaders(UnityWebRequest req)
    {
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Accept", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + openAIApiKey);
        req.SetRequestHeader("OpenAI-Beta", "assistants=v2");
        if (!string.IsNullOrEmpty(openAIOrganization))
            req.SetRequestHeader("OpenAI-Organization", openAIOrganization);
    }

    static bool Is2xx(long code) => code >= 200 && code < 300;

    void SetOutput(string msg)
    {
        if (outputText != null && !string.IsNullOrEmpty(msg)) {
            //outputText.text = msg ?? "";
            outputText.text = "";
            StartCoroutine(SlowAddLetters(msg));
        }
        //Debug.Log("[AI_Assistant] " + msg);
    }

    private IEnumerator SlowAddLetters(string message){
        char[] chars = message.ToCharArray();
        int g = 0;
        while(g < chars.Length) {
            yield return new WaitForSeconds(1 / textAnimationSpeed);
            outputText.text += chars[g];
            g++;
        }
    }

    void ShowFail(string label, UnityWebRequest req)
    {
        var raw = req.downloadHandler != null ? req.downloadHandler.text : "";
        var err = ExtractOpenAIError(raw);
        SetOutput($"[FAIL] {label} ({(int)req.responseCode}) : {req.error}\n{(string.IsNullOrEmpty(err) ? "" : "Error: " + err + "\n")}Raw: {raw}");
    }

    // -------- Extraction helpers --------

    // Robust extractor: find first assistant block, then first text.value within it (across newlines)
// -------- Extraction helpers --------

// Find the first assistant message (order=desc) and the first text.value inside it
static string ExtractLatestAssistantTextValue(string json)
{
    if (string.IsNullOrEmpty(json)) return null;

    // One-pass regex:
    //  - role":"assistant
    //  - then somewhere ahead a text block
    //  - capture its "value"
    var rx = new Regex(
        "\"role\"\\s*:\\s*\"assistant\"(?s).*?\"type\"\\s*:\\s*\"text\"(?s).*?\"value\"\\s*:\\s*\"(.*?)\"",
        RegexOptions.Singleline);

    var m = rx.Match(json);
    if (!m.Success) return null;

    var rawVal = m.Groups[1].Value;
    return UnescapeJson(rawVal);
}


    // Extract "error.message" from OpenAI JSON (best-effort)
    static string ExtractOpenAIError(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return "";
        int ei = raw.IndexOf("\"error\"");
        if (ei < 0) return "";
        int mi = raw.IndexOf("\"message\"", ei);
        if (mi < 0) return "";
        int qi = raw.IndexOf('"', mi + 10);
        if (qi < 0) return "";
        int qj = NextUnescapedQuote(raw, qi + 1);
        if (qj < 0) return "";
        return UnescapeJson(raw.Substring(qi + 1, qj - qi - 1));
    }

    // Extract simple string field from JSON: "key":"value" (best-effort)
    static string ExtractJsonString(string json, string key)
    {
        if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(key)) return null;
        var needle = "\"" + key + "\"";
        int i = json.IndexOf(needle, StringComparison.Ordinal);
        if (i < 0) return null;
        int c = json.IndexOf(':', i + needle.Length);
        if (c < 0) return null;
        int q1 = NextUnescapedQuote(json, c + 1);
        if (q1 < 0) return null;
        int end;
        var raw = ReadJsonString(json, q1, out end);
        if (raw == null) return null;
        return UnescapeJson(raw);
    }

    // Returns index of the next unescaped '"' starting at or after 'start'
    static int NextUnescapedQuote(string s, int start)
    {
        for (int i = start; i < s.Length; i++)
        {
            if (s[i] == '"')
            {
                int back = 0; int j = i - 1;
                while (j >= 0 && s[j] == '\\') { back++; j--; }
                if ((back % 2) == 0) return i;
            }
        }
        return -1;
    }

    // Reads a JSON-escaped string starting at opening quote index 'q1'.
    // Returns raw contents (still escaped), and sets 'end' to the closing quote index.
    static string ReadJsonString(string s, int q1, out int end)
    {
        end = -1;
        if (q1 < 0 || q1 >= s.Length || s[q1] != '"') return null;

        var sb = new StringBuilder();
        bool escaped = false;
        for (int i = q1 + 1; i < s.Length; i++)
        {
            char c = s[i];
            if (escaped)
            {
                sb.Append('\\').Append(c);
                escaped = false;
                continue;
            }
            if (c == '\\') { escaped = true; continue; }
            if (c == '"') { end = i; break; }
            sb.Append(c);
        }
        if (end == -1) return null;
        return sb.ToString();
    }

    // Minimal JSON string unescape for common sequences
    static string UnescapeJson(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return s
            .Replace("\\\"", "\"")
            .Replace("\\\\", "\\")
            .Replace("\\/", "/")
            .Replace("\\n", "\n")
            .Replace("\\r", "\r")
            .Replace("\\t", "\t");
        // (Ignoring \uXXXX for brevity in this minimal step)
    }

    // Optional: strip inline citation markers if the assistant inserts them
    static string StripCitations(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        s = Regex.Replace(s, "【[^】]*】", "");
        s = Regex.Replace(s, "［[^］]*］", "");
        s = Regex.Replace(s, "\\[[0-9]+:[0-9]+[^\\]]*\\]", "");
        s = Regex.Replace(s, "\\[[0-9]+[^\\]]*\\]", "");
        return s.Trim();
    }

    /// <summary>Helper to JSON-escape and wrap a string in quotes.</summary>
    static string JsonString(string s)
    {
        if (s == null) return "null";
        return "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r") + "\"";
    }
}
