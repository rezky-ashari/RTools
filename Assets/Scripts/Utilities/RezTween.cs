using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <para>Simple tween engine.</para>
/// Author: Rezky Ashari
/// </summary>
public class RezTween {

    /// <summary>
    /// Tween manager instance for all tweens.
    /// </summary>
    static RezTweenManager TweenManager
    {
        get
        {
            if (_manager == null)
            {
                _manager = RezTweenManager.Init();
            }
            return _manager;
        }
    }
    static RezTweenManager _manager;

    /// <summary>
    /// Special properties used by RezTween.
    /// </summary>
    static readonly string[] specialProperties = new string[]{ "delay", "repeat", "repeatDelay", "yoyo", "parentProperty", "ease" };

    static Dictionary<string, RezTweenEase> easeFunctions;
    RezTweenEase ease = EaseFunctions.Linear;

    object target;
    float duration;

    object targetParent;
    string parentPropName = "";
    string targetPropName;

    float timer = 0;
    float durationInv = 0;
    float delay = 0;
    float timePassed = 0;
    float lastProgress = 0;

    int repeat = 0;
    float repeatDelay = 0;
    float repeatWaitTime = 0;

    bool yoyo = false;

    /// <summary>
    /// Property values to tween.
    /// </summary>
    List<TweenValue> tweenValues = new List<TweenValue>();

    /// <summary>
    /// Tween duration.
    /// </summary>
    public float Duration { get { return duration; } }

    /// <summary>
    /// Called when tween complete.
    /// </summary>
    public Action OnComplete;

    /// <summary>
    /// Called every update.
    /// </summary>
    public Action OnUpdate;

    /// <summary>
    /// Called when progress changed.
    /// </summary>
    public Action<float> OnUpdateProgress;

    /// <summary>
    /// Whether tween is paused.
    /// </summary>
    public bool paused = false;

    /// <summary>
    /// Tween progress from 0 (start) to 1 (complete).
    /// </summary>
    public float Progress
    {
        get
        {
            return Mathf.Clamp(timer / duration, 0, 100);
        }
    }

    /// <summary>
    /// Whether tween process was completed.
    /// </summary>
    public bool IsCompleted
    {
        get { return timer >= duration && repeat == 0; }
    }

    /// <summary>
    /// Create a tween instance that affect many properties.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="duration"></param>
    /// <param name="properties"></param>
    public RezTween(object target, float duration, params string[] properties)
    {
        // Can't run in editor mode.
        if (!Application.isPlaying) return;

        if (target == null)
        {
            Debug.LogError("Can't tween a null object!");
            return;
        }

        durationInv = 1f / (duration != 0f ? duration : 1f);

        if (ContainsTweenKey(properties, "parentProperty"))
        {
            parentPropName = GetPropertyValue<string>(properties, "parentProperty");
            targetParent = target;
            target = GetValue(target, parentPropName);
        }

        string[] propData;
        string propName;
        string propValue;
        foreach (string property in properties)
        {
            propData = property.Split(':');
            propName = propData[0];
            propValue = propData[1];
            switch (propName)
            {
                case "ease":
                    ease = GetEaseFunction(propValue);
                    break;
                case "delay":
                    delay = float.Parse(propValue.Replace("f", ""));
                    break;
                case "repeat":
                    repeat = int.Parse(propValue);
                    break;
                case "repeatDelay":
                    repeatDelay = float.Parse(propValue);
                    break;
                case "yoyo":
                    yoyo = propValue.ToLower() == "true";
                    break;
                default:
                    if (ExistInArray(specialProperties, propName)) continue;
                    if (!ObjectHasProperty(target, propName))
                    {
                        Debug.Log("There's no property with name " + propName + " in " + target.GetType().Name);
                        continue;
                    }
                    float initialValue = (float)GetValue(target, propName);
                    float targetValue = float.Parse(propValue.Replace("f", ""));
                    if (initialValue != targetValue)
                    {
                        tweenValues.Add(new TweenValue(propName, initialValue, targetValue));
                    }
                    break;
            }
        }

        this.target = target;
        this.duration = duration;

        if (duration <= 0)
        {
            FinalizeValues();
            DelayedCall(0.01f, TweenComplete);
            if (targetParent != null) SetValue(targetParent, parentPropName, target);
        }
        else
        {
            TweenManager.AddTween(this);
        }        
    }

