namespace CoffeeShop
{
    /// <summary>
    /// Represents the abstract base class for all entities in the game that can be considered items,
    /// such as ingredients, prepared drinks, or other holdable/usable objects.
    /// Provides common properties like name and description.
    /// </summary>
    public abstract class Item
    {
        /// <summary>
        /// The display name of the item. This field is read-only after initialization in a derived class constructor.
        /// </summary>
        protected readonly string _name;
        /// <summary>
        /// A textual description of the item. This field is read-only after initialization in a derived class constructor.
        /// While not exposed as a public property in this base class snippet, derived classes may use or expose it.
        /// </summary>
        protected readonly string _description;

        /// <summary>
        /// Gets the name of the item (e.g., "Espresso", "Coffee Bean").
        /// This name is set upon creation and is read-only.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Gets the description of the item.
        /// </summary>
        public string Description => _description;

        /// <summary>
        /// Initializes a new instance of the <see cref="Item"/> abstract class.
        /// This constructor is intended to be called by constructors of derived classes.
        /// </summary>
        /// <param name="name">The name to be assigned to the item. If null or whitespace, defaults to "Unnamed Item".</param>
        /// <param name="description">The description for the item. If null, defaults to an empty string.</param>
        protected Item(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                _name = "Unnamed Item";
            }
            else
            {
                _name = name;
            }
            _description = description ?? string.Empty;
        }
    }
}