using System.Reflection;
using HarmonyLib;

namespace DeathTweaks;

[HarmonyPatch(typeof(Player), "OnDeath")]
[HarmonyPriority(Priority.First)]
internal class MainOnDeath
{
    private static bool Prefix(Player __instance)
    {
        if (!modEnabled.Value)
            return true;

        __instance.m_nview.GetZDO().Set("dead", true);
        __instance.m_nview.InvokeRPC(ZNetView.Everybody, "OnDeath");
        Game.instance.IncrementPlayerStat(PlayerStatType.Deaths);
        switch (__instance.m_lastHit.m_hitType)
        {
            case HitData.HitType.Undefined:
                Game.instance.IncrementPlayerStat(PlayerStatType.DeathByUndefined);
                break;
            case HitData.HitType.EnemyHit:
                Game.instance.IncrementPlayerStat(PlayerStatType.DeathByEnemyHit);
                break;
            case HitData.HitType.PlayerHit:
                Game.instance.IncrementPlayerStat(PlayerStatType.DeathByPlayerHit);
                break;
            case HitData.HitType.Fall:
                Game.instance.IncrementPlayerStat(PlayerStatType.DeathByFall);
                break;
            case HitData.HitType.Drowning:
                Game.instance.IncrementPlayerStat(PlayerStatType.DeathByDrowning);
                break;
            case HitData.HitType.Burning:
                Game.instance.IncrementPlayerStat(PlayerStatType.DeathByBurning);
                break;
            case HitData.HitType.Freezing:
                Game.instance.IncrementPlayerStat(PlayerStatType.DeathByFreezing);
                break;
            case HitData.HitType.Poisoned:
                Game.instance.IncrementPlayerStat(PlayerStatType.DeathByPoisoned);
                break;
            case HitData.HitType.Water:
                Game.instance.IncrementPlayerStat(PlayerStatType.DeathByWater);
                break;
            case HitData.HitType.Smoke:
                Game.instance.IncrementPlayerStat(PlayerStatType.DeathBySmoke);
                break;
            case HitData.HitType.EdgeOfWorld:
                Game.instance.IncrementPlayerStat(PlayerStatType.DeathByEdgeOfWorld);
                break;
            case HitData.HitType.Impact:
                Game.instance.IncrementPlayerStat(PlayerStatType.DeathByImpact);
                break;
            case HitData.HitType.Cart:
                Game.instance.IncrementPlayerStat(PlayerStatType.DeathByCart);
                break;
            case HitData.HitType.Tree:
                Game.instance.IncrementPlayerStat(PlayerStatType.DeathByTree);
                break;
            case HitData.HitType.Self:
                Game.instance.IncrementPlayerStat(PlayerStatType.DeathBySelf);
                break;
            case HitData.HitType.Structural:
                Game.instance.IncrementPlayerStat(PlayerStatType.DeathByStructural);
                break;
            case HitData.HitType.Turret:
                Game.instance.IncrementPlayerStat(PlayerStatType.DeathByTurret);
                break;
            case HitData.HitType.Boat:
                Game.instance.IncrementPlayerStat(PlayerStatType.DeathByBoat);
                break;
            case HitData.HitType.Stalagtite:
                Game.instance.IncrementPlayerStat(PlayerStatType.DeathByStalagtite);
                break;
            default:
                ZLog.LogWarning("Not implemented death type " + __instance.m_lastHit.m_hitType);
                break;
        }

        Game.instance.GetPlayerProfile().SetDeathPoint(__instance.transform.position);

        if (createDeathEffects.Value)
            Traverse.Create(__instance).Method("CreateDeathEffects").GetValue();

        var drop_inventorys = new List<InventoryInfos>();


        if (!keepAllItems.Value)
        {
            //List<Inventory> inventories = new List<Inventory>();

            if (quickSlotsAssembly != null)
            {
                var extendedInventory = quickSlotsAssembly.GetType("EquipmentAndQuickSlots.InventoryExtensions")
                    .GetMethod("Extended", BindingFlags.Public | BindingFlags.Static)
                    .Invoke(null, new object[] { __instance.m_inventory });
                foreach (var inventory in (List<Inventory>)quickSlotsAssembly
                             .GetType("EquipmentAndQuickSlots.ExtendedInventory")
                             .GetField("_inventories", BindingFlags.NonPublic | BindingFlags.Instance)
                             .GetValue(extendedInventory))
                    drop_inventorys.Add(new InventoryInfos(inventory, new List<ItemDrop.ItemData>()));
            } else
            {
                drop_inventorys.Add(new InventoryInfos(__instance.m_inventory, new List<ItemDrop.ItemData>()));
            }

            var keepItemTypeArray = string.IsNullOrEmpty(keepItemTypes.Value)
                ? new string[0]
                : keepItemTypes.Value.Split(',');
            var keepItemNameArray = string.IsNullOrEmpty(keepItemNames.Value)
                ? new string[0]
                : keepItemNames.Value.Split(',');
            var destroyItemTypeArray = string.IsNullOrEmpty(destroyItemTypes.Value)
                ? new string[0]
                : destroyItemTypes.Value.Split(',');
            var destroyItemNameArray = string.IsNullOrEmpty(destroyItemNames.Value)
                ? new string[0]
                : destroyItemNames.Value.Split(',');
            var dropItemTypeArray = string.IsNullOrEmpty(dropItemTypes.Value)
                ? new string[0]
                : dropItemTypes.Value.Split(',');
            var dropItemNameArray = string.IsNullOrEmpty(dropItemNames.Value)
                ? new string[0]
                : dropItemNames.Value.Split(',');

            for (var inv_num = 0; inv_num < drop_inventorys.Count; inv_num++)
            {
                var inv = drop_inventorys[inv_num].inventory;
                var dropItems = drop_inventorys[inv_num].drop_list;

                Debug($"  inventory {inv_num}");

                var items2 = inv.GetAllItems();
                for (var i2 = items2.Count - 1; i2 >= 0; i2--)
                {
                    var item = items2[i2];
                    Debug($"   Item  Name: {item.m_dropPrefab.name}   Cat: {item.m_shared.m_itemType}");
                }

                if (quickSlotsAssembly != null && keepQuickSlotItems.Value && inv == (Inventory)quickSlotsAssembly
                        .GetType("EquipmentAndQuickSlots.PlayerExtensions")
                        .GetMethod("GetQuickSlotInventory", BindingFlags.Public | BindingFlags.Static)
                        .Invoke(null, new object[] { __instance }))
                {
                    Debug("Skipping quick slot inventory");
                    continue;
                }

                var keepItems =
                    Traverse.Create(inv).Field("m_inventory").GetValue<List<ItemDrop.ItemData>>();

                if (destroyAllItems.Value)
                    keepItems.Clear();
                else
                    for (var j = keepItems.Count - 1; j >= 0; j--)
                    {
                        var item = keepItems[j];

                        if (keepEquippedItems.Value && item.m_equipped)
                            continue;

                        if (keepHotbarItems.Value && inv.GetName() == __instance.m_inventory.GetName()
                                                  && item.m_gridPos.y == 0)
                            continue;

                        if (item.m_shared.m_questItem)
                            continue;

                        if (destroyItemTypeArray.Contains(Enum.GetName(typeof(ItemDrop.ItemData.ItemType),
                                item.m_shared.m_itemType)))
                        {
                            keepItems.RemoveAt(j);
                            continue;
                        }

                        if (destroyItemNameArray.Contains(item.m_dropPrefab.name))
                        {
                            keepItems.RemoveAt(j);
                            continue;
                        }

                        if (keepItemTypeArray.Contains(Enum.GetName(typeof(ItemDrop.ItemData.ItemType),
                                item.m_shared.m_itemType)))
                            continue;

                        if (keepItemNameArray.Contains(item.m_dropPrefab.name))
                            continue;


                        if (dropItemTypeArray.Contains(Enum.GetName(typeof(ItemDrop.ItemData.ItemType),
                                item.m_shared.m_itemType)))
                        {
                            dropItems.Add(item);
                            keepItems.RemoveAt(j);
                            continue;
                        }

                        if (dropItemNameArray.Contains(item.m_dropPrefab.name))
                        {
                            dropItems.Add(item);
                            keepItems.RemoveAt(j);
                            continue;
                        }

                        if (item.m_shared.m_teleportable && keepTeleportableItems.Value) continue;

                        dropItems.Add(item);
                        keepItems.RemoveAt(j);
                    }

                Traverse.Create(inv).Method("Changed").GetValue();
            }
        }

        /*
         * with the EquipmentAndQuickSlots Mod we need a custom Tombstone for the Quick and Eqipment Slots
         * otherwitse the items are lost if the Tombstone is collected after quiting the game
         *
         * The Items in the special slots have to be marked with item.m_customData to detect in which slot they have to inserted
         *
         */

        for (var inv_num = 0; inv_num < drop_inventorys.Count; inv_num++)
        {
            var inv = drop_inventorys[inv_num].inventory;
            var dropItems = drop_inventorys[inv_num].drop_list;

            if (useTombStone.Value && dropItems.Any())
            {
                Debug("    dropItems.Any");


                var position = __instance.GetCenterPoint() + Vector3.left * (inv_num * 2);

                var gameObject = Instantiate(__instance.m_tombstone, position, __instance.transform.rotation);
                gameObject.GetComponent<Container>().GetInventory().RemoveAll();


                var width = Traverse.Create(inv).Field("m_width").GetValue<int>();
                var height = Traverse.Create(inv).Field("m_height").GetValue<int>();
                Traverse.Create(gameObject.GetComponent<Container>().GetInventory()).Field("m_width")
                    .SetValue(width);
                Traverse.Create(gameObject.GetComponent<Container>().GetInventory()).Field("m_height")
                    .SetValue(height);


                var component = gameObject.GetComponent<TombStone>();
                var playerProfile = Game.instance.GetPlayerProfile();
                switch (inv_num)
                {
                    case 0:
                        component.Setup(playerProfile.GetName(), playerProfile.GetPlayerID());
                        break;
                    case 1:
                        component.Setup(playerProfile.GetName() + "- Quickslots", playerProfile.GetPlayerID());
                        foreach (var item in dropItems)
                        {
                            var oldSlot = item.m_gridPos;
                            item.m_customData["eaqs-qs"] = $"{oldSlot.x},{oldSlot.y}";
                            Debug(
                                $"   Quickslot Item  Name: {item.m_dropPrefab.name}   Cat: {item.m_shared.m_itemType}");
                        }

                        break;
                    case 2:
                        component.Setup(playerProfile.GetName() + "- Eqipment", playerProfile.GetPlayerID());
                        foreach (var item in dropItems)
                        {
                            item.m_customData["eaqs-e"] = "1";
                            Debug(
                                $"   Eqipment Item  Name: {item.m_dropPrefab.name}   Cat: {item.m_shared.m_itemType}");
                        }


                        break;
                }

                Traverse.Create(gameObject.GetComponent<Container>().GetInventory()).Field("m_inventory")
                    .SetValue(dropItems);
                Traverse.Create(gameObject.GetComponent<Container>().GetInventory()).Method("Changed").GetValue();
            } else
            {
                Debug("   !! dropItems.Any");


                foreach (var item in dropItems)
                {
                    Debug($"       Item : {item.m_dropPrefab.name}");

                    var position = __instance.transform.position + Vector3.up * 0.5f
                                                                 + Random.insideUnitSphere * 0.3f;
                    var rotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
                    ItemDrop.DropItem(item, 0, position, rotation);
                }
            }
        }

        if (!keepFoodLevels.Value)
            __instance.m_foods.Clear();

        var hardDeath = noSkillProtection.Value || __instance.m_timeSinceDeath > __instance.m_hardDeathCooldown;

        if (hardDeath && reduceSkills.Value) __instance.m_skills.LowerAllSkills(skillReduceFactor.Value);

        __instance.m_seman.RemoveAllStatusEffects();
        Game.instance.RequestRespawn(10f);
        __instance.m_timeSinceDeath = 0;

        if (!hardDeath) __instance.Message(MessageHud.MessageType.TopLeft, "$msg_softdeath");

        __instance.Message(MessageHud.MessageType.Center, "$msg_youdied");
        __instance.ShowTutorial("death");
        Minimap.instance.AddPin(__instance.transform.position, Minimap.PinType.Death,
            $"$hud_mapday {EnvMan.instance.GetDay(ZNet.instance.GetTimeSeconds())}", true, false);

        if (__instance.m_onDeath != null) __instance.m_onDeath();

        var eventLabel = "biome:" + __instance.GetCurrentBiome();
        Gogan.LogEvent("Game", "Death", eventLabel, 0L);

        return false;
    }
}