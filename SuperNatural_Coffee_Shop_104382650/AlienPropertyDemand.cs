
/// <summary>
/// Represents the property demands of an alien customer in the coffee shop,
/// specifying the desired levels of bitterness, sweetness, and sourness.
/// </summary>
namespace CoffeeShop
{
    public struct AlienPropertyDemand
    {
        /// <summary>
        /// Gets or sets the target level of bitterness requested by the alien.
        /// </summary>
        public int TargetBitterness { get; set; }

        /// <summary>
        /// Gets or sets the target level of sweetness requested by the alien.
        /// </summary>
        public int TargetSweetness { get; set; }

        /// <summary>
        /// Gets or sets the target level of sourness requested by the alien.
        /// </summary>
        public int TargetSourness { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AlienPropertyDemand"/> struct
        /// with specified target values for bitterness, sweetness, and sourness.
        /// </summary>
        /// <param name="bitter">The target bitterness value.</param>
        /// <param name="sweet">The target sweetness value.</param>
        /// <param name="sour">The target sourness value.</param>
        public AlienPropertyDemand(int bitter, int sweet, int sour)
        {
            TargetBitterness = bitter;
            TargetSweetness = sweet;
            TargetSourness = sour;
        }

        /// <summary>
        /// Returns a string that represents the current property demand,
        /// including the target values for bitterness, sweetness, and sourness.
        /// </summary>
        /// <returns>
        /// A string representation of the alien's property demand.
        /// </returns>
        public override string ToString()
        {
            return $"Demand: Bitter({TargetBitterness}), Sweet({TargetSweetness}), Sour({TargetSourness})";
        }
    }
}