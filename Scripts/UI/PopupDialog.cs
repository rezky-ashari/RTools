﻿using UnityEngine;
using UnityEngine.UI;

namespace RTools
{
    public class PopupDialog : MonoBehaviour
    {
        public GameObject container;
        public Text dialogText;

        private Image Overlay
        {
            get
            {
                if (_overlay == null)
                {
                    _overlay = GetComponent<Image>();
                }
                return _overlay;
            }
        }
        private Image _overlay;

        bool isShowing = false;

        public bool IsVisible
        {
            get
            {
                return isShowing;
            }
        }

        // Start is called before the first frame update
        protected virtual void Start()
        {
            Overlay.enabled = false;
            container.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public virtual void Show(string text)
        {
            dialogText.text = text;
            Show();
        }

        public virtual void Show()
        {
            if (!isShowing)
            {
                container.SetActive(true);
                RezTween.ScaleFromTo(container, 0.5f, 0, 1, RezTweenEase.SPRING).OnComplete = () =>
                Overlay.enabled = true;

                isShowing = true;
            }
        }

        public virtual void Hide()
        {
            if (isShowing)
            {
                RezTween.ScaleTo(container, 0.5f, 0, RezTweenEase.BACK_IN).OnComplete = () =>
                {
                    Overlay.enabled = false;
                    container.SetActive(false);
                };

                isShowing = false;
            }
        }

        public void OnConfirm()
        {

        }

        [ContextMenu("Toggle Visibility")]
        void ToggleVisibility()
        {
            bool show = !Overlay.enabled;
            Overlay.enabled = show;
            container.SetActive(show);
        }
    }

}