using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// This feedback will make the bound renderer flicker for the set duration when played (and restore its initial color when stopped)
    /// </summary>
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback lets you flicker the color of a specified renderer (sprite, mesh, etc) for a certain duration, at the specified octave, and with the specified color. Useful when a character gets hit, for example (but so much more!).")]
    [FeedbackPath("Renderer/Flicker")]
    public class MMFeedbackFlicker : MMFeedback
    {
        /// sets the inspector color for this feedback
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.RendererColor; } }
        #endif

        /// the possible modes
        /// Color : will control material.color
        /// PropertyName : will target a specific shader property by name
        public enum Modes { Color, PropertyName }

        [Header("Flicker")]
        /// the renderer to flicker when played
        [Tooltip("the renderer to flicker when played")]
        public Renderer BoundRenderer;
        /// the selected mode to flicker the renderer 
        [Tooltip("the selected mode to flicker the renderer")]
        public Modes Mode = Modes.Color;
        /// the name of the property to target
        [MMFEnumCondition("Mode", (int)Modes.PropertyName)]
        [Tooltip("the name of the property to target")]
        public string PropertyName = "_Tint";
        /// the duration of the flicker when getting damage
        [Tooltip("the duration of the flicker when getting damage")]
        public float FlickerDuration = 0.2f;
        /// the frequency at which to flicker
        [Tooltip("the frequency at which to flicker")]
        public float FlickerOctave = 0.04f;
        /// the color we should flicker the sprite to 
        [Tooltip("the color we should flicker the sprite to")]
        public Color FlickerColor = new Color32(255, 20, 20, 255);

        /// the duration of this feedback is the duration of the flicker
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(FlickerDuration); } set { FlickerDuration = value; } }

        protected Color _initialFlickerColor;
        protected int _propertyID = -1;
        protected bool _propertyFound = false;

        /// <summary>
        /// On init we grab our initial color and components
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(GameObject owner)
        {
            _propertyFound = false;

            if (Active && (BoundRenderer == null) && (owner != null))
            {
                if (owner.MMFGetComponentNoAlloc<Renderer>() != null)
                {
                    BoundRenderer = owner.GetComponent<Renderer>();
                }
                if (BoundRenderer == null)
                {
                    BoundRenderer = owner.GetComponentInChildren<Renderer>();
                }
                if (BoundRenderer != null)
                {
                    if (BoundRenderer.material.HasProperty("_Color"))
                    {
                        _initialFlickerColor = BoundRenderer.material.color;
                    }
                }
            }

            if (Active && (BoundRenderer != null))
            {
                if (Mode == Modes.Color)
                {
                    if (BoundRenderer.material.HasProperty("_Color"))
                    {
                        _propertyFound = true;
                        _initialFlickerColor = BoundRenderer.material.color;
                    }
                }
                else
                {
                    if (BoundRenderer.material.HasProperty(PropertyName))
                    {
                        _propertyID = Shader.PropertyToID(PropertyName);
                        _propertyFound = true;
                        _initialFlickerColor = BoundRenderer.material.GetColor(_propertyID);
                    }
                }
            }
        }

        /// <summary>
        /// On play we make our renderer flicker
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active && (BoundRenderer != null))
            {
                StartCoroutine(Flicker(BoundRenderer, _initialFlickerColor, FlickerColor, FlickerOctave, FeedbackDuration));
            }
        }

        /// <summary>
        /// On reset we make our renderer stop flickering
        /// </summary>
        protected override void CustomReset()
        {
            base.CustomReset();

            if (InCooldown)
            {
                return;
            }

            if (Active && (BoundRenderer != null))
            {
                SetColor(_initialFlickerColor);                
            }
        }

        public virtual IEnumerator Flicker(Renderer renderer, Color initialColor, Color flickerColor, float flickerSpeed, float flickerDuration)
        {
            if (renderer == null)
            {
                yield break;
            }

            if (!_propertyFound)
            {
                yield break;
            }

            if (initialColor == flickerColor)
            {
                yield break;
            }

            float flickerStop = FeedbackTime + flickerDuration;

            while (FeedbackTime < flickerStop)
            {
                SetColor(flickerColor);
                if (Timing.TimescaleMode == TimescaleModes.Scaled)
                {
                    yield return MMFeedbacksCoroutine.WaitFor(flickerSpeed);
                }
                else
                {
                    yield return MMFeedbacksCoroutine.WaitForUnscaled(flickerSpeed);
                }
                SetColor(initialColor);
                if (Timing.TimescaleMode == TimescaleModes.Scaled)
                {
                    yield return MMFeedbacksCoroutine.WaitFor(flickerSpeed);
                }
                else
                {
                    yield return MMFeedbacksCoroutine.WaitForUnscaled(flickerSpeed);
                }
            }

            SetColor(initialColor);
        }

        protected virtual void SetColor(Color color)
        {
            if (Mode == Modes.Color)
            {
                if (BoundRenderer.material.HasProperty("_Color"))
                {
                    BoundRenderer.material.color = color;
                }
            }
            else
            {
                if (_propertyFound)
                {
                    BoundRenderer.material.SetColor(_propertyID, color);
                }
            }            
        }
    }
}
