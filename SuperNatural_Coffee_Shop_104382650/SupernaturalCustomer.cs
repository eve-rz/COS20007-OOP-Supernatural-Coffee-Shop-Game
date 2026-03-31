using Raylib_cs;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoffeeShop
{
    /// <summary>
    /// Represents a supernatural customer in the coffee shop, inheriting from the base <see cref="Customer"/> class.
    /// Supernatural customers have unique types (e.g., Ghost, Alien), moods, and may have special demands
    /// or trigger environmental effects. They also have distinct visual representations and behaviors.
    /// </summary>
    public class SupernaturalCustomer : Customer
    {
        /// <summary>
        /// The specific type of this supernatural customer (e.g., Ghost, Alien). This is read-only after creation.
        /// </summary>
        private readonly SupernaturalCustomerType _type;
        /// <summary>
        /// The current mood of the supernatural customer, which can affect behavior or interactions.
        /// </summary>
        private CustomerMood _mood;
        /// <summary>
        /// Any special demands or unique requests this supernatural customer might have. This is read-only after creation.
        /// </summary>
        private readonly string _specialDemands;

        /// <summary>
        /// Texture for the animated dialogue/portrait sheet specific to this supernatural customer.
        /// </summary>
        private Texture2D _dialogueSheetTexture_SC;
        /// <summary>
        /// Array of rectangles defining individual frames on the <see cref="_dialogueSheetTexture_SC"/>.
        /// </summary>
        private Rectangle[] _dialogueSheetFrames_SC;
        /// <summary>
        /// Index of the current frame being displayed from <see cref="_dialogueSheetFrames_SC"/>.
        /// </summary>
        private int _currentDialogueFrameIndex_SC;

        /// <summary>
        /// Texture for the 'Hi/Waiting' state of this supernatural customer.
        /// </summary>
        private Texture2D _texHi_SC;
        /// <summary>
        /// Texture for the 'Ordering' state of this supernatural customer.
        /// </summary>
        private Texture2D _texOrder_SC;
        /// <summary>
        /// Texture for the 'Order Complete/Successful' state of this supernatural customer.
        /// </summary>
        private Texture2D _texComplete_SC;
        /// <summary>
        /// Texture for the 'Order Failed' state of this supernatural customer.
        /// </summary>
        private Texture2D _texFailed_SC;

        private const int TOYOL_TOTAL_FRAMES = 6;
        private const int TOYOL_SHEET_COLS = 2;
        private const int TOYOL_SHEET_ROWS = 3;

        private const int FIREMONSTER_DIALOGUE_TOTAL_FRAMES = 6;
        private const int FIREMONSTER_DIALOGUE_COLS = 2;
        private const int FIREMONSTER_DIALOGUE_ROWS = 3;

        private const int GHOST_DIALOGUE_TOTAL_FRAMES = 6;
        private const int GHOST_DIALOGUE_COLS = 2;
        private const int GHOST_DIALOGUE_ROWS = 3;

        private const int ALIEN_DIALOGUE_TOTAL_FRAMES = 6;
        private const int ALIEN_DIALOGUE_COLS = 2;
        private const int ALIEN_DIALOGUE_ROWS = 3;

        /// <summary>
        /// Calculated width of a single frame in the character's dialogue animation spritesheet.
        /// </summary>
        private float _characterFrameWidth;
        /// <summary>
        /// Calculated height of a single frame in the character's dialogue animation spritesheet.
        /// </summary>
        private float _characterFrameHeight;

        /// <summary>
        /// Gets the array of <see cref="Rectangle"/> defining the frames for the dialogue animation.
        /// Can be null if textures are not loaded or if there are no frames.
        /// </summary>
        /// <returns>An array of <see cref="Rectangle"/> for dialogue frames, or null.</returns>
        public Rectangle[]? GetDialogueSheetFrames() { return _dialogueSheetFrames_SC; }

        /// <summary>
        /// Static random number generator for decisions specific to supernatural customers, like order choices.
        /// </summary>
        private static Random _random = new Random();

        /// <summary>
        /// Gets the specific <see cref="SupernaturalCustomerType"/> of this customer.
        /// </summary>
        public SupernaturalCustomerType Type => _type;

        /// <summary>
        /// Gets or protected sets the current <see cref="CustomerMood"/> of this supernatural customer.
        /// Changing the mood will also trigger an update to the customer's sprite.
        /// </summary>
        public CustomerMood Mood
        {
            get => _mood;
            protected set
            {
                if (_mood != value)
                {
                    _mood = value;
                    UpdateSpriteForState(); // Update sprite when mood changes
                }
            }
        }
        /// <summary>
        /// Gets any special demands this supernatural customer might have, often related to unique order properties.
        /// </summary>
        public string SpecialDemands => _specialDemands;

        /// <summary>
        /// Initializes a new instance of the <see cref="SupernaturalCustomer"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for this customer.</param>
        /// <param name="spritePrefix">The prefix used for loading sprite assets (e.g., "ghost", "alien").</param>
        /// <param name="dialogueSet">The <see cref="Dialogue"/> object containing lines for this customer.</param>
        /// <param name="type">The specific <see cref="SupernaturalCustomerType"/> of this customer.</param>
        /// <param name="initialMood">The initial <see cref="CustomerMood"/> of the customer. Defaults to <see cref="CustomerMood.Calm"/>.</param>
        /// <param name="specialDemands">Any special demands or requests the customer might have. Defaults to an empty string.</param>
        public SupernaturalCustomer(string id, string spritePrefix, Dialogue dialogueSet, SupernaturalCustomerType type, CustomerMood initialMood = CustomerMood.Calm, string specialDemands = "")
            : base(id, CustomerType.DefaultNormal, spritePrefix, dialogueSet)
        {
            _type = type;
            _mood = initialMood;
            _specialDemands = specialDemands ?? string.Empty;

            _dialogueSheetTexture_SC = new Texture2D { Id = 0 };
            _dialogueSheetFrames_SC = new Rectangle[1] { new Rectangle(0, 0, 1, 1) }; 
            _currentDialogueFrameIndex_SC = 0;

            _texHi_SC = new Texture2D { Id = 0 };
            _texOrder_SC = new Texture2D { Id = 0 };
            _texComplete_SC = new Texture2D { Id = 0 };
            _texFailed_SC = new Texture2D { Id = 0 };
        }

        /// <summary>
        /// Sets the mood of the supernatural customer.
        /// </summary>
        /// <param name="newMood">The new <see cref="CustomerMood"/> to set.</param>
        public void SetMood(CustomerMood newMood)
        {
            this.Mood = newMood;
            Console.WriteLine($"SupernaturalCustomer {CustomerID} ({Type}) mood changed to: {newMood}");
        }

        /// <summary>
        /// Determines and returns the drink <see cref="Recipe"/> that this supernatural customer wishes to order.
        /// Behavior varies significantly based on the <see cref="Type"/> of supernatural customer:
        /// - Toyols do not place recipe orders.
        /// - Aliens state property demands directly (this method returns null for them, demand handled elsewhere).
        /// - Ghosts may prefer simple, non-supernatural coffee.
        /// - FireMonsters may prefer hot, non-supernatural coffee-based drinks.
        /// - Other types may prefer supernatural recipes if available, or fallback to normal ones.
        /// </summary>
        /// <param name="availableRecipes">A list of all <see cref="Recipe"/>s currently available in the game.</param>
        /// <param name="currentLevelNumber">The current game level number, used to filter recipes by their unlock level.</param>
        /// <returns>The chosen <see cref="Recipe"/> if a suitable one is found; otherwise, <c>null</c>.</returns>
        public override Recipe? GetDrinkOrder(List<Recipe> availableRecipes, int currentLevelNumber)
        {
            Console.WriteLine($"SupernaturalCustomer {CustomerID} ({Type}) deciding order. Level: {currentLevelNumber}.");

            if (this.Type == SupernaturalCustomerType.Toyol || this.Type == SupernaturalCustomerType.Alien)
            {
                return null; 
            }

            var suitableRecipes = availableRecipes.Where(r => r.UnlockLevel <= currentLevelNumber).ToList();
            Console.WriteLine($"[DEBUG] Suitable recipes for {Type}: " + string.Join(", ", suitableRecipes.Select(r => r.RecipeName)));

            if (!suitableRecipes.Any())
            {
                Console.WriteLine($"Warning: Customer {CustomerID} has no recipes unlocked for level {currentLevelNumber}.");
                return null;
            }

            if (this.Type == SupernaturalCustomerType.FireMonster)
            {
                var fireyBrewRecipe = suitableRecipes.FirstOrDefault(r => r.RecipeName == "Firey Brew");
                if (fireyBrewRecipe != null)
                {
                    Console.WriteLine($"[LOGIC] FireMonster ({CustomerID}) found its favorite: 'Firey Brew'. Ordering it.");
                    return fireyBrewRecipe;
                }

                var hotDrinks = suitableRecipes.Where(r => !r.IsSupernatural &&
                                                    (r.RecipeName.Contains("Espresso") ||
                                                    r.RecipeName.Contains("Coffee") ||
                                                    r.RecipeName.Contains("Latte"))).ToList();
                if (hotDrinks.Any())
                {
                    Console.WriteLine($"[LOGIC] FireMonster ({CustomerID}) did not find 'Firey Brew'. Falling back to a normal hot drink.");
                    return hotDrinks[_random.Next(hotDrinks.Count)];
                }
            }
            else if (this.Type == SupernaturalCustomerType.Ghost)
            {
                var shadowBrewRecipe = suitableRecipes.FirstOrDefault(r => r.RecipeName == "Shadow Brew");
                if (shadowBrewRecipe != null)
                {
                    Console.WriteLine($"[LOGIC] Ghost ({CustomerID}) found its favorite: 'Shadow Brew'. Ordering it.");
                    return shadowBrewRecipe;
                }

                var ghostPreferredSimple = suitableRecipes.FirstOrDefault(r =>
                    !r.IsSupernatural &&
                    r.RequiredIngredients.Count == 1 &&
                    r.RequiredIngredients.Keys.Any(k => k.Type == IngredientType.CoffeeBean)
                );
                if (ghostPreferredSimple != null)
                {
                    Console.WriteLine($"[LOGIC] Ghost ({CustomerID}) did not find 'Shadow Brew'. Falling back to a simple coffee.");
                    return ghostPreferredSimple;
                }
            }

            var anySupernaturalRecipe = suitableRecipes.Where(r => r.IsSupernatural).ToList();
            if (anySupernaturalRecipe.Any())
            {
                Console.WriteLine($"[LOGIC] Customer {CustomerID} ({Type}) found no specific favorites. Falling back to a generic supernatural drink.");
                return anySupernaturalRecipe[_random.Next(anySupernaturalRecipe.Count)];
            }

            var anyNormalRecipe = suitableRecipes.Where(r => !r.IsSupernatural).ToList();
            if (anyNormalRecipe.Any())
            {
                Console.WriteLine($"[LOGIC] Customer {CustomerID} ({Type}) found no preferred or supernatural drinks. Falling back to any normal recipe.");
                return anyNormalRecipe[_random.Next(anyNormalRecipe.Count)];
            }
            
            Console.WriteLine($"[LOGIC] Customer {CustomerID} ({Type}) could not find any suitable recipe after all checks.");
            return null; 
        }

        /// <summary>
        /// Triggers an environmental effect based on the supernatural customer's type and mood.
        /// For example, an agitated FireMonster might cause the brewing station to overheat.
        /// </summary>
        /// <param name="layout">The current <see cref="ShopLayout"/>, not directly used in this implementation but available for future effects.</param>
        /// <param name="gameManager">The <see cref="GameManager"/> instance to interact with game systems (e.g., setting station state).</param>
        public void TriggerEnvironmentalEffect(ShopLayout layout, GameManager gameManager)
        {
            Console.WriteLine($"SupernaturalCustomer {CustomerID} ({Type}) is triggering an environmental effect. Mood: {Mood}");
            if (this.Type == SupernaturalCustomerType.FireMonster &&
                (this.Mood == CustomerMood.Agitated || this.Mood == CustomerMood.Hostile)) 
            {
                if (!gameManager.IsBrewingStationOverheated) 
                {
                    gameManager.SetBrewingStationOverheated(true, true); 
                }
            }
        }

        /// <summary>
        /// Loads the character-specific textures for this supernatural customer.
        /// This includes textures for different states (hi, order, complete, failed) and an animated dialogue spritesheet.
        /// The specific frames for dialogue animations are calculated based on constants for each supernatural type.
        /// </summary>
        /// <param name="basePathForImages">The root directory path where character image assets are stored.</param>
        /// <param name="characterPrefixParam">The specific file name prefix for this customer's textures (e.g., "ghost", "alien").
        /// Note: This parameter is currently overridden by the customer's internal <see cref="Customer.SpritePrefix"/>.</param>
        public override void LoadCharacterTextures(string basePathForImages, string characterPrefixParam)
        {
            string effectivePrefix = this.SpritePrefix; // Use the prefix set during construction
            Console.WriteLine($"DEBUG: SC.LoadCharacterTextures for Type: {this.Type}, effectivePrefix: '{effectivePrefix}'");
            _texturesLoaded = false;
            string customerSubFolder = Path.Combine(basePathForImages, "Customer");

            _texHi_SC = new Texture2D { Id = 0 }; _texOrder_SC = new Texture2D { Id = 0 };
            _texComplete_SC = new Texture2D { Id = 0 }; _texFailed_SC = new Texture2D { Id = 0 };
            _dialogueSheetTexture_SC = new Texture2D { Id = 0 };
            _dialogueSheetFrames_SC = new Rectangle[1] { new Rectangle(0, 0, 1, 1) }; 

            if (this.Type == SupernaturalCustomerType.Toyol)
            {
                bool essentialToyolTexturesLoaded = true;
                _texHi_SC = Raylib.LoadTexture(Path.Combine(customerSubFolder, $"{effectivePrefix}_hi.png"));
                if (_texHi_SC.Id == 0) { Console.WriteLine($"WARNING: Toyol FAILED to load '{effectivePrefix}_hi.png'"); essentialToyolTexturesLoaded = false; }
                else { Console.WriteLine($"SUCCESS: Toyol loaded _texHi_SC (ID: {_texHi_SC.Id})"); }

                _texOrder_SC = Raylib.LoadTexture(Path.Combine(customerSubFolder, $"{effectivePrefix}_order.png"));
                if (_texOrder_SC.Id == 0) { Console.WriteLine($"WARNING: Toyol FAILED to load '{effectivePrefix}_order.png'"); essentialToyolTexturesLoaded = false; }
                else { Console.WriteLine($"SUCCESS: Toyol loaded _texOrder_SC (ID: {_texOrder_SC.Id})"); }

                string animatedSpritePath = Path.Combine(customerSubFolder, $"{effectivePrefix}.png");
                _dialogueSheetTexture_SC = Raylib.LoadTexture(animatedSpritePath);
                if (_dialogueSheetTexture_SC.Id != 0)
                {
                    Console.WriteLine($"SUCCESS: Toyol ANIMATED dialogue sheet '{animatedSpritePath}' loaded (ID: {_dialogueSheetTexture_SC.Id}). Dimensions: {_dialogueSheetTexture_SC.Width}x{_dialogueSheetTexture_SC.Height}");
                    _characterFrameWidth = (float)_dialogueSheetTexture_SC.Width / TOYOL_SHEET_COLS;
                    _characterFrameHeight = (float)_dialogueSheetTexture_SC.Height / TOYOL_SHEET_ROWS;
                    _dialogueSheetFrames_SC = new Rectangle[TOYOL_TOTAL_FRAMES];
                    int frameIndex = 0;
                    for (int y = 0; y < TOYOL_SHEET_ROWS && frameIndex < TOYOL_TOTAL_FRAMES; y++)
                    {
                        for (int x = 0; x < TOYOL_SHEET_COLS && frameIndex < TOYOL_TOTAL_FRAMES; x++)
                        {
                            _dialogueSheetFrames_SC[frameIndex++] = new Rectangle(x * _characterFrameWidth, y * _characterFrameHeight, _characterFrameWidth, _characterFrameHeight);
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"WARNING: Failed to load Toyol ANIMATED dialogue sheet from '{animatedSpritePath}'.");
                    essentialToyolTexturesLoaded = false; 
                }
                _texturesLoaded = essentialToyolTexturesLoaded;
            }
            else if (this.Type == SupernaturalCustomerType.FireMonster)
            {
                _texHi_SC = Raylib.LoadTexture(Path.Combine(customerSubFolder, $"{effectivePrefix}_hi.png"));
                _texOrder_SC = Raylib.LoadTexture(Path.Combine(customerSubFolder, $"{effectivePrefix}_order.png"));
                _texComplete_SC = Raylib.LoadTexture(Path.Combine(customerSubFolder, $"{effectivePrefix}_complete.png"));
                _texFailed_SC = Raylib.LoadTexture(Path.Combine(customerSubFolder, $"{effectivePrefix}_failed.png"));
                _dialogueSheetTexture_SC = Raylib.LoadTexture(Path.Combine(customerSubFolder, $"{effectivePrefix}_dialogue.png"));

                _texturesLoaded = _texHi_SC.Id != 0 && _texOrder_SC.Id != 0 && _texComplete_SC.Id != 0 && _texFailed_SC.Id != 0;

                if (_dialogueSheetTexture_SC.Id != 0)
                {
                    _characterFrameWidth = (float)_dialogueSheetTexture_SC.Width / FIREMONSTER_DIALOGUE_COLS; 
                    _characterFrameHeight = (float)_dialogueSheetTexture_SC.Height / FIREMONSTER_DIALOGUE_ROWS;
                    _dialogueSheetFrames_SC = new Rectangle[FIREMONSTER_DIALOGUE_TOTAL_FRAMES];
                    int frameIndex = 0;
                    for (int row = 0; row < FIREMONSTER_DIALOGUE_ROWS && frameIndex < FIREMONSTER_DIALOGUE_TOTAL_FRAMES; row++)
                    {
                        for (int col = 0; col < FIREMONSTER_DIALOGUE_COLS && frameIndex < FIREMONSTER_DIALOGUE_TOTAL_FRAMES; col++)
                        {
                            _dialogueSheetFrames_SC[frameIndex] = new Rectangle(
                                col * _characterFrameWidth, row * _characterFrameHeight,
                                _characterFrameWidth, _characterFrameHeight);
                            frameIndex++;
                        }
                    }
                }
                else { Console.WriteLine($"WARNING: Failed to load dialogue sheet for {effectivePrefix}."); }

                if (!_texturesLoaded) Console.WriteLine($"WARNING: One or more state textures failed to load for {effectivePrefix}.");
                else Console.WriteLine($"FireMonster '{effectivePrefix}' state textures and dialogue sheet loaded.");
            }
            else if (this.Type == SupernaturalCustomerType.Ghost)
            {
                _texHi_SC = Raylib.LoadTexture(Path.Combine(customerSubFolder, $"{effectivePrefix}_hi.png"));
                _texOrder_SC = Raylib.LoadTexture(Path.Combine(customerSubFolder, $"{effectivePrefix}_order.png"));
                _texComplete_SC = Raylib.LoadTexture(Path.Combine(customerSubFolder, $"{effectivePrefix}_complete.png"));
                _texFailed_SC = Raylib.LoadTexture(Path.Combine(customerSubFolder, $"{effectivePrefix}_failed.png"));
                _dialogueSheetTexture_SC = Raylib.LoadTexture(Path.Combine(customerSubFolder, $"{effectivePrefix}_dialogue.png"));

                _texturesLoaded = _texHi_SC.Id != 0 && _texOrder_SC.Id != 0 && _texComplete_SC.Id != 0 && _texFailed_SC.Id != 0;

                if (_dialogueSheetTexture_SC.Id != 0)
                {
                    _characterFrameWidth = (float)_dialogueSheetTexture_SC.Width / GHOST_DIALOGUE_COLS; 
                    _characterFrameHeight = (float)_dialogueSheetTexture_SC.Height / GHOST_DIALOGUE_ROWS; 
                    _dialogueSheetFrames_SC = new Rectangle[GHOST_DIALOGUE_TOTAL_FRAMES];
                    int frameIndex = 0;
                    for (int row = 0; row < GHOST_DIALOGUE_ROWS && frameIndex < GHOST_DIALOGUE_TOTAL_FRAMES; row++)
                    {
                        for (int col = 0; col < GHOST_DIALOGUE_COLS && frameIndex < GHOST_DIALOGUE_TOTAL_FRAMES; col++)
                        {
                            _dialogueSheetFrames_SC[frameIndex] = new Rectangle(
                                col * _characterFrameWidth, row * _characterFrameHeight,
                                _characterFrameWidth, _characterFrameHeight);
                            frameIndex++;
                        }
                    }
                }
                else { Console.WriteLine($"WARNING: Failed to load dialogue sheet for {effectivePrefix}."); }

                if (!_texturesLoaded) Console.WriteLine($"WARNING: One or more state textures failed to load for {effectivePrefix}.");
                else Console.WriteLine($"Ghost '{effectivePrefix}' state textures and dialogue sheet loaded.");
            }
            else if (this.Type == SupernaturalCustomerType.Alien)
            {
                _texHi_SC = Raylib.LoadTexture(Path.Combine(customerSubFolder, $"{effectivePrefix}_hi.png"));
                _texOrder_SC = Raylib.LoadTexture(Path.Combine(customerSubFolder, $"{effectivePrefix}_order.png"));
                _texComplete_SC = Raylib.LoadTexture(Path.Combine(customerSubFolder, $"{effectivePrefix}_complete.png"));
                _texFailed_SC = Raylib.LoadTexture(Path.Combine(customerSubFolder, $"{effectivePrefix}_failed.png"));
                _dialogueSheetTexture_SC = Raylib.LoadTexture(Path.Combine(customerSubFolder, $"{effectivePrefix}_dialogue.png"));

                _texturesLoaded = _texHi_SC.Id != 0 && _texOrder_SC.Id != 0 && _texComplete_SC.Id != 0 && _texFailed_SC.Id != 0;

                if (_dialogueSheetTexture_SC.Id != 0)
                {
                    _characterFrameWidth = (float)_dialogueSheetTexture_SC.Width / ALIEN_DIALOGUE_COLS; 
                    _characterFrameHeight = (float)_dialogueSheetTexture_SC.Height / ALIEN_DIALOGUE_ROWS; 
                    _dialogueSheetFrames_SC = new Rectangle[ALIEN_DIALOGUE_TOTAL_FRAMES];
                    int frameIndex = 0;
                    for (int row = 0; row < ALIEN_DIALOGUE_ROWS && frameIndex < ALIEN_DIALOGUE_TOTAL_FRAMES; row++)
                    {
                        for (int col = 0; col < ALIEN_DIALOGUE_COLS && frameIndex < ALIEN_DIALOGUE_TOTAL_FRAMES; col++)
                        {
                            _dialogueSheetFrames_SC[frameIndex] = new Rectangle(
                                col * _characterFrameWidth, row * _characterFrameHeight,
                                _characterFrameWidth, _characterFrameHeight);
                            frameIndex++;
                        }
                    }
                }
                else { Console.WriteLine($"WARNING: Failed to load dialogue sheet for {effectivePrefix}."); }

                if (!_texturesLoaded) Console.WriteLine($"WARNING: One or more state textures failed to load for {effectivePrefix}.");
                else Console.WriteLine($"Alien '{effectivePrefix}' state textures and dialogue sheet loaded.");
            }
            else
            {
                Console.WriteLine($"DEBUG: SupernaturalCustomer.LoadCharacterTextures called for unhandled Type: {this.Type}, effectivePrefix: '{effectivePrefix}', base Customer SpritePrefix: '{this.SpritePrefix}''");
                _texturesLoaded = false; 
            }
            UpdateSpriteForState(); 
        }

        /// <summary>
        /// Updates the customer's current sprite (<see cref="Customer._currentSprite"/>) and source rectangle
        /// (<see cref="Customer._currentSourceFrameRect"/>) based on their current <see cref="Customer.CurrentState"/>
        /// and <see cref="Type"/>. This method selects from textures specific to the supernatural customer.
        /// </summary>
        protected override void UpdateSpriteForState()
        {
            if (!_texturesLoaded)
            {
                _currentSprite = new Texture2D { Id = 0 }; 
                _currentSourceFrameRect = new Rectangle(0, 0, 1, 1);
                return;
            }

            if (this.Type == SupernaturalCustomerType.Toyol)
            {
                switch (CurrentState)
                {
                    case CustomerState.Waiting:
                        _currentSprite = (_texHi_SC.Id != 0) ? _texHi_SC : _dialogueSheetTexture_SC;
                        break;
                    case CustomerState.Ordering: 
                        _currentSprite = (_texOrder_SC.Id != 0) ? _texOrder_SC : _dialogueSheetTexture_SC;
                        break;
                    case CustomerState.Leaving: 
                        _currentSprite = (_texHi_SC.Id != 0) ? _texHi_SC : _dialogueSheetTexture_SC; 
                        break;
                    default: 
                        _currentSprite = (_texHi_SC.Id != 0) ? _texHi_SC : _dialogueSheetTexture_SC;
                        break;
                }

                if (_currentSprite.Id == _texHi_SC.Id || _currentSprite.Id == _texOrder_SC.Id) 
                {
                    if (_currentSprite.Id != 0) 
                    {
                        _currentSourceFrameRect = new Rectangle(0, 0, _currentSprite.Width, _currentSprite.Height);
                    }
                    else 
                    {
                        _currentSourceFrameRect = new Rectangle(0,0,1,1);
                    }
                } 
                else if (_currentSprite.Id == _dialogueSheetTexture_SC.Id) 
                { 
                    if (_dialogueSheetFrames_SC != null && _dialogueSheetFrames_SC.Length > 0 &&
                        _currentDialogueFrameIndex_SC >= 0 && _currentDialogueFrameIndex_SC < _dialogueSheetFrames_SC.Length)
                    {
                        _currentSourceFrameRect = _dialogueSheetFrames_SC[_currentDialogueFrameIndex_SC];
                    } else { 
                        _currentSourceFrameRect = (_dialogueSheetFrames_SC != null && _dialogueSheetFrames_SC.Length > 0) ? _dialogueSheetFrames_SC[0] : new Rectangle(0,0,1,1);
                    }
                } else { 
                    _currentSprite = new Texture2D{Id = 0}; 
                    _currentSourceFrameRect = new Rectangle(0,0,1,1);
                }
            }
            else if (this.Type == SupernaturalCustomerType.FireMonster || this.Type == SupernaturalCustomerType.Ghost || this.Type == SupernaturalCustomerType.Alien)
            {
                switch (CurrentState)
                {
                    case CustomerState.Waiting:
                    case CustomerState.WaitingForDrink:
                        _currentSprite = _texHi_SC;
                        break;
                    case CustomerState.Ordering:
                        _currentSprite = _texOrder_SC;
                        break;
                    case CustomerState.Consuming:
                        if (this.Order != null)
                        {
                            _currentSprite = (this.Order.CurrentStatus == OrderStatus.Served) ? _texComplete_SC : _texFailed_SC;
                        }
                        else { _currentSprite = _texHi_SC; } 
                        break;
                    case CustomerState.Leaving:
                        _currentSprite = (this.Order != null && this.Order.CurrentStatus == OrderStatus.Served) ? _texComplete_SC : _texFailed_SC;
                        break;
                    default:
                        _currentSprite = _texHi_SC;
                        break;
                }
                if (_currentSprite.Id == 0 && _texHi_SC.Id != 0) _currentSprite = _texHi_SC; 
                
                if(_currentSprite.Id != 0) 
                    _currentSourceFrameRect = new Rectangle(0, 0, _currentSprite.Width, _currentSprite.Height);
                else 
                    _currentSourceFrameRect = new Rectangle(0,0,1,1);
            }
            else 
            {
                _currentSprite = new Texture2D { Id = 0 };
                _currentSourceFrameRect = new Rectangle(0, 0, 1, 1);
            }
        }

        /// <summary>
        /// Gets the sprite sheet texture used for displaying this supernatural customer's dialogue expressions.
        /// </summary>
        /// <returns>The <see cref="Texture2D"/> for the dialogue sprite sheet.</returns>
        public override Texture2D GetDialogueSheetTexture()
        {
            return _dialogueSheetTexture_SC;
        }

        /// <summary>
        /// Gets the source <see cref="Rectangle"/> for the current dialogue expression frame
        /// from this supernatural customer's dialogue sprite sheet.
        /// </summary>
        /// <returns>The source rectangle for the current dialogue frame, or a default 1x1 rectangle if not available.</returns>
        public override Rectangle GetCurrentDialogueFrameSourceRect()
        {
            if (!_texturesLoaded || _dialogueSheetTexture_SC.Id == 0 || _dialogueSheetFrames_SC == null || _dialogueSheetFrames_SC.Length == 0)
            {
                return new Rectangle(0, 0, 1, 1);
            }

            int frameCount = _dialogueSheetFrames_SC.Length; 
            int idx = _currentDialogueFrameIndex_SC;
            if (idx < 0 || idx >= frameCount) idx = 0; 

            return _dialogueSheetFrames_SC[idx];
        }

        /// <summary>
        /// Advances the dialogue expression to the next frame in a cyclic manner,
        /// using the appropriate total frame count based on the supernatural customer type.
        /// For Toyol, this also triggers an update to its main sprite if it uses the dialogue sheet for its current state.
        /// </summary>
        public override void CycleDialogueFrame() 
        {
            if (!_texturesLoaded || _dialogueSheetTexture_SC.Id == 0 || _dialogueSheetFrames_SC == null || _dialogueSheetFrames_SC.Length == 0) return;

            int frameCount = _dialogueSheetFrames_SC.Length; 

            if (this.Type == SupernaturalCustomerType.Toyol)
            {
                frameCount = TOYOL_TOTAL_FRAMES; 
            }
            else if (this.Type == SupernaturalCustomerType.FireMonster)
            {
                frameCount = FIREMONSTER_DIALOGUE_TOTAL_FRAMES; 
            }
            else if (this.Type == SupernaturalCustomerType.Ghost)
            {
                frameCount = GHOST_DIALOGUE_TOTAL_FRAMES;
            }
             else if (this.Type == SupernaturalCustomerType.Alien)
            {
                frameCount = ALIEN_DIALOGUE_TOTAL_FRAMES;
            }


            if (frameCount > 0) 
            {
                _currentDialogueFrameIndex_SC = (_currentDialogueFrameIndex_SC + 1) % frameCount;
            }

            if (this.Type == SupernaturalCustomerType.Toyol && _currentSprite.Id == _dialogueSheetTexture_SC.Id)
            {
                UpdateSpriteForState();
            }
        }

        /// <summary>
        /// Resets the dialogue expression to its initial frame (index 0).
        /// For Toyol, this also triggers an update to its main sprite if it uses the dialogue sheet for its current state.
        /// </summary>
        public override void ResetDialogueFrame()
        {
            _currentDialogueFrameIndex_SC = 0;
            if (this.Type == SupernaturalCustomerType.Toyol && _currentSprite.Id == _dialogueSheetTexture_SC.Id)
            {
                UpdateSpriteForState();
            }
        }

        /// <summary>
        /// Unloads all textures specifically loaded for this supernatural customer to free up graphics memory.
        /// Resets texture fields to their initial empty state.
        /// </summary>
        public override void UnloadTextures()
        {
            Console.WriteLine($"SupernaturalCustomer.UnloadTextures for {CustomerID} ({Type}).");
            if (_dialogueSheetTexture_SC.Id != 0) Raylib.UnloadTexture(_dialogueSheetTexture_SC);

            if (_texHi_SC.Id != 0) Raylib.UnloadTexture(_texHi_SC);
            if (_texOrder_SC.Id != 0) Raylib.UnloadTexture(_texOrder_SC);
            if (_texComplete_SC.Id != 0) Raylib.UnloadTexture(_texComplete_SC);
            if (_texFailed_SC.Id != 0) Raylib.UnloadTexture(_texFailed_SC);

            _texturesLoaded = false;
            _dialogueSheetTexture_SC = new Texture2D { Id = 0 };
            _texHi_SC = new Texture2D { Id = 0 };
            _texOrder_SC = new Texture2D { Id = 0 };
            _texComplete_SC = new Texture2D { Id = 0 };
            _texFailed_SC = new Texture2D { Id = 0 };
        }

        /// <summary>
        /// Handles the event of this supernatural customer receiving a drink.
        /// This implementation primarily logs the event and calls the base class's <see cref="Customer.ReceiveDrink"/> method.
        /// </summary>
        /// <param name="drink">The <see cref="Drink"/> object received by the customer.</param>
        /// <param name="servedSuccessfully">A boolean indicating if the game logic considers the serving action successful (e.g., correct order).</param>
        public override void ReceiveDrink(Drink drink, bool servedSuccessfully)
        {
            Console.WriteLine($"SupernaturalCustomer {this.CustomerID} ({this.Type}) processing ReceiveDrink (servedSuccessfully: {servedSuccessfully}).");
            base.ReceiveDrink(drink, servedSuccessfully); 
        }
        
        /// <summary>
        /// Called when the supernatural customer's patience has been fully depleted.
        /// Triggers specific effects based on the customer type (e.g., Ghost interference, FireMonster environmental effect).
        /// </summary>
        /// <param name="gameManager">A reference to the <see cref="GameManager"/> for broader game interactions.</param>
        protected override void OnPatienceDepleted(GameManager gameManager)
        {
            Console.WriteLine($"SupernaturalCustomer {CustomerID} ({Type}) OnPatienceDepleted hook called.");
            if (this.Type == SupernaturalCustomerType.Ghost)
            {
                Console.WriteLine($"Ghost {CustomerID} patience depleted, activating interference.");
                gameManager.ActivateGhostlyInterference(duration: 30.0f, dimAlpha: 0.5f, slowFactor: 0.6f); 
            }
            else if (this.Type == SupernaturalCustomerType.FireMonster)
            {
                this.TriggerEnvironmentalEffect(gameManager.ShopLayout, gameManager);
            }
        }

        /// <summary>
        /// Updates the patience for this supernatural customer.
        /// This method calls the base class's patience update logic.
        /// </summary>
        /// <param name="deltaTime">The time elapsed since the last frame, in seconds.</param>
        /// <param name="gameManager">A reference to the <see cref="GameManager"/> for interactions.</param>
        public override void UpdatePatience(float deltaTime, GameManager gameManager) 
        {
            base.UpdatePatience(deltaTime, gameManager);
        }
    }
}
