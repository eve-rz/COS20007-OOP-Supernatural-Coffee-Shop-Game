using Raylib_cs;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;

namespace CoffeeShop
{
    public class GameManager
    {
        private GameStateBase _currentState;
        private GameStateBase _previousState;
        
        public GameView CurrentGameView { get; private set; }
        public GameState GameState => _currentState.GetState();
        public float GameTime { get; private set; }
        public int PlayerScore { get; private set; }
        public bool QuitProgramRequested { get; private set; } = false;
        public bool GameWasWon { get; private set; } = false;

        private Level? _currentLevel;
        public Level? CurrentLevel => _currentLevel;
        private bool _isCurrentlyLevel4 = false;
        private int _levelToLoadAfterShop = 1;

        public Player Player { get; }
        public UIManager UIManager { get; }
        public ShopLayout ShopLayout { get; }
        public AudioManager AudioManager { get; }
        private readonly Random _random = new Random();

        public const float DEFAULT_PLAYER_START_MONEY = 10f;
        public const float DEFAULT_PLAYER_START_SANITY = 100f;
        public bool JustOpenedShop { get; set; } = false;
        public GameStateBase PreviousState => _previousState;

        private readonly List<Ingredient> _allDefinedIngredients = new List<Ingredient>();
        public List<Ingredient> AllDefinedIngredients => _allDefinedIngredients;
        private readonly HashSet<IngredientType> _globallyUnlockedIngredientTypes = new HashSet<IngredientType>();
        public HashSet<IngredientType> GloballyUnlockedIngredientTypes => _globallyUnlockedIngredientTypes;
        private const int LEVEL_START_INGREDIENT_STOCK = 50;

        public Customer? CurrentlyInteractingCustomer { get; private set; }

        private readonly int _screenWidth;
        private readonly int _screenHeight;
        public float ScreenDimAlpha { get; private set; } = 0.0f;
        public float CursorSlowFactor { get; private set; } = 1.0f;

        public IngredientType? StolenIngredientType { get; private set; }
        private float _stolenIngredientRecoveryTimer = 0f;
        private const float STOLEN_INGREDIENT_RECOVERY_DURATION = 60.0f;
        public bool IsIngredientCurrentlyStolen => StolenIngredientType.HasValue;
        public bool IsGhostInterferenceActive { get; private set; } = false;
        private float _ghostInterferenceTimer = 0f;
        public bool IsToyolHiding { get; private set; } = false;
        public Rectangle HiddenToyolClickRect { get; private set; }
        private const float HIDDEN_TOYOL_BASE_SIZE = 60f;
        public bool IsBrewingStationOverheated { get; private set; } = false;

