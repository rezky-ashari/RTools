using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RTools
{
    public class OptionMenu : MonoBehaviour
    {

        /// <summary>
        /// Game's main scene.
        /// </summary>
        public string homeScene = "MainMenu";

        /// <summary>
        /// Current option menu instance.
        /// </summary>
        public static OptionMenu Instance
        {
            get { return _instance; }
        }
        static OptionMenu _instance;

        public GameObject settingButton;
        public GameObject soundButton;
        public GameObject homeButton;

        [Tooltip("Whether to show ads when return to home.")]
        public bool showAd = false;

        const string settingButtonID = "Setting";
        const string soundButtonID = "Sound";
        const string homeButtonID = "Home";

        float buttonSpacing;
        bool isShown = false;

        /// <summary>
        /// Called when option menu state changed.
        /// Will pass value <code>true</code> when all menu is shown, otherwise <code>false</code>.
        /// IMPORTANT: Don't forget to unsubscribe to this event when you're done using it.
        /// </summary>
        public static event Action<bool> OnStateChanged;

        /// <summary>
        /// Gets or Sets option menu's visible state.
        /// </summary>
        public static bool Visible
        {
            get
            {
                if (Instance == null) return false;
                return Instance.gameObject.activeInHierarchy;
            }
            set
            {
                if (Instance != null)
                {
                    Instance.gameObject.SetActive(value);
                }
            }
        }

        // Use this for initialization
        void Start()
        {

            if (_instance == null)
            {
                AssignOnClickListener(settingButton, ToggleSettingView);
                AssignOnClickListener(soundButton, ToggleSoundState);
                AssignOnClickListener(homeButton, BackToHome);

                homeButton.transform.SetAsFirstSibling();
                settingButton.transform.SetAsLastSibling();

                DontDestroyOnLoad(GetComponentInParent<Canvas>().gameObject);
                _instance = this;

                buttonSpacing = settingButton.transform.ToRectTransform().rect.width * settingButton.transform.localScale.x + 10;
                SceneSwitchR.OnSceneChanged += SceneSwitchR_OnSceneChanged;

                Init();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void AssignOnClickListener(GameObject settingButton, UnityAction listener)
        {
            settingButton.GetComponent<Button>().onClick.AddListener(listener);
        }

        private void SceneSwitchR_OnSceneChanged()
        {
            Init();
        }

        public void Init()
        {
            settingButton.SetActive(SceneSwitchR.CurrentSceneName != homeScene);
            RezTween.MoveTo(soundButton, 0f, settingButton.transform.localPosition);
            RezTween.MoveTo(homeButton, 0f, settingButton.transform.localPosition);
            isShown = false;
        }

        public void HandleClick(string id)
        {
            switch (id)
            {
                case settingButtonID:
                    ToggleSettingView();
                    break;
                case soundButtonID:
                    ToggleSoundState();
                    break;
                case homeButtonID:
                    BackToHome();
                    break;
            }
            //Resound.PlaySFX(Playlist.SFX_CLICK);
        }

        private void ToggleSoundState()
        {
            Resound.ToggleMute();
            soundButton.GetComponentInChild<ImageSwitcher>(0).ActiveImage = Resound.IsMute ? 1 : 0;
        }

        private void ToggleSettingView()
        {
            if (isShown) HideOptions();
            else ShowOptions();

            if (OnStateChanged != null) OnStateChanged(isShown);
        }

        private void BackToHome()
        {
            SceneSwitchR.To(homeScene, showAd);
        }

        private void HideOptions()
        {
            if (isShown)
            {
                RezTween.MoveTo(soundButton, 0.3f, settingButton.transform.localPosition);
                RezTween.MoveTo(homeButton, 0.3f, settingButton.transform.localPosition);
                isShown = false;
            }
        }

        public void ShowOptions()
        {
            if (!isShown)
            {
                RezTween.MoveBy(soundButton, 0.3f, "y:-" + buttonSpacing, RezTweenEase.BACK_OUT);
                RezTween.MoveBy(homeButton, 0.3f, "y:-" + (buttonSpacing * 2), RezTweenEase.BACK_OUT);
                isShown = true;
            }
        }

        public static void Show()
        {
            Instance.gameObject.SetActive(true);
        }

        public static void Hide()
        {
            Instance.gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
