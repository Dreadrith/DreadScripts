using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DreadScripts
{
    [System.Serializable]
    public class TextureAutoPackerData : TAPDreadData<TextureAutoPackerData>
    {
        public bool active;
        public List<TextureAutoPackerModule> activeModules = new List<TextureAutoPackerModule>();
        private static readonly string SavePath = "Assets/DreadScripts/SavedData/TextureUtility/TextureAutoPackerData.asset";
        public static TextureAutoPackerData GetInstance()
        {
            return GetInstance(SavePath);
        }
    }
}
