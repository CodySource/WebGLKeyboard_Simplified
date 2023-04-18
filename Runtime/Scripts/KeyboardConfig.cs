using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CodySource
{
    namespace WebGLKeyboard_Simplified
    {
        [System.Serializable]
        [CreateAssetMenu(fileName = "WebGLKeyboard_Config", menuName = "WebGLKeyboard/Config", order = 0)]
        public class KeyboardConfig : ScriptableObject
        {
            public KeyRowConfig[] rows;
        }
    }
}