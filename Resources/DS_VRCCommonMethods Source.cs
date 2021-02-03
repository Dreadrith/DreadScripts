#if UNITY_EDITOR
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDKBase;
using UnityEditor.Animations;
using System.Linq;
using DS_CommonMethods;
using UnityEditor;
using UnityEngine;


using System.Collections.Generic;

namespace DS_VRCCommonMethods
{
   
    public static class DSVRCCommonMethods
    {

        public static readonly string[] builtinParameters = new string[] {
    "IsLocal",
    "Viseme",
    "GestureLeft",
    "GestureRight",
    "GestureLeftWeight",
    "GestureRightWeight",
    "AngularY",
    "VelocityX",
    "VelocityZ",
    "Upright",
    "Grounded",
    "Seated",
    "AFK",
    "TrackingType",
    "VRMode",
    "MuteSelf",
    "InStation"
        };

        /// <summary>
        /// Changes all the Tracking Parts of the behaviour to the specified type.
        /// </summary>
        /// <param name="behaviour">The Behaviour to modify.</param>
        /// <param name="type">New tracking type.</param>
        public static void trackingAll(this VRCAnimatorTrackingControl behaviour, VRC_AnimatorTrackingControl.TrackingType type)
        {
            behaviour.trackingHead = type;
            behaviour.trackingEyes = type;
            behaviour.trackingHip = type;
            behaviour.trackingLeftFingers = type;
            behaviour.trackingLeftFoot = type;
            behaviour.trackingLeftHand = type;
            behaviour.trackingMouth = type;
            behaviour.trackingRightFingers = type;
            behaviour.trackingRightFoot = type;
            behaviour.trackingRightHand = type;
        }

        public static void IterateStates(this VRCAvatarDescriptor avi, System.Action<AnimatorState> action, bool nested = true)
        {
            for (int i = 0; i < avi.baseAnimationLayers.Length; i++)
            {
                RuntimeAnimatorController run;
                if ((run = avi.baseAnimationLayers[i].animatorController))
                    DSCommonMethods.IterateStates(run.GetController(), action);
            }
            for (int i = 0; i < avi.specialAnimationLayers.Length; i++)
            {
                RuntimeAnimatorController run;
                if ((run = avi.specialAnimationLayers[i].animatorController))
                    DSCommonMethods.IterateStates(run.GetController(), action);
            }
            if (nested)
            {
                Animator[] animators = avi.GetComponentsInChildren<Animator>();
                {
                    foreach (Animator ani in animators)
                    {
                        RuntimeAnimatorController run;
                        if ((run = ani.runtimeAnimatorController))
                            DSCommonMethods.IterateStates(run.GetController(), action);
                    }
                }
            }
        }

        public static bool Remove(this VRCExpressionsMenu menu, string name)
        {
            return menu.controls.Remove(menu.controls.Find((VRCExpressionsMenu.Control c) => c.name == name));
        }

        public static bool Remove(this VRCExpressionParameters exparam, string name)
        {
            for (int i = 0; i < exparam.parameters.Length; i++)
            {
                if (exparam.parameters[i].name == name)
                {
                    exparam.parameters[i].name = "";
                    return true;
                }
            }
            return false;
        }

        public static int AddOrFind(this VRCExpressionParameters exparam, string name, VRCExpressionParameters.ValueType type)
        {
            for (int i = 0; i < exparam.parameters.Length; i++)
            {
                if (exparam.parameters[i].name == name && exparam.parameters[i].valueType == type)
                {
                    return i;
                }
            }
            for (int i = 0; i < exparam.parameters.Length; i++)
            {
                if (exparam.parameters[i].name == "")
                {
                    exparam.parameters[i].name = name;
                    exparam.parameters[i].valueType = type;
                    return i;
                }
            }
            return -1;
        }

        public static int HasParameter(this VRCExpressionParameters expressionParameters, string name)
        {
            for (int i = 0; i < expressionParameters.parameters.Length; i++)
            {
                if (expressionParameters.parameters[i].name == name)
                    return i;
            }
            return -1;
        }

