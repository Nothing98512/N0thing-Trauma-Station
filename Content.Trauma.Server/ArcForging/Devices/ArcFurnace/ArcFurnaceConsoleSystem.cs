using System.Linq;
using Content.Server.Administration.Logs;
using Content.Server.DeviceLinking.Systems;
using Content.Server.Power.EntitySystems;
using Content.Shared.DeviceLinking;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.Mind;
using Content.Shared.Power;
using Content.Shared.UserInterface;
using Content.Trauma.Server.ArcForging.Devices.ArcFurnace.Components;
using Content.Trauma.Shared.ArcForging.ArcFurnaceConsole;
using Robust.Server.GameObjects;
using Robust.Server.Player;

namespace Content.Trauma.Server.ArcForging.Devices.ArcFurnace
{
    public sealed partial class ArcFurnaceSystem : EntitySystem
    {
        [Dependency] private DeviceLinkSystem _signalSystem = default!;
        [Dependency] private IAdminLogManager _adminLogger = default!;
        [Dependency] private IPlayerManager _playerManager = default!;
        [Dependency] private ArcFurnaceSystem _arcFurnaceSystem = default!;
        [Dependency] private UserInterfaceSystem _uiSystem = default!;
        [Dependency] private PowerReceiverSystem _powerReceiverSystem = default!;
        [Dependency] private SharedMindSystem _mindSystem = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<ArcFurnaceConsoleComponent, ComponentInit>(OnInit);
//            SubscribeLocalEvent<ArcFurnaceConsoleComponent, UiButtonPressedMessage>(OnButtonPressed);
            SubscribeLocalEvent<ArcFurnaceConsoleComponent, AfterActivatableUIOpenEvent>(OnUIOpen);
            SubscribeLocalEvent<ArcFurnaceConsoleComponent, PowerChangedEvent>(OnPowerChanged);
            SubscribeLocalEvent<ArcFurnaceConsoleComponent, MapInitEvent>(OnMapInit);
            SubscribeLocalEvent<ArcFurnaceConsoleComponent, NewLinkEvent>(OnNewLink);
            SubscribeLocalEvent<ArcFurnaceConsoleComponent, PortDisconnectedEvent>(OnPortDisconnected);
            SubscribeLocalEvent<ArcFurnaceConsoleComponent, AnchorStateChangedEvent>(OnAnchorChanged);
        }

        private void OnInit(EntityUid uid, ArcFurnaceConsoleComponent component, ComponentInit args)
        {
            _signalSystem.EnsureSourcePorts(uid, ArcFurnaceConsoleComponent.CranePort, ArcFurnaceConsoleComponent.FurnacePort);
        }

/*        private void OnButtonPressed(EntityUid uid, ArcFurnaceConsoleComponent consoleComponent, UiButtonPressedMessage args)
        {
            if (!_powerReceiverSystem.IsPowered(uid))
                return;

            switch (args.Button)
            {
                case UiButton.Clone:
                    if (consoleComponent.Crane != null && consoleComponent.ArcFurnace != null)
                        TryClone(uid, consoleComponent.ArcFurnace.Value, consoleComponent.GeneticScanner.Value, consoleComponent: consoleComponent);
                    break;
            }
            UpdateUserInterface(uid, consoleComponent);
        }*/

        private void OnPowerChanged(EntityUid uid, ArcFurnaceConsoleComponent component, ref PowerChangedEvent args)
        {
            UpdateUserInterface(uid, component);
        }

        private void OnMapInit(EntityUid uid, ArcFurnaceConsoleComponent component, MapInitEvent args)
        {
            if (!TryComp<DeviceLinkSourceComponent>(uid, out var receiver))
                return;

            foreach (var port in receiver.Outputs.Values.SelectMany(ports => ports))
            {
                if (TryComp<ArcFurnaceCraneComponent>(port, out var crane))
                {
                    component.Crane = port;
                    crane.ConnectedConsole = uid;
                }

                if (TryComp<ArcFurnaceComponent>(port, out var furnace))
                {
                    component.ArcFurnace = port;
                    furnace.ConnectedConsole = uid;
                }
            }
        }

