using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollingImage : MonoBehaviour {

    public float speed = 0.5f;
    public float offset = 0f;
    public Transform clone;

    float lastOffset = 0;

    Image _image;
    public Image Image {
        get
        {
            if (_image == null) _image = GetComponent<Image>();
            return _image;
        }
    }

    // Use this for initialization
    void Start ()
    {
        UpdateOffset();
    }

    private void UpdateOffset()
    {
        float imageWidth = transform.ToRectTransform().rect.width + offset;
        if (speed > 0)
        {
            imageWidth *= -1;
        }
        clone.localPosition = clone.localPosition.SetX(imageWidth);
        lastOffset = offset;
    }

    // Update is called once per frame
    void Update () {
        Vector3 targetPosition = transform.localPosition + new Vector3(speed, 0);
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime);
        if (speed > 0)
        {
            if (clone.position.x >= 0)
            {
                transform.localPosition = transform.localPosition.SetX(0);
            }
        }
        else
        {
            if (clone.position.x <= 0)
            {
                transform.localPosition = transform.localPosition.SetX(0);
            }
        }
        if (offset != lastOffset)
        {
            UpdateOffset();
        }
    }
}
