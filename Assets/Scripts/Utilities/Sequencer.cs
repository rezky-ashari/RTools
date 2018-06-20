using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sequencer : MonoBehaviour {

    public bool playOnAwake = true;
    public List<SequencerActionParent> actions = new List<SequencerActionParent>();
    public int currentAct = 0;

	// Use this for initialization
	void Start () {
        if (playOnAwake) ExecuteNextAct();
	}

    void ExecuteNextAct()
    {
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
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}

public enum SequencerActionMode { Wait, MoveTo, PlayAnimation }

[System.Serializable]
public class SequencerAction
{
    public bool foldout = true;
    public bool active = true;

    public SequencerActionMode mode;
    public GameObject gameObject;
    public Animator animator;
    public float duration;
    public string name;
    public Vector3 position;

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
                    if (!string.IsNullOrEmpty(name) && name.Length > 0) info += " '" + name + "'";
                    return info;
                default:
                    return info;
            }
        }

    }

    public float PlayAnimation()
    {
        animator.Play(name);
        return animator.GetCurrentAnimatorStateInfo(0).length;
    }
}

[System.Serializable]
public class SequencerActionParent : SequencerAction
{
    public List<SequencerAction> linkedActions = new List<SequencerAction>();
}