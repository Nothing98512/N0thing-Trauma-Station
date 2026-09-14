using Robust.Shared.Serialization;

namespace Content.Trauma.Shared.ArcForging.ArcFurnaceConsole;

[Serializable, NetSerializable]
public sealed class ArcFurnaceConsoleBoundUserInterfaceState : BoundUserInterfaceState
{
    public readonly bool FurnaceConnected;
    public readonly bool CraneConnected;
    public readonly bool FurnaceInRange;
    public readonly bool CraneInRange;

    public ArcFurnaceConsoleBoundUserInterfaceState(bool craneConnected, bool craneInRange, bool furnaceConnected, bool arcFurnaceInRange)
    {
        FurnaceConnected = furnaceConnected;
        CraneConnected = craneConnected;
        FurnaceInRange = arcFurnaceInRange;
        CraneInRange = craneInRange;
    }

    [Serializable, NetSerializable]
    public enum ArcFurnaceConsoleUiKey : byte
    {
        Key
    }
}
