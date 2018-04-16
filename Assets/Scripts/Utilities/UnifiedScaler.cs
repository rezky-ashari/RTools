using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <para>Set game object scale in lazy way.</para>
/// Author: Rezky - Ikaan Studio
/// </summary>
[ExecuteInEditMode]
public class UnifiedScaler : MonoBehaviour {

    [SerializeField]
    private float scale;

    float lastScale;

    /// <summary>
    /// Scale value of x, y, z for this game object.
    /// </summary>
    public float Scale
    {
        get
        {
            return scale;
        }
        set
        {
            scale = value;
        }
    }

	// Use this for initialization
	void Start () {
        scale = gameObject.transform.localScale.x;
        lastScale = scale;
	}
	
	// Update is called once per frame
	void Update () {
		if (scale != lastScale)
        {
            gameObject.transform.ScaleTo(scale);
            lastScale = scale;
        }
	}
}
