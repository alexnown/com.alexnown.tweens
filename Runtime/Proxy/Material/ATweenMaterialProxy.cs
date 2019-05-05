using Unity.Entities;
using UnityEngine;

namespace EcsTweens
{
    public abstract class ATweenMaterialProxy : MonoBehaviour
    {
        public bool InitializeTweeningOnEnable = true;
        [Header("Grab material settings")]
        public Material SpecialSharedMaterial;
        public bool UseSharedMaterial;
#if UNITY_EDITOR
        [Header("Editor only")]
        public bool DontUseSharedMaterialInEditor = true;
#endif
        

        protected TweeningMaterialComponent _materialContainer;

        private void OnEnable()
        {
            if (InitializeTweeningOnEnable) InitializeTweening();
        }

        private Material GetMaterialForTweening()
        {
            if (SpecialSharedMaterial != null)
            {
#if UNITY_EDITOR
                return DontUseSharedMaterialInEditor ? Instantiate(SpecialSharedMaterial) : SpecialSharedMaterial;
#else
                return SpecialSharedMaterial;
#endif
            }
            else
            {
                bool isShared = UseSharedMaterial;
#if UNITY_EDITOR
                isShared = UseSharedMaterial && !DontUseSharedMaterialInEditor;
#endif
                var render = GetComponent<Renderer>();
                if (render == null) return null;
                return isShared ? render.sharedMaterial : render.material;
            }
        }

        private void InitializeTweening()
        {
            var mat = GetMaterialForTweening();
            if (mat == null)
            {
                Debug.LogError("Material not found in " + name, gameObject);
                return;
            }
            if (_materialContainer == null)
            {
                _materialContainer = gameObject.AddComponent<TweeningMaterialComponent>();
                _materialContainer.TweenedMaterial = mat;
            }
            InitializeTweeningForMaterial(_materialContainer);
        }

        protected abstract Entity InitializeTweeningForMaterial(TweeningMaterialComponent container);
        
    }
}
