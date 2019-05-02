using UnityEngine;

namespace RTools
{
    public class CanvasScalerConfig : ScriptableData<CanvasScalerConfig>
    {
        public enum MatchMode { Portrait, Landscape, Custom }

        public RenderMode renderMode = RenderMode.ScreenSpaceOverlay;
        public float referenceWidth = 1024;
        public float referenceHeight = 720;
        public MatchMode matchMode;

        [Range(0, 1)]
        public float match = 1;

        public Vector2 ReferenceResolution
        {
            get
            {
                return new Vector2(referenceWidth, referenceHeight);
            }
        }
    }
}
