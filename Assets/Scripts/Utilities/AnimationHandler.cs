using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Animator))]
public class AnimationHandler : MonoBehaviour, IPointerClickHandler
{
    /// <summary>
    /// Animation state to play when clicked.
    /// </summary>
    public string onClickPlay;
    /// <summary>
    /// ID to send to the scene's click event.
    /// </summary>
    public string clickID;
    public UnityEvent onStart;
    public UnityEvent onComplete;

    Animator _animator;
    public Animator Animator
    {
        get
        {
            if (_animator == null) _animator = GetComponent<Animator>();
            return _animator;
        }
    }

    // Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void PlaySound(string name)
    {
        if (name.Contains("|"))
        {
            string[] names = name.Split('|');
            Resound.PlaySFX(names[Random.Range(0, names.Length)].Trim());
        }
        else if (name.Contains(","))
        {
            string[] names = name.Split(',');
            for (int i = 0; i < names.Length; i++)
            {
                Resound.PlaySFX(names[i].Trim());
            }
        }
        else
        {
            Resound.PlaySFX(name);
        }
        
    }

    void PlaySoundLoop(string name)
    {
        Resound.PlaySFX(name, true);
    }

    void StopAllSound()
    {
        Resound.StopSFX();
    }

    void StopSoundLoop(string name)
    {
        Resound.StopSFX(name);
    }

    void OnStartAnimation()
    {
        onStart.Invoke();
    }

    void OnCompleteAnimation()
    {
        onComplete.Invoke();
    }

    /// <summary>
    /// Play an animation state.
    /// </summary>
    /// <param name="stateName"></param>
    public void Play(string stateName)
    {
        Animator.Play(stateName);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!string.IsNullOrEmpty(onClickPlay)) Animator.Play(onClickPlay);
        if (!string.IsNullOrEmpty(clickID)) Scene.SendClickEvent(clickID);
    }

    public void SendSceneEvent(string eventName)
    {
        Scene.SendEvent(eventName);
    }
}
