using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DraggableUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler {

    /// <summary>
    /// Whether to disable drag for all dragable UIs.
    /// </summary>
    public static bool disableDrag = false;

    /// <summary>
    /// Current dragged gameobject.
    /// </summary>
    public static GameObject Current
    {
        get
        {
            return _current;
        }
    }
    static GameObject _current;

    public bool lockX = false;
    public bool lockY = false;

    public bool useBounds = false;
    public Rect dragBounds;
    public float boundLeft;
    public float boundRight;
    public float boundTop;
    public float boundBottom;

    public bool lockSiblingIndex = false;

    public bool backOnDrop = false;
    public Vector3 defaultPosition;

    public UnityEvent onDrag;
    public UnityEvent onDrop;

    /// <summary>
    /// Event listener when drag operation start.
    /// </summary>
    public event Action OnStartDrag;
    /// <summary>
    /// Event listener when drag operation end.
    /// </summary>
    public event Action OnEndDrag;
    /// <summary>
    /// Called when this draggable UI returned to it's default position.
    /// </summary>
    public event Action OnReturnedToDefaultPosition;

    Action onComplete;

    Canvas _parentCanvas;
    Vector3 offset;

    float planeDistance = 10f;
    float defaultZ = 0;
    int defaultSiblingIndex = 0;

    public float TopBound
    {
        get { return dragBounds.yMin; }
    }

    public float BottomBound
    {
        get { return dragBounds.yMax; }
    }

    public float RightBound
    {
        get { return dragBounds.xMin; }
    }

    public float LeftBound
    {
        get { return dragBounds.xMax; }
    }

    Canvas ParentCanvas
    {
        get
        {
            if (_parentCanvas == null)
            {
                _parentCanvas = GetComponentInParent<Canvas>();
            }
            return _parentCanvas;
        }
    }

    bool IsOverlayMode
    {
        get
        {            
            return ParentCanvas.renderMode == RenderMode.ScreenSpaceOverlay;
        }
    }

    private void Reset()
    {
        defaultPosition = transform.localPosition;
        defaultZ = transform.localPosition.z;
    }

    /// <summary>
    /// Attach a box collider to use with DropArea.
    /// </summary>
    public void AttachBoxCollider()
    {
        BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
        Rect transformRect = (transform as RectTransform).rect;
        collider.size = new Vector2(transformRect.width, transformRect.height);
    }

    // Use this for initialization
    void Start () {
        
        planeDistance = ParentCanvas.planeDistance;
	}
	
	// Update is called once per frame
	void Update () {
		if (useBounds)
        {
            if (!lockY)
            {
                /* Old way
                if (transform.localPosition.y > dragBounds.yMin)
                {
                    transform.localPosition = transform.localPosition.SetY(dragBounds.yMin);
                }
                else if (transform.localPosition.y < dragBounds.yMax)
                {
                    transform.localPosition = transform.localPosition.SetY(dragBounds.yMax);
                } */

                // My way
                if (transform.localPosition.y > boundTop)
                {
                    transform.localPosition = transform.localPosition.SetY(boundTop);
                }
                else if (transform.localPosition.y < boundBottom)
                {
                    transform.localPosition = transform.localPosition.SetY(boundBottom);
                }
            }
            if (!lockX)
            {
                /* Old way
                if (transform.localPosition.x > dragBounds.xMin)
                {
                    transform.localPosition = transform.localPosition.SetX(dragBounds.xMin);
                }
                else if (transform.localPosition.x < dragBounds.xMax)
                {
                    transform.localPosition = transform.localPosition.SetX(dragBounds.xMax);
                } */

                // My way
                if (transform.localPosition.x > boundRight)
                {
                    transform.localPosition = transform.localPosition.SetX(boundRight);
                }
                else if (transform.localPosition.x < boundLeft)
                {
                    transform.localPosition = transform.localPosition.SetX(boundLeft);
                }
            }
        }
	}

    public void OnPointerDown(PointerEventData eventData)
    {
        if (disableDrag) return;

        if (IsOverlayMode)
        {
            offset = transform.position - Input.mousePosition;
        }
        else
        {
            offset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        defaultSiblingIndex = transform.GetSiblingIndex();
        if (!lockSiblingIndex) transform.SetAsLastSibling();

        _current = gameObject;

        onDrag.Invoke();
        if (OnStartDrag != null) OnStartDrag();
    }

    public void UpdateDefaultPosition()
    {
        defaultPosition = transform.localPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (disableDrag) return;

        Vector3 target;
        if (IsOverlayMode)
        {
            target = Input.mousePosition + offset;
        }
        else
        {
            Vector3 screenPoint = Input.mousePosition;
            screenPoint.z = planeDistance;
            target = Camera.main.ScreenToWorldPoint(screenPoint) + offset;
        }
        if (lockX) target.x = transform.position.x;
        if (lockY) target.y = transform.position.y;
        transform.position = target.SetZ(defaultZ);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (disableDrag) return;

        onDrop.Invoke();
        if (OnEndDrag != null) OnEndDrag();
        if (backOnDrop) BackToDefaultPosition();
        _current = null;
    }

    public void BackToDefaultPosition(Action onComplete)
    {
        this.onComplete = onComplete;
        BackToDefaultPosition();
    }

    public void BackToDefaultPosition()
    {
        if (gameObject.activeInHierarchy)
        {
            RezTween tween = RezTween.MoveTo(gameObject, 0.5f, defaultPosition);
            tween.OnComplete = () =>
            {
                if (onComplete != null) onComplete();
                if (OnReturnedToDefaultPosition != null) OnReturnedToDefaultPosition();
                BackToDefaultSiblingIndex();
            };
        }
        else
        {
            transform.localPosition = defaultPosition;
            if (onComplete != null) onComplete();
            if (OnReturnedToDefaultPosition != null) OnReturnedToDefaultPosition();
            BackToDefaultSiblingIndex();
        }
    }

    public void BackToDefaultSiblingIndex()
    {
        transform.SetSiblingIndex(defaultSiblingIndex);
    }

    private void OnDisable()
    {
        if (_current == gameObject) _current = null;
    }

    private void OnDestroy()
    {
        OnDisable();
    }

    /// <summary>
    /// Disable drag on this UI.
    /// </summary>
    public void LockMovements()
    {
        lockX = lockY = true;
    }
}
