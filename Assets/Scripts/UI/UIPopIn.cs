using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopIn : MonoBehaviour {

    public float delay = 0;
    public float defaultScale = 1;

    private void Reset()
    {
        defaultScale = transform.localScale.x;
    }

    // Use this for initialization
    void Start () {
        transform.localScale = Vector3.one * defaultScale;
        RezTween.ScaleFrom(gameObject, 0.5f, 0, RezTweenOptions.Delay(delay), RezTweenOptions.Ease.BACK_OUT).OnComplete = ()=>
        transform.localScale = Vector3.one * defaultScale;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
