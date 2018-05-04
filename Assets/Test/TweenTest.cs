using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TweenTest : MonoBehaviour {

    public GameObject testObject;
    public GameObject sprite;
    public float duration = 0.5f;
    public int repeat = 0;
    public float repeatDelay = 0;
    public bool yoyo = false;

	// Use this for initialization
	void Start () {
        if (testObject == null) return;

        //RezTween.ScaleFrom(testObject, 0.5f, 0);

        RezTween.ColorTo(testObject.GetComponent<Image>(), Color.red, 10f);
    }
	
	// Update is called once per frame
	void Update () {
        if (testObject != null)
        {
            // Test move tween
            if (Input.GetKeyDown(KeyCode.Space))
            {
                testObject.transform.localPosition = Vector3.zero;
                RezTween.MoveTo(testObject, duration, "x:100", RezTweenEase.BACK_OUT, RezTweenOptions.Repeat(repeat), RezTweenOptions.RepeatDelay(repeatDelay), RezTweenOptions.Yoyo(yoyo)).OnComplete = () =>
                {
                    Debug.Log("Complete position tween!");
                    PlayRandomSFX();
                };
            }
            // Test alpha (with base) tween 
            if (Input.GetKeyDown(KeyCode.A))
            {
                CanvasGroup cg = testObject.GetComponent<CanvasGroup>();
                if (cg == null) cg = testObject.AddComponent<CanvasGroup>();

                cg.alpha = 1;
                RezTween.To(cg, duration, "alpha:0", RezTweenOptions.Repeat(repeat), RezTweenOptions.RepeatDelay(repeatDelay), RezTweenOptions.Yoyo(yoyo)).OnComplete = () =>
                {
                    Debug.Log("Complete alpha tween!");

                };
            }
            // Test scale tween
            if (Input.GetKeyDown(KeyCode.S))
            {
                testObject.transform.localScale = Vector3.one;
                RezTween.ScaleTo(testObject, duration, 1.5f, RezTweenEase.BACK_OUT, RezTweenOptions.Repeat(repeat), RezTweenOptions.RepeatDelay(repeatDelay), RezTweenOptions.Yoyo(yoyo));
            }
        }
        
        if (sprite != null && Input.GetKeyDown(KeyCode.N))
        {
            RezTween.MoveTo(sprite, 0.5f, "x:5");
        }
	}

    private void PlayRandomSFX()
    {
        System.Random rand = new System.Random();
        string[] availableSFX = new string[] { "Click", "Clear", "Cring"  };
        Resound.PlaySFX(availableSFX[rand.Next(availableSFX.Length)]);
    }
}