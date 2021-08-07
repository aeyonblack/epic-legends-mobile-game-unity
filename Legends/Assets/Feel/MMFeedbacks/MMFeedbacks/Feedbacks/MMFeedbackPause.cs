using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// This feedback will cause a pause when met, preventing any other feedback lower in the sequence to run until it's complete.
    /// </summary>
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback will cause a pause when met, preventing any other feedback lower in the sequence to run until it's complete.")]
    [FeedbackPath("Pause/Pause")]
    public class MMFeedbackPause : MMFeedback
    {
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.PauseColor; } }
        #endif
        public override IEnumerator Pause { get { return PauseWait(); } }
        
        [Header("Pause")]
        /// the duration of the pause, in seconds
        [Tooltip("the duration of the pause, in seconds")]
        public float PauseDuration = 1f;
        /// if this is true, you'll need to call the Resume() method on the host MMFeedbacks for this pause to stop, and the rest of the sequence to play
        [Tooltip("if this is true, you'll need to call the Resume() method on the host MMFeedbacks for this pause to stop, and the rest of the sequence to play")]
        public bool ScriptDriven = false;
        
        /// the duration of this feedback is the duration of the pause
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(PauseDuration); } set { PauseDuration = value; } }

        /// <summary>
        /// An IEnumerator used to wait for the duration of the pause, on scaled or unscaled time
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator PauseWait()
        {
            if (Timing.TimescaleMode == TimescaleModes.Scaled)
            {
                return MMFeedbacksCoroutine.WaitFor(PauseDuration);
            }
            else
            {
                return MMFeedbacksCoroutine.WaitForUnscaled(PauseDuration);
            }
        }

        /// <summary>
        /// On init we cache our wait for seconds
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(GameObject owner)
        {
            base.CustomInitialization(owner);
            ScriptDrivenPause = ScriptDriven;
        }

        /// <summary>
        /// On play we trigger our pause
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active)
            {
                StartCoroutine(PlayPause());
            }
        }

        /// <summary>
        /// Pause coroutine
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator PlayPause()
        {
            yield return Pause;
        }
    }
}
