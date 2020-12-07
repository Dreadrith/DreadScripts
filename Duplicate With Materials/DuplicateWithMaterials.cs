#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using DS_CommonMethods;

public class DuplicateWithMaterials : EditorWindow
{
    private static string folderPath;
    private static bool separate;


    [MenuItem("GameObject/Duplicate With Materials", false, -1000)]
    static void Init()
    {
        folderPath = PlayerPrefs.GetString("DupeWithMatsPath", "DreadScripts/Duplicate With Materials/Generated Materials");
        separate = PlayerPrefs.GetInt("DupeWithMatsSep", 1) == 1;
        Dictionary<Material, Material> matDict = new Dictionary<Material, Material>();
        List<GameObject> selectedObjects = Selection.GetFiltered<GameObject>(SelectionMode.OnlyUserModifiable).ToList();
        DSCommonMethods.RecreateFolders(folderPath);
        if (selectedObjects.Count > 0)
        {
            foreach (GameObject obj in selectedObjects.ToList())
            {
                foreach (GameObject subobj in selectedObjects.ToList())
                {
                    if (subobj != obj)
                        if (subobj.transform.IsChildOf(obj.transform))
                        {
                            selectedObjects.Remove(subobj);
                            Debug.Log("Removed " + subobj.name + " from selection List");
                        }
                }
            }

            string assetPath = "";
            string subFolderPath = "";
            for (int o = 0; o < selectedObjects.Count; o++)
            {
                GameObject dupeObj = Instantiate(selectedObjects[o]);
                List<Renderer> myRenderers = dupeObj.GetComponentsInChildren<Renderer>(true).ToList();

                if (myRenderers.Count > 0)
                {

                    assetPath = AssetDatabase.GenerateUniqueAssetPath(folderPath + "/" + selectedObjects.ElementAt(o).name);
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
                            if (!separate && matDict.ContainsKey(myRenderers[i].sharedMaterials[j]))
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
                            if (!separate)
                                matDict.Add(myRenderers[i].sharedMaterials[j], newMaterials[j]);
                        }
                        myRenderers[i].sharedMaterials = newMaterials;
                    }
                }
                
            }
            string[] subDirectories = System.IO.Directory.GetDirectories(assetPath);
            foreach (string s in subDirectories)
            {
                if (!System.IO.Directory.EnumerateFileSystemEntries(s).Any())
                {
                    FileUtil.DeleteFileOrDirectory(s);
                    FileUtil.DeleteFileOrDirectory(s+".meta");
                }
            }
            AssetDatabase.Refresh();
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(assetPath));
        }
        else
            Debug.Log("No GameObjects Selected!");
    }

    [MenuItem("DreadTools/Scripts Settings/Dupe With Mats")]
    public static void showWindow()
    {
        DuplicateWithMaterials win = GetWindow<DuplicateWithMaterials>(false, "Dupe With Mats Settings", true);
        win.minSize = new Vector2(400, 90);
        win.maxSize = new Vector2(400, 90);
    }
    private void OnGUI()
    {
        DSCommonMethods.AssetFolderPath(ref folderPath, "New Materials Path", "DupeWithMatsPath");
        
        EditorGUI.BeginChangeCheck();
        separate = EditorGUILayout.Toggle(new GUIContent("Separate Shared Materials", "Force each material slot to have its own material."), separate);
        if (EditorGUI.EndChangeCheck())
        {
            PlayerPrefs.SetInt("DupeWithMatsSep", separate ? 1 : 0);
        }
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        DSCommonMethods.Credit();
    }

    private void OnEnable()
    {
        folderPath = PlayerPrefs.GetString("DupeWithMatsPath", "DreadScripts/Duplicate With Materials/Generated Materials");
        separate = PlayerPrefs.GetInt("DupeWithMatsSep", 1) == 1;
    }
}
#endif