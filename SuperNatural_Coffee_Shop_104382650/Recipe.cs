using System.Collections.Generic; 
using System; 


namespace CoffeeShop
{
    /// <summary>
    /// Defines a recipe for a drink, including ingredients, preparation, and pricing.
    /// </summary>
    public class Recipe
    {
        private readonly string _recipeName;
        private readonly Dictionary<Ingredient, int> _requiredIngredients;
        private readonly List<string> _preparationSteps;
        private readonly float _price;
        private readonly float _basePreparationTime;
        private readonly bool _isSupernatural;
        private readonly List<string> _ritualisticSteps; 
        private readonly EquipmentType _requiredEquipmentType;
        private readonly int _unlockLevel; 

        /// <summary>
        /// Gets the name of the recipe.
        /// </summary>
        public string RecipeName => _recipeName;

        /// <summary>
        /// Gets the dictionary of required ingredients and their quantities.
        /// The key is the Ingredient object, and the value is the quantity required.
        /// </summary>
        public Dictionary<Ingredient, int> RequiredIngredients => _requiredIngredients;

        /// <summary>
        /// Gets the price of the drink made from this recipe.
        /// </summary>
        public float Price => _price;

        /// <summary>
        /// Gets a value indicating whether this recipe is for a supernatural drink.
        /// </summary>
        public bool IsSupernatural => _isSupernatural;

        /// <summary>
        /// Gets the type of equipment primarily required for this recipe.
        /// </summary>
        public EquipmentType RequiredEquipmentType => _requiredEquipmentType;

        /// <summary>
        /// Gets the level at which this recipe is typically unlocked or becomes commonly available.
        /// </summary>
        public int UnlockLevel => _unlockLevel;

        /// <summary>
        /// Initializes a new instance of the <see cref="Recipe"/> class.
        /// </summary>
        /// <param name="name">The name of the recipe.</param>
        /// <param name="price">The price of the drink.</param>
        /// <param name="prepTime">The base preparation time.</param>
        /// <param name="equipmentType">The primary type of equipment required.</param>
        /// <param name="unlockLevel">The level at which this recipe is typically unlocked. Defaults to 1.</param>
        /// <param name="isSupernatural">Whether the recipe is for a supernatural drink. Defaults to false.</param>
        public Recipe(string name, float price, float prepTime, EquipmentType equipmentType, int unlockLevel = 1, bool isSupernatural = false)
        {
            _recipeName = string.IsNullOrWhiteSpace(name) ? "Unnamed Recipe" : name;
            _price = price > 0 ? price : 0f;
            _basePreparationTime = prepTime > 0 ? prepTime : 0f;
            _requiredEquipmentType = equipmentType;
            _unlockLevel = unlockLevel > 0 ? unlockLevel : 1; // NEW: Assign and validate unlockLevel
            _isSupernatural = isSupernatural;

            _requiredIngredients = new Dictionary<Ingredient, int>();
            _preparationSteps = new List<string>();
            _ritualisticSteps = new List<string>();
        }

        /// <summary>
        /// Adds an ingredient requirement to the recipe.
        /// If the ingredient already exists, its quantity can be updated or added based on desired logic.
        /// For simplicity, this implementation overwrites/sets the quantity.
        /// </summary>
        /// <param name="ingredient">The ingredient to add.</param>
        /// <param name="quantity">The required quantity of the ingredient.</param>
        public Recipe AddIngredientRequirement(Ingredient ingredient, int quantity)
        {
            if (ingredient != null && quantity > 0)
            {
                _requiredIngredients[ingredient] = quantity;
            }
            else {
                Console.WriteLine($"Warning: Attempted to add invalid ingredient requirement to recipe '{this.RecipeName}'. Ingredient null: {ingredient == null}, Quantity: {quantity}");
            }
            return this;
        }
    }
}
