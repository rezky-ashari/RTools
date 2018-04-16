using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <para>Swap sprite renderer's sprite in lazy way</para>
/// Author: Rezky Ashari
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteSwitcher : MonoBehaviour {

    public int activeSprite;
    public List<Sprite> sprites = new List<Sprite>();

    SpriteRenderer spriteRenderer;
    int lastActiveSprite;

	// Use this for initialization
	void Start () {
        spriteRenderer = GetComponent<SpriteRenderer>();
        sprites.Add(spriteRenderer.sprite);
	}
	
	// Update is called once per frame
	void Update () {
		
        if (lastActiveSprite != activeSprite && activeSprite < sprites.Count)
        {
            spriteRenderer.sprite = sprites[activeSprite];
            lastActiveSprite = activeSprite;
        }
	}
}
