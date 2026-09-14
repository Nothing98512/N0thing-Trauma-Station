using JetBrains.Annotations;
using Content.Trauma.Shared.ArcForging.ArcFurnaceConsole;
using Robust.Client.UserInterface;

namespace Content.Trauma.Client.ArcForging.UI;

[UsedImplicitly]
public sealed class ArcFurnaceConsoleBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private ArcFurnaceConsoleWindow? _window;

    public ArcFurnaceConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<ArcFurnaceConsoleWindow>();
        _window.Title = Loc.GetString("arc-furnace-window-title");
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        _window?.Populate((ArcFurnaceConsoleBoundUserInterfaceState) state);
    }
}
