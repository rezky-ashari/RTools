using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RTools
{
    /// <summary>
    /// Mode for selectable group.
    /// </summary>
    public enum SelectableGroupMode
    {
        SwapSprite, TintColor
    }

    /// <summary>
    /// <para>Grouping a list of game buttons.</para>
    /// Author: Rezky Ashari
    /// </summary>
    public class SelectableGroup : MonoBehaviour
    {

        public Image current;
        public SelectableGroupMode mode;
        public Sprite defaultSprite;
        public Sprite selectedSprite;
        public Color defaultColor = Color.white;
        public Color selectedColor = Color.white;

        // Use this for initialization
        void Start()
        {
            if (current != null) current.sprite = selectedSprite;
        }

        /// <summary>
        /// Set a game button as selected.
        /// </summary>
        /// <param name="gameButton">Target game button</param>
        public void SetSelected(Image gameButton)
        {
            if (current != null)
            {
                if (mode == SelectableGroupMode.SwapSprite)
                {
                    current.sprite = defaultSprite;
                }
                else
                {
                    current.color = defaultColor;
                }
            }

            current = gameButton;

            if (mode == SelectableGroupMode.SwapSprite)
            {
                current.sprite = selectedSprite;
            }
            else
            {
                current.color = selectedColor;
            }
        }

        /// <summary>
        /// Set a game button as selected.
        /// </summary>
        /// <param name="gameButton">Target GameButton</param>
        public void SetSelected(GameButton gameButton)
        {
            SetSelected(gameButton.GetComponent<Image>());
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SelectableGroup))]
    public class SelectableGroupEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            SelectableGroup selectableGroup = (SelectableGroup)target;

            selectableGroup.current = (Image)EditorGUILayout.ObjectField("Current", selectableGroup.current, typeof(Image), true);

            selectableGroup.mode = (SelectableGroupMode)EditorGUILayout.EnumPopup("Mode", selectableGroup.mode);

            if (selectableGroup.mode == SelectableGroupMode.SwapSprite)
            {
                selectableGroup.defaultSprite = (Sprite)EditorGUILayout.ObjectField("Normal", selectableGroup.defaultSprite, typeof(Sprite), true);
                selectableGroup.selectedSprite = (Sprite)EditorGUILayout.ObjectField("Selected", selectableGroup.selectedSprite, typeof(Sprite), true);
            }
            else
            {
                selectableGroup.defaultColor = EditorGUILayout.ColorField("Normal", selectableGroup.defaultColor);
                selectableGroup.selectedColor = EditorGUILayout.ColorField("Selected", selectableGroup.selectedColor);
            }
        }
    }
#endif
}