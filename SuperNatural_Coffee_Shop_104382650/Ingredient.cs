using System; 
using System.Collections.Generic; 

namespace CoffeeShop
{
    /// <summary>
    /// Represents an ingredient that can be used in recipes to create drinks or other items.
    /// Each ingredient has defined taste properties, a cost, and a type, and can be supernatural.
    /// Inherits common properties like name and description from the <see cref="Item"/> base class.
    /// </summary>
    public class Ingredient : Item
    {
        /// <summary>
        /// Gets the categorical <see cref="IngredientType"/> of this ingredient (e.g., CoffeeBean, Milk, Herb).
        /// This helps in identifying how an ingredient can be used or processed.
        /// </summary>
        public IngredientType Type { get; }

        /// <summary>
        /// Gets a value indicating whether this ingredient has supernatural or magical properties.
        /// Supernatural ingredients might have unique effects or be required for special recipes.
        /// </summary>
        public bool IsSupernatural { get; }

        /// <summary>
        /// Backing field for the bitterness value. Clamped between 0 and 8.
        /// </summary>
        private readonly int _bitterness;
        /// <summary>
        /// Backing field for the sourness value. Clamped between 0 and 8.
        /// </summary>
        private readonly int _sourness;
        /// <summary>
        /// Backing field for the sweetness value. Clamped between 0 and 8.
        /// </summary>
        private readonly int _sweetness;
        /// <summary>
        /// Backing field for the cost of the ingredient.
        /// </summary>
        private readonly float _cost;

        /// <summary>
        /// Gets the intensity of bitterness for this ingredient, typically on a scale (e.g., 0-8).
        /// This value is determined at creation and clamped within a predefined range.
        /// </summary>
        public int Bitterness
        {
            get { return _bitterness; }
        }

        /// <summary>
        /// Gets the intensity of sourness for this ingredient, typically on a scale (e.g., 0-8).
        /// This value is determined at creation and clamped within a predefined range.
        /// </summary>
        public int Sourness
        {
            get { return _sourness; }
        }

        /// <summary>
        /// Gets the intensity of sweetness for this ingredient, typically on a scale (e.g., 0-8).
        /// This value is determined at creation and clamped within a predefined range.
        /// </summary>
        public int Sweetness
        {
            get { return _sweetness; }
        }

        /// <summary>
        /// Gets the cost of acquiring or using this ingredient.
        /// </summary>
        public float Cost
        {
            get { return _cost; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Ingredient"/> class with specified properties.
        /// Taste values (bitterness, sourness, sweetness) are clamped to a range of 0 to 8.
        /// </summary>
        /// <param name="name">The unique name of the ingredient (e.g., "Arabica Beans", "Ectoplasm").</param>
        /// <param name="description">A brief description of the ingredient.</param>
        /// <param name="type">The <see cref="IngredientType"/> categorizing this ingredient.</param>
        /// <param name="isSupernatural">A boolean indicating if the ingredient has supernatural qualities.</param>
        /// <param name="bitterness">The inherent bitterness value of the ingredient, ideally from 0 to 8.</param>
        /// <param name="sourness">The inherent sourness value of the ingredient, ideally from 0 to 8.</param>
        /// <param name="sweetness">The inherent sweetness value of the ingredient, ideally from 0 to 8.</param>
        /// <param name="cost">The cost associated with the ingredient. Defaults to 1.0f.
        public Ingredient(string name, string description, IngredientType type, bool isSupernatural, int bitterness,
        int sourness, int sweetness, float cost = 1.0f)
            : base(name, description) 
        {
            Type = type;
            IsSupernatural = isSupernatural;
            _cost = cost;

            _bitterness = Math.Max(0, Math.Min(8, bitterness));
            _sourness = Math.Max(0, Math.Min(8, sourness));
            _sweetness = Math.Max(0, Math.Min(8, sweetness));
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current ingredient.
        /// Equality is based on the ingredient's <see cref="Item.Name"/>, <see cref="Type"/>, and <see cref="IsSupernatural"/> status.
        /// Taste properties and cost are not considered for this equality check.
        /// </summary>
        /// <param name="obj">The object to compare with the current ingredient.</param>
        /// <returns>
        /// <c>true</c> if the specified object is an <see cref="Ingredient"/> and has the same name, type, and supernatural status as the current ingredient;
        /// otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object? obj)
        {
            // Standard type and null checks.
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Ingredient other = (Ingredient)obj;

            return Name == other.Name &&   
                   Type == other.Type &&
                   IsSupernatural == other.IsSupernatural;
        }

        /// <summary>
        /// Serves as the default hash function.
        /// Generates a hash code based on the same properties used for equality checking:
        /// <see cref="Item.Name"/>, <see cref="Type"/>, and <see cref="IsSupernatural"/>.
        /// </summary>
        /// <returns>A hash code for the current ingredient.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Type, IsSupernatural); // Name is inherited from Item.
        }

        /// <summary>
        /// Returns a string that represents the current ingredient, including its key properties.
        /// The format is: "Name (Type: IngredientType, Supernatural: True/False, B:Bitterness, So:Sourness, Sw:Sweetness)".
        /// Note: The original code for this method appears to have a syntax error with a missing closing parenthesis and curly brace.
        /// This documentation describes the apparent intended output.
        /// </summary>
        /// <returns>A string representation of the ingredient.</returns>
        public override string ToString()
        {
            return $"{Name} (Type: {Type}, Supernatural: {IsSupernatural}, B:{Bitterness}, So:{Sourness}, Sw:{Sweetness}";
        }
    }
}