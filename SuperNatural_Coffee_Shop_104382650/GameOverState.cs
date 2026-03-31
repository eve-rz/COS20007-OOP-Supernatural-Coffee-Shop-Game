using Raylib_cs;

namespace CoffeeShop
{
    public class GameOverState : GameStateBase
    {
        public override GameState StateType => GameState.GameOver;
        private readonly GameStateBase _finalGameplayState;

        public GameOverState(GameStateBase finalGameplayState)
        {
            _finalGameplayState = finalGameplayState;
        }

        public override void Enter(GameManager gameManager)
        {
            gameManager.AudioManager.StopCurrentMusic();
            gameManager.SwitchView(GameView.GameOverScreen);
        }

        public override void Update(GameManager gameManager, float deltaTime)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Enter) || Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                gameManager.ChangeState(new MainMenuState());
            }
        }

        public override void Draw(GameManager gameManager)
        {
            _finalGameplayState.Draw(gameManager);
            gameManager.UIManager.DrawGameOverScreen(gameManager);
        }

        public override GameState GetState()
        {
            return GameState.GameOver;
        }
    }
}