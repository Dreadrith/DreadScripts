#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
#if DS_HASRESOURCES
using DS_CommonMethods;
#endif

namespace DreadScripts
{
    public class DuplicateWithMaterials : EditorWindow
    {
        public static GameObject target;
        public static string targetPath;
        public static bool separateShared;

#if DS_HASRESOURCES
        public static void CreateVariant(GameObject targetObject, string folderPath,bool separateSharedMaterials)
        {
            folderPath = PlayerPrefs.GetString("DupeWithMatsPath", "Assets/DreadScripts/Duplicate With Materials/Generated Materials");
            separateSharedMaterials = PlayerPrefs.GetInt("DupeWithMatsSep", 1) == 1;
            Dictionary<Material, Material> matDict = new Dictionary<Material, Material>();

            DSCommonMethods.RecreateFolders(folderPath);

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
                            newMaterials[j] = DSCommonMethods.CopyAssetAndReturn<Material>(matPath, AssetDatabase.GenerateUniqueAssetPath(subFolderPath + "/" + myRenderers[i].sharedMaterials[j].name + ".mat"));
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
            target = target.QuickField(true);

            separateShared = EditorGUILayout.Toggle(new GUIContent("Separate Shared Materials", "Force each material slot to have its own material."), separateShared);

            if (GUILayout.Button("Create Variant", "toolbarbutton"))
                CreateVariant(target,targetPath,separateShared);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            DSCommonMethods.AssetFolderPath(ref targetPath, "New Materials Path", "DupeWithMatsPath");
            DSCommonMethods.Credit();
        }

        private void OnEnable()
        {
            targetPath = PlayerPrefs.GetString("DupeWithMatsPath", "DreadScripts/Duplicate With Materials/Generated Materials");
        }
#else

    [MenuItem("DreadTools/Utilities/Dupe With Mats")]
    public static void showWindow()
    {
        DuplicateWithMaterials win = GetWindow<DuplicateWithMaterials>(false, "Dupe With Mats", true);
        win.minSize = new Vector2(400, 90);
        win.maxSize = new Vector2(400, 90);
    }

    private void OnGUI()
    {
        EditorGUILayout.HelpBox("Script requires DS_Resources! Download below", MessageType.Warning);
        if (GUILayout.Button("Download Resources","toolbarbutton"))
            Application.OpenURL("https://github.com/Dreadrith/DreadScripts/releases/download/Scripts/DS_DLLResources.unitypackage");
    }
#endif

    }
}
#endif
