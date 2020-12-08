#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using DS_CommonMethods;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

#if VRC_SDK_VRCSDK3
using VRC_AvatarDescriptor = VRC.SDK3.Avatars.Components.VRCAvatarDescriptor;
#elif VRC_SDK_VRCSDK2
using VRC_AvatarDescriptor = VRCSDK2.VRC_AvatarDescriptor;
#endif


public class QuickToggle : EditorWindow
{
    public static GameObject root;
    public static List<ToggleObject> targets = new List<ToggleObject>();
    public static UnityEditorInternal.ReorderableList targetList;
    public static bool gesture, autoName,pingClip,autoClose;
    public static string clipName;

    private static GUIContent warnIcon;
    private static GUIContent greenLight, redLight;
    private static GUIContent switchIcon;
    

    private static bool init;
    private static string myPath;
    private static bool clipValid = false;
    private static Vector2 scroll;

    [MenuItem("GameObject/Quick Actions/Quick Toggle", false, -10)]
    public static void ShowWindow()
    {
        targets.Clear();
        GetWindow<QuickToggle>(false, "Quick Toggle", true);
        GameObject[] targetObjs = Selection.GetFiltered<GameObject>(SelectionMode.OnlyUserModifiable);
        for (int i = 0; i < targetObjs.Length; i++)
            targets.Add(new ToggleObject(targetObjs[i]));
        if (!root)
            try
            {
                root = Selection.GetFiltered<GameObject>(SelectionMode.TopLevel).First().transform.root.GetComponentInChildren<VRC_AvatarDescriptor>(true).gameObject;
            }
            catch {
                root = Selection.GetFiltered<GameObject>(SelectionMode.TopLevel).First().transform.root.gameObject;
            }
        if (autoName)
        {
            clipName = "";
            if (targetObjs.Length == 0)
                clipName = "Objects";
            else
                if (targetObjs.Length == 1)
                clipName = targetObjs[0].name;
            else
            {
                for (int i = 0; i < targetObjs.Length; i++)
                {
                    int letterCount = Mathf.Clamp(7 - targetObjs.Length, 2, 5);
                    clipName += targetObjs[i].name.Substring(0, Mathf.Clamp(letterCount,1,targetObjs[i].name.Length));
                    if (i != targetObjs.Length - 1)
                        clipName += "-";
                }
            }
            clipName += " Enable";
        }
        CheckIfValid();
        init = false;
    } 

    private void Rename()
    {
        if (!autoName || targets.Count == 0)
            return;
        string statusName = "";
        bool enabled=false, disabled=false;
        for (int i=0;i<targets.Count;i++)
        {
            if (targets[i].Obj)
                if (targets[i].active)
                {
                    enabled = true;
                    statusName = " Enable";
                }
                else 
                {
                    disabled = true;
                    statusName = " Disable";
                }
            if (enabled && disabled)
            {
                statusName = " Toggle";
                break;
            }
        }

        if (clipName == (clipName = Regex.Replace(clipName, " enable", statusName, RegexOptions.IgnoreCase)))
        {
            if (clipName == (clipName = Regex.Replace(clipName, " disable", statusName, RegexOptions.IgnoreCase)))
            {
                clipName = Regex.Replace(clipName, " toggle", statusName, RegexOptions.IgnoreCase);
            }
        }
    }

