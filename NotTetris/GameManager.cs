
using DirtyLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NotTetris {
    public class GameManager {
        private Board _board;
        private Player _player;

        private Texture2D _nextPanel;
        private Rectangle _nextPanelDestRect;
        private TetrominoPosition _middleNextPanelDestRect;
        private ushort _uiLeft;
        private ushort _uiWidth;
        public GameManager(Board board, Player player) {
            _board = board;
            _player = player;
            _uiLeft = Board.CELL_SIZE * Board.WIDTH_WITH_BORDER;
            _uiWidth = Game.VIRTUAL_WIDTH - Board.CELL_SIZE * Board.WIDTH_WITH_BORDER;

            _nextPanelDestRect.Width = 128 + 64;
            _nextPanelDestRect.Height = _nextPanelDestRect.Width;

            short middleWidth = (short)(_uiLeft + _uiWidth / 2 - _nextPanelDestRect.Width / 2);

            _nextPanelDestRect.Y = 0;
            _nextPanelDestRect.X = middleWidth;

            // _middleNextPanelDestRect.X = 

        }

        public void Update() {
            _board.Update();
        }

        public void Draw() {
            _board.Draw();
            Core.SpriteBatch.Draw(_nextPanel, _nextPanelDestRect, Color.White);
        }

        public void LoadContent() {
            _board.LoadContent();
            _nextPanel = Core.Content.Load<Texture2D>("images/next_panel");
        }
    }
}