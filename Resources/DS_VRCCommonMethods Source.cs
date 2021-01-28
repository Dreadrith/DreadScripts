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

        public static bool[] GetUsedValues(this VRCAvatarDescriptor avi, string parameter, bool nested = true)
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
                foreach (Animator anim in avi.gameObject.GetComponentsInChildren<Animator>(true))
                {
                    AnimatorController subController = anim.runtimeAnimatorController?.GetController();
                    if (subController)
                        DSCommonMethods.GetUsedValues(subController, parameter, ref usedValues);
                }
            }

            return usedValues;
        }

        public static AnimatorController[] ReplaceControllers(VRCAvatarDescriptor avi, string folderPath, System.Action<Motion> motionAction = null, Dictionary<Motion, Motion> motionDict = null, bool nested = true)
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

        public static bool AddControls(VRCExpressionsMenu target, List<VRCExpressionsMenu.Control> newCons, string suffix = "")
        {
            if (8 - newCons.Count < 0)
                return false;
            for (int i = 0; i < newCons.Count; i++)
            {
                newCons[i].name += suffix;
                target.controls.Add(newCons[i]);
            }

            EditorUtility.SetDirty(target);
            return true;
        }

        public static List<VRCExpressionParameters.Parameter> GetParameters(VRCExpressionParameters paramObject)
        {
            List<VRCExpressionParameters.Parameter> myParams = new List<VRCExpressionParameters.Parameter>();
            for (int i = 0; i < paramObject.parameters.Length; i++)
                if (!string.IsNullOrEmpty(paramObject.parameters[i].name))
                    myParams.Add(paramObject.parameters[i]);
            return myParams;
        }

        public static bool CheckParameters(this VRCExpressionParameters target, List<VRCExpressionParameters.Parameter> newParams)
        {
            int cost = 0;
            for (int i = 0; i < newParams.Count; i++)
            {
                if (newParams[i].valueType == VRCExpressionParameters.ValueType.Bool)
                    cost++;
                else
                    cost += 8;
            }
            if (target.CalcTotalCost() + cost > 128)
                return false;
            else
                return true;
        }

        public static void AddParameters(this VRCExpressionParameters target, List<VRCExpressionParameters.Parameter> newParams, string suffix = "")
        {
            if (!target.CheckParameters(newParams))
            {
                Debug.LogError("Expression Parameters " + target.name + " Does not have enough free Memory to hold the new parameters!");
                return;
            }

            if (!string.IsNullOrEmpty(suffix))
            {
                for (int i = 0; i < newParams.Count; i++)
                {
                    newParams[i].name += suffix;
                }
            }

            target.parameters = target.parameters.Concat(newParams).ToArray();

            EditorUtility.SetDirty(target);
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
        public static bool SetPlayableLayer(this VRCAvatarDescriptor avi, VRCAvatarDescriptor.AnimLayerType type, RuntimeAnimatorController ani)
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
