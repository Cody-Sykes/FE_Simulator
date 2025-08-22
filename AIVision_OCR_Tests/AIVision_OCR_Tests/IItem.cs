using System.Numerics;
using FireEmblemCombat;

namespace AIVision_OCR_Tests
{
    //Interface for items
    public interface IItem
    {
        public string Name { get; set; }
        public uint Qty { get; set; }
        public abstract void Use(Unit unit);
    }

    //Concrete classes for different item types
    public class HealingItem : IItem
    {
        Dictionary<string, int> healingItems = new Dictionary<string, int>()
        {
            { "Health Potion", 10 },
            { "Elixir", 30 },
            { "Water", 5 }
        };
        public string Name { get; set; }
        public uint Qty { get; set; }
        
        public HealingItem(string name, uint qty = 1)
        {
            Name = name;
            Qty = qty;
        }

        public void Use(Unit unit)
        {
            if (healingItems.ContainsKey(Name))
            {
                unit.CurrentHP += healingItems[Name];

                // Ensure HP does not exceed max
                if (unit.CurrentHP > unit.MaxHP)
                    unit.CurrentHP = unit.MaxHP; 

                Console.WriteLine($"Using {Name}, {unit.Name} healed {healingItems[Name]} HP. {unit.CurrentHP}/{unit.MaxHP}");

                Qty--;
            }
            else
            {
                Console.WriteLine($"{Name} is not a valid healing item.");
            }
        }
    }
}
