namespace DeathTweaks;

public class InventoryInfos
{
    public readonly List<ItemDrop.ItemData> drop_list;
    public readonly Inventory inventory;

    public InventoryInfos(Inventory i, List<ItemDrop.ItemData> d)
    {
        drop_list = d;
        inventory = i;
    }
}