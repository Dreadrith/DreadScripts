#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;


public class AssetOrganizer : EditorWindow
{
    private static Object mainAsset;
    private static List<string> createdFolders;
    private static string[] assetsPath;
    private static Object[] assets;
    private static bool[] moveAsset;
    private static bool[] copyAsset;
    private static bool[] checkedAsset;
    private static string myPath;
    private bool typeToggle;
    private bool folderToggle;
    private static Texture2D discordIcon;
    
    private Vector2 scrollview = Vector2.zero;

    [SerializeField] private bool  moveTexture = true, moveCubemap = true, moveMaterial = true, moveMesh = true, movePrefab = true, moveAudio = true, moveAnimation = true, moveAnimator = true, moveMask = true, moveShader=false, moveVRC=true, moveScript=false;
    [SerializeField] private bool copyTexture = false, copyCubemap = false, copyMaterial = false, copyMesh = false, copyPrefab = false, copyAudio = false, copyAnimation = false, copyAnimator = false, copyMask = false, copyShader = false, copyVRC = false, copyScript = false;
    [SerializeField] private List<customFolder> folders;
    [SerializeField] private bool deleteEmptyFolders = true;


    [MenuItem("DreadTools/Asset Organizer",false,300)]
    private static void showWindow()
    {
        GetWindow<AssetOrganizer>(false, "Asset Organizer", true);
    }

    private void OnGUI()
    {

        GUIStyle centerLabel = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter};
        scrollview = EditorGUILayout.BeginScrollView(scrollview);
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        mainAsset = EditorGUILayout.ObjectField("Main Asset", mainAsset,typeof(Object),false);
        if (EditorGUI.EndChangeCheck())
        {
            myPath = null;
            assetsPath = null;
        }
        EditorGUI.BeginDisabledGroup(mainAsset == null);
        if (GUILayout.Button("Get Assets",GUILayout.Width(80)))
        {
            myPath = AssetDatabase.GetAssetPath(mainAsset);
            assetsPath = AssetDatabase.GetDependencies(myPath);
            assets = new Object[assetsPath.Length];
            for (int i=0;i<assetsPath.Length;i++)
            {
                assets[i] = AssetDatabase.LoadAssetAtPath<Object>(assetsPath[i]);
            }
            myPath = myPath.Replace('\\', '/').Substring(0, myPath.LastIndexOf('/'));
            moveAsset = new bool[assetsPath.Length];
            copyAsset = new bool[assetsPath.Length];
            checkedAsset = new bool[assetsPath.Length];
            for (int i = 0; i < assetsPath.Length; i++)
            {
                string[] subFolders = assetsPath[i].Split('/');

                bool found = false;
                for (int j = 0; j < folders.Count; j++)
                {
                    if (!folders.ElementAt(j).copy && !folders.ElementAt(j).ignore)
                        continue;

                    foreach (string s in subFolders)
                    {
                        if (s == folders.ElementAt(j).name)
                        {
                            found = true;
                            if (folders.ElementAt(j).ignore)
                            {
                                moveAsset[i] = false;
                                checkedAsset[i] = true;
                            }
                            else
                                if (folders.ElementAt(j).copy)
                            {
                                copyAsset[i] = true;
                                checkedAsset[i] = true;
                            }
                            break;
                        }
                    }
                    if (found)
                        break;
                }

                if (checkedAsset[i])
                    continue;

                Object asset = assets[i];
                switch (asset.GetType().ToString())
                {
                    case "UnityEngine.Texture2D":
                        if (moveTexture)
                            moveAsset[i] = true;
                        else
                            if (copyTexture)
                            copyAsset[i] = true;
                        break;

                    case "UnityEngine.AudioClip":
                        if (moveAudio)
                            moveAsset[i] = true;
                        else
                            if (copyAudio)
                            copyAsset[i] = true;
                        break;

                    case "UnityEngine.Material":
                        if (moveMaterial)
                            moveAsset[i] = true;
                        else
                            if (copyMaterial)
                            copyAsset[i] = true;
                        break;

                    case "UnityEditor.Animations.BlendTree":
                    case "UnityEngine.AnimationClip":
                        if (moveAnimation)
                            moveAsset[i] = true;
                        else
                            if (copyAnimation)
                            copyAsset[i] = true;
                        break;

                    case "UnityEngine.AnimatorOverrideController":
                    case "UnityEditor.Animations.AnimatorController":
                        if (moveAnimator)
                            moveAsset[i] = true;
                        else
                            if (copyAnimator)
                            copyAsset[i] = true;
                        break;

                    case "UnityEngine.Shader":
                        if (moveShader)
                            moveAsset[i] = true;
                        else
                            if (copyShader)
                            copyAsset[i] = true;
                        break;

                    case "UnityEngine.Cubemap":
                        if (moveCubemap)
                            moveAsset[i] = true;
                        else
                            if (copyCubemap)
                            copyAsset[i] = true;
                        break;

                    case "UnityEngine.AvatarMask":
                        if (moveMask)
                            moveAsset[i] = true;
                        else
                            if (copyMask)
                            copyAsset[i] = true;
                        break;
                    case "UnityEditor.MonoScript":
                        if (moveScript)
                            moveAsset[i] = true;
                        else
                            if (copyScript)
                            copyAsset[i] = true;
                        break;
                    case "VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters":
                    case "VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu":
                        if (moveVRC)
                            moveAsset[i] = true;
                        else
                            if (copyVRC)
                            copyAsset[i] = true;
                        break;
                    default:
                        switch (Path.GetExtension(assetsPath[i]))
                        {
                            case ".fbx":
                                if (moveMesh)
                                    moveAsset[i] = true;
                                else
                            if (copyMesh)
                                    copyAsset[i] = true;
                                break;
                            case ".prefab":
                                if (movePrefab)
                                    moveAsset[i] = true;
                                else
                            if (copyPrefab)
                                    copyAsset[i] = true;
                                break;
                            case ".dll":
                                if (moveScript)
                                    moveAsset[i] = true;
                                else
                            if (copyScript)
                                    copyAsset[i] = true;
                                break;
                        }
                        break;
                }
            }

        }
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUIUtility.labelWidth = 150;
        deleteEmptyFolders = EditorGUILayout.Toggle("Delete Empty Folders", deleteEmptyFolders);
        EditorGUIUtility.labelWidth = 0;
        EditorGUI.BeginChangeCheck();
        typeToggle = GUILayout.Toggle(typeToggle, "Type Move/Copy", "button");
        if (EditorGUI.EndChangeCheck())
            folderToggle = false;
        EditorGUI.BeginChangeCheck();
        folderToggle = GUILayout.Toggle(folderToggle, "Folder Ignore/Copy", "button");
        if (EditorGUI.EndChangeCheck())
            typeToggle = false;


