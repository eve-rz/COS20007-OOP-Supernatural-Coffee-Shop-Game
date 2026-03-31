using System;
using System.Collections.Generic;
using System.Linq; 

namespace CoffeeShop
{
    /// <summary>
    /// Represents a coffee machine equipment item in the coffee shop.
    /// It can brew drinks, requires water, and its condition degrades with use.
    /// </summary>
    public class CoffeeMachine : Equipment
    {
        /// <summary>
        /// The maximum amount of water the coffee machine's tank can hold, in arbitrary units.
        /// </summary>
        private readonly int _waterTankCapacityUnits = 100;
        /// <summary>
        /// The current amount of water in the coffee machine's tank, in arbitrary units.
        /// </summary>
        private int _currentWaterUnits;

        /// <summary>
        /// Gets the total capacity of the water tank in units.
        /// </summary>
        public int WaterTankCapacityUnits => _waterTankCapacityUnits;

        /// <summary>
        /// Gets or privately sets the current amount of water in the tank.
        /// The value is clamped between 0 and the <see cref="WaterTankCapacityUnits"/>.
        /// </summary>
        public int CurrentWaterUnits
        {
            get => _currentWaterUnits;
            private set => _currentWaterUnits = Math.Clamp(value, 0, _waterTankCapacityUnits);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoffeeMachine"/> class.
        /// </summary>
        /// <param name="name">The specific name of this coffee machine (e.g., "Main Espresso Machine").</param>
        /// <param name="interactionName">The name displayed for interaction prompts (e.g., "Use Coffee Machine").</param>
        /// <param name="capacityLiters_unused_for_now">An unused parameter, potentially for future capacity specifications in liters.</param>
        public CoffeeMachine(string name, string interactionName, float capacityLiters_unused_for_now)
            : base(name, EquipmentType.CoffeeMachine, interactionName)
        {
            _currentWaterUnits = _waterTankCapacityUnits; // Start with a full tank
        }

        /// <summary>
        /// Refills the coffee machine's water tank to its maximum capacity.
        /// </summary>
        public void RefillWaterToFull()
        {
            CurrentWaterUnits = _waterTankCapacityUnits;
            Console.WriteLine($"{EquipmentName} water tank refilled to full ({_waterTankCapacityUnits} units).");
        }

        /// <summary>
        /// Initiates an interaction with the coffee machine, providing context based on its current state.
        /// Checks for water level and machine condition before allowing brewing.
        /// </summary>
        /// <returns>An <see cref="InteractionContext"/> detailing possible actions and messages.</returns>
        public override InteractionContext InitiateInteraction()
        {
            if (CurrentWaterUnits < 30) 
            {
                return new InteractionContext(false, $"Needs more water!", new List<string> { "Refill Water" });
            }
            return new InteractionContext(true, $"{EquipmentName} is ready. Current water: {CurrentWaterUnits} units", new List<string> { "Brew Drink", "Check Status" });
        }

        /// <summary>
        /// Attempts to prepare a drink using the provided ingredients and optionally a target recipe.
        /// Consumes water and degrades the machine's condition upon use.
        /// </summary>
        /// <param name="targetRecipe">The <see cref="Recipe"/> the user is attempting to make. Can be null if making a custom concoction.</param>
        /// <param name="ingredientsToProcess">A dictionary of <see cref="Ingredient"/>s and their quantities provided for brewing.</param>
        /// <returns>A <see cref="ProcessResult"/> indicating success or failure, the <see cref="Drink"/> produced (if any), and an outcome message.</returns>
        public override ProcessResult PerformProcess(Recipe? targetRecipe, Dictionary<Ingredient, int> ingredientsToProcess, bool isForAlienDemand = false)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[EQUIP.CoffeeMachine.PerformProcess] START. TargetRecipe (passed in): '{targetRecipe?.RecipeName ?? "None"}'. Ingredients given: {ingredientsToProcess.Count} types. Water: {CurrentWaterUnits}");
            Console.ResetColor();

            if (IsBusy)
                return new ProcessResult(false, null, $"{EquipmentName} is busy.");

            bool isCoolDownRecipe = targetRecipe?.RecipeName == "Cool Down";
            int waterUnitsNeeded = isCoolDownRecipe ? 0 : 10; // Standard water usage per drink. 

            if (CurrentWaterUnits < waterUnitsNeeded && !isCoolDownRecipe)
            {
                Console.WriteLine($"[EQUIP.CoffeeMachine.PerformProcess] Not enough water. Required: {waterUnitsNeeded}, Have: {CurrentWaterUnits}");
                return new ProcessResult(false, null, "Not enough water in the station!");
            }

            Drink preparedDrink;
            string outcomeMessage;
            string quality = "Okay"; // Default quality.

            bool canMakeTargetRecipeAndItWasGiven = false;
            if (targetRecipe != null)
            {
                canMakeTargetRecipeAndItWasGiven = true;
                if (ingredientsToProcess.Count != targetRecipe.RequiredIngredients.Count) {
                    canMakeTargetRecipeAndItWasGiven = false;
                } else {
                    foreach (var reqEntry in targetRecipe.RequiredIngredients) {
                        if (!ingredientsToProcess.TryGetValue(reqEntry.Key, out int providedQty) || providedQty != reqEntry.Value) {
                            canMakeTargetRecipeAndItWasGiven = false;
                            Console.WriteLine($"[EQUIP.CoffeeMachine.PerformProcess] Mismatch for target recipe '{targetRecipe.RecipeName}': Required {reqEntry.Key.Name} x{reqEntry.Value}, Got x{(ingredientsToProcess.TryGetValue(reqEntry.Key, out int pq) ? pq : 0)}");
                            break;
                        }
                    }
                }
                if(canMakeTargetRecipeAndItWasGiven) Console.WriteLine($"[EQUIP.CoffeeMachine.PerformProcess] Ingredients MATCH target recipe '{targetRecipe.RecipeName}'.");
                else Console.WriteLine($"[EQUIP.CoffeeMachine.PerformProcess] Ingredients DO NOT MATCH target recipe '{targetRecipe.RecipeName}'. Will make muddled drink.");
            } else {
                Console.WriteLine("[EQUIP.CoffeeMachine.PerformProcess] No targetRecipe object provided. Will make muddled drink from ingredients.");
            }

            // Calculate summed taste properties from the ingredients provided, for any drink produced.
            int sumBitterness = 0; int sumSweetness = 0; int sumSourness = 0;
            foreach(var entry in ingredientsToProcess) {
                if (entry.Key != null) { 
                    sumBitterness += entry.Key.Bitterness * entry.Value;
                    sumSweetness += entry.Key.Sweetness * entry.Value;
                    sumSourness += entry.Key.Sourness * entry.Value;
                }
            }
            Console.WriteLine($"[EQUIP.CoffeeMachine.PerformProcess] Calculated custom brew props: B:{sumBitterness}, Sw:{sumSweetness}, So:{sumSourness}");


            if (canMakeTargetRecipeAndItWasGiven && targetRecipe != null) // Successfully making the intended recipe.
            {
                Console.WriteLine($"[EQUIP.CoffeeMachine.PerformProcess] Brewing TARGET: '{targetRecipe.RecipeName}'.");
                quality = "Standard"; 
                preparedDrink = new Drink(targetRecipe.RecipeName, $"A {quality.ToLower()} {targetRecipe.RecipeName}.", targetRecipe, quality,
                                        sumBitterness, sumSweetness, sumSourness);
                outcomeMessage = $"{targetRecipe.RecipeName} prepared.";
            }
            else 
            {
                string concoctionName;
                string concoctionDesc;
                
                
                if (isForAlienDemand)
                {
                    concoctionName = "Alien Concoction";
                    concoctionDesc = "A brew formulated to alien specifications. It hums faintly.";
                    Console.WriteLine($"[EQUIP.CoffeeMachine.PerformProcess] Brewing '{concoctionName}'.");
                }
                else 
                {
                    concoctionName = "Muddled Mess";
                    concoctionDesc = "An experimental brew with muddled properties.";
                    Console.WriteLine($"[EQUIP.CoffeeMachine.PerformProcess] Brewing '{concoctionName}'.");
                }

                Recipe baseConcoctionRecipe = new Recipe(concoctionName, 0.1f, 5f, EquipmentType.CoffeeMachine, 1);
                foreach(var entry in ingredientsToProcess) {
                    if(entry.Key != null) baseConcoctionRecipe.AddIngredientRequirement(entry.Key, entry.Value);
                }

                quality = "Questionable";
                preparedDrink = new Drink(concoctionName, concoctionDesc, baseConcoctionRecipe, quality,
                                        sumBitterness, sumSweetness, sumSourness);
                outcomeMessage = $"Formulated a {concoctionName}.";
                if (targetRecipe != null && !canMakeTargetRecipeAndItWasGiven) {
                    outcomeMessage += $" (Intended: '{targetRecipe.RecipeName}')";
                }
            }

            if (!isCoolDownRecipe) CurrentWaterUnits -= waterUnitsNeeded;
            SetBusy(false); 

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[EQUIP.CoffeeMachine.PerformProcess] END. Produced: '{preparedDrink.Name}' (BaseRecipeName: '{preparedDrink.BaseRecipe.RecipeName}'). Water After: {CurrentWaterUnits}. Msg: {outcomeMessage}");
            Console.ResetColor();
            return new ProcessResult(true, preparedDrink, outcomeMessage);
        }
    }
}