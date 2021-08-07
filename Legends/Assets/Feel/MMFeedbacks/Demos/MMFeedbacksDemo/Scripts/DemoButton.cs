using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.Feedbacks
{
    [ExecuteAlways]
    public class DemoButton : MonoBehaviour
    {
        [Header("Behaviour")]
        public bool NotSupportedInWebGL = false;

        [Header("Bindings")]
        public Button TargetButton;
        public Text ButtonText;
        public Text WebGL;
        protected Color _disabledColor = new Color(255, 255, 255, 0.5f);

        protected virtual void OnEnable()
        {
            HandleWebGL();
        }

        protected virtual void HandleWebGL()
        {
            if (WebGL != null)
            {
#if UNITY_WEBGL
                TargetButton.interactable = !NotSupportedInWebGL;    
                    WebGL.gameObject.SetActive(NotSupportedInWebGL);   
                ButtonText.color = NotSupportedInWebGL ? _disabledColor : Color.white;
#else
                WebGL.gameObject.SetActive(false);
                TargetButton.interactable = true;
                ButtonText.color = Color.white;
#endif
            }
        }
    }
}
