#define UseResound
// Comment the line above if you don't use Resound library.

using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif

[ExecuteInEditMode]
public class GameButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {

    [Serializable]
    public class OnClickHandler : UnityEvent<string> {};

    public string ID;
    public bool interactable = true;

    [Tooltip("Whether to ignore click on transparent area of this button. You have to check the 'Read/Write Enabled' in image's Import Settings.")]
    public bool ignoreTransparentArea = false;

    [Tooltip("Ignore Y drag in vertical scroll list")]
    public bool ignoreYDrag = false;

    public bool ignoreXDrag = false;

    public bool scaleOnDown = true;
    public GameObject scaleTarget;
    public SelectableGroup selectableGroup;

    [SerializeField]
    public OnClickHandler onClick;

    float defaultScale = 1;
    float currentAlpha = 1;
    float lastAlpha = 0;

    const float dragThreshold = 25;
    bool listenToMouseUp = false;
    float startY;
    float startX;

    #region Context/Menu.
#if UNITY_EDITOR
    private static double renameTime;

    [MenuItem("GameObject/UI/Game Button", false, 11)]
    static void AddGameButton()
    {
        GameObject buttonObject = new GameObject("GameButton");
        buttonObject.AddComponent<RectTransform>();
        buttonObject.AddComponent<Image>();
        buttonObject.AddComponent<GameButton>();

        if (Selection.activeGameObject != null)
        {
            buttonObject.transform.SetParent(Selection.activeGameObject.transform, false);
        }
        else
        {
            Canvas parentCanvas = FindObjectOfType<Canvas>();
            if (parentCanvas != null)
            {
                buttonObject.transform.SetParent(parentCanvas.transform, false);
            }
        }
        Selection.activeGameObject = buttonObject;
        renameTime = EditorApplication.timeSinceStartup + 0.2d;
        EditorApplication.update += EngageRenameMode;
    }

    private static void EngageRenameMode()
    {
        if (EditorApplication.timeSinceStartup >= renameTime)
        {
            EditorApplication.update -= EngageRenameMode;
            var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
            var hierarchyWindow = EditorWindow.GetWindow(type);
            var rename = type.GetMethod("RenameGO", BindingFlags.Instance | BindingFlags.NonPublic);
            rename.Invoke(hierarchyWindow, null);
        }
    }
#endif
    #endregion

    void Reset()
    {
        ID = gameObject.name;
        scaleTarget = gameObject;
    }

    // Use this for initialization
    void Start () {
        if (Application.isPlaying)
        {
            if (ignoreTransparentArea)
            {
                GetComponent<Image>().alphaHitTestMinimumThreshold = 0.01f;
            }
        }
        else
        {
            UpdateInteractableState();
        }
	}

    private void UpdateAlpha()
    {
        currentAlpha = interactable ? 1f : 0.5f;
        if (currentAlpha != lastAlpha)
        {
            Image[] images = GetComponentsInChildren<Image>();
            for (int i = 0; i < images.Length; i++)
            {
                Image imageComponent = images[i];
                Color currentColor = imageComponent.color;
                imageComponent.color = new Color(currentColor.r, currentColor.g, currentColor.b, currentAlpha);
            }
            lastAlpha = currentAlpha;
        }        
    }

    // Update is called once per frame
    void Update () {
        if (Application.isPlaying)
        {
            if (listenToMouseUp && Input.GetMouseButtonUp(0))
            {
                Vector3 mousePosition = Input.mousePosition;
                if (ignoreYDrag)
                {
                    float distanceY = Math.Abs(startY - mousePosition.y);
                    if (distanceY <= dragThreshold) ExecuteClick();
                }
                if (ignoreXDrag)
                {
                    float distanceX = Math.Abs(startX - mousePosition.x);
                    if (distanceX <= dragThreshold) ExecuteClick();
                }

                listenToMouseUp = false;
            }
        }
        else
        {
            UpdateInteractableState();
        }
	}

    void UpdateInteractableState()
    {
        if (interactable)
        {
            if (currentAlpha != 1)
            {
                UpdateAlpha();
            }
        }
        else
        {
            if (currentAlpha != 0)
            {
                UpdateAlpha();
            }
        }
    }

    /// <summary>
    /// Send click event of this button.
    /// </summary>
    public void SendClickToScene()
    {
        if (sceneListener != null) sceneListener(ID);
        onClick.Invoke(ID);

#if UseResound
        Resound.PlaySFX("click");
#endif
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!interactable) return;

        if (scaleOnDown && scaleTarget != null)
        {
            defaultScale = scaleTarget.transform.localScale.x;
            //scaleTarget.transform.localScale = Vector3.one * (defaultScale * 0.95f);
            ScaleTo(defaultScale * 0.95f);
        }
    }

    void ScaleTo(float scale)
    {
        scaleTarget.transform.localScale = Vector3.one * scale;
    }

    void ExecuteClick()
    {
        if (selectableGroup != null) selectableGroup.SetSelected(this);
        SendClickToScene();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (scaleOnDown && scaleTarget != null) scaleTarget.transform.ScaleTo(defaultScale);
        if (!interactable) return;

        listenToMouseUp = ignoreYDrag || ignoreXDrag;
        if (listenToMouseUp)
        {
            startY = Input.mousePosition.y;
            startX = Input.mousePosition.x;
        }
        else
        {
            ExecuteClick();            
        }
    }

    #region Do not edit. For Scene Handler.
    static Action<string> sceneListener;
    public static void SetSceneListener(Action<string> listener)
    {
       sceneListener = listener;
    }
#endregion
}