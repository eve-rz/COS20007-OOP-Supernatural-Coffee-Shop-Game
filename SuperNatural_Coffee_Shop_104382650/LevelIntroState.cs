using Raylib_cs;

namespace CoffeeShop
{
    /// <summary>
    /// Represents the transitional state that displays the level introduction screen.
    /// This state loads the specified level and shows an introductory message for a short duration
    /// before transitioning to the active gameplay state.
    /// </summary>
    public class LevelIntroState : GameStateBase
    {
        /// <summary>
        /// Gets the type of this game state.
        /// </summary>
        /// <remarks>
        /// This returns <see cref="GameState.Playing"/> as the intro is considered part of the overall playing sequence.
        /// </remarks>
        /// <value>
        /// Returns <see cref="GameState.Playing"/>.
        /// </value>
        public override GameState StateType => GameState.Playing;

        /// <summary>
        /// A countdown timer for the duration of the intro screen.
        /// </summary>
        private float _levelIntroTimer;

        /// <summary>
        /// The total duration in seconds for the level intro screen to be displayed.
        /// </summary>
        private const float LEVEL_INTRO_DURATION = 3.0f;

        /// <summary>
        /// The level number that this state will load.
        /// </summary>
        private readonly int _levelToLoad;

        /// <summary>
        /// Initializes a new instance of the <see cref="LevelIntroState"/> class.
        /// </summary>
        /// <param name="levelNumber">The number of the level to be loaded and introduced.</param>
        public LevelIntroState(int levelNumber)
        {
            _levelToLoad = levelNumber;
        }

        /// <summary>
        /// Called when the state is first entered. It loads the required level,
        /// switches the view to the intro screen, and initializes the countdown timer.
        /// </summary>
        /// <param name="gameManager">The central game manager instance that controls the game's flow and components.</param>
        public override void Enter(GameManager gameManager)
        {
            gameManager.LoadLevel(_levelToLoad);
            gameManager.SwitchView(GameView.LevelIntroScreen);
            _levelIntroTimer = LEVEL_INTRO_DURATION;
        }

        /// <summary>
        /// Called on every frame. It counts down the intro timer and transitions
        /// to the <see cref="PlayingState"/> once the timer reaches zero.
        /// </summary>
        /// <param name="gameManager">The central game manager instance.</param>
        /// <param name="deltaTime">The time elapsed since the last frame.</param>
        public override void Update(GameManager gameManager, float deltaTime)
        {
            _levelIntroTimer -= deltaTime;
            if (_levelIntroTimer <= 0)
            {
                // Once the intro duration is over, switch to the actual gameplay state.
                gameManager.SwitchView(GameView.CustomerScreen);
                gameManager.ChangeState(new PlayingState());
            }
        }

        /// <summary>
        /// Called on every frame to draw the state's visuals.
        /// This method delegates the drawing of the intro screen to the UIManager.
        /// </summary>
        /// <param name="gameManager">The central game manager, providing access to UI and rendering services.</param>
        public override void Draw(GameManager gameManager)
        {
            gameManager.UIManager.DrawCurrentView(gameManager);
        }

        /// <summary>
        /// Gets the enumeration value representing the current state.
        /// </summary>
        /// <returns>The <see cref="GameState"/> enumeration value, which is <see cref="GameState.Playing"/>.</returns>
        public override GameState GetState()
        {
            return GameState.Playing;
        }
    }
}