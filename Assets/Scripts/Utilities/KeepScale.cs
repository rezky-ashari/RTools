using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <para>Maintain scale of a game object even when it's parent was scaled.</para>
/// Author: Rezky Ashari
/// </summary>
public class KeepScale : MonoBehaviour {

    public Vector3 globalScale = Vector3.one;

    private void Reset()
    {
        Start();
    }

    // Use this for initialization
    void Start () {
        globalScale = transform.lossyScale;
	}
	
	// Update is called once per frame
	void Update () {
        if (globalScale != transform.lossyScale)
        {
            Vector3 diff = globalScale - transform.lossyScale;
            transform.localScale += diff;
        }
	}
}
