using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// <para>Quick handler for animation events.</para>
/// Author: Rezky Ashari
/// </summary>
public class AnimationEvent : MonoBehaviour {

    public UnityEvent onStart;
    public UnityEvent onComplete;
    
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnStartAnimation()
    {
        onStart.Invoke();
    }

    void OnCompleteAnimation()
    {
        onComplete.Invoke();        
    }
}
