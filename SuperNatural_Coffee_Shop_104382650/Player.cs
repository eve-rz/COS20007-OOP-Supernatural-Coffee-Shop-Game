using System.Collections.Generic;
using System.Linq;
using System;

namespace CoffeeShop
{
    public class Player
    {
        /// <summary>
        /// The order the player is currently focusing on or has most recently taken.
        /// </summary>
        private Order? _currentOrder;
        /// <summary>
        /// The item currently being held by the player (e.g., a prepared drink).
        /// </summary>
        private Item? _heldItem;
        /// <summary>
        /// The amount of money the player currently possesses.
        /// </summary>
        private float _money;
        /// <summary>
        /// The player's inventory, storing ingredients and their quantities.
        /// This field is read-only (the dictionary instance itself), but its contents can be modified.
        /// </summary>
        private readonly Dictionary<Ingredient, int> _inventory;
        /// <summary>
        /// The player's current sanity level, typically on a scale (e.g., 0-100).
        /// </summary>
        private float _sanityLevel;
        /// <summary>
        /// A list of ingredients currently selected by the player for the brewing process.
        /// </summary>
        private List<Ingredient> _selectedIngredientsForBrewing;
        /// <summary>
        /// A static random number generator shared for player-related randomization tasks.
        /// </summary>
        private static Random _random = new Random();
        /// <summary>
        /// The number of water jugs the player currently has, used for cooling the brewing station.
        /// </summary>
        private int _waterJugCount = 0;

        /// <summary>
        /// Gets or privately sets the player's current active <see cref="Order"/>.
        /// This might be the order they are preparing or have most recently interacted with.
        /// </summary>
        public Order? CurrentOrder
        {
            get => _currentOrder;
            private set => _currentOrder = value;
        }

        /// <summary>
        /// Gets or privately sets the <see cref="Item"/> currently held by the player.
        /// This could be a drink they've prepared or another interactive item.
        /// </summary>
        public Item? HeldItem
        {
            get => _heldItem;
            private set => _heldItem = value;
        }

        /// <summary>
        /// Gets or privately sets the amount of money the player has.
        /// </summary>
        public float Money
        {
            get => _money;
            private set => _money = value;
        }

        /// <summary>
        /// Gets the player's inventory, which is a dictionary mapping <see cref="Ingredient"/>s to their quantities.
        /// Note: This directly exposes the internal dictionary, allowing external modification of its contents.
        /// </summary>
        public Dictionary<Ingredient, int> Inventory => _inventory;

        /// <summary>
        /// Gets or privately sets the player's current sanity level.
        /// Sanity can affect gameplay and is typically clamped between 0 and 100.
        /// </summary>
        public float SanityLevel
        {
            get => _sanityLevel;
            private set => _sanityLevel = value;
        }

        /// <summary>
        /// Gets or sets the list of <see cref="Ingredient"/>s currently selected by the player for brewing a drink.
        /// This list is typically populated in a UI before attempting to brew.
        /// </summary>
        public List<Ingredient> SelectedIngredientsForBrewing
        {
            get => _selectedIngredientsForBrewing;
            set => _selectedIngredientsForBrewing = value; 
        }

        /// <summary>
        /// Gets the number of water jugs the player currently possesses.
        /// </summary>
        public int WaterJugCount => _waterJugCount;


        /// <summary>
        /// Initializes a new instance of the <see cref="Player"/> class.
        /// Sets up the initial inventory, selected ingredients list, money, and sanity level.
        /// </summary>
        public Player()
        {
            _inventory = new Dictionary<Ingredient, int>();
            _selectedIngredientsForBrewing = new List<Ingredient>();
            _money = 0f;         // Player starts with no money by default here.
            _sanityLevel = 100f; // Player starts with full sanity.
        }

        /// <summary>
        /// Adds a specified amount of water jugs to the player's possession.
        /// </summary>
        /// <param name="amount">The number of water jugs to add. Must be positive.</param>
        public void AddWaterJugs(int amount)
        {
            if (amount > 0)
            {
                _waterJugCount += amount;
                Console.WriteLine($"Player acquired {amount} water jug(s). Total: {_waterJugCount}");
            }
        }

        /// <summary>
        /// Checks if the player currently has at least one water jug.
        /// </summary>
        /// <returns><c>true</c> if the player has one or more water jugs; otherwise, <c>false</c>.</returns>
        public bool HasWaterJug()
        {
            return _waterJugCount > 0;
        }

        /// <summary>
        /// Consumes one water jug from the player's possession, if available.
        /// Typically used to cool down equipment.
        /// </summary>
        /// <returns><c>true</c> if a water jug was successfully consumed; otherwise, <c>false</c>.</returns>
        public bool ConsumeWaterJug()
        {
            if (HasWaterJug())
            {
                _waterJugCount--;
                Console.WriteLine($"Player consumed a water jug. Remaining: {_waterJugCount}");
                return true;
            }
            Console.WriteLine("Player tried to consume a water jug, but has none.");
            return false;
        }

