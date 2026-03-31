using System;

namespace CoffeeShop
{
    /// <summary>
    /// Represents the state when the game has been successfully completed.
    /// This state is responsible for displaying the game-complete screen and handling related logic.
    /// </summary>
    public class GameCompleteState : GameStateBase
    {
        /// <summary>
        /// Gets the type of this game state.
        /// </summary>
        /// <value>
        /// Always returns <see cref="GameState.GameComplete"/>.
        /// </value>
        public override GameState StateType => GameState.GameComplete;

        /// <summary>
        /// Stores a reference to the final gameplay state before the game was completed.
        /// This is used to draw the final scene in the background of the completion screen.
        /// </summary>
        private readonly GameStateBase _finalGameplayState;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameCompleteState"/> class.
        /// </summary>
        /// <param name="finalGameplayState">The state of the game at the moment of completion. This is used for drawing the background.</param>
        public GameCompleteState(GameStateBase finalGameplayState)
        {
            _finalGameplayState = finalGameplayState;
        }

        /// <summary>
        /// Called when the state is first entered.
        /// This method stops any currently playing music and switches the active view to the game completion screen.
        /// </summary>
        /// <param name="gameManager">The central game manager instance that controls the game's flow and components.</param>
        public override void Enter(GameManager gameManager)
        {
            gameManager.AudioManager.StopCurrentMusic();
            gameManager.SwitchView(GameView.GameCompleteScreen);
        }

        /// <summary>
        /// Called on every frame to update the state's logic.
        /// In this state, user input is handled by the UIManager, so this method is empty.
        /// </summary>
        /// <param name="gameManager">The central game manager instance.</param>
        /// <param name="deltaTime">The time elapsed since the last frame, used for frame-rate independent updates.</param>
        public override void Update(GameManager gameManager, float deltaTime)
        {
            // Input is handled by the UIManager for the game complete screen.
        }

        /// <summary>
        /// Called on every frame to draw the visual elements of the state.
        /// It first draws the final gameplay scene in the background and then overlays the game completion UI.
        /// </summary>
        /// <param name="gameManager">The central game manager instance, which provides access to drawing and UI components.</param>
        public override void Draw(GameManager gameManager)
        {
            _finalGameplayState.Draw(gameManager);
            gameManager.UIManager.DrawGameCompleteScreen(gameManager);
        }

        /// <summary>
        /// Gets the enumeration value representing the current state.
        /// </summary>
        /// <returns>The <see cref="GameState"/> enumeration value for this state, which is <see cref="GameState.GameComplete"/>.</returns>
        public override GameState GetState()
        {
            return GameState.GameComplete;
        }
    }
}