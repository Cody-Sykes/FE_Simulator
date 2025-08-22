namespace FireEmblemCombat
{
    public static class CO_Service
    {
        private static Random _random = new Random();
        private const int WEAPON_TRIANGLE_HIT_BONUS = 15;
        private const int WEAPON_TRIANGLE_MIGHT_BONUS = 1;
        private const int SPEED_THRESHOLD_FOR_DOUBLE_ATTACK = 4;

        // Determines weapon triangle advantage
        // Returns: (HitBonus, MightBonus)
        public static void OpenCombatMenu(Unit selectedPlayerUnit, List<Unit> enemyUnits)
        {
            bool validOption = false;
            Unit selectedEnemyUnit = ChooseUnit(enemyUnits, $"Choose an enemy to attack with {selectedPlayerUnit.Name}:");
            
            while (!validOption)
            {
                Console.WriteLine("What would you like to do?\n[A]ttack, [S]tats (Enemy), [R]etreat");
                switch (Console.ReadLine().ToUpper())
                {
                    case "A":
                        Console.Clear();
                        int distance = GetDistance(selectedPlayerUnit, selectedEnemyUnit); // Simplified distance for console
                        CO_Service.InitiateCombat(selectedPlayerUnit, selectedEnemyUnit, distance);
                        selectedPlayerUnit.Moved = true;
                        validOption = true;
                        break;
                    case "S":
                        Console.Clear();
                        selectedEnemyUnit.DisplayStats();
                        Console.ReadLine();
                        Console.Clear();
                        continue;
                    case "R":
                        Console.Clear();
                        Console.WriteLine($"{selectedPlayerUnit.Name} retreated from combat.");
                        validOption = true; // Exit attack loop
                        break;
                    default:
                        Console.Clear();
                        Console.WriteLine("Invalid choice. Returning to action selection.");
                        Console.ReadLine();
                        Console.Clear();
                        break; // Redo player choice for action
                }
            }
        }

        public static (int, int) GetWeaponTriangleBonus(WeaponType attackerWeapon, WeaponType defenderWeapon)
        {
            if ((attackerWeapon == WeaponType.Sword && defenderWeapon == WeaponType.Axe) ||
                (attackerWeapon == WeaponType.Axe && defenderWeapon == WeaponType.Lance) ||
                (attackerWeapon == WeaponType.Lance && defenderWeapon == WeaponType.Sword))
            {
                return (WEAPON_TRIANGLE_HIT_BONUS, WEAPON_TRIANGLE_MIGHT_BONUS); // Advantage
            }
            if ((attackerWeapon == WeaponType.Axe && defenderWeapon == WeaponType.Sword) ||
                (attackerWeapon == WeaponType.Lance && defenderWeapon == WeaponType.Axe) ||
                (attackerWeapon == WeaponType.Sword && defenderWeapon == WeaponType.Lance))
            {
                return (-WEAPON_TRIANGLE_HIT_BONUS, -WEAPON_TRIANGLE_MIGHT_BONUS); // Disadvantage
            }
            return (0, 0); // Neutral
        }

        public static CombatForecast GetCombatForecast(Unit attacker, Unit defender, int distance)
        {
            if (attacker.EquippedWeapon.RangeMin > distance || attacker.EquippedWeapon.RangeMax < distance)
            {
                // Attacker cannot attack at this distance
                return new CombatForecast(attacker, defender, 0, 0, 0, 0, 0, 0, false, false, false, false, "Attacker out of range");
            }

            var (atkTriangleHit, atkTriangleMight) = GetWeaponTriangleBonus(attacker.EquippedWeapon.Type, defender.EquippedWeapon.Type);

            int attackerDamage = (attacker.AttackPower + atkTriangleMight) - (attacker.EquippedWeapon.IsMagic ? defender.Resistance : defender.Defense);
            if (attackerDamage < 0) attackerDamage = 0;

            int attackerHitChance = attacker.HitRate + atkTriangleHit - defender.AvoidRate;
            attackerHitChance = Math.Clamp(attackerHitChance, 0, 100);

            int attackerCritChance = attacker.CritRate - defender.CritEvade;
            attackerCritChance = Math.Clamp(attackerCritChance, 0, 100);

            bool attackerDoubles = attacker.AttackSpeed - defender.AttackSpeed >= SPEED_THRESHOLD_FOR_DOUBLE_ATTACK;

            // Defender's forecast (for counter-attack)
            int defenderDamage = 0;
            int defenderHitChance = 0;
            int defenderCritChance = 0;
            bool defenderCanCounter = false;
            bool defenderDoubles = false;
            string defenderStatus = "Cannot counter (out of range or dead)";

            if (defender.IsAlive && defender.EquippedWeapon.RangeMin <= distance && defender.EquippedWeapon.RangeMax >= distance)
            {
                defenderCanCounter = true;
                var (defTriangleHit, defTriangleMight) = GetWeaponTriangleBonus(defender.EquippedWeapon.Type, attacker.EquippedWeapon.Type);

                defenderDamage = (defender.AttackPower + defTriangleMight) - (defender.EquippedWeapon.IsMagic ? attacker.Resistance : attacker.Defense);
                if (defenderDamage < 0) defenderDamage = 0;

                defenderHitChance = defender.HitRate + defTriangleHit - attacker.AvoidRate;
                defenderHitChance = Math.Clamp(defenderHitChance, 0, 100);

                defenderCritChance = defender.CritRate - attacker.CritEvade;
                defenderCritChance = Math.Clamp(defenderCritChance, 0, 100);

                defenderDoubles = defender.AttackSpeed - attacker.AttackSpeed >= SPEED_THRESHOLD_FOR_DOUBLE_ATTACK;
                defenderStatus = defenderCanCounter ? "Can counter" : defenderStatus;
            }


            return new CombatForecast(
                attacker, defender,
                attackerDamage, attackerHitChance, attackerCritChance,
                defenderDamage, defenderHitChance, defenderCritChance,
                attackerDoubles, defenderDoubles, defenderCanCounter, true, defenderStatus
            );
        }

        // Simulates a single attack instance
        private static AttackResult PerformAttack(Unit attacker, Unit defender, int baseDamage, int hitChance, int critChance, string attackContext)
        {
            Console.WriteLine($"\n{attackContext} {attacker.Name} attacks {defender.Name} with {attacker.EquippedWeapon.Name}!");
            if (_random.Next(1, 101) <= hitChance)
            {
                bool isCrit = _random.Next(1, 101) <= critChance;
                int actualDamage = isCrit ? baseDamage * 3 : baseDamage; // Crits deal 3x damage

                defender.TakeDamage(actualDamage);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{(isCrit ? "CRITICAL HIT! " : "HIT! ")}{attacker.Name} deals {actualDamage} damage to {defender.Name}. {defender.Name} HP: {defender.CurrentHP}/{defender.MaxHP}");
                Console.ResetColor();
                return new AttackResult(true, isCrit, actualDamage);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"MISS! {attacker.Name}'s attack missed {defender.Name}.");
                Console.ResetColor();
                return new AttackResult(false, false, 0);
            }
        }

        public static void InitiateCombat(Unit attacker, Unit defender, int distance)
        {
            Console.WriteLine($"\n--- COMBAT START: {attacker.Name} vs {defender.Name} (Distance: {distance}) ---");
            attacker.DisplayStats();
            defender.DisplayStats();

            CombatForecast forecast = GetCombatForecast(attacker, defender, distance);
            forecast.Display();

            if (!forecast.AttackerCanInitiate)
            {
                Console.WriteLine($"{attacker.Name} cannot initiate combat: {forecast.DefenderStatus}");
                return;
            }

            Console.WriteLine("\nPress Enter to proceed with combat...");
            Console.ReadLine();
            Console.Clear();

            // Attacker's first strike
            if (attacker.IsAlive)
            {
                PerformAttack(attacker, defender, forecast.AttackerDamage, forecast.AttackerHitChance, forecast.AttackerCritChance, "Attacker's Strike:");
            }

            // Defender's counter-attack
            if (defender.IsAlive && forecast.DefenderCanCounter)
            {
                PerformAttack(defender, attacker, forecast.DefenderDamage, forecast.DefenderHitChance, forecast.DefenderCritChance, "Defender's Counter:");
            }
            else if (defender.IsAlive && !forecast.DefenderCanCounter)
            {
                Console.WriteLine($"{defender.Name} cannot counter-attack ({forecast.DefenderStatus}).");
            }


            // Attacker's follow-up (if doubles)
            if (attacker.IsAlive && defender.IsAlive && forecast.AttackerDoubles)
            {
                Console.WriteLine($"{attacker.Name} gets a follow-up attack!");
                PerformAttack(attacker, defender, forecast.AttackerDamage, forecast.AttackerHitChance, forecast.AttackerCritChance, "Attacker's Follow-up:");
            }

            // Defender's follow-up (if doubles and countered)
            if (defender.IsAlive && attacker.IsAlive && forecast.DefenderCanCounter && forecast.DefenderDoubles)
            {
                Console.WriteLine($"{defender.Name} gets a follow-up attack!");
                PerformAttack(defender, attacker, forecast.DefenderDamage, forecast.DefenderHitChance, forecast.DefenderCritChance, "Defender's Follow-up:");
            }

            Console.WriteLine("\n--- COMBAT END ---");
            if (!attacker.IsAlive)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{attacker.Name} has been defeated!");
                Console.ResetColor();
                if (defender.IsPlayerUnit)
                {
                     defender.GainExperience(attacker.ExpDrop);
                     defender.DisplayStats();
                }
            }
            if (!defender.IsAlive)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{defender.Name} has been defeated!");
                Console.ResetColor();
                if (attacker.IsPlayerUnit)
                {
                    attacker.GainExperience(defender.ExpDrop);
                    attacker.DisplayStats();
                }
            }
            Console.ReadLine();
            Console.Clear();
        }

        static int GetDistance(Unit unit1, Unit unit2)
        {
            Console.Write($"Specify distance between {unit1.Name} and {unit2.Name} (e.g., 1 or 2): ");
            int distance;
            while (!int.TryParse(Console.ReadLine(), out distance) || distance < 1 || distance > 2)
            {
                Console.Write("Invalid distance. Please enter a number (e.g., 1 or 2): ");
            }
            return distance;
        }

        public static Unit ChooseUnit(List<Unit> availableUnits, string prompt)
        {
            List<Unit> aliveUnits = availableUnits.Where(u => u.IsAlive).ToList();
            if (!aliveUnits.Any())
            {
                Console.WriteLine("No units available in this list.");
                return null;
            }

            Console.WriteLine("\n" + prompt);
            for (int i = 0; i < aliveUnits.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {aliveUnits[i].Name} (HP: {aliveUnits[i].CurrentHP}/{aliveUnits[i].MaxHP}) {(aliveUnits[i].Moved ? "-- [ALREADY MOVED]" : "")}");
            }

            int choice = -1;
            while (choice < 1 || choice > aliveUnits.Count)
            {
                Console.Write($"Enter number (1-{aliveUnits.Count}): ");
                if (!int.TryParse(Console.ReadLine(), out choice))
                {
                    choice = -1; // Invalid input
                }
                if(choice < 1 || choice > aliveUnits.Count)
                {
                    Console.WriteLine($"Please enter a valid number between 1 and {aliveUnits.Count}.");
                    continue;
                }
                if (aliveUnits[choice - 1].Moved)
                {
                    Console.WriteLine("This unit has already moved this turn. Please choose another unit.");
                    choice = -1; // Reset choice if already moved
                }
            }
            return aliveUnits[choice - 1];
        }
    }

    public struct CombatForecast
    {
        public Unit Attacker { get; }
        public Unit Defender { get; }
        public int AttackerDamage { get; }
        public int AttackerHitChance { get; }
        public int AttackerCritChance { get; }
        public int DefenderDamage { get; }
        public int DefenderHitChance { get; }
        public int DefenderCritChance { get; }
        public bool AttackerDoubles { get; }
        public bool DefenderDoubles { get; }
        public bool DefenderCanCounter { get; }
        public bool AttackerCanInitiate { get; }
        public string DefenderStatus { get; }


        public CombatForecast(Unit attacker, Unit defender,
                              int attackerDamage, int attackerHitChance, int attackerCritChance,
                              int defenderDamage, int defenderHitChance, int defenderCritChance,
                              bool attackerDoubles, bool defenderDoubles, bool defenderCanCounter, bool attackerCanInitiate, string defenderStatus)
        {
            Attacker = attacker;
            Defender = defender;
            AttackerDamage = attackerDamage;
            AttackerHitChance = attackerHitChance;
            AttackerCritChance = attackerCritChance;
            DefenderDamage = defenderDamage;
            DefenderHitChance = defenderHitChance;
            DefenderCritChance = defenderCritChance;
            AttackerDoubles = attackerDoubles;
            DefenderDoubles = defenderDoubles;
            DefenderCanCounter = defenderCanCounter;
            AttackerCanInitiate = attackerCanInitiate;
            DefenderStatus = defenderStatus;
        }

        public void Display()
        {
            Console.WriteLine("\n--- COMBAT FORECAST ---");
            if (!AttackerCanInitiate)
            {
                Console.WriteLine($"{Attacker.Name} cannot attack: {DefenderStatus}");
                return;
            }
            Console.WriteLine($"{Attacker.Name} (Attacker) vs {Defender.Name} (Defender)");
            Console.WriteLine($"  {Attacker.Name}: Dmg: {AttackerDamage}, Hit: {AttackerHitChance}%, Crit: {AttackerCritChance}%{(AttackerDoubles ? " (x2)" : "")}");

            if (Defender.IsAlive && DefenderCanCounter)
            {
                Console.WriteLine($"  {Defender.Name}: Dmg: {DefenderDamage}, Hit: {DefenderHitChance}%, Crit: {DefenderCritChance}%{(DefenderDoubles ? " (x2)" : "")}");
            }
            else if (Defender.IsAlive)
            {
                Console.WriteLine($"  {Defender.Name}: {DefenderStatus}");
            }
            else
            {
                Console.WriteLine($"  {Defender.Name}: Defeated");
            }
            Console.WriteLine("-----------------------");
        }
    }

    public struct AttackResult
    {
        public bool Hit { get; }
        public bool Crit { get; }
        public int DamageDealt { get; }

        public AttackResult(bool hit, bool crit, int damageDealt)
        {
            Hit = hit;
            Crit = crit;
            DamageDealt = damageDealt;
        }
    }
}
