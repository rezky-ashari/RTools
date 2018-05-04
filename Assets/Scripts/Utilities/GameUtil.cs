﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// <para>Collection of utility methods.</para>
/// Author: Rezky Ashari
/// </summary>
public class GameUtil {

    /// <summary>
    /// Destroy a gameObject. Use destroy immediate on editor and normal destroy when playing.
    /// Fail silently if the gameObject is null. 
    /// </summary>
    /// <param name="gameObject"></param>
	public static void Destroy(GameObject gameObject)
    {
        if (gameObject == null) return;
        if (Application.isPlaying)
        {
            GameObject.Destroy(gameObject);
        }
        else
        {
            GameObject.DestroyImmediate(gameObject);
        }
    }

    /// <summary>
    /// Destroy Children of a game object.
    /// </summary>
    /// <param name="gameObject">Parent gameObject</param>
    public static void DestroyChildren(GameObject gameObject)
    {
        DestroyChildren(gameObject.transform);
    }

    /// <summary>
    /// Destroy children of a game object.
    /// </summary>
    /// <param name="transform"></param>
    public static void DestroyChildren(Transform transform)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// Quick way to set local position properties.
    /// </summary>
    /// <param name="transform">Target transform</param>
    /// <param name="properties">Vector3 value in string format, example: 'x:10', 'y:-2', 'z:100'</param>
    public static void SetLocalPosition(Transform transform, params string[] properties)
    {
        Vector3 position = transform.localPosition;
        foreach (string prop in properties)
        {
            string[] propData = prop.Split(':');
            float value = float.Parse(propData[1]);
            switch (propData[0])
            {
                case "x":
                    position.x = value;
                    break;
                case "y":
                    position.y = value;
                    break;
                case "z":
                    position.z = value;
                    break;
            }
        }
        transform.localPosition = position;
    }

    /// <summary>
    /// Quick way to set local position properties.
    /// </summary>
    /// <param name="gameObject">Target gameObject</param>
    /// <param name="properties">Vector3 property and value in string format, example: 'x:10', 'y:-2', 'z:100'</param>
    public static void SetLocalPosition(GameObject gameObject, params string[] properties)
    {
        SetLocalPosition(gameObject.transform, properties);
    }

    /// <summary>
    /// Set image alpha. 
    /// </summary>
    /// <param name="image">Target image</param>
    /// <param name="alpha">Desired alpha</param>
    public static void SetImageAlpha(Image image, float alpha)
    {
        Color imageColor = image.color;
        imageColor.a = alpha;
        image.color = imageColor;
    }

    public static GameObject ExpandHitArea(GameObject gameObject, float expandValue = 50)
    {
        GameObject hitArea = gameObject.GetChild("GameUtil HitArea");

        if (hitArea == null)
        {
            hitArea = new GameObject("GameUtil HitArea");
            Image image = hitArea.AddComponent<Image>();
            image.sprite = gameObject.GetComponent<Image>().sprite;
            image.SetNativeSize();
            SetImageAlpha(image, 0);
            Rect rect = image.rectTransform.rect;
            image.rectTransform.sizeDelta = new Vector2(rect.width + expandValue, rect.height + expandValue);
            hitArea.SetParent(gameObject, false);
        }
        

        return hitArea;
    }

    /// <summary>
    /// Get a random value from the given values.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    /// <param name="values">Values to randomize.</param>
    /// <returns></returns>
    public static T GetRandomFrom<T>(params T[] values)
    {
        return values[UnityEngine.Random.Range(0, values.Length)];
    }
}
