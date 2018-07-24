using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// <para>Execute sequence of actions in lazy way.</para>
/// Author: Rezky Ashari
/// </summary>
public class Sequencer : MonoBehaviour {

    [Serializable]
    public class EventDelegate : UnityEvent<int> { };

    public bool playOnAwake = true;
    public List<SequencerActionParent> actions = new List<SequencerActionParent>();
    public int currentAct = 0;
    public bool isPlaying = false;
    public bool debugLog = false;

    public EventDelegate onBreak;
    public UnityEvent onComplete;

    bool isTakingABreak = false;

    /// <summary>
    /// Whether this sequence is started and running (not completed yet).
    /// </summary>
    public bool IsOnProgress
    {
        get
        {
            return (isPlaying || isTakingABreak) && !isComplete;
        }
    }

    /// <summary>
    /// Whether this sequence is currently taking a break.
    /// </summary>
    public bool IsTakingABreak
    {
        get
        {
            return isTakingABreak;
        }
    }

    /// <summary>
    /// Whether all sequence has been executed.
    /// </summary>
    public bool IsComplete
    {
        get { return isComplete; }
    }
    bool isComplete = false;

	// Use this for initialization
	void Start () {
        if (playOnAwake) Play();
	}

    /// <summary>
    /// Play sequence from the last action index.
    /// </summary>
    public void Play()
    {
        if (!isPlaying)
        {
            Log("Start playing sequence from " + currentAct);
            isPlaying = true;
            ExecuteNextAct();
        }        
    }

    /// <summary>
    /// Play sequence from the given action index.
    /// </summary>
    /// <param name="startingAct">Action index that will be the playhead.</param>
    public void Play(int startingAct)
    {
        isTakingABreak = false;
        isComplete = false;
        currentAct = startingAct;
        Play();
    }

    /// <summary>
    /// Stop this sequence.
    /// </summary>
    public void Stop()
    {
        isPlaying = false;
    }

    void ExecuteNextAct()
    {
        if (!isPlaying) return;

        currentAct++;
        if (currentAct <= actions.Count)
        {
            SequencerActionParent action = actions[currentAct - 1];
            if (action.active)
            {
                for (int i = 0; i < action.linkedActions.Count; i++)
                {
                    Execute(action.linkedActions[i]);
                }
                Execute(action);
            }
            else ExecuteNextAct();
        }
        else
        {
            isPlaying = false;
            isComplete = true;
            onComplete.Invoke();
        }
    }

    void Execute(SequencerAction action)
    {
        Log("Execute Action : " + action.ModeInfo);
        bool isParentAction = action is SequencerActionParent;
        switch (action.mode)
        {
            case SequencerActionMode.Wait:
                if (isParentAction)
                {
                    RezTween.DelayedCall(action.duration, ExecuteNextAct);
                }                
                break;
            case SequencerActionMode.MoveTo:
                if (action.duration <= 0)
                {
                    action.gameObject.transform.localPosition = action.position;
                    if (isParentAction) ExecuteNextAct();
                }
                else
                {
                    RezTween moveTween = RezTween.MoveTo(action.gameObject, action.duration, action.position, action.paramString);
                    if (isParentAction) moveTween.OnComplete = ExecuteNextAct;
                }   
                break;
            case SequencerActionMode.PlayAnimation:
                float animLength = action.PlayAnimation();
                StartCoroutine(WaitForNextFrame(()=>
                {
                    animLength = action.animator.GetCurrentAnimatorStateInfo(0).length;
                    if (isParentAction)
                    {
                        RezTween.DelayedCall(animLength, ExecuteNextAct);
                        Debug.Log("Play anim " + action.target + " with length: " + animLength);
                    }
                }));                         
                break;
            case SequencerActionMode.InvokeFunction:
                //action.script.Invoke(action.target, 0);
                Type[] paramType = Type.EmptyTypes;                
                switch (action.paramType)
                {
                    case "int":
                        paramType = new[] { typeof(int) };
                        break;
                    case "float":
                        paramType = new[] { typeof(float) };
                        break;
                    case "boolean":
                        paramType = new[] { typeof(bool) };
                        break;
                    case "string":
                        paramType = new[] { typeof(string) };
                        break;
                }
                MethodInfo method = action.script.GetType().GetMethod(action.target, paramType);
                object paramValue = action.GetParameterValue();
                if (paramValue != null)
                {
                    method.Invoke(action.script, new object[] { paramValue });
                }
                else
                {
                    method.Invoke(action.script, null);
                }
                if (isParentAction) ExecuteNextAct();
                break;
            case SequencerActionMode.SetActive:
                if (action.gameObject != null)
                {
                    action.gameObject.SetActive(action.paramBool);
                }
                if (isParentAction) ExecuteNextAct();
                break;
            case SequencerActionMode.Break:
                isTakingABreak = true;
                Stop();
                onBreak.Invoke(currentAct);
                break;
            case SequencerActionMode.SetAlpha:
                if (action.gameObject != null)
                {
                    Graphic graphic = action.gameObject.GetComponent<Graphic>();
                    if (graphic != null)
                    {
                        if (action.duration <= 0)
                        {
                            Color color = graphic.color;
                            color.a = action.paramFloat;
                            graphic.color = color;
                        }
                        else
                        {
                            RezTween alphaTween = RezTween.AlphaTo(graphic, action.duration, action.paramFloat);
                            if (isParentAction) alphaTween.OnComplete = ExecuteNextAct;
                            return;
                        }
                    }
                }
                if (isParentAction) ExecuteNextAct();
                break;
            case SequencerActionMode.SetScale:
                if (action.gameObject != null)
                {
                    if (action.duration <= 0)
                    {
                        action.gameObject.transform.localScale = action.position;
                    }
                    else
                    {
                        RezTween scaleTween = RezTween.ScaleTo(action.gameObject, action.duration, action.position);
                        if (isParentAction) scaleTween.OnComplete = ExecuteNextAct;
                        return;
                    }
                }
                if (isParentAction) ExecuteNextAct();
                break;
        }
    }