    private RezTweenEase GetEaseFunction(string ease)
    {
        //Debug.Log("Assign ease " + ease);
        string easeName = "ease:" + ease;
        if (easeFunctions == null)
        {
            easeFunctions = new Dictionary<string, RezTweenEase>();
            easeFunctions[RezTweenOptions.Ease.SPRING] = EaseFunctions.Spring;
            easeFunctions[RezTweenOptions.Ease.BACK_IN] = EaseFunctions.BackIn;
            easeFunctions[RezTweenOptions.Ease.BACK_OUT] = EaseFunctions.BackOut;
            easeFunctions[RezTweenOptions.Ease.BACK_IN_OUT] = EaseFunctions.BackInOut;
            easeFunctions[RezTweenOptions.Ease.BOUNCE_IN] = EaseFunctions.BounceIn;
            easeFunctions[RezTweenOptions.Ease.BOUNCE_OUT] = EaseFunctions.BounceOut;
            easeFunctions[RezTweenOptions.Ease.BOUNCE_IN_OUT] = EaseFunctions.BounceInOut;
            easeFunctions[RezTweenOptions.Ease.ELASTIC_IN] = EaseFunctions.ElasticIn;
            easeFunctions[RezTweenOptions.Ease.ELASTIC_OUT] = EaseFunctions.ElasticOut;
            easeFunctions[RezTweenOptions.Ease.ELASTIC_IN_OUT] = EaseFunctions.ElasticInOut;
        }
        return easeFunctions.ContainsKey(easeName)? easeFunctions[easeName] : EaseFunctions.Linear;
    }

    /// <summary>
    /// Get a property value in an object.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="propName"></param>
    /// <returns></returns>
    static object GetValue(object obj, string propName)
    {
        if (!ObjectHasProperty(obj, propName))
        {
            //Debug.Log("Can't set value of " + propName + " because it is not exist in " + obj.GetType().Name);
            return null;
        }

        if (obj.GetType().IsValueType)
        {
            return obj.GetType().GetField(propName).GetValue(obj);
        }
        else
        {
            return obj.GetType().GetProperty(propName).GetValue(obj, null);
        }
    }

    /// <summary>
    /// Set a property value in an object.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="propName"></param>
    /// <param name="value"></param>
    static void SetValue(object obj, string propName, object value)
    {
        if (!ObjectHasProperty(obj, propName))
        {
            //Debug.Log("Can't set value of " + propName + " because it is not exist in " + obj.GetType().Name);
            return;
        }

        if (obj.GetType().IsValueType)
        {
            obj.GetType().GetField(propName).SetValue(obj, value);
        }
        else
        {
            obj.GetType().GetProperty(propName).SetValue(obj, value, null);
        }
    }

    /// <summary>
    /// Check whether an object has property.
    /// </summary>
    /// <param name="obj">Object to check</param>
    /// <param name="propName">Property name</param>
    /// <returns></returns>
    static bool ObjectHasProperty(object obj, string propName)
    {
        if (obj == null) return false;
        if (obj.GetType().IsValueType)
        {
            return obj.GetType().GetField(propName) != null;
        }
        else
        {
            return obj.GetType().GetProperty(propName) != null;
        }
    }

    /// <summary>
    /// Join a list of string to a string.
    /// </summary>
    /// <param name="texts"></param>
    /// <param name="separator"></param>
    /// <returns></returns>
    static string Join(string[] texts, string separator = ", ")
    {
        return string.Join(separator, texts);
    }

    /// <summary>
    /// Check if an array contains a keyword.
    /// </summary>
    /// <param name="list"></param>
    /// <param name="keyword"></param>
    /// <returns></returns>
    static bool Contains(string[] list, string keyword)
    {
        return Array.FindIndex(list, x => x.Contains(keyword)) >= 0;
    }

    /// <summary>
    /// Check if string list starts with a key.
    /// </summary>
    /// <param name="list"></param>
    /// <param name="keyword"></param>
    /// <returns></returns>
    static bool ContainsTweenKey(string[] list, string keyword)
    {
        return Array.FindIndex(list, x => x.StartsWith(keyword)) >= 0;
    }

    /// <summary>
    /// Check if an array contains a keyword.
    /// </summary>
    /// <param name="list"></param>
    /// <param name="keyword"></param>
    /// <returns></returns>
    static bool ExistInArray(string[] list, string keyword)
    {
        return Array.IndexOf(list, keyword) >= 0;
    }

    internal void Update(float deltaTime)
    {
        if (paused)
        {
            UpdateValues();
            return;
        }

        if (repeatWaitTime > 0)
        {
            repeatWaitTime -= deltaTime;
            return;
        }

        if (timePassed < delay)
        {
            timePassed += deltaTime;
        }
        else
        {
            UpdateValues();
            timer += deltaTime;
        }
    }

    void UpdateValues()
    {
        try
        {
            foreach (TweenValue tValues in tweenValues)
            {
                tValues.Update(timer * durationInv, target, ease);
            }
            if (targetParent != null) SetValue(targetParent, parentPropName, target);
            if (OnUpdate != null) OnUpdate();
            if (OnUpdateProgress != null && lastProgress != Progress)
            {
                OnUpdateProgress(Progress);
                lastProgress = Progress;
            }
            if (timer >= duration) TweenComplete();
        }
        catch
        {
            //Debug.Log(e.Message);
            //Debug.Log("Object " + target.GetType().Name + " was destroyed. Remove this tween...");
            TweenManager.RemoveTween(this);
        }
    }

    void ResetValues()
    {
        if (tweenValues != null)
        {
            foreach (TweenValue tValues in tweenValues)
            {
                tValues.SetToInitialValue(target);
            }
        }       
    }

