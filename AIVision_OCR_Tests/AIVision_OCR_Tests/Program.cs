using AIVision_OCR_Tests;

namespace FireEmblemCombat
{
    class Program
    {
        static List<Unit> playerUnits = new List<Unit>();
        static List<Unit> enemyUnits = new List<Unit>();

        static Dictionary<string, string> grid = new Dictionary<string, string>() {
            { "A", ".........." },
            { "B", ".........." },
            { "C", ".........." },
            { "D", ".........." },
            { "E", ".........." }
        };

        static void Main(string[] args)
        {
            char c = 'C';
            Console.WriteLine($"Character: {((char)(c + 2))}");
            InitializeUnits();

            PopulateGrid();
            Console.ReadLine();

            GameLoop();

            Console.WriteLine("\nGame Over. Press any key to exit.");
            Console.ReadKey();
        }
        
        static void InitializeUnits()
        {
            // Define some weapons
            Weap ironSword = new Weap("Iron Sword", WeaponType.Sword, 5, 90, 0, 1, 1, false, 5);
            Weap steelSword = new Weap("Steel Sword", WeaponType.Sword, 8, 80, 0, 1, 1, false, 10);
            Weap ironLance = new Weap("Iron Lance", WeaponType.Lance, 7, 80, 0, 1, 1, false, 8);
            Weap javelin = new Weap("Javelin", WeaponType.Lance, 6, 70, 0, 1, 2, false, 11); // 1-2 range
            Weap ironAxe = new Weap("Iron Axe", WeaponType.Axe, 8, 70, 0, 1, 1, false, 10);
            Weap handAxe = new Weap("Hand Axe", WeaponType.Axe, 7, 60, 0, 1, 2, false, 12); // 1-2 range
            Weap ironBow = new Weap("Iron Bow", WeaponType.Bow, 6, 85, 5, 2, 2, false, 6); // Bows are 2-range
            Weap fireTome = new Weap("Fire Tome", WeaponType.AnimaMagic, 6, 90, 0, 1, 2, true, 4);
            Weap starSword = new Weap("Star Sword", WeaponType.Sword, 15, 95, 1, 1, 2, true, 7);
            Weap demonSword = new Weap("Demon Sword", WeaponType.Sword, 18, 95, 1, 1, 1, false, 9);

            // Player Units
            playerUnits.Add(new Unit("Cade", 30, 11, 5, 7, 8, 7, 6, 4, 2, ironSword, true, new List<IItem>() { new HealingItem("Health Potion", 2), new HealingItem("Elixir") }));
            playerUnits.Add(new Unit("Archer", 35, 10, 3, 6, 6, 5, 9, 2, 2, ironAxe, true, new List<IItem>() { new HealingItem("Elixir") }));
            playerUnits.Add(new Unit("Faust", 28, 7, 4, 9, 10, 8, 4, 5, 2, steelSword, true, new List<IItem>() { new HealingItem("Water") }));
            playerUnits.Add(new Unit("Oswin", 40, 12, 2, 5, 4, 4, 12, 3, 2, ironLance, true, new List<IItem>() { new HealingItem("Health Potion", 3) }));
            playerUnits.Add(new Unit("Ariana", 22, 3, 8, 6, 7, 9, 3, 7, 2, fireTome, true, new List<IItem>() { new HealingItem("Water") }));
            playerUnits.Add(new Unit("Odain", 50, 15, 15, 15, 15, 7, 15, 15,2, starSword, true, new List<IItem>() { new HealingItem("Elixir", 2) }));

            // Enemy Units
            enemyUnits.Add(new Unit("Bandit (Axe)", 25, 7, 1, 4, 5, 2, 5, 1, 2, ironAxe, false));
            enemyUnits.Add(new Unit("Brigand (Lance)", 28, 6, 2, 5, 4, 3, 6, 2, 2, ironLance, false));
            enemyUnits.Add(new Unit("Archer (Bow)", 22, 5, 2, 6, 6, 4, 3, 2, 2, ironBow, false));
            enemyUnits.Add(new Unit("Mage (Fire)", 20, 2, 7, 5, 5, 3, 2, 6, 2, fireTome, false));
            enemyUnits.Add(new Unit("Knight (Sword)", 38, 9, 1, 3, 2, 1, 11, 1, 2, steelSword, false));
            enemyUnits.Add(new Unit("Ovain", 50, 15, 15, 15, 15, 7, 15, 15, 2, demonSword, false));
        }

