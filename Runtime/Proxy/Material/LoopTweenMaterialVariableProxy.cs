using Unity.Entities;
using UnityEngine;

namespace EcsTweens
{
    public class LoopTweenMaterialVariableProxy : ATweenMaterialProxy
    {
        [Header("Tween settings")]
        public string FieldName;
        public float LoopTime = 1;
        public LoopTweenParams Settings;

        protected override Entity InitializeTweeningForMaterial(TweeningMaterialComponent container)
        {
            if (container.FieldId == 0) container.FieldId = Shader.PropertyToID(FieldName);
            return container.TweeningLoop(LoopTime, Settings);
        }
    }
}
