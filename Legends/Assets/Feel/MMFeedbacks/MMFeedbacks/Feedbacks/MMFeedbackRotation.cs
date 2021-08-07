using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// This feedback animates the rotation of the specified object when played
    /// </summary>
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback will animate the target's rotation on the 3 specified animation curves (one per axis), for the specified duration (in seconds).")]
    [FeedbackPath("Transform/Rotation")]
    public class MMFeedbackRotation : MMFeedback
    {
        /// the possible modes for this feedback (Absolute : always follow the curve from start to finish, Additive : add to the values found when this feedback gets played)
        public enum Modes { Absolute, Additive, ToDestination }
        /// the timescale modes this feedback can operate on
        public enum TimeScales { Scaled, Unscaled }

        /// sets the inspector color for this feedback
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TransformColor; } }
        #endif

        [Header("Rotation Target")]
        /// the object whose rotation you want to animate
        [Tooltip("the object whose rotation you want to animate")]
        public Transform AnimateRotationTarget;

        [Header("Animation")]
        /// whether this feedback should animate in absolute values or additive
        [Tooltip("whether this feedback should animate in absolute values or additive")]
        public Modes Mode = Modes.Absolute;
        /// whether this feedback should play in scaled or unscaled time
        [Tooltip("whether this feedback should play in scaled or unscaled time")]
        public TimeScales TimeScale = TimeScales.Scaled;
        /// whether this feedback should play on local or world rotation
        [Tooltip("whether this feedback should play on local or world rotation")]
        public Space RotationSpace = Space.World;
        /// the duration of the transition
        [Tooltip("the duration of the transition")]
        public float AnimateRotationDuration = 0.2f;
        /// the value to remap the curve's 0 value to
        [Tooltip("the value to remap the curve's 0 value to")]
        public float RemapCurveZero = 0f;
        /// the value to remap the curve's 1 value to
        [Tooltip("the value to remap the curve's 1 value to")]
        [FormerlySerializedAs("Multiplier")]
        public float RemapCurveOne = 360f;
        /// if this is true, should animate the X rotation
        [Tooltip("if this is true, should animate the X rotation")]
        public bool AnimateX = true;
        /// how the x part of the rotation should animate over time, in degrees
        [Tooltip("how the x part of the rotation should animate over time, in degrees")]
        [MMFCondition("AnimateX", true)]
        public AnimationCurve AnimateRotationX = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
        /// if this is true, should animate the X rotation
        [Tooltip("if this is true, should animate the X rotation")]
        public bool AnimateY = true;
        /// how the y part of the rotation should animate over time, in degrees
        [Tooltip("how the y part of the rotation should animate over time, in degrees")]
        [MMFCondition("AnimateY", true)]
        public AnimationCurve AnimateRotationY = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
        /// if this is true, should animate the X rotation
        [Tooltip("if this is true, should animate the X rotation")]
        public bool AnimateZ = true;
        /// how the z part of the rotation should animate over time, in degrees
        [Tooltip("how the z part of the rotation should animate over time, in degrees")]
        [MMFCondition("AnimateZ", true)]
        public AnimationCurve AnimateRotationZ = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
        /// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
        [Tooltip("if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over")] 
        public bool AllowAdditivePlays = false;
        
        [Header("To Destination")]
        /// the space in which the ToDestination mode should operate 
        [Tooltip("the space in which the ToDestination mode should operate")]
        [MMFEnumCondition("Mode", (int)Modes.ToDestination)]
        public Space ToDestinationSpace = Space.World;
        /// the angles to match when in ToDestination mode
        [Tooltip("the angles to match when in ToDestination mode")]
        [MMFEnumCondition("Mode", (int)Modes.ToDestination)]
        public Vector3 DestinationAngles = new Vector3(0f, 180f, 0f);

        /// the duration of this feedback is the duration of the rotation
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(AnimateRotationDuration); } set { AnimateRotationDuration = value; } }

        protected Quaternion _initialRotation;
        protected Quaternion _destinationRotation;
        protected Coroutine _coroutine;

        /// <summary>
        /// On init we store our initial rotation
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(GameObject owner)
        {
            base.CustomInitialization(owner);
            if (Active && (AnimateRotationTarget != null))
            {
                _initialRotation = (RotationSpace == Space.World) ? AnimateRotationTarget.rotation : AnimateRotationTarget.localRotation;
            }
        }

        /// <summary>
        /// On play, we trigger our rotation animation
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active && (AnimateRotationTarget != null))
            {
                float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
                if (isActiveAndEnabled || _hostMMFeedbacks.AutoPlayOnEnable)
                {
                    if ((Mode == Modes.Absolute) || (Mode == Modes.Additive))
                    {
                        if (!AllowAdditivePlays && (_coroutine != null))
                        {
                            return;
                        }
                        _coroutine = StartCoroutine(AnimateRotation(AnimateRotationTarget, Vector3.zero, FeedbackDuration, AnimateRotationX, AnimateRotationY, AnimateRotationZ, RemapCurveZero * intensityMultiplier, RemapCurveOne * intensityMultiplier));
                    }
                    if (Mode == Modes.ToDestination)
                    {
                        if (!AllowAdditivePlays && (_coroutine != null))
                        {
                            return;
                        }
                        StartCoroutine(RotateToDestination());
                    }
                }
            }
        }

        /// <summary>
        /// A coroutine used to rotate the target to its destination rotation
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator RotateToDestination()
        {
            if (AnimateRotationTarget == null)
            {
                yield break;
            }

            if ((AnimateRotationX == null) || (AnimateRotationY == null) || (AnimateRotationZ == null))
            {
                yield break;
            }

            if (FeedbackDuration == 0f)
            {
                yield break;
            }

            float journey = NormalPlayDirection ? 0f : FeedbackDuration;

            _initialRotation = AnimateRotationTarget.transform.rotation;
            if (ToDestinationSpace == Space.Self)
            {
                AnimateRotationTarget.transform.localRotation = Quaternion.Euler(DestinationAngles);
            }
            else
            {
                AnimateRotationTarget.transform.rotation = Quaternion.Euler(DestinationAngles);
            }
            
            _destinationRotation = AnimateRotationTarget.transform.rotation;
            AnimateRotationTarget.transform.rotation = _initialRotation;

            while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
            {
                float percent = Mathf.Clamp01(journey / FeedbackDuration);

                Quaternion newRotation = Quaternion.LerpUnclamped(_initialRotation, _destinationRotation, percent);
                AnimateRotationTarget.transform.rotation = newRotation;

                if (TimeScale == TimeScales.Scaled)
                {
                    journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
                }
                else
                {
                    journey += NormalPlayDirection ? Time.unscaledDeltaTime : -Time.unscaledDeltaTime;
                }
                yield return null;
            }

            if (ToDestinationSpace == Space.Self)
            {
                AnimateRotationTarget.transform.localRotation = Quaternion.Euler(DestinationAngles);
            }
            else
            {
                AnimateRotationTarget.transform.rotation = Quaternion.Euler(DestinationAngles);
            }

            _coroutine = null;
            yield break;
        }

        /// <summary>
        /// A coroutine used to compute the rotation over time
        /// </summary>
        /// <param name="targetTransform"></param>
        /// <param name="vector"></param>
        /// <param name="duration"></param>
        /// <param name="curveX"></param>
        /// <param name="curveY"></param>
        /// <param name="curveZ"></param>
        /// <param name="multiplier"></param>
        /// <returns></returns>
        protected virtual IEnumerator AnimateRotation(Transform targetTransform,
                                                    Vector3 vector,
                                                    float duration,
                                                    AnimationCurve curveX,
                                                    AnimationCurve curveY,
                                                    AnimationCurve curveZ,
                                                    float remapZero,
                                                    float remapOne)
        {
            if (targetTransform == null)
            {
                yield break;
            }

            if ((curveX == null) || (curveY == null) || (curveZ == null))
            {
                yield break;
            }

            if (duration == 0f)
            {
                yield break;
            }

            float journey = NormalPlayDirection ? 0f : duration;

            if (Mode == Modes.Additive)
            {
                _initialRotation = (RotationSpace == Space.World) ? targetTransform.rotation : targetTransform.localRotation;
            }

            while ((journey >= 0) && (journey <= duration) && (duration > 0))
            {
                float percent = Mathf.Clamp01(journey / duration);
                
                ApplyRotation(targetTransform, remapZero, remapOne, curveX, curveY, curveZ, percent);

                if (TimeScale == TimeScales.Scaled)
                {
                    journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
                }
                else
                {
                    journey += NormalPlayDirection ? Time.unscaledDeltaTime : -Time.unscaledDeltaTime;
                }
                yield return null;
            }

            ApplyRotation(targetTransform, remapZero, remapOne, curveX, curveY, curveZ, FinalNormalizedTime);
            _coroutine = null;
            yield break;
        }

        /// <summary>
        /// Computes and applies the rotation to the object
        /// </summary>
        /// <param name="targetTransform"></param>
        /// <param name="multiplier"></param>
        /// <param name="curveX"></param>
        /// <param name="curveY"></param>
        /// <param name="curveZ"></param>
        /// <param name="percent"></param> 
        protected virtual void ApplyRotation(Transform targetTransform, float remapZero, float remapOne, AnimationCurve curveX, AnimationCurve curveY, AnimationCurve curveZ, float percent)
        {
            if (RotationSpace == Space.World)
            {
                targetTransform.transform.rotation = _initialRotation;    
            }
            else
            {
                targetTransform.transform.localRotation = _initialRotation;
            }

            if (AnimateX)
            {
                float x = curveX.Evaluate(percent);
                x = MMFeedbacksHelpers.Remap(x, 0f, 1f, remapZero, remapOne);
                targetTransform.Rotate(Vector3.right, x, RotationSpace);
            }
            if (AnimateY)
            {
                float y = curveY.Evaluate(percent);
                y = MMFeedbacksHelpers.Remap(y, 0f, 1f, remapZero, remapOne);
                targetTransform.Rotate(Vector3.up, y, RotationSpace);
            }
            if (AnimateZ)
            {
                float z = curveZ.Evaluate(percent);
                z = MMFeedbacksHelpers.Remap(z, 0f, 1f, remapZero, remapOne);
                targetTransform.Rotate(Vector3.forward, z, RotationSpace);
            }
        }

        /// <summary>
        /// On disable we reset our coroutine
        /// </summary>
        protected void OnDisable()
        {
            _coroutine = null;
        }
    }
}
