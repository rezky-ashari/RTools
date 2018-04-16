using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class SpriteLoader : MonoBehaviour {

    public string resourceDirectory;
    public List<string> resourceDirectories = new List<string>();
    public int index;
    public string itemName;
    public string position;

    [Tooltip("Only show files with certain filename")]
    public string filter;

    public List<string> fileNames = new List<string>();

    int lastIndex = -1;
    string[] lastResourceDirectories;

    private void Reset()
    {
        
    }

    void ListDir()
    {
        if (!string.IsNullOrEmpty(resourceDirectory))
        {
            fileNames = new List<string>();
            Sprite[] files = Resources.LoadAll<Sprite>(resourceDirectory);
            for (int i = 0; i < files.Length; i++)
            {
                fileNames.Add(files[i].name);
            }
        }
    }

    void ListDirectories()
    {
        if (resourceDirectories.Count > 0)
        {
            fileNames = new List<string>();
            for (int i = 0; i < resourceDirectories.Count; i++)
            {
                Sprite[] files = Resources.LoadAll<Sprite>(resourceDirectories[i]);
                for (int j = 0; j < files.Length; j++)
                {
                    fileNames.Add(resourceDirectories[i] + "/" + files[j].name);
                }
            }
        }
    }

    // Use this for initialization
    void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
        if (lastIndex != index)
        {
            if (fileNames == null || fileNames.Count == 0) return;

            index = (int)Mathf.Repeat(index, fileNames.Count);
            itemName = Path.GetFileName(fileNames[index]);
            Image img = GetComponent<Image>();
            string assetPath = /*resourceDirectory + "/" + itemName*/ fileNames[index];
            img.sprite = Resources.Load<Sprite>(assetPath);
            img.SetNativeSize();
            lastIndex = index;
        }
#if UNITY_EDITOR
        position = transform.localPosition.x + ", " + transform.localPosition.y;
#endif
    }

    public void UpdateList()
    {
        if (resourceDirectory != "")
        {
            resourceDirectories.Add(resourceDirectory);
            resourceDirectory = "";
        } 
        if (!IsEquals(resourceDirectories, lastResourceDirectories))
        {
            lastIndex = -1;
            index = 0;
            ListDirectories();
            lastResourceDirectories = resourceDirectories.ToArray();
        }
        Update();
    }

    bool IsEquals(List<string> a1, string[] a2)
    {
        if (a1 == null || a2 == null) return false;
        if (a1.Count != a2.Length) return false;
        for (int i = 0; i < a1.Count; i++)
        {
            if (i < a2.Length)
            {
                if (a1[i] != a2[i]) return false;
            }
        }
        return true;
    }

    public void Show(string spriteName)
    {
        index = Array.FindIndex(fileNames.ToArray(), x => x.Contains(spriteName));
        if (index == -1)
        {
            Debug.Log("Can't find " + spriteName + " in filename list.");
            index = 0;
        }
    }

    public void Show(int index)
    {
        this.index = index;
    }
}
