using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DynamicBone)), CanEditMultipleObjects]
public class PublicDynamicBoneEditor : Editor
{
    private SerializedObject selectedBones;
    private SerializedProperty myRoot;
    private SerializedProperty myUpdateRate;
    private SerializedProperty myUMode;
    private SerializedProperty myDamping;
    private SerializedProperty myDDistrib;
    private SerializedProperty myElas;
    private SerializedProperty myEDistrib;
    private SerializedProperty myStif;
    private SerializedProperty mySDistrib;
    private SerializedProperty myInert;
    private SerializedProperty myIDistrib;
    private SerializedProperty myRad;
    private SerializedProperty myRDistrib;
    private SerializedProperty myEndLength;
    private SerializedProperty myEndOffset;
    private SerializedProperty myGravity;
    private SerializedProperty myForce;
    private SerializedProperty myColliders;
    private SerializedProperty myExclusions;
    private SerializedProperty myFreezeAxis;
    private SerializedProperty myDistantDisable;
    private SerializedProperty myReferenceObject;
    private SerializedProperty myDistanceToObject;
    private SerializedProperty myTransformProp;
    private Transform myTransform;

    Object[] targetBones;
    private static bool  editOffset = false, editRadius = false, singlePositionHandle = true,customize=false;
    private static int handleIndex = 0;

    private List<Transform> allChildren;
    private List<Transform> bottomChildren;
    private int[] intMask;
    

    