        static void GameLoop()
        {
            bool gameRunning = true;
            while (gameRunning)
            {
                //Console.Clear();

                // Check win/loss conditions
                if (!playerUnits.Any(u => u.IsAlive))
                {
                    Console.WriteLine("All player units defeated! GAME OVER.");
                    gameRunning = false;
                    break;
                }
                if (!enemyUnits.Any(u => u.IsAlive))
                {
                    Console.WriteLine("All enemy units defeated! VICTORY!");
                    gameRunning = false;
                    break;
                }

                // Player Phase
                #region p phase
                Console.WriteLine("\n--- PLAYER PHASE ---");
                foreach (var unit in playerUnits.Where(u => u.IsAlive))
                {
                    DisplayAllUnits();

                    bool chooseDifferentUnit = false;
                    Unit selectedPlayerUnit = CO_Service.ChooseUnit(playerUnits, "Choose your unit to act:");

                    if (selectedPlayerUnit == null) continue; // Should not happen with current logic
                    while (selectedPlayerUnit.Moved == false && chooseDifferentUnit == false)
                    {
                        Console.WriteLine($"\n{selectedPlayerUnit.Name} selected.");
                        Console.WriteLine("Actions: [A]ttack, [S]tats, [B]ag, [W]ait, [C]hoose Different Unit");
                        string action = Console.ReadLine().ToUpper();
                        Console.Clear();
                        if (action == "A")
                        {
                            CO_Service.OpenCombatMenu(selectedPlayerUnit, enemyUnits);
                            Console.Clear();
                        }
                        else if (action == "S")
                        {
                            Console.Clear();
                            selectedPlayerUnit.DisplayStats();
                            Console.ReadLine(); // Pause
                            Console.Clear();
                        }
                        else if (action == "W")
                        {
                            Console.Clear();
                            Console.WriteLine($"{selectedPlayerUnit.Name} waits.");
                            selectedPlayerUnit.Moved = true; // Mark as moved
                        }
                        else if (action == "C")
                        {
                            Console.Clear();
                            chooseDifferentUnit = true;
                        }
                        else if (action == "B")
                        {
                            Console.Clear();
                            Console.WriteLine($"{selectedPlayerUnit.Name} opens their bag.");
                            selectedPlayerUnit.AccessInventory();
                            continue;
                        }
                        else if(action == "M")
                        {
                            Console.Clear();
                            MoveUnit(selectedPlayerUnit);
                            selectedPlayerUnit.Moved = true; // Mark as moved
                            Console.ReadLine(); // Pause to see move
                        }
                        else
                        {
                            Console.Clear();
                            Console.WriteLine("Invalid action.");
                            continue; // Redo player choice for action
                        }
                    }
                }
                #endregion
                // Remove defeated units
                playerUnits.RemoveAll(u => !u.IsAlive);
                enemyUnits.RemoveAll(u => !u.IsAlive);

                Console.WriteLine("\nEnd of Turn. Press Enter to continue to the next turn...");
                foreach (var unit in playerUnits)
                {
                    unit.Moved = false; // Reset moved status for next turn
                }
                Console.ReadLine();
                Console.Clear();
            }
        }

        static void DisplayAllUnits()
        {
            Console.WriteLine("\n-- Player Units --");
            foreach (var unit in playerUnits.Where(u => u.IsAlive))
            {
                Console.WriteLine($"- {unit.Name} (HP: {unit.CurrentHP}/{unit.MaxHP}, Wpn: {unit.EquippedWeapon.Name})");
            }

            Console.WriteLine("\n-- Enemy Units --");
            foreach (var unit in enemyUnits.Where(u => u.IsAlive))
            {
                Console.WriteLine($"- {unit.Name} (HP: {unit.CurrentHP}/{unit.MaxHP}, Wpn: {unit.EquippedWeapon.Name})");
            }
        }

        static void PopulateGrid()
        {
            Random rand = new Random();
            bool validPos;
            // Populate the grid with player units
            foreach (Unit unit in playerUnits)
            {
                validPos = false;
                while (!validPos)
                {
                    string row = rand.Next(2) == 1 ? "A" : "B";
                    int column = rand.Next(10);

                    if (grid[row][column] == '.')
                    {
                        string currentRow = grid[row];
                        currentRow = currentRow.Remove(column, 1).Insert(column, unit.Name.Substring(0, 2));
                        grid[row] = currentRow;
                        validPos = true;
                    }
                }
            }

            foreach (Unit unit in enemyUnits)
            {
                validPos = false;
                while (!validPos)
                {
                    string row = rand.Next(2) == 1 ? "D" : "E";
                    int column = rand.Next(10);

                    if (grid[row][column] == '.')
                    {
                        string currentRow = grid[row];
                        currentRow = currentRow.Remove(column, 1).Insert(column, unit.Name.Substring(0, 2));
                        grid[row] = currentRow;
                        validPos = true;
                    }
                }
            }

            Console.WriteLine("Game Grid:");
            foreach (var row in grid)
            {
                Console.WriteLine($"{row.Key}: {row.Value}");
            }
        }

        static void MoveUnit(Unit unit)
        {
            foreach (var row in grid)
            {
                if(row.Value.Contains(unit.Name.Substring(0, 2)))
                {
                    Console.WriteLine($"{unit.Name} is ready to move");
                }
            }
        }
    }
}