    private void OnGUI()
    {
        if (!init)
            RefreshList();

        scroll = EditorGUILayout.BeginScrollView(scroll);

        EditorGUI.BeginChangeCheck();
        root = (GameObject)EditorGUILayout.ObjectField("Root", root, typeof(GameObject), true);
        if (EditorGUI.EndChangeCheck())
        {
            CheckIfValid();
        }
        EditorGUILayout.BeginHorizontal();
        clipName = EditorGUILayout.TextField("Clip Name", clipName);
        EditorGUIUtility.labelWidth = 70;
        EditorGUI.BeginChangeCheck();
        autoName = EditorGUILayout.Toggle(new GUIContent("AutoName","Automatically generate a clip name when using context menu button"), autoName,GUILayout.Width(100));
        if (EditorGUI.EndChangeCheck())
            PlayerPrefs.SetInt("QuickToggleAutoName", autoName ? 1 : 0);
        EditorGUIUtility.labelWidth = 0;
        EditorGUILayout.EndHorizontal();

        targetList.DoLayoutList();
        EditorGUI.BeginChangeCheck();
        gesture = EditorGUILayout.Toggle(new GUIContent("Is Gesture","Generates curves in 2 frames rather than 1 and sets loop time to true."), gesture);
        if (EditorGUI.EndChangeCheck())
            PlayerPrefs.SetInt("QuickToggleIsGesture", gesture ? 1 : 0);
        EditorGUI.BeginChangeCheck();
        pingClip = EditorGUILayout.Toggle(new GUIContent("Ping Clip", "Automatically highlights the newly generated clip in Assets"), pingClip);
        if (EditorGUI.EndChangeCheck())
            PlayerPrefs.SetInt("QuickTogglePingClip", pingClip ? 1 : 0);
        EditorGUI.BeginChangeCheck();
        autoClose = EditorGUILayout.Toggle(new GUIContent("Close Window", "Close window upon clip creation."), pingClip);
        if (EditorGUI.EndChangeCheck())
            PlayerPrefs.SetInt("QuickToggleAutoClose", autoClose ? 1 : 0);
        EditorGUI.BeginDisabledGroup(!clipValid || string.IsNullOrWhiteSpace(clipName));
        GUI.SetNextControlName("CreateClip");
        if (GUILayout.Button("Create Clip"))
            CreateClip();
        EditorGUI.EndDisabledGroup();

        DSCommonMethods.AssetFolderPath(ref myPath, "Clips Path", "QuickTogglePath");
       
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        
        DSCommonMethods.Credit();
        EditorGUILayout.EndScrollView();

        if (!init)
        {
            GUI.FocusControl("CreateClip");
            init = true;
        }

        if (GUI.GetNameOfFocusedControl() == "CreateClip" && Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter))
            CreateClip();
    }

    void CreateClip()
    {
        DSCommonMethods.RecreateFolders(myPath);
        AnimationClip myClip = new AnimationClip();
        foreach (ToggleObject obj in targets)
        {
            if (obj == null)
                continue;
            string path = AnimationUtility.CalculateTransformPath(obj.Obj.transform, root.transform);

            if (!gesture)
                myClip.SetCurve(path, typeof(GameObject), "m_IsActive", new AnimationCurve { keys = new Keyframe[] { new Keyframe { time = 0, value = obj.active ? 1 : 0 } } });
            else
                myClip.SetCurve(path, typeof(GameObject), "m_IsActive", new AnimationCurve { keys = new Keyframe[] { new Keyframe { time = 0, value = obj.active ? 1 : 0 }, new Keyframe { time = 1f / 60f, value = obj.active ? 1 : 0 } } });
        }
        if (gesture)
        {
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(myClip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(myClip, settings);
        }
        string clipPath = AssetDatabase.GenerateUniqueAssetPath(myPath + "/" + clipName + ".anim");
        AssetDatabase.CreateAsset(myClip, clipPath);
        if (pingClip)
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(clipPath));
        Debug.Log(clipPath.Substring(clipPath.LastIndexOf('/') + 1, clipPath.Length - clipPath.LastIndexOf('/') - 6) + " Created.");
        if (autoClose)
            Close();
    }

    private static void CheckIfValid()
    {
        bool validated = root;
        if (root)
            foreach (ToggleObject obj in targets)
                if (obj.Obj && (!obj.Obj.transform.IsChildOf(root.transform)))
                {
                    obj.valid = false;
                    validated = false;
                }
                else
                    obj.valid = true;
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i].Obj)
            {
                goto EndValidation;
            }

        }
        validated = false;

    EndValidation:
        clipValid = validated;
    }


    private void OnEnable()
    {
        warnIcon = new GUIContent(EditorGUIUtility.IconContent("d_console.warnicon.sml")) {tooltip="Object is not a child of Root!" };
        greenLight = new GUIContent(EditorGUIUtility.IconContent("d_greenLight")) {tooltip="Enable"};
        redLight = new GUIContent(EditorGUIUtility.IconContent("d_redLight")) {tooltip="Disable"};
        switchIcon = new GUIContent(EditorGUIUtility.IconContent("d_Animation.Record")) {tooltip = "Invert Toggles"};

        myPath = PlayerPrefs.GetString("QuickTogglePath", "Assets/DreadScripts/Quick Actions/Quick Toggle/Generated Clips");
        autoName = PlayerPrefs.GetInt("QuickToggleAutoName", 1) == 1;
        pingClip = PlayerPrefs.GetInt("QuickTogglePingClip", 1) == 1;
        gesture = PlayerPrefs.GetInt("QuickToggleIsGesture", 0) == 1;
        autoClose = PlayerPrefs.GetInt("QuickToggleAutoClose", 1) == 1;
        RefreshList();
        CheckIfValid();
        init = false;
    }

    private void DrawHeader(Rect rect)
    {
        EditorGUI.LabelField(rect, "Targets");

        if (GUI.Button(new Rect(rect.width-10,rect.y+2,20,EditorGUIUtility.singleLineHeight),switchIcon,GUIStyle.none))
        {
            foreach (ToggleObject obj in targets)
                obj.active = !obj.active;
            Rename();
        }
    }

    private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        if (!(index < targets.Count && index >= 0))
            return;
        if (GUI.Button(new Rect(rect.x, rect.y + 2, 20, EditorGUIUtility.singleLineHeight), "X"))
        {
            targets.RemoveAt(index);
            CheckIfValid();
            Rename();
            return;
        }

        ToggleObject toggleObj = targets[index];
        Rect myRect = new Rect(rect.x + 22, rect.y + 2, rect.width - 42, EditorGUIUtility.singleLineHeight);
        EditorGUI.BeginChangeCheck();

        Object dummy;
            dummy = EditorGUI.ObjectField(myRect, toggleObj.Obj, typeof(GameObject), true);

        if (EditorGUI.EndChangeCheck())
        {
            if (dummy == null)
                targets[index] = new ToggleObject();
            else
            {
                if (((GameObject)dummy).scene.IsValid())
                {
                    targets[index] = new ToggleObject((GameObject)dummy);
                }
                else
                    Debug.LogWarning("[QuickToggle] GameObject must be a scene object!");
            }
            CheckIfValid();
        }
        float xCoord = rect.x + rect.width - 18;
        if (!toggleObj.valid)
            EditorGUI.LabelField(new Rect(xCoord - 40, rect.y+2, 25, EditorGUIUtility.singleLineHeight), warnIcon);

        if (toggleObj.active)
            if (GUI.Button(new Rect(xCoord, rect.y, 20, 18), greenLight, GUIStyle.none))
            {
                toggleObj.active = false;
                Rename();
            }
        if (!toggleObj.active)
            if (GUI.Button(new Rect(xCoord, rect.y, 20, 18), redLight, GUIStyle.none))
            {
                toggleObj.active = true;
                Rename();
            }
    
    }

    private void RefreshList()
    {
        targetList = new UnityEditorInternal.ReorderableList(targets, typeof(ToggleObject), false, true, true, false)
        {
            drawElementCallback = DrawElement,
            drawHeaderCallback = DrawHeader
        };
    }

    public class ToggleObject
    {
        public GameObject Obj = null;
        public bool active = true;
        public bool valid = true;
        public ToggleObject() { }

        public ToggleObject(GameObject obj)
        {
            Obj = obj;
        }
    }
}


    


#endif
