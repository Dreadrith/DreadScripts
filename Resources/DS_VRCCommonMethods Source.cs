using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDKBase;
using UnityEditor.Animations;
using System.Linq;
using DS_CommonMethods;
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

        public static bool[] GetUsedValues(this VRCAvatarDescriptor avi, string parameter)
        {
            bool[] usedValues = new bool[255];

            foreach (VRCAvatarDescriptor.CustomAnimLayer playlayer in avi.baseAnimationLayers.Concat(avi.specialAnimationLayers))
            {
                AnimatorController myController = playlayer.animatorController.GetController();
                if (myController)
                    DSCommonMethods.GetUsedValues(myController, parameter, ref usedValues);
            }

            return usedValues;
        }

        public static AnimatorController[] ReplaceControllers(this VRCAvatarDescriptor avi, string copyPath, System.Action<Motion> motionAction = null, bool replaceAnimations = true, bool nested = true)
        {
            DSCommonMethods.RecreateFolders(copyPath);
            List<AnimatorController> controllers = new List<AnimatorController>();
            Dictionary<RuntimeAnimatorController, AnimatorController> conDict = new Dictionary<RuntimeAnimatorController, AnimatorController>();
            Dictionary<Motion, Motion> motionDict = new Dictionary<Motion, Motion>();

            AnimatorController conFunc(RuntimeAnimatorController runtime)
            {
                if (conDict.ContainsKey(runtime))
                    return conDict[runtime];

                AnimatorController newCon = runtime.ReplaceController(copyPath, replaceAnimations, motionDict, motionAction);

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
