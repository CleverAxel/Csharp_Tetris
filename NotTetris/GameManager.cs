
using System;
using DirtyLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NotTetris {
    public class GameManager {
        private Board _board;
        private Player _player;


        private Vector2 _nextTexTPosition;
        private string _nextLabel = "next";
        private Texture2D _nextPanel;
        private Rectangle _nextPanelDestRect;
        private TetrominoType _nextTetromino;
        private const short _nextTetrominoSize = 32;
        private Rectangle _nextTetrominoDestRect;
        private TetrominoPosition[] _nextTetrominoOffsets = new TetrominoPosition[4];
        private Vector2 _nextTetrominoOffsetToCenter; //offset to center the next tetromino in the destRect




        private Vector2 _middleNextPanelDestRect;
        private ushort _uiLeft;
        private ushort _uiWidth;
        public GameManager(Board board, Player player) {
            //shit tons of magic number for the UI part
            _board = board;
            _player = player;
            _uiLeft = Board.CELL_SIZE * Board.WIDTH_WITH_BORDER;
            _uiWidth = Game.VIRTUAL_WIDTH - Board.CELL_SIZE * Board.WIDTH_WITH_BORDER;

            _nextPanelDestRect.Width = 128 + 64;
            _nextPanelDestRect.Height = _nextPanelDestRect.Width;

            short middleWidth = (short)(_uiLeft + _uiWidth / 2 - _nextPanelDestRect.Width / 2);

            _nextPanelDestRect.Y = 28;
            _nextPanelDestRect.X = middleWidth;


            _nextTexTPosition.X = _nextPanelDestRect.X + _nextPanelDestRect.Width / 2;
            _nextTexTPosition.Y = 0;

            _middleNextPanelDestRect.X = _nextPanelDestRect.X + _nextPanelDestRect.Width / 2 - _nextTetrominoSize / 2;
            _middleNextPanelDestRect.Y = _nextPanelDestRect.Y + 3 + _nextPanelDestRect.Height / 2 - _nextTetrominoSize / 2;
            _nextTetrominoDestRect.Width = _nextTetrominoSize;
            _nextTetrominoDestRect.Height = _nextTetrominoSize;

            _board.OnNextTetrominoChange += OnNextTetrominoChange;

        }

        public void Update() {
            _board.Update();
        }

        public void OnNextTetrominoChange(TetrominoType nextTetromino, TetrominoPosition[] nextTetrominoOffsets) {
            _nextTetromino = nextTetromino;
            _nextTetrominoOffsets = nextTetrominoOffsets;
            _nextTetrominoOffsetToCenter = GetOffsetCenterNextTetromino(nextTetromino);
        }

        public void Draw() {
            _board.Draw();
            for (byte i = 0; i < _nextTetrominoOffsets.Length; i++) {
                _nextTetrominoDestRect.X = (int)(_middleNextPanelDestRect.X + _nextTetrominoOffsets[i].X * _nextTetrominoSize + _nextTetrominoOffsetToCenter.X);
                _nextTetrominoDestRect.Y = (int)(_middleNextPanelDestRect.Y + _nextTetrominoOffsets[i].Y * _nextTetrominoSize + _nextTetrominoOffsetToCenter.Y);

                Core.SpriteBatch.Draw(_board.GetSpikyTileTexture(), _nextTetrominoDestRect, Tetromino.GetColorBasedOnType(_nextTetromino));
            }
            Core.SpriteBatch.Draw(_nextPanel, _nextPanelDestRect, Color.LightGray);
            Core.SpriteBatch.DrawString(Game.font, _nextLabel, _nextTexTPosition, Color.LightGray);


        }

        public void LoadContent() {
            _board.LoadContent();
            _nextPanel = Core.Content.Load<Texture2D>("images/next_panel");

            var textsize = Game.font.MeasureString(_nextLabel);
            _nextTexTPosition.X -= textsize.X / 2;
        }

        private Vector2 GetOffsetCenterNextTetromino(TetrominoType type) {
            return type switch {
                TetrominoType.I => new Vector2(-_nextTetrominoSize / 2, 0),
                TetrominoType.O => new Vector2(-_nextTetrominoSize / 2, -_nextTetrominoSize / 2),
                TetrominoType.T => new Vector2(0, -_nextTetrominoSize / 2),
                TetrominoType.S => new Vector2(0, -_nextTetrominoSize / 2),
                TetrominoType.Z => new Vector2(0, -_nextTetrominoSize / 2),
                TetrominoType.J => new Vector2(0, -_nextTetrominoSize / 2),
                TetrominoType.L => new Vector2(0, -_nextTetrominoSize / 2),
                _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unknown tetromino type: {type}")
            };
        }
    }
}