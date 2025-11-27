using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class TagSetter : EditorWindow
{
    [MenuItem("Tools/Set Tag for gameobject and it's children gameobject")]
    static void SetTagRecursively()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        
        foreach (GameObject obj in selectedObjects)
        {
            SetTagForObjectAndChildren(obj, obj.tag);
        }
    }
    
    static void SetTagForObjectAndChildren(GameObject parent, string tagName)
    {
        parent.tag = tagName;
        
        Transform[] children = parent.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            child.gameObject.tag = tagName;
        }
        
        Debug.Log($"set target of {parent.name} and {children.Length} children gameobject to {tagName}");
    }
}