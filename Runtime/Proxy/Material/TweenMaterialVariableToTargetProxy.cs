using Unity.Entities;
using UnityEngine;

namespace EcsTweens
{
    public class TweenMaterialVariableToTargetProxy : ATweenMaterialProxy
    {
        [Header("Tween settings")]
        public string FieldName;
        public float StartValue;
        public float EndValue = 1;
        public float TweenTime = 1;

        protected override Entity InitializeTweeningForMaterial(TweeningMaterialComponent container)
        {
            if (container.FieldId == 0) container.FieldId = Shader.PropertyToID(FieldName);
            return container.TweeningToward(StartValue, EndValue, TweenTime, true);
        }
    }
}