        private void OnNewLink(EntityUid uid, ArcFurnaceConsoleComponent component, NewLinkEvent args)
        {
            if (TryComp<ArcFurnaceCraneComponent>(args.Sink, out var crane) && args.SourcePort == ArcFurnaceConsoleComponent.CranePort)
            {
                component.Crane = args.Sink;
                crane.ConnectedConsole = uid;
            }

            if (TryComp<ArcFurnaceComponent>(args.Sink, out var furnace) && args.SourcePort == ArcFurnaceConsoleComponent.FurnacePort)
            {
                component.ArcFurnace = args.Sink;
                furnace.ConnectedConsole = uid;
            }
            RecheckConnections(uid, component.ArcFurnace, component.Crane, component);
        }

        private void OnPortDisconnected(EntityUid uid, ArcFurnaceConsoleComponent component, PortDisconnectedEvent args)
        {
            if (args.Port == ArcFurnaceConsoleComponent.FurnacePort)
                component.Crane = null;

            if (args.Port == ArcFurnaceConsoleComponent.FurnacePort)
                component.ArcFurnace = null;

            UpdateUserInterface(uid, component);
        }

        private void OnUIOpen(EntityUid uid, ArcFurnaceConsoleComponent component, AfterActivatableUIOpenEvent args)
        {
            UpdateUserInterface(uid, component);
        }

        private void OnAnchorChanged(EntityUid uid, ArcFurnaceConsoleComponent component, ref AnchorStateChangedEvent args)
        {
            if (args.Anchored)
            {
                RecheckConnections(uid, component.ArcFurnace, component.Crane, component);
                return;
            }
            UpdateUserInterface(uid, component);
        }

        public void UpdateUserInterface(EntityUid consoleUid, ArcFurnaceConsoleComponent consoleComponent)
        {
            if (!_uiSystem.HasUi(consoleUid, ArcFurnaceConsoleBoundUserInterfaceState.ArcFurnaceConsoleUiKey.Key))
                return;

            if (!_powerReceiverSystem.IsPowered(consoleUid))
            {
                _uiSystem.CloseUis(consoleUid);
                return;
            }

            var newState = GetUserInterfaceState(consoleComponent);
            _uiSystem.SetUiState(consoleUid, ArcFurnaceConsoleBoundUserInterfaceState.ArcFurnaceConsoleUiKey.Key, newState);
        }

        public void RecheckConnections(EntityUid console, EntityUid? arcFurnace, EntityUid? crane, ArcFurnaceConsoleComponent? consoleComp = null)
        {
            if (!Resolve(console, ref consoleComp))
                return;

            if (crane != null)
            {
                Transform(crane.Value).Coordinates.TryDistance(EntityManager, Transform((console)).Coordinates, out float craneDistance);
                consoleComp.ArcFurnaceCraneInRange = craneDistance <= consoleComp.MaxDistance;
            }
            if (arcFurnace != null)
            {
                Transform(arcFurnace.Value).Coordinates.TryDistance(EntityManager, Transform((console)).Coordinates, out float arcFurnaceDistance);
                consoleComp.ArcFurnaceInRange = arcFurnaceDistance <= consoleComp.MaxDistance;
            }

            UpdateUserInterface(console, consoleComp);
        }

        private ArcFurnaceConsoleBoundUserInterfaceState GetUserInterfaceState(ArcFurnaceConsoleComponent consoleComponent)
        {

            // Industrial Crane info
            bool craneConnected = false;
            bool craneInRange = consoleComponent.ArcFurnaceCraneInRange;
            if (consoleComponent.Crane != null && TryComp<ArcFurnaceCraneComponent>(consoleComponent.Crane, out var crane))
            {
                craneConnected = true;
            }

            // Arc Furnace info
            bool furnaceConnected = false;
            bool arcFurnaceInRange = consoleComponent.ArcFurnaceInRange;
            if (consoleComponent.ArcFurnace != null && TryComp<ArcFurnaceComponent>(consoleComponent.ArcFurnace, out var arcFurnace)
            && Transform(consoleComponent.ArcFurnace.Value).Anchored)
            {
                furnaceConnected = true;
            }

            return new ArcFurnaceConsoleBoundUserInterfaceState(
                craneConnected,
                craneInRange,
                furnaceConnected,
                arcFurnaceInRange
                );
        }
    }
}

