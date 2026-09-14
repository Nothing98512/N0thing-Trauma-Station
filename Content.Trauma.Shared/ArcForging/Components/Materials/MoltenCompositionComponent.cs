

namespace Content.Trauma.Shared.ArcForging.Components.Materials;

[RegisterComponent]
public sealed partial class MoltenCompositionComponent : Component
{
    [DataField]
    public Dictionary<ProtoId<MoltenMaterialPrototype>, int> MoltenMaterialComposition = new();
}
