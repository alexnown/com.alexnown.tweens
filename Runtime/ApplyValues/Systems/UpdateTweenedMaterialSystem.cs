namespace EcsTweens
{
    public class UpdateTweenedMaterialSystem : ASetTweenedValueSystem<TweeningMaterialComponent>
    {
        protected override void ApplyTweenValue(TweeningMaterialComponent renderer, ref FloatContainer value)
        {
            renderer.TweenedMaterial.SetFloat(renderer.FieldId, value.Value);
        }
    }
}
