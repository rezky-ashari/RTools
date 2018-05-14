using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// <para>Tint one or more Image component in lazy way.</para>
/// Author: Rezky Ashari
/// </summary>
[ExecuteInEditMode]
public class ImageTinter : MonoBehaviour {

    [SerializeField]
    Color tintColor = Color.white;

    [SerializeField]
    List<Image> imageList = new List<Image>();

    Color lastTintColor = Color.white;

    public Color color
    {
        set
        {
            tintColor = value;
        }
        get
        {
            return tintColor;
        }
    }

    private void Reset()
    {
        AddImage(GetComponent<Image>());
    }

    // Use this for initialization
    void Start () {
		 
	}
	
	// Update is called once per frame
	void Update () {
		if (tintColor != lastTintColor)
        {
            for (int i = imageList.Count - 1; i >= 0; i--)
            {
                if (imageList[i] == null)
                {
                    imageList.RemoveAt(i);
                }
                else
                {
                    imageList[i].color = tintColor;
                }                
            }
            lastTintColor = tintColor;
        }
	}

    /// <summary>
    /// Add an image to tint.
    /// </summary>
    /// <param name="image">Image to tint.</param>
    public void AddImage(Image image)
    {
        if (image != null)
        {
            imageList.AddIfNotExists(image);
        }        
    }

    /// <summary>
    /// Add an image to tint.
    /// </summary>
    /// <param name="gameObject">Game Object (that contains an Image component) to tint.</param>
    public void AddImage(GameObject gameObject)
    {
        AddImage(gameObject.GetComponent<Image>());
    }

    /// <summary>
    /// Add images to tint.
    /// </summary>
    /// <param name="images">Images to tint.</param>
    public void AddImages(Image[] images)
    {
        for (int i = 0; i < images.Length; i++)
        {
            AddImage(images[i]);
        }
    }

    /// <summary>
    /// Add images (inside a game object) to tint.
    /// </summary>
    /// <param name="gameObject">Game object that contains one or more Image components.</param>
    public void AddImages(GameObject gameObject)
    {
        AddImages(gameObject.GetComponentsInChildren<Image>());
    }
}
