﻿using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace RTools
{
    /// <summary>
    /// <para>For updating dots after a Loading Text.</para>
    /// Author: Rezky Ashari
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class LoadingText : MonoBehaviour
    {
        public string baseText;
        public float updateRate = 0.5f;

        Text _textComponent;
        Text TextComponent
        {
            get
            {
                if (_textComponent == null) _textComponent = GetComponent<Text>();
                return _textComponent;
            }
        }

        int dotCount = 0;
        float passedTime = 0;

        private void Reset()
        {
            baseText = TextComponent.text;
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (passedTime >= updateRate)
            {
                StringBuilder sb = new StringBuilder(baseText);
                dotCount++;
                if (dotCount > 3) dotCount = 0;
                for (int i = 0; i < dotCount; i++)
                {
                    sb.Append('.');
                }
                TextComponent.text = sb.ToString();
                passedTime = 0;
            }
            else
            {
                passedTime += Time.deltaTime;
            }
        }
    }
}
