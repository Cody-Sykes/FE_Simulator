using AIVision_OCR_Tests;

namespace FireEmblemCombat
{
    public class Unit
    {
        public string Name { get; }
        public int MaxHP { get; private set; }
        public int CurrentHP { get; set; }
        public int Strength { get; private set; }    // For physical damage
        public int Magic { get; private set; }       // For magical damage
        public int Skill { get; private set; }
        public int Speed { get; private set; }
        public int Luck { get; private set; }
        public int Defense { get; private set; }
        public int Resistance { get; private set; }
        public Weap EquippedWeapon { get; set; }
        public bool IsPlayerUnit { get; } // To distinguish player vs enemy

        public bool Moved = false; // Track if the unit has moved this turn
        public bool IsAlive => CurrentHP > 0;
        public int Level { get; set; } = 1;
        public int Experience { get; set; } = 0; // For leveling up, if needed
        public int ExpDrop { get; set; } = 100; // Experience dropped on death, if applicable
        public int MoveRange { get; set; }
        // Calculated stats
        public int AttackPower => EquippedWeapon.IsMagic ? Magic + EquippedWeapon.Might : Strength + EquippedWeapon.Might;
        public int AttackSpeed => Speed - EquippedWeapon.Weight < 0 ? 0 : Speed - EquippedWeapon.Weight; // Cannot be negative
        public int HitRate => EquippedWeapon.Hit + (Skill * 2) + (Luck / 2); // Base hit before weapon triangle or enemy avoid
        public int AvoidRate => (AttackSpeed * 2) + Luck;
        public int CritRate => (Skill / 2) + EquippedWeapon.Crit; // Base crit before enemy luck 
        public int CritEvade => Luck;
        public List<IItem> HeldItems { get; set; } = new List<IItem>();
        private Inventory inventory;

        public Unit(string name, int maxHP, int strength, int magic, int skill, int speed, int luck, int defense, int resistance, int moveRange, Weap equippedWeapon, bool isPlayerUnit, List<IItem> items = null)
        {
            Name = name;
            MaxHP = maxHP;
            CurrentHP = maxHP;
            Strength = strength;
            Magic = magic;
            Skill = skill;
            Speed = speed;
            Luck = luck;
            Defense = defense;
            Resistance = resistance;
            EquippedWeapon = equippedWeapon;
            IsPlayerUnit = isPlayerUnit;
            HeldItems = items ?? new List<IItem>();
            MoveRange = moveRange;
        }

        public void TakeDamage(int amount)
        {
            CurrentHP -= amount;
            if (CurrentHP < 0)
            {
                CurrentHP = 0;
            }
        }

        public void Heal(int amount)
        {
            CurrentHP += amount;
            if (CurrentHP > MaxHP)
            {
                CurrentHP = MaxHP;
            }
        }

        public void DisplayStats()
        {
            Console.WriteLine($"--- NAME: {Name} ({(IsPlayerUnit ? "Player" : "Enemy")}) ---");
            Console.WriteLine($"\n| HP: {CurrentHP}/{MaxHP} \n| Str: {Strength} \n| Mag: {Magic} \n| Skl: {Skill} \n| Spd: {Speed} \n| Lck: {Luck} \n| Def: {Defense} \n| Res: {Resistance}");
            Console.WriteLine($"| AtkPwr: {AttackPower} \n| AtkSpd: {AttackSpeed} \n| Hit: {HitRate} \n| Avoid: {AvoidRate} \n| Crit: {CritRate} \n| CritEvade: {CritEvade}");
            Console.WriteLine($"| Weapon: {EquippedWeapon}");
            Console.WriteLine("--------------------");
        }

        public void GainExperience(int amount)
        {
            Experience += amount;
            if (Experience >= 100) // Simple level up condition
            {
                LevelUp();
                Experience -= 100; // Reset experience after leveling up
            }
        }

        private void LevelUp()
        {
            Level++;
            MaxHP += 2; // Example growth
            Strength += 2;
            Magic += 1;
            Skill += 1;
            Speed += 2;
            Luck += 1;
            Defense += 1;
            Resistance += 1;
            // Reset current HP to max after level up
            CurrentHP = MaxHP;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{Name} leveled up! Now at Level {Level}.");
            Console.ResetColor();
        }

        public void AccessInventory()
        {
            if (inventory == null)
            {
                inventory = new Inventory(this);
            }

            if (inventory.DisplayItems())
            {
                try
                {
                    inventory.selectItem(Convert.ToInt32(Console.ReadLine()));
                }
                catch
                {
                    Console.Clear();
                    Console.WriteLine("Invalid item selection. Returning to action selection.");
                }
            }
        }
    }
}