        public static bool[] GetUsedValues(this VRCAvatarDescriptor avi, string parameter,bool nested=true)
        {
            bool[] usedValues = new bool[256];

            foreach (VRCAvatarDescriptor.CustomAnimLayer playlayer in avi.baseAnimationLayers.Concat(avi.specialAnimationLayers))
            {
                AnimatorController myController = playlayer.animatorController?.GetController();
                if (myController)
                    DSCommonMethods.GetUsedValues(myController, parameter, ref usedValues);
            }
            if (nested)
            {
                foreach(Animator anim in avi.gameObject.GetComponentsInChildren<Animator>(true))
                {
                    AnimatorController subController = anim.runtimeAnimatorController?.GetController();
                    if (subController)
                        DSCommonMethods.GetUsedValues(subController, parameter, ref usedValues);
                }
            }

            return usedValues;
        }

        public static AnimatorController[] ReplaceControllers(VRCAvatarDescriptor avi, string folderPath, System.Action<Motion> motionAction = null, Dictionary<Motion, Motion> motionDict=null, bool nested = true)
        {
            DSCommonMethods.RecreateFolders(folderPath);
            List<AnimatorController> controllers = new List<AnimatorController>();
            Dictionary<RuntimeAnimatorController, AnimatorController> conDict = new Dictionary<RuntimeAnimatorController, AnimatorController>();
            if (motionDict == null)
                motionDict = new Dictionary<Motion, Motion>();

            AnimatorController conFunc(RuntimeAnimatorController runtime)
            {
                if (conDict.ContainsKey(runtime))
                    return conDict[runtime];

                AnimatorController newCon = DSCommonMethods.ReplaceController(runtime, folderPath, motionAction, motionDict);

                conDict.Add(runtime, newCon);
                controllers.Add(newCon);
                return newCon;
            }

            for (int i = 0; i < avi.baseAnimationLayers.Length; i++)
            {
                RuntimeAnimatorController con;
                if (con = avi.baseAnimationLayers[i].animatorController)
                {
                    avi.baseAnimationLayers[i].animatorController = conFunc(con);
                }
            }
            for (int i = 0; i < avi.specialAnimationLayers.Length; i++)
            {
                RuntimeAnimatorController con;
                if (con = avi.specialAnimationLayers[i].animatorController)
                {
                    avi.specialAnimationLayers[i].animatorController = conFunc(con);
                }
            }
            if (nested)
                foreach (Animator ani in avi.GetComponentsInChildren<Animator>())
                {
                    RuntimeAnimatorController con;
                    if (con = ani.runtimeAnimatorController)
                        ani.runtimeAnimatorController = conFunc(con);
                }
            return controllers.ToArray();
        }

        

        public static List<VRCExpressionsMenu.Control> GetControls(this VRCExpressionsMenu source, params int[] except)
        {
            List<VRCExpressionsMenu.Control> newCons = new List<VRCExpressionsMenu.Control>();
            for (int i=0;i<source.controls.Count;i++)
            {
                VRCExpressionsMenu.Control c = source.controls[i];
                if (except != null)
                    if (except.Contains(i))
                        continue;
                VRCExpressionsMenu.Control.Parameter[] newSubParams = new VRCExpressionsMenu.Control.Parameter[c.subParameters.Length];
                for (int j=0;j<newSubParams.Length;j++)
                {
                    newSubParams[j] = new VRCExpressionsMenu.Control.Parameter() { name = c.subParameters[j].name };
                }
                VRCExpressionsMenu.Control newCon = new VRCExpressionsMenu.Control() { icon = c.icon, name = c.name, parameter = new VRCExpressionsMenu.Control.Parameter() { name = c.parameter.name }, style = c.style, subMenu = c.subMenu, subParameters = newSubParams, type = c.type, value = c.value };
                
                newCons.Add(newCon);
            }
            return newCons;
        }

        public static bool CheckControls(this VRCExpressionsMenu target, List<VRCExpressionsMenu.Control> newCons,bool debug=true)
        {
            if (!target)
                return true;

            if (target.controls.Count + newCons.Count > 8)
            {
                if (debug)
                    Debug.LogError(target.name + " does not have enough free control slots to contain "+newCons.Count+" more controls!");
                return false;
            }
            else
                return true;
        }

        public static void AddControls(this VRCExpressionsMenu target, List<VRCExpressionsMenu.Control> newCons,string suffix="")
        {
            for (int i = 0; i < newCons.Count; i++)
            {
                if (!string.IsNullOrEmpty(suffix))
                {
                    newCons[i].name += suffix;
                    newCons[i].parameter.name += suffix;
                    VRCExpressionsMenu.Control.Parameter[] newSubParams = new VRCExpressionsMenu.Control.Parameter[newCons[i].subParameters.Length];
                    for (int j = 0; j < newCons[j].subParameters.Length; j++)
                    {
                        newSubParams[j] = newCons[i].subParameters[j];
                        newSubParams[j].name += suffix;
                    }
                    newCons[i].subParameters = newSubParams;
                }

                target.controls.Add(newCons[i]);
            }
            EditorUtility.SetDirty(target);
        }

