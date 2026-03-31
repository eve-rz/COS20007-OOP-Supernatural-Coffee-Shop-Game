namespace CoffeeShop
{
    /// <summary>
    /// Represents a 2D vector or point with X and Y coordinates.
    /// </summary>
    public class Vector2D
    {
        /// <summary>
        /// Gets the X-coordinate of the vector.
        /// </summary>
        public float X { get; }

        /// <summary>
        /// Gets the Y-coordinate of the vector.
        /// </summary>
        public float Y { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector2D"/> class.
        /// </summary>
        /// <param name="x">The X-coordinate.</param>
        /// <param name="y">The Y-coordinate.</param>
        public Vector2D(float x, float y)
        {
            X = x;
            Y = y;
        }
    }
}