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

    [Tooltip("Prevent interactable state to change the image's alpha.")]
    public bool lockAlpha = false;

    [Tooltip("Whether to ignore click on transparent area of this button. You have to check the 'Read/Write Enabled' in image's Import Settings.")]
    public bool ignoreTransparentArea = false;

    [Tooltip("Ignore click when vertically dragged.")]
    public bool ignoreYDrag = false;

    [Tooltip("Ignore click when horizontally dragged.")]
    public bool ignoreXDrag = false;

    [Tooltip("Wheter to scale game button to give a 'pressed' effect.")]
    public bool scaleOnDown = true;
    public GameObject scaleTarget;

    [Tooltip("For group of buttons that required only one button to be selected. Attach 'SelectableGroup' component to SceneManager to get started.")]
    public SelectableGroup selectableGroup;

    [SerializeField]
    public OnClickHandler onClick;

    float defaultScale = 1;
    float currentAlpha = 1;
    float lastAlpha = 0;

    bool receiveRaycast = true;

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
        GameObject buttonObject = CreateGameButton("GameButton");

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

    /// <summary>
    /// Whether this button receives raycast.
    /// </summary>
    public bool RaycastTarget
    {
        get
        {
            return receiveRaycast;
        }
        set
        {
            receiveRaycast = value;
            ProcessImages(image =>
            {
                image.raycastTarget = receiveRaycast;
            });
        }
    }

    public static GameObject CreateGameButton(string name, GameObject parent = null)
    {
        GameObject buttonObject = new GameObject(name);
        buttonObject.AddComponent<RectTransform>();
        buttonObject.AddComponent<Image>();
        buttonObject.AddComponent<GameButton>();

        if (parent != null) buttonObject.SetParent(parent, false);

        return buttonObject;
    }

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
            UpdateAlpha();
        }
	}

    private void UpdateAlpha()
    {
        if (!lockAlpha)
        {
            currentAlpha = interactable ? 1f : 0.5f;
            if (currentAlpha != lastAlpha)
            {
                ProcessImages(image =>
                {
                    Color currentColor = image.color;
                    image.color = new Color(currentColor.r, currentColor.g, currentColor.b, currentAlpha);
                });
                lastAlpha = currentAlpha;
            }
        }       
    }

    /// <summary>
    /// Do operation for all button images.
    /// </summary>
    /// <param name="handler"></param>
    void ProcessImages(Action<Image> handler)
    {
        Image[] images = GetComponentsInChildren<Image>();
        for (int i = 0; i < images.Length; i++)
        {
            handler(images[i]);
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
        UpdateAlpha();
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
        if (!interactable) return;
        if (scaleOnDown && scaleTarget != null) scaleTarget.transform.ScaleTo(defaultScale);        

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