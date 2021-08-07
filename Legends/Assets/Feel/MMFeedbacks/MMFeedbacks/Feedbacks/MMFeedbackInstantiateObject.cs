using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// This feedback will instantiate the associated object (usually a VFX, but not necessarily), optionnally creating an object pool of them for performance
    /// </summary>
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback allows you to instantiate the object specified in its inspector, at the feedback's position (plus an optional offset). You can also optionally (and automatically) create an object pool at initialization to save on performance. In that case you'll need to specify a pool size (usually the maximum amount of these instantiated objects you plan on having in your scene at each given time).")]
    [FeedbackPath("GameObject/Instantiate Object")]
    public class MMFeedbackInstantiateObject : MMFeedback
    {
        /// the different ways to position the instantiated object :
        /// - FeedbackPosition : object will be instantiated at the position of the feedback, plus an optional offset
        /// - Transform : the object will be instantiated at the specified Transform's position, plus an optional offset
        /// - WorldPosition : the object will be instantiated at the specified world position vector, plus an optional offset
        /// - Script : the position passed in parameters when calling the feedback
        public enum PositionModes { FeedbackPosition, Transform, WorldPosition, Script }

        /// sets the inspector color for this feedback
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }
        #endif

        [Header("Instantiate Object")]
        /// the vfx object to instantiate
        [Tooltip("the vfx object to instantiate")]
        [FormerlySerializedAs("VfxToInstantiate")]
        public GameObject GameObjectToInstantiate;

        [Header("Position")]
        /// the chosen way to position the object 
        [Tooltip("the chosen way to position the object")]
        public PositionModes PositionMode = PositionModes.FeedbackPosition;
        /// the transform at which to instantiate the object
        [Tooltip("the transform at which to instantiate the object")]
        [MMFEnumCondition("PositionMode", (int)PositionModes.Transform)]
        public Transform TargetTransform;
        /// the transform at which to instantiate the object
        [Tooltip("the transform at which to instantiate the object")]
        [MMFEnumCondition("PositionMode", (int)PositionModes.WorldPosition)]
        public Vector3 TargetPosition;
        /// the position offset at which to instantiate the vfx object
        [Tooltip("the position offset at which to instantiate the vfx object")]
        public Vector3 VfxPositionOffset;

        [Header("Object Pool")]
        /// whether or not we should create automatically an object pool for this vfx
        [Tooltip("whether or not we should create automatically an object pool for this vfx")]
        public bool VfxCreateObjectPool;
        /// the initial and planned size of this object pool
        [Tooltip("the initial and planned size of this object pool")]
        public int VfxObjectPoolSize = 5;

        protected MMMiniObjectPooler _objectPool; 
        protected GameObject _newGameObject;

        /// <summary>
        /// On init we create an object pool if needed
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(GameObject owner)
        {
            base.CustomInitialization(owner);

            if (Active && VfxCreateObjectPool)
            {
                if (_objectPool != null)
                {
                    _objectPool.DestroyObjectPool();
                    Destroy(_objectPool.gameObject);
                }

                GameObject objectPoolGo = new GameObject();
                objectPoolGo.name = "FeedbackObjectPool";
                _objectPool = objectPoolGo.AddComponent<MMMiniObjectPooler>();
                _objectPool.GameObjectToPool = GameObjectToInstantiate;
                _objectPool.PoolSize = VfxObjectPoolSize;
                _objectPool.FillObjectPool();
            }
        }

        /// <summary>
        /// On Play we instantiate the specified object, either from the object pool or from scratch
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active && (GameObjectToInstantiate != null))
            {
                if (_objectPool != null)
                {
                    _newGameObject = _objectPool.GetPooledGameObject();
                    if (_newGameObject != null)
                    {
                        _newGameObject.transform.position = GetPosition(position);
                        _newGameObject.SetActive(true);
                    }
                }
                else
                {
                    _newGameObject = GameObject.Instantiate(GameObjectToInstantiate) as GameObject;
                    _newGameObject.transform.position = GetPosition(position);
                }
            }
        }

        protected virtual Vector3 GetPosition(Vector3 position)
        {
            switch (PositionMode)
            {
                case PositionModes.FeedbackPosition:
                    return this.transform.position + VfxPositionOffset;
                case PositionModes.Transform:
                    return TargetTransform.position + VfxPositionOffset;
                case PositionModes.WorldPosition:
                    return TargetPosition + VfxPositionOffset;
                case PositionModes.Script:
                    return position + VfxPositionOffset;
                default:
                    return position + VfxPositionOffset;
            }
        }
    }
}