    void SwapValues()
    {
        foreach (TweenValue tValues in tweenValues)
        {
            tValues.SwapValue();
        }
    }

    void FinalizeValues()
    {
        foreach (TweenValue value in tweenValues)
        {
            value.SetToTargetValue(target);
        }
        if (targetParent != null) SetValue(targetParent, parentPropName, target);
    }

    void TweenComplete()
    {
        FinalizeValues();

        if (repeat > 0)
        {
            repeat--;
            RepeatTween();
        }
        else if (repeat == 0)
        {
            Destroy();
            if (OnComplete != null)
            {
                //Debug.Log("Fired this " + OnComplete);
                OnComplete();
            }
        }
        else
        {
            RepeatTween();
        }
    }

    void RepeatTween()
    {
        if (yoyo) SwapValues();
        else ResetValues();
        timer = 0;
        repeatWaitTime = repeatDelay;
    }

    /// <summary>
    /// Start this tween from the beginning.
    /// </summary>
    public void Restart()
    {
        ResetValues();
        timer = 0;
        paused = false;
    }

    /// <summary>
    /// Destroy this tween.
    /// </summary>
    public void Destroy()
    {
        TweenManager.RemoveTween(this);
        tweenValues = null;
    }

    /// <summary>
    /// Pause this tween.
    /// </summary>
    public void Pause()
    {
        paused = true;
    }

    /// <summary>
    /// Resume this tween.
    /// </summary>
    public void Resume()
    {
        paused = false;
    }

    /// <summary>
    /// Get property data. Basically convert from string 'propName:propValue' to Array of string {'propName', 'propValue'}
    /// </summary>
    /// <param name="property">Property data (eg: 'alpha:1')</param>
    /// <returns></returns>
    static string[] GetPropData(string property)
    {
        //Debug.Log("Get property data of " + property);
        return property.Split(':');
    }

    /// <summary>
    /// Tween an object property to a new property value.
    /// </summary>
    /// <param name="target">Target object</param>
    /// <param name="duration">Tween duration</param>
    /// <param name="properties">Target property with their target value (eg: 'alpha:1')</param>
    /// <returns></returns>
    public static RezTween To(object target, float duration, params string[] properties)
    {
        return new RezTween(target, duration, properties);
    }

    /// <summary>
    /// Tween an image's alpha.
    /// </summary>
    /// <param name="image"></param>
    /// <param name="duration"></param>
    /// <param name="targetAlpha"></param>
    /// <returns></returns>
    public static RezTween AlphaTo(object image, float duration, float targetAlpha)
    {
        if (!ObjectHasProperty(image, "color"))
        {
            Debug.LogError("This object is not an image.");
            return null;
        }
        return To(image, duration, "a:" + targetAlpha, "parentProperty:color");
    }

    /// <summary>
    /// Tween an image's alpha (transparency).
    /// </summary>
    /// <param name="image">Target image.</param>
    /// <param name="duration">Tween duration.</param>
    /// <param name="initialAlpha">Initial alpha</param>
    /// <param name="targetAlpha">Target alpha</param>
    /// <returns></returns>
    public static RezTween AlphaFromTo(object image, float duration, float initialAlpha, float targetAlpha)
    {
        var color = GetValue(image, "color");
        if (color != null)
        {
            SetValue(color, "a", initialAlpha);
            SetValue(image, "color", color);
        }
        return AlphaTo(image, duration, targetAlpha);
    }

    /// <summary>
    /// Tween an image's color.
    /// </summary>
    /// <param name="image"></param>
    /// <param name="target"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    public static RezTween ColorTo(object image, Color target, float duration)
    {
        Debug.Log("Tween color: " + target);
        return To(image, duration, "r:" + target.r, "g:" + target.g, "b:" + target.b, "a:" + target.a, RezTweenOptions.ReadOnlyProperty("color"));
    }

    /// <summary>
    /// Find a property from the list of properties and return the value.
    /// </summary>
    /// <typeparam name="T">Return type</typeparam>
    /// <param name="properties">Property list</param>
    /// <param name="name">Property name to find</param>
    /// <param name="checkedIndex">For store the checked property index</param>
    /// <returns></returns>
    private static T GetPropertyValue<T>(string[] properties, string name, ref List<int> checkedIndex)
    {
        int propIndex = Array.FindIndex<string>(properties, x => x.Contains(name));
        if (propIndex >= 0)
        {
            checkedIndex.Add(propIndex);
            return (T)Convert.ChangeType(GetPropData(properties[propIndex])[1], typeof(T));
        }
        else
        {
            return default(T);
        }
    }

    /// <summary>
    /// Find a property from the list of properties and return the value.
    /// </summary>
    /// <typeparam name="T">Return type</typeparam>
    /// <param name="properties">Property list</param>
    /// <param name="name">Property name to find</param>
    /// <param name="checkedIndex">For store the checked property index</param>
    /// <returns></returns>
    private static T GetPropertyValue<T>(string[] properties, string name)
    {
        int propIndex = Array.FindIndex<string>(properties, x => x.StartsWith(name));
        if (propIndex >= 0)
        {
            return (T)Convert.ChangeType(GetPropData(properties[propIndex])[1], typeof(T));
        }
        else
        {
            return default(T);
        }
    }

