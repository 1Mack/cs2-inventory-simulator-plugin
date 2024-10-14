/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;

namespace InventorySimulator;

[MinimumApiVersion(235)]
public partial class InventorySimulator : BasePlugin, IPluginConfig<InventorySimulatorConfig>
{
    public override string ModuleAuthor => "Ian Lucas";
    public override string ModuleDescription => "Inventory Simulator (inventory.cstrike.app)";
    public override string ModuleName => "InventorySimulator";
    public override string ModuleVersion => "1.0.1";
    public static int ConfigVersion => 2;
    public override void Load(bool hotReload)
    {
        RegisterListener<Listeners.OnTick>(OnTick);
        RegisterListener<Listeners.OnEntityCreated>(OnEntityCreated);
        RegisterEventHandler<EventPlayerConnect>(OnPlayerConnect);
        RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
        RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
        VirtualFunctions.GiveNamedItemFunc.Hook(OnGiveNamedItemPost, HookMode.Post);
        RegisterEventHandler<EventPlayerDeath>(OnPlayerDeathPre, HookMode.Pre);
        RegisterEventHandler<EventRoundMvp>(OnRoundMvpPre, HookMode.Pre);
        RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);

        LoadPlayerInventories();
        invsim_file.ValueChanged += OnInvsimFileChanged;

        foreach (string command in Config.Invsim_ws_alias)
        {
            AddCommand("css_" + command, "Refreshes player's inventory.", OnWSCommand);
        }

        foreach (string command in Config.Invsim_spray_alias)
        {
            AddCommand("css_" + command, "Spray player's graffiti.", OnSprayCommand);
        }

    }
}
