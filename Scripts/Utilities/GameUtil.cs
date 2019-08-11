using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RTools
{
    /// <summary>
    /// <para>Collection of utility methods.</para>
    /// Author: Rezky Ashari
    /// </summary>
    public class GameUtil
    {
        /// <summary>
        /// Main camera reference. Stored for faster access (better than <code>Camera.main</code>).
        /// </summary>
        public static Camera MainCamera
        {
            get
            {
                if (_mainCamera == null) _mainCamera = Camera.main;
                return _mainCamera;
            }
        }
        static Camera _mainCamera;

        /// <summary>
        /// Destroy a gameObject. Use destroy immediate on editor and normal destroy when playing.
        /// Fail silently if the gameObject is null. 
        /// </summary>
        /// <param name="gameObject"></param>
        public static void Destroy(GameObject gameObject)
        {
            if (gameObject == null) return;
            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(gameObject);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        /// <summary>
        /// Destroy Children of a game object.
        /// </summary>
        /// <param name="gameObject">Parent gameObject</param>
        public static void DestroyChildren(GameObject gameObject)
        {
            DestroyChildren(gameObject.transform);
        }

        /// <summary>
        /// Destroy children of a game object.
        /// </summary>
        /// <param name="transform"></param>
        public static void DestroyChildren(Transform transform)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Quick way to set local position properties.
        /// </summary>
        /// <param name="transform">Target transform</param>
        /// <param name="properties">Vector3 value in string format, example: 'x:10', 'y:-2', 'z:100'</param>
        public static void SetLocalPosition(Transform transform, params string[] properties)
        {
            Vector3 position = transform.localPosition;
            foreach (string prop in properties)
            {
                string[] propData = prop.Split(':');
                float value = float.Parse(propData[1]);
                switch (propData[0])
                {
                    case "x":
                        position.x = value;
                        break;
                    case "y":
                        position.y = value;
                        break;
                    case "z":
                        position.z = value;
                        break;
                }
            }
            transform.localPosition = position;
        }

        /// <summary>
        /// Quick way to set local position properties.
        /// </summary>
        /// <param name="gameObject">Target gameObject</param>
        /// <param name="properties">Vector3 property and value in string format, example: 'x:10', 'y:-2', 'z:100'</param>
        public static void SetLocalPosition(GameObject gameObject, params string[] properties)
        {
            SetLocalPosition(gameObject.transform, properties);
        }

        /// <summary>
        /// Quick way to set localPosition of a transform.
        /// </summary>
        public static void SetLocalPosition(Transform transform, float? x = null, float? y = null, float? z = null)
        {
            Vector3 localPosition = transform.localPosition;
            if (x.HasValue) localPosition.x = (float)x;
            if (y.HasValue) localPosition.y = (float)y;
            if (z.HasValue) localPosition.z = (float)z;
            transform.localPosition = localPosition;
        }

        /// <summary>
        /// Quick way to set localPosition of a GameObject.
        /// </summary>
        public static void SetLocalPosition(GameObject gameObject, float? x = null, float? y = null, float? z = null)
        {
            SetLocalPosition(gameObject.transform, x, y, z);
        }

        /// <summary>
        /// Quick way to set localPosition of a MonoBehaviour.
        /// </summary>
        public static void SetLocalPosition(MonoBehaviour monoBehaviour, float? x = null, float? y = null, float? z = null)
        {
            SetLocalPosition(monoBehaviour.transform, x, y, z);
        }

        /// <summary>
        /// Set image alpha. 
        /// </summary>
        /// <param name="image">Target image</param>
        /// <param name="alpha">Desired alpha</param>
        public static void SetImageAlpha(Image image, float alpha)
        {
            Color imageColor = image.color;
            imageColor.a = alpha;
            image.color = imageColor;
        }

        public static GameObject ExpandHitArea(GameObject gameObject, float expandValue = 50)
        {
            GameObject hitArea = gameObject.GetChild("GameUtil HitArea");

            if (hitArea == null)
            {
                hitArea = new GameObject("GameUtil HitArea");
                Image image = hitArea.AddComponent<Image>();
                image.sprite = gameObject.GetComponent<Image>().sprite;
                image.SetNativeSize();
                SetImageAlpha(image, 0);
                Rect rect = image.rectTransform.rect;
                image.rectTransform.sizeDelta = new Vector2(rect.width + expandValue, rect.height + expandValue);
                hitArea.SetParent(gameObject, false);
            }


            return hitArea;
        }

        /// <summary>
        /// Get a random value from a collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T GetRandomValue<T>(T[] list)
        {
            return list[UnityEngine.Random.Range(0, list.Length)];
        }

        /// <summary>
        /// Get a random value from the given values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T GetRandomFrom<T>(params T[] list)
        {
            return GetRandomValue<T>(list);
        }

        public static float RandomRange(float min, float max)
        {
            return UnityEngine.Random.Range(min, max);
        }
    }
}