        /// <summary>
        /// Sets the player's money to a specified amount.
        /// The amount will be clamped to be non-negative.
        /// </summary>
        /// <param name="amount">The new amount of money for the player. If negative, it will be set to 0.</param>
        public void SetMoney(float amount)
        {
            _money = amount >= 0 ? amount : 0;
            // Console.WriteLine($"Player money set to: {_money}"); 
        }

        /// <summary>
        /// Clears all items from the player's inventory.
        /// </summary>
        public void ClearInventory()
        {
            _inventory.Clear();
            Console.WriteLine("Player inventory cleared.");
        }

        /// <summary>
        /// Resets the player's state for a new game.
        /// This includes setting initial money and sanity, clearing inventory,
        /// current order, held item, selected ingredients, and water jugs.
        /// </summary>
        /// <param name="initialMoney">The starting amount of money for the new game.</param>
        /// <param name="initialSanity">The starting sanity level for the new game.</param>
        public void ResetPlayerForNewGame(float initialMoney, float initialSanity)
        {
            SetMoney(initialMoney);
            SetSanityLevel(initialSanity);
            ClearInventory();
            _currentOrder = null;
            _heldItem = null;
            _selectedIngredientsForBrewing.Clear();
            _waterJugCount = 0;
            Console.WriteLine($"Player reset for new game. Money: {_money}, Sanity: {SanityLevel}.");
        }

        /// <summary>
        /// Prepares the player for a new level.
        /// Resets sanity to an initial value for the level, and clears the current order,
        /// held item, and selected ingredients. Money, inventory, and water jugs persist between levels.
        /// </summary>
        /// <param name="initialSanity">The starting sanity level for the new level.</param>
        public void PreparePlayerForNewLevel(float initialSanity)
        {
            SetSanityLevel(initialSanity);
            _currentOrder = null;
            _heldItem = null;
            _selectedIngredientsForBrewing.Clear();
            Console.WriteLine($"Player prepared for new level. Sanity reset to: {SanityLevel}. Money, inventory, water jugs persist.");
        }

        /// <summary>
        /// Sets the player's currently active order.
        /// </summary>
        /// <param name="order">The <see cref="Order"/> to set as current, or <c>null</c> to clear the current order.</param>
        public void SetCurrentOrder(Order? order)
        {
            CurrentOrder = order;
        }

        /// <summary>
        /// Sets the item currently held by the player.
        /// </summary>
        /// <param name="item">The <see cref="Item"/> to be held, or <c>null</c> if the player is holding nothing.</param>
        public void SetHeldItem(Item? item)
        {
            HeldItem = item; 
        }

        /// <summary>
        /// Sets the player's sanity level, clamping it between 0 and 100.
        /// </summary>
        /// <param name="level">The new sanity level. Values outside [0, 100] will be clamped.</param>
        public void SetSanityLevel(float level)
        {
            float oldSanity = SanityLevel;
            if (level < 0) SanityLevel = 0; 
            else if (level > 100) SanityLevel = 100; 
            else SanityLevel = level; 
            // Console.WriteLine($"Player sanity changed from {oldSanity:F0} to {SanityLevel:F0}"); 
        }