        public GameManager(int screenWidth, int screenHeight)
        {
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            Player = new Player();
            UIManager = new UIManager(screenWidth, screenHeight);
            ShopLayout = new ShopLayout();
            AudioManager = new AudioManager(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Audio"));
            InitializeAllIngredients();
            UIManager.LoadAssets();
            _previousState = new MainMenuState();
            _currentState = new MainMenuState();
            _currentState.Enter(this);
        }
        
        public void ChangeState(GameStateBase newState)
        {
            if (_currentState == newState) return;
            if (_currentState is PlayingState || _currentState is LevelIntroState) _previousState = _currentState;
            _currentState?.Exit(this);
            _currentState = newState;
            _currentState.Enter(this);
        }

        public void Update(float deltaTime)
        {
            UIManager.Update(deltaTime, this);
            AudioManager.UpdateMusicStreams();
            _currentState.Update(this, deltaTime);
        }

        public void Draw()
        {
            _currentState.Draw(this);
            UIManager.DrawCursor();
        }
        
        public void SwitchView(GameView newView) 
        { 
            if (CurrentGameView != newView) 
            { 
                CurrentGameView = newView; UIManager.TransitionToScreen(newView); UpdateBackgroundMusic(); 
            } 
        }
        
        public void StartGame() 
        { 
            GameWasWon = false; 
            PlayerScore = 0; 
            Player.ResetPlayerForNewGame(DEFAULT_PLAYER_START_MONEY, DEFAULT_PLAYER_START_SANITY); 

            ChangeState(new LevelIntroState(1)); 
        }
        
        public void PauseGame() 
        { 
            if (_currentState is PlayingState || _currentState is LevelIntroState) ChangeState(new PausedState(_currentState)); 
            }

        public void ResumeGame() 
        { if (_currentState is PausedState p) p.Resume(this); 
        }
        
        public void OpenShop()
        {
            if (_currentState is PlayingState)
            {
                JustOpenedShop = true;
                ChangeState(new ShopState(this));
            }
        }
        
        public void CloseShop()
        {
            if (!(_currentState is ShopState shopState)) return;

            if (_levelToLoadAfterShop > GetCurrentLevelNumber())
            {
                ProceedToNextLevelFromShop();
            }
            else 
            {
                shopState.ReturnToPreviousState(this);
            }
        }
        
        public void OpenSettings()
        {
            ChangeState(new SettingsState(_currentState));
        }

        
        public void CloseSettings()
        {
            if (_currentState is SettingsState s)
            {
                s.ReturnToPreviousState(this);
            }
        }
        
        public GameStateBase GetCurrentStateForTesting()
        {
            return _currentState;
        }
        
        public void EndLevel(bool wasSuccessful)
        {
            if (!(_currentState is PlayingState)) return;
            int levelNum = GetCurrentLevelNumber(); Player.SetCurrentOrder(null);
            if (wasSuccessful)
            {
                PlayerScore += 50 + (levelNum * 10) + (int)(GameTime * 0.1f);
                if (levelNum >= 4) { GameWasWon = true; ChangeState(new GameCompleteState(_currentState)); }
                else { _levelToLoadAfterShop = levelNum + 1; ChangeState(new ShopState(this)); }
            }
            else { GameWasWon = false; ChangeState(new GameOverState(_currentState)); }
        }
        
        public void ReturnToMainMenu() => ChangeState(new MainMenuState());
        
        public void RequestQuit() => QuitProgramRequested = true;
        
        public void SetPlayerScore(int score) => PlayerScore = score;
        
        public void SetGameTime(float time) => GameTime = time;

        public void UpdateCoreGameplay(float deltaTime)
        {
            if(CurrentLevel == null) return;
            GameTime -= deltaTime; if (GameTime < 0) GameTime = 0;
            if (Player.SanityLevel <= 0.01f || (ShopLayout.GetStation(EquipmentType.CoffeeMachine)?.AssignedEquipment as CoffeeMachine)?.CurrentWaterUnits <= 0.01f) { EndLevel(false); return; }
            if (CurrentLevel.TargetEarnings > 0 && Player.Money >= CurrentLevel.TargetEarnings) { EndLevel(true); return; }
            if (GameTime <= 0 && CurrentLevel.TimeLimit > 0) { EndLevel(false); return; }

            ShopLayout.UpdateCustomerSpawning(deltaTime, CurrentLevel, this);

            List<Customer> customersInShop = ShopLayout.AllCustomersInShop(this.CurrentlyInteractingCustomer);
            List<Customer> customersToRemove = new List<Customer>();

            foreach (var customer in customersInShop)
            {
                customer.UpdatePatience(deltaTime, this);
                if (customer.CurrentState == CustomerState.Leaving)
                {
                    customersToRemove.Add(customer);
                }
            }

            foreach (var customer in customersToRemove)
            {
                ShopLayout.RemoveCustomerFromQueue(customer);
                if (CurrentlyInteractingCustomer == customer)
                {
                    CurrentlyInteractingCustomer = null;
                }
                customer.UnloadTextures(); 
            }
            
            if (IsIngredientCurrentlyStolen) { _stolenIngredientRecoveryTimer -= deltaTime; if (_stolenIngredientRecoveryTimer <= 0) SetStolenIngredient(null); }
            if (IsGhostInterferenceActive) { _ghostInterferenceTimer -= deltaTime; if (_ghostInterferenceTimer <= 0) DeactivateGhostlyInterference(); }
        }

        public void ProceedToNextLevelFromShop()
        {
            Console.WriteLine($"GameManager: Closing shop and proceeding to Level {_levelToLoadAfterShop}.");
            ChangeState(new LevelIntroState(_levelToLoadAfterShop));
        }

        public void InitiateInteractionWithCustomer(Customer customer)
        {
            if (CurrentlyInteractingCustomer != null) { UIManager.DisplayMessage("Finish with the current customer first!", MessageType.Warning, true); return; }
            if (Player.CurrentOrder != null && Player.CurrentOrder.RequestingCustomer != customer) { UIManager.DisplayMessage("You are already handling an order for someone else!", MessageType.Warning, true); return; }

            CurrentlyInteractingCustomer = customer;
            customer.ActivatePatience();
            customer.SetCurrentState(CustomerState.Ordering);

            if (customer is SupernaturalCustomer sc)
            {
                if (sc.Type == SupernaturalCustomerType.Toyol && !IsIngredientCurrentlyStolen)
                {
                    var availableToSteal = AllDefinedIngredients.Where(i => !i.IsSupernatural).Select(i => i.Type).ToList();
                    if (availableToSteal.Any())
                    {
                        SetStolenIngredient(availableToSteal[_random.Next(availableToSteal.Count)], true);
                    }
                }
                else if (sc.Type == SupernaturalCustomerType.FireMonster && (sc.Mood == CustomerMood.Agitated || sc.Mood == CustomerMood.Hostile))
                {
                    Player.SetSanityLevel(Player.SanityLevel - (sc.Mood == CustomerMood.Hostile ? 20f : 10f));
                    if (sc.Mood == CustomerMood.Hostile)
                    {
                        sc.TriggerEnvironmentalEffect(ShopLayout, this);
                    }
                }
            }
            SwitchView(GameView.DialogueScreen);
        }
        
        public void ConcludeInteractionAfterDialogue(bool orderTaken)
        {
            var customer = CurrentlyInteractingCustomer; CurrentlyInteractingCustomer = null; if (customer == null) { SwitchView(GameView.CustomerScreen); return; }
            if (orderTaken && Player.CurrentOrder?.RequestingCustomer == customer) SwitchView(GameView.WorkstationScreen);
            else { if (customer.CurrentState != CustomerState.Leaving) customer.LeaveShop(); SwitchView(GameView.CustomerScreen); }
        }
        
        public void LoadLevel(int levelNumber)
        {
            Console.WriteLine($"[GameManager.LoadLevel] Loading level {levelNumber}.");
            Player.PreparePlayerForNewLevel(DEFAULT_PLAYER_START_SANITY);
            _isCurrentlyLevel4 = (levelNumber == 4);
            SetBrewingStationOverheated(false, false);
            ShopLayout.CustomerQueue.Clear();
            CurrentlyInteractingCustomer = null;
            Player.SelectedIngredientsForBrewing.Clear();

            if (ShopLayout.Stations.Count == 0)
            {
                var station = new Station("S_CM_Main", new Vector2D(200, 300));
                station.AssignEquipment(new CoffeeMachine("Main Brewing Station", "Use Station", 100));
                ShopLayout.AddStation(station);
            }

            string levelID;
            float targetEarnings;
            float timeLimit;
            var allowedSupernatural = new List<SupernaturalCustomerType>();
            var newIngredients = new List<IngredientType>();

            switch (levelNumber)
            {
                case 1:
                    levelID = "1 Welcome to the Counter";
                    targetEarnings = 30f;   //Debug 15
                    timeLimit = 120f;
                    newIngredients.AddRange(new[] { IngredientType.CoffeeBean, IngredientType.Milk, IngredientType.Lemon, IngredientType.Tea });
                    break;
                case 2:
                    levelID = "2 Honey?";
                    targetEarnings = 60f;   //Debug 20
                    timeLimit = 180f;
                    newIngredients.AddRange(new[] { IngredientType.Honey, IngredientType.GreenTea, IngredientType.Mint });
                    break;
                case 3:
                    levelID = "3 Something's Brewing...";
                    targetEarnings = 90f;   //Debug 25
                    timeLimit = 270f;
                    newIngredients.AddRange(new[] { IngredientType.Chocolate, IngredientType.Cinnamon, IngredientType.Ginger });
                    break;
                case 4: 
                    levelID = "4 SupernaturalShift";
                    targetEarnings = 120f;  //Debug 35
                    timeLimit = 360f;
                    allowedSupernatural.AddRange(Enum.GetValues<SupernaturalCustomerType>());
                    newIngredients.AddRange(Enum.GetValues<IngredientType>());
                    break;
                default:
                    Console.WriteLine($"Error: Level {levelNumber} data definition not found. Returning to MainMenu.");
                    ChangeState(new MainMenuState());
                    return;
            }

            _currentLevel = new Level(levelID, targetEarnings, timeLimit, $"Level{levelNumber}_Default", 0.1f, allowedSupernatural);
            
            newIngredients.ForEach(type => _globallyUnlockedIngredientTypes.Add(type));

            foreach (var type in _globallyUnlockedIngredientTypes)
            {
                var ing = AllDefinedIngredients.FirstOrDefault(i => i.Type == type);
                if (ing != null)
                {
                    _currentLevel.AddUnlockedIngredient(ing);
                    if (!Player.Inventory.ContainsKey(ing) || Player.Inventory[ing] == 0)
                    {
                        Player.RestockIngredient(ing, LEVEL_START_INGREDIENT_STOCK);
                    }
                }
            }
            
            List<Recipe> masterRecipeList = new List<Recipe>();
            Ingredient? coffee = GetIngredientByName("Coffee Bean");
            Ingredient? milk = GetIngredientByName("Milk");
            Ingredient? lemon = GetIngredientByName("Lemon");
            Ingredient? tea = GetIngredientByName("Tea");
            Ingredient? honey = GetIngredientByName("Honey");
            Ingredient? greenTea = GetIngredientByName("Green Tea");
            Ingredient? mint = GetIngredientByName("Mint");
            Ingredient? chocolate = GetIngredientByName("Chocolate");
            Ingredient? cinnamon = GetIngredientByName("Cinnamon");
            Ingredient? ginger = GetIngredientByName("Ginger");

            // Supernatural recipe
            if (mint != null && cinnamon != null && ginger != null)
            {
                Recipe fireyBrew = new Recipe("Firey Brew", 8.5f, 16f, EquipmentType.CoffeeMachine, 4, isSupernatural: true)
                    .AddIngredientRequirement(mint, 1)
                    .AddIngredientRequirement(cinnamon, 1)
                    .AddIngredientRequirement(ginger, 1);
                masterRecipeList.Add(fireyBrew);
            }
            if (coffee != null && chocolate != null && tea != null)
            {
                Recipe shadowBrew = new Recipe("Shadow Brew", 9.0f, 20f, EquipmentType.CoffeeMachine, 4, isSupernatural: true)
                    .AddIngredientRequirement(coffee, 1)
                    .AddIngredientRequirement(chocolate, 1)
                    .AddIngredientRequirement(tea, 1);
                masterRecipeList.Add(shadowBrew);
            }

            // Level 1-3 Recipes
            if (coffee != null) masterRecipeList.Add(new Recipe("Espresso", 3.0f, 10f, EquipmentType.CoffeeMachine, 1).AddIngredientRequirement(coffee, 1));
            if (tea != null) masterRecipeList.Add(new Recipe("Classic Tea", 2.5f, 8f, EquipmentType.CoffeeMachine, 1).AddIngredientRequirement(tea, 1));
            if (coffee != null && milk != null) masterRecipeList.Add(new Recipe("Latte", 4.0f, 15f, EquipmentType.CoffeeMachine, 1).AddIngredientRequirement(coffee, 1).AddIngredientRequirement(milk, 1));
            if (tea != null && lemon != null) masterRecipeList.Add(new Recipe("Lemon Tea", 3.0f, 10f, EquipmentType.CoffeeMachine, 1).AddIngredientRequirement(tea, 1).AddIngredientRequirement(lemon, 1));
            if (coffee != null && lemon != null) masterRecipeList.Add(new Recipe("Zesty Coffee", 3.75f, 12f, EquipmentType.CoffeeMachine, 1).AddIngredientRequirement(coffee, 1).AddIngredientRequirement(lemon, 1));
            if (coffee != null && honey != null) masterRecipeList.Add(new Recipe("Pooh Coffee", 4.25f, 12f, EquipmentType.CoffeeMachine, 2).AddIngredientRequirement(coffee, 1).AddIngredientRequirement(honey, 1));
            if (honey != null && lemon != null) masterRecipeList.Add(new Recipe("Honey Lemon", 3.0f, 8f, EquipmentType.CoffeeMachine, 2).AddIngredientRequirement(honey, 1).AddIngredientRequirement(lemon, 1));
            if (greenTea != null) masterRecipeList.Add(new Recipe("Green Tea", 3.0f, 10f, EquipmentType.CoffeeMachine, 2).AddIngredientRequirement(greenTea, 1));
            if (tea != null && milk != null) masterRecipeList.Add(new Recipe("Milk Tea", 4.0f, 15f, EquipmentType.CoffeeMachine, 2).AddIngredientRequirement(tea, 1).AddIngredientRequirement(milk, 1));
            if (chocolate != null && milk != null) masterRecipeList.Add(new Recipe("Chocolate Milk", 4.0f, 12f, EquipmentType.CoffeeMachine, 3).AddIngredientRequirement(chocolate, 1).AddIngredientRequirement(milk, 1));
            if (coffee != null && cinnamon != null) masterRecipeList.Add(new Recipe("Cinnamon Coffee", 4.5f, 14f, EquipmentType.CoffeeMachine, 3).AddIngredientRequirement(coffee, 1).AddIngredientRequirement(cinnamon, 1));
            if (mint != null) masterRecipeList.Add(new Recipe("Mint", 3.25f, 10f, EquipmentType.CoffeeMachine, 3).AddIngredientRequirement(mint, 1));
            if (greenTea != null && milk != null) masterRecipeList.Add(new Recipe("Green Milk Tea", 4.5f, 18f, EquipmentType.CoffeeMachine, 3).AddIngredientRequirement(greenTea, 1).AddIngredientRequirement(milk, 1)); 
            if (coffee != null && milk != null && ginger != null) masterRecipeList.Add(new Recipe("Ginger Latte", 5.25f, 20f, EquipmentType.CoffeeMachine, 3).AddIngredientRequirement(coffee, 1).AddIngredientRequirement(milk, 1).AddIngredientRequirement(ginger, 1)); 
            if (tea != null && honey != null && cinnamon != null) masterRecipeList.Add(new Recipe("Cinnamon Honey Tea", 4.25f, 14f, EquipmentType.CoffeeMachine, 3).AddIngredientRequirement(tea, 1).AddIngredientRequirement(honey, 1).AddIngredientRequirement(cinnamon, 1)); 

            _currentLevel.AvailableRecipes.Clear(); 
            foreach (Recipe rec in masterRecipeList)
            {
                if (rec.UnlockLevel <= levelNumber) _currentLevel.AddAvailableRecipe(rec);
            }

            SetGameTime(_currentLevel.TimeLimit);
        }
        

        private void InitializeAllIngredients()
        {
            _allDefinedIngredients.Clear();
            _allDefinedIngredients.Add(new Ingredient("Coffee Bean", "", IngredientType.CoffeeBean, false, 6, 1, 1, 0.5f));
            _allDefinedIngredients.Add(new Ingredient("Milk", "", IngredientType.Milk, false, 0, 0, 2, 0.3f));
            _allDefinedIngredients.Add(new Ingredient("Lemon", "", IngredientType.Lemon, false, 0, 7, 1, 0.2f));
            _allDefinedIngredients.Add(new Ingredient("Tea", "", IngredientType.Tea, false, 3, 1, 0, 0.4f));
            _allDefinedIngredients.Add(new Ingredient("Honey", "", IngredientType.Honey, false, 0, 0, 7, 0.4f));
            _allDefinedIngredients.Add(new Ingredient("Green Tea", "", IngredientType.GreenTea, false, 2, 1, 0, 0.5f));
            _allDefinedIngredients.Add(new Ingredient("Mint", "", IngredientType.Mint, false, 0, 0, 1, 0.3f));
            _allDefinedIngredients.Add(new Ingredient("Chocolate", "", IngredientType.Chocolate, false, 2, 0, 6, 0.8f));
            _allDefinedIngredients.Add(new Ingredient("Cinnamon", "", IngredientType.Cinnamon, false, 1, 0, 3, 0.6f));
            _allDefinedIngredients.Add(new Ingredient("Ginger", "", IngredientType.Ginger, false, 2, 1, 1, 0.7f));
        }

        public void SetBrewingStationOverheated(bool isOverheated, bool causedBySupernaturalEvent = false)
        {
            bool wasAlreadyOverheated = IsBrewingStationOverheated; IsBrewingStationOverheated = isOverheated;
            if (isOverheated && !wasAlreadyOverheated) { if (causedBySupernaturalEvent) Player.SetSanityLevel(Player.SanityLevel - 5f); }
        }

        public bool AttemptToCoolStation() 
        { 
            if (!IsBrewingStationOverheated) return true; if (Player.ConsumeWaterJug()) 
            { 
                SetBrewingStationOverheated(false); return true; 
            } return false; 
        }

        public bool PlayerAttemptPurchaseIngredient(Ingredient ingredient, int quantity) 
        { 
            bool s = Player.PurchaseIngredient(ingredient, quantity);
            UIManager.DisplayMessage(s ? $"Purchased {quantity} {ingredient.Name}!" : "Not enough money.", s ? MessageType.Info : MessageType.Warning);
            return s; 
        
        }
        public bool PlayerAttemptPurchaseWaterJug(int quantity) 
        { 
            if (Player.Money >= 5.0f * quantity) 
            { 
                Player.SetMoney(Player.Money - 5.0f * quantity); Player.AddWaterJugs(quantity);
                UIManager.DisplayMessage($"Purchased {quantity} Water Jugs!", MessageType.Info);
                return true; 
            } 

            UIManager.DisplayMessage("Not enough money.", MessageType.Warning);
            return false; 
        }

        public void InitiateBrewingProcess()
        {
            if (CurrentGameView != GameView.WorkstationScreen || Player.CurrentOrder == null || Player.SelectedIngredientsForBrewing.Count == 0)
            {
                UIManager.DisplayMessage("Cannot brew!", MessageType.Warning, true);
                return;
            }
            UIManager.ClearWorkstationMessage();
            UIManager.StartWorkstationBrewingAnimations();
        }

        public void AllBrewingAnimationsComplete() 
        {   
            var tool = ShopLayout.GetStation(EquipmentType.CoffeeMachine)?.AssignedEquipment; if (tool == null) return;
            var result = Player.AttemptToPrepareDrink(Player.CurrentOrder?.AssignedRecipe, Player.SelectedIngredientsForBrewing, tool, this);
            UIManager.ShowWorkstationServeTrashUI(result.WasSuccessful, result.PreparedDrink, result.OutcomeReason); 
        }

        public void PlayerConfirmedServe() 
        { 
            if (Player.HeldItem is Drink) 
            { Player.CurrentOrder?.UpdateStatus(OrderStatus.Ready);
             SwitchView(GameView.CustomerScreen); } 
        }

        public void PlayerTrashedDrink() 
        { 
            Player.SetHeldItem(null);
            UIManager.ResetWorkstationToSelection(this); 
        }

        public void PlayerDisposedHeldItemOnScreen() 
        { 
            Player.SetHeldItem(null);
            UIManager.DisplayMessage("Drink discarded.", MessageType.Info, true); 
        }
        
        public void AttemptRefillCoffeeMachineWater()
        {
            var coffeeMachine = ShopLayout.GetStation(EquipmentType.CoffeeMachine)?.AssignedEquipment as CoffeeMachine;
            if (coffeeMachine != null && Player.ConsumeWaterJug())
            {
                coffeeMachine.RefillWaterToFull(); UIManager.DisplayMessage("Coffee machine water refilled!", MessageType.Info, true);
            }
            else
            {
                UIManager.DisplayMessage("No water jugs to refill!", MessageType.Warning, true);
            }
        }
        
        public bool IsIngredientStolen(IngredientType type)
        {
            return StolenIngredientType.HasValue && StolenIngredientType.Value == type;
        }
        
        public void SetStolenIngredient(IngredientType? type, bool byToyol = false)
        {
            IsToyolHiding = false; 
            if (StolenIngredientType.HasValue && type == null) { UIManager?.DisplayMessage($"{StolenIngredientType.Value} returned!", MessageType.Info, true); _stolenIngredientRecoveryTimer = 0f; }
            else if (type.HasValue && StolenIngredientType != type) { if (byToyol) { _stolenIngredientRecoveryTimer = STOLEN_INGREDIENT_RECOVERY_DURATION; IsToyolHiding = true; float size = HIDDEN_TOYOL_BASE_SIZE * 0.5f; HiddenToyolClickRect = new Rectangle((float)_random.NextDouble() * (_screenWidth - 100 - size) + 50, (float)_random.NextDouble() * (_screenHeight - 200 - size) + 150, size, size); } }
            StolenIngredientType = type;
        }

        public void PlayerFoundHiddenToyol() 
        { 
            if (IsToyolHiding) 
            { 
                UIManager?.DisplayMessage("You found the Toyol!", MessageType.Info); SetStolenIngredient(null, false); 
            } 
        }
        
        public void ActivateGhostlyInterference(float duration, float dimAlpha, float slowFactor) 
        { 
            if (!IsGhostInterferenceActive) UIManager?.DisplayMessage("A chilling presence...", MessageType.Warning); 
            IsGhostInterferenceActive = true; 
            _ghostInterferenceTimer = duration; 
            ScreenDimAlpha = dimAlpha; 
            CursorSlowFactor = slowFactor; 
        }
        
        private void DeactivateGhostlyInterference() 
        { 
            IsGhostInterferenceActive = false;
            _ghostInterferenceTimer = 0f; 
            ScreenDimAlpha = 0.0f; 
            CursorSlowFactor = 1.0f; 
         }
        
        public int GetCurrentLevelNumber() 
        {
            if (_currentLevel == null) return 0; 
            return int.TryParse(new string(_currentLevel.LevelID.TakeWhile(char.IsDigit).ToArray()), out int n) ? n : 0; 
        }
        
        public Ingredient? GetIngredientByName(string name) => AllDefinedIngredients.FirstOrDefault(ing => ing.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        private void UpdateBackgroundMusic()
        {
            string songName = "game_song";
            if (_isCurrentlyLevel4) songName = "level4_song";
            if (CurrentGameView == GameView.WorkstationScreen) songName = _isCurrentlyLevel4 ? "level4_workstation" : "workstation_song";
            AudioManager.PlayMusicLooping(AudioManager.GetSongByName(songName), songName);
        }
    }
}