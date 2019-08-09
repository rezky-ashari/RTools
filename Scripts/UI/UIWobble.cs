using UnityEngine;

namespace RTools
{
    /// <summary>
    /// <para>Make a UI wobble by using RezTween.</para>
    /// Author: Rezky Ashari
    /// </summary>
    public class UIWobble : MonoBehaviour
    {
        [Tooltip("Scaling duration. Lower will make wobble faster.")]
        public float duration = 1f;

        [Tooltip("Whether to automatically start the wobble from the start")]
        public bool playOnAwake = true;

        [Tooltip("Delay before making this UI wobble. This will be ignored if Play On Awake is false.")]
        public float delay = 0.1f;

        float defaultScale;
        RezTween tween;

        // Use this for initialization
        void Start()
        {
            if (delay < 0.1f) delay = 0.1f;
            RezTween.DelayedCall(delay, StartWobble);
        }

        /// <summary>
        /// Start wobble.
        /// </summary>
        public void StartWobble()
        {
            RezTween.Destroy(ref tween);

            defaultScale = transform.localScale.x;
            tween = RezTween.ScaleTo(gameObject, duration, defaultScale + (defaultScale * 0.05f), RezTweenOptions.Repeat(), RezTweenOptions.Yoyo());
        }

        private void OnEnable()
        {
            if (tween != null)
                tween.Restart();
        }

        private void OnDisable()
        {
            if (tween != null)
                tween.Pause();
        }

        /// <summary>
        /// Set the default scale for this wobble.
        /// </summary>
        /// <param name="scale"></param>
        public void SetDefaultScale(float scale)
        {
            transform.localScale = new Vector3(scale, scale, scale);
            bool inPauseState = (tween != null && tween.paused);
            StartWobble();
            tween.paused = inPauseState;
        }
    }
}
