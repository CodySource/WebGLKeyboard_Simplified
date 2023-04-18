using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace CodySource
{
    namespace WebGLKeyboard_Simplified
    {
        public class Keyboard : MonoBehaviour
        {

            #region PROPERTIES

            public static Keyboard instance => _instance;
            private static Keyboard _instance = null;

            [Header("CUSTOMIZATIONS")]
            public Animator anim = null;
            public CanvasGroup group = null;
            public Color defaultButtonColor = new Color(0.703f, 0.703f, 0.703f, 1f);
            public Color normalTextColor = Color.white;
            public Color alternateTextColor = Color.blue;
            public float normalSize = 72f;
            public float alternateSize = 60f;

            [Space(10)]
            [Header("KEYBOARD CONFIGUARTIONS")]
            public KeyboardConfig alphaConfig;
            public KeyboardConfig shiftConfig;
            public KeyboardConfig capsConfig;
            public KeyboardConfig numberConfig;
            public KeyboardConfig symbolConfig;

            public TMP_InputField target => _target;
            private TMP_InputField _target = null;
            private Dictionary<GameObject, KeyConfig> _keys = new Dictionary<GameObject, KeyConfig>();

            #endregion

            #region PUBLIC METHODS

            /// <summary>
            /// Evaluates the provided key configuration
            /// </summary>
            public void EvaluateKey(KeyConfig pKey)
            {
                //  Breakout if no target or the key config is empty
                if (_target == null || pKey.name == "") return;
                switch (pKey.function)
                {
                    //  Apply the text and reset to alpha config if the key requires it (used when hitting the "Shift" option)
                    case KeyConfig.KeyFunction.Value:
                        _AddText(pKey.name);
                        if (pKey.resetToAlpha) _SetConfig(alphaConfig);
                        break;

                    case KeyConfig.KeyFunction.Operation:
                        switch(pKey.operation)
                        {
                            //  Set applicable configs
                            case KeyConfig.KeyOperation.Shift: _SetConfig(shiftConfig); break;
                            case KeyConfig.KeyOperation.Caps: _SetConfig(capsConfig); break;
                            case KeyConfig.KeyOperation.Numbers: _SetConfig(numberConfig); break;
                            case KeyConfig.KeyOperation.Symbols: _SetConfig(symbolConfig); break;
                            case KeyConfig.KeyOperation.Alpha:_SetConfig(alphaConfig); break;

                            //  Apply the space text
                            case KeyConfig.KeyOperation.Space: _AddText(" "); break;

                            //  Apply a line-break for multiline text and call submit for single-line
                            case KeyConfig.KeyOperation.Submit:
                                if (_target.multiLine) _AddText("\n");
                                else _target.OnSubmit(new UnityEngine.EventSystems.BaseEventData(EventSystem.current));
                                break;

                            //  Apply the backspace text
                            case KeyConfig.KeyOperation.Backspace: _AddText("\b"); break;

                            //  In case the user touches the keyboard background
                            case KeyConfig.KeyOperation.None:
                                _target.text = _target.text;
                                if (_target.selectionAnchorPosition == _target.selectionFocusPosition) _target.caretPosition = _target.caretPosition;
                                else
                                {
                                    _target.selectionAnchorPosition = _target.selectionAnchorPosition;
                                    _target.selectionFocusPosition = _target.selectionFocusPosition;
                                }
                                break;

                            default: break;
                        }
                        break;
                }

                //  Reset focus on the target input
                bool cache = _target.onFocusSelectAll;
                _target.onFocusSelectAll = false;
                _target.Select();
                _target.onFocusSelectAll = cache;

                /// Adds the text to the target input
                void _AddText(string pText)
                {
                    bool isBackspace = pText == "\b";
                    string text = (isBackspace) ? "" : pText;
                    int caretStart = Mathf.Min(_target.selectionAnchorPosition, _target.selectionFocusPosition);
                    int caretEnd = Mathf.Max(_target.selectionAnchorPosition, _target.selectionFocusPosition);
                    bool isSelection = caretStart == caretEnd;
                    string newText = _target.text.Substring(0, Mathf.Max(caretStart - ((isBackspace && isSelection) ? 1 : 0), 0)) + text + _target.text.Substring(caretEnd);
                    _target.text = newText;
                    //  The caret will increment by 1 unless the key was backspace.
                    //  If the key was a backspace but the caret was at the end of the text or it was a text selection, the caret will stay the same.
                    //  Otherwise, the caret will decrement by 1 if the key was backspace.
                    _target.caretPosition = (isBackspace) ? Mathf.Max(((caretStart > newText.Length || !isSelection) ? caretStart : caretStart - 1), 0) : caretStart + 1;
                }
            }

            /// <summary>
            /// Used to prevent the keyboard from closing when touching the background of the keyboard
            /// </summary>
            public void BackgroundTouch() => EvaluateKey(new KeyConfig() { name = "Background", function = KeyConfig.KeyFunction.Operation, operation = KeyConfig.KeyOperation.None });

            #endregion

            #region PRIVATE METHODS

            /// <summary>
            /// Singleton
            /// </summary>
            private void Awake() => _instance = _instance ?? this;

            /// <summary>
            /// Monitor keyboard output target and animate in/out as appropriate
            /// </summary>
            private void FixedUpdate()
            {
                if (WebGLKeyboardIO_TMP.target != null)
                {
                    if (_target == WebGLKeyboardIO_TMP.target) return;
                    _target = WebGLKeyboardIO_TMP.target;
                    _SetConfig(alphaConfig, false);
                    //  Set Open Animation Trigger
                    anim.ResetTrigger("Close");
                    anim.SetTrigger("Open");
                    return;
                }
                if (_target == null) return;
                _target = null;
                //  Set Clsoe Animation Trigger
                anim.ResetTrigger("Open");
                anim.SetTrigger("Close");
            }

            /// <summary>
            /// Set the provided keyboard config
            /// </summary>
            private void _SetConfig(KeyboardConfig pConfig, bool pIsOpen = true)
            {
                try
                {
                    //  Clear the existing dictionary & get the keyboard body
                    _keys.Clear();
                    RectTransform keyboardTransform = transform.GetChild(0).GetComponent<RectTransform>();
                    for (int r = 0; r < keyboardTransform.childCount; r++)
                    {
                        //  For each existing keyboard row, get the row body and get the appropriate config row if it exists
                        KeyRowConfig row = (r < pConfig.rows.Length) ? pConfig.rows[r] : new KeyRowConfig();
                        RectTransform rowTransform = keyboardTransform.transform.GetChild(r).GetComponent<RectTransform>();
                        for (int k = 0; k < rowTransform.childCount; k++)
                        {
                            //  For each existing keyboard key, get the key body and the appropriate config key if it exists
                            KeyConfig key = (k < row.keys.Length) ? row.keys[k] : new KeyConfig() { name = "" };
                            RectTransform keyTransform = rowTransform.transform.GetChild(k).GetComponent<RectTransform>();

                            //  Set the key's properties based on the key config
                            keyTransform.gameObject.SetActive(key.name != "");
                            keyTransform.sizeDelta = new Vector2(175f * key.widthMultiplier, keyTransform.sizeDelta.y);
                            TMP_Text text = keyTransform.GetComponentInChildren<TMP_Text>();
                            text.color = (!key.isAlternateColor) ? normalTextColor : alternateTextColor;
                            text.fontSize = (!key.isAlternateSize) ? normalSize : alternateSize;
                            text.text = key.name;
                            text.margin = key.customMargin;
                            keyTransform.GetComponent<Image>().color = (key.alternateButtonColor != Color.clear) ? key.alternateButtonColor : defaultButtonColor;
                            Button btn = keyTransform.GetComponent<Button>();
                            btn.onClick.RemoveAllListeners();
                            btn.onClick.AddListener(() => EvaluateKey(_keys[keyTransform.gameObject]));
                            if (keyTransform != null) _keys.Add(keyTransform.gameObject, key);
                        }
                    }
                    //  This is to prevent the refocus from missing the key press on function keys
                    if (pIsOpen)
                    {
                        _target.text = _target.text;
                        if (_target.selectionAnchorPosition == _target.selectionFocusPosition) _target.caretPosition = _target.caretPosition;
                        else
                        {
                            _target.selectionAnchorPosition = _target.selectionAnchorPosition;
                            _target.selectionFocusPosition = _target.selectionFocusPosition;
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Unable to load WebGLKeyboard Config \"{pConfig.name}!\"\n{e.Message}");
                }
            }

            #endregion

        }

        [System.Serializable]
        public struct KeyRowConfig
        {
            public KeyConfig[] keys;
        }

        [System.Serializable]
        public struct KeyConfig
        {
            public string name;
            public KeyFunction function;
            public KeyOperation operation;
            public float widthMultiplier;
            public Color alternateButtonColor;
            public bool isAlternateColor;
            public bool isAlternateSize;
            public bool resetToAlpha;
            public Vector4 customMargin;
            public enum KeyFunction { Value, Operation };
            public enum KeyOperation { None, Shift, Caps, Numbers, Symbols, Alpha, Submit, Space, Backspace };
        }

    }
}