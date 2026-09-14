using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;

namespace Content.Trauma.Shared.ArcForging.Components.Materials;

[Prototype("moltenmaterial")]
public sealed partial class MoltenMaterialPrototype : IPrototype, IInheritingPrototype
{
    [ViewVariables]
    [ParentDataField(typeof(AbstractPrototypeIdArraySerializer<MoltenMaterialPrototype>))]
    public string[]? Parents { get; private set; }

    [ViewVariables]
    [AbstractDataField]
    public bool Abstract { get; private set; } = false;

    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public string Name = string.Empty;
}
