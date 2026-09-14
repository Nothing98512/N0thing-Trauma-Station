using Content.Shared.DeviceLinking;

namespace Content.Trauma.Server.ArcForging.Devices.ArcFurnace.Components;

[RegisterComponent]
public sealed partial class ArcFurnaceComponent : Component
{
    [DataField]
    public ProtoId<SinkPortPrototype> FurnacePort = "ArcFurnaceReceiver";

    [ViewVariables]
    public EntityUid? ConnectedConsole;
}
