#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;


public class SelectDependencies : EditorWindow
{
    //By Dreadrith#3238
    //https://discord.gg/ZsPfrGn

    [MenuItem("Assets/Selection Helper/Select Dependencies/No Shaders or Scripts", false, -10)]
   private static void selectDependenciesNeither()
    {
        Object[] myAssets = Selection.GetFiltered<Object>(SelectionMode.Assets);
        HashSet<Object> files = new HashSet<Object>();
        string[] ignoreExt = {".cs",".shader",".dll" };
        foreach (Object obj in myAssets)
           foreach(string path in AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(obj)))
            {
                bool ignore = false;
                string myExt = path.Substring(path.LastIndexOf('.'),path.Length- path.LastIndexOf('.'));
                foreach (string ext in ignoreExt)
                    if (myExt == ext)
                    {
                        ignore = true;
                        break;
                    }

                if (!ignore)
                    files.Add(AssetDatabase.LoadAssetAtPath<Object>(path));
            }
        Selection.objects = files.ToArray();
    }

    [MenuItem("Assets/Selection Helper/Select Dependencies/No Shaders", false, -10)]
    private static void selectDependenciesShader()
    {
        Object[] myAssets = Selection.GetFiltered<Object>(SelectionMode.Assets);
        HashSet<Object> files = new HashSet<Object>();
        string[] ignoreExt = {".shader" };
        foreach (Object obj in myAssets)
            foreach (string path in AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(obj)))
            {
                bool ignore = false;
                string myExt = path.Substring(path.LastIndexOf('.'), path.Length - path.LastIndexOf('.'));
                foreach (string ext in ignoreExt)
                    if (myExt == ext)
                    {
                        ignore = true;
                        break;
                    }
                if (!ignore)
                    files.Add(AssetDatabase.LoadAssetAtPath<Object>(path));
            }
        Selection.objects = files.ToArray();
    }

    [MenuItem("Assets/Selection Helper/Select Dependencies/No Scripts", false, -10)]
    private static void selectDependenciesSript()
    {
        Object[] myAssets = Selection.GetFiltered<Object>(SelectionMode.Assets);
        HashSet<Object> files = new HashSet<Object>();
        string[] ignoreExt = { ".cs", ".dll" };
        foreach (Object obj in myAssets)
            foreach (string path in AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(obj)))
            {
                bool ignore = false;
                string myExt = path.Substring(path.LastIndexOf('.'), path.Length - path.LastIndexOf('.'));
                foreach (string ext in ignoreExt)
                    if (myExt == ext)
                    {
                        ignore = true;
                        break;
                    }

                if (!ignore)
                    files.Add(AssetDatabase.LoadAssetAtPath<Object>(path));
            }
        Selection.objects = files.ToArray();
    }
}
#endif
