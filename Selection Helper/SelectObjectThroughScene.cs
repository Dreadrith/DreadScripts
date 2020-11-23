#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

[InitializeOnLoad]
public class SelectObjectThroughScene : EditorWindow
{

    //By Dreadrith#3238
    //https://discord.gg/ZsPfrGn

    [InitializeOnLoadMethod]
    public static void Enable()
    {
        if (discordIcon == null)
            discordIcon = Resources.Load<Texture2D>("DS_DiscordIcon");
        OnColor = new Color(PlayerPrefs.GetFloat("SelectedHandleR", 0.4f), PlayerPrefs.GetFloat("SelectedHandleG", 0.85f), PlayerPrefs.GetFloat("SelectedHandleB", 0.65f));
        OffColor = new Color(PlayerPrefs.GetFloat("DeSelectedHandleR", 0.5f), PlayerPrefs.GetFloat("DeSelectedHandleG",0), PlayerPrefs.GetFloat("DeSelectedHandleB",0));
        minHandleSize = PlayerPrefs.GetFloat("MinSelectSize",0.005f);
        maxHandleSize = PlayerPrefs.GetFloat("MaxSelectSize",0.04f);
        handleSize = PlayerPrefs.GetFloat("HandleSelectSize", 0.00525f);
        SceneView.onSceneGUIDelegate += OnScene;
        Selection.selectionChanged += OnSelectionChange;
    }
    public static void Disable()
    {
        SceneView.onSceneGUIDelegate -= OnScene;
        Selection.selectionChanged -= OnSelectionChange;
    }

    [MenuItem("DreadTools/Scene Object Select/Settings")]
    public static void showWindow()
    {
        GetWindow<SelectObjectThroughScene>(false, "Scene Selector Settings", true);
    }

