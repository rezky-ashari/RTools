using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <para>Link image switcher.</para>
/// </summary>
[ExecuteInEditMode]
public class SwitchLinker : MonoBehaviour {

    public ImageSwitcher source;
    public ImageSwitcher target;

    private void Reset()
    {
        source = GetComponent<ImageSwitcher>();
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (target != null && source != null)
        {
             target.ActiveImage = source.ActiveImage;
        }
	}
}
