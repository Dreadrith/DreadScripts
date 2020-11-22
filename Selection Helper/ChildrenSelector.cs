#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class ChildrenSelector : Editor
{
    //By Dreadrith#3238
    //https://discord.gg/ZsPfrGn

    static System.Type myCurrentType;

    [MenuItem("CONTEXT/Component/[SH] Choose Type", false, 900)]
    static void selectType(MenuCommand selected)
    {
        myCurrentType = selected.context.GetType();
    }
    
    [MenuItem("GameObject/Selection Helper/Select Immediate Children", false, -10)]
    static void selectImmediate(MenuCommand selected)
    {
        if (!selected.context)
        {
            Debug.Log("[SH] No GameObject was actively selected");
            return;
        }
        Transform parent = ((GameObject)selected.context).transform;
        if (parent.childCount > 0)
        {
            GameObject[] children = new GameObject[parent.childCount];
            for (int i = 0; i < parent.childCount; i++)
            {
                children[i] = parent.GetChild(i).gameObject;
            }
            Selection.objects = Selection.objects.Concat(children).ToArray();
        }
    }

    [MenuItem("GameObject/Selection Helper/Select Children By Type", false, -10)]
    static void selectChildrenOverwrite(MenuCommand selected)
    {
        selectChildren(selected, true);
    }

    [MenuItem("GameObject/Selection Helper/Select Children By Type (ADD)", false, -10)]
    static void selectChildrenAddition(MenuCommand selected)
    {
        selectChildren(selected, false);
    }

    static void selectChildren(MenuCommand selected,bool overwrite)
    {
        if (myCurrentType == null)
        {
            Debug.Log("[SH] No Component Type Chosen");
            return;
        }
        if (!selected.context)
        {
            Debug.Log("[SH] No GameObject was actively selected");
            return;
        }
        Component[] comps = ((GameObject)selected.context).GetComponentsInChildren(myCurrentType, true);
        GameObject[] objs = new GameObject[comps.Length];
        for (int i = 0; i < comps.Length; i++)
            objs[i] = comps[i].gameObject;
        if (!overwrite)
            Selection.objects = Selection.objects.Concat(objs).ToArray();
        else
            Selection.objects = objs;
    }




}
#endif