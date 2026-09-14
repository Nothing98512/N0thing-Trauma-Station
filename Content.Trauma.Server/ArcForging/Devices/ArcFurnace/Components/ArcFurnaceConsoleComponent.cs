using Content.Shared.DeviceLinking;

namespace Content.Trauma.Server.ArcForging.Devices.ArcFurnace.Components;

[RegisterComponent]
public sealed partial class ArcFurnaceConsoleComponent : Component
{
    public const string FurnacePort = "ArcFurnaceSender";

    public const string CranePort = "CraneSender";

    [ViewVariables]
    public EntityUid? ArcFurnace = null;

    [ViewVariables]
    public EntityUid? Crane = null;

    [DataField("maxDistance")]
    public float MaxDistance = 4f;

    public bool ArcFurnaceCraneInRange = true;

    public bool ArcFurnaceInRange = true;
}
