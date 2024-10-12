/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace InventorySimulator;

public partial class InventorySimulator
{
    public required InventorySimulatorConfig Config { get; set; }

    public void OnConfigParsed(InventorySimulatorConfig config)
    {
        if (config.Version != ConfigVersion) throw new Exception($"You have a wrong config version ({config.Version}). Delete it and restart the server to get the right version ({ConfigVersion})!");

        Config = config;
    }

}

public class InventorySimulatorConfig : BasePluginConfig
{
    public override int Version { get; set; } = 2;


    [JsonPropertyName("Invsim_stattrak_ignore_bots")]
    public bool Invsim_stattrak_ignore_bots { get; set; } = true;

    [JsonPropertyName("Invsim_spraychanger_enabled")]
    public bool Invsim_spraychanger_enabled { get; set; } = true;

    [JsonPropertyName("Invsim_ws_enabled")]
    public bool Invsim_ws_enabled { get; set; } = true;

    [JsonPropertyName("Invsim_validation_enabled")]
    public bool Invsim_validation_enabled { get; set; } = true;

    [JsonPropertyName("Invsim_minmodels")]
    public int Invsim_minmodels { get; set; } = 1;

    [JsonPropertyName("Invsim_ws_cooldown")]
    public int Invsim_ws_cooldown { get; set; } = 30;

    [JsonPropertyName("Invsim_spray_cooldown")]
    public int Invsim_spray_cooldown { get; set; } = 30;

    [JsonPropertyName("Invsim_apikey")]
    public string Invsim_apikey { get; set; } = "";

    [JsonPropertyName("Invsim_hostname")]
    public string Invsim_hostname { get; set; } = "inventory.cstrike.app";

    [JsonPropertyName("Invsim_protocol")]
    public string Invsim_protocol { get; set; } = "https";

    [JsonPropertyName("Invsim_ws_alias")]
    public string[] Invsim_ws_alias { get; set; } = ["ws", "wp"];

    [JsonPropertyName("Invsim_spray_alias")]
    public string[] Invsim_spray_alias { get; set; } = ["spray"];
}
