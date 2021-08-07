using UnityEngine;

namespace MoreMountains.Tools
{
    [System.Serializable]
    public class MMLayer
    {
        [SerializeField]
        protected int _layerIndex = 0;

        public int LayerIndex
        {
            get { return _layerIndex; }
        }

        public void Set(int _layerIndex)
        {
            if (_layerIndex > 0 && _layerIndex < 32)
            {
                this._layerIndex = _layerIndex;
            }
        }

        public int Mask
        {
            get { return 1 << _layerIndex; }
        }
    }
}