    /// <summary>
    /// Add a string prefix to all properties except for the special properties (like 'delay' and 'loop').
    /// </summary>
    /// <param name="properties"></param>
    /// <param name="prefix"></param>
    private static void AddPrefixToProperties(string[] properties, string prefix)
    {
        for (int i = 0; i < properties.Length; i++)
        {
            string propertyName = properties[i];
            if (Contains(specialProperties, propertyName)) continue;
            propertyName = prefix + propertyName;
            properties[i] = propertyName;
        }
    }

    /// <summary>
    /// Move a game object to a new position by time.
    /// </summary>
    /// <param name="gameObject">Target game object</param>
    /// <param name="duration">Tween duration</param>
    /// <param name="properties">Target position (ex: 'x:100', 'y:520')</param>
    public static RezTween MoveTo(GameObject gameObject, float duration, params string[] properties)
    {
        //AddPrefixToProperties(properties, "localPosition.");
        List<string> targetProperties = new List<string>();
        targetProperties.AddRange(properties);
        if (!Contains(properties, "parentProperty"))
        {
            targetProperties.Add("parentProperty:localPosition");
        }        
        return To(gameObject.transform, duration, targetProperties.ToArray());
    }

    /// <summary>
    /// Move a game object to a new position by time.
    /// </summary>
    /// <param name="gameObject">Target game object</param>
    /// <param name="duration">Tween duration</param>
    /// <param name="destination">Target position</param>
    /// <param name="properties">Tween properties (ex: delay)</param>
    public static RezTween MoveTo(GameObject gameObject, float duration, Vector3 destination, params string[] properties)
    {
        List<string> propList = new List<string>();
        propList.AddRange(properties);
        propList.AddRange(CreateProperties(destination));
        return MoveTo(gameObject, duration, propList.ToArray());
    }

    /// <summary>
    /// Move a game object based on current position.
    /// </summary>
    /// <param name="gameObject">Target game object</param>
    /// <param name="duration">Tween duration</param>
    /// <param name="properties">Move distance (ex: 'x:100', 'y:520')</param>
    /// <returns></returns>
    public static RezTween MoveBy(GameObject gameObject, float duration, params string[] properties)
    {
        for (int i = 0; i < properties.Length; i++)
        {
            string[] pData = GetPropData(properties[i]);
            switch(pData[0])
            {
                case "x":
                case "y":
                case "z":
                    string curValue = GetValue(gameObject.transform.localPosition, pData[0]).ToString();
                    properties[i] = pData[0] + ":" + (float.Parse(curValue) + float.Parse(pData[1]));
                    break;
            }
        }
        //Debug.Log(gameObject.name + " Move by -> " + properties.Join());
        return MoveTo(gameObject, duration, properties);
    }

    /// <summary>
    /// Move a game object based on current position.
    /// </summary>
    /// <param name="gameObject">Target game object</param>
    /// <param name="duration">Tween duration</param>
    /// <param name="properties">Move distance</param>
    public static RezTween MoveBy(GameObject gameObject, float duration, Vector3 properties)
    {
        return MoveBy(gameObject, duration, CreateProperties(properties));
    }

    /// <summary>
    /// Move a game object from the given position to current position.
    /// </summary>
    /// <param name="gameObject">Target game object</param>
    /// <param name="duration">Tween duration</param>
    /// <param name="properties">Start position (ex: 'x:100', 'y:300')</param>
    public static RezTween MoveFrom(GameObject gameObject, float duration, params string[] properties)
    {
        List<string> targetProperties = new List<string>();
        string parentProperty = "localPosition";

        // Assign start position
        object currentPosition = gameObject.transform.localPosition;
        if (ContainsTweenKey(properties, "parentProperty"))
        {
            parentProperty = GetPropertyValue<string>(properties, "parentProperty");
            if (parentProperty == "position") currentPosition = gameObject.transform.position;
        }
        string[] propList = new string[] { "x", "y", "z" };
        foreach (string propName in propList)
        {
            if (ContainsTweenKey(properties, propName))
            {
                targetProperties.Add(propName + ":" + GetValue(currentPosition, propName));
                SetValue(currentPosition, propName, GetPropertyValue<float>(properties, propName));
            }
        }
        // Add special properties
        foreach (string property in properties)
        {
            if (ExistInArray(specialProperties, GetPropData(property)[0]))
            {
                targetProperties.Add(property);
            }
        }
        // Move to start position
        if (parentProperty == "localPosition")
        {
            gameObject.transform.localPosition = (Vector3)currentPosition;
        }
        else
        {
            gameObject.transform.position = (Vector3)currentPosition;
        }
        
        return MoveTo(gameObject, duration, targetProperties.ToArray());
    }
    
