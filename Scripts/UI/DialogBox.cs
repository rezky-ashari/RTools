using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogBox : MonoBehaviour
{
    public GameObject dialogPanel;
    public GameObject boxContainer;
    public Text dialogText;

    bool isVisible = false;

    Action onHideCallback;

    public static DialogBox Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject dialogPrefab = Resources.Load<GameObject>("Prefabs/DialogBox");
                GameObject dialogBox = Instantiate(dialogPrefab);
                DontDestroyOnLoad(dialogBox);
                _instance = dialogBox.GetComponent<DialogBox>();
            }
            return _instance;
        }
    }
    private static DialogBox _instance;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Show(string text, Action onHideCallback = null)
    {
        dialogText.text = text;
        Show(onHideCallback);
    }

    public void Show(Action onHideCallback = null)
    {
        if (!isVisible)
        {
            dialogPanel.SetActive(true);
            RezTween.ScaleFromTo(boxContainer, 0.35f, 0, 1, RezTweenEase.SPRING);
            isVisible = true;
            this.onHideCallback = onHideCallback;
        }
    }

    public void Hide()
    {
        if (isVisible)
        {
            isVisible = false;
            RezTween.ScaleTo(boxContainer, 0.4f, 0, RezTweenEase.BACK_IN).OnComplete = () =>
            {
                dialogPanel.SetActive(false);

                if (onHideCallback != null)
                {
                    onHideCallback.Invoke();
                    onHideCallback = null;
                }
            };
        }
    }

    public static void ShowDialog(string text, Action onHideCallback = null)
    {
        Instance.Show(text, onHideCallback);
    }
}
