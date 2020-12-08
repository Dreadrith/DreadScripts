using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;

namespace DS_CommonMethods
{
    public static class DSCommonMethods
    {
        private static Texture2D discordIcon = Resources.Load<Texture2D>("DS_DiscordIcon");
        private static Texture2D githubIcon = Resources.Load<Texture2D>("DS_GithubIcon");
        


        #region Asset Methods
        /// <param name="script">The script whose path to get.</param>
        /// <returns>The Path to the script relative to Assets folder</returns>
        public static string GetPath(this ScriptableObject script)
        {
            string myPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(script));
            myPath = myPath.Substring(0, myPath.LastIndexOf('/'));
            return myPath;
        }

        /// <param name="script">The script whose path to get.</param>
        /// <returns>The Path to the script relative to Assets folder</returns>
        public static string GetPath(this MonoBehaviour script)
        {
            string myPath = AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(script));
            myPath = myPath.Substring(0, myPath.LastIndexOf('/'));
            return myPath;
        }


        public static T GetAsset<T>(this T obj) where T : Object
        {
            T myAsset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GetAssetPath(obj));
            if (myAsset != obj)
            {
                Object[] assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(obj));
                foreach (Object asset in assets)
                    if (asset == obj)
                        return asset as T;
                return myAsset;
            }
            else
                return myAsset;
        }

        public static T CopyAssetAndReturn<T>(string path, string newpath) where T : Object
        {
            if (path != newpath)
                AssetDatabase.CopyAsset(path, newpath);
            return AssetDatabase.LoadAssetAtPath<T>(newpath);

        }

        public static T CopyAssetAndReturn<T>(Object obj, string newpath) where T : Object
        {
            Object newAsset = Object.Instantiate(obj);
            AssetDatabase.CreateAsset(newAsset, newpath);
            return AssetDatabase.LoadAssetAtPath<T>(newpath);
        }
        
        /// <summary>
        /// Given a Folder Path, Checks if each folder exits and creates it if needed
        /// </summary>
        /// <param name="fullPath">Path of the desired folders to exist</param>
        public static void RecreateFolders(string fullPath)
        {
            string[] folderNames = fullPath.Split('/');
            string[] folderPaths = new string[folderNames.Length];
            for (int i = 0; i < folderNames.Length; i++)
            {
                folderPaths[i] = folderNames[0];
                for (int j = 1; j <= i; j++)
                {
                    folderPaths[i] = folderPaths[i] + "/" + folderNames[j];
                }
            }
            for (int i = 0; i < folderPaths.Length; i++)
            {
                if (!AssetDatabase.IsValidFolder(folderPaths[i]))
                {
                    AssetDatabase.CreateFolder(folderPaths[i].Substring(0, folderPaths[i].LastIndexOf('/')), folderPaths[i].Substring(folderPaths[i].LastIndexOf('/') + 1, folderPaths[i].Length - folderPaths[i].LastIndexOf('/') - 1));
                }

            }
        }

        #endregion

        #region Animator Methods
        #region Animator Avatar Methods
        public static void IterateBoneTransforms(Animator ani, System.Action<Transform,int> action,List<int> except=null)
        {
            List<int> validNums = new List<int>();
            for (int i = 0; i < 55; i++)
                validNums.Add(i);
            if (except != null)
                foreach (int i in except)
                    validNums.Remove(i);

            for (int i = 0; i < validNums.Count; i++)
            {
                Transform bone;
                if (bone = ani.GetBoneTransform((HumanBodyBones)validNums[i]))
                {
                    action(bone,validNums[i]);
                }
            }
        }

        public static void IterateBoneTransforms(Animator ani,Animator ani2, System.Action<Transform,Transform,int> action,bool flipParts=false, List<int> except = null)
        {
            
            List<int> validNums = new List<int>();
            for (int i = 0; i < 55; i++)
                validNums.Add(i);
            if (except != null)
                foreach (int i in except)
                    validNums.Remove(i);

            int currentNum, variant;
            for (int i = 0; i < validNums.Count; i++)
            {
                variant = 0;
                currentNum = validNums[i];
                if (flipParts)
                {
                    if ((currentNum > 10 && currentNum < 23) || (currentNum > 0 && currentNum < 7))
                        if (currentNum % 2 == 1)
                            variant = 1;
                        else
                            variant = -1;

                    if (currentNum > 23 && currentNum < 39)
                        variant = 15;
                    if (currentNum > 38 && currentNum < 54)
                        variant = -15;
                }
                Transform source,target;
                if (source = ani.GetBoneTransform((HumanBodyBones)currentNum))
                {
                    if (target = ani2.GetBoneTransform((HumanBodyBones)currentNum + variant)) 
                    {
                        action(source,target,currentNum);
                    }
                }
            }
        }
        #endregion
        #region Animator Controller Methods

        /// <param name="controller">The RuntimeAnimatorController whose Controller to get.</param>
        /// <returns>The AnimatorController Asset.</returns>
        public static AnimatorController GetController(this RuntimeAnimatorController controller)
        {
            return AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GetAssetPath(controller));
        }
        public static AnimatorController ReplaceController(this RuntimeAnimatorController controller, string copyPath, bool ReplaceAnimations, Dictionary<Motion, Motion> motionDict = null, System.Action<Motion> motionAction = null)
        {
            if (motionDict == null)
                motionDict = new Dictionary<Motion, Motion>();

            AnimatorController newCon = CopyAssetAndReturn<AnimatorController>(controller.GetController(), AssetDatabase.GenerateUniqueAssetPath(copyPath + "/" + controller.name + ".controller"));

            if (ReplaceAnimations)
            {
                foreach (AnimatorControllerLayer layer in newCon.layers)
                    ReplaceMachine(layer.stateMachine, copyPath, motionDict, motionAction);
            }
            return newCon;
        }

        public static void ReplaceMachine(AnimatorStateMachine machine, string copyPath, Dictionary<Motion, Motion> motionDict = null, System.Action<Motion> motionAction = null)
        {
            foreach (ChildAnimatorState childState in machine.states)
            {
                if (childState.state.motion != null)
                {
                    childState.state.motion = ReplaceMotion(childState.state.motion, copyPath, motionDict, motionAction);
                }
            }
            foreach (ChildAnimatorStateMachine childMachine in machine.stateMachines)
            {
                if (childMachine.stateMachine != machine)
                    ReplaceMachine(childMachine.stateMachine, copyPath, motionDict, motionAction);
            }
        }

        public static Motion ReplaceMotion(Motion motion, string copyPath, Dictionary<Motion, Motion> motionDict = null, System.Action<Motion> motionAction = null)
        {
            if (motionDict != null)
                if (motionDict.ContainsKey(motion))
                    return motionDict[motion];

            if (motion is AnimationClip)
            {
                AnimationClip newMotion = CopyAssetAndReturn<AnimationClip>(motion, AssetDatabase.GenerateUniqueAssetPath(copyPath + "/" + motion.name + ".anim"));
                motionAction?.Invoke(newMotion);

                if (motionDict != null)
                    motionDict.Add(motion, newMotion);

                return newMotion;
            }
            else
            {
                if (motion is BlendTree tree)
                {
                    string treepath = AssetDatabase.GetAssetPath(motion);
                    BlendTree newMotion = AssetDatabase.LoadAssetAtPath<BlendTree>(treepath);

                    if (newMotion)
                    {
                        string newAssetPath = AssetDatabase.GenerateUniqueAssetPath(copyPath + "/" + motion.name + ".blendtree");
                        newMotion = CopyAssetAndReturn<BlendTree>(treepath, newAssetPath);
                    }
                    else
                        newMotion = tree;

                    motionAction?.Invoke(newMotion);

                    ChildMotion[] newChildren = newMotion.children;
                    for (int i = 0; i < newMotion.children.Length; i++)
                    {
                        if (newChildren[i].motion)
                            newChildren[i].motion = ReplaceMotion(newMotion.children[i].motion, copyPath, motionDict, motionAction);
                    }
                    newMotion.children = newChildren;
                    motionDict.Add(motion, newMotion);

                    return newMotion;
                }
            }
            return null;
        }

        /// <param name="parameter">The Name of the AnimatorParameter</param>
        /// <returns>True if the Parameter was found. False otherwise.</returns>
        public static bool HasParameter(this AnimatorController controller, string parameter)
        {
            for (int i = 0; i < controller.parameters.Length; i++)
            {
                if (controller.parameters[i].name == parameter)
                {
                    return true;
                }
            }
            return false;
        }

        /// <param name="parameter">The Name of the AnimatorParameter</param>
        /// <returns>True if the Parameter was found and removed. False otherwise.</returns>
        public static bool RemoveParameter(this AnimatorController controller, string parameter)
        {

            for (int i = 0; i < controller.parameters.Length; i++)
            {
                if (controller.parameters[i].name == parameter)
                {
                    controller.RemoveParameter(i);
                    return true;
                }
            }
            return false;
        }

        

        /// <summary>
        /// Creates Layer before adding it, allowing you to modify parameters beforehand such as Layer Weight.
        /// </summary>
        /// <param name="controller">Controller that you'll add the layer to</param>
        /// <param name="layerName">Name of the new Layer</param>
        /// <returns>The newly created layer. Make sure to use AnimatorController.AddLayer(newLayer) afterwards!.</returns>
        public static AnimatorControllerLayer CreateLayer(this AnimatorController controller, string layerName)
        {
            AnimatorControllerLayer newlayer = new AnimatorControllerLayer();
            newlayer.name = layerName;
            newlayer.stateMachine = new AnimatorStateMachine();
            newlayer.stateMachine.name = layerName;
            newlayer.stateMachine.hideFlags = HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(newlayer.stateMachine, controller);
            return newlayer;
        }


        public static bool RemoveLayer(this AnimatorController controller, string layerName)
        {
            for (int i = 0; i < controller.layers.Length; i++)
            {
                if (controller.layers[i].name == layerName)
                {
                    controller.RemoveLayer(i);
                    return true;
                }
            }
            return false;
        }


        public static void GetUsedValues(AnimatorController controller, string parameter, ref bool[] array)
        {
            foreach (AnimatorControllerLayer layer in controller.layers)
            {
                GetUsedValues(layer.stateMachine, parameter, ref array);
            }
        }
        public static void GetUsedValues(AnimatorStateMachine machine, string parameter, ref bool[] array)
        {
            foreach (AnimatorTransitionBase transition in machine.entryTransitions)
            {
                foreach (AnimatorCondition condition in transition.conditions)
                {
                    if (condition.parameter == parameter)
                        if (condition.threshold < 255)
                            array[(int)condition.threshold] = true;
                }
            }
            foreach (AnimatorTransitionBase transition in machine.anyStateTransitions)
            {
                foreach (AnimatorCondition condition in transition.conditions)
                {
                    if (condition.parameter == parameter)
                        if (condition.threshold < 255)
                            array[(int)condition.threshold] = true;
                }
            }
            foreach (ChildAnimatorState state in machine.states)
            {
                foreach (AnimatorTransitionBase transition in state.state.transitions)
                {
                    foreach (AnimatorCondition condition in transition.conditions)
                    {
                        if (condition.parameter == parameter)
                            if (condition.threshold < 255)
                                array[(int)condition.threshold] = true;
                    }
                }
            }
            foreach (ChildAnimatorStateMachine submachine in machine.stateMachines)
            {
                if (machine != submachine.stateMachine)
                    GetUsedValues(submachine.stateMachine, parameter, ref array);
            }
        }

        #endregion
        #region Animator Transition Methods

        //Add transition to state while also specifying transition duration
        public static AnimatorStateTransition AddTransition(this AnimatorState sourceState, AnimatorState destinationState, bool defaultExitTime, float transitionDuration)
        {
            AnimatorStateTransition transition = sourceState.AddTransition(destinationState, defaultExitTime);
            transition.duration = transitionDuration;
            return transition;
        }

        //Add transition to state while also specifying exit time and transition duration
        public static AnimatorStateTransition AddTransition(this AnimatorState sourceState, AnimatorState destinationState, bool defaultExitTime, float exitTime, float transitionDuration)
        {
            AnimatorStateTransition transition = sourceState.AddTransition(destinationState, defaultExitTime);
            transition.exitTime = exitTime;
            transition.duration = transitionDuration;
            return transition;
        }

        //Add Exit transition to state while also specifying transition duration
        public static AnimatorStateTransition AddExitTransition(this AnimatorState sourceState, bool defaultExitTime, float transitionDuration)
        {
            AnimatorStateTransition transition = sourceState.AddExitTransition(defaultExitTime);
            transition.duration = transitionDuration;
            return transition;
        }

        //Add Exit transition to state while also specifying exit time and transition duration
        public static AnimatorStateTransition AddExitTransition(this AnimatorState sourceState, bool defaultExitTime, float exitTime, float transitionDuration)
        {
            AnimatorStateTransition transition = sourceState.AddExitTransition(defaultExitTime);
            transition.exitTime = exitTime;
            transition.duration = transitionDuration;
            return transition;
        }

        public static AnimatorCondition AddCondition(this AnimatorTransitionBase transition, string parameter, bool condition)
        {
            AnimatorCondition c = new AnimatorCondition();
            switch (condition)
            {
                case false:
                    c.mode = AnimatorConditionMode.IfNot;
                    break;
                case true:
                    c.mode = AnimatorConditionMode.If;
                    break;
            }
            c.threshold = condition ? 1 : 0;
            c.parameter = parameter;
            transition.AddCondition(c);
            return c;
        }

        //Add Condition to transition and return the transition
        public static T AddConditionRT<T>(this T transition, AnimatorConditionMode mode, float threshold, string parameter) where T : AnimatorTransitionBase
        {
            transition.AddCondition(mode, threshold, parameter);
            return transition;
        }

        //Add Condition to transition and return the condition
        public static AnimatorCondition AddConditionRC(this AnimatorTransitionBase transition, AnimatorConditionMode mode, float threshold, string parameter)
        {
            AnimatorCondition c = new AnimatorCondition
            {
                mode = mode,
                threshold = threshold,
                parameter = parameter
            };
            transition.AddCondition(c);
            return c;
        }

        //Add Condition to transition
        public static T AddCondition<T>(this T transition, AnimatorCondition condition) where T : AnimatorTransitionBase
        {
            transition.AddCondition(condition.mode, condition.threshold, condition.parameter);
            return transition;
        }

        //Add Condition Array to transition
        public static void AddConditions(this AnimatorTransitionBase transition, AnimatorCondition[] conditions)
        {
            for (int i = 0; i < conditions.Length; i++)
            {
                transition.AddCondition(conditions[i]);
            }
        }
        #endregion
        #endregion

        #region ComponentMethods


        public static void LockConstraints(HashSet<IConstraint> cons, bool clear = true)
        {
            foreach (IConstraint con in cons)
                con.locked = true;
            if (clear)
                cons.Clear();
        }

        public static PositionConstraint PositionConstrain(GameObject source, GameObject target,HashSet<IConstraint> cons=null, Axis axis = Axis.X | Axis.Y | Axis.Z, float w = 1)
        {
            PositionConstraint con;
            if (!(con = source.GetComponent<PositionConstraint>()))
            {
                con = source.AddComponent<PositionConstraint>();
                con.translationAxis = axis;
                con.constraintActive = true;
                con.weight = 1;
            }
            else
                con.locked = false;
            con.AddSource(new ConstraintSource { sourceTransform = target.transform, weight = w });
            if (cons != null)
                if (!cons.Contains(con))
                    cons.Add(con);
            return con;
        }

        public static RotationConstraint RotationConstrain(GameObject source, GameObject target, HashSet<IConstraint> cons=null, Axis axis = Axis.X | Axis.Y | Axis.Z, float w = 1)
        {
            RotationConstraint con;
            if (!(con = source.GetComponent<RotationConstraint>()))
            {
                con = source.AddComponent<RotationConstraint>();
                con.rotationAxis = axis;
                con.constraintActive = true;
                con.weight = 1;
            }
            else
                con.locked = false;

            con.AddSource(new ConstraintSource { sourceTransform = target.transform, weight = w });
            if (cons != null)
                if (!cons.Contains(con))
                    cons.Add(con);
            return con;
        }

        public static RotationConstraint RotationConstrainLock(GameObject source, GameObject target)
        {
            RotationConstraint con = source.AddComponent<RotationConstraint>();
            con.constraintActive = true;
            con.weight = 1;
            con.rotationAxis = Axis.X | Axis.Y | Axis.Z;
            con.rotationAtRest = source.transform.localRotation.eulerAngles;
            con.rotationOffset = (Quaternion.Inverse(target.transform.rotation) * source.transform.rotation).eulerAngles;
            con.AddSource(new ConstraintSource { sourceTransform = target.transform, weight = 1 });
            con.locked = true;
            return con;
        }

        public static ParentConstraint ParentConstrain(GameObject source, GameObject target, HashSet<IConstraint> cons=null, Axis posAxis = Axis.X | Axis.Y | Axis.Z, Axis rotAxis = Axis.X | Axis.Y | Axis.Z, float w = 1)
        {
            ParentConstraint con;
            if (!(con = source.GetComponent<ParentConstraint>()))
            {
                con = source.AddComponent<ParentConstraint>();
                con.rotationAxis = rotAxis;
                con.translationAxis = posAxis;
                con.constraintActive = true;
                con.weight = 1;
            }
            else
                con.locked = false;

            con.AddSource(new ConstraintSource { sourceTransform = target.transform, weight = w });
            if (cons != null)
                if (!cons.Contains(con))
                    cons.Add(con);
            return con;
        }

        #endregion

        #region GUIMethods
        /// <summary>
        /// Automatically lays out the Asset Path GUI for a given string
        /// Opens Folder Panel and restricts you to choose a Folder within Assets, otherwise returns Empty.
        /// Automatically cuts System path and only returns the path starting from Assets.
        /// Saves new chosen path to PlayerPrefs under the "playerpref" key.
        /// </summary>
        /// <param name="variable">Variable to save new path to</param>
        /// <param name="title">Title of the opened Panel</param>
        /// <param name="playerpref">Key of the PlayerPref to save to</param>
        /// <returns></returns>
        public static void AssetFolderPath(ref string variable,string title, string playerpref)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField(title, variable);
            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                string dummyPath = EditorUtility.OpenFolderPanel(title, variable, "");
                if (string.IsNullOrEmpty(dummyPath))
                    return;

                if (!dummyPath.Contains("Assets"))
                {
                    Debug.LogWarning("New Path must be a folder within Assets!");
                    return;
                }
                variable = FileUtil.GetProjectRelativePath(dummyPath);
                PlayerPrefs.SetString(playerpref, variable);
            }
            EditorGUILayout.EndHorizontal();
        }
        #endregion

        #region Other Methods
        /// <summary>
        /// Adds The Children GameObjects of parent that are Type T to the List
        /// </summary>
        /// <param name="myList">List being added to</param>
        /// <param name="parent">The GameObject whose Type is being currently inspected</param>
        /// <param name="T">Type of GameObject to include</param>
        /// <param name="includeParent">Whether to include the parent in the inspection or not</param>
        public static void AddChildrenOfType(this List<GameObject> myList, GameObject parent, System.Type T, bool includeParent = false)
        {
            if (includeParent)
                if (parent.GetComponent(T))
                    myList.Add(parent);
            for (int i = 0; i < parent.transform.childCount; i++)
            {
                myList.AddChildrenOfType(parent.transform.GetChild(i).gameObject, T, true);
            }
        }

        //My Watermark for my work
        public static void Credit()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.SelectableLabel("by Dreadrith#3238", GUILayout.Width(115));
            if (GUILayout.Button(githubIcon, GUIStyle.none, GUILayout.Width(32), GUILayout.Height(32)))
                Application.OpenURL("https://github.com/Dreadrith/DreadScripts");
            if (GUILayout.Button(discordIcon, GUIStyle.none, GUILayout.Width(32), GUILayout.Height(32)))
                Application.OpenURL("https://discord.gg/ZsPfrGn");
            EditorGUILayout.EndHorizontal();
        }
        #endregion

        public static readonly Dictionary<int, int> armatureHierarchy = new Dictionary<int, int>
    {
        {0,0},
        {1,0},
        {2,0},
        {3,1},
        {4,2},
        {5,3},
        {6,4},
        {7,0},
        {8,7},
        {9,8},
        {10,9},
        {11,8},
        {12,8},
        {13,11},
        {14,12},
        {15,13},
        {16,14},
        {17,15},
        {18,16},
        {19,5},
        {20,6},
        {21,10},
        {22,10},
        {23,10},
        {24,17},
        {25,24},
        {26,25},
        {27,17},
        {28,27},
        {29,28},
        {30,17},
        {31,30},
        {32,31},
        {33,17},
        {34,33},
        {35,34},
        {36,17},
        {37,36},
        {38,37},
        {39,18},
        {40,39},
        {41,40},
        {42,18},
        {43,42},
        {44,43 },
        {45,18 },
        {46,45 },
        {47,46 },
        {48,18 },
        {49,48 },
        {50,49 },
        {51,18 },
        {52,51 },
        {53,52 },
        {54,8 },
        {55,0 }
    };
    }
}
