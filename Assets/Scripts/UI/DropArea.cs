using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class DropArea : MonoBehaviour {

    /// <summary>
    /// Called when a game object with collider entered this drop area.
    /// </summary>
    public Action<GameObject> OnEnter;

    /// <summary>
    /// Called when a game object with collider leave this drop area.
    /// </summary>
    public Action<GameObject> OnLeave;

    [SerializeField, Tooltip("A draggable UI which currently in drop area")]
    private GameObject currentGameObject;

    private void Reset()
    {
        GetComponent<Rigidbody2D>().isKinematic = true;
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        Rect transformRect = (transform as RectTransform).rect;
        collider.isTrigger = true;
        collider.size = new Vector2(transformRect.width, transformRect.height);
    }

    /// <summary>
    /// A draggable UI which currently in drop area. 
    /// </summary>
    public GameObject CurrentGameObject
    {
        get { return currentGameObject; }
    }

	// Use this for initialization
	void Start () {
        currentGameObject = null;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (OnEnter != null) OnEnter(collision.gameObject);
        currentGameObject = collision.gameObject;
        
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject == currentGameObject)
        currentGameObject = null;

        if (OnLeave != null) OnLeave(collision.gameObject);
    }
}
