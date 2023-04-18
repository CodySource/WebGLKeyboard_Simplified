using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace CodySource
{
    namespace WebGLKeyboard_Simplified
    {
        public class KeyboardKey : MonoBehaviour, ISelectHandler, IDeselectHandler
        {

            #region PROPERTIES

            public bool UseDeselect = true;

            #endregion

            #region PUBLIC METHODS

            public void OnSelect(BaseEventData eventData)
            {
                if (Keyboard.instance.target != null) WebGLKeyboardIO_TMP.target = Keyboard.instance.target;
            }

            public void OnDeselect(BaseEventData eventData) => WebGLKeyboardIO_TMP.target = (UseDeselect)? null : WebGLKeyboardIO_TMP.target;

            #endregion

        }
    }
}