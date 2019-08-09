using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockerUI : MonoBehaviour
{
    public GameObject panel;
    public Text display;

    bool isVisible = false;

    public static BlockerUI Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject blockerPrefab = Resources.Load<GameObject>("Prefabs/BlockerUI");
                GameObject blockerUI = Instantiate(blockerPrefab);
                DontDestroyOnLoad(blockerUI);
                _instance = blockerUI.GetComponent<BlockerUI>();
            }
            return _instance;
        }
    }
    private static BlockerUI _instance;

    public void ShowBlocker(string text)
    {
        display.text = text;
        ShowBlocker();
    }

    public void ShowBlocker()
    {
        if (!isVisible)
        {
            panel.SetActive(true);
            RezTween.AlphaFromTo(panel, 0.15f, 0, 0.5f);
            isVisible = true;
        }
    }

    public void HideBlocker()
    {
        if (isVisible)
        {
            isVisible = false;
            RezTween.AlphaTo(panel, 0.15f, 0).OnComplete = () =>
            panel.SetActive(false);
        }
    }

    public static void Show(string text)
    {
        Instance.ShowBlocker(text);
    }

    public static void Show()
    {
        Instance.ShowBlocker("");
    }

    public static void Hide()
    {
        Instance.HideBlocker();
    }
}