    private static bool selecting = false;
    private static float handleSize, minHandleSize, maxHandleSize;
    private static Transform[] sceneObjects;
    private static bool ignoreDBones = true,includeRoots=true;
    private static bool hasDbones = (null != System.Type.GetType("DynamicBone"));
    private static bool[] bitmask;
    private static Color OnColor;
    private static Color OffColor;
    private static Texture2D discordIcon;
    private static void OnScene(SceneView sceneview)
    {
        Handles.BeginGUI();
        GUILayout.Space(20);
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(EditorGUIUtility.IconContent("CapsuleCollider2D Icon"), GUIStyle.none, GUILayout.Width(20),GUILayout.Height(20)))
        {
            Event e = Event.current;
            if (e.button == 0)
            {
                selecting = !selecting;
                if (selecting)
                {
                    sceneObjects = FindObjectsOfType<Transform>();
                    
                    if (hasDbones && ignoreDBones)
                    {
                        System.Type dboneType = System.Type.GetType("DynamicBone");
                        List<Transform> dbones = new List<Transform>();
                        Object[] dboneScripts = FindObjectsOfType(dboneType);
                        foreach (Object b in dboneScripts)
                        {
                            SerializedObject sb = new SerializedObject(b);
                            List<Transform> exclusionList = new List<Transform>();
                            SerializedProperty excProp = sb.FindProperty("m_Exclusions");
                            for (int i = 0; i < excProp.arraySize; i++)
                                exclusionList.Add((Transform)excProp.GetArrayElementAtIndex(i).objectReferenceValue);
                            getBoneChildren(dbones, exclusionList, (Transform)sb.FindProperty("m_Root").objectReferenceValue,includeRoots);
                        }
                        sceneObjects = sceneObjects.Except(dbones).ToArray();
                    }
                    refreshBitMask();
                }
            }
            if (e.button == 1)
            {
                GetWindow<SelectObjectThroughScene>(false, "Bone Selector Settings", true);
            }
        }
        EditorGUILayout.EndHorizontal();
        if (selecting)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            handleSize = GUILayout.VerticalSlider(handleSize, maxHandleSize, minHandleSize, GUILayout.Height(50));
            GUILayout.Space(1);
            EditorGUILayout.EndHorizontal();
        }
        Handles.EndGUI();

        if (selecting)
        {
            if (sceneObjects.Length > 0)
            {
                for (int i=0;i<sceneObjects.Length;i++)
                {
                    if (!sceneObjects[i])
                        continue;
                    int controlID = sceneObjects[i].GetHashCode();
                    Event e = Event.current;
                    if (bitmask[i])
                        Handles.color = OnColor;
                    else
                        Handles.color = OffColor;
                    Handles.SphereHandleCap(controlID, sceneObjects[i].position, Quaternion.identity, handleSize, EventType.Repaint);
                    switch (e.GetTypeForControl(controlID))
                    {
                        case EventType.MouseDown:
                            if (HandleUtility.nearestControl == controlID && e.button == 0)
                            {
                                if (e.control)
                                {
                                    if (!Selection.objects.Contains(sceneObjects[i].gameObject))
                                    {
                                        Selection.objects = Selection.objects.Concat(new GameObject[] { sceneObjects[i].gameObject }).ToArray();
                                    }
                                    else
                                    {
                                        Selection.objects = Selection.objects.Except(new GameObject[] { sceneObjects[i].gameObject }).ToArray();
                                    }
                                }
                                else
                                {
                                    Selection.activeObject = sceneObjects[i].gameObject;
                                }
                                e.Use();
                            }
                            break;
                        case EventType.Layout:
                            float distance = HandleUtility.DistanceToCircle(sceneObjects[i].position, handleSize / 2f);
                            HandleUtility.AddControl(controlID, distance);
                            break;
                    }
                }
            }
        }

    }

    private static void getBoneChildren(List<Transform> dbones, List<Transform> exclusionList, Transform parent, bool first = false)
    {
        if (exclusionList.Contains(parent))
            return;
        else
        {
            if (!first)
            dbones.Add(parent);
            for (int i = 0; i < parent.childCount; i++)
            {
                getBoneChildren(dbones, exclusionList, parent.GetChild(i));
            }
        }
    }
    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUIUtility.labelWidth = 20;
        EditorGUILayout.LabelField("Handle Size");

        EditorGUI.BeginChangeCheck();
        minHandleSize = EditorGUILayout.FloatField(minHandleSize,GUILayout.Width(40));
        if (EditorGUI.EndChangeCheck())
        {
            PlayerPrefs.SetFloat("MinSelectSize", minHandleSize);
        }

        EditorGUI.BeginChangeCheck();
        handleSize = GUILayout.HorizontalSlider(handleSize, minHandleSize, maxHandleSize);
        if (EditorGUI.EndChangeCheck())
        {
            PlayerPrefs.SetFloat("HandleSelectSize", handleSize);
        }

        EditorGUI.BeginChangeCheck();
        maxHandleSize = EditorGUILayout.FloatField(maxHandleSize, GUILayout.Width(40));
        if (EditorGUI.EndChangeCheck())
        {
            PlayerPrefs.SetFloat("MaxSelectSize", maxHandleSize);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUIUtility.labelWidth = 0;
        EditorGUI.BeginChangeCheck();
        OnColor = EditorGUILayout.ColorField("Selected", OnColor);
        if (EditorGUI.EndChangeCheck())
        {
            PlayerPrefs.SetFloat("SelectedHandleR", OnColor.r);
            PlayerPrefs.SetFloat("SelectedHandleG", OnColor.g);
            PlayerPrefs.SetFloat("SelectedHandleB", OnColor.b);
            SceneView.RepaintAll();
        }
        EditorGUI.BeginChangeCheck();
        OffColor = EditorGUILayout.ColorField("Deselected", OffColor);
        if (EditorGUI.EndChangeCheck())
        {
            PlayerPrefs.SetFloat("DeSelectedHandleR", OffColor.r);
            PlayerPrefs.SetFloat("DeSelectedHandleG", OffColor.g);
            PlayerPrefs.SetFloat("DeSelectedHandleB", OffColor.b);
            SceneView.RepaintAll();
        }
        if (hasDbones)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 70;
            ignoreDBones = EditorGUILayout.ToggleLeft("Ignore D-Bones", ignoreDBones);
            includeRoots = EditorGUILayout.ToggleLeft("Include D-Bone Roots", includeRoots);
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        EditorGUILayout.SelectableLabel("by Dreadrith#3238", GUILayout.Width(115));
        if (GUILayout.Button(discordIcon, GUIStyle.none, GUILayout.Width(32), GUILayout.Height(32)))
            Application.OpenURL("https://discord.gg/ZsPfrGn");
        EditorGUILayout.EndHorizontal();
    }

    private static void OnSelectionChange()
    {
        if (selecting)
            refreshBitMask();
    }
    private static void refreshBitMask()
    {
        bitmask = new bool[sceneObjects.Length];
        for (int i=0;i<sceneObjects.Length;i++)
        {
            if (!sceneObjects[i])
            {
                sceneObjects = sceneObjects.Except(new Transform[] { sceneObjects[i] }).ToArray();
                refreshBitMask();
                break;
            }

            if (Selection.objects.Contains(sceneObjects[i].gameObject))
                bitmask[i] = true;
            else
                bitmask[i] = false;
        }
    }
}
#endif