        /// <summary>
        /// Attempts to take an order from a customer.
        /// Determines if the customer wants a standard recipe or if it's an alien customer with a property demand.
        /// Updates the player's and customer's current order if successful.
        /// </summary>
        /// <param name="customer">The <see cref="Customer"/> attempting to place an order.</param>
        /// <param name="availableRecipes">A list of <see cref="Recipe"/>s available for the customer to choose from.</param>
        /// <param name="currentLevelNumber">The current game level number, influencing recipe availability or alien demand generation.</param>
        /// <param name="gameManager">A reference to the <see cref="GameManager"/> for accessing game state like available ingredients.</param>
        /// <returns>The <see cref="Order"/> created, or <c>null</c> if an order could not be taken.</returns>
        public Order? TakeOrder(Customer customer, List<Recipe> availableRecipes, int currentLevelNumber, GameManager gameManager)
        {
            if (customer == null || customer.CurrentState != CustomerState.Ordering)
            {
                Console.WriteLine("[Player.TakeOrder] Invalid conditions: Customer null or not in Ordering state.");
                return null;
            }

            Order? newOrder = null;

            Recipe? desiredRecipe = customer.GetDrinkOrder(availableRecipes, currentLevelNumber);

            if (desiredRecipe != null)
            {
                newOrder = new Order(customer, desiredRecipe);
                Console.WriteLine($"Player taking order for RECIPE: {desiredRecipe.RecipeName} from {customer.CustomerID}.");
            }
            else if (customer is SupernaturalCustomer sc && sc.Type == SupernaturalCustomerType.Alien)
            {
                Console.WriteLine($"[Player.TakeOrder] Alien detected. Generating property demand for {customer.CustomerID}.");

                List<Ingredient> availableBrewingIngredients = new List<Ingredient>();

                if (gameManager.CurrentLevel != null)
                {
                    availableBrewingIngredients = gameManager.CurrentLevel.UnlockedIngredients
                        .Where(ing => !ing.IsSupernatural && gameManager.GloballyUnlockedIngredientTypes.Contains(ing.Type))
                        .ToList();
                }
                else
                {
                    Console.WriteLine("[Player.TakeOrder] ERROR: CurrentLevel is null. Cannot get ingredients for Alien demand.");
                    customer.DialogueSet.ClearLines();
                    customer.DialogueSet.AddLine("Alien Visitor", "System error... dimensional flux in ingredient database...");
                    customer.DialogueSet.Reset();
                    return null;
                }

                if (availableBrewingIngredients.Count == 0) {
                    Console.WriteLine("[Player.TakeOrder] No non-supernatural ingredients available to base Alien demand on. Cannot take order.");
                    customer.DialogueSet.ClearLines();
                    customer.DialogueSet.AddLine("Alien Visitor", "Your... 'pantry'... lacks the fundamental compounds I require. Disappointing.");
                    customer.DialogueSet.Reset();
                    return null;
                }

                int maxIngredientsForAlienDemand = 3;
                int numIngredientsToUse = _random.Next(1, Math.Min(maxIngredientsForAlienDemand, availableBrewingIngredients.Count) + 1);

                List<Ingredient> chosenBasisIngredients = availableBrewingIngredients.OrderBy(x => _random.Next()).Take(numIngredientsToUse).ToList();

                if (!chosenBasisIngredients.Any()){
                    Console.WriteLine("[Player.TakeOrder] Alien cannot make a demand, failed to select basis ingredients (should not happen if availableBrewingIngredients is populated).");
                    return null;
                }

                int demandBitterness = 0;
                int demandSweetness = 0;
                int demandSourness = 0;

                Console.Write("[Player.TakeOrder] Alien demand will be based on: ");
                foreach(var ing in chosenBasisIngredients)
                {
                    demandBitterness += ing.Bitterness;
                    demandSweetness += ing.Sweetness;
                    demandSourness += ing.Sourness;
                    Console.Write($"{ing.Name}(B{ing.Bitterness} Sw{ing.Sweetness} So{ing.Sourness}); ");
                }
                Console.WriteLine();

                demandBitterness = Math.Clamp(demandBitterness, 0, 16); 
                demandSweetness = Math.Clamp(demandSweetness, 0, 16);
                demandSourness = Math.Clamp(demandSourness, 0, 16);

                AlienPropertyDemand demand = new AlienPropertyDemand(demandBitterness, demandSweetness, demandSourness);
                newOrder = new Order(customer, null, demand); 
                Console.WriteLine($"Player taking order for GUARANTEED SOLVABLE ALIEN DEMAND: {demand.ToString()} from {customer.CustomerID}.");

                customer.DialogueSet.ClearLines();
                customer.DialogueSet.AddLine(
                    "Alien",
                    $"Bitterness: {demand.TargetBitterness}, Sweetness: {demand.TargetSweetness}, Sourness: {demand.TargetSourness}. "
                );
                customer.DialogueSet.Reset();
            }
            else
            {
                Console.WriteLine($"Player could not determine a recipe or special order for {customer.CustomerID}.");
                return null;
            }

            if (newOrder != null)
            {
                this.SetCurrentOrder(newOrder); 
                customer.SetOrder(newOrder);    
                return newOrder;
            }
            return null; 
        }

        /// <summary>
        /// Simulates the player moving to a target position.
        /// Currently, this method only logs the action to the console.
        /// </summary>
        /// <param name="targetPosition">The <see cref="Vector2D"/> representing the target coordinates.</param>
        public void MoveTo(Vector2D targetPosition)
        {
            System.Console.WriteLine($"Player moving to ({targetPosition.X}, {targetPosition.Y})");
        }

        /// <summary>
        /// Initiates an interaction between the player and a specified piece of equipment.
        /// Retrieves and logs the interaction context from the equipment.
        /// </summary>
        /// <param name="equipment">The <see cref="IInteractable"/> equipment to interact with.</param>
        public void InteractWithEquipment(IInteractable equipment)
        {
            if (equipment != null)
            {
                InteractionContext context = equipment.InitiateInteraction();
                System.Console.WriteLine($"Player interacting with: {equipment.InteractionName}. Status: {context.StatusMessage}");
            }
        }

