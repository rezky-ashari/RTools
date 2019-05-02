using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTools
{
    /// <summary>
    /// <para>Click effect. Please attach to a gameObject with ParticleSystem set.</para>
    /// Author: Rezky Ashari
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public class ClickEffect : MonoBehaviour
    {
        static ClickEffect instance;

        ParticleSystem particle;

        // Use this for initialization
        void Start()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
            particle = GetComponent<ParticleSystem>();
            particle.Stop();
        }

        // Update is called once per frame
        void Update()
        {
            transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (Input.GetMouseButtonDown(0))
            {
                particle.Play();
            }
            if (Input.GetMouseButtonUp(0))
            {
                particle.Stop();
            }
        }
    }
}
