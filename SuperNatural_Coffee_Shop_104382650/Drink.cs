using System; 
using System.Collections.Generic;

namespace CoffeeShop
{
    /// <summary>
    /// Represents a prepared drink that can be made and served to customers.
    /// This class holds information about the drink's origin (recipe), its quality, and its taste profile.
    /// It inherits common properties like name and description from the <see cref="Item"/> base class.
    /// </summary>
    public class Drink : Item
    {
        /// <summary>
        /// Gets the base <see cref="Recipe"/> that was used to create this drink.
        /// This provides information about the intended ingredients and preparation.
        /// </summary>
        public Recipe BaseRecipe { get; }

        /// <summary>
        /// Gets the assessed quality of the prepared drink (e.g., "Perfect", "Standard", "Muddled").
        /// This can influence customer satisfaction or scoring.
        /// </summary>
        public string Quality { get; }

        /// <summary>
        /// Gets the calculated bitterness value of the drink, based on its ingredients and preparation.
        /// </summary>
        public int CalculatedBitterness { get; }

        /// <summary>
        /// Gets the calculated sweetness value of the drink, based on its ingredients and preparation.
        /// </summary>
        public int CalculatedSweetness { get; }

        /// <summary>
        /// Gets the calculated sourness value of the drink, based on its ingredients and preparation.
        /// </summary>
        public int CalculatedSourness { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Drink"/> class.
        /// </summary>
        /// <param name="name">The name of the drink, often derived from its base recipe (e.g., "Espresso", "Muddled Mess").</param>
        /// <param name="description">A textual description of the drink.</param>
        /// <param name="baseRecipe">The <see cref="Recipe"/> instance used as a blueprint for this drink. Cannot be null.</param>
        /// <param name="quality">The quality rating of the drink. Defaults to "Standard" if null or whitespace.</param>
        /// <param name="actualBitterness">The final calculated bitterness of the drink. Defaults to 0.</param>
        /// <param name="actualSweetness">The final calculated sweetness of the drink. Defaults to 0.</param>
        /// <param name="actualSourness">The final calculated sourness of the drink. Defaults to 0.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="baseRecipe"/> is null.</exception>
        public Drink(string name, string description, Recipe baseRecipe, string quality,
                     int actualBitterness = 0, int actualSweetness = 0, int actualSourness = 0)
            : base(name, description) 
        {
            BaseRecipe = baseRecipe ?? throw new ArgumentNullException(nameof(baseRecipe));
            Quality = string.IsNullOrWhiteSpace(quality) ? "Standard" : quality;

            CalculatedBitterness = actualBitterness;
            CalculatedSweetness = actualSweetness;
            CalculatedSourness = actualSourness;

            if (BaseRecipe.RecipeName == "Alien Concoction" || BaseRecipe.RecipeName == "Muddled Mess")
            {
                Console.WriteLine($"[DRINK CREATED] Name: '{Name}', Quality: '{Quality}', Recipe: '{BaseRecipe.RecipeName}', Props: Bitter({CalculatedBitterness}), Sweet({CalculatedSweetness}), Sour({CalculatedSourness})");
            }
        }
    }
}