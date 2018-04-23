using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AnimationTrigger : MonoBehaviour, IPointerClickHandler {

    public string OnClick;

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
            animator.Play(OnClick);
        }
    }
}