    public override void OnInspectorGUI()
    {

        selectedBones.Update();
        if (!PlayerPrefs.HasKey("BoneEditorIsRead"))
        {
            PlayerPrefs.SetInt("BoneEditorIsRead", 0);
            PlayerPrefs.Save();
        }
        if (PlayerPrefs.GetInt("BoneEditorIsRead") != 1)
        {
            if (GUILayout.Button("Editor Made by Dreadrith#3238"))
            {
                PlayerPrefs.SetInt("BoneEditorIsRead", 1);
                PlayerPrefs.Save();
            }
        }
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(myRoot, new GUIContent("Root", ""));
        if (GUILayout.Button("Set to Self", GUILayout.Width(100)))
        {
            Undo.RecordObjects(targets, "EditBonesValues");
            foreach (DynamicBone subbone in targets)
                subbone.m_Root = subbone.transform;
            SceneView.RepaintAll();
            Repaint();
        }
        EditorGUILayout.EndHorizontal();




        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(myUpdateRate, new GUIContent("Update Rate", ""));
        EditorGUIUtility.labelWidth = 80;

        EditorGUILayout.PropertyField(myUMode, new GUIContent("Mode", ""));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUIUtility.labelWidth = 0;

        EditorGUILayout.PropertyField(myDamping, new GUIContent("Damping", ""));
        EditorGUILayout.PropertyField(myDDistrib, new GUIContent("", ""), GUILayout.Width(150));
        if (GUILayout.Button("X", GUIStyle.none, GUILayout.Width(10)))
        {
            myDDistrib.animationCurveValue = new AnimationCurve();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.PropertyField(myElas, new GUIContent("Elasticity", ""));
        EditorGUILayout.PropertyField(myEDistrib, new GUIContent("", ""), GUILayout.Width(150));
        if (GUILayout.Button("X", GUIStyle.none, GUILayout.Width(10)))
        {
            myEDistrib.animationCurveValue = new AnimationCurve();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(myStif, new GUIContent("Stiffness", ""));
        EditorGUILayout.PropertyField(mySDistrib, new GUIContent("", ""), GUILayout.Width(150));
        if (GUILayout.Button("X", GUIStyle.none, GUILayout.Width(10)))
        {
            mySDistrib.animationCurveValue = new AnimationCurve();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(myInert, new GUIContent("Inert", ""));
        EditorGUILayout.PropertyField(myIDistrib, new GUIContent("", ""), GUILayout.Width(150));
        if (GUILayout.Button("X", GUIStyle.none, GUILayout.Width(10)))
        {
            myIDistrib.animationCurveValue = new AnimationCurve();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUIUtility.labelWidth = 100;
        EditorGUILayout.LabelField("Radius");
        EditorGUIUtility.labelWidth = 0;


        EditorGUI.BeginChangeCheck();
        editRadius = GUILayout.Toggle(editRadius, "Edit", "button", GUILayout.Width(50));
        if (EditorGUI.EndChangeCheck())
            if (editRadius)
            {
                if (myRDistrib.animationCurveValue.length < 2)
                {
                    Keyframe[] keys = { new Keyframe(0, 1), new Keyframe(1, 1) };
                    myRDistrib.animationCurveValue = new AnimationCurve(keys);
                }
                getAllChildren();
                editOffset = false;
                SceneView.RepaintAll();
                EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));
            }
            else
            {
                SceneView.RepaintAll();
                EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));
            }





        EditorGUILayout.PropertyField(myRad, new GUIContent("", ""));
        EditorGUILayout.PropertyField(myRDistrib, new GUIContent("", ""), GUILayout.Width(150));
        if (GUILayout.Button("X", GUIStyle.none, GUILayout.Width(10)))
        {
            myRDistrib.animationCurveValue = new AnimationCurve();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(myEndLength, new GUIContent("End Length", ""));
        if (GUILayout.Button("X", GUIStyle.none, GUILayout.Width(10)))
        {
            myEndLength.floatValue = 0;
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUIUtility.labelWidth = 60;
        EditorGUILayout.LabelField("End Offset");
        EditorGUIUtility.labelWidth = 0;


        EditorGUI.BeginChangeCheck();
        editOffset = GUILayout.Toggle(editOffset, "Edit", "button", GUILayout.Width(50)); 
        if (EditorGUI.EndChangeCheck())
            if (editOffset)
            {
                getBottomChildren();
                editRadius = false;
                SceneView.RepaintAll();
                EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));
            }
            else
            {
                SceneView.RepaintAll();
                EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));
            }
      

        EditorGUILayout.PropertyField(myEndOffset, new GUIContent("", ""));
        if (GUILayout.Button("X", GUIStyle.none, GUILayout.Width(10)))
        {
            myEndOffset.vector3Value = Vector3.zero;
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(myGravity, new GUIContent("Gravity", ""));
        if (GUILayout.Button("X", GUIStyle.none, GUILayout.Width(10)))
        {
            myGravity.vector3Value = Vector3.zero;
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(myForce, new GUIContent("Force", ""));
        if (GUILayout.Button("X", GUIStyle.none, GUILayout.Width(10)))
        {
            myForce.vector3Value = Vector3.zero;
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.PropertyField(myColliders, null, true);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(myExclusions, null, true);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(myFreezeAxis, new GUIContent("Freeze Axis", ""));
        EditorGUILayout.PropertyField(myDistantDisable, new GUIContent("Distant Disable", ""));
        EditorGUILayout.PropertyField(myReferenceObject, new GUIContent("Reference Object", ""));
        EditorGUILayout.PropertyField(myDistanceToObject, new GUIContent("Distance To Object", ""));



        customize = EditorGUILayout.Foldout(customize, "Customize");
        if (customize)
        {
            EditorGUI.indentLevel++;
            EditorGUIUtility.labelWidth = 0;
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            singlePositionHandle = EditorGUILayout.Toggle("Single Position Handle", singlePositionHandle);
            if (EditorGUI.EndChangeCheck())
                SceneView.RepaintAll();
            
            if (singlePositionHandle)
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Cycle Handle",GUILayout.Width(150)))
                {
                    handleIndex++;
                    SceneView.RepaintAll();
                }
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();
        }
        selectedBones.ApplyModifiedProperties();
    }

    

    private void OnSceneGUI()
    {
        if (editRadius || editOffset)
        {
            if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.Escape))
            {
                editRadius = false;
                editOffset = false;
            }
            Tools.hidden = true;
        }
        else
            Tools.hidden = false;

        if (editRadius)
        {
            Handles.color = new Color(1,0.65f,0);
            float increment=1f/allChildren.Count, current = 0;
            foreach (Transform child in allChildren)
            {
                myRad.floatValue = Handles.RadiusHandle(Quaternion.identity, child.position, myRad.floatValue * myTransform.lossyScale.x * (myRDistrib.animationCurveValue.Evaluate(0) == 0 && current == 0 ? float.MinValue : myRDistrib.animationCurveValue.Evaluate(current)), true) / myTransform.lossyScale.x / (myRDistrib.animationCurveValue.Evaluate(0) == 0 && current == 0 ? float.MinValue : myRDistrib.animationCurveValue.Evaluate(current));
                current += increment;
            }
            

            selectedBones.ApplyModifiedProperties();
        }
        if (editOffset)
        {
            if (!singlePositionHandle)
            {
                if (Tools.pivotRotation == PivotRotation.Global)
                    foreach (Transform child in bottomChildren)
                    {
                        myEndOffset.vector3Value = Quaternion.Euler(Quaternion.Inverse(myTransform.rotation).eulerAngles) * ((Handles.PositionHandle(child.position + myTransform.TransformDirection(myEndOffset.vector3Value), Quaternion.identity)) - child.position);
                    }
                else
                    foreach (Transform child in bottomChildren)
                    {
                        myEndOffset.vector3Value = Quaternion.Euler(Quaternion.Inverse(myTransform.rotation).eulerAngles) * ((Handles.PositionHandle(child.position + myTransform.TransformDirection(myEndOffset.vector3Value), child.rotation)) - child.position);
                    }
            }
            else
            {
                if (handleIndex >= bottomChildren.Count)
                    handleIndex = 0;
                if (Tools.pivotRotation == PivotRotation.Global)
                    myEndOffset.vector3Value = Quaternion.Euler(Quaternion.Inverse(myTransform.rotation).eulerAngles) * ((Handles.PositionHandle(bottomChildren[handleIndex].position + myTransform.TransformDirection(myEndOffset.vector3Value), Quaternion.identity)) - bottomChildren[handleIndex].position);
                else
                    myEndOffset.vector3Value = Quaternion.Euler(Quaternion.Inverse(myTransform.rotation).eulerAngles) * ((Handles.PositionHandle(bottomChildren[handleIndex].position + myTransform.TransformDirection(myEndOffset.vector3Value), bottomChildren[handleIndex].rotation)) - bottomChildren[handleIndex].position);
            }
            selectedBones.ApplyModifiedProperties();
        }


    }

    private void OnEnable()
    {
        selectedBones = new SerializedObject(targets);
        myRoot = selectedBones.FindProperty("m_Root");
        myTransformProp = (new SerializedObject(selectedBones.FindProperty("m_GameObject").objectReferenceValue)).FindProperty("m_Component.Array.data[0].component");
        myTransform = (Transform)myTransformProp.objectReferenceValue;



        targetBones = targets;
        foreach (DynamicBone tbone in targetBones)
        {
            if (tbone.m_Root == null)
                tbone.m_Root = tbone.transform;
            if (tbone.m_Colliders == null)
                tbone.m_Colliders = new List<DynamicBoneColliderBase>();
            if (tbone.m_Exclusions == null)
                tbone.m_Exclusions = new List<Transform>();
        }

        allChildren = new List<Transform>();
        bottomChildren = new List<Transform>();
        
        
        myUpdateRate = selectedBones.FindProperty("m_UpdateRate");
        myUMode = selectedBones.FindProperty("m_UpdateMode");
        myDamping = selectedBones.FindProperty("m_Damping");
        myDDistrib = selectedBones.FindProperty("m_DampingDistrib");
        myElas = selectedBones.FindProperty("m_Elasticity");
        myEDistrib = selectedBones.FindProperty("m_ElasticityDistrib");
        myStif = selectedBones.FindProperty("m_Stiffness");
        mySDistrib = selectedBones.FindProperty("m_StiffnessDistrib");
        myInert = selectedBones.FindProperty("m_Inert");
        myIDistrib = selectedBones.FindProperty("m_InertDistrib");
        myRad = selectedBones.FindProperty("m_Radius");
        myRDistrib = selectedBones.FindProperty("m_RadiusDistrib");
        myEndLength = selectedBones.FindProperty("m_EndLength");
        myEndOffset = selectedBones.FindProperty("m_EndOffset");
        myGravity = selectedBones.FindProperty("m_Gravity");
        myForce = selectedBones.FindProperty("m_Force");
        myColliders = selectedBones.FindProperty("m_Colliders");
        myExclusions = selectedBones.FindProperty("m_Exclusions");
        myFreezeAxis = selectedBones.FindProperty("m_FreezeAxis");
        myDistantDisable = selectedBones.FindProperty("m_DistantDisable");
        myReferenceObject = selectedBones.FindProperty("m_ReferenceObject");
        myDistanceToObject = selectedBones.FindProperty("m_DistanceToObject");

    }


    private void OnDisable()
    {
        editOffset = false;
        editRadius = false;
        Tools.hidden = false;
    }

    private Transform getParent(Transform currentTransform)
    {
        if (currentTransform.parent == null)
            return currentTransform;
        else
            return getParent(currentTransform.parent);
    }


    private List<Transform> getChildren(Transform parent)
    {
        return parent.gameObject.GetComponentsInChildren<Transform>().ToList();
    }


    private void getBottomChildren()
    {
        bottomChildren.Clear();
        RefreshExclusionIntMask();
        for (int i=0;i<allChildren.Count;i++)
        {
            if (allChildren[i].childCount == 0 && intMask[i]!=1)
                bottomChildren.Add(allChildren[i]);
        }
    }
    private void getAllChildren()
    {
        allChildren.Clear();
        foreach (DynamicBone bone in targetBones)
        {
            allChildren = allChildren.Concat(getChildren(bone.m_Root)).ToList();
        }
        allChildren = allChildren.Distinct().ToList();
    }
    private void RefreshExclusionIntMask()
    {
        getAllChildren();
        intMask = new int[allChildren.Count];
        bool first = true;
        foreach (DynamicBone bone in targetBones)
        {
            for (int i = 0; i < intMask.Length; i++)
            {
                if (intMask[i] == 2)
                    continue;
                if (bone.m_Exclusions.Contains(allChildren.ElementAt(i)))
                {
                    if (intMask[i] == 0 && !first)
                        intMask[i] = 2;
                    else
                        intMask[i] = 1;
                }
                else
                {
                    if (intMask[i] == 1 && !first)
                        intMask[i] = 2;
                    else
                        intMask[i] = 0;
                }

            }
            first = false;
        }
    }

   

}
