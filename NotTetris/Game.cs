using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DirtyLibrary;

namespace NotTetris {
    public class Game : Core {
        private Player _player;
        private Board _board;
        private GameManager _manager;

        public const short VIRTUAL_HEIGHT = Board.CELL_SIZE * Board.HEIGHT_WITH_BORDER;
        public const short VIRTUAL_WIDTH = VIRTUAL_HEIGHT;

        public Game() : base("Not Tetris (at all)", VIRTUAL_WIDTH, VIRTUAL_HEIGHT, false) {
            _player = new Player();
            _board = new Board();
            _board.Player = _player;
            _manager = new GameManager(_board, _player);
        }

        protected override void Initialize() {
            base.Initialize();
        }

        protected override void LoadContent() {
            _manager.LoadContent();
            Audio.LoadContent();

            Audio.PlaySong(Audio.songs["gravity"], true);
            base.LoadContent();
        }

        protected override void Update(GameTime gameTime) {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            TimeElapsed = (int)gameTime.TotalGameTime.TotalMilliseconds;
            DeltaTime = gameTime.ElapsedGameTime.Milliseconds / 1000.0f;

            InputManager.Update();
            Audio.Update();
            _manager.Update();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.SetRenderTarget(renderTarget2D);
            GraphicsDevice.Clear(Color.Black);


            SpriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
            _manager.Draw();
            SpriteBatch.End();


            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);


            SpriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
            SpriteBatch.Draw(renderTarget2D, renderDestRect, Color.White);
            SpriteBatch.End();

            base.Draw(gameTime);
        }
    }
}