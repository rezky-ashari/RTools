#define UseResound
// Comment the line above if you don't use Resound library.

using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {

    [Serializable]
    public class OnClickHandler : UnityEvent<string> {};

    public string ID;
    public bool interactable = true;

    [Tooltip("Whether to ignore click on transparent area of this button. You have to check the 'Read/Write Enabled' in image's Import Settings.")]
    public bool ignoreTransparentArea = false;
    [Tooltip("Ignore Y drag in vertical scroll list")]
    public bool ignoreYDrag = false;
    public bool scaleOnDown = true;
    public GameObject scaleTarget;
    public SelectableGroup selectableGroup;

    [SerializeField]
    public OnClickHandler onClick;

    float defaultScale = 1;
    float currentAlpha = 1;

    const float dragThreshold = 25;
    bool listenToMouseUp = false;
    float startY;

    void Reset()
    {
        ID = gameObject.name;
        scaleTarget = gameObject;
    }

    // Use this for initialization
    void Start () {
        if (ignoreTransparentArea)
        {
            GetComponent<Image>().alphaHitTestMinimumThreshold = 0.01f;
        }        
	}

    private void UpdateAlpha()
    {
        currentAlpha = interactable ? 1f : 0.5f;
        Image imageComponent = GetComponent<Image>();
        Color currentColor = imageComponent.color;
        imageComponent.color = new Color(currentColor.r, currentColor.g, currentColor.b, currentAlpha);
    }

    // Update is called once per frame
    void Update () {
		if (listenToMouseUp && Input.GetMouseButtonUp(0))
        {
            float distance = Math.Abs(startY - Input.mousePosition.y);
            //Debug.Log("Y distance " + distance);
            if (distance <= 25)
            {
                ExecuteClick();
            }
            listenToMouseUp = false;
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

        if (ignoreYDrag)
        {
            listenToMouseUp = true;
            startY = Input.mousePosition.y;
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