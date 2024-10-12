/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using System.Collections.Concurrent;

namespace InventorySimulator;

public partial class InventorySimulator
{
    public readonly FakeConVar<string> invsim_file = new("invsim_file", "File to load when plugin is loaded.", "inventories.json");

    public readonly HashSet<ulong> FetchingPlayerInventory = [];
    public readonly HashSet<ulong> LoadedPlayerInventory = [];

    public readonly ConcurrentDictionary<ulong, long> PlayerCooldownManager = [];
    public readonly ConcurrentDictionary<ulong, long> PlayerSprayCooldownManager = [];
    public readonly ConcurrentDictionary<ulong, (CCSPlayerController?, PlayerInventory)> PlayerOnTickInventoryManager = [];
    public readonly ConcurrentDictionary<ulong, PlayerInventory> PlayerInventoryManager = [];
    public readonly ConcurrentDictionary<ulong, bool> PlayerGiveNextSpawn = [];

    public readonly PlayerInventory EmptyInventory = new();

    public static readonly string InventoryFileDir = "csgo/addons/counterstrikesharp/configs/plugins/InventorySimulator";
    public static readonly ulong MinimumCustomItemID = 68719476736;

    public ulong NextItemId = MinimumCustomItemID;
    public int NextFadeSeed = 3;
}