    /// <summary>
    /// Move a game object from the given position to current position.
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="duration"></param>
    /// <param name="startPosition"></param>
    /// <param name="properties"></param>
    /// <returns></returns>
    public static RezTween MoveFrom(GameObject gameObject, float duration, Vector3 startPosition, params string[] properties)
    {
        List<string> prop = new List<string>();
        prop.AddRange(CreateProperties(startPosition));
        prop.AddRange(properties);
        return MoveFrom(gameObject, duration, prop.ToArray());
    }

    /// <summary>
    /// Create a properties format of Vector3 values.
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    static string[] CreateProperties(Vector3 source)
    {
        List<string> properties = new List<string>();
        string[] propList = new string[] { "x", "y", "z" };
        foreach (string propName in propList)
        {
            properties.Add(propName + ":" + GetValue(source, propName));
        }
        return properties.ToArray();
    }

    /// <summary>
    /// Scale a game object to a specified value (current value is starting value).
    /// </summary>
    /// <param name="gameObject">Target game object</param>
    /// <param name="duration">Tween duration</param>
    /// <param name="properties">Scale properties (ex: 'x:2', 'y:-1')</param>
    /// <returns></returns>
    public static RezTween ScaleTo(GameObject gameObject, float duration, params string[] properties)
    {
        List<string> optionProperties = new List<string>();
        object targetScale = gameObject.transform.localScale;
        foreach (string prop in properties)
        {
            string[] propData = GetPropData(prop);
            if (Contains(specialProperties, propData[0]))
            {
                optionProperties.Add(prop);
                continue;
            }
            SetValue(targetScale, propData[0], float.Parse(propData[1].Replace("f", "")));
        }
        return ScaleTo(gameObject, duration, (Vector3)targetScale, optionProperties.ToArray());
    }

    /// <summary>
    /// Scale a game object to a specified value (current value is starting value).
    /// </summary>
    /// <param name="gameObject">Target game object</param>
    /// <param name="duration">Tween duration</param>
    /// <param name="targetScale">Target scale</param>
    /// <param name="properties">Scale properties (ex: 'x:2', 'y:-1')</param>
    /// <returns></returns>
    public static RezTween ScaleTo(GameObject gameObject, float duration, Vector3 targetScale, params string[] properties)
    {
        List<string> propList = new List<string>
        {
            "x:" + targetScale.x,
            "y:" + targetScale.y,
            "z:" + targetScale.z,
            "parentProperty:localScale"
        };
        propList.AddRange(properties);

        //Debug.Log("Tween scale to: " + targetScale + ", prop: " + string.Join(", ", properties));
        return To(gameObject.transform, duration, propList.ToArray());
    }

    /// <summary>
    /// Scale a game object to a specified value (current value is starting value).
    /// </summary>
    /// <param name="gameObject">Target game object</param>
    /// <param name="duration">Tween duration</param>
    /// <param name="targetScale">Target scale</param>
    /// <param name="properties">Scale properties (ex: 'x:2', 'y:-1')</param>
    /// <returns></returns>
    public static RezTween ScaleTo(GameObject gameObject, float duration, float scale, params string[] properties)
    {
        return ScaleTo(gameObject, duration, new Vector3(scale, scale, scale), properties);
    }

    /// <summary>
    /// Scale a game object from a specified value (target value is starting value).
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="duration"></param>
    /// <param name="scale"></param>
    /// <param name="properties"></param>
    /// <returns></returns>
    public static RezTween ScaleFrom(GameObject gameObject, float duration, float scale, params string[] properties)
    {
        Vector3 targetScale = gameObject.transform.localScale;
        gameObject.transform.localScale = new Vector3(scale, scale, scale);
        return ScaleTo(gameObject, duration, targetScale, properties);
    }

    /// <summary>
    /// Scale a gameObject from a start value to the end value.
    /// </summary>
    /// <param name="gameObject">GameObject to scale</param>
    /// <param name="duration">Tween duration</param>
    /// <param name="startScale">Start value</param>
    /// <param name="endScale">End value</param>
    /// <param name="properties">Extra properties</param>
    /// <returns></returns>
    public static RezTween ScaleFromTo(GameObject gameObject, float duration, float startScale, float endScale, params string[] properties)
    {
        gameObject.transform.localScale = new Vector3(startScale, startScale, startScale);
        return ScaleTo(gameObject, duration, endScale, properties);
    }

    /// <summary>
    /// Rotate a game object over time.
    /// </summary>
    /// <param name="gameObject">Target game object</param>
    /// <param name="duration">Tween duration</param>
    /// <param name="properties">Rotation property to change (ex: 'x:10', 'z:-50')</param>
    /// <returns></returns>
    public static RezTween RotateTo(GameObject gameObject, float duration, params string[] properties)
    {
        List<string> propList = new List<string>()
        {
            "parentProperty:localEulerAngles"
        };
        propList.AddRange(properties);
        return To(gameObject.transform, duration, propList.ToArray());
    }

