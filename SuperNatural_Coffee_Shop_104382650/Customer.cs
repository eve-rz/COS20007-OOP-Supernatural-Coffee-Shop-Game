using Raylib_cs;
using System.Collections.Generic; // Required for List
using System; // Required for Console.WriteLine

namespace CoffeeShop
{
    /// <summary>
    /// Represents an abstract base class for all customer types in the coffee shop.
    /// Defines common properties, states, and behaviors like ordering, patience, and interaction.
    /// Concrete customer types must implement abstract methods for specific behaviors and appearances.
    /// </summary>
    public abstract class Customer
    {
        /// <summary>
        /// A unique identifier for this customer.
        /// </summary>
        protected readonly string _customerID;
        /// <summary>
        /// The customer's current order. Null if no order has been placed yet.
        /// </summary>
        protected Order? _order;
        /// <summary>
        /// The customer's current patience level. Decreases over time when waiting.
        /// </summary>
        protected float _patience;
        /// <summary>
        /// The current state of the customer (e.g., Waiting, Ordering, Consuming).
        /// </summary>
        protected CustomerState _currentState;

        /// <summary>
        /// The current texture used to render the customer.
        /// </summary>
        protected Texture2D _currentSprite;
        /// <summary>
        /// The source rectangle defining the portion of the <see cref="_currentSprite"/> texture to draw.
        /// Used for sprite sheet animations.
        /// </summary>
        protected Rectangle _currentSourceFrameRect;

        /// <summary>
        /// The specific type of this customer (e.g., Human, Alien).
        /// </summary>
        private readonly CustomerType _customerType;
        /// <summary>
        /// The set of dialogue lines associated with this customer type or instance.
        /// </summary>
        private readonly Dialogue _dialogueSet;
        /// <summary>
        /// A prefix used for loading character-specific sprite assets.
        /// </summary>
        private readonly string _spritePrefix;

        /// <summary>
        /// Flag indicating whether the customer's patience is currently actively depleting.
        /// </summary>
        protected bool _isPatienceActive = false;
        /// <summary>
        /// The initial patience level for a new customer.
        /// </summary>
        public const float INITIAL_PATIENCE = 100f;

        /// <summary>
        /// Timer for tracking how long the customer has been in the 'Consuming' state.
        /// </summary>
        private float _consumingTimer;
        /// <summary>
        /// The fixed duration a customer spends consuming their drink.
        /// </summary>
        private const float CONSUMING_DURATION = 3.0f;

        /// <summary>
        /// Gets the unique identifier for this customer.
        /// </summary>
        public string CustomerID
        {
            get { return _customerID; }
        }

        /// <summary>
        /// Gets or sets (protected) the customer's current order.
        /// </summary>
        public Order? Order
        {
            get { return _order; }
            protected set { _order = value; }
        }

        /// <summary>
        /// Gets a value indicating whether the customer's patience has completely run out.
        /// Set internally when patience drops to zero.
        /// </summary>
        public bool HasPatienceRunOut { get; protected set; }

        /// <summary>
        /// Gets or sets (protected) the customer's current patience level.
        /// Ensures patience does not drop below zero.
        /// </summary>
        public float Patience
        {
            get { return _patience; }
            protected set
            {
                if (value < 0) _patience = 0;
                else _patience = value;
            }
        }

        /// <summary>
        /// Gets or sets (protected) the customer's current state.
        /// When the state changes, <see cref="UpdateSpriteForState"/> is called.
        /// </summary>
        public CustomerState CurrentState
        {
            get { return _currentState; }
            protected set
            {
                if (_currentState != value)
                {
                    _currentState = value;
                    UpdateSpriteForState(); // Abstract method, implemented by derived classes
                }
            }
        }

        /// <summary>
        /// Gets the type of this customer.
        /// </summary>
        public CustomerType CustomerType
        {
            get { return _customerType; }
        }

        /// <summary>
        /// Gets the dialogue set for this customer.
        /// </summary>
        public Dialogue DialogueSet
        {
            get { return _dialogueSet; }
        }

