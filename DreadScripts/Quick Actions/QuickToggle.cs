#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using VRC.SDK3.Avatars.Components;
using DS_CommonMethods;

public class QuickToggle : EditorWindow
{
    public static GameObject root;
    public static GameObject myObject;
    public static bool enable=true, disable;
    public static bool gesture;

    private static GUIContent warnIcon;
    private static bool notChild, noRoot, noObject,noToggle;
    private static string myPath;

    private static bool init;

    [MenuItem("GameObject/Quick Actions/Toggle Clip", false, -10)]
    public static void showWindow(MenuCommand selected)
    {
        QuickToggle win = GetWindow<QuickToggle>(false, "Quick Toggle", true);
        win.maxSize = new Vector2(400, 160);
        win.minSize = new Vector2(400, 160);
        GameObject dummyObj = selected.context as GameObject;
        if (dummyObj)
        {
            myObject = dummyObj;
            if (!root)
            {
                try { root = myObject.transform.root.GetComponentInChildren<VRCAvatarDescriptor>(true).gameObject; } catch { root = myObject.transform.root.gameObject; };
            }
        }
        checkIfValid();
        GUI.FocusControl("CreateClip");
        init = false;
    }

    private void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        root = (GameObject)EditorGUILayout.ObjectField("Root", root, typeof(GameObject), true);
        EditorGUILayout.BeginHorizontal();
        myObject = (GameObject)EditorGUILayout.ObjectField("Target", myObject, typeof(GameObject), true);
        if (notChild)
        {
            EditorGUILayout.LabelField(warnIcon, GUILayout.Width(20));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox(myObject.name + " is not a child of " + root.name + "!", MessageType.Warning);
        }
        else
            EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        enable = EditorGUILayout.ToggleLeft("Enable", enable);
        disable = EditorGUILayout.ToggleLeft("Disable", disable);
        EditorGUILayout.EndHorizontal();
        gesture = EditorGUILayout.ToggleLeft("Is Gesture", gesture);
        if (EditorGUI.EndChangeCheck())
        {
            checkIfValid();
        }
        EditorGUI.BeginDisabledGroup(noRoot || noObject || notChild || noToggle);
        GUI.SetNextControlName("CreateClip");
        if (GUILayout.Button("Create Clip"))
            makeClip();
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        DSCommonMethods.Credit();
        
        if (!init)
        {
            GUI.FocusControl("CreateClip");
            init = true;
        }

        if (GUI.GetNameOfFocusedControl() == "CreateClip" && Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter))
            makeClip();
    }

    static void makeClip()
    {
        if (!AssetDatabase.IsValidFolder(myPath + "/Generated Clips"))
            AssetDatabase.CreateFolder(myPath, "Generated Clips");
        string assetFolder = myPath + "/Generated Clips/" + root.name;
        if (!AssetDatabase.IsValidFolder(assetFolder))
            AssetDatabase.CreateFolder(myPath+ "/Generated Clips",root.name);
        string path = AnimationUtility.CalculateTransformPath(myObject.transform, root.transform);
        if (enable)
        {
            AnimationClip enableClip = new AnimationClip();
            if (!gesture)
                enableClip.SetCurve(path, typeof(GameObject), "m_IsActive", new AnimationCurve { keys = new Keyframe[] { new Keyframe { time = 0, value = 1 } } });
            else
            {
                enableClip.SetCurve(path, typeof(GameObject), "m_IsActive", new AnimationCurve { keys = new Keyframe[] { new Keyframe { time = 0, value = 1 }, new Keyframe { time = 1f / 60f, value = 1 } } });
                AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(enableClip);
                settings.loopTime = true;
                AnimationUtility.SetAnimationClipSettings(enableClip, settings);
            }
            string clipPath = assetFolder + "/" + myObject.name + " Enable.anim";
            if (!AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath))
                AssetDatabase.CreateAsset(enableClip, clipPath);
            else
                Debug.Log(myObject.name + " Enable.anim already exists");
        }
        if (disable)
        {
            AnimationClip disableClip = new AnimationClip();
            if (!gesture)
                disableClip.SetCurve(path, typeof(GameObject), "m_IsActive", new AnimationCurve { keys = new Keyframe[] { new Keyframe { time = 0, value = 0 } } });
            else
            {
                disableClip.SetCurve(path, typeof(GameObject), "m_IsActive", new AnimationCurve { keys = new Keyframe[] { new Keyframe { time = 0, value = 0 }, new Keyframe { time = 1f / 60f, value = 0 } } });
                AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(disableClip);
                settings.loopTime = true;
                AnimationUtility.SetAnimationClipSettings(disableClip, settings);
            }
            string clipPath = assetFolder + "/" + myObject.name + " Disable.anim";
            if (!AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath))
                AssetDatabase.CreateAsset(disableClip, clipPath);
            else
                Debug.Log(myObject.name + " Disable.anim already exists");
        }
        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(assetFolder));
        Debug.Log(myObject.name +" Clips Created.");
    }

    private static void checkIfValid()
    {
        noRoot = !root;
        noObject = !myObject;
        if (!noRoot && !noObject)
            notChild = ( myObject==root || !myObject.transform.IsChildOf(root.transform));
        else
            notChild = false;
        noToggle = !enable && !disable;
    }


    private void OnEnable()
    {
        warnIcon = EditorGUIUtility.IconContent("d_console.warnicon.sml");
        myPath = this.GetPath();
        checkIfValid();
        init = false;
    }

}

#endif
