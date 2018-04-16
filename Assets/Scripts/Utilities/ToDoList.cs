using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ToDoList", menuName = "To Do List")]
public class ToDoList : ScriptableObject {

    public List<Item> list;

    [System.Serializable]
    public class Item
    {
        public bool isDone;
        public string task;
    }
}