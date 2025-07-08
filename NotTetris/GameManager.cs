
using System;
using System.Collections.Generic;
using DirtyLibrary;
using DirtyLibrary.Input.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace NotTetris {
    public class GameManager {
        private Board _board;
        private Player _player;

        private Texture2D _uiAtlas;


        #region NEXT TETROMINO UI
        private Vector2 _nextTexTPosition;
        private const string _nextLabel = "next";
        private Rectangle _nextPanelDestRect;
        private Rectangle _nextPanelSrcRect = new Rectangle(0, 0, 32, 32);
        private TetrominoType _nextTetromino;
        private const short _nextTetrominoSize = 32;
        private Rectangle _nextTetrominoDestRect;
        private TetrominoPosition[] _nextTetrominoOffsets = new TetrominoPosition[4];
        private Vector2 _nextTetrominoOffsetToCenter; //offset to center the next tetromino in the destRect
        private Vector2 _middleNextPanel;
        #endregion


        #region SCORE UI
        private const string _scoreLabel = "score:";
        private string _score = "0";
        private Vector2 _scoreLabelTextPosition;
        private Vector2 _scoreTextPosition;
        #endregion


        private List<Button> _buttons = new List<Button>(2);


        private const string _levelLabel = "Level: ";
        private string _level = "0";
        private Vector2 _levelLabelTextPosition;
        private Vector2 _levelTextPosition;


        private ushort _uiLeft;
        private ushort _uiWidth;
        private bool _isPaused = false;
        private bool _isGameOver = false;
        public GameManager(Board board, Player player) {
            _board = board;
            _player = player;
            _uiLeft = Board.CELL_SIZE * Board.WIDTH_WITH_BORDER;
            _uiWidth = Game.VIRTUAL_WIDTH - Board.CELL_SIZE * Board.WIDTH_WITH_BORDER;

            _board.OnNextTetrominoChange = OnNextTetrominoChange;
            _board.OnScoreChange = OnScoreChange;
            _board.OnLevelChange = OnLevelChange;
            _board.OnGameOver = OnGameOver;


            const float buttonScale = 3;
            const int buttonWidth = (int)(46.0f * buttonScale);
            const int buttonHeight = (int)(14.0f * buttonScale);
            Button buttonPause = new Button();
            buttonPause.AddOnMouseEnter(b => b.SetSrcRect(46, 32));
            buttonPause.AddOnMouseLeave(b => b.SetSrcRect(0, 32));
            buttonPause.AddOnClick((b) => { _isPaused = true; MediaPlayer.Pause(); });
            byte gap = 10;
            buttonPause.SetDestRect(_uiLeft + _uiWidth / 2 - buttonWidth - gap / 2, 475, buttonWidth, buttonHeight);
            buttonPause.SetSrcRect(0, 32, 46, 14);
            _buttons.Add(buttonPause);

            Button buttonPlay = new Button();
            buttonPlay.AddOnMouseEnter(b => b.SetSrcRect(32 + 46, 0));
            buttonPlay.AddOnMouseLeave(b => b.SetSrcRect(32, 0));
            buttonPlay.AddOnClick((b) => { _isPaused = false; MediaPlayer.Resume(); });
            buttonPlay.SetDestRect(_uiLeft + _uiWidth / 2 + gap / 2, 475, buttonWidth, buttonHeight);
            buttonPlay.SetSrcRect(32, 0, 46, 14);
            _buttons.Add(buttonPlay);

            Button buttonReset = new Button();
            buttonReset.AddOnMouseEnter(b => b.SetSrcRect(46, 46));
            buttonReset.AddOnMouseLeave(b => b.SetSrcRect(0, 46));
            buttonReset.AddOnClick((b) => Reset());
            buttonReset.SetDestRect(_uiLeft + _uiWidth / 2 - buttonWidth / 2, 475 + gap + buttonHeight, buttonWidth, buttonHeight);
            buttonReset.SetSrcRect(0, 46, 46, 14);
            _buttons.Add(buttonReset);

        }

        public void Update() {

            foreach (var button in _buttons) {
                button.Update();
            }

            if (!_isPaused && !_isGameOver)
                _board.Update();

        }

        private void OnNextTetrominoChange(TetrominoType nextTetromino, TetrominoPosition[] nextTetrominoOffsets) {
            _nextTetromino = nextTetromino;
            _nextTetrominoOffsets = nextTetrominoOffsets;
            _nextTetrominoOffsetToCenter = GetOffsetCenterNextTetromino(nextTetromino);
        }

        private void OnScoreChange(int score) {
            _score = score.ToString();
            _scoreTextPosition.X = (short)(_uiLeft + _uiWidth / 2 - Game.font.MeasureString(_score).X / 2);
        }

        private void OnLevelChange(int level) {
            _level = level.ToString();
            var labelSize = Game.font.MeasureString(_levelLabel);
            var valueSize = Game.font.MeasureString(_level);

            _levelLabelTextPosition.X = _uiLeft + _uiWidth / 2 - (labelSize.X + valueSize.X) / 2;
            _levelTextPosition.X = _levelLabelTextPosition.X + labelSize.X;
        }

        public void Draw() {
            _board.Draw();

            DrawNextTetromino();
            Core.SpriteBatch.DrawString(Game.font, _nextLabel, _nextTexTPosition, Color.LightGray);

            Core.SpriteBatch.DrawString(Game.font, _scoreLabel, _scoreLabelTextPosition, Color.LightGray);
            Core.SpriteBatch.DrawString(Game.font, _score, _scoreTextPosition, Color.White);

            Core.SpriteBatch.DrawString(Game.font, _levelLabel, _levelLabelTextPosition, Color.White);
            Core.SpriteBatch.DrawString(Game.font, _level, _levelTextPosition, Color.White);

            foreach (var button in _buttons) {
                button.Draw(_uiAtlas);
            }
        }

        private void DrawNextTetromino() {
            for (byte i = 0; i < _nextTetrominoOffsets.Length; i++) {
                _nextTetrominoDestRect.X = (int)(_middleNextPanel.X + _nextTetrominoOffsets[i].X * _nextTetrominoSize + _nextTetrominoOffsetToCenter.X);
                _nextTetrominoDestRect.Y = (int)(_middleNextPanel.Y + _nextTetrominoOffsets[i].Y * _nextTetrominoSize + _nextTetrominoOffsetToCenter.Y);

                Core.SpriteBatch.Draw(_board.GetSpikyTileTexture(), _nextTetrominoDestRect, Tetromino.GetColorBasedOnType(_nextTetromino));
            }
            Core.SpriteBatch.Draw(_uiAtlas, _nextPanelDestRect, _nextPanelSrcRect, Color.White);
        }

        public void LoadContent() {
            _board.LoadContent();
            _uiAtlas = Core.Content.Load<Texture2D>("images/ui_atlas");

            //need to call it inside LoadContent, because it's using a FONT.
            InitializeUI();
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

        private void InitNextTetrominoUI() {
            _nextPanelDestRect.Width = 128 + 64;
            _nextPanelDestRect.Height = _nextPanelDestRect.Width;

            short middleWidth = (short)(_uiLeft + _uiWidth / 2 - _nextPanelDestRect.Width / 2);
            _nextPanelDestRect.X = middleWidth;
            _nextPanelDestRect.Y = 28;

            _nextTexTPosition.X = _nextPanelDestRect.X + _nextPanelDestRect.Width / 2;
            _nextTexTPosition.Y = _nextPanelDestRect.Y - 28;

            var textsize = Game.font.MeasureString(_nextLabel);
            _nextTexTPosition.X -= textsize.X / 2;

            _middleNextPanel.X = _nextPanelDestRect.X + _nextPanelDestRect.Width / 2 - _nextTetrominoSize / 2;
            _middleNextPanel.Y = _nextPanelDestRect.Y + 3 + _nextPanelDestRect.Height / 2 - _nextTetrominoSize / 2;

            _nextTetrominoDestRect.Width = _nextTetrominoSize;
            _nextTetrominoDestRect.Height = _nextTetrominoSize;
        }

        private void InitScoreUI() {
            _scoreLabelTextPosition.X = _nextPanelDestRect.X + _nextPanelDestRect.Width / 2;
            _scoreLabelTextPosition.Y = _nextPanelDestRect.Y + _nextPanelDestRect.Height + 14;

            var textsize = Game.font.MeasureString(_scoreLabel);
            _scoreLabelTextPosition.X -= textsize.X / 2;

            _score = "0";
            textsize = Game.font.MeasureString(_score);
            _scoreTextPosition.X = (short)(_uiLeft + _uiWidth / 2 - textsize.X / 2);
            _scoreTextPosition.Y = _scoreLabelTextPosition.Y + textsize.Y * 0.7f;
        }

        private void InitLevelUI() {
            var labelSize = Game.font.MeasureString(_levelLabel);
            var valueSize = Game.font.MeasureString(_level);

            _levelLabelTextPosition.X = (short)(_uiLeft + _uiWidth / 2 - (labelSize.X + valueSize.X) / 2);
            _levelLabelTextPosition.Y = _scoreTextPosition.Y + labelSize.Y + 20;

            _levelTextPosition.X = _levelLabelTextPosition.X + labelSize.X;
            _levelTextPosition.Y = _levelLabelTextPosition.Y;
        }

        private void InitializeUI() {
            InitNextTetrominoUI();
            InitScoreUI();
            InitLevelUI();
        }

        private void OnGameOver() {
            _isGameOver = true;
            _isPaused = true;
            MediaPlayer.Pause();
        }

        private void Reset() {
            _isGameOver = false;
            _isPaused = false;
            MediaPlayer.Resume();
            _board.Reset();
            _level = "0";
            _score = "0";
        }
    }
}