        /// <summary>
        /// Attempts to prepare a drink using specified ingredients, a target recipe (optional), and a specific tool.
        /// Validates ingredients, checks equipment status, and then delegates the actual processing to the equipment.
        /// Updates player's held item and consumes ingredients if successful.
        /// </summary>
        /// <param name="targetRecipe">The <see cref="Recipe"/> the player is trying to make. Can be null for experimental brews or alien demands.</param>
        /// <param name="providedIngredients">A list of <see cref="Ingredient"/> instances selected by the player for brewing.</param>
        /// <param name="specificTool">The <see cref="Equipment"/> (tool) to be used for preparation.</param>
        /// <param name="gameManager">A reference to the <see cref="GameManager"/> for context like workstation status.</param>
        /// <returns>A <see cref="ProcessResult"/> indicating the outcome, including the prepared drink (if any) and a status message.</returns>
        public ProcessResult AttemptToPrepareDrink(Recipe? targetRecipe, List<Ingredient> providedIngredients, Equipment specificTool, GameManager gameManager)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            string targetRecipeNameForLog = targetRecipe?.RecipeName ?? (CurrentOrder?.AlienDemand.HasValue == true ? "Alien Property Demand" : "None");
            Console.WriteLine($"[PLAYER.AttemptToPrepareDrink] START. Target: '{targetRecipeNameForLog}'. Provided Ingredients: {string.Join(", ", providedIngredients?.Select(i => i?.Name ?? "NULL_ING") ?? new List<string>())}");
            Console.ResetColor();

            // Pre-checks before attempting to use equipment.
            if (gameManager.IsBrewingStationOverheated)
            {
                Console.WriteLine("[PLAYER.AttemptToPrepareDrink] Aborted: Workstation OVERHEATED!");
                return new ProcessResult(false, null, "Workstation is OVERHEATED! Must be cooled.");
            }

            if (targetRecipe == null && (providedIngredients == null || !providedIngredients.Any()))
            {
                Console.WriteLine("[PLAYER.AttemptToPrepareDrink] Aborted: No target recipe and no ingredients.");
                return new ProcessResult(false, null, "No target recipe and no ingredients for brewing.");
            }

            if (targetRecipe == null && providedIngredients != null && providedIngredients.Any())
            {
                Console.WriteLine("[PLAYER.AttemptToPrepareDrink] Brewing experimentally (no specific target recipe name).");
            }

            if (specificTool == null)
            {
                Console.WriteLine("[PLAYER.AttemptToPrepareDrink] Aborted: No specific tool provided.");
                return new ProcessResult(false, null, "No specific tool provided for brewing.");
            }
            if (specificTool.IsBusy)
            {
                Console.WriteLine($"[PLAYER.AttemptToPrepareDrink] Aborted: {specificTool.EquipmentName} is busy.");
                return new ProcessResult(false, null, $"{specificTool.EquipmentName} is currently busy.");
            }

            // Check if the correct equipment is being used for the target recipe.
            if (targetRecipe != null && specificTool.Type != targetRecipe.RequiredEquipmentType)
            {
                Console.WriteLine($"[PLAYER.AttemptToPrepareDrink] Aborted: Wrong equipment for '{targetRecipe.RecipeName}'.");
                return new ProcessResult(false, null, $"Wrong equipment. Recipe '{targetRecipe.RecipeName}' requires {targetRecipe.RequiredEquipmentType}, but tried to use {specificTool.Type}.");
            }

            var providedIngredientsMap = new Dictionary<Ingredient, int>();
            if (providedIngredients != null)
            {
                foreach (var ingredientInstance in providedIngredients)
                {
                    if (ingredientInstance == null) {
                        Console.WriteLine("[PLAYER.AttemptToPrepareDrink] Null ingredient in provided list. Skipping.");
                        continue;
                    }
                    if (providedIngredientsMap.ContainsKey(ingredientInstance))
                        providedIngredientsMap[ingredientInstance]++;
                    else
                        providedIngredientsMap[ingredientInstance] = 1;
                }
            }

            if (!providedIngredientsMap.Any())
            {
                Console.WriteLine("[PLAYER.AttemptToPrepareDrink] Aborted: No ingredients were actually selected for brewing.");
                return new ProcessResult(false, null, "No ingredients were actually selected for brewing.");
            }

            if (!HasIngredients(providedIngredientsMap, gameManager)) 
            {
                Console.WriteLine("[PLAYER.AttemptToPrepareDrink] Aborted: Inventory check failed.");
                return new ProcessResult(false, null, "Inventory check failed: You don't possess all the selected ingredients in your main inventory.");
            }

