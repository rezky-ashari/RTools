﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// <para>Handy class for changing scene asynchronously and showing Ads.</para>
/// Author: Rezky Ashari
/// </summary>
public class SceneSwitchR {

    /// <summary>
    /// Transition duration in seconds.
    /// </summary>
    const float transitionDuration = 0.5f;

    /// <summary>
    /// Whether to show interstitial ad after load.
    /// </summary>
    static bool showAdAfterLoad = false;

    /// <summary>
    /// Whether transition is currently running.
    /// </summary>
    public static bool IsOnTransition { get; private set; }

    /// <summary>
    /// CanvasGroup of a transition overlay to dispose.
    /// </summary>
    static CanvasGroup transitionOverlay;

    /// <summary>
    /// Called before changing scene. Automatically will be set to null after called once.
    /// </summary>
    public static Action OnBeforeChangingScene;

    /// <summary>
    /// Called after changing scene. Automatically will be set to null after called once.
    /// </summary>
    public static Action OnAfterChangingScene;

    /// <summary>
    /// Called when scene changed.
    /// </summary>
    public static event Action OnSceneChanged;

    /// <summary>
    /// Called before destroying the transition overlay.
    /// </summary>
    public static event Action OnDestroyingOverlay;

    /// <summary>
    /// Create a dark fader from the resource prefab.
    /// Dark fader must be a game object that contains a CanvasGroup element.
    /// </summary>
    /// <param name="initialAlpha"></param>
    /// <returns></returns>
    static CanvasGroup CreateTransitionOverlay(float initialAlpha = 0)
    {
        GameObject overlayPrefab = Resources.Load<GameObject>("Prefabs/TransitionCanvas");
        if (overlayPrefab == null)
        {
            overlayPrefab = Resources.Load<GameObject>("Prefabs/DarkFader");
        }
        CanvasGroup cg = UnityEngine.Object.Instantiate(overlayPrefab).GetComponent<CanvasGroup>();
        cg.alpha = initialAlpha;
        UnityEngine.Object.DontDestroyOnLoad(cg.gameObject);
        return cg;
    }

    /// <summary>
    /// Name of the current active scene.
    /// </summary>
    public static string CurrentSceneName
    {
        get {
            if (currentSceneName == "") currentSceneName = SceneManager.GetActiveScene().name;
            return currentSceneName;
        }
    }
    static string currentSceneName = "";

    /// <summary>
    /// Switch scene to a new scene.
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="showAd"></param>
	public static void To(object sceneName, bool showAd = false)
    {
        if (!IsOnTransition)
        {
            ExecuteOnce(ref OnBeforeChangingScene);
            IsOnTransition = true;
            showAdAfterLoad = showAd;
            transitionOverlay = CreateTransitionOverlay();
            RezTween.To(transitionOverlay, transitionDuration / 2, "alpha:1").OnComplete = () =>
            RezTween.StartCoroutine(LoadScene(sceneName));
        }        
    }

    /// <summary>
    /// Go to the next scene based on build index.
    /// </summary>
    /// <param name="showAd">Whether to show ad on transition.</param>
    public static void ToNextScene(bool showAd = false)
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            nextSceneIndex = 0;
        }
        To(nextSceneIndex, showAd);
    }

    /// <summary>
    /// Load scene async.
    /// </summary>
    /// <param name="sceneName">Target scene name</param>
    /// <returns></returns>
    static IEnumerator LoadScene(object sceneName)
    {
        if (sceneName is string)
        {
            currentSceneName = (string)sceneName;
            yield return SceneManager.LoadSceneAsync(currentSceneName);            
        }
        else
        {
            currentSceneName = "";
            yield return SceneManager.LoadSceneAsync((int)sceneName);
        }

        ExecuteOnce(ref OnAfterChangingScene);
        OnSceneChanged?.Invoke();

        if (showAdAfterLoad)
        {
            RezTween.DelayedCall(0.1f, () =>
            {
                DestroyLoadingImage();
                FadeOutOverlay();
            });
            showAdAfterLoad = false;
        }
        else
        {
            FadeOutOverlay();
        }
    }

    /// <summary>
    /// Destroy the loading image. Leave the overlay.
    /// </summary>
    private static void DestroyLoadingImage()
    {
        if (transitionOverlay != null)
        {
            Transform secondChild = transitionOverlay.transform.GetChild(1);
            if (secondChild != null) secondChild.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Destroy the overlay.
    /// </summary>
    static void DestroyOverlay()
    {
        if (transitionOverlay != null)
        {
            IsOnTransition = false;
            ExecuteOnce(ref OnDestroyingOverlay);

            UnityEngine.Object.Destroy(transitionOverlay.gameObject);
            transitionOverlay = null;
        }
    }

    /// <summary>
    /// Make overlay fade out.
    /// </summary>
    static void FadeOutOverlay()
    {
        if (transitionOverlay == null) return;
        RezTween.To(transitionOverlay, transitionDuration / 2, "alpha:0").OnComplete = () =>
        DestroyOverlay();
    }

    /// <summary>
    /// Execute a listener then sets it's value to null.
    /// </summary>
    /// <param name="action"></param>
    static void ExecuteOnce(ref Action action)
    {
        action?.Invoke();
        action = null;
    }

    /// <summary>
    /// Execute a method after scene transition.
    /// </summary>
    /// <param name="action"></param>
    public static void ExecuteAfterTransition(Action action)
    {
        if (IsOnTransition)
        {
            OnDestroyingOverlay += action;
        }
        else
        {
            action();
        }
    }
}
