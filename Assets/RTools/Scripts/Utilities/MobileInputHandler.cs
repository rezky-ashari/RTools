using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace RTools
{
    /// <summary>
    /// <para>Handle Android back button.</para>
    /// Author: Rezky Ashari
    /// </summary>
    public class MobileInputHandler : MonoBehaviour
    {
        [Tooltip("Whether to exit application when back button was pressed")]
        public bool autoExit = true;
        public UnityEvent onAndroidBackKeyPressed;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                OnPressBackKey();
            }
        }

        public void OnPressBackKey()
        {
            onAndroidBackKeyPressed.Invoke();
            if (autoExit) Application.Quit();
        }
    }
}
