using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using DirtyLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NotTetris {

    public class Board {
        public const byte WIDTH = 10;
        public const byte HEIGHT = 20;

        public const byte WIDTH_WITH_BORDER = WIDTH + 2;
        public const byte HEIGHT_WITH_BORDER = HEIGHT + 2;
        public const byte CELL_SIZE = 32;
        private const byte EMPTY = 0;

        private Rectangle _destRect = new Rectangle(0, 0, CELL_SIZE, CELL_SIZE);
        private Texture2D _spikyTile;

        private bool _tetrominoInGame = false;
        private TetrominoPosition[][] _tetrominoOffsets;
        private TetrominoPosition[] _previousTetrominoPosition = new TetrominoPosition[4];

        private TetrominoType _nextTetromino;
        private TetrominoType _currentTetronimo;

        private byte[,] _data;
        public Player Player { get; set; }
        private int _lastFallTimestamp = 0;
        private short _fallTimeout = 1000;

        public Board() {
            _nextTetromino = Tetromino.GetRandomType();
            _data = Init();
        }

        public void Update() {
            if (!_tetrominoInGame) {
                AddNewTetrominoInGame();
                _lastFallTimestamp = Core.TimeElapsed;
                return;
            }

            bool fallApplied = false;

            PlayerAction playerAction = Player.Update();

            if ((playerAction & PlayerAction.Fall) != 0) {
                fallApplied = true;
                _lastFallTimestamp = Core.TimeElapsed;
            }

            if (!FallInCoolDown()) {
                _lastFallTimestamp = Core.TimeElapsed;
                fallApplied = true;
                Player.ApplyFall();
            }

            if ((playerAction & PlayerAction.Rotation) != 0) {
                short wallKick = GetWallKick();
                Player.ApplyWallKick(wallKick);
                bool outOfBoundsOrColliding = TetronimoOutOfBoundsOrColliding();
                byte attempt = 0;
                //Even with the wall kick it's still colliding with something, let's rotate to see if it's possible to
                //unlock it
                while (outOfBoundsOrColliding && attempt < 4) {
                    Player.ApplyRotation();
                    outOfBoundsOrColliding = TetronimoOutOfBoundsOrColliding();
                    attempt++;
                }

                //still out of bounds or colliding, undo the movement :(
                if (outOfBoundsOrColliding)
                    Player.ApplyWallKick((short)-wallKick);



            }
            if ((playerAction & PlayerAction.LeftRight) != 0 && TetrominoHitWallOrOtherTetromino()) {
                Player.UndoLateralMove();
            }

            if (fallApplied && TetrominoHitBottom()) {
                Player.UnApplyFall();
                _tetrominoInGame = false;
            }


            EraseTetrominoInGameFromBoard();
            DrawTetrominoToBoard();



        }

        private bool FallInCoolDown() {
            return Core.TimeElapsed - _lastFallTimestamp < _fallTimeout;
        }

        private bool TetrominoHitBottom() {
            for (short i = 0; i < _tetrominoOffsets[Player.Rotation].Length; i++) {

                short X = (short)(Player.Position.X + _tetrominoOffsets[Player.Rotation][i].X);
                short Y = (short)(Player.Position.Y + _tetrominoOffsets[Player.Rotation][i].Y);

                bool inPrevPosition = _previousTetrominoPosition.Any((position) => position.X == X && position.Y == Y);
                if (!InBoardYBounds(Y) || (!inPrevPosition && _data[Y, X] != EMPTY))
                    return true;
            }

            return false;
        }

        private bool TetrominoHitWallOrOtherTetromino() {
            for (short i = 0; i < _tetrominoOffsets[Player.Rotation].Length; i++) {

                short X = (short)(Player.Position.X + _tetrominoOffsets[Player.Rotation][i].X);
                short Y = (short)(Player.Position.Y + _tetrominoOffsets[Player.Rotation][i].Y);
                bool inPrevPosition = _previousTetrominoPosition.Any((position) => position.X == X && position.Y == Y);

                if (!InBoardXBounds(X) || !InBoardYBounds(Y) || (!inPrevPosition && _data[Y, X] != EMPTY))
                    return true;
            }

            return false;
        }



        private void AddNewTetrominoInGame() {
            _currentTetronimo = _nextTetromino;
            _nextTetromino = Tetromino.GetRandomType();
            while (_nextTetromino == _currentTetronimo) {
                _nextTetromino = Tetromino.GetRandomType();
            }

            Player.ResetPositionRotation();

            _tetrominoOffsets = Tetromino.GetOffsetsFromTypeAndRotation(_currentTetronimo);
            _tetrominoInGame = true;
            short wallKick = GetWallKick();

            Player.Position = new TetrominoPosition((short)(Player.Position.X + wallKick), Player.Position.Y);

            DrawTetrominoToBoard();
        }

        /// <summary>
        /// Will resolve only the X axis. If the tetromino is outside the grid by a rotation
        /// this method will calculate the offset to fix that
        /// </summary>
        /// <returns>return a X offset to replace the tetronimo in the grid</returns>
        private short GetWallKick() {
            TetrominoPosition[] offsets = _tetrominoOffsets[Player.Rotation];
            short correctedOffset = 0;

            foreach (var offset in offsets) {

                short offsetPosition = (short)(Player.Position.X + offset.X);
                short tempCorrectedOffset = 0;

                while (!InBoardXBounds(offsetPosition)) {
                    if (offsetPosition < 1) {
                        offsetPosition++;
                        tempCorrectedOffset++;
                    } else {
                        offsetPosition--;
                        tempCorrectedOffset--;
                    }
                }

                if (Math.Abs(tempCorrectedOffset) > Math.Abs(correctedOffset)) {
                    correctedOffset = tempCorrectedOffset;
                }
            }

            return correctedOffset;
        }

        private bool TetronimoOutOfBoundsOrColliding() {
            for (short i = 0; i < _tetrominoOffsets[Player.Rotation].Length; i++) {

                short X = (short)(Player.Position.X + _tetrominoOffsets[Player.Rotation][i].X);
                short Y = (short)(Player.Position.Y + _tetrominoOffsets[Player.Rotation][i].Y);

                bool inPrevPosition = _previousTetrominoPosition.Any((position) => position.X == X && position.Y == Y);
                if (Y >= HEIGHT || !InBoardXBounds(X) || (InBoardYBounds(Y) && !inPrevPosition && _data[Y, X] != EMPTY))
                    return true;
            }

            return false;
        }

        public void Draw() {
            for (byte y = 0; y < HEIGHT; y++) {
                _destRect.Y = (y + 1) * CELL_SIZE;
                for (byte x = 0; x < WIDTH; x++) {
                    _destRect.X = (x + 1) * CELL_SIZE;

                    switch (_data[y, x]) {
                        case EMPTY:
                            break;

                        case (byte)TetrominoType.I:
                            Core.SpriteBatch.Draw(_spikyTile, _destRect, Color.Cyan);
                            break;

                        case (byte)TetrominoType.O:
                            Core.SpriteBatch.Draw(_spikyTile, _destRect, Color.Yellow);
                            break;

                        case (byte)TetrominoType.T:
                            Core.SpriteBatch.Draw(_spikyTile, _destRect, Color.Magenta);
                            break;

                        case (byte)TetrominoType.S:
                            Core.SpriteBatch.Draw(_spikyTile, _destRect, Color.LightGreen);
                            break;

                        case (byte)TetrominoType.Z:
                            Core.SpriteBatch.Draw(_spikyTile, _destRect, Color.OrangeRed);
                            break;

                        case (byte)TetrominoType.J:
                            Core.SpriteBatch.Draw(_spikyTile, _destRect, Color.SteelBlue);
                            break;

                        case (byte)TetrominoType.L:
                            Core.SpriteBatch.Draw(_spikyTile, _destRect, Color.Orange);
                            break;
                    }
                }
            }

            //border left - right
            for (byte y = 0; y < HEIGHT_WITH_BORDER; y++) {
                _destRect.Y = y * CELL_SIZE;
                _destRect.X = 0;

                Core.SpriteBatch.Draw(_spikyTile, _destRect, Color.DarkGray);
                _destRect.X = (WIDTH + 1) * CELL_SIZE;
                Core.SpriteBatch.Draw(_spikyTile, _destRect, Color.DarkGray);
            }

            // border top - bottom
            for (byte x = 1; x < WIDTH + 1; x++) {
                _destRect.Y = 0;
                _destRect.X = x * CELL_SIZE;
                Core.SpriteBatch.Draw(_spikyTile, _destRect, Color.DarkGray);
                _destRect.Y = (HEIGHT_WITH_BORDER - 1) * CELL_SIZE;
                Core.SpriteBatch.Draw(_spikyTile, _destRect, Color.DarkGray);
            }
        }

        private bool InBoardBounds(ref TetrominoPosition position) {
            return position.X >= 0 && position.X < WIDTH && position.Y >= 0 && position.Y < HEIGHT;
        }
        private bool InBoardXBounds(short x) {
            return x >= 0 && x < WIDTH;
        }
        private bool InBoardYBounds(short y) {
            return y >= 0 && y < HEIGHT;
        }

        public void LoadContent() {
            _spikyTile = Core.Content.Load<Texture2D>("images/spiky_tiles");
        }

        public void EraseTetrominoInGameFromBoard() {
            foreach (var item in _previousTetrominoPosition) {
                if (InBoardYBounds(item.Y) && InBoardXBounds(item.X))
                    _data[item.Y, item.X] = EMPTY;
            }
        }

        public void DrawTetrominoToBoard() {

            for (short i = 0; i < _tetrominoOffsets[Player.Rotation].Length; i++) {

                short X = (short)(Player.Position.X + _tetrominoOffsets[Player.Rotation][i].X);
                short Y = (short)(Player.Position.Y + _tetrominoOffsets[Player.Rotation][i].Y);

                _previousTetrominoPosition[i].X = X;
                _previousTetrominoPosition[i].Y = Y;

                if (InBoardYBounds(Y) && InBoardXBounds(X))
                    _data[Y, X] = (byte)_currentTetronimo;
            }

        }

        private byte[,] Init() {
            return new byte[HEIGHT, WIDTH] {
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0}
            };
        }
    }
}