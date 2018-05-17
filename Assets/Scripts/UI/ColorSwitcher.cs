using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image)), ExecuteInEditMode]
public class ColorSwitcher : MonoBehaviour {

    public int current = 0;
    public List<Color> colors = new List<Color>();

    Image _image;
    Image Image
    {
        get
        {
            if (_image == null) _image = GetComponent<Image>();
            return _image;
        }
    }

    int lastActiveColor;

    private void Reset()
    {
        if (colors.Count == 0) colors.Add(Image.color);
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (lastActiveColor != current)
        {
            current = (int)Mathf.Repeat(current, colors.Count);
            Image.color = colors[current];
            lastActiveColor = current;
        }
	}

    public void SetCurrentColor(int colorIndex)
    {
        current = colorIndex;
    }
}
