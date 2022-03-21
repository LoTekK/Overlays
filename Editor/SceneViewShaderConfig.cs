using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeckArtist.Overlays.Editor
{
    [CreateAssetMenu]
    public class SceneViewShaderConfig : ScriptableObject
    {
        [System.Serializable]
        public struct TextureProp
        {
            public string Name;
            public Texture Texture;
        }

        public Shader Shader;
        public TextureProp[] DefaultTextures;
    }
}