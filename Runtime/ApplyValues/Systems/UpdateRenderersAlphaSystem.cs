using UnityEngine;

namespace EcsTweens
{
    public class UpdateRenderersAlphaSystem : ASetTweenedValueSystem<SpriteRenderer>
    {
        protected override void ApplyTweenValue(SpriteRenderer renderer, ref FloatContainer value)
        {
            var color = renderer.color;
            color.a = value.Value;
            renderer.color = color;
        }
    }
}
