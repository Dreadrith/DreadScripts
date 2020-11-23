using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;
using UnityEditor;
using System.Linq;

public class PublicMultiTransitionEditor : EditorWindow
{
    System.Func<System.Enum, bool> showEnum;
    private AnimatorController selectedController;
    private AnimatorStateMachine selectedMachine;
    private AnimatorStateTransition[] transitions;
    private List<AnimatorState> allStates;
    static GUIStyle center;
    Vector2 scrollpos;
    private bool editingExpanded = false;

    private string[] parameterOptions;
    private int paramtererIndex = 0;


    [MenuItem("DreadTools/Transition Editor %t", false, 200)]
    public static void showWindow()
    {
        GetWindow<PublicMultiTransitionEditor>(false, "Transition Editor", false);
    }



    private void OnFocus()
    {
        OnSelectionChange();
        SceneView.RepaintAll();
    }

    public void OnGUI()
    {

        if ((Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter) && Event.current.type == EventType.KeyDown)
        {
            GUI.FocusControl(null);
            Repaint();
        }
        scrollpos = EditorGUILayout.BeginScrollView(scrollpos, false, false);
        center = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };

        if (selectedController)
        {
            EditorGUIUtility.labelWidth = 180;
            if (selectedMachine != null)
                EditorGUILayout.LabelField(selectedMachine.name, center, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Select All Transitions"))
                if (allStates != null)
                {
                    foreach (AnimatorState state in allStates)
                    {
                        Selection.objects = Selection.objects.ToList().Concat(state.transitions).ToArray();
                    }
                        Selection.objects = Selection.objects.ToList().Concat(selectedMachine.anyStateTransitions).ToArray();
                    EditorApplication.ExecuteMenuItem("Window/Animation/Animator");
                }
            if (GUILayout.Button("Select All States"))
                if (allStates != null)
                {
                    Selection.objects = Selection.objects.ToList().Concat(allStates).ToArray();
                    EditorApplication.ExecuteMenuItem("Window/Animation/Animator");
                }

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent("Rename Empty Transitions", "Renames Empty Named Transitions to Source -> Destination"), GUILayout.Width(220)))
            {
                foreach (AnimatorState state in allStates)
                {
                    foreach (AnimatorStateTransition transition in state.transitions)
                    {
                        if (transition.name == "")
                            if (transition.destinationState != null)
                                transition.name = state.name + " -> " + transition.destinationState.name;
                            else
                                transition.name = state.name + " -> Exit";
                    }
                }
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent("Rename All Transitions", "Renames Every Transition to Source -> Destination"), GUILayout.Width(220)))
            {
                foreach (AnimatorState state in allStates)
                {
                    foreach (AnimatorStateTransition transition in state.transitions)
                    {
                        if (transition.destinationState != null)
                            transition.name = state.name + " -> " + transition.destinationState.name;
                        else
                            transition.name = state.name + " -> Exit";
                    }
                }
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();
        }


        if (Selection.objects.Length != 0)
        {
            if (transitions != null)
            {
                if (transitions.Length != 0)
                {
                    EditorGUIUtility.labelWidth = 140;
                    Undo.RecordObjects(transitions, "Transitions editing");

                    EditorGUILayout.BeginHorizontal();
                    EditorGUI.showMixedValue = false;
                    for (int i = 0; i < transitions.Length; i++)
                        if (transitions[i].hasExitTime != transitions[0].hasExitTime)
                        {
                            EditorGUI.showMixedValue = true;
                            break;
                        }
                    EditorGUI.BeginChangeCheck();
                    transitions[0].hasExitTime = EditorGUILayout.Toggle(new GUIContent("Has Exit Time", "Transition has a fixed Exit time"), transitions[0].hasExitTime);
                    if (EditorGUI.EndChangeCheck() && transitions.Length > 1)
                        for (int i = 1; i < transitions.Length; i++)
                            transitions[i].hasExitTime = transitions[0].hasExitTime;
                    if (!EditorGUI.showMixedValue && transitions[0].hasExitTime || EditorGUI.showMixedValue)
                    {
                        EditorGUI.showMixedValue = false;
                        for (int i = 0; i < transitions.Length; i++)
                            if (transitions[i].exitTime != transitions[0].exitTime)
                            {
                                EditorGUI.showMixedValue = true;
                                break;
                            }


                        EditorGUI.BeginChangeCheck();
                        transitions[0].exitTime = EditorGUILayout.FloatField(new GUIContent("Exit Time", "Exit time in normalized time from current state"), transitions[0].exitTime);
                        if (EditorGUI.EndChangeCheck() && transitions.Length > 1)
                            for (int i = 1; i < transitions.Length; i++)
                                transitions[i].exitTime = transitions[0].exitTime;
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUI.showMixedValue = false;
                    for (int i = 0; i < transitions.Length; i++)
                        if (transitions[i].hasFixedDuration != transitions[0].hasFixedDuration)
                        {
                            EditorGUI.showMixedValue = true;
                            break;
                        }

                    EditorGUILayout.BeginHorizontal();
                    EditorGUI.BeginChangeCheck();
                    transitions[0].hasFixedDuration = EditorGUILayout.Toggle(new GUIContent("Fixed Duration", "Transition duraton is independant of animation length"), transitions[0].hasFixedDuration);
                    if (EditorGUI.EndChangeCheck() && transitions.Length > 1)
                        for (int i = 1; i < transitions.Length; i++)
                            transitions[i].hasFixedDuration = transitions[0].hasFixedDuration;

                    EditorGUI.showMixedValue = false;
                    for (int i = 0; i < transitions.Length; i++)
                        if (transitions[i].duration != transitions[0].duration)
                        {
                            EditorGUI.showMixedValue = true;
                            break;
                        }


                    EditorGUI.BeginChangeCheck();
                    if (transitions[0].hasFixedDuration)
                        transitions[0].duration = EditorGUILayout.FloatField(new GUIContent("Transition Duration (s)", "Transition duration in seconds"), transitions[0].duration);
                    else
                        transitions[0].duration = EditorGUILayout.FloatField(new GUIContent("Transition Duration (%)", "Transition duration in normalized time from current state"), transitions[0].duration);
                    if (EditorGUI.EndChangeCheck() && transitions.Length > 1)
                        for (int i = 1; i < transitions.Length; i++)
                            transitions[i].duration = transitions[0].duration;
                    EditorGUILayout.EndHorizontal();

                    EditorGUI.showMixedValue = false;
                    for (int i = 0; i < transitions.Length; i++)
                        if (transitions[i].offset != transitions[0].offset)
                        {
                            EditorGUI.showMixedValue = true;
                            break;
                        }

                    EditorGUI.BeginChangeCheck();
                    transitions[0].offset = EditorGUILayout.FloatField(new GUIContent("Transition Offset", "Normalized start time in next state"), transitions[0].offset);
                    if (EditorGUI.EndChangeCheck() && transitions.Length > 1)
                        for (int i = 1; i < transitions.Length; i++)
                            transitions[i].offset = transitions[0].offset;

                    EditorGUI.showMixedValue = false;
                    for (int i = 0; i < transitions.Length; i++)
                        if (transitions[i].interruptionSource != transitions[0].interruptionSource)
                        {
                            EditorGUI.showMixedValue = true;
                            break;
                        }

                    EditorGUI.BeginChangeCheck();
                    transitions[0].interruptionSource = (TransitionInterruptionSource)EditorGUILayout.EnumPopup(new GUIContent("Interruption Source", "Can be interrupted by transitions from"), transitions[0].interruptionSource);
                    if (EditorGUI.EndChangeCheck() && transitions.Length > 1)
                        for (int i = 1; i < transitions.Length; i++)
                            transitions[i].interruptionSource = transitions[0].interruptionSource;

                    EditorGUI.showMixedValue = false;
                    for (int i = 0; i < transitions.Length; i++)
                        if (transitions[i].orderedInterruption != transitions[0].orderedInterruption)
                        {
                            EditorGUI.showMixedValue = true;
                            break;
                        }

                    EditorGUI.BeginChangeCheck();
                    transitions[0].orderedInterruption = EditorGUILayout.Toggle(new GUIContent("Ordered Interruption", "Can only be interrupted by higher priority transitions"), transitions[0].orderedInterruption);
                    if (EditorGUI.EndChangeCheck() && transitions.Length > 1)
                        for (int i = 1; i < transitions.Length; i++)
                            transitions[i].orderedInterruption = transitions[0].orderedInterruption;

                    EditorGUI.showMixedValue = false;
                    for (int i = 0; i < transitions.Length; i++)
                        if (transitions[i].canTransitionToSelf != transitions[0].canTransitionToSelf)
                        {
                            EditorGUI.showMixedValue = true;
                            break;
                        }

                    EditorGUI.BeginChangeCheck();
                    transitions[0].canTransitionToSelf = EditorGUILayout.Toggle("Can Transition to Self", transitions[0].canTransitionToSelf);
                    if (EditorGUI.EndChangeCheck() && transitions.Length > 1)
                        for (int i = 1; i < transitions.Length; i++)
                            transitions[i].canTransitionToSelf = transitions[0].canTransitionToSelf;
                    EditorGUI.showMixedValue = false;
                }
                else
                {
                    GUILayout.Space(45);
                    EditorGUILayout.LabelField("No Transitions Selected", center, GUILayout.ExpandWidth(true));
                    GUILayout.Space(45);
                }
            }
        }
        else
        {
            GUILayout.Space(45);
            EditorGUILayout.LabelField("No Transitions Selected", center, GUILayout.ExpandWidth(true));
            GUILayout.Space(45);
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        if (Selection.activeObject is AnimatorStateTransition && Selection.objects.Length==1)
        {
            AnimatorStateTransition selectedTransition = (AnimatorStateTransition)Selection.activeObject;
            EditorGUILayout.LabelField("Conditions:");

            foreach (AnimatorCondition condition in (selectedTransition).conditions)
            {
                AnimatorConditionMode dummyMode = AnimatorConditionMode.If;
                float dummyThreshold = 0;
                paramtererIndex = 0;
                for (int j = 0; j < parameterOptions.Length; j++)
                {
                    if (condition.parameter == parameterOptions[j])
                    {
                        paramtererIndex = j;
                        break;
                    }
                }
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                if (!(selectedController.parameters[paramtererIndex].type == AnimatorControllerParameterType.Bool))
                    paramtererIndex = EditorGUILayout.Popup(paramtererIndex, parameterOptions);
                if (!(selectedController.parameters[paramtererIndex].type == AnimatorControllerParameterType.Trigger))
                    dummyMode = (AnimatorConditionMode)EditorGUILayout.EnumPopup(new GUIContent("", ""), condition.mode, showEnum, false);
                else
                    dummyMode = AnimatorConditionMode.If;
                if (selectedController.parameters[paramtererIndex].type == AnimatorControllerParameterType.Bool && (int)dummyMode > 2)
                    dummyMode = AnimatorConditionMode.If;
                else
                if (selectedController.parameters[paramtererIndex].type == AnimatorControllerParameterType.Float && (((int)dummyMode < 3) || ((int)dummyMode > 5)) || (selectedController.parameters[paramtererIndex].type == AnimatorControllerParameterType.Int && ((int)dummyMode < 3)))
                    dummyMode = AnimatorConditionMode.Greater;
                if (selectedController.parameters[paramtererIndex].type == AnimatorControllerParameterType.Bool)
                    paramtererIndex = EditorGUILayout.Popup(paramtererIndex, parameterOptions);

                if (!(dummyMode == AnimatorConditionMode.If || dummyMode == AnimatorConditionMode.IfNot))
                {
                    char chr = Event.current.character;
                    if (chr != '-' && (chr < '0' || chr > '9') && chr != '.')
                    {
                        Event.current.character = '\0';
                    }
                    dummyThreshold = EditorGUILayout.DelayedFloatField(condition.threshold);
                    if (selectedController.parameters[paramtererIndex].type == AnimatorControllerParameterType.Int)
                        dummyThreshold = (int)dummyThreshold;

                }
                if (EditorGUI.EndChangeCheck())
                {
                    AnimatorCondition conditionToDelete = new AnimatorCondition();
                    conditionToDelete.mode = condition.mode;
                    conditionToDelete.parameter = condition.parameter;
                    conditionToDelete.threshold = condition.threshold;
                    AnimatorCondition newCondition = new AnimatorCondition();
                    newCondition.mode = dummyMode;
                    newCondition.parameter = parameterOptions[paramtererIndex];
                    newCondition.threshold = dummyThreshold;
                    List<AnimatorCondition> newConditions = (selectedTransition).conditions.ToList();
                    int index = newConditions.IndexOf(conditionToDelete);
                    newConditions.Insert(index, newCondition);
                    newConditions.RemoveAt(index + 1);
                    (selectedTransition).conditions = newConditions.ToArray();
                    OnSelectionChange();
                }
                if (GUILayout.Button("x", GUILayout.Width(20)))
                {
                    AnimatorCondition conditionToDelete = new AnimatorCondition();
                    conditionToDelete.mode = condition.mode;
                    conditionToDelete.parameter = condition.parameter;
                    conditionToDelete.threshold = condition.threshold;
                    (selectedTransition).RemoveCondition(conditionToDelete);
                    OnSelectionChange();
                }
                EditorGUILayout.EndHorizontal();

            }
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add Condition"))
            {
                if ((selectedTransition).conditions.Length > 0)
                    selectedTransition.AddCondition(selectedTransition.conditions.Last().mode, selectedTransition.conditions.Last().threshold, selectedTransition.conditions.Last().parameter);
                else
                    selectedTransition.AddCondition(AnimatorConditionMode.Greater, 0, selectedController.parameters[0].name);
                OnSelectionChange();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
        if (transitions != null)
        {
            if (transitions.Length > 0)
            {
                if (editingExpanded)
                {
                    EditorGUILayout.BeginHorizontal();
                    editingExpanded = EditorGUILayout.Foldout(editingExpanded, "Editing " + transitions.Length + " Transitions");
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("x", center))
                    {
                        Selection.objects = Selection.objects.Except(transitions).ToArray();
                        EditorApplication.ExecuteMenuItem("Window/Animation/Animator");
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.FlexibleSpace();
                    GUILayout.FlexibleSpace();
                    GUILayout.FlexibleSpace();
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    int i = 0;
                    foreach (AnimatorStateTransition trans in transitions)
                    {
                        if (!trans)
                            continue;
                        if (i % 2 == 0)
                        {
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                        }
                        if (trans.name == "")
                            EditorGUILayout.LabelField(trans.GetDisplayName(trans), center, GUILayout.ExpandWidth(true));
                        else
                            EditorGUILayout.LabelField(trans.name, center, GUILayout.ExpandWidth(true));
                        i++;
                        if (GUILayout.Button("x", center, GUILayout.Width(15)))
                        {
                            List<Object> newObjects = Selection.objects.ToList();
                            newObjects.Remove(trans);
                            Selection.objects = newObjects.ToArray();
                        }
                    }
                    if (i % 2 == 1)
                    {
                        EditorGUILayout.LabelField("");
                        GUILayout.Space(19);
                    }
                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(10);
                }
                else
                    editingExpanded = EditorGUILayout.Foldout(editingExpanded, "");
            }
        }

        if (!PlayerPrefs.HasKey("TransitionEditorIsRead"))
        {
            PlayerPrefs.SetInt("TransitionEditorIsRead", 0);
            PlayerPrefs.Save();
        }
        if (PlayerPrefs.GetInt("TransitionEditorIsRead") != 1)
        {
            if (GUILayout.Button("Made by Dreadrith#3238"))
            {
                PlayerPrefs.SetInt("TransitionEditorIsRead", 1);
                PlayerPrefs.Save();
            }
        }
        GUILayout.EndScrollView();
    }



    public void OnSelectionChange()
    {
        transitions = Selection.GetFiltered<AnimatorStateTransition>(SelectionMode.OnlyUserModifiable);

        foreach (AnimatorState anotherstate in allStates.ToList())
        {
            if (anotherstate == null)
            {
                populateStates(selectedMachine);
                break;
            }
        }

        if (Selection.activeObject is AnimatorState || Selection.activeObject is AnimatorStateTransition)
        {
            AnimatorState stateCheck = new AnimatorState();
            if (Selection.activeObject is AnimatorState)
                stateCheck = (AnimatorState)Selection.activeObject;
            else
            {
                stateCheck = ((AnimatorStateTransition)Selection.activeObject).destinationState;
            }
                
            if (stateCheck && allStates != null)
                if (!allStates.Contains(stateCheck))
                {
                    selectedController = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GetAssetPath(stateCheck.GetInstanceID()));
                    updateParameterOptions();
                    foreach (AnimatorControllerLayer layer in selectedController.layers)
                    {
                        bool found = false;
                        foreach (ChildAnimatorState child in layer.stateMachine.states)
                        {

                            if (child.state == stateCheck)
                            {
                                selectedMachine = layer.stateMachine;
                                populateStates(selectedMachine);
                                found = true;
                                break;
                            }
                        }
                        if (found)
                            break;
                    }
                }
        }

        if (Selection.activeObject is AnimatorStateMachine)
        {
            if (!selectedMachine)
            {
                selectedMachine = (AnimatorStateMachine)Selection.activeObject;
                selectedController = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GetAssetPath(selectedMachine.GetInstanceID()));
                updateParameterOptions();
                populateStates(selectedMachine);
            }
            else
                if (selectedMachine != (AnimatorStateMachine)Selection.activeObject)
            {
                selectedMachine = (AnimatorStateMachine)Selection.activeObject;
                selectedController = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GetAssetPath(selectedMachine.GetInstanceID()));
                updateParameterOptions();
                populateStates(selectedMachine);
            }
        }

        Repaint();
    }

    public void onUndoRedo()
    {
        Repaint();
    }

    public void OnEnable()
    {
        allStates = new List<AnimatorState>();
        Undo.undoRedoPerformed += onUndoRedo;
        showEnum = showEnumMethod;
    }

    private bool showEnumMethod(System.Enum myenum)
    {

        if (selectedController)
        {
            bool isFloat = (int)selectedController.parameters[paramtererIndex].type == 1 ? true : false;
            bool isInt = (int)selectedController.parameters[paramtererIndex].type == 3 ? true : false;
            bool isBool = (int)selectedController.parameters[paramtererIndex].type == 4 ? true : false;
            bool isTrigger = (int)selectedController.parameters[paramtererIndex].type == 9 ? true : false;

            //Parameter Type
            // 1 -> Float
            // 3 -> Int
            // 4 -> Bool
            // 9 -> Trigger

            //Condition Mode
            // 1 -> If
            // 2 -> IfNot
            // 3 -> Greater
            // 4 -> Less
            // 6 -> Equal
            // 7 -> NotEqual
            switch ((int)(AnimatorConditionMode)myenum)
            {
                case 1:
                case 2: return isBool;
                case 3:
                case 4: return isFloat || isInt;
                case 6:
                case 7: return !isFloat && !isTrigger && !isBool;
            }
        }
        return true;
    }

    private void populateStates(AnimatorStateMachine machine)
    {
        if (allStates == null)
            allStates = new List<AnimatorState>();
        allStates.Clear();
        if (machine)
            foreach (ChildAnimatorState child in machine.states)
                allStates.Add(child.state);
    }

    private void updateParameterOptions()
    {
        parameterOptions = new string[selectedController.parameters.Length];
        for (int i = 0; i < parameterOptions.Length; i++)
            parameterOptions[i] = selectedController.parameters[i].name;
    }

}