    public static RezTween RotateFrom(GameObject gameObject, float duration, params string[] properties)
    {
        object targetEulerAngles = gameObject.transform.localEulerAngles;
        object startEulerAngles = gameObject.transform.localEulerAngles;
        for (int i = 0; i < properties.Length; i++)
        {
            string[] propData = GetPropData(properties[i]);
            switch (propData[0])
            {
                case "x":
                case "y":
                case "z":
                    SetValue(startEulerAngles, propData[0], float.Parse(propData[1]));
                    properties[i] = propData[0] + ":" + GetValue(targetEulerAngles, propData[0]);
                    break;
            }
        }
        gameObject.transform.localEulerAngles = (Vector3)startEulerAngles;
        return RotateTo(gameObject, duration, properties);
    }

    public static RezTween ResizeUI(GameObject gameObject, float duration, params string[] properties)
    {
        List<string> propList = new List<string>(properties)
        {
            RezTweenOptions.ReadOnlyProperty("sizeDelta")
        };
        return To(gameObject.transform as RectTransform, duration, propList.ToArray());
    }

    public static RezTween ValueRange(float start, float end, float duration, Action<float> onUpdate = null)
    {
        TweenProgress progress = new TweenProgress(start);
        return new RezTween(progress, duration, "Value:" + end)
        {
            OnUpdate = () =>
            {
                if (onUpdate != null) onUpdate(progress.Value);
            }
        };
    }

    /// <summary>
    /// Starts a courotine.
    /// </summary>
    /// <param name="routine"></param>
    public static void StartCoroutine(System.Collections.IEnumerator routine)
    {
        TweenManager.StartCoroutine(routine);
    }

    /// <summary>
    /// Make a delayed call to a function.
    /// </summary>
    /// <param name="duration">Delay duration in seconds</param>
    /// <param name="callback">Function to call</param>
    public static RezTween DelayedCall(float duration, Action callback)
    {
        TweenProgress progress = new TweenProgress();
        RezTween tween = new RezTween(progress, duration, "Value:100")
        {
            OnComplete = callback
        };
        return tween;
    }

    /// <summary>
    /// Stops a delayed call instance from being executed.
    /// </summary>
    /// <param name="delayedCall">Instance to stop.</param>
    /// <param name="executeCompleteCallback">Whether to execute the onComplete callback. Default value is <code>false</code>.</param>
    public static void StopDelayedCall(ref RezTween delayedCall, bool executeCompleteCallback = false)
    {
        if (delayedCall != null)
        {
            delayedCall.Destroy();
            if (executeCompleteCallback && delayedCall.OnComplete != null) delayedCall.OnComplete();
            delayedCall = null;
        }
    }

    /// <summary>
    /// Stops and remove a tween, then set the reference to null.
    /// </summary>
    /// <param name="tween">Tween instance to clear. Nothing happen if <code>null</code>.</param>
    /// <param name="resetValue">'0' will reset to the initial values, '1' will reset to the final values. Any other values will be ignored.</param>
    public static void Clear(ref RezTween tween, float resetValue = -1)
    {
        if (tween != null)
        {
            tween.Destroy();
            if (resetValue == 0)
            {
                tween.ResetValues();
            }
            else if (resetValue == 1)
            {
                tween.FinalizeValues();
            }
            tween = null;
        }
    }

    /// <summary>
    /// Dummy object for tween operation.
    /// </summary>
    class TweenProgress
    {
        public TweenProgress(float startValue = 0)
        {
            Value = startValue;
        }
        public float Value { get; set; }
    }

    /// <summary>
    /// Store values that need to be updated.
    /// </summary>
    class TweenValue
    {
        string propertyName;
        float initialValue;
        float targetValue;
        float currentValue;

        /// <summary>
        /// Initial tween value.
        /// </summary>
        public float Start { get { return initialValue; } }

        /// <summary>
        /// Target tween value.
        /// </summary>
        public float End { get { return targetValue; } }

        /// <summary>
        /// Current tween value.
        /// </summary>
        public float Current { get { return currentValue; } }

        public TweenValue(string propertyName, float initialValue, float targetValue)
        {
            this.propertyName = propertyName;
            this.initialValue = initialValue;
            this.targetValue = targetValue;

            //Debug.Log("Add tween value " + propertyName + ", initial Value: " + initialValue + ", targetValue: " + targetValue);
        }

        public void Update(float time, object target, RezTweenEase ease)
        {
            currentValue = ease(initialValue, targetValue, time);
            SetValue(target, propertyName, currentValue);
            //Debug.Log("Update value of " + propertyName + " to " + currentValue);
        }

        /// <summary>
        /// Set the current value to initial value.
        /// </summary>
        /// <param name="target"></param>
        public void SetToInitialValue(object target)
        {
            currentValue = initialValue;
            SetValue(target, propertyName, currentValue);
        }

        /// <summary>
        /// Set the current value to target value.
        /// </summary>
        /// <param name="target"></param>
        public void SetToTargetValue(object target)
        {
            currentValue = targetValue;
            SetValue(target, propertyName, currentValue);
            //Debug.Log("Set to final value " + currentValue);
        }

        /// <summary>
        /// Swap initial value with target value.
        /// </summary>
        public void SwapValue()
        {
            float temp = initialValue;
            initialValue = targetValue;
            targetValue = temp;
        }
    }

