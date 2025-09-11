using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingUI : MonoBehaviour
{
    public static LoadingUI Instance;

    private void Awake() {
        if(Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            animator.SetBool("Open", false);
            SceneManager.sceneLoaded += OnSceneLoadComplete;
        }
        else {
            Destroy(gameObject);
        }
    }

    public float sceneLoadDelay;
    public Animator animator;
    public CanvasGroup cg;

    private string SceneName;

    public void LoadScene(string Name)
    {
        SceneName = Name;
        animator.SetBool("Open", true);
        Invoke(nameof(DelayedSceneLoad), sceneLoadDelay);
    }

    private void DelayedSceneLoad()
    {
        SceneManager.LoadScene(SceneName);
    }

    private void OnSceneLoadComplete(Scene arg0, LoadSceneMode arg1)
    {
        //scene load complete?
        animator.SetBool("Open", false);
    }
}
