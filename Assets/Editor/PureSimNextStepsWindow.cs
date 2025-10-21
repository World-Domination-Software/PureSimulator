using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PureSim.Editor
{
    /// <summary>
    /// EditorWindow for managing development tasks tracked in .copilot/next_steps.json
    /// Provides filtering, instruction copying, and stub creation capabilities.
    /// </summary>
    public class PureSimNextStepsWindow : EditorWindow
    {
        private const string TasksJsonPath = ".copilot/next_steps.json";
        
        private List<TaskData> tasks = new List<TaskData>();
        private Vector2 scrollPosition;
        private TaskStatus filterStatus = TaskStatus.All;
        private string filterType = "All";
        private string focusedTaskId = null;
        
        [MenuItem("Tools/PureSim/Next Steps...")]
        public static void ShowWindow()
        {
            var window = GetWindow<PureSimNextStepsWindow>("PureSim Next Steps");
            window.Show();
        }
        
        public static void ShowWindow(string taskId)
        {
            var window = GetWindow<PureSimNextStepsWindow>("PureSim Next Steps");
            window.focusedTaskId = taskId;
            window.Show();
        }
        
        private void OnEnable()
        {
            LoadTasks();
        }
        
        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            
            // Header
            EditorGUILayout.LabelField("PureSim Development Tasks", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Toolbar
            DrawToolbar();
            EditorGUILayout.Space();
            
            // Task List
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            DrawTaskList();
            EditorGUILayout.EndScrollView();
            
            // Footer
            DrawFooter();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            if (GUILayout.Button("Reload", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                LoadTasks();
            }
            
            GUILayout.Space(10);
            GUILayout.Label("Status:", GUILayout.Width(50));
            filterStatus = (TaskStatus)EditorGUILayout.EnumPopup(filterStatus, EditorStyles.toolbarPopup, GUILayout.Width(80));
            
            GUILayout.Space(10);
            GUILayout.Label("Type:", GUILayout.Width(40));
            string[] typeOptions = { "All", "code", "ui", "asset" };
            int typeIndex = Array.IndexOf(typeOptions, filterType);
            if (typeIndex == -1) typeIndex = 0;
            typeIndex = EditorGUILayout.Popup(typeIndex, typeOptions, EditorStyles.toolbarPopup, GUILayout.Width(80));
            filterType = typeOptions[typeIndex];
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawTaskList()
        {
            var filteredTasks = tasks.Where(t => 
                (filterStatus == TaskStatus.All || GetTaskStatus(t.status) == filterStatus) &&
                (filterType == "All" || t.type == filterType)
            ).ToList();
            
            if (filteredTasks.Count == 0)
            {
                EditorGUILayout.HelpBox("No tasks match the current filter.", MessageType.Info);
                return;
            }
            
            foreach (var task in filteredTasks)
            {
                DrawTask(task);
            }
        }
        
        private void DrawTask(TaskData task)
        {
            bool isFocused = task.id == focusedTaskId;
            
            EditorGUILayout.BeginVertical(isFocused ? "box" : EditorStyles.helpBox);
            
            // Header row
            EditorGUILayout.BeginHorizontal();
            
            // Status icon
            string statusIcon = task.status == "done" ? "✓" : "○";
            Color statusColor = task.status == "done" ? Color.green : Color.yellow;
            var oldColor = GUI.color;
            GUI.color = statusColor;
            GUILayout.Label(statusIcon, GUILayout.Width(20));
            GUI.color = oldColor;
            
            // Title
            EditorGUILayout.LabelField($"[{task.id}] {task.title}", EditorStyles.boldLabel);
            
            // Type badge
            GUILayout.Label($"[{task.type}]", EditorStyles.miniLabel, GUILayout.Width(50));
            
            EditorGUILayout.EndHorizontal();
            
            // Description
            EditorGUILayout.LabelField(task.description, EditorStyles.wordWrappedLabel);
            
            // Actions
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Copy Instructions", GUILayout.Width(130)))
            {
                CopyTaskInstructions(task);
            }
            
            if (task.status != "done" && GUILayout.Button("Mark Done", GUILayout.Width(100)))
            {
                MarkTaskDone(task);
            }
            
            if (task.type == "code" && GUILayout.Button("Create Stub Script", GUILayout.Width(130)))
            {
                CreateScriptStub(task);
            }
            
            if (task.type == "ui" && GUILayout.Button("Create Stub Prefab", GUILayout.Width(130)))
            {
                CreatePrefabStub(task);
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
        
        private void DrawFooter()
        {
            EditorGUILayout.BeginHorizontal();
            
            int openCount = tasks.Count(t => t.status != "done");
            int doneCount = tasks.Count(t => t.status == "done");
            
            EditorGUILayout.LabelField($"Tasks: {openCount} open, {doneCount} done, {tasks.Count} total", EditorStyles.miniLabel);
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void LoadTasks()
        {
            string fullPath = Path.Combine(Application.dataPath, "..", TasksJsonPath);
            
            if (!File.Exists(fullPath))
            {
                Debug.LogWarning($"Tasks file not found: {fullPath}");
                tasks = new List<TaskData>();
                return;
            }
            
            try
            {
                string json = File.ReadAllText(fullPath);
                var wrapper = JsonUtility.FromJson<TaskListWrapper>("{\"tasks\":" + json + "}");
                tasks = wrapper.tasks ?? new List<TaskData>();
                Debug.Log($"Loaded {tasks.Count} tasks from {TasksJsonPath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load tasks: {e.Message}");
                tasks = new List<TaskData>();
            }
        }
        
        private void SaveTasks()
        {
            string fullPath = Path.Combine(Application.dataPath, "..", TasksJsonPath);
            
            try
            {
                // Manual JSON serialization for better formatting
                var jsonLines = new List<string> { "[" };
                for (int i = 0; i < tasks.Count; i++)
                {
                    var task = tasks[i];
                    jsonLines.Add("  {");
                    jsonLines.Add($"    \"id\": \"{task.id}\",");
                    jsonLines.Add($"    \"title\": \"{task.title}\",");
                    jsonLines.Add($"    \"description\": \"{task.description}\",");
                    jsonLines.Add($"    \"type\": \"{task.type}\",");
                    jsonLines.Add($"    \"status\": \"{task.status}\"");
                    jsonLines.Add(i < tasks.Count - 1 ? "  }," : "  }");
                }
                jsonLines.Add("]");
                
                File.WriteAllText(fullPath, string.Join("\n", jsonLines));
                
                // Regenerate menu
                PureSimTasksMenuGenerator.GenerateMenu(tasks);
                
                Debug.Log($"Saved {tasks.Count} tasks to {TasksJsonPath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save tasks: {e.Message}");
            }
        }
        
        private void CopyTaskInstructions(TaskData task)
        {
            string instructions = $"Task: {task.title}\n\n{task.description}\n\nID: {task.id}\nType: {task.type}\nStatus: {task.status}";
            EditorGUIUtility.systemCopyBuffer = instructions;
            Debug.Log($"Copied instructions for task {task.id} to clipboard");
        }
        
        private void MarkTaskDone(TaskData task)
        {
            task.status = "done";
            SaveTasks();
            Repaint();
        }
        
        private void CreateScriptStub(TaskData task)
        {
            // Determine path based on task ID prefix
            string directory = "Assets/Scripts";
            if (task.id.StartsWith("console-"))
                directory = "Assets/Scripts/Console";
            else if (task.id.StartsWith("serial-"))
                directory = "Assets/Scripts/Serial";
            else if (task.id.StartsWith("sim-"))
                directory = "Assets/Scripts/Simulation";
            
            string fileName = task.title.Replace(" ", "") + ".cs";
            string fullPath = Path.Combine(directory, fileName);
            
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            if (File.Exists(fullPath))
            {
                EditorUtility.DisplayDialog("File Exists", $"File already exists:\n{fullPath}", "OK");
                return;
            }
            
            string stubContent = $@"using System;
using UnityEngine;

namespace PureSim
{{
    /// <summary>
    /// {task.description}
    /// </summary>
    public class {Path.GetFileNameWithoutExtension(fileName)}
    {{
        // TODO: Implement {task.title}
    }}
}}
";
            
            File.WriteAllText(fullPath, stubContent);
            AssetDatabase.Refresh();
            
            var asset = AssetDatabase.LoadAssetAtPath<MonoScript>(fullPath);
            if (asset != null)
            {
                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
            }
            
            Debug.Log($"Created script stub: {fullPath}");
        }
        
        private void CreatePrefabStub(TaskData task)
        {
            string directory = "Assets/Prefabs/UI";
            string fileName = task.title.Replace(" ", "") + ".prefab";
            string fullPath = Path.Combine(directory, fileName);
            
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            if (File.Exists(fullPath))
            {
                EditorUtility.DisplayDialog("File Exists", $"Prefab already exists:\n{fullPath}", "OK");
                return;
            }
            
            // Create a simple GameObject and save as prefab
            GameObject go = new GameObject(Path.GetFileNameWithoutExtension(fileName));
            Canvas canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            PrefabUtility.SaveAsPrefabAsset(go, fullPath);
            DestroyImmediate(go);
            AssetDatabase.Refresh();
            
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
            if (asset != null)
            {
                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
            }
            
            Debug.Log($"Created prefab stub: {fullPath}");
        }
        
        private TaskStatus GetTaskStatus(string status)
        {
            return status == "done" ? TaskStatus.Done : TaskStatus.Open;
        }
        
        private enum TaskStatus
        {
            All,
            Open,
            Done
        }
        
        [Serializable]
        private class TaskListWrapper
        {
            public List<TaskData> tasks;
        }
    }
    
    [Serializable]
    public class TaskData
    {
        public string id;
        public string title;
        public string description;
        public string type;
        public string status;
    }
}
