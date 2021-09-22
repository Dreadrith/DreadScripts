#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DreadScripts
{
    public class DuplicateWithMaterials : EditorWindow
    {
        public static GameObject target;
        public static string targetPath;
        public static bool separateShared;


        public static void CreateVariant(GameObject targetObject)
        {
            string folderPath = PlayerPrefs.GetString("DupeWithMatsPath", "Assets/DreadScripts/Duplicate With Materials/Generated Materials");
            bool separateSharedMaterials = PlayerPrefs.GetInt("DupeWithMatsSep", 1) == 1;
            Dictionary<Material, Material> matDict = new Dictionary<Material, Material>();
            ReadyPath(folderPath);

            string assetPath = "";
            string subFolderPath = "";

            GameObject dupeObj = Instantiate(targetObject);
            List<Renderer> myRenderers = dupeObj.GetComponentsInChildren<Renderer>(true).ToList();

            if (myRenderers.Count > 0)
            {
                assetPath = AssetDatabase.GenerateUniqueAssetPath(folderPath + "/" + targetObject.name);
                AssetDatabase.CreateFolder(folderPath, assetPath.Substring(assetPath.LastIndexOf('/') + 1, assetPath.Length - assetPath.LastIndexOf('/') - 1));

                for (int i = 0; i < myRenderers.Count; i++)
                {
                    if (!AssetDatabase.IsValidFolder(assetPath + "/" + myRenderers.ElementAt(i).gameObject.name + " Materials"))
                    {
                        AssetDatabase.CreateFolder(assetPath, myRenderers.ElementAt(i).gameObject.name + " Materials");
                        subFolderPath = assetPath + "/" + myRenderers.ElementAt(i).gameObject.name + " Materials";
                    }

                    Material[] newMaterials = myRenderers.ElementAt(i).sharedMaterials;
                    for (int j = 0; j < myRenderers.ElementAt(i).sharedMaterials.Length; j++)
                    {
                        if (!myRenderers[i].sharedMaterials[j])
                            continue;
                        if (!separateSharedMaterials && matDict.ContainsKey(myRenderers[i].sharedMaterials[j]))
                        {
                            newMaterials[j] = matDict[myRenderers[i].sharedMaterials[j]];
                            continue;
                        }
                        string matPath = AssetDatabase.GetAssetPath(myRenderers[i].sharedMaterials[j]);
                        if (PrefabUtility.IsPartOfModelPrefab(myRenderers[i].sharedMaterials[j]) || matPath.IndexOf("Assets") != 0)
                        {
                            Material instantiatedMaterial = Instantiate(myRenderers[i].sharedMaterials[j]);
                            string uniquePath = AssetDatabase.GenerateUniqueAssetPath(subFolderPath + "/" + myRenderers[i].sharedMaterials[j].name + ".mat");
                            AssetDatabase.CreateAsset(instantiatedMaterial, uniquePath);
                            newMaterials[j] = AssetDatabase.LoadAssetAtPath<Material>(uniquePath);
                        }
                        else
                        {
                            newMaterials[j] = CopyAssetAndReturn<Material>(matPath, AssetDatabase.GenerateUniqueAssetPath(subFolderPath + "/" + myRenderers[i].sharedMaterials[j].name + ".mat"));
                        }
                        if (!separateSharedMaterials)
                            matDict.Add(myRenderers[i].sharedMaterials[j], newMaterials[j]);
                    }
                    myRenderers[i].sharedMaterials = newMaterials;
                }
            }

            string[] subDirectories = System.IO.Directory.GetDirectories(assetPath);
            foreach (string s in subDirectories)
            {
                if (!System.IO.Directory.EnumerateFileSystemEntries(s).Any())
                {
                    FileUtil.DeleteFileOrDirectory(s);
                    FileUtil.DeleteFileOrDirectory(s + ".meta");
                }
            }

            AssetDatabase.Refresh();
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(assetPath));

        }

        [MenuItem("DreadTools/Utilities/Dupe With Mats")]
        public static void showWindow()
        {
            DuplicateWithMaterials win = GetWindow<DuplicateWithMaterials>(false, "Dupe With Mats", true);
            win.minSize = new Vector2(400, 120);
            win.maxSize = new Vector2(400, 120);
        }
        private void OnGUI()
        {
            target = (GameObject)EditorGUILayout.ObjectField("Target", target, typeof(GameObject), true);

            separateShared = EditorGUILayout.Toggle(new GUIContent("Separate Shared Materials", "Force each material slot to have its own material."), separateShared);

            if (GUILayout.Button("Create Variant", "toolbarbutton"))
                CreateVariant(target);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            AssetFolderPath(ref targetPath, "New Materials Path", "DupeWithMatsPath");
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Made By Dreadrith#3238","boldlabel"))
                    Application.OpenURL("https://linktr.ee/Dreadrith");
            }
        }

        private void OnEnable()
        {
            targetPath = PlayerPrefs.GetString("DupeWithMatsPath", "DreadScripts/Duplicate With Materials/Generated Materials");
        }

        private static void ReadyPath(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
        private static T CopyAssetAndReturn<T>(string path, string newpath) where T : Object
        {
            if (path != newpath)
                AssetDatabase.CopyAsset(path, newpath);
            return AssetDatabase.LoadAssetAtPath<T>(newpath);

        }
        private static void AssetFolderPath(ref string variable,string title, string playerpref)
        {
            using (new GUILayout.HorizontalScope())
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField(title, variable);
                EditorGUI.EndDisabledGroup();
                if (GUILayout.Button("...", GUILayout.Width(30)))
                {
                    var dummyPath = EditorUtility.OpenFolderPanel(title, AssetDatabase.IsValidFolder(variable) ? variable : string.Empty, string.Empty);
                    if (string.IsNullOrEmpty(dummyPath))
                        return;

                    if (!dummyPath.StartsWith("Assets"))
                    {
                        Debug.LogWarning("New Path must be a folder within Assets!");
                        return;
                    }

                    variable = FileUtil.GetProjectRelativePath(dummyPath);
                    PlayerPrefs.SetString(playerpref, variable);
                }
            }
        }

    }
}
#endif