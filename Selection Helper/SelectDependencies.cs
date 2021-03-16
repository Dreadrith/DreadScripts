#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace DreadScripts
{
    public class SelectDependencies
    {
        //By Dreadrith#3238
        //https://discord.gg/ZsPfrGn

        [MenuItem("Assets/Selection Helper/Select Dependencies/No Scripts", false, -10)]
        private static void selectDependenciesSript()
        {
            selectDependencies(new string[] { ".cs", ".dll" });
        }

        [MenuItem("Assets/Selection Helper/Select Dependencies/No Shaders", false, -10)]
        private static void selectDependenciesShader()
        {
            selectDependencies(new string[] { ".shader" });
        }

        [MenuItem("Assets/Selection Helper/Select Dependencies/No Shaders or Scripts", false, -10)]
        private static void selectDependenciesNeither()
        {
            selectDependencies(new string[] { ".cs", ".dll", ".shader" });
        }

        [MenuItem("Assets/Selection Helper/Select Dependencies/No Rule", false, -10)]
        private static void selectDependencies()
        {
            selectDependencies(null);
        }
        private static void selectDependencies(string[] ignoreExt = null)
        {
            Object[] myAssets = Selection.GetFiltered<Object>(SelectionMode.Assets);
            HashSet<Object> files = new HashSet<Object>();
            foreach (Object obj in myAssets)
                foreach (string path in AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(obj)))
                {
                    if (ignoreExt != null)
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
                    else
                        files.Add(AssetDatabase.LoadAssetAtPath<Object>(path));
                }
            Selection.objects = files.ToArray();
        }
    }
}
#endif
