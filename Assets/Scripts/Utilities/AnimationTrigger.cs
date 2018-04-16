using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// <para>Play an animation when clicked.</para>
/// Author: Rezky Ashari
/// </summary>
public class AnimationTrigger : MonoBehaviour, IPointerClickHandler {

    [Tooltip("Animation state name to call when click event occurred")]
    public string onClick;

    private Animator animator;

    // Use this for initialization
    void Start () {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component not found on " + gameObject.name);
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnPointerClick(PointerEventData eventData)
    {
        if (animator != null)
        {
            animator.Play(onClick);
        }
    }
}
