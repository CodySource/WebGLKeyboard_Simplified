using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace CodySource
{
    namespace WebGLKeyboard_Simplified
    {
        [RequireComponent(typeof(TMP_InputField))]
        public class WebGLKeyboardIO_TMP : MonoBehaviour
        {

            #region PROPERTIES

            public static TMP_InputField target = null;
            private TMP_InputField input = null;

            #endregion

            #region PRIVATE METHODS

            private void Start()
            {
                input = GetComponent<TMP_InputField>();
                input.onSelect.RemoveListener(_OnSelect);
                input.onSelect.AddListener(_OnSelect);
                input.onDeselect.RemoveListener(_OnDeselect);
                input.onDeselect.AddListener(_OnDeselect);
            }

            /// <summary>
            /// Select the input and open the keyboard
            /// </summary>
            private void _OnSelect(string pText) => target = input;

            /// <summary>
            /// Close the input and keyboard
            /// </summary>
            private void _OnDeselect(string pText) => target = null;

            #endregion

        }
    }
}