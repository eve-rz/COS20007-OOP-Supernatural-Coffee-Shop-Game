using System.Collections.Generic;
using System.Linq; 

namespace CoffeeShop
{
    /// <summary>
    /// Represents a game level, defining its objectives, conditions, and available elements.
    /// </summary>
    public class Level
    {
        private readonly string _levelID;
        private readonly float _targetEarnings;
        private readonly float _timeLimit; 
        private readonly string _customerSpawnInstructions; 
        private readonly List<Recipe> _availableRecipes;
        private readonly List<Ingredient> _unlockedIngredients;
        private readonly List<SupernaturalCustomerType> _allowedSupernaturalTypes;
        private readonly float _sanityDecayModifier; 

        /// <summary>
        /// Gets the unique identifier for this level.
        /// </summary>
        public string LevelID => _levelID;

        /// <summary>
        /// Gets the target amount of earnings the player needs to achieve to pass the level.
        /// </summary>
        public float TargetEarnings => _targetEarnings;

        /// <summary>
        /// Gets the time limit for completing the level, in seconds or game units.
        /// </summary>
        public float TimeLimit => _timeLimit;

        /// <summary>
        /// Gets a list of recipes available to the player in this level.
        /// Returns a new list to prevent external modification of the internal list.
        /// </summary>
        public List<Recipe> AvailableRecipes => new List<Recipe>(_availableRecipes);

        /// <summary>
        /// Gets a list of ingredients unlocked and available for use or purchase in this level.
        /// Returns a new list to prevent external modification of the internal list.
        /// </summary>
        public List<Ingredient> UnlockedIngredients => new List<Ingredient>(_unlockedIngredients);

        /// <summary>
        /// Gets a list of supernatural customer types allowed to spawn in this level.
        /// Returns a new list to prevent external modification of the internal list.
        /// </summary>
        public List<SupernaturalCustomerType> AllowedSupernaturalTypes => new List<SupernaturalCustomerType>(_allowedSupernaturalTypes);

        /// <summary>
        /// Initializes a new instance of the <see cref="Level"/> class.
        /// </summary>
        /// <param name="levelID">The unique identifier for the level.</param>
        /// <param name="targetEarnings">The target earnings for the level.</param>
        /// <param name="timeLimit">The time limit for the level.</param>
        /// <param name="customerSpawnInstructions">Instructions for customer spawning. Defaults to "DefaultSpawning".</param>
        /// <param name="sanityDecayModifier">Modifier for sanity decay. Defaults to 1.0f.</param>
        /// <param name="allowedSupernaturalTypes">A list of supernatural customer types allowed. Defaults to an empty list if null.</param>
        public Level(string levelID, float targetEarnings, float timeLimit,
                     string customerSpawnInstructions = "DefaultSpawning",
                     float sanityDecayModifier = 1.0f,
                     List<SupernaturalCustomerType>? allowedSupernaturalTypes = null) 
        {
            _levelID = string.IsNullOrWhiteSpace(levelID) ? System.Guid.NewGuid().ToString() : levelID;
            _targetEarnings = targetEarnings > 0 ? targetEarnings : 100f;
            _timeLimit = timeLimit > 0 ? timeLimit : 300f; 
            _customerSpawnInstructions = string.IsNullOrWhiteSpace(customerSpawnInstructions) ? "DefaultSpawning" : customerSpawnInstructions;
            _sanityDecayModifier = sanityDecayModifier >= 0 ? sanityDecayModifier : 1.0f; 

            _availableRecipes = new List<Recipe>();
            _unlockedIngredients = new List<Ingredient>();
            _allowedSupernaturalTypes = allowedSupernaturalTypes ?? new List<SupernaturalCustomerType>();
        }

        /// <summary>
        /// Adds an available recipe to this level.
        /// </summary>
        /// <param name="recipe">The recipe to make available. Must not be null.</param>
        public void AddAvailableRecipe(Recipe recipe)
        {
            if (recipe != null && !_availableRecipes.Contains(recipe))
            {
                _availableRecipes.Add(recipe);
                System.Console.WriteLine($"Recipe '{recipe.RecipeName}' added to Level '{LevelID}'.");
            }
        }

        /// <summary>
        /// Adds an unlocked ingredient to this level.
        /// </summary>
        /// <param name="ingredient">The ingredient to unlock. Must not be null.</param>
        public void AddUnlockedIngredient(Ingredient ingredient)
        {
            if (ingredient != null && !_unlockedIngredients.Contains(ingredient))
            {
                _unlockedIngredients.Add(ingredient);
                System.Console.WriteLine($"Ingredient '{ingredient.Name}' unlocked in Level '{LevelID}'.");
            }
        }
    }
}