﻿/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API;

namespace InventorySimulator;

public partial class InventorySimulator
{
    public void GivePlayerMusicKit(CCSPlayerController player)
    {
        if (player.InventoryServices == null) return;
        var inventory = GetPlayerInventory(player);
        if (!inventory.HasProperty("mk")) return;
        player.InventoryServices.MusicID = inventory.GetUShort("mk");
    }

    public void GivePlayerPin(CCSPlayerController player)
    {
        if (player.InventoryServices == null) return;
        var inventory = GetPlayerInventory(player);
        if (!inventory.HasProperty("pi")) return;
        for (var index = 0; index < player.InventoryServices.Rank.Length; index++)
        {
            player.InventoryServices.Rank[index] = index == 5 ? (MedalRank_t)inventory.GetUInt("pi") : MedalRank_t.MEDAL_RANK_NONE;
        }
    }

    public void GivePlayerGloves(CCSPlayerController player)
    {
        // To avoid exceptions, we check if the player is alive before accessing EconGloves. This ensures that
        // the code only proceeds if the player is in a valid state.
        if (player.PlayerPawn.Value!.LifeState != (byte)LifeState_t.LIFE_ALIVE)
            return;

        var inventory = GetPlayerInventory(player);
        var team = player.TeamNum;
        if (!inventory.HasProperty("gl", team)) return;

        var itemDef = inventory.GetUShort("gl", team);
        if (!inventory.HasProperty("pa", team, itemDef)) return;

        var glove = player.PlayerPawn.Value.EconGloves;
        var paintKit = inventory.GetInt("pa", team, itemDef, 0);
        var seed = inventory.GetInt("se", team, itemDef, 1);
        var wear = inventory.GetFloat("fl", team, itemDef, 0.0f);

        Server.NextFrame(() =>
        {
            glove.Initialized = true;
            glove.ItemDefinitionIndex = itemDef;
            UpdatePlayerEconItemID(glove);
            glove.NetworkedDynamicAttributes.Attributes.RemoveAll();
            glove.AttributeList.Attributes.RemoveAll();
            SetOrAddAttributeValueByName(glove.NetworkedDynamicAttributes, "set item texture prefab", paintKit);
            SetOrAddAttributeValueByName(glove.NetworkedDynamicAttributes, "set item texture seed", seed);
            SetOrAddAttributeValueByName(glove.NetworkedDynamicAttributes, "set item texture wear", wear);
            // We also need to update AttributeList to overwrite owned glove attributes.
            SetOrAddAttributeValueByName(glove.AttributeList, "set item texture prefab", paintKit);
            SetOrAddAttributeValueByName(glove.AttributeList, "set item texture seed", seed);
            SetOrAddAttributeValueByName(glove.AttributeList, "set item texture wear", wear);
            SetBodygroup(player, "default_gloves");
        });
    }

    public void GivePlayerAgent(CCSPlayerController player)
    {
        if (MinModelsCvar.Value > 0)
        {
            // For now any value non-zero will force SAS & Phoenix.
            // In the future: 1 - Map agents only, 2 - SAS & Phoenix.
            if (player.Team == CsTeam.Terrorist)
                SetPlayerModel(player, "characters/models/tm_phoenix/tm_phoenix.vmdl");

            if (player.Team == CsTeam.CounterTerrorist)
                SetPlayerModel(player, "characters/models/ctm_sas/ctm_sas.vmdl");

            return;
        }

        var team = player.TeamNum;
        var inventory = GetPlayerInventory(player);
        var model = inventory.GetString("agm", team);

        if (model == null) return;
        SetPlayerModel(player, GetAgentModelPath(model));
    }

    public void GivePlayerWeaponSkin(CCSPlayerController player, CBasePlayerWeapon weapon)
    {
        var isKnife = IsKnifeClassName(weapon.DesignerName);
        var inventory = GetPlayerInventory(player);
        var itemDef = weapon.AttributeManager.Item.ItemDefinitionIndex;
        var team = player.TeamNum;
        var hasKnife = inventory.HasProperty("me", team);
        var isCustom = inventory.HasProperty("cw", team, itemDef);

        if (!hasKnife && !isCustom) return;

        if (isKnife)
        {
            itemDef = inventory.GetUShort("me", team);
            if (weapon.AttributeManager.Item.ItemDefinitionIndex != itemDef)
            {
                SubclassChange(weapon, itemDef);
            }
            weapon.AttributeManager.Item.ItemDefinitionIndex = itemDef;
            weapon.AttributeManager.Item.EntityQuality = 3;
        }

        UpdatePlayerEconItemID(weapon.AttributeManager.Item);

        var paintKit = inventory.GetInt("pa", team, itemDef, 0);
        var seed = inventory.GetInt("se", team, itemDef, 1);
        var wear = inventory.GetFloat("fl", team, itemDef, 0.0f);
        weapon.FallbackPaintKit = paintKit;
        weapon.FallbackSeed = seed;
        weapon.FallbackWear = wear;
        weapon.FallbackStatTrak = inventory.GetInt("st", team, itemDef, -1);
        weapon.AttributeManager.Item.CustomName = inventory.GetString("nt", team, itemDef, "");

        weapon.AttributeManager.Item.NetworkedDynamicAttributes.Attributes.RemoveAll();
        SetOrAddAttributeValueByName(weapon.AttributeManager.Item.NetworkedDynamicAttributes, "set item texture prefab", paintKit);
        SetOrAddAttributeValueByName(weapon.AttributeManager.Item.NetworkedDynamicAttributes, "set item texture seed", seed);
        SetOrAddAttributeValueByName(weapon.AttributeManager.Item.NetworkedDynamicAttributes, "set item texture wear", wear);

        if (!isKnife)
        {
            for (int slot = 0; slot < 4; slot++)
            {
                // To set the ID of the sticker, we need to use a workaround. In the items_game.txt file, locate the
                // sticker slot 0 id entry. It should be marked with stored_as_integer set to 1. This means we need to
                // treat a uint as a float. For example, if the uint stickerId is 2229, we would interpret its value as
                // if it were a float (e.g., float stickerId = 3.12349e-42f).
                // @see https://gitlab.com/KittenPopo/csgo-2018-source/-/blame/main/game/shared/econ/econ_item_view.cpp#L194
                SetOrAddAttributeValueByName(weapon.AttributeManager.Item.NetworkedDynamicAttributes, $"sticker slot {slot} id", inventory.GetIntAsFloat("ss", team, itemDef, slot, 0));
                SetOrAddAttributeValueByName(weapon.AttributeManager.Item.NetworkedDynamicAttributes, $"sticker slot {slot} wear", inventory.GetFloat("sf", team, itemDef, slot, 0.0f));
            }
            UpdatePlayerWeaponMeshGroupMask(player, weapon, inventory.HasProperty("pal", team, itemDef));
        }
    }
}