        public static List<VRCExpressionParameters.Parameter> GetParameters(this VRCExpressionParameters source,params int[] except)
        {
            List<VRCExpressionParameters.Parameter> myParams = new List<VRCExpressionParameters.Parameter>();
            for (int i = 0; i < source.parameters.Length; i++)
            {
                if (except != null)
                    if (except.Contains(i))
                        continue;

                if (!string.IsNullOrEmpty(source.parameters[i].name))
                {
                    VRCExpressionParameters.Parameter p = source.parameters[i];
                    VRCExpressionParameters.Parameter newParam = new VRCExpressionParameters.Parameter() { defaultValue = p.defaultValue, name = p.name, saved = p.saved, valueType = p.valueType };
                    myParams.Add(newParam);
                }
                
            }
            return myParams;
        }

        public static bool CheckParameters(this VRCExpressionParameters target, List<VRCExpressionParameters.Parameter> newParams,bool debug=true)
        {
            if (!target)
                return true;

            int cost = 0;
            for (int i = 0; i < newParams.Count; i++)
            {
                if (newParams[i].valueType == VRCExpressionParameters.ValueType.Bool)
                    cost++;
                else
                    cost += 8;
            }

            if (target.CalcTotalCost() + cost > 128)
            {
                if (debug)
                    Debug.LogError(target.name + "requires "+cost+" more free memory to contain the new parameters!");
                return false;
            }
            else
                return true;
        }

        public static bool CheckParameters(this VRCExpressionParameters target, int cost, bool debug = true)
        {
            if (!target)
                return true;
            if (target.parameters == null)
                return true;
            if (target.parameters.Length == 0)
                return true;

            if (target.CalcTotalCost() + cost > 128)
            {
                if (debug)
                    UnityEngine.Debug.LogError(target.name + "requires " + cost + " more free memory to contain the new parameters!");
                return false;
            }
            else
                return true;
        }

        public static void AddParameters(this VRCExpressionParameters target, List<VRCExpressionParameters.Parameter> newParams, string suffix="")
        {
            if (!string.IsNullOrEmpty(suffix))
            {
                for (int i=0;i<newParams.Count;i++)
                {
                    newParams[i].name += suffix;
                }
            }

            if (target.parameters == null)
                target.parameters = newParams.ToArray();
            else
                target.parameters = target.parameters.Concat(newParams).ToArray();

            EditorUtility.SetDirty(target);
        }

        public static VRCExpressionParameters ReplaceParameters(this VRCAvatarDescriptor avi, string folderPath)
        {
            avi.customExpressions = true;
            if (avi.expressionParameters)
            {
                avi.expressionParameters = DSCommonMethods.CopyAssetAndReturn<VRCExpressionParameters>(avi.expressionParameters, AssetDatabase.GenerateUniqueAssetPath(folderPath + "/" + avi.expressionParameters.name + ".asset"));
                return avi.expressionParameters;
            }
            else
            {
                VRCExpressionParameters newParameters = ScriptableObject.CreateInstance<VRCExpressionParameters>();
                AssetDatabase.CreateAsset(newParameters, AssetDatabase.GenerateUniqueAssetPath(folderPath + "/" + avi.gameObject.name + " Parameters.asset"));
                AssetDatabase.SaveAssets();
                avi.expressionParameters = newParameters;
                avi.customExpressions = true;
                return newParameters;
            }
        }

