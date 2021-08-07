using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// When played, this feedback will activate the Wiggle method of a MMWiggle object based on the selected settings, wiggling either its position, rotation, scale, or all of these.
    /// </summary>
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback lets you trigger position, rotation and/or scale wiggles on an object equipped with a MMWiggle component, for the specified durations.")]
    [FeedbackPath("Transform/Wiggle")]
    public class MMFeedbackWiggle : MMFeedback
    {
        /// sets the inspector color for this feedback
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TransformColor; } }
        #endif

        [Header("Target")]
        /// the Wiggle component to target
        [Tooltip("the Wiggle component to target")]
        public MMWiggle TargetWiggle;
        
        [Header("Position")]
        /// whether or not to wiggle position
        [Tooltip("whether or not to wiggle position")]
        public bool WigglePosition = true;
        /// the duration (in seconds) of the position wiggle
        [Tooltip("the duration (in seconds) of the position wiggle")]
        public float WigglePositionDuration;

        [Header("Rotation")]
        /// whether or not to wiggle rotation
        [Tooltip("whether or not to wiggle rotation")]
        public bool WiggleRotation;
        /// the duration (in seconds) of the rotation wiggle
        [Tooltip("the duration (in seconds) of the rotation wiggle")]
        public float WiggleRotationDuration;

        [Header("Scale")]
        /// whether or not to wiggle scale
        [Tooltip("whether or not to wiggle scale")]
        public bool WiggleScale;
        /// the duration (in seconds) of the scale wiggle
        [Tooltip("the duration (in seconds) of the scale wiggle")]
        public float WiggleScaleDuration;


        /// the duration of this feedback is the duration of the clip being played
        public override float FeedbackDuration
        {
            get { return Mathf.Max(ApplyTimeMultiplier(WigglePositionDuration), ApplyTimeMultiplier(WiggleRotationDuration), ApplyTimeMultiplier(WiggleScaleDuration)); }
            set { WigglePositionDuration = value;
                WiggleRotationDuration = value;
                WiggleScaleDuration = value;
            } 
        }

        /// <summary>
        /// On Play we trigger the desired wiggles
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active && (TargetWiggle != null))
            {
                if (WigglePosition)
                {
                    TargetWiggle.PositionWiggleProperties.UseUnscaledTime = Timing.TimescaleMode == TimescaleModes.Unscaled;
                    TargetWiggle.WigglePosition(ApplyTimeMultiplier(WigglePositionDuration));
                }
                if (WiggleRotation)
                {
                    TargetWiggle.RotationWiggleProperties.UseUnscaledTime = Timing.TimescaleMode == TimescaleModes.Unscaled;
                    TargetWiggle.WiggleRotation(ApplyTimeMultiplier(WiggleRotationDuration));
                }
                if (WiggleScale)
                {
                    TargetWiggle.ScaleWiggleProperties.UseUnscaledTime = Timing.TimescaleMode == TimescaleModes.Unscaled;
                    TargetWiggle.WiggleScale(ApplyTimeMultiplier(WiggleScaleDuration));
                }
            }
        }
    }
}
