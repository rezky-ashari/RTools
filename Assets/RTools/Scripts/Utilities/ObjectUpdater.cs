using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTools
{
    /// <summary>
    /// <para>Utility to update a GameObject over time.</para>
    /// Author: Rezky Ashari
    /// </summary>
    public class ObjectUpdater : MonoBehaviour
    {
        [Tooltip("Position values to update")]
        public Vector3 position;

        [Tooltip("Rotation values to update")]
        public Vector3 rotation;

        [Tooltip("Whether to use frame rate or constant update over time")]
        public bool useFrameRate = false;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            float speed = useFrameRate ? 1 : Time.deltaTime;
            transform.Rotate(rotation * speed);
            transform.localPosition += position * speed;
        }
    }
}
