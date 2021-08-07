using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// This feedback will play the associated particles system on play, and stop it on stop
    /// </summary>
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback will simply play the specified ParticleSystem (from your scene) when played.")]
    [FeedbackPath("Particles/Particles Play")]
    public class MMFeedbackParticles : MMFeedback
    {
        /// sets the inspector color for this feedback
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.ParticlesColor; } }
        #endif

        [Header("Bound Particles")]
        /// the particle system to play with this feedback
        [Tooltip("the particle system to play with this feedback")]
        public ParticleSystem BoundParticleSystem;
        /// if this is true, the particles will be moved to the position passed in parameters
        [Tooltip("if this is true, the particles will be moved to the position passed in parameters")]
        public bool MoveToPosition = false;
        /// a list of (optional) particle systems 
        [Tooltip("a list of (optional) particle systems")]
        public List<ParticleSystem> RandomParticleSystems;

        /// <summary>
        /// On init we stop our particle system
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(GameObject owner)
        {
            base.CustomInitialization(owner);
            StopParticles();
        }

        /// <summary>
        /// On play we play our particle system
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (!Active)
            {
                return;
            }
            PlayParticles(position);
        }
        
        /// <summary>
        /// On Stop, stops the particle system
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (!Active)
            {
                return;
            }
            StopParticles();
        }

        /// <summary>
        /// On Reset, stops the particle system 
        /// </summary>
        protected override void CustomReset()
        {
            base.CustomReset();

            if (InCooldown)
            {
                return;
            }

            StopParticles();
        }

        /// <summary>
        /// Plays a particle system
        /// </summary>
        /// <param name="position"></param>
        protected virtual void PlayParticles(Vector3 position)
        {
            if (MoveToPosition)
            {
                BoundParticleSystem.transform.position = position;
                foreach (ParticleSystem system in RandomParticleSystems)
                {
                    system.transform.position = position;
                }
            }

            if (RandomParticleSystems.Count > 0)
            {
                int random = Random.Range(0, RandomParticleSystems.Count);
                RandomParticleSystems[random].Play();
                return;
            }
            else if (BoundParticleSystem != null)
            {
                BoundParticleSystem?.Play();
            }
        }

        /// <summary>
        /// Stops all particle systems
        /// </summary>
        protected virtual void StopParticles()
        {
            foreach(ParticleSystem system in RandomParticleSystems)
            {
                system?.Stop();
            }
            if (BoundParticleSystem != null)
            {
                BoundParticleSystem.Stop();
            }            
        }
    }
}
