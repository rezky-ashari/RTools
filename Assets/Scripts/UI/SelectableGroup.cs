using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectableGroup : MonoBehaviour {

    public Image current;
    public Sprite defaultSprite;
    public Sprite selectedSprite;

    // Use this for initialization
	void Start () {
        if (current != null) current.sprite = selectedSprite;
    }

    public void SetSelected(Image gameButton)
    {
        if (current != null) current.sprite = defaultSprite;
        current = gameButton;
        current.sprite = selectedSprite;
    }

    public void SetSelected(GameButton gameButton)
    {
        SetSelected(gameButton.GetComponent<Image>());
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
