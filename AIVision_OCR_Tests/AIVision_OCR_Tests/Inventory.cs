using FireEmblemCombat;

namespace AIVision_OCR_Tests
{
    public class Inventory
    {
        private List<IItem> inventoryItems = new List<IItem>(5);
        private Unit unit;
        public Inventory(Unit unit) 
        {
            this.unit = unit;
            this.inventoryItems = unit.HeldItems;
        }

        //Add item to inventory
        public void AddItem(IItem item)
        {
            if(inventoryItems.Count >= 5)
            {
                Console.WriteLine("Inventory full, cannot add more items.");
                return;
            }

            inventoryItems.Add(item);
        }

        //Display items in inventory, return false if empty
        public bool DisplayItems()
        {
            if (inventoryItems.Count < 1)
            {
                Console.WriteLine("No items in bag.");
                return false;
            }
            
            int count = 1;
            Console.WriteLine("Select an item from the list to use, or [0] to exit:\n------------------------------------");
            foreach(IItem item in inventoryItems)
            {
                Console.WriteLine($"{count}: {item.Name}    [{item.Qty}]");
                count++;
            }
            return true;
        }

        //Choose and use an item from the list
        public void selectItem(int index)
        {
            Console.Clear();
            if (index < 0 || index > inventoryItems.Count)
            {
                Console.WriteLine("Invalid item selection.");
                return;
            }
            else if (index == 0) return;

            IItem selectedItem = inventoryItems[index - 1];
            selectedItem.Use(unit);

            if (selectedItem.Qty == 0)
            {
                inventoryItems.RemoveAt(index - 1);
                Console.WriteLine($"All available {selectedItem.Name}'s have been used.");
                Console.Clear();
            }
        }
    }
}
