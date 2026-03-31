// NormalCustomer.cs
using Raylib_cs;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoffeeShop
{
    /// <summary>
    /// Represents a "normal" type of customer in the coffee shop.
    /// This class provides specific implementations for texture loading, sprite updates,
    /// order preferences, and patience handling suitable for a standard customer.
    /// It inherits common customer behaviors and properties from the <see cref="Customer"/> base class.
    /// </summary>
    public class NormalCustomer : Customer
    {
        private readonly float _preferredPatienceModifier;
        private Random _randomOrderChoice = new Random();

        private Texture2D _texHi;
        private Texture2D _texOrder;
        private Texture2D _texComplete;
        private Texture2D _texFailed;

        private Texture2D _dialogueSheetTexture;
        private Rectangle[] _dialogueSheetFrames = new Rectangle[6];
        private int _currentDialogueFrameIndex = 0;
        private const int DIALOGUE_SHEET_TOTAL_FRAMES = 6;
        private const int DIALOGUE_SHEET_COLS = 2;
        private const int DIALOGUE_SHEET_ROWS = 3;
        private const int DIALOGUE_FRAME_WIDTH = 600;
        private const int DIALOGUE_FRAME_HEIGHT = 600;

        public float PreferredPatienceModifier => _preferredPatienceModifier;

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalCustomer"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for this customer.</param>
        /// <param name="customerType">The <see cref="CustomerType"/> of this customer (e.g., could be 'NormalHuman', 'NormalRobot' if sub-typed).</param>
        /// <param name="spritePrefix">The prefix used to locate sprite assets for this customer (e.g., "norm_cust_01").</param>
        /// <param name="dialogueSet">The <see cref="Dialogue"/> object containing lines for this customer.</param>
        /// <param name="patienceModifier">A multiplier affecting patience depletion. Defaults to 1.0f. Must be positive.</param>
        public NormalCustomer(string id, CustomerType customerType, string spritePrefix, Dialogue dialogueSet, float patienceModifier = 1.0f)
            : base(id, customerType, spritePrefix, dialogueSet)
        {
            _preferredPatienceModifier = patienceModifier > 0 ? patienceModifier : 1.0f;
            _texHi = new Texture2D { Id = 0 };
            _texOrder = new Texture2D { Id = 0 };
            _texComplete = new Texture2D { Id = 0 };
            _texFailed = new Texture2D { Id = 0 };
            _dialogueSheetTexture = new Texture2D { Id = 0 };
        }

        /// <summary>
        /// Loads the character-specific textures for this normal customer, including state sprites and dialogue sheet.
        /// Textures are loaded based on the provided base path and character prefix.
        /// Also calculates the frames for the dialogue sprite sheet.
        /// </summary>
        /// <param name="basePathForImages">The root directory path where character image assets are stored.</param>
        /// <param name="characterPrefix">The specific file name prefix for this customer's textures (e.g., "customer_A").</param>
        public override void LoadCharacterTextures(string basePathForImages, string characterPrefix)
        {
            if (string.IsNullOrWhiteSpace(basePathForImages) || string.IsNullOrWhiteSpace(characterPrefix))
            {
                Console.WriteLine($"Error loading textures for {CustomerID}: Base path or character prefix is null or empty.");
                _texturesLoaded = false;
                return;
            }

            string customerSubFolder = Path.Combine(basePathForImages, "Customer");
            if (!Directory.Exists(customerSubFolder))
            {
                Console.WriteLine($"ERROR: Customer images directory not found: {customerSubFolder} for {characterPrefix}");
                _texturesLoaded = false;
                return;
            }

            try
            {
                // Load individual state textures and the dialogue sheet.
                _texHi = Raylib.LoadTexture(Path.Combine(customerSubFolder, $"{characterPrefix}_hi.png"));
                _texOrder = Raylib.LoadTexture(Path.Combine(customerSubFolder, $"{characterPrefix}_order.png"));
                _texComplete = Raylib.LoadTexture(Path.Combine(customerSubFolder, $"{characterPrefix}_complete.png"));
                _texFailed = Raylib.LoadTexture(Path.Combine(customerSubFolder, $"{characterPrefix}_failed.png"));
                _dialogueSheetTexture = Raylib.LoadTexture(Path.Combine(customerSubFolder, $"{characterPrefix}_dialogue.png"));

                Action<string, Texture2D> checkLoad = (fileName, texture) =>
                {
                    if (texture.Id == 0) Console.WriteLine($"WARNING: Failed to load '{Path.Combine(customerSubFolder, fileName)}' for {characterPrefix}");
                };

                checkLoad($"{characterPrefix}_hi.png", _texHi);
                checkLoad($"{characterPrefix}_order.png", _texOrder);
                checkLoad($"{characterPrefix}_complete.png", _texComplete);
                checkLoad($"{characterPrefix}_failed.png", _texFailed);
                checkLoad($"{characterPrefix}_dialogue.png", _dialogueSheetTexture);

                _texturesLoaded = _texHi.Id != 0 && _texOrder.Id != 0 && _texComplete.Id != 0 && _texFailed.Id != 0;

                if (_dialogueSheetTexture.Id != 0)
                {
                    int frameIndex = 0;
                    for (int row = 0; row < DIALOGUE_SHEET_ROWS; row++)
                    {
                        for (int col = 0; col < DIALOGUE_SHEET_COLS; col++)
                        {
                            if (frameIndex < DIALOGUE_SHEET_TOTAL_FRAMES)
                            {
                                _dialogueSheetFrames[frameIndex] = new Rectangle(
                                    col * DIALOGUE_FRAME_WIDTH,
                                    row * DIALOGUE_FRAME_HEIGHT,
                                    DIALOGUE_FRAME_WIDTH,
                                    DIALOGUE_FRAME_HEIGHT
                                );
                                frameIndex++;
                            }
                        }
                    }
                }
                else { Console.WriteLine($"Warning: Dialogue sheet {characterPrefix}_dialogue.png NOT LOADED for {CustomerID}."); }

                if (_texturesLoaded) { UpdateSpriteForState(); }
                else { _currentSprite = new Texture2D { Id = 0 }; } 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception loading textures for {CustomerID} ({characterPrefix}): {ex.Message}");
                _texturesLoaded = false;
                _currentSprite = new Texture2D { Id = 0 };
                _dialogueSheetTexture = new Texture2D { Id = 0 };
            }
        }

        /// <summary>
        /// Updates the customer's current sprite (<see cref="Customer._currentSprite"/>) and source rectangle
        /// (<see cref="Customer._currentSourceFrameRect"/>) based on their current <see cref="Customer.CurrentState"/>.
        /// This implementation selects from textures specific to the <see cref="NormalCustomer"/>.
        /// </summary>
        protected override void UpdateSpriteForState()
        {
            if (!_texturesLoaded)
            {
                if (_currentSprite.Id != 0) _currentSprite = new Texture2D { Id = 0 };
                _currentSourceFrameRect = new Rectangle(0, 0, 1, 1);
                return;
            }

            switch (CurrentState)
            {
                case CustomerState.Waiting:
                case CustomerState.WaitingForDrink:
                    _currentSprite = _texHi;
                    break;
                case CustomerState.Ordering:
                    _currentSprite = _texOrder;
                    break;
                case CustomerState.Consuming:
                    if (this.Order != null)
                    {
                        _currentSprite = (this.Order.CurrentStatus == OrderStatus.Served) ? _texComplete : _texFailed;
                        Console.WriteLine($"NormalCustomer {CustomerID} consuming. OrderStatus: {this.Order.CurrentStatus}. Sprite set to Id: {_currentSprite.Id} (CompleteID: {_texComplete.Id}, FailedID: {_texFailed.Id})");
                    }
                    else { _currentSprite = _texHi; /* Fallback if no order info somehow */ }
                    break;
                case CustomerState.Leaving:
                    _currentSprite = (this.Order != null && this.Order.CurrentStatus == OrderStatus.Served) ? _texComplete : _texFailed;
                    break;
                default:
                    _currentSprite = _texHi; 
                    break;
            }

            if (_currentSprite.Id == 0 && _texHi.Id != 0) _currentSprite = _texHi;

            if (_currentSprite.Id != 0)
            {
                _currentSourceFrameRect = new Rectangle(0, 0, _currentSprite.Width, _currentSprite.Height);
            }
            else
            {
                _currentSourceFrameRect = new Rectangle(0, 0, 1, 1);
            }
        }

        /// <summary>
        /// Gets the sprite sheet texture used for displaying this customer's dialogue expressions.
        /// </summary>
        /// <returns>The <see cref="Texture2D"/> for the dialogue sprite sheet.</returns>
        public override Texture2D GetDialogueSheetTexture()
        {
            return _dialogueSheetTexture;
        }

        /// <summary>
        /// Gets the source <see cref="Rectangle"/> for the current dialogue expression frame
        /// from this customer's dialogue sprite sheet.
        /// </summary>
        /// <returns>The source rectangle for the current dialogue frame, or a default 1x1 rectangle if not available.</returns>
        public override Rectangle GetCurrentDialogueFrameSourceRect()
        {
            if (_dialogueSheetTexture.Id != 0 && _dialogueSheetFrames != null && _dialogueSheetFrames.Length > _currentDialogueFrameIndex && _currentDialogueFrameIndex >= 0)
            {
                return _dialogueSheetFrames[_currentDialogueFrameIndex];
            }
            return new Rectangle(0, 0, 1, 1); 
        }

        /// <summary>
        /// Advances the dialogue expression to the next frame in a cyclic manner.
        /// </summary>
        public override void CycleDialogueFrame()
        {
            if (_dialogueSheetFrames != null && DIALOGUE_SHEET_TOTAL_FRAMES > 0)
            {
                _currentDialogueFrameIndex = (_currentDialogueFrameIndex + 1) % DIALOGUE_SHEET_TOTAL_FRAMES;
            }
            else
            {
                _currentDialogueFrameIndex = 0; 
            }
        }
        /// <summary>
        /// Resets the dialogue expression to the initial frame.
        /// </summary>
        public override void ResetDialogueFrame()
        {
            _currentDialogueFrameIndex = 0;
        }

        /// <summary>
        /// Unloads all textures specifically loaded for this normal customer to free up graphics memory.
        /// Resets texture fields to their initial empty state.
        /// </summary>
        public override void UnloadTextures()
        {
            if (_texHi.Id != 0) Raylib.UnloadTexture(_texHi);
            if (_texOrder.Id != 0) Raylib.UnloadTexture(_texOrder);
            if (_texComplete.Id != 0) Raylib.UnloadTexture(_texComplete);
            if (_texFailed.Id != 0) Raylib.UnloadTexture(_texFailed);
            if (_dialogueSheetTexture.Id != 0) Raylib.UnloadTexture(_dialogueSheetTexture);

            _texturesLoaded = false; 
            _texHi = new Texture2D { Id = 0 };
            _texOrder = new Texture2D { Id = 0 };
            _texComplete = new Texture2D { Id = 0 };
            _texFailed = new Texture2D { Id = 0 };
            _dialogueSheetTexture = new Texture2D { Id = 0 };
            _currentSprite = new Texture2D { Id = 0 }; 

            Console.WriteLine($"Textures unloaded for NormalCustomer {CustomerID}.");
        }

        /// <summary>
        /// Determines and returns the drink <see cref="Recipe"/> that this normal customer wishes to order.
        /// The customer randomly chooses from available, non-supernatural recipes appropriate for the current game level.
        /// </summary>
        /// <param name="availableRecipes">A list of all <see cref="Recipe"/>s currently available in the game.</param>
        /// <param name="currentLevelNumber">The current level number, used to filter recipes by their unlock level.</param>
        /// <returns>The chosen <see cref="Recipe"/> if a suitable one is found; otherwise, <c>null</c>.</returns>
        public override Recipe? GetDrinkOrder(List<Recipe> availableRecipes, int currentLevelNumber)
        {
            Console.WriteLine($"NormalCustomer {CustomerID} ({this.CustomerType}) deciding order. Level: {currentLevelNumber}. Available Recipes: {availableRecipes?.Count ?? 0}");

            if (availableRecipes == null || !availableRecipes.Any())
            {
                Console.WriteLine($"Warning: No available recipes for customer {CustomerID} to order from.");
                return null;
            }

            List<Recipe> levelAppropriateNormalRecipes = availableRecipes
                                                       .Where(r => !r.IsSupernatural && r.UnlockLevel <= currentLevelNumber)
                                                       .ToList();

            if (levelAppropriateNormalRecipes.Any())
            {
                Recipe chosenRecipe = levelAppropriateNormalRecipes[_randomOrderChoice.Next(levelAppropriateNormalRecipes.Count)];
                Console.WriteLine($"{CustomerID} randomly ordered '{chosenRecipe.RecipeName}' (UnlockLevel: {chosenRecipe.UnlockLevel}) from level-appropriate normal options.");
                return chosenRecipe;
            }

            Console.WriteLine($"Warning: NormalCustomer {CustomerID} ({this.CustomerType}) could not find any suitable normal recipe for level {currentLevelNumber}.");
            return null;
        }

        /// <summary>
        /// Handles the event of this normal customer receiving a drink.
        /// This implementation primarily logs the event and calls the base class's <see cref="Customer.ReceiveDrink"/> method.
        /// </summary>
        /// <param name="drink">The <see cref="Drink"/> object received by the customer.</param>
        /// <param name="servedSuccessfully">A boolean indicating if the game logic considers the serving action successful (e.g., correct order).</param>
        public override void ReceiveDrink(Drink drink, bool servedSuccessfully)
        {
            Console.WriteLine($"NormalCustomer {this.CustomerID} processing ReceiveDrink (servedSuccessfully: {servedSuccessfully}).");
            base.ReceiveDrink(drink, servedSuccessfully); 
        }

        /// <summary>
        /// Updates the patience for this normal customer, taking into account the <see cref="PreferredPatienceModifier"/>.
        /// This method interacts with the <see cref="GameManager"/> for broader game effects if patience runs out.
        /// </summary>
        /// <param name="deltaTime">The time elapsed since the last frame, in seconds.</param>
        /// <param name="gameManager">A reference to the <see cref="GameManager"/> for interactions like triggering events or penalties when patience depletes.</param>
        public override void UpdatePatience(float deltaTime, GameManager gameManager)
        {
            if (CurrentState == CustomerState.Leaving)
            {
                if (HasPatienceRunOut) 
                {
                    return; 
                }
                base.UpdatePatience(deltaTime, gameManager);
                return;
            }

            if (CurrentState == CustomerState.Consuming)
            {
                base.UpdatePatience(deltaTime, gameManager);
            }
            else if (_isPatienceActive &&
                     (CurrentState == CustomerState.Waiting ||
                      CurrentState == CustomerState.Ordering ||
                      CurrentState == CustomerState.WaitingForDrink))
            {
                float effectiveModifier = (_preferredPatienceModifier > 0.001f) ? _preferredPatienceModifier : 1.0f;
                Patience -= 1.0f / effectiveModifier * deltaTime; 

                if (Patience <= 0 && !HasPatienceRunOut) 
                {
                    Console.WriteLine($"Customer {CustomerID} ({this.CustomerType}) patience ran out!");
                    HasPatienceRunOut = true; 

                    OnPatienceDepleted(gameManager);

                    if (this.Order != null && this.Order.CurrentStatus != OrderStatus.Served && this.Order.CurrentStatus != OrderStatus.Failed)
                    {
                        this.Order.UpdateStatus(OrderStatus.Failed); 
                        Console.WriteLine($"Order for {CustomerID} marked as Failed due to depleted patience.");
                    }
                    LeaveShop();
                }
            }
        }
    }
}
