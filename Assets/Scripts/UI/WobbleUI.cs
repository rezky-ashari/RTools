using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WobbleUI : MonoBehaviour {

    public float delay = 0.1f;
    public float duration = 1f;

    float defaultScale;
    RezTween tween;

	// Use this for initialization
	void Start () {
        if (delay < 0.1f) delay = 0.1f;
        RezTween.DelayedCall(delay, StartWobble);
    }

    private void StartWobble()
    {
        defaultScale = transform.localScale.x;
        tween = RezTween.ScaleTo(gameObject, duration, defaultScale + (defaultScale * 0.05f), RezTweenOptions.Repeat(), RezTweenOptions.Yoyo());
    }

    // Update is called once per frame
    void Update () {
		
	}

    private void OnEnable()
    {
        if (tween != null)
        tween.Restart();
    }

    private void OnDisable()
    {
        if (tween != null)
        tween.Pause();
    }

    private void OnDestroy()
    {
        
    }

    public void SetDefaultScale(float scale)
    {
        transform.localScale = new Vector3(scale, scale, scale);
        bool inPauseState = (tween != null && tween.paused);
        StartWobble();
        tween.paused = inPauseState;
    }
}