        /// <summary>
        /// Gets the sprite prefix used for loading character textures.
        /// </summary>
        public string SpritePrefix
        {
            get { return _spritePrefix; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Customer"/> class.
        /// This constructor is called by derived classes.
        /// </summary>
        /// <param name="id">A unique identifier for the customer. If null or whitespace, a new GUID substring is generated.</param>
        /// <param name="customerType">The type of the customer.</param>
        /// <param name="spritePrefix">The prefix for loading character sprites. Defaults to "default_cust" if null or whitespace.</param>
        /// <param name="dialogueSet">The dialogue set for the customer. If null, a new empty <see cref="Dialogue"/> object is created.</param>
        protected Customer(string id, CustomerType customerType, string spritePrefix, Dialogue dialogueSet)
        {
            _customerID = string.IsNullOrWhiteSpace(id) ? System.Guid.NewGuid().ToString().Substring(0, 8) : id;
            _customerType = customerType;
            _spritePrefix = string.IsNullOrWhiteSpace(spritePrefix) ? "default_cust" : spritePrefix;
            _dialogueSet = dialogueSet ?? new Dialogue(); 

            _isPatienceActive = false;
            HasPatienceRunOut = false;
            _patience = INITIAL_PATIENCE;
            _currentState = CustomerState.Waiting; 
            _currentSprite = new Texture2D { Id = 0 }; 
            _currentSourceFrameRect = new Rectangle(0, 0, 1, 1); // Default small rectangle
            _consumingTimer = 0f;
        }

        /// <summary>
        /// Activates the patience depletion mechanism for this customer.
        /// Patience will start to decrease in relevant states.
        /// </summary>
        public void ActivatePatience()
        {
            if (!_isPatienceActive)
            {
                _isPatienceActive = true;
                Console.WriteLine($"Patience activated for customer {CustomerID}.");
            }
        }

        /// <summary>
        /// Sets the customer's order.
        /// </summary>
        /// <param name="order">The <see cref="Order"/> to assign to the customer.</param>
        public void SetOrder(Order order) { this.Order = order; }

        /// <summary>
        /// Sets the current state of the customer.
        /// Provides base behavior for state transitions, especially handling the 'Consuming' state.
        /// Derived classes can override this to add specific logic for other state transitions.
        /// </summary>
        /// <param name="state">The new <see cref="CustomerState"/> to set.</param>
        public virtual void SetCurrentState(CustomerState state)
        {
            if (_currentState != state) 
            {
                Console.WriteLine($"Customer {CustomerID} state changing from {CurrentState} to {state}.");
                this.CurrentState = state;

                if (this.CurrentState == CustomerState.Consuming) // Check the new state
                {
                    _consumingTimer = CONSUMING_DURATION;
                    _isPatienceActive = false; // Stop patience drain while consuming
                    Console.WriteLine($"Customer {CustomerID} started consuming. Timer set to {CONSUMING_DURATION}s.");
                }
            }
        }

        /// <summary>
        /// Handles the customer receiving a drink.
        /// Typically transitions the customer to the 'Consuming' state.
        /// Derived classes can override this to implement custom reactions to receiving a drink.
        /// </summary>
        /// <param name="drink">The <see cref="Drink"/> received by the customer.</param>
        /// <param name="servedSuccessfully">A boolean indicating if the game logic considers the serving action successful (e.g., correct order).</param>
        public virtual void ReceiveDrink(Drink drink, bool servedSuccessfully)
        {
            if (drink != null && this.Order != null)
            {
                Console.WriteLine($"Customer {CustomerID} in ReceiveDrink for {drink.Name}. ServedSuccessfully reported: {servedSuccessfully}. Actual OrderStatus before Consuming: {this.Order.CurrentStatus}. Setting state to Consuming.");
                SetCurrentState(CustomerState.Consuming); 
            }
            else
            {
                Console.WriteLine($"Customer {CustomerID} cannot ReceiveDrink. Drink null: {drink == null}, Order null: {this.Order == null}");
            }
        }

        /// <summary>
        /// Transitions the customer to the 'Leaving' state.
        /// Derived classes can override this to add specific actions upon leaving.
        /// </summary>
        public virtual void LeaveShop()
        {
            _isPatienceActive = false; // Stop patience mechanics
            SetCurrentState(CustomerState.Leaving); // Use method for state change
            Console.WriteLine($"Customer {CustomerID} is leaving the shop.");
        }

        /// <summary>
        /// When implemented by a derived class, determines the drink order for this customer.
        /// This allows different customer types to have different ordering behaviors.
        /// </summary>
        /// <param name="availableRecipes">A list of <see cref="Recipe"/>s currently available for ordering.</param>
        /// <param name="currentLevelNumber">The current game level number, which might influence the order.</param>
        /// <returns>The <see cref="Recipe"/> the customer wants to order, or null if no suitable order can be made.</returns>
        public abstract Recipe? GetDrinkOrder(List<Recipe> availableRecipes, int currentLevelNumber);

        /// <summary>
        /// When implemented by a derived class, loads all necessary character textures.
        /// </summary>
        /// <param name="basePathForImages">The base directory path where character images are stored.</param>
        /// <param name="characterPrefix">The specific prefix for this character's image files.</param>
        public abstract void LoadCharacterTextures(string basePathForImages, string characterPrefix);

        /// <summary>
        /// When implemented by a derived class, updates the customer's sprite (<see cref="_currentSprite"/> and <see cref="_currentSourceFrameRect"/>)
        /// to match the current <see cref="CurrentState"/>. This is called automatically when <see cref="CurrentState"/> changes.
        /// </summary>
        protected abstract void UpdateSpriteForState();

        /// <summary>
        /// Gets the texture to be displayed when the customer is in a queue or generally visible.
        /// By default, returns the <see cref="_currentSprite"/>. Derived classes can override this if needed.
        /// </summary>
        /// <returns>The <see cref="Texture2D"/> for queue display.</returns>
        public virtual Texture2D GetInQueueTexture()
        {
            return _currentSprite;
        }

        /// <summary>
        /// When implemented by a derived class, gets the sprite sheet texture used for dialogue bubbles or portraits.
        /// </summary>
        /// <returns>The <see cref="Texture2D"/> for dialogue display.</returns>
        public abstract Texture2D GetDialogueSheetTexture();

        /// <summary>
        /// When implemented by a derived class, gets the source rectangle for the current frame of the dialogue sprite.
        /// </summary>
        /// <returns>A <see cref="Rectangle"/> defining the current dialogue frame on its sprite sheet.</returns>
        public abstract Rectangle GetCurrentDialogueFrameSourceRect();

        /// <summary>
        /// When implemented by a derived class, advances the dialogue sprite to its next frame (if animated).
        /// </summary>
        public abstract void CycleDialogueFrame();

        /// <summary>
        /// When implemented by a derived class, resets the dialogue sprite animation to its initial frame.
        /// </summary>
        public abstract void ResetDialogueFrame();

        /// <summary>
        /// When implemented by a derived class, unloads all textures used by this customer to free resources.
        /// </summary>
        public abstract void UnloadTextures();

        /// <summary>
        /// Flag indicating whether textures for this customer have been successfully loaded.
        /// Should be managed by implementations of <see cref="LoadCharacterTextures"/> and <see cref="UnloadTextures"/>.
        /// </summary>
        protected bool _texturesLoaded = false;

        /// <summary>
        /// Updates the customer's state, handling consuming timer or patience depletion, with access to the <see cref="GameManager"/>.
        /// This is the preferred version for updating patience due to its ability to interact with global game state if needed.
        /// </summary>
        /// <param name="deltaTime">The time elapsed since the last frame, in seconds.</param>
        /// <param name="gameManager">A reference to the <see cref="GameManager"/> for potential interactions (e.g., scoring, events).</param>
        public virtual void UpdatePatience(float deltaTime, GameManager gameManager)
        {
            if (CurrentState == CustomerState.Leaving) return;

            if (CurrentState == CustomerState.Consuming)
            {
                _consumingTimer -= deltaTime;
                if (_consumingTimer <= 0)
                {
                    Console.WriteLine($"Customer {CustomerID} ({this.GetType().Name}) finished consuming. Now leaving.");
                    LeaveShop();
                }
            }
            else if (_isPatienceActive && (CurrentState == CustomerState.Ordering || CurrentState == CustomerState.WaitingForDrink || CurrentState == CustomerState.Waiting))
            {
                Patience -= 1.0f * deltaTime;
                if (Patience <= 0 && !HasPatienceRunOut)
                {
                    HasPatienceRunOut = true;
                    OnPatienceDepleted(gameManager);
                    if (this.Order != null) this.Order.UpdateStatus(OrderStatus.Failed);
                    LeaveShop();
                }
            }
        }

        /// <summary>
        /// Called when the customer's patience has been fully depleted.
        /// Base implementation logs a message. Derived classes can override this to implement
        /// specific consequences or behaviors (e.g., affecting player score, triggering events).
        /// </summary>
        /// <param name="gameManager">A reference to the <see cref="GameManager"/> for broader game interactions.</param>
        protected virtual void OnPatienceDepleted(GameManager gameManager)
        {
            Console.WriteLine($"Customer {CustomerID} ({this.GetType().Name}) OnPatienceDepleted: No special effect by default.");
        }
    }
}