    #region Ease
    delegate float RezTweenEase(float start, float end, float time);

    /// <summary>
    /// Contains ease equations taken and modified from https://gist.github.com/cjddmut/d789b9eb78216998e95c
    /// Originally Created by C.J. Kimberlin (http://cjkimberlin.com)
    /// </summary>
    class EaseFunctions
    {
        const float backEaseThreshold = 3f;

        internal static float Linear(float start, float end, float time)
        {
            return Mathf.Lerp(start, end, time);
        }

        internal static float Spring(float start, float end, float value)
        {
            value = Mathf.Clamp01(value);
            value = (Mathf.Sin(value * Mathf.PI * (0.2f + 2.5f * value * value * value)) * Mathf.Pow(1f - value, 2.2f) + value) * (1f + (1.2f * (1f - value)));
            return start + (end - start) * value;
        }

        internal static float BackIn(float start, float end, float time)
        {
            end -= start;
            time /= 1;
            float s = backEaseThreshold;
            return end * (time) * time * ((s + 1) * time - s) + start;
        }

        internal static float BackOut(float start, float end, float time)
        {
            float s = backEaseThreshold;
            end -= start;
            time = (time) - 1;
            return end * ((time) * time * ((s + 1) * time + s) + 1) + start;
        }

        internal static float BackInOut(float start, float end, float time)
        {
            float s = backEaseThreshold;
            end -= start;
            time /= .5f;
            if ((time) < 1)
            {
                s *= (1.525f);
                return end * 0.5f * (time * time * (((s) + 1) * time - s)) + start;
            }
            time -= 2;
            s *= (1.525f);
            return end * 0.5f * ((time) * time * (((s) + 1) * time + s) + 2) + start;
        }

        internal static float BounceIn(float start, float end, float time)
        {
            end -= start;
            float d = 1f;
            return end - BounceOut(0, end, d - time) + start;
        }

        internal static float BounceOut(float start, float end, float time)
        {
            time /= 1f;
            end -= start;
            if (time < (1 / 2.75f))
            {
                return end * (7.5625f * time * time) + start;
            }
            else if (time < (2 / 2.75f))
            {
                time -= (1.5f / 2.75f);
                return end * (7.5625f * (time) * time + .75f) + start;
            }
            else if (time < (2.5 / 2.75))
            {
                time -= (2.25f / 2.75f);
                return end * (7.5625f * (time) * time + .9375f) + start;
            }
            else
            {
                time -= (2.625f / 2.75f);
                return end * (7.5625f * (time) * time + .984375f) + start;
            }
        }

        internal static float BounceInOut(float start, float end, float value)
        {
            end -= start;
            float d = 1f;
            if (value < d * 0.5f) return BounceIn(0, end, value * 2) * 0.5f + start;
            else return BounceOut(0, end, value * 2 - d) * 0.5f + end * 0.5f + start;
        }

        internal static float ElasticIn(float start, float end, float value)
        {
            end -= start;

            float d = 1f;
            float p = d * .3f;
            float s;
            float a = 0;

            if (value == 0) return start;

            if ((value /= d) == 1) return start + end;

            if (a == 0f || a < Mathf.Abs(end))
            {
                a = end;
                s = p / 4;
            }
            else
            {
                s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
            }

            return -(a * Mathf.Pow(2, 10 * (value -= 1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p)) + start;
        }

        internal static float ElasticOut(float start, float end, float value)
        {
            end -= start;

            float d = 1f;
            float p = d * .3f;
            float s;
            float a = 0;

            if (value == 0) return start;

            if ((value /= d) == 1) return start + end;

            if (a == 0f || a < Mathf.Abs(end))
            {
                a = end;
                s = p * 0.25f;
            }
            else
            {
                s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
            }

            return (a * Mathf.Pow(2, -10 * value) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p) + end + start);
        }

