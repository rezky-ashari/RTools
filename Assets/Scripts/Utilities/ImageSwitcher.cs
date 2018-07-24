using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// <para>Swap image's sprite in lazy way.</para>
/// Author: Rezky Ashari
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(Image))]
public class ImageSwitcher : MonoBehaviour {

    [SerializeField]
    int activeImage = 0;

    public bool useNativeSize = false;
    public bool preserveAspect = true;
    [Tooltip("Whether to link to Transporter component")]
    public bool linkToTransporter = false;
    public List<Sprite> images = new List<Sprite>();

    int lastActiveImage;

    /// <summary>
    /// Active sprite.
    /// </summary>
    public Sprite CurrentSprite
    {
        get
        {
            return images[activeImage];
        }
    }

    /// <summary>
    /// Index of the current active image.
    /// </summary>
    public int ActiveImage
    {
        get { return activeImage; }
        set
        {
            activeImage = (int)Mathf.Repeat(value, images.Count);
            Update();
        }
    }

    /// <summary>
    /// Transporter instance.
    /// </summary>
    Transporter PositionManager
    {
        get
        {
            if (_transporter == null)
            {
                _transporter = GetComponent<Transporter>();
                if (_transporter == null) _transporter = gameObject.AddComponent<Transporter>();                
            }
            _transporter.PreventInspectorChanges(GetType().Name);
            return _transporter;
        }
    }
    Transporter _transporter;

    Image ImageComponent
    {
        get
        {
            if (_imageComponent == null) _imageComponent = GetComponent<Image>();
            return _imageComponent;
        }
    }
    Image _imageComponent;


    // Use this for initialization
    void Start () {        
        if (images.Count == 0) images.Add(ImageComponent.sprite);
    }
	
	// Update is called once per frame
	void Update () {

        if (images.Count == 0) images.Add(ImageComponent.sprite);
        if (lastActiveImage != activeImage)
        {
            activeImage = Mathf.Clamp(activeImage, 0, images.Count - 1);
            ImageComponent.sprite = images[activeImage];
            if (ImageComponent.sprite == null)
            {
                // hide the white box
                ImageComponent.enabled = false;
            }
            else
            {
                ImageComponent.preserveAspect = preserveAspect;
                if (useNativeSize) ImageComponent.SetNativeSize();
                ImageComponent.enabled = true;
            }
            lastActiveImage = activeImage;
        }
        if (linkToTransporter) SyncPosition(activeImage);
        else if (GetComponent<Transporter>() != null) PositionManager.AllowInspectorChanges();
    }

    // Sync with Transporter
    void SyncPosition(int index)
    {
        if (PositionManager.positions.Count > index && PositionManager.currentPosition != index) PositionManager.MoveTo(index, false);
        else
        {
            while (PositionManager.positions.Count - 1 < index)
            {
                PositionManager.AddCurrentPosition();
            }
            if (PositionManager.positions.Count > images.Count)
            {
                PositionManager.positions.RemoveRange(images.Count, PositionManager.positions.Count - images.Count);
            }
        }
    }

    public void Randomize()
    {
        activeImage = UnityEngine.Random.Range(0, images.Count - 1);
    }

    /// <summary>
    /// Set a sprite as the activeImage.
    /// </summary>
    /// <param name="name">Sprite name.</param>
    public void Show(string name)
    {
        activeImage = Array.FindIndex(images.ToArray(), x => x.name == name);
    }

    /// <summary>
    /// Set a sprite as the activeImage.
    /// </summary>
    /// <param name="index">Sprite index.</param>
    public void Show(int index)
    {
        activeImage = index;
    }

    void OnDestroy()
    {
        if (_transporter != null) PositionManager.AllowInspectorChanges();
    }
}
