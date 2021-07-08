using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DreadScripts
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "New Auto-Packer Module", menuName = "DreadScripts/Auto-Packer Module")]
    public class TextureAutoPackerModule : ScriptableObject
    {
        public List<AutoPackedTexture> packedTextures;

        public TextureAutoPackerModule()
        {
            packedTextures = new List<AutoPackedTexture>();
        }
    }

    [System.Serializable]
    public class AutoPackedTexture
    {
        public bool expanded;
        public bool forward=true;
        public string name;
        public ChannelTexture[] channels;
        public string[] channelsHashes;
        public bool forceModified;

        public Texture2D packed;

        public TextureUtility.TexEncoding encoding;
        public int jpgQuality = 75;
        public AutoPackedTexture()
        {
            name = "New Packed Texture";
            channels = new ChannelTexture[] {new ChannelTexture("Red",0), new ChannelTexture("Green", 1), new ChannelTexture("Blue", 2), new ChannelTexture("Alpha", 0) };
            channelsHashes = new string[] { string.Empty, string.Empty, string.Empty, string.Empty, string.Empty };
            encoding = TextureUtility.TexEncoding.SaveAsPNG;
        }

        public bool WasModified()
        {
            if (forceModified)
            {
                return true;
            }
            for (int i = 0; i < 4; i++)
            {
                string textureHash = string.Empty;
                if (channels[i].texture)
                    textureHash = channels[i].texture.imageContentsHash.ToString();
                if (textureHash != channelsHashes[i])
                    return true;
            }
            return false;
        }

        public string Pack()
        {
            string newTexturePath;
            if (packed)
                newTexturePath = TextureUtility.PackTexture(channels, AssetDatabase.GetAssetPath(packed), packed.width, packed.height, encoding, false, true, false);
            else
                newTexturePath = TextureUtility.PackTexture(channels, encoding, true, false);
            return newTexturePath;
        }
    }
}