    IEnumerator WaitForNextFrame(Action action)
    {
        yield return new WaitForEndOfFrame();
        action();
    }

    // Update is called once per frame
    void Update () {
		
	}

    void Log(string text)
    {
        if (debugLog) Debug.Log("[Sequencer " + gameObject.name + "] " + text);
    }
}

public enum SequencerActionMode { Wait, MoveTo, PlayAnimation, InvokeFunction, Break, SetActive, SetAlpha, SetScale }

[Serializable]
public class SequencerAction
{
    public bool foldout = true;
    public bool active = true;
    public bool isRecording = false;
    public Color color;

    public SequencerActionMode mode;

    public GameObject gameObject;
    public GameObject lastGameObject;

    public Animator animator;
    public Animator lastAnimator;

    public int selectedMethod;
    public int selected;
    string[] selectOptions;
    
    public float duration = 1;
    public string target;

    public Vector3 position;
    public Vector3 positionBeforeRecording;

    public Component script;
    public string paramType;
    public string paramString;
    public float paramFloat;
    public int paramInt;
    public bool paramBool;

    public string GameObjectName
    {
        get
        {
            return gameObject == null ? "GameObject" : gameObject.name;
        }
    }

    public string SelectedValue
    {
        get
        {
            if (selectOptions == null || selectOptions.Length == 0) return "";
            if (selected < 0) selected = 0;
            else if (selected > selectOptions.Length - 1) selected = selectOptions.Length - 1;
            return selectOptions[selected];
        }
    }

    public string[] Options
    {
        get { return selectOptions; }
        set { selectOptions = value; }
    }

    public string ModeInfo
    {
        get
        {
            string info = mode.ToString();
            switch (mode)
            {
                case SequencerActionMode.Wait:
                    return "Wait for " + duration + " second(s)";
                case SequencerActionMode.MoveTo:
                    info = "Move to ";
                    if (gameObject != null) info = "Move " + gameObject.name + " to ";
                    return info + position + " in " + duration + " sec";
                case SequencerActionMode.PlayAnimation:
                    info = "Play animation";
                    if (!string.IsNullOrEmpty(target) && target.Length > 0) info += " " + target;
                    if (animator != null && animator.transform.parent != null) info += " from " + animator.gameObject.name + " (" + animator.transform.parent.name + ")";
                    return info;
                case SequencerActionMode.InvokeFunction:
                    info = string.Format(" from {0} ({1})", GameObjectName, script != null? script.GetType().Name : "Script");
                    object paramValue = GetParameterValue();
                    if (paramType == "string" && (paramValue as string).Length > 10)
                    {
                        paramValue = (paramValue as string).Substring(0, 8) + "...";
                    }
                    else if (paramType == "boolean") {
                        paramValue = paramValue.ToString().ToLower();
                    }
                    string fName = target == ""? "None (function)" : target + "(" + paramValue + ")";
                    return "Call " + fName + info;
                case SequencerActionMode.Break:
                    return "Sequence break";
                case SequencerActionMode.SetActive:
                    return string.Format("Set {0} active to {1}", GameObjectName, paramBool.ToString().ToLower());
                case SequencerActionMode.SetAlpha:
                    return string.Format("Set {0} alpha to {1}", GameObjectName, paramFloat);
                default:
                    return info;
            }
        }
    }

    public float PlayAnimation()
    {
        if (animator != null)
        {
            animator.Play(target);
            //Debug.Log("Length for " + target + ", " + animator.GetCurrentAnimatorClipInfo(0)[0].clip.length);
            return animator.GetCurrentAnimatorStateInfo(0).length;
        }
        return 0;
    }

    public object GetParameterValue()
    {
        object paramValue = null;
        switch (paramType)
        {
            case "int":
                paramValue = paramInt;
                break;
            case "string":
                paramValue = paramString;
                break;
            case "float":
                paramValue = paramFloat;
                break;
            case "boolean":
                paramValue = paramBool;
                break;
        }
        return paramValue;
    }

    public SequencerAction Copy()
    {
        SequencerAction action = new SequencerAction
        {
            animator = animator,
            duration = duration,
            gameObject = gameObject,
            mode = mode,
            paramBool = paramBool,
            paramFloat = paramFloat,
            paramInt = paramInt,
            paramString = paramString,
            paramType = paramType,
            position = position,
            script = script,
            selected = selected,
            target = target,
            selectedMethod = selectedMethod
        };
        return action;
    }

    public void Paste(SequencerAction action)
    {
        animator = action.animator;
        duration = action.duration;
        gameObject = action.gameObject;
        mode = action.mode;
        paramBool = action.paramBool;
        paramFloat = action.paramFloat;
        paramInt = action.paramInt;
        paramString = action.paramString;
        paramType = action.paramType;
        position = action.position;
        script = action.script;
        selected = action.selected;
        target = action.target;
        selectedMethod = action.selectedMethod;
        selectOptions = null;
    }
}

[Serializable]
public class SequencerActionParent : SequencerAction
{
    public List<SequencerAction> linkedActions = new List<SequencerAction>();
}

public class SequencerActionConditional : SequencerActionParent
{
    public string condition = "";
    public string compareMode = "equal";
}