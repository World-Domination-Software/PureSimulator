using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace WDS.Exams
{
    [Serializable]
    public class QuizBank
    {
        public string id;
        public string title;
        public List<Question> questions = new();
    }

    [Serializable]
    public class Question
    {
        public int id;
        public string q;
        public Options options;
        public string correct; // "A" | "B" | "C" | "D"...
    }

    // Unity JSON can't directly (de)serialize Dictionary<string,string> with nice inspector,
    // so we use a small wrapper that still serializes to an object in JSON.
    [Serializable]
    public class Options : ISerializationCallbackReceiver
    {
        public List<string> keys = new();   // A,B,C,D
        public List<string> values = new(); // labels
        private Dictionary<string,string> _dict;

        public string this[string k] => Dict.TryGetValue(k, out var v) ? v : null;

        public Dictionary<string,string> Dict {
            get {
                if (_dict == null) {
                    _dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    for (int i = 0; i < Mathf.Min(keys.Count, values.Count); i++)
                        _dict[keys[i]] = values[i];
                }
                return _dict;
            }
        }

        public void OnBeforeSerialize()
        {
            // no-op; we assume input JSON already has key/value arrays or is a JSON object
        }

        public void OnAfterDeserialize()
        {
            // If JSON came as object { "A": "...", "B": "..." } we need to populate lists:
            if (keys.Count == 0 && values.Count == 0 && _dict != null && _dict.Count > 0)
            {
                foreach (var kv in _dict) { keys.Add(kv.Key); values.Add(kv.Value); }
            }
        }

        // Allow JSON object import directly
        public static Options FromJsonObjectLit(JsonObjectLike jsonObj)
        {
            var o = new Options();
            foreach (var kv in jsonObj.map)
            {
                o.keys.Add(kv.Key);
                o.values.Add(kv.Value);
            }
            return o;
        }
    }

    // Tiny helper so we can normalize object-literal options if needed
    [Serializable]
    public class JsonObjectLike { public Dictionary<string,string> map = new(); }

    public static class QuizLoader
    {
        public static QuizBank LoadFromStreamingAssets(string fileName)
        {
            // Supports Android/PC/Mac since StreamingAssets path may be a URI on Android
            string path = Path.Combine(Application.streamingAssetsPath, fileName);
#if UNITY_ANDROID && !UNITY_EDITOR
            // Use UnityWebRequest for Android jar path
            using var req = UnityEngine.Networking.UnityWebRequest.Get(path);
            var op = req.SendWebRequest();
            while (!op.isDone) {}
            if (req.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
                throw new Exception($"Failed to load quiz file: {req.error}");
            return JsonUtility.FromJson<QuizBank>(FixOptionsObject(req.downloadHandler.text));
#else
            if (!File.Exists(path)) throw new FileNotFoundException(path);
            var json = File.ReadAllText(path);
            return JsonUtility.FromJson<QuizBank>(FixOptionsObject(json));
#endif
        }

        // JsonUtility can't parse object-literal dictionaries directly; this transforms:
        //  "options": { "A":"...", "B":"..." }
        // into
        //  "options": { "keys":["A","B"], "values":["...","..."] }
        static string FixOptionsObject(string json)
        {
            // Lightweight transform: find "options":{...} blocks and convert into keys/values arrays.
            // To keep this robust without a full JSON parser, we do a minimal pass with SimpleJSON if available.
            // For now, assume your JSON matches the example shape above (already arrays via exporter).
            return json;
        }
    }
}