        public static VRCExpressionsMenu ReplaceMenu(this VRCAvatarDescriptor avi, string folderPath, bool deep = false, System.Action<VRCExpressionsMenu.Control> action = null)
        {
            avi.customExpressions = true;
            if (avi.expressionsMenu)
            {
                avi.expressionsMenu = ReplaceMenu(avi.expressionsMenu, folderPath, deep, action);
                return avi.expressionsMenu;
            }
            else
            {
                VRCExpressionsMenu newMenu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
                AssetDatabase.CreateAsset(newMenu, AssetDatabase.GenerateUniqueAssetPath(folderPath + "/" + avi.gameObject.name + " Menu.asset"));
                avi.expressionsMenu = newMenu;
                avi.customExpressions = true;
                return newMenu;
            }
        }
        public static VRCExpressionsMenu ReplaceMenu(VRCExpressionsMenu menu, string folderPath, bool deep = true, System.Action<VRCExpressionsMenu.Control> action = null, Dictionary<VRCExpressionsMenu, VRCExpressionsMenu> copyDict = null)
        {
            if (!menu)
                return null;
            if (copyDict == null)
                copyDict = new Dictionary<VRCExpressionsMenu, VRCExpressionsMenu>();
            VRCExpressionsMenu newMenu;
            if (copyDict.ContainsKey(menu))
                newMenu = copyDict[menu];
            else
            {
                newMenu = DSCommonMethods.CopyAssetAndReturn<VRCExpressionsMenu>(menu, AssetDatabase.GenerateUniqueAssetPath(folderPath + "/" + menu.name + ".asset"));
                copyDict.Add(menu, newMenu);
                if (action != null || deep)
                {
                    for (int i = 0; i < newMenu.controls.Count; i++)
                    {
                        if (deep)
                        {
                            if (newMenu.controls[i].type == VRCExpressionsMenu.Control.ControlType.SubMenu && newMenu.controls[i].subMenu != null)
                                newMenu.controls[i].subMenu = ReplaceMenu(newMenu.controls[i].subMenu, folderPath, true, action, copyDict);
                        }
                        action?.Invoke(newMenu.controls[i]);

                    }
                    EditorUtility.SetDirty(newMenu);
                }
            }
            return newMenu;
        }

        public static AnimatorController GetPlayableLayer(this VRCAvatarDescriptor avi, VRCAvatarDescriptor.AnimLayerType type)
        {
            for (int i = 0; i < avi.baseAnimationLayers.Length; i++)
                if (avi.baseAnimationLayers[i].type == type)
                    return avi.baseAnimationLayers[i].animatorController.GetController();

            for (int i = 0; i < avi.specialAnimationLayers.Length; i++)
                if (avi.specialAnimationLayers[i].type == type)
                    return avi.specialAnimationLayers[i].animatorController.GetController();

            return null;
        }
        public static bool SetPlayableLayer(this VRCAvatarDescriptor avi, VRCAvatarDescriptor.AnimLayerType type,RuntimeAnimatorController ani)
        {
            for (int i = 0; i < avi.baseAnimationLayers.Length; i++)
                if (avi.baseAnimationLayers[i].type == type)
                {
                    if (ani)
                        avi.customizeAnimationLayers = true;
                    avi.baseAnimationLayers[i].isDefault = !(ani ? true : false);
                    avi.baseAnimationLayers[i].animatorController = ani;
                    return true;
                }

            for (int i = 0; i < avi.specialAnimationLayers.Length; i++)
                if (avi.specialAnimationLayers[i].type == type)
                {
                    if (ani)
                        avi.customizeAnimationLayers = true;
                    avi.specialAnimationLayers[i].isDefault = !(ani ? true : false);
                    avi.specialAnimationLayers[i].animatorController = ani;
                    return true;
                }

            return false;
        }
    }

    public class VRCAvatarDescriptorInfo
    {
        public AnimatorController BaseController, AdditiveController, GestureController, ActionController, FXController;
        public AnimatorController SittingController, TPoseController, IKPoseController;
        public VRCExpressionParameters ExpressionParameters;
        public VRCExpressionsMenu ExpressionMenu;

        public VRCAvatarDescriptorInfo(VRCAvatarDescriptor avi)
        {
            foreach (VRCAvatarDescriptor.CustomAnimLayer layer in avi.baseAnimationLayers.Concat(avi.specialAnimationLayers))
            {
                switch ((int)layer.type)
                {
                    case 0:
                        BaseController = layer.animatorController.GetController();
                        break;
                    case 2:
                        AdditiveController = layer.animatorController.GetController();
                        break;
                    case 3:
                        GestureController = layer.animatorController.GetController();
                        break;
                    case 4:
                        ActionController = layer.animatorController.GetController();
                        break;
                    case 5:
                        FXController = layer.animatorController.GetController();
                        break;
                    case 6:
                        SittingController = layer.animatorController.GetController();
                        break;
                    case 7:
                        TPoseController = layer.animatorController.GetController();
                        break;
                    case 8:
                        IKPoseController = layer.animatorController.GetController();
                        break;
                }
            }
            ExpressionParameters = avi.expressionParameters;
            ExpressionMenu = avi.expressionsMenu;
        }
    }
}
#endif
