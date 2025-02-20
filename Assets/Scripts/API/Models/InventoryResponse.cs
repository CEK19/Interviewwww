using System.Collections.Generic;

public class Item
{
    public string ItemId { get; set; }
    public string DisplayName { get; set; }
}

public class InventoryResponse
{
    public List<Item> Inventory { get; set; }
}