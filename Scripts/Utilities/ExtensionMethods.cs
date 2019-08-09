using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

public static class ExtensionMethods
{
    /// <summary>
    /// Get a component in a child.
    /// </summary>
    /// <typeparam name="T">Component Type</typeparam>
    /// <param name="parent">Parent game object</param>
    /// <param name="childName">Child name</param>
    /// <returns></returns>
    public static T GetComponentInChild<T>(this GameObject parent, string childName)
    {
        try
        {
            Transform child = parent.transform.Find(childName);
            if (child != null)
            {
                return child.gameObject.GetComponent<T>();
            }          
        }
        catch (Exception)
        {
            Debug.Log("Can't find component " + typeof(T).GetType().FullName + " in child " + childName + " of " + parent.name);            
        }
        return default(T);
    }

    /// <summary>
    /// Get a component from a game object. If not found, add it.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="target"></param>
    /// <returns></returns>
    public static T GetOrAddComponent<T>(this GameObject target) where T : MonoBehaviour
    {
        T component = target.GetComponent<T>();
        if (component == null) component = target.AddComponent<T>();
        return component;
    }

    /// <summary>
    /// Get a component in a child.
    /// </summary>
    /// <typeparam name="T">Component Type</typeparam>
    /// <param name="parent">Parent game object</param>
    /// <param name="childName">Child name</param>
    /// <returns></returns>
    public static T GetComponentInChild<T>(this GameObject parent, int childIndex)
    {
        try
        {
            Transform child = parent.transform.GetChild(childIndex);
            if (child != null)
            {
                return child.gameObject.GetComponent<T>();
            }
        }
        catch (Exception)
        {
            Debug.Log("Can't find component " + typeof(T).GetType().FullName + " in child #" + childIndex + " of " + parent.name);
        }
        return default(T);
    }

    /// <summary>
    /// Get a game object's child.
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="childName">Child name to get</param>
    /// <returns></returns>
    public static GameObject GetChild(this GameObject gameObject, string childName)
    {
        try
        {
            return gameObject.transform.Find(childName).gameObject;
        }
        catch
        {
            Debug.Log("Can't find child with name " + childName);
        }
        return null;
    }

    /// <summary>
    /// Get a game object's child.
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="childName">Child index to get</param>
    /// <returns></returns>
    public static GameObject GetChild(this GameObject gameObject, int childIndex)
    {
        if (childIndex < 0 || childIndex >= gameObject.transform.childCount) return null;
        return gameObject.transform.GetChild(childIndex).gameObject;
    }

    /// <summary>
    /// Remove all children from this game object.
    /// </summary>
    /// <param name="gameObject"></param>
    public static GameObject RemoveChildren(this GameObject gameObject)
    {
        Transform transform = gameObject.transform;
        for (int i = 0; i < transform.childCount; i++)
        {
            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(transform.GetChild(i).gameObject);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }
        return gameObject;
    }

    /// <summary>
    /// Scale this transform by a value.
    /// </summary>
    /// <param name="transform">Transform to scale</param>
    /// <param name="scale">Scale value (will be assigned as the x, y, and z scale value</param>
    public static void ScaleTo(this Transform transform, float scale)
    {
        transform.localScale = new Vector3(scale, scale, scale);
    }

    /// <summary>
    /// Convert an UI's transform to RectTransform.
    /// </summary>
    public static RectTransform ToRectTransform(this Transform transform)
    {
        return (transform as RectTransform);
    }

    /// <summary>
    /// Set the parent of this game object.
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="parent">Parent game object</param>
    /// <param name="worldPositionStays"></param>
    public static void SetParent(this GameObject gameObject, GameObject parent, bool worldPositionStays = true)
    {
        gameObject.transform.SetParent(parent.transform, worldPositionStays);
    }

    /// <summary>
    /// Set the parent of this game object.
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="parent">Parent game object</param>
    /// <param name="worldPositionStays"></param>
    public static void SetParent(this GameObject gameObject, Transform parent, bool worldPositionStays = true)
    {
        gameObject.transform.SetParent(parent, worldPositionStays);
    }

    /// <summary>
    /// Add a gameobject as a child.
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="child"></param>
    public static void AddChild(this GameObject parent, GameObject child, bool worldPositionStays = true)
    {
        GameObject gameObject = UnityEngine.Object.Instantiate(child);
        gameObject.transform.SetParent(parent.transform, worldPositionStays);
    }

    /// <summary>
    /// Get the calculated rectangle in the local space of the game object's transform.<para />
    /// Please note that this game object must be an UI game object that contains the RectTransform component.
    /// </summary>
    public static Rect GetRect(this GameObject gameobject)
    {
        return gameobject.transform.ToRectTransform().rect;
    }

    /// <summary>
    /// Convert this string to an enum type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static T ToEnum<T>(this string value)
    {
        if (value == "") throw new Exception("Can't convert empty string to Enum!");
        return (T)Enum.Parse(typeof(T), value, true);
    }

    /// <summary>
    /// Get enum name.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="enumValue"></param>
    /// <returns></returns>
    public static string GetName<T>(this Enum enumValue)
    {
        return Enum.GetName(typeof(T), enumValue);
    }

    /// <summary>
    /// Set the z value of a Vector3 property.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="value"></param>
    public static Vector3 SetZ(this Vector3 position, float value)
    {
        position = new Vector3(position.x, position.y, value);
        return position;
    }

    /// <summary>
    /// Set the x value of a Vector3 property.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="value"></param>
    public static Vector3 SetX(this Vector3 position, float value)
    {
        position = new Vector3(value, position.y, position.z);
        return position;
    }

    /// <summary>
    /// Set the x value of a Vector3 property.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="value"></param>
    public static Vector3 SetY(this Vector3 position, float value)
    {
        position = new Vector3(position.x, value, position.z);
        return position;
    }

    /// <summary>
    /// Set every first letter of words to uppercase.
    /// </summary>
    /// <param name="str">Target string</param>
    /// <returns></returns>
    public static string ToTitleCase(this string str)
    {
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
    }

    /// <summary>
    /// Add a value to list if not already added.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="value"></param>
    public static void AddIfNotExists<T>(this List<T> list, T value)
    {
        if (list.IndexOf(value) < 0)
        {
            list.Add(value);
        }
    }

    /// <summary>
    /// Replace character in a string by index.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="charToReplace"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public static string ReplaceChar(this string source, char charToReplace, int index)
    {
        StringBuilder sBuilder = new StringBuilder(source);
        if (index >= 0 && index < sBuilder.Length) sBuilder[index] = charToReplace;
        return sBuilder.ToString();
    }
}