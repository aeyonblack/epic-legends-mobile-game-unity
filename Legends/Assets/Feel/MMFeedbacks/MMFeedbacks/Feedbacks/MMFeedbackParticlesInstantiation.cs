using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// This feedback will instantiate a particle system and play/stop it when playing/stopping the feedback
    /// </summary>
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback will instantiate the specified ParticleSystem at the specified position on Start or on Play, optionally nesting them.")]
    [FeedbackPath("Particles/Particles Instantiation")]
    public class MMFeedbackParticlesInstantiation : MMFeedback
    {
        /// sets the inspector color for this feedback
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.ParticlesColor; } }
        #endif
        /// the different ways to position the instantiated object :
        /// - FeedbackPosition : object will be instantiated at the position of the feedback, plus an optional offset
        /// - Transform : the object will be instantiated at the specified Transform's position, plus an optional offset
        /// - WorldPosition : the object will be instantiated at the specified world position vector, plus an optional offset
        /// - Script : the position passed in parameters when calling the feedback
        public enum PositionModes { FeedbackPosition, Transform, WorldPosition, Script }
        /// the possible delivery modes
        /// - cached : will cache a copy of the particle system and reuse it
        /// - on demand : will instantiate a new particle system for every play
        public enum Modes { Cached, OnDemand }

        [Header("Particles Instantiation")]
        /// whether the particle system should be cached or created on demand the first time
        [Tooltip("whether the particle system should be cached or created on demand the first time")]
        public Modes Mode = Modes.Cached;
        /// if this is false, a brand new particle system will be created every time
        [Tooltip("if this is false, a brand new particle system will be created every time")]
        [MMFEnumCondition("Mode", (int)Modes.OnDemand)]
        public bool CachedRecycle = true;
        /// the particle system to spawn
        [Tooltip("the particle system to spawn")]
        public ParticleSystem ParticlesPrefab;
        /// the possible random particle systems
        [Tooltip("the possible random particle systems")]
        public List<ParticleSystem> RandomParticlePrefabs;

        [Header("Position")]
        /// the selected position mode
        [Tooltip("the selected position mode")]
        public PositionModes PositionMode = PositionModes.FeedbackPosition;
        /// the position at which to spawn this particle system
        [Tooltip("the position at which to spawn this particle system")]
        [MMFEnumCondition("PositionMode", (int)PositionModes.Transform)]
        public Transform InstantiateParticlesPosition;
        /// the world position to move to when in WorldPosition mode 
        [Tooltip("the world position to move to when in WorldPosition mode")]
        [MMFEnumCondition("PositionMode", (int)PositionModes.WorldPosition)]
        public Vector3 TargetWorldPosition;
        /// an offset to apply to the instantiation position
        [Tooltip("an offset to apply to the instantiation position")]
        public Vector3 Offset;
        /// whether or not the particle system should be nested in hierarchy or floating on its own
        [Tooltip("whether or not the particle system should be nested in hierarchy or floating on its own")]
        [MMFEnumCondition("PositionMode", (int)PositionModes.Transform, (int)PositionModes.FeedbackPosition)]
        public bool NestParticles = true;

        protected ParticleSystem _instantiatedParticleSystem;
        protected List<ParticleSystem> _instantiatedRandomParticleSystems;

        /// <summary>
        /// On init, instantiates the particle system, positions it and nests it if needed
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(GameObject owner)
        {
            if (Active)
            {
                if (Mode == Modes.Cached)
                {
                    InstantiateParticleSystem();
                }
            }
        }

        /// <summary>
        /// Instantiates the particle system
        /// </summary>
        protected virtual void InstantiateParticleSystem()
        {
            if ((Mode == Modes.OnDemand) && (!CachedRecycle))
            {
                //do nothing
            }
            else
            {
                if (_instantiatedParticleSystem != null)
                {
                    Destroy(_instantiatedParticleSystem.gameObject);
                }
            }

            if (RandomParticlePrefabs.Count > 0)
            {
                if (Mode == Modes.Cached)
                {
                    _instantiatedRandomParticleSystems = new List<ParticleSystem>();
                    foreach(ParticleSystem system in RandomParticlePrefabs)
                    {
                        ParticleSystem newSystem = GameObject.Instantiate(system) as ParticleSystem;
                        _instantiatedRandomParticleSystems.Add(newSystem);
                    }
                }
                else
                {
                    int random = Random.Range(0, RandomParticlePrefabs.Count);
                    _instantiatedParticleSystem = GameObject.Instantiate(RandomParticlePrefabs[random]) as ParticleSystem;
                }
            }
            else
            {
                _instantiatedParticleSystem = GameObject.Instantiate(ParticlesPrefab) as ParticleSystem;
            }

            if (_instantiatedParticleSystem != null)
            {
                PositionParticleSystem(_instantiatedParticleSystem);
            }

            if ((_instantiatedRandomParticleSystems != null) && (_instantiatedRandomParticleSystems.Count > 0))
            {
                foreach (ParticleSystem system in _instantiatedRandomParticleSystems)
                {
                    PositionParticleSystem(system);
                }
            }
        }

        protected virtual void PositionParticleSystem(ParticleSystem system)
        {
            if (InstantiateParticlesPosition == null)
            {
                if (Owner != null)
                {
                    InstantiateParticlesPosition = Owner.transform;
                }
            }

            if (system != null)
            {
                system.Stop();
            }

            if (NestParticles)
            {
                if (PositionMode == PositionModes.FeedbackPosition)
                {
                    system.transform.SetParent(this.transform);
                }
                if (PositionMode == PositionModes.Transform)
                {
                    system.transform.SetParent(InstantiateParticlesPosition);
                }
            }

            system.transform.position = GetPosition(this.transform.position);
            system.Clear();
        }

        /// <summary>
        /// Gets the position 
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        protected virtual Vector3 GetPosition(Vector3 position)
        {
            switch (PositionMode)
            {
                case PositionModes.FeedbackPosition:
                    return this.transform.position + Offset;
                case PositionModes.Transform:
                    return InstantiateParticlesPosition.position + Offset;
                case PositionModes.WorldPosition:
                    return TargetWorldPosition + Offset;
                case PositionModes.Script:
                    return position + Offset;
                default:
                    return position + Offset;
            }
        }

        /// <summary>
        /// On Play, plays the feedback
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (!Active)
            {
                return;
            }

            if (Mode == Modes.OnDemand)
            {
                InstantiateParticleSystem();
            }

            if (_instantiatedParticleSystem != null)
            {
                _instantiatedParticleSystem.Stop();
                _instantiatedParticleSystem.transform.position = GetPosition(position);
                _instantiatedParticleSystem.Play();
            }

            if ((_instantiatedRandomParticleSystems != null) && (_instantiatedRandomParticleSystems.Count > 0))
            {
                foreach (ParticleSystem system in _instantiatedRandomParticleSystems)
                {
                    system.Stop();
                    system.transform.position = GetPosition(position);
                }
                int random = Random.Range(0, _instantiatedRandomParticleSystems.Count);
                _instantiatedRandomParticleSystems[random].Play();
            }
        }

        /// <summary>
        /// On Stop, stops the feedback
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (!Active)
            {
                return;
            }
            if (_instantiatedParticleSystem != null)
            {
                _instantiatedParticleSystem?.Stop();
            }    
            if ((_instantiatedRandomParticleSystems != null) && (_instantiatedRandomParticleSystems.Count > 0))
            {
                foreach(ParticleSystem system in _instantiatedRandomParticleSystems)
                {
                    system.Stop();
                }
            }
        }

        /// <summary>
        /// On Reset, stops the feedback
        /// </summary>
        protected override void CustomReset()
        {
            base.CustomReset();

            if (!Active)
            {
                return;
            }

            if (InCooldown)
            {
                return;
            }

            if (_instantiatedParticleSystem != null)
            {
                _instantiatedParticleSystem?.Stop();
            }
            if ((_instantiatedRandomParticleSystems != null) && (_instantiatedRandomParticleSystems.Count > 0))
            {
                foreach (ParticleSystem system in _instantiatedRandomParticleSystems)
                {
                    system.Stop();
                }
            }
        }
    }
}