            bool ingredientsDoMatchTargetRecipeObject = false;
            if (targetRecipe != null)
            {
                Console.WriteLine($"[PLAYER.AttemptToPrepareDrink] Validating against TargetRecipe: {targetRecipe.RecipeName}");
                Console.WriteLine($"[PLAYER.AttemptToPrepareDrink] Target requires: [{string.Join(", ", targetRecipe.RequiredIngredients.Select(kv => $"{kv.Key.Name}x{kv.Value}"))}]");

                if (providedIngredientsMap.Count == targetRecipe.RequiredIngredients.Count)
                {
                    ingredientsDoMatchTargetRecipeObject = true;
                    foreach (KeyValuePair<Ingredient, int> requiredEntry in targetRecipe.RequiredIngredients)
                    {
                        Console.WriteLine($"[PLAYER.AttemptToPrepareDrink] Checking for required: {requiredEntry.Key.Name} x{requiredEntry.Value}");
                        if (!providedIngredientsMap.TryGetValue(requiredEntry.Key, out int providedCount) || providedCount != requiredEntry.Value)
                        {
                            ingredientsDoMatchTargetRecipeObject = false; 
                            Console.WriteLine($"[PLAYER.AttemptToPrepareDrink] Mismatch found: Provided map has {(providedIngredientsMap.TryGetValue(requiredEntry.Key, out int pc) ? pc.ToString() : "0")} of {requiredEntry.Key.Name}, required {requiredEntry.Value}.");
                            break; 
                        }
                        Console.WriteLine($"[PLAYER.AttemptToPrepareDrink] Matched: {requiredEntry.Key.Name} x{requiredEntry.Value}");
                    }
                }
                else
                {
                    Console.WriteLine($"[PLAYER.AttemptToPrepareDrink] Count mismatch: Provided {providedIngredientsMap.Count} ingredient types, Recipe requires {targetRecipe.RequiredIngredients.Count}.");
                    ingredientsDoMatchTargetRecipeObject = false;
                }
            }
            else
            {
                Console.WriteLine("[PLAYER.AttemptToPrepareDrink] No targetRecipe object provided (e.g., Alien order or experimental). ingredientsDoMatchTargetRecipeObject remains false.");
            }
            Console.WriteLine($"[PLAYER.AttemptToPrepareDrink] Final check: Ingredients provided {(ingredientsDoMatchTargetRecipeObject ? "MATCH" : "DO NOT MATCH")} target recipe object '{targetRecipe?.RecipeName ?? "N/A"}'.");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Recipe? recipeForEquipmentToAimFor = ingredientsDoMatchTargetRecipeObject ? targetRecipe : null;

            bool isForAlienDemand = this.CurrentOrder?.AlienDemand.HasValue == true;

            Console.WriteLine($"[PLAYER.AttemptToPrepareDrink] Calling {specificTool.EquipmentName}.PerformProcess. AimingForRecipe: '{recipeForEquipmentToAimFor?.RecipeName ?? "Generic/Muddled Concoction"}'. Provided to Equip: {string.Join(", ", providedIngredientsMap.Select(kv => $"{kv.Key.Name}x{kv.Value}"))}");
            Console.ResetColor();

            ProcessResult equipmentProcessResult = specificTool.PerformProcess(recipeForEquipmentToAimFor, providedIngredientsMap, isForAlienDemand);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[PLAYER.AttemptToPrepareDrink] Equipment.PerformProcess returned: Success(DrinkMade)={equipmentProcessResult.WasSuccessful}, Drink='{equipmentProcessResult.PreparedDrink?.Name ?? "NULL_DRINK"}', Quality='{equipmentProcessResult.PreparedDrink?.Quality}', Reason='{equipmentProcessResult.OutcomeReason}'");

            if (equipmentProcessResult.PreparedDrink != null) 
            {
                ConsumeIngredients(providedIngredientsMap); 
                SetHeldItem(equipmentProcessResult.PreparedDrink); 
                Console.WriteLine($"[PLAYER.AttemptToPrepareDrink] Player now holding: '{HeldItem?.Name}'. BaseRecipe of HeldItem: '{(HeldItem as Drink)?.BaseRecipe?.RecipeName ?? "N/A"}'.");

                string finalOutcomeReason = equipmentProcessResult.OutcomeReason;
                if (targetRecipe != null && (HeldItem as Drink)?.BaseRecipe?.RecipeName != targetRecipe.RecipeName)
                {
                    finalOutcomeReason = $"Made '{(HeldItem as Drink)?.BaseRecipe?.RecipeName ?? "Unknown Drink"}'; Target was '{targetRecipe.RecipeName}'. Equipment: {equipmentProcessResult.OutcomeReason}";
                }
                else if (targetRecipe == null && HeldItem != null)
                {
                    finalOutcomeReason = $"Formulated '{(HeldItem as Drink)?.Name ?? "Unknown Concoction"}'. " + equipmentProcessResult.OutcomeReason;
                }
                Console.ResetColor();
                return new ProcessResult(true, HeldItem as Drink, finalOutcomeReason); // Success, a drink was made.
            }
            else
            {
                SetHeldItem(null); 
                Console.WriteLine($"[PLAYER.AttemptToPrepareDrink] Equipment FAILED to produce any drink item. Clearing HeldItem. Reason: {equipmentProcessResult.OutcomeReason}");
                Console.ResetColor();
                return new ProcessResult(false, null, $"Brewing failed at '{specificTool.EquipmentName}': {equipmentProcessResult.OutcomeReason}"); // Failure, no drink made.
            }
        }

