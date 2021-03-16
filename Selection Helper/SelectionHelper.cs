#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace DreadScripts
{
    public class SelectionHelper
    {
        //By Dreadrith#3238
        //https://discord.gg/ZsPfrGn


        public static System.Type myCurrentType = System.Type.GetType(SessionState.GetString("SelectionHelperSelectType", ""));


        [MenuItem("CONTEXT/Component/[SH] Choose Type", false, 900)]
        static void selectType(MenuCommand selected)
        {
            myCurrentType = selected.context.GetType();
            SessionState.SetString("SelectionHelperSelectType", myCurrentType.AssemblyQualifiedName);
        }

        [MenuItem("GameObject/Selection Helper/Select Immediate Children", false, 50)]
        static void selectImmediate(MenuCommand selected)
        {
            Transform[] obj = Selection.GetFiltered<Transform>(SelectionMode.OnlyUserModifiable);
            if (obj.Length == 0)
            {
                Debug.Log("[SH] No GameObject was selected");
                return;
            }
            List<GameObject> newSelection = new List<GameObject>();
            for (int i = 0; i < obj.Length; i++)
            {
                for (int j = 0; j < obj[i].childCount; j++)
                {
                    newSelection.Add(obj[i].GetChild(j).gameObject);
                }
            }
            Selection.objects = Selection.objects.Concat(newSelection).ToArray();
        }

        [MenuItem("GameObject/Selection Helper/By Type/Filter", false, -50)]
        static void selectSelectedType()
        {
            Selection.objects = Selection.GetFiltered(myCurrentType, SelectionMode.OnlyUserModifiable);
            List<GameObject> newSelection = new List<GameObject>();
            for (int i = 0; i < Selection.objects.Length; i++)
            {
                newSelection.Add(((Component)Selection.objects[i]).gameObject);
            }
            Selection.objects = newSelection.ToArray();
        }

        [MenuItem("GameObject/Selection Helper/By Type/Children", false, -51)]
        static void selectChildrenType(MenuCommand selected)
        {
            selectByType(selected, true);
        }

        [MenuItem("GameObject/Selection Helper/By Type/Parents", false, -52)]
        static void selectParentsType(MenuCommand selected)
        {
            selectByType(selected, false);
        }



        static void selectByType(MenuCommand selected, bool child)
        {
            if (myCurrentType == null)
            {
                Debug.Log("[SH] No Component Type Chosen");
                return;
            }
            if (!selected.context)
            {
                Debug.Log("[SH] No GameObject was selected");
                return;
            }
            GameObject[] objs;
            if (child)
                objs = ((GameObject)selected.context).GetComponentsInChildren(myCurrentType, true).Select(c => c.gameObject).ToArray();
            else
                objs = ((GameObject)selected.context).GetComponentsInParent(myCurrentType, true).Select(c => c.gameObject).ToArray();

            Selection.objects = objs;

        }
    }
}
#endif