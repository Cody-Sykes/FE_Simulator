namespace FireEmblemCombat
{
    // Enum for weapon types
    public enum WeaponType
    {
        Sword,
        Lance,
        Axe,
        Bow,
        AnimaMagic, // Basic magic type
        // LightMagic, // Could expand later
        // DarkMagic,  // Could expand later
        Staff // Typically for healing, but can be offensive
    }

    // Represents a weapon
    public class Weap
    {
        public string Name { get; }
        public WeaponType Type { get; }
        public int Might { get; } // Base damage
        public int Hit { get; }   // Base accuracy
        public int Crit { get; }  // Base critical chance
        public int RangeMin { get; }
        public int RangeMax { get; }
        public bool IsMagic { get; } // True if it targets Resistance, false for Defense
        public int Weight { get; } // Weapon weight, affects attack speed

        public Weap(string name, WeaponType type, int might, int hit, int crit, int rangeMin, int rangeMax, bool isMagic, int weight)
        {
            Name = name;
            Type = type;
            Might = might;
            Hit = hit;
            Crit = crit;
            RangeMin = rangeMin;
            RangeMax = rangeMax;
            IsMagic = isMagic;
            Weight = weight;
        }

        public override string ToString()
        {
            return $"{Name} ({Type}) - Mt:{Might}, Hit:{Hit}, Crit:{Crit}, Rng:{RangeMin}-{RangeMax}, Wt:{Weight}{(IsMagic ? ", Magic" : "")}";
        }
    }
}
