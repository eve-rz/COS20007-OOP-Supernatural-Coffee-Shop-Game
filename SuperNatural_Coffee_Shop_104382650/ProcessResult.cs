namespace CoffeeShop
{
    /// <summary>
    /// Encapsulates the result of an operation, such as preparing a drink.
    /// </summary>
    public class ProcessResult
    {
        private readonly bool _wasSuccessful;
        private readonly Drink? _preparedDrink; 
        private readonly string _outcomeReason;

        /// <summary>
        /// Gets a value indicating whether the process was successful.
        /// </summary>
        public bool WasSuccessful => _wasSuccessful;

        /// <summary>
        /// Gets the drink that was prepared, if the process was successful and produced a drink.
        /// May be null if the process failed or did not result in a drink.
        /// </summary>
        public Drink? PreparedDrink => _preparedDrink;

        /// <summary>
        /// Gets a message describing the outcome or reason for failure.
        /// </summary>
        public string OutcomeReason => _outcomeReason;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessResult"/> class.
        /// </summary>
        /// <param name="success">Whether the process was successful.</param>
        /// <param name="preparedDrink">The drink that was prepared (can be null).</param>
        /// <param name="reason">A message describing the outcome.</param>
        public ProcessResult(bool success, Drink? preparedDrink, string reason)
        {
            _wasSuccessful = success;
            _preparedDrink = preparedDrink; 
            _outcomeReason = reason ?? string.Empty;

            if (!_wasSuccessful && _preparedDrink != null)
            {

            }
        }
    }
}