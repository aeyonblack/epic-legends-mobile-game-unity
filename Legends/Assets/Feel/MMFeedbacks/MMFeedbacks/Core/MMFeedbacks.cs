using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;
using System.Linq;
using UnityEditor.Experimental;
using UnityEngine.Events;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// A collection of MMFeedback, meant to be played altogether.
    /// This class provides a custom inspector to add and customize feedbacks, and public methods to trigger them, stop them, etc.
    /// You can either use it on its own, or bind it from another class and trigger it from there.
    /// </summary>
    [ExecuteAlways]
    [AddComponentMenu("More Mountains/Feedbacks/MMFeedbacks")]
    [DisallowMultipleComponent]
    public class MMFeedbacks : MonoBehaviour
    {
        /// the possible directions MMFeedbacks can be played
        public enum Directions { TopToBottom, BottomToTop }
        /// the possible SafeModes (will perform checks to make sure no serialization error has damaged them)
        /// - nope : no safety
        /// - editor only : performs checks on enable
        /// - runtime only : performs checks on Awake
        /// - full : performs both editor and runtime checks, recommended setting
        public enum SafeModes { Nope, EditorOnly, RuntimeOnly, Full }
        
        /// a list of MMFeedback to trigger
        public List<MMFeedback> Feedbacks = new List<MMFeedback>();
        
        /// the possible initialization modes. If you use Script, you'll have to initialize manually by calling the Initialization method and passing it an owner
        /// Otherwise, you can have this component initialize itself at Awake or Start, and in this case the owner will be the MMFeedbacks itself
        public enum InitializationModes { Script, Awake, Start }
        /// the chosen initialization mode
        [Tooltip("the chosen initialization modes. If you use Script, you'll have to initialize manually by calling the " +
                 "Initialization method and passing it an owner. Otherwise, you can have this component initialize " +
                 "itself at Awake or Start, and in this case the owner will be the MMFeedbacks itself")]
        public InitializationModes InitializationMode = InitializationModes.Start;
        /// the selected safe mode
        [Tooltip("the selected safe mode")]
        public SafeModes SafeMode = SafeModes.Full;
        /// the selected direction
        [Tooltip("the selected direction these feedbacks should play in")]
        public Directions Direction = Directions.TopToBottom;
        /// whether or not this MMFeedbacks should invert its direction when all feedbacks have played
        [Tooltip("whether or not this MMFeedbacks should invert its direction when all feedbacks have played")]
        public bool AutoChangeDirectionOnEnd = false;
        /// whether or not to play this feedbacks automatically on Start
        [Tooltip("whether or not to play this feedbacks automatically on Start")]
        public bool AutoPlayOnStart = false;
        /// whether or not to play this feedbacks automatically on Enable
        [Tooltip("whether or not to play this feedbacks automatically on Enable")]
        public bool AutoPlayOnEnable = false;
        /// a time multiplier that will be applied to all feedback durations (initial delay, duration, delay between repeats...)
        [Tooltip("a time multiplier that will be applied to all feedback durations (initial delay, duration, delay between repeats...)")]
        public float DurationMultiplier = 1f;
        /// if this is true, more editor-only, detailed info will be displayed per feedback in the duration slot
        [Tooltip("if this is true, more editor-only, detailed info will be displayed per feedback in the duration slot")]
        public bool DisplayFullDurationDetails = false;

        /// a duration, in seconds, during which triggering a new play of this MMFeedbacks after it's been played once will be impossible
        [Tooltip("a duration, in seconds, during which triggering a new play of this MMFeedbacks after it's been played once will be impossible")]
        public float CooldownDuration = 0f;
        /// a duration, in seconds, to delay the start of this MMFeedbacks' contents play
        [Tooltip("a duration, in seconds, to delay the start of this MMFeedbacks' contents play")]
        public float InitialDelay = 0f;
        
        /// the intensity at which to play this feedback. That value will be used by most feedbacks to tune their amplitude. 1 is normal, 0.5 is half power, 0 is no effect.
        /// Note that what this value controls depends from feedback to feedback, don't hesitate to check the code to see what it does exactly.  
        [Tooltip("the intensity at which to play this feedback. That value will be used by most feedbacks to tune their amplitude. 1 is normal, 0.5 is half power, 0 is no effect." +
                 "Note that what this value controls depends from feedback to feedback, don't hesitate to check the code to see what it does exactly.")]
        public float FeedbacksIntensity = 1f;

        /// a number of UnityEvents that can be triggered at the various stages of this MMFeedbacks 
        [Tooltip("a number of UnityEvents that can be triggered at the various stages of this MMFeedbacks")] 
        public MMFeedbacksEvents Events;
        
        /// a global switch used to turn all feedbacks on or off globally
        [Tooltip("a global switch used to turn all feedbacks on or off globally")]
        public static bool GlobalMMFeedbacksActive = true;
        
        [HideInInspector]
        /// whether or not this MMFeedbacks is in debug mode
        public bool DebugActive = false;
        /// whether or not this MMFeedbacks is playing right now - meaning it hasn't been stopped yet.
        /// if you don't stop your MMFeedbacks it'll remain true of course
        public bool IsPlaying { get; protected set; }
        /// whether or not the execution of this MMFeedbacks' sequence is being prevented and waiting for a Resume() call
        public bool InScriptDrivenPause { get; set; }
        /// true if this MMFeedbacks contains at least one loop
        public bool ContainsLoop { get; set; }
        public bool ShouldRevertOnNextPlay { get; set; }

        /// The total duration (in seconds) of all the active feedbacks in this MMFeedbacks
        public float TotalDuration
        {
            get
            {
                float total = 0f;
                foreach (MMFeedback feedback in Feedbacks)
                {
                    if ((feedback != null) && (feedback.Active))
                    {
                        total += feedback.TotalDuration;    
                    }
                }
                return total;
            }
        }
        
        protected float _startTime = 0f;
        protected float _holdingMax = 0f;
        protected float _lastStartAt = 0f;
        protected WaitForSeconds _initialDelayWFS;
        
        /// <summary>
        /// On Awake we initialize our feedbacks if we're in auto mode
        /// </summary>
        protected virtual void Awake()
        {
            if ((InitializationMode == InitializationModes.Awake) && (Application.isPlaying))
            {
                Initialization(this.gameObject);
            }
            CheckForLoops();
        }

        /// <summary>
        /// On Start we initialize our feedbacks if we're in auto mode
        /// </summary>
        protected virtual void Start()
        {
            if ((InitializationMode == InitializationModes.Start) && (Application.isPlaying))
            {
                Initialization(this.gameObject);
            }
            if (AutoPlayOnStart && Application.isPlaying)
            {
                PlayFeedbacks();
            }
            CheckForLoops();
        }

        /// <summary>
        /// On Enable we initialize our feedbacks if we're in auto mode
        /// </summary>
        protected virtual void OnEnable()
        {
            if (AutoPlayOnEnable && Application.isPlaying)
            {
                PlayFeedbacks();
            }
        }

        /// <summary>
        /// Checks whether or not this MMFeedbacks contains one or more looper feedbacks
        /// </summary>
        protected virtual void CheckForLoops()
        {
            ContainsLoop = false;
            for (int i = 0; i < Feedbacks.Count; i++)
            {
                if (Feedbacks[i] != null)
                {
                    if (Feedbacks[i].LooperPause && Feedbacks[i].Active)
                    {
                        ContainsLoop = true;
                        return;
                    }
                }                
            }
        }

        /// <summary>
        /// Initializes the MMFeedbacks, setting this MMFeedbacks as the owner
        /// </summary>
        public virtual void Initialization()
        {
            Initialization(this.gameObject);
        }

        /// <summary>
        /// A public method to initialize the feedback, specifying an owner that will be used as the reference for position and hierarchy by feedbacks
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="feedbacksOwner"></param>
        public virtual void Initialization(GameObject owner)
        {
            if ((SafeMode == MMFeedbacks.SafeModes.RuntimeOnly) || (SafeMode == MMFeedbacks.SafeModes.Full))
            {
                AutoRepair();
            }

            IsPlaying = false;

            _initialDelayWFS = new WaitForSeconds(InitialDelay);

            for (int i = 0; i < Feedbacks.Count; i++)
            {
                if (Feedbacks[i] != null)
                {
                    Feedbacks[i].Initialization(owner);
                }                
            }
        }

        /// <summary>
        /// Plays all feedbacks using the MMFeedbacks' position as reference, and no attenuation
        /// </summary>
        public virtual void PlayFeedbacks()
        {
            StartCoroutine(PlayFeedbacksInternal(this.transform.position, FeedbacksIntensity));
        }

        /// <summary>
        /// Plays all feedbacks using the MMFeedbacks' position as reference, and no attenuation, and in reverse (from bottom to top)
        /// </summary>
        public virtual void PlayFeedbacksInReverse()
        {
            StartCoroutine(PlayFeedbacksInternal(this.transform.position, FeedbacksIntensity, true));
        }

        /// <summary>
        /// Plays all feedbacks, specifying a position and intensity. The position may be used by each Feedback and taken into account to spark a particle or play a sound for example.
        /// The feedbacks intensity is a factor that can be used by each Feedback to lower its intensity, usually you'll want to define that attenuation based on time or distance (using a lower 
        /// intensity value for feedbacks happening further away from the Player).
        /// Additionally you can force the feedback to play in reverse, ignoring its current condition
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksOwner"></param>
        /// <param name="feedbacksIntensity"></param>
        public virtual void PlayFeedbacks(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
        {
            StartCoroutine(PlayFeedbacksInternal(position, feedbacksIntensity, forceRevert));
        }

        /// <summary>
        /// A public coroutine you can call externally when you want to yield in a coroutine of yours until the MMFeedbacks has stopped playing
        /// typically : yield return myFeedback.PlayFeedbacksCoroutine(this.transform.position, 1.0f, false);
        /// </summary>
        /// <param name="position">The position at which the MMFeedbacks should play</param>
        /// <param name="feedbacksIntensity">The intensity of the feedback</param>
        /// <param name="forceRevert">Whether or not the MMFeedbacks should play in reverse or not</param>
        /// <returns></returns>
        public virtual IEnumerator PlayFeedbacksCoroutine(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
        {
            return PlayFeedbacksInternal(position, feedbacksIntensity, forceRevert);
        }

        /// <summary>
        /// An internal method used to play feedbacks, shouldn't be called externally
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected virtual IEnumerator PlayFeedbacksInternal(Vector3 position, float feedbacksIntensity, bool forceRevert = false)
        {
            if (ShouldRevertOnNextPlay)
            {
                Revert();
                ShouldRevertOnNextPlay = false;
            }

            if (forceRevert)
            {
                Direction = (Direction == Directions.BottomToTop) ? Directions.TopToBottom : Directions.BottomToTop;
            }

            // if we have a cooldown we prevent execution if needed
            if (CooldownDuration > 0f)
            {
                if (Time.unscaledTime - _lastStartAt < CooldownDuration)
                {
                    yield break;
                }
            }

            if (InitialDelay > 0f)
            {
                yield return _initialDelayWFS;
            }
            
            // if this MMFeedbacks is disabled in any way, we stop and don't play
            if (!this.isActiveAndEnabled)
            {
                yield break;
            }

            // if all MMFeedbacks are disabled globally, we stop and don't play
            if (!GlobalMMFeedbacksActive)
            {
                yield break;
            }
            
            Events.TriggerOnPlay(this);

            _startTime = Time.unscaledTime;
            _holdingMax = 0f;
            _lastStartAt = _startTime;

            ResetFeedbacks();

            // test if a pause or holding pause is found
            bool pauseFound = false;
            for (int i = 0; i < Feedbacks.Count; i++)
            {
                if ((Feedbacks[i].Pause != null) && (Feedbacks[i].Active))
                {
                    pauseFound = true;
                }
                if ((Feedbacks[i].HoldingPause == true) && (Feedbacks[i].Active))
                {
                    pauseFound = true;
                }
            }

            if (!pauseFound)
            {
                // if no pause was found, we just play all feedbacks at once
                IsPlaying = true;
                for (int i = 0; i < Feedbacks.Count; i++)
                {
                    if (FeedbackCanPlay(Feedbacks[i]))
                    {
                        Feedbacks[i].Play(position, feedbacksIntensity);
                    }
                }

                Events.TriggerOnComplete(this);
                ApplyAutoRevert();
            }
            else
            {
                // if at least one pause was found
                StartCoroutine(PausedFeedbacksCo(position, feedbacksIntensity));
            }            
        }

        /// <summary>
        /// This will return true if the conditions defined in the specified feedback's Timing section allow it to play in the current play direction of this MMFeedbacks
        /// </summary>
        /// <param name="feedback"></param>
        /// <returns></returns>
        protected bool FeedbackCanPlay(MMFeedback feedback)
        {
            if (feedback.Timing.MMFeedbacksDirectionCondition == MMFeedbackTiming.MMFeedbacksDirectionConditions.Always)
            {
                return true;
            }
            else if (((Direction == Directions.TopToBottom) && (feedback.Timing.MMFeedbacksDirectionCondition == MMFeedbackTiming.MMFeedbacksDirectionConditions.OnlyWhenForwards))
                || ((Direction == Directions.BottomToTop) && (feedback.Timing.MMFeedbacksDirectionCondition == MMFeedbackTiming.MMFeedbacksDirectionConditions.OnlyWhenBackwards)))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// A coroutine used to pause feedbacks
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        /// <returns></returns>
        protected virtual IEnumerator PausedFeedbacksCo(Vector3 position, float feedbacksIntensity)
        {
            IsPlaying = true;

            int i = (Direction == Directions.TopToBottom) ? 0 : Feedbacks.Count-1;

            while ((i >= 0) && (i < Feedbacks.Count))
            {
                if (!IsPlaying)
                {
                    yield break;
                }
                
                if (((Feedbacks[i].Active) && (Feedbacks[i].ScriptDrivenPause)) || InScriptDrivenPause)
                {
                    InScriptDrivenPause = true;
                    while (InScriptDrivenPause)
                    {
                        yield return null;
                    }
                }

                // handles holding pauses
                if ((Feedbacks[i].Active)
                    && ((Feedbacks[i].HoldingPause == true) || (Feedbacks[i].LooperPause == true)))
                {
                    Events.TriggerOnPause(this);
                    // we stay here until all previous feedbacks have finished
                    while (Time.unscaledTime - _lastStartAt < _holdingMax)
                    {
                        yield return null;
                    }
                    _holdingMax = 0f;
                    _lastStartAt = Time.unscaledTime;
                }

                // plays the feedback
                if (FeedbackCanPlay(Feedbacks[i]))
                {
                    Feedbacks[i].Play(position, feedbacksIntensity);
                }

                // Handles pause
                if ((Feedbacks[i].Pause != null) && (Feedbacks[i].Active))
                {
                    bool shouldPause = true;
                    if (Feedbacks[i].Chance < 100)
                    {
                        float random = Random.Range(0f, 100f);
                        if (random > Feedbacks[i].Chance)
                        {
                            shouldPause = false;
                        }
                    }

                    if (shouldPause)
                    {
                        yield return Feedbacks[i].Pause;
                        Events.TriggerOnResume(this);
                        _lastStartAt = Time.unscaledTime;
                        _holdingMax = 0f;
                    }
                }

                // updates holding max
                if (Feedbacks[i].Active)
                {
                    if (Feedbacks[i].Pause == null)
                    {
                        float feedbackDuration = Feedbacks[i].TotalDuration;
                        _holdingMax = Mathf.Max(feedbackDuration, _holdingMax);
                    }
                }

                // handles looper
                if ((Feedbacks[i].LooperPause == true)
                    && (Feedbacks[i].Active)
                    && (((Feedbacks[i] as MMFeedbackLooper).NumberOfLoopsLeft > 0) || (Feedbacks[i] as MMFeedbackLooper).InInfiniteLoop))
                {
                    // we determine the index we should start again at
                    bool loopAtLastPause = (Feedbacks[i] as MMFeedbackLooper).LoopAtLastPause;
                    bool loopAtLastLoopStart = (Feedbacks[i] as MMFeedbackLooper).LoopAtLastLoopStart;

                    int newi = 0;

                    int j = (Direction == Directions.TopToBottom) ? i - 1 : i + 1;

                    while ((j >= 0) && (j <= Feedbacks.Count))
                    {
                        // if we're at the start
                        if (j == 0)
                        {
                            newi = j - 1;
                            break;
                        }
                        if (j == Feedbacks.Count)
                        {
                            newi = j ;
                            break;
                        }
                        // if we've found a pause
                        if ((Feedbacks[j].Pause != null)
                            && (Feedbacks[j].FeedbackDuration > 0f)
                            && loopAtLastPause && (Feedbacks[j].Active))
                        {
                            newi = j;
                            break;
                        }
                        // if we've found a looper start
                        if ((Feedbacks[j].LooperStart == true)
                            && loopAtLastLoopStart
                            && (Feedbacks[j].Active))
                        {
                            newi = j;
                            break;
                        }

                        j += (Direction == Directions.TopToBottom) ? -1 : 1;
                    }
                    i = newi;
                }
                i += (Direction == Directions.TopToBottom) ? 1 : -1;
            }
            Events.TriggerOnComplete(this);
            ApplyAutoRevert();
        }

        /// <summary>
        /// Readies the MMFeedbacks to revert direction on the next play
        /// </summary>
        protected virtual void ApplyAutoRevert()
        {
            if (AutoChangeDirectionOnEnd)
            {
                ShouldRevertOnNextPlay = true;
            }
        }

        /// <summary>
        /// Stops all feedbacks from playing. 
        /// </summary>
        public virtual void StopFeedbacks()
        {
            for (int i = 0; i < Feedbacks.Count; i++)
            {
                Feedbacks[i].Stop(this.transform.position, 1.0f);
            }
            IsPlaying = false;
        }

        /// <summary>
        /// Stops all feedbacks from playing, specifying a position and intensity that can be used by the Feedbacks 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        public virtual void StopFeedbacks(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            for (int i = 0; i < Feedbacks.Count; i++)
            {
                Feedbacks[i].Stop(position, feedbacksIntensity);
            }
            IsPlaying = false;
        }

        /// <summary>
        /// Calls each feedback's Reset method if they've defined one. An example of that can be resetting the initial color of a flickering renderer.
        /// </summary>
        public virtual void ResetFeedbacks()
        {
            for (int i = 0; i < Feedbacks.Count; i++)
            {
                if ((Feedbacks[i] != null) && (Feedbacks[i].Active))
                {
                    Feedbacks[i].ResetFeedback();    
                }
            }
            IsPlaying = false;
        }

        /// <summary>
        /// Changes the direction of this MMFeedbacks
        /// </summary>
        public virtual void Revert()
        {
            Events.TriggerOnRevert(this);
            Direction = (Direction == Directions.BottomToTop) ? Directions.TopToBottom : Directions.BottomToTop;
        }

        /// <summary>
        /// Pauses execution of a sequence, which can then be resumed by calling ResumeFeedbacks()
        /// </summary>
        public virtual void PauseFeedbacks()
        {
            Events.TriggerOnPause(this);
            InScriptDrivenPause = true;
        }

        /// <summary>
        /// Resumes execution of a sequence if a script driven pause is in progress
        /// </summary>
        public virtual void ResumeFeedbacks()
        {
            Events.TriggerOnResume(this);
            InScriptDrivenPause = false;
        }

        /// <summary>
        /// On Disable we stop all feedbacks
        /// </summary>
        protected virtual void OnDisable()
        {
            /*if (IsPlaying)
            {
                StopFeedbacks();
                StopAllCoroutines();
            }*/
        }

        /// <summary>
        /// On validate, we make sure our DurationMultiplier remains positive
        /// </summary>
        protected void OnValidate()
        {
            DurationMultiplier = Mathf.Clamp(DurationMultiplier, 0f, Single.MaxValue);
        }

        /// <summary>
        /// On Destroy, removes all feedbacks from this MMFeedbacks to avoid any leftovers
        /// </summary>
        protected virtual void OnDestroy()
        {
            IsPlaying = false;
            #if UNITY_EDITOR
            if (!Application.isPlaying)
            {            
                // we remove all binders
                foreach (MMFeedback feedback in Feedbacks)
                {
                    EditorApplication.delayCall += () =>
                    {
                        DestroyImmediate(feedback);
                    };                    
                }
            }
            #endif
        }

        /// <summary>
        /// Applies this feedback's time multiplier to a duration (in seconds)
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        public virtual float ApplyTimeMultiplier(float duration)
        {
            return duration * DurationMultiplier;
        }

        /// <summary>
        /// Unity sometimes has serialization issues. 
        /// This method fixes that by fixing any bad sync that could happen.
        /// </summary>
        public virtual void AutoRepair()
        {
            List<Component> components = components = new List<Component>();
            components = this.gameObject.GetComponents<Component>().ToList();
            foreach (Component component in components)
            {
                if (component is MMFeedback)
                {
                    bool found = false;
                    for (int i = 0; i < Feedbacks.Count; i++)
                    {
                        if (Feedbacks[i] == (MMFeedback)component)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        Feedbacks.Add((MMFeedback)component);
                    }
                }
            }
        }            
    }
}