        /// <summary>
        /// Attempts to serve the currently held drink to a specified customer.
        /// Validates the customer, held item, and compares the served drink against the customer's order (recipe or alien demand).
        /// Updates order status, player money, and sanity based on the outcome.
        /// </summary>
        /// <param name="customer">The <see cref="Customer"/> to serve the drink to.</param>
        /// <param name="gameManager">A reference to the <see cref="GameManager"/> for accessing UI and game state.</param>
        public void ServeDrink(Customer customer, GameManager gameManager)
        {
            if (customer == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[PLAYER.ServeDrink] ERROR: Customer is null.");
                Console.ResetColor();
                if (HeldItem is Drink) SetHeldItem(null); 
            }

            if (!(HeldItem is Drink drinkToServe)) 
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[PLAYER.ServeDrink] ERROR: Player is not holding a drink.");
                Console.ResetColor();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"[PLAYER.ServeDrink] CALLED. Target Customer: {customer?.CustomerID} ({customer?.GetType().Name}). Customer expects: '{customer?.Order?.AssignedRecipe?.RecipeName ?? (customer?.Order?.AlienDemand.HasValue == true ? "Alien Demand" : "NO ACTIVE ORDER FOR CUSTOMER")}'. Player is holding: '{drinkToServe.Name}' (BaseRecipe: '{drinkToServe.BaseRecipe?.RecipeName}').");
            Console.ResetColor();

