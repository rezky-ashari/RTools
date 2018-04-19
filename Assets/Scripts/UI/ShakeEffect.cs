using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeEffect : MonoBehaviour {

    public float shakeAmount = 0.7f;
    public float decreaseFactor = 1.0f;
    public float shakeDuration = 0.5f;

    float shake = 0f;
    bool isShaking = false;
    Vector3 originalPos;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (isShaking)
        {
            if (shake > 0)
            {
                transform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount;
                shake -= Time.fixedDeltaTime * decreaseFactor;
            }
            else
            {
                shake = 0f;
                transform.localPosition = originalPos;
                isShaking = false;
            }
        }
    }

    /// <summary>
    /// Shake this object.
    /// </summary>
    /// <param name="duration">Shake duration in seconds</param>
    public void Shake(float duration)
    {
        originalPos = transform.localPosition;
        shake = duration;
        isShaking = true;
    }

    /// <summary>
    /// Shake this object.
    /// </summary>
    public void Shake()
    {
        Shake(shakeDuration);
    }
}