        if (folderToggle)
        {
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Folders");
            EditorGUILayout.LabelField("---------");
            for (int i = 0; i < folders.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("X", GUIStyle.none))
                {
                    folders.RemoveAt(i);
                    myPath = null;
                    assetsPath = null;
                    continue;
                }
                string dummyText = folders.ElementAt(i).name;
                EditorGUI.BeginChangeCheck();
                dummyText = EditorGUILayout.DelayedTextField(dummyText);
                EditorGUILayout.EndHorizontal();
                if (EditorGUI.EndChangeCheck())
                {
                    folders.Insert(i, new customFolder(dummyText, folders.ElementAt(i).ignore, folders.ElementAt(i).copy));
                    folders.RemoveAt(i + 1);
                    Repaint();
                }
            }
            if (GUILayout.Button("Add Folder"))
                folders.Add(new customFolder("", false, false));
            EditorGUILayout.EndVertical();
            EditorGUIUtility.labelWidth = 30;
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Ignore");
            EditorGUILayout.LabelField("--------");
            for (int i = 0; i < folders.Count; i++)
            {
                bool dummyToggle = folders.ElementAt(i).ignore;
                EditorGUI.BeginChangeCheck();
                spacedToggle(ref dummyToggle, 18);
                if (EditorGUI.EndChangeCheck())
                {
                    myPath = null;
                    assetsPath = null;
                    folders.Insert(i, new customFolder(folders.ElementAt(i).name, dummyToggle, false));
                    folders.RemoveAt(i + 1);
                    Repaint();
                }
            }

            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Copy");
            EditorGUILayout.LabelField("------");
            for (int i = 0; i < folders.Count; i++)
            {
                bool dummyToggle = folders.ElementAt(i).copy;
                EditorGUI.BeginChangeCheck();
                spacedToggle(ref dummyToggle, 15);
                if (EditorGUI.EndChangeCheck())
                {
                    myPath = null;
                    assetsPath = null;
                    folders.Insert(i, new customFolder(folders.ElementAt(i).name, false, dummyToggle));
                    folders.RemoveAt(i + 1);
                    Repaint();
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
        if (typeToggle)
        {
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 30;
           
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Type");
            EditorGUILayout.LabelField("------");
            EditorGUILayout.LabelField("Texture:");
            EditorGUILayout.LabelField("Cubemap:");
            EditorGUILayout.LabelField("Material:");
            EditorGUILayout.LabelField("Mesh:");
            EditorGUILayout.LabelField("Prefab:");
            EditorGUILayout.LabelField("Audio:");
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Move");
            EditorGUILayout.LabelField("--------");
            EditorGUI.BeginChangeCheck();
            spacedToggle(ref moveTexture, 18, ref copyTexture);
            spacedToggle(ref moveCubemap, 18, ref copyCubemap);
            spacedToggle(ref moveMaterial, 18, ref copyMaterial);
            spacedToggle(ref moveMesh, 18, ref copyMesh);
            spacedToggle(ref movePrefab, 18, ref copyPrefab);
            spacedToggle(ref moveAudio, 18, ref copyAudio);
            if (EditorGUI.EndChangeCheck())
            {
                myPath = null;
                assetsPath = null;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Copy");
            EditorGUILayout.LabelField("------");
            EditorGUI.BeginChangeCheck();
            spacedToggle(ref copyTexture, 15, ref moveTexture);
            spacedToggle(ref copyCubemap, 15, ref moveCubemap);
            spacedToggle(ref copyMaterial, 15, ref moveMaterial);
            spacedToggle(ref copyMesh, 15, ref moveMesh);
            spacedToggle(ref copyPrefab, 15, ref movePrefab);
            spacedToggle(ref copyAudio, 15, ref moveAudio);
            if (EditorGUI.EndChangeCheck())
            {
                myPath = null;
                assetsPath = null;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Type");
            EditorGUILayout.LabelField("------");
            EditorGUILayout.LabelField("Animation:");
            EditorGUILayout.LabelField("Animator:");
            EditorGUILayout.LabelField("Mask:");
            EditorGUILayout.LabelField("VRC Expression:");
            EditorGUILayout.LabelField("Shader:");
            EditorGUILayout.LabelField("Script:");
            EditorGUILayout.EndVertical();


            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Move");
            EditorGUILayout.LabelField("--------");
            EditorGUI.BeginChangeCheck();
            spacedToggle(ref moveAnimation, 18, ref copyAnimation);
            spacedToggle(ref moveAnimator, 18, ref copyAnimator);
            spacedToggle(ref moveMask, 18, ref copyMask);
            spacedToggle(ref moveVRC, 18, ref copyVRC);
            spacedToggle(ref moveShader, 18, ref copyShader);
            spacedToggle(ref moveScript, 18, ref copyScript);
            if (EditorGUI.EndChangeCheck())
            {
                myPath = null;
                assetsPath = null;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Copy");
            EditorGUILayout.LabelField("------");
            EditorGUI.BeginChangeCheck();
            spacedToggle(ref copyAnimation, 15, ref moveAnimation);
            spacedToggle(ref copyAnimator, 15, ref moveAnimator);
            spacedToggle(ref copyMask, 15, ref moveMask);
            spacedToggle(ref copyVRC, 15, ref moveVRC);
            spacedToggle(ref copyShader, 15, ref moveShader);
            spacedToggle(ref copyScript, 15, ref moveScript);
            if (EditorGUI.EndChangeCheck())
            {
                myPath = null;
                assetsPath = null;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
            
            EditorGUIUtility.labelWidth = 0;
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUI.BeginDisabledGroup(myPath == null || mainAsset==null);
        EditorGUI.EndDisabledGroup();
        if (assetsPath != null)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(30);
            EditorGUIUtility.labelWidth = 15;
            EditorGUILayout.LabelField("Asset",centerLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Move", centerLabel);
            EditorGUILayout.LabelField("Copy", centerLabel);
            EditorGUIUtility.labelWidth = 0;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            for (int i=0;i<assetsPath.Length;i++)
            {

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("   " + assetsPath[i], centerLabel))
                {
                    EditorGUIUtility.PingObject(assets[i]);
                    
                }
                GUILayout.FlexibleSpace();

                

                EditorGUI.BeginChangeCheck();
                moveAsset[i] = EditorGUILayout.Toggle(moveAsset[i], GUILayout.Width(70));
                if (EditorGUI.EndChangeCheck())
                    if (moveAsset[i])
                    {
                        copyAsset[i] = false;
                        Repaint();
                    }
                EditorGUI.BeginChangeCheck();
                copyAsset[i] = EditorGUILayout.Toggle(copyAsset[i], GUILayout.Width(40));
                if (EditorGUI.EndChangeCheck())
                    if (copyAsset[i])
                    {
                        moveAsset[i] = false;
                        Repaint();
                    }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
            }
            if (GUILayout.Button("Organize Assets"))
            {
                createdFolders.Clear();
                checkFolders();
                for (int k = 0; k < assetsPath.Length; k++)
                {
                    if (!moveAsset[k] && !copyAsset[k])
                        continue;
                    bool moved = false;
                    Object asset = assets[k];
                    switch (asset.GetType().ToString())
                    {
                        case "UnityEngine.Texture2D":
                            moved = true;
                            AssetDatabase.MoveAsset(assetsPath[k], myPath + "/Texture2D/" + Path.GetFileName(assetsPath[k]));
                            if (copyAsset[k])
                                AssetDatabase.CopyAsset(myPath + "/Texture2D/" + Path.GetFileName(assetsPath[k]), assetsPath[k]);
                            break;

                        case "UnityEngine.AudioClip":
                            moved = true;
                            AssetDatabase.MoveAsset(assetsPath[k], myPath + "/AudioClip/" + Path.GetFileName(assetsPath[k]));
                            if (copyAsset[k])
                                AssetDatabase.CopyAsset(myPath + "/AudioClip/" + Path.GetFileName(assetsPath[k]), assetsPath[k]);
                            break;

                        case "UnityEngine.Material":
                            moved = true;
                            AssetDatabase.MoveAsset(assetsPath[k], myPath + "/Material/" + Path.GetFileName(assetsPath[k]));
                            if (copyAsset[k])
                                AssetDatabase.CopyAsset(myPath + "/Material/" + Path.GetFileName(assetsPath[k]), assetsPath[k]);
                            break;

                        case "UnityEditor.Animations.BlendTree":
                        case "UnityEngine.AnimationClip":
                            moved = true;
                            AssetDatabase.MoveAsset(assetsPath[k], myPath + "/Animation/" + Path.GetFileName(assetsPath[k]));
                            if (copyAsset[k])
                                AssetDatabase.CopyAsset(myPath + "/Animation/" + Path.GetFileName(assetsPath[k]), assetsPath[k]);
                            break;

                        case "UnityEngine.AnimatorOverrideController":
                        case "UnityEditor.Animations.AnimatorController":
                            moved = true;
                            AssetDatabase.MoveAsset(assetsPath[k], myPath + "/AnimatorController/" + Path.GetFileName(assetsPath[k]));
                            if (copyAsset[k])
                                AssetDatabase.CopyAsset(myPath + "/AnimatorController/" + Path.GetFileName(assetsPath[k]), assetsPath[k]);
                            break;

                        case "UnityEngine.Shader":
                            moved = true;
                            AssetDatabase.MoveAsset(assetsPath[k], myPath + "/Shader/" + Path.GetFileName(assetsPath[k]));
                            if (copyAsset[k])
                                AssetDatabase.CopyAsset(myPath + "/Shader/" + Path.GetFileName(assetsPath[k]), assetsPath[k]);
                            break;

                        case "UnityEngine.Cubemap":
                            moved = true;
                            AssetDatabase.MoveAsset(assetsPath[k], myPath + "/Cubemap/" + Path.GetFileName(assetsPath[k]));
                            if (copyAsset[k])
                                AssetDatabase.CopyAsset(myPath + "/Cubemap/" + Path.GetFileName(assetsPath[k]), assetsPath[k]);
                            break;

                        case "UnityEngine.AvatarMask":
                            moved = true;
                            AssetDatabase.MoveAsset(assetsPath[k], myPath + "/AvatarMask/" + Path.GetFileName(assetsPath[k]));
                            if (copyAsset[k])
                                AssetDatabase.CopyAsset(myPath + "/AvatarMask/" + Path.GetFileName(assetsPath[k]), assetsPath[k]);
                            break;

                        case "VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters":
                        case "VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu":
                            moved = true;
                            AssetDatabase.MoveAsset(assetsPath[k], myPath + "/VRC/" + Path.GetFileName(assetsPath[k]));
                            if (copyAsset[k])
                                AssetDatabase.CopyAsset(myPath + "/VRC/" + Path.GetFileName(assetsPath[k]), assetsPath[k]);
                            break;
                        case "UnityEditor.MonoScript":
                            moved = true;
                            AssetDatabase.MoveAsset(assetsPath[k], myPath + "/Script/" + Path.GetFileName(assetsPath[k]));
                            if (copyAsset[k])
                                AssetDatabase.CopyAsset(myPath + "/Script/" + Path.GetFileName(assetsPath[k]), assetsPath[k]);
                            break;
                        default:
                            switch (Path.GetExtension(assetsPath[k]))
                            {
                                case ".fbx":
                                    moved = true;
                                    AssetDatabase.MoveAsset(assetsPath[k], myPath + "/Model/" + Path.GetFileName(assetsPath[k]));
                                    if (copyAsset[k])
                                        AssetDatabase.CopyAsset(myPath + "/Model/" + Path.GetFileName(assetsPath[k]), assetsPath[k]);
                                    break;
                                case ".prefab":
                                    moved = true;
                                    AssetDatabase.MoveAsset(assetsPath[k], myPath + "/Prefab/" + Path.GetFileName(assetsPath[k]));
                                    if (copyAsset[k])
                                        AssetDatabase.CopyAsset(myPath + "/Prefab/" + Path.GetFileName(assetsPath[k]), assetsPath[k]);
                                    break;
                                case ".dll":
                                    moved = true;
                                    AssetDatabase.MoveAsset(assetsPath[k], myPath + "/Script/" + Path.GetFileName(assetsPath[k]));
                                    if (copyAsset[k])
                                        AssetDatabase.CopyAsset(myPath + "/Script/" + Path.GetFileName(assetsPath[k]), assetsPath[k]);
                                    break;
                            }
                            break;
                    }
                    if (moved && !copyAsset[k] && deleteEmptyFolders)
                    {
                        DeleteEmptyFolder(assetsPath[k]);
                    }
                }
                foreach (string folderGUID in createdFolders)
                {
                    string folder = AssetDatabase.GUIDToAssetPath(folderGUID);
                    if (IsDirectoryEmpty(folder))
                    {
                        FileUtil.DeleteFileOrDirectory(folder);
                        FileUtil.DeleteFileOrDirectory(folder + ".meta");
                    }
                }
                assetsPath = null;
                myPath = null;
                AssetDatabase.Refresh();
            }

        }
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        
        EditorGUILayout.SelectableLabel("by Dreadrith#3238",GUILayout.Width(115));
        if (GUILayout.Button(discordIcon, GUIStyle.none, GUILayout.Width(32), GUILayout.Height(32)))
            Application.OpenURL("https://discord.gg/ZsPfrGn");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();
    }
    public bool IsDirectoryEmpty(string path)
    {
        return !Directory.EnumerateFileSystemEntries(path).Any();
    }

    private void OnEnable()
    {
        string data = EditorPrefs.GetString("AvatarAssetOrganizerSettings", JsonUtility.ToJson(this, false));
        JsonUtility.FromJsonOverwrite(data, this);
        if (!EditorPrefs.HasKey("AvatarAssetOrganizerSettings"))
        {
            folders.Add(new customFolder("VRCSDK",false,false));
            folders.Add(new customFolder("_PoiyomiShaders", false, false));
            folders.Add(new customFolder("VRLabs", false, false));
            folders.Add(new customFolder("PumkinsAvatarTools", false, false));
            folders.Add(new customFolder("DreadScripts", false, false));
            folders.Add(new customFolder("Packages", false, false));
            folders.Add(new customFolder("Plugins", false, false));
            folders.Add(new customFolder("Editor", false, false));
        }

        if (discordIcon == null)
            discordIcon = Resources.Load<Texture2D>("DS_DiscordIcon");

        createdFolders = new List<string>();
    }

    private void OnDisable()
    {
        string data = JsonUtility.ToJson(this, false);
        EditorPrefs.SetString("AvatarAssetOrganizerSettings", data);
    }
    private void checkFolders()
    {
        createdFolders.Clear();
        if (!AssetDatabase.IsValidFolder(myPath + "/AudioClip"))
           createdFolders.Add(AssetDatabase.CreateFolder(myPath, "AudioClip"));
        if (!AssetDatabase.IsValidFolder(myPath + "/Texture2D"))
            createdFolders.Add(AssetDatabase.CreateFolder(myPath, "Texture2D"));
        if (!AssetDatabase.IsValidFolder(myPath + "/Material"))
            createdFolders.Add(AssetDatabase.CreateFolder(myPath, "Material"));
        if (!AssetDatabase.IsValidFolder(myPath + "/Animation"))
            createdFolders.Add(AssetDatabase.CreateFolder(myPath, "Animation"));
        if (!AssetDatabase.IsValidFolder(myPath + "/AnimatorController"))
            createdFolders.Add(AssetDatabase.CreateFolder(myPath, "AnimatorController"));
        if (!AssetDatabase.IsValidFolder(myPath + "/Shader"))
            createdFolders.Add(AssetDatabase.CreateFolder(myPath, "Shader"));
        if (!AssetDatabase.IsValidFolder(myPath + "/Model"))
            createdFolders.Add(AssetDatabase.CreateFolder(myPath, "Model"));
        if (!AssetDatabase.IsValidFolder(myPath + "/Prefab"))
            createdFolders.Add(AssetDatabase.CreateFolder(myPath, "Prefab"));
        if (!AssetDatabase.IsValidFolder(myPath + "/Cubemap"))
            createdFolders.Add(AssetDatabase.CreateFolder(myPath, "Cubemap"));
        if (!AssetDatabase.IsValidFolder(myPath + "/AvatarMask"))
            createdFolders.Add(AssetDatabase.CreateFolder(myPath, "AvatarMask"));
        if (!AssetDatabase.IsValidFolder(myPath + "/VRC"))
            createdFolders.Add(AssetDatabase.CreateFolder(myPath, "VRC"));
        if (!AssetDatabase.IsValidFolder(myPath + "/Script"))
            createdFolders.Add(AssetDatabase.CreateFolder(myPath, "Script"));
    }


    private void spacedToggle(ref bool toggle, float sp)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(sp);
        toggle = EditorGUILayout.Toggle(toggle);
        EditorGUILayout.EndHorizontal();
    }

    private void spacedToggle(ref bool toggle,float sp,ref bool counterToggle)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(sp);
        EditorGUI.BeginChangeCheck();
        toggle = EditorGUILayout.Toggle(toggle);
        if (EditorGUI.EndChangeCheck())
            if (toggle)
                counterToggle = false;
        EditorGUILayout.EndHorizontal();
    }

    public void DeleteEmptyFolder(string filePath)
    {
        string currentDirectory = Path.GetDirectoryName(filePath);
        string parentDirectory;
        while (IsDirectoryEmpty(currentDirectory) && currentDirectory!="Assets")
        {
            parentDirectory = Path.GetDirectoryName(currentDirectory);
            FileUtil.DeleteFileOrDirectory(currentDirectory);
            FileUtil.DeleteFileOrDirectory(currentDirectory + ".meta");
            currentDirectory = parentDirectory;
        }
    }

    [System.Serializable]
    public struct customFolder
    {
        public customFolder(string newName, bool ignoreValue, bool copyValue)
        {
            name = newName;
            ignore = ignoreValue;
            copy = copyValue;
        }
        public string name;
        public bool ignore;
        public bool copy;
    }
    

}
#endif

