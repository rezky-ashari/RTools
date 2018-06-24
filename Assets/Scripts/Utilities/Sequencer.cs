using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <para>Execute sequence of actions in lazy way.</para>
/// Author: Rezky Ashari
/// </summary>
public class Sequencer : MonoBehaviour {

    public bool playOnAwake = true;
    public List<SequencerActionParent> actions = new List<SequencerActionParent>();
    public int currentAct = 0;
    public bool isPlaying = false;

	// Use this for initialization
	void Start () {
        if (playOnAwake) Play();
	}

    public void Play()
    {
        if (!isPlaying)
        {
            isPlaying = true;
            ExecuteNextAct();
        }        
    }

    public void Play(int startingAct)
    {
        currentAct = startingAct;
        Play();
    }

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
            Execute(action);
            for (int i = 0; i < action.linkedActions.Count; i++)
            {
                Execute(action.linkedActions[i]);
            }
        }
    }

    void Execute(SequencerAction action)
    {
        if (!action.active)
        {
            ExecuteNextAct();
            return;
        }
        switch (action.mode)
        {
            case SequencerActionMode.Wait:
                RezTween.DelayedCall(action.duration, ExecuteNextAct);
                break;
            case SequencerActionMode.MoveTo:
                RezTween.MoveTo(action.gameObject, action.duration, action.position).OnComplete = ExecuteNextAct;
                break;
            case SequencerActionMode.PlayAnimation:
                float animLength = action.PlayAnimation();
                RezTween.DelayedCall(animLength, ExecuteNextAct);
                break;
            case SequencerActionMode.InvokeFunction:
                //action.script.Invoke(action.target, 0);
                var method = action.script.GetType().GetMethod(action.target);
                object paramValue = null;
                switch (action.paramType)
                {
                    case "int":
                        paramValue = action.paramInt;
                        break;
                    case "string":
                        paramValue = action.paramString;
                        break;
                    case "float":
                        paramValue = action.paramFloat;
                        break;
                }
                if (paramValue != null)
                {
                    method.Invoke(action.script, new object[] { paramValue });
                }
                else
                {
                    method.Invoke(action.script, null);
                }
                ExecuteNextAct();
                break;
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}

public enum SequencerActionMode { Wait, MoveTo, PlayAnimation, InvokeFunction }

[System.Serializable]
public class SequencerAction
{
    public bool foldout = true;
    public bool active = true;
    public bool isRecording = false;

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

    public MonoBehaviour script;
    public string paramType;
    public string paramString;
    public float paramFloat;
    public int paramInt;

    public string SelectedValue
    {
        get
        {
            if (selectOptions == null) return "";
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
                    info = "Play Animation";
                    if (!string.IsNullOrEmpty(target) && target.Length > 0) info += " " + target;
                    return info;
                case SequencerActionMode.InvokeFunction:
                    return "Call function " + target;
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
            return animator.GetCurrentAnimatorStateInfo(0).length;
        }
        return 0;
    }
}

[System.Serializable]
public class SequencerActionParent : SequencerAction
{
    public List<SequencerAction> linkedActions = new List<SequencerAction>();
}