        internal static float ElasticInOut(float start, float end, float value)
        {
            end -= start;

            float d = 1f;
            float p = d * .3f;
            float s;
            float a = 0;

            if (value == 0) return start;

            if ((value /= d * 0.5f) == 2) return start + end;

            if (a == 0f || a < Mathf.Abs(end))
            {
                a = end;
                s = p / 4;
            }
            else
            {
                s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
            }

            if (value < 1) return -0.5f * (a * Mathf.Pow(2, 10 * (value -= 1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p)) + start;
            return a * Mathf.Pow(2, -10 * (value -= 1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p) * 0.5f + end + start;
        }

    }
    #endregion

    #region TweenManager
    /// <summary>
    /// <para>Manager for all RezTween instances.</para>
    /// Author: Rezky Ashari
    /// </summary>
    class RezTweenManager : MonoBehaviour
    {
        /// <summary>
        /// Total running tween.
        /// </summary>
        public int activeTween = 0;

        /// <summary>
        /// Running tweens.
        /// </summary>
        List<RezTween> tweens = new List<RezTween>();

        /// <summary>
        /// Initialize the TweenManager by adding it to the scene.
        /// </summary>
        /// <returns></returns>
        internal static RezTweenManager Init()
        {
            GameObject tweenManagerObject = new GameObject("TweenManager");
            DontDestroyOnLoad(tweenManagerObject);
            return tweenManagerObject.AddComponent<RezTweenManager>();            
        }

        /// <summary>
        /// Add tween to the TweenManager for update.
        /// </summary>
        /// <param name="tween"></param>
        internal void AddTween(RezTween tween)
        {
            if (tweens.IndexOf(tween) == -1)
            {
                bool replaced = false;
                for (int i = 0; i < tweens.Count; i++)
                {
                    if (tweens[i].targetParent != null && ReferenceEquals(tweens[i].targetParent, tween.targetParent))
                    {
                        //Debug.Log("Same parent! Have same target? " + ReferenceEquals(tweens[i].target, tween.target));
                        if (tweens[i].parentPropName == tween.parentPropName)
                        {
                            tweens[i] = tween;
                            replaced = true;
                            //Debug.Log("Replace tween for the same target parent");
                        }
                    }
                    else if (ReferenceEquals(tweens[i].target, tween.target))
                    {
                        tweens[i] = tween;
                        replaced = true;
                        //Debug.Log("Replace tween for the same target");
                        break;
                    }
                }
                if (!replaced) tweens.Add(tween);
                //Debug.Log("Added tween to index: " + tweens.IndexOf(tween) + " Target: " + GetTypeName(tween.target) + ", parentTarget: " + GetTypeName(tween.targetParent) + ", replaced? " + replaced);
            }
        }

        private string GetTypeName(object obj)
        {
            if (obj == null) return "null";
            return obj.GetType().Name;
        }

        /// <summary>
        /// Remove tween from the TweenManager to stop update.
        /// </summary>
        /// <param name="tween"></param>
        internal void RemoveTween(RezTween tween)
        {
            if (tweens.IndexOf(tween) >= 0)
            {
                //Debug.Log("Tween to remove " + tweens.IndexOf(tween));
                tweens.Remove(tween);
            }
        }

        void Awake()
        {
            //Debug.Log("Manager awake!");
        }

        private void Update()
        {
            for (int i = tweens.Count - 1; i >= 0; i--)
            {
                tweens[i].Update(Time.smoothDeltaTime);
            }
            activeTween = tweens.Count;
        }
    }
    #endregion
}

/// <summary>
/// <para>Options for RezTween.</para>
/// Author: Rezky
/// </summary>
public class RezTweenOptions
{
    /// <summary>
    /// <para>Apply ease option to a RezTween</para>
    /// Author: Rezky Ashari
    /// </summary>
    public struct Ease
    {
        public const string LINEAR = "ease:linear";
        public const string SPRING = "ease:spring";
        public const string BACK_IN = "ease:backIn";
        public const string BACK_OUT = "ease:backOut";
        public const string BACK_IN_OUT = "ease:backInOut";
        public const string BOUNCE_IN = "ease:bounceIn";
        public const string BOUNCE_OUT = "ease:bounceOut";
        public const string BOUNCE_IN_OUT = "ease:bounceInOut";
        public const string ELASTIC_IN = "ease:elasticIn";
        public const string ELASTIC_OUT = "ease:elasticOut";
        public const string ELASTIC_IN_OUT = "ease:elasticInOut";
    }

    /// <summary>
    /// Add delay (in seconds) before tween start.
    /// </summary>
    /// <param name="delay">Delay in seconds</param>
    /// <returns></returns>
    public static string Delay(float delay)
    {
        return "delay:" + delay;
    }

    /// <summary>
    /// Repeat tween.
    /// </summary>
    /// <param name="repeatTimes">How many times to repeat. Use value '-1' for infinity repeat.</param>
    /// <returns></returns>
    public static string Repeat(int repeatTimes = -1)
    {
        return "repeat:" + repeatTimes;
    }

    /// <summary>
    /// Delay before repeat starts.
    /// </summary>
    /// <param name="repeatDelay">Delay in seconds.</param>
    /// <returns></returns>
    public static string RepeatDelay(float repeatDelay)
    {
        return "repeatDelay:" + repeatDelay;
    }

    /// <summary>
    /// Repeat tween with yoyo mode. Only works with Repeat.
    /// </summary>
    /// <param name="repeatWithYoyoMode"></param>
    /// <returns></returns>
    public static string Yoyo(bool repeatWithYoyoMode = true)
    {
        return "yoyo:" + repeatWithYoyoMode;
    }

    /// <summary>
    /// Specify the read only property to overwrite.
    /// (eg: Transform.position is readonly property, so pass 'position' as a parameter)
    /// </summary>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static string ReadOnlyProperty(string propertyName)
    {
        return "parentProperty:" + propertyName;
    }

    /// <summary>
    /// Use global position in 'MoveTo', 'MoveFrom', and 'MoveBy' tween.
    /// </summary>
    public static string UseGlobalPosition
    {
        get { return ReadOnlyProperty("position"); }
    }
}