            if (customer?.Order == null) // Check if the customer actually has an order.
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[PLAYER.ServeDrink] Customer {customer?.CustomerID} has no active order. Drink '{drinkToServe.Name}' not served to them. Player keeps holding the drink.");
                Console.ResetColor();
                return;
            }

            bool wasPlayersCurrentOrderForThisCustomer = (this.CurrentOrder != null && this.CurrentOrder.RequestingCustomer == customer);
            bool servedCorrectDrink;

            if (customer.Order.AlienDemand.HasValue)
            {
                Console.WriteLine($"[PLAYER.ServeDrink] Serving to Alien. Checking property demand.");
                AlienPropertyDemand demand = customer.Order.AlienDemand.Value;

                int bitternessDiff = Math.Abs(drinkToServe.CalculatedBitterness - demand.TargetBitterness);
                int sweetnessDiff = Math.Abs(drinkToServe.CalculatedSweetness - demand.TargetSweetness);
                int sournessDiff = Math.Abs(drinkToServe.CalculatedSourness - demand.TargetSourness);
                int propertyTolerance = 1; 

                Console.WriteLine($"[PLAYER.ServeDrink] Alien Demand: B({demand.TargetBitterness}) Sw({demand.TargetSweetness}) So({demand.TargetSourness})");
                Console.WriteLine($"[PLAYER.ServeDrink] Served Drink Props: B({drinkToServe.CalculatedBitterness}) Sw({drinkToServe.CalculatedSweetness}) So({drinkToServe.CalculatedSourness})");
                Console.WriteLine($"[PLAYER.ServeDrink] Differences: B({bitternessDiff}) Sw({sweetnessDiff}) So({sournessDiff}). Tolerance: {propertyTolerance}");

                if (bitternessDiff <= propertyTolerance && sweetnessDiff <= propertyTolerance && sournessDiff <= propertyTolerance)
                {
                    customer.Order.UpdateStatus(OrderStatus.Served);
                    CollectMoney(5.0f); 
                    servedCorrectDrink = true;
                    Console.WriteLine($"[PLAYER.ServeDrink] Alien property demand MET! Order Served. Money: {_money}");
                }
                else
                {
                    customer.Order.UpdateStatus(OrderStatus.Failed);
                    servedCorrectDrink = false;
                    float sanityLoss = 20f;
                    this.SetSanityLevel(this.SanityLevel - sanityLoss);
                    gameManager.UIManager?.DisplayMessage($"The Alien screeches! Properties misaligned... Sanity -{sanityLoss}!", MessageType.Error, true);
                    Console.WriteLine($"[PLAYER.ServeDrink] Alien property demand FAILED. Sanity loss. Order status: {customer.Order.CurrentStatus}");
                }
            }
            else if (customer.Order.AssignedRecipe != null && drinkToServe.BaseRecipe != null &&
                     customer.Order.AssignedRecipe.RecipeName == drinkToServe.BaseRecipe.RecipeName)
            {
                Console.WriteLine($"[PLAYER.ServeDrink] Serving to Customer. Checking recipe match.");
                customer.Order.UpdateStatus(OrderStatus.Served);
                CollectMoney(customer.Order.AssignedRecipe.Price); 
                servedCorrectDrink = true;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[PLAYER.ServeDrink] Served CORRECT drink '{drinkToServe.Name}' to {customer.CustomerID}. OrderStatus: {customer.Order.CurrentStatus}. Money: {_money}");
                Console.ResetColor();

                if (customer is SupernaturalCustomer sc && sc.Type == SupernaturalCustomerType.FireMonster && gameManager.IsBrewingStationOverheated)
                {
                    gameManager.SetBrewingStationOverheated(false); 
                }
            }
            else
            {
                servedCorrectDrink = false;
                string expectedDrinkName = "Unknown (Order or Recipe Missing)";
                float sanityLoss = 15f;

                if (customer.Order != null) 
                {
                    customer.Order.UpdateStatus(OrderStatus.Failed);
                    if (customer.Order.AssignedRecipe != null)
                    {
                        expectedDrinkName = customer.Order.AssignedRecipe.RecipeName;
                    }
                    else if (customer.Order.AlienDemand.HasValue) 
                    {
                        expectedDrinkName = $"Alien Demand ({customer.Order.AlienDemand.Value.ToString()})";
                    }
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[PLAYER.ServeDrink] Served WRONG drink '{drinkToServe.Name}' to {customer.CustomerID} (expected: '{expectedDrinkName}'). OrderStatus set to Failed.");
                Console.ResetColor();

                this.SetSanityLevel(this.SanityLevel - sanityLoss);
                gameManager.UIManager?.DisplayMessage($"Wrong Drink! Customer is not pleased... Sanity -{sanityLoss}!", MessageType.Warning, true);
                Console.WriteLine($"[PLAYER.ServeDrink] Player sanity decreased by {sanityLoss}. New sanity: {this.SanityLevel}");

                if (customer is SupernaturalCustomer sc && sc.Type == SupernaturalCustomerType.Ghost)
                {
                    Console.WriteLine($"[PLAYER.ServeDrink] Wrong drink for Ghost {sc.CustomerID}, activating interference.");
                    gameManager.ActivateGhostlyInterference(duration: 30.0f, dimAlpha: 0.5f, slowFactor: 0.6f);
                }
            }

            Console.WriteLine($"[PLAYER.ServeDrink] Calling customer.ReceiveDrink() for {customer.CustomerID} with {drinkToServe.Name}. CorrectlyServed: {servedCorrectDrink}");
            customer.ReceiveDrink(drinkToServe, servedCorrectDrink); 

            if (wasPlayersCurrentOrderForThisCustomer)
            {
                this.SetCurrentOrder(null);
                Console.WriteLine($"[PLAYER.ServeDrink] Player's active order for {customer.CustomerID} cleared.");
            }
            SetHeldItem(null); 
            Console.WriteLine("[PLAYER.ServeDrink] Player's HeldItem cleared.");
        }

        /// <summary>
        /// Adds a specified amount of money to the player's total.
        /// Only positive amounts are added.
        /// </summary>
        /// <param name="amount">The amount of money to add. Must be greater than 0.</param>
        public void CollectMoney(float amount)
        {
            if (amount > 0)
            {
                _money += amount;
                // Console.WriteLine($"Collected {amount}. Total money: {_money}"); 
            }
        }

        /// <summary>
        /// Adds a specified quantity of an ingredient to the player's inventory.
        /// If the ingredient already exists in the inventory, its quantity is increased. Otherwise, it's added.
        /// </summary>
        /// <param name="ingredient">The <see cref="Ingredient"/> to add or update.</param>
        /// <param name="quantity">The quantity of the ingredient to add. Must be positive.</param>
        public void RestockIngredient(Ingredient ingredient, int quantity)
        {
            if (ingredient != null && quantity > 0)
            {
                if (_inventory.ContainsKey(ingredient))
                {
                    _inventory[ingredient] += quantity;
                }
                else
                {
                    _inventory[ingredient] = quantity;
                }
                // Console.WriteLine($"Restocked/Purchased {quantity} of {ingredient.Name}. New total: {_inventory[ingredient]}"); 
            }
        }

        /// <summary>
        /// Checks if the player has a sufficient quantity of all required ingredients in their inventory.
        /// This version does not check for game-specific conditions like stolen ingredients.
        /// </summary>
        /// <param name="requiredIngredientsMap">A dictionary mapping each required <see cref="Ingredient"/> to its needed quantity.</param>
        /// <returns><c>true</c> if all required ingredients are available in sufficient quantities; otherwise, <c>false</c>.</returns>
        public bool HasIngredients(Dictionary<Ingredient, int> requiredIngredientsMap)
        {
            if (requiredIngredientsMap == null || requiredIngredientsMap.Count == 0)
            {
                return true;
            }

            foreach (KeyValuePair<Ingredient, int> requiredEntry in requiredIngredientsMap)
            {
                if (!_inventory.TryGetValue(requiredEntry.Key, out int currentQuantity) || currentQuantity < requiredEntry.Value)
                {
                    System.Console.WriteLine($"Player.HasIngredients: Missing/insufficient ingredient: {requiredEntry.Key.Name}. Required: {requiredEntry.Value}, Have: {(_inventory.TryGetValue(requiredEntry.Key, out int val) ? val : 0)}");
                    return false; 
                }
            }
            return true; 
        }

        /// <summary>
        /// Consumes the specified quantities of ingredients from the player's inventory.
        /// Logs an error if an attempt is made to consume more ingredients than available, but still tries to reduce quantity.
        /// </summary>
        /// <param name="ingredientsToConsumeMap">A dictionary mapping each <see cref="Ingredient"/> to be consumed to its quantity.</param>
        public void ConsumeIngredients(Dictionary<Ingredient, int> ingredientsToConsumeMap)
        {
            if (ingredientsToConsumeMap == null) return;

            foreach (KeyValuePair<Ingredient, int> entry in ingredientsToConsumeMap)
            {
                if (_inventory.ContainsKey(entry.Key))
                {
                    if (_inventory[entry.Key] >= entry.Value)
                    {
                        _inventory[entry.Key] -= entry.Value;
                        // Console.WriteLine($"Consumed {entry.Value} of {entry.Key.Name}. Remaining in inventory: {_inventory[entry.Key]}");
                    }
                    else
                    {
                        System.Console.WriteLine($"Error in ConsumeIngredients: Tried to consume {entry.Value} of {entry.Key.Name}, but only {_inventory[entry.Key]} available. Consuming all available.");
                        _inventory[entry.Key] = 0; 
                    }
                }
                else
                {
                    System.Console.WriteLine($"Error in ConsumeIngredients: Tried to consume {entry.Key.Name}, but it's not in inventory.");
                }
            }
        }

        /// <summary>
        /// Attempts to spend a specified amount of money from the player's current funds.
        /// </summary>
        /// <param name="amount">The amount of money to spend. If non-positive, the method returns true without action.</param>
        /// <returns><c>true</c> if the player has enough money and it was spent, or if amount was non-positive; <c>false</c> if the player cannot afford the amount.</returns>
        public bool SpendMoney(float amount)
        {
            if (amount <= 0) return true; 

            if (_money >= amount)
            {
                _money -= amount;
                // Console.WriteLine($"Spent {amount:F2}. Remaining money: {_money:F2}");
                return true;
            }
            else
            {
                System.Console.WriteLine($"Not enough money. Tried to spend {amount:F2}, but only have {_money:F2}");
                return false;
            }
        }

        /// <summary>
        /// Attempts to purchase a specified quantity of an ingredient.
        /// Deducts the total cost from player's money and adds the ingredient to their inventory if successful.
        /// </summary>
        /// <param name="ingredient">The <see cref="Ingredient"/> to purchase.</param>
        /// <param name="quantityToBuy">The quantity of the ingredient to purchase. Must be positive.</param>
        /// <returns><c>true</c> if the purchase was successful; otherwise, <c>false</c>.</returns>
        public bool PurchaseIngredient(Ingredient ingredient, int quantityToBuy)
        {
            if (ingredient == null || quantityToBuy <= 0)
            {
                Console.WriteLine("PurchaseIngredient: Invalid ingredient or quantity.");
                return false;
            }

            float totalCost = ingredient.Cost * quantityToBuy;
            if (SpendMoney(totalCost)) 
            {
                RestockIngredient(ingredient, quantityToBuy); 
                Console.WriteLine($"Purchased {quantityToBuy} of {ingredient.Name} for ${totalCost:F2}.");
                return true;
            }
            else
            {
                Console.WriteLine($"PurchaseIngredient: Could not afford {quantityToBuy} of {ingredient.Name}. Needed ${totalCost:F2}, have ${_money:F2}.");
                return false;
            }
        }

        /// <summary>
        /// Checks if the player has sufficient quantities of all required ingredients in their inventory,
        /// also considering game-specific conditions like stolen ingredients via the <see cref="GameManager"/>.
        /// </summary>
        /// <param name="requiredIngredientsMap">A dictionary mapping each required <see cref="Ingredient"/> to its needed quantity.</param>
        /// <param name="gameManager">A reference to the <see cref="GameManager"/> to check for game conditions like stolen ingredients.</param>
        /// <returns><c>true</c> if all required ingredients are available and usable; otherwise, <c>false</c>.</returns>
        public bool HasIngredients(Dictionary<Ingredient, int> requiredIngredientsMap, GameManager gameManager)
        {
            if (requiredIngredientsMap == null || requiredIngredientsMap.Count == 0) return true;

            foreach (KeyValuePair<Ingredient, int> requiredEntry in requiredIngredientsMap)
            {
                if (gameManager.IsIngredientCurrentlyStolen && gameManager.StolenIngredientType == requiredEntry.Key.Type)
                {
                    Console.WriteLine($"Player.HasIngredients: Cannot use {requiredEntry.Key.Name}, it's STOLEN!");
                    return false;
                }

                if (!_inventory.TryGetValue(requiredEntry.Key, out int currentQuantity) || currentQuantity < requiredEntry.Value)
                {
                    System.Console.WriteLine($"Player.HasIngredients: Missing/insufficient ingredient: {requiredEntry.Key.Name}. Required: {requiredEntry.Value}, Have: {(_inventory.TryGetValue(requiredEntry.Key, out int val) ? val : 0)}");
                    return false;
                }
            }
            return true;
        }
    }
}