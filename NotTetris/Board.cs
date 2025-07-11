using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using DirtyLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NotTetris {

    public class Board {
        public Action<TetrominoType, TetrominoPosition[]> OnNextTetrominoChange;
        public Action<int> OnScoreChange;
        public Action<int> OnLevelChange;
        public Action OnGameOver;
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
        private short ghostPieceYLevel = 0;
        private TetrominoType _nextTetromino;
        private TetrominoType _currentTetronimo;

        private byte[] _lineClearIndexes = new byte[HEIGHT];
        private bool _mustClearLine = false;
        private byte[,] _data;
        public Player Player { get; set; }
        private int _lastFallTimestamp = 0;
        private short[] _fallTimeOutsMs = [800, 720, 630, 550, 470, 380, 300, 220, 130, 100, 75];
        private byte _indexFallTimeOut = 0;
        private byte _nbrLinesCleared = 0;

        private int _score = 0;
        private int _level = 0;

        private byte _blinkCount = 0;
        private const byte maxBlinkCount = 2 * 2;
        private const short elapsedBtwBlinkTimeoutMs = 125;
        private const short allowMovementOnGroundTimeoutMs = 750;
        private short _allowMovementT = 0;
        private short _blinkT = 0;
        private byte _lineOpacity = 0;


        public Board() {
            _nextTetromino = Tetromino.GetRandomType();
            _data = Init();
            ResetLineClearIndexes();
        }

        public void Reset() {
            ResetLineClearIndexes();
            for (byte y = 0; y < HEIGHT; y++) {
                for (byte x = 0; x < WIDTH; x++) {
                    _data[y, x] = EMPTY;
                }
            }

            _score = 0;
            _level = 0;
            _indexFallTimeOut = 0;
            _lastFallTimestamp = 0;
            _tetrominoInGame = false;
            _nbrLinesCleared = 0;
            _nextTetromino = Tetromino.GetRandomType();
        }

        private void ResetLineClearIndexes() {
            for (byte i = 0; i < _lineClearIndexes.Length; i++) {
                _lineClearIndexes[i] = byte.MaxValue;
            }
        }

        public void Update() {
            //need to call player update first, else there is a small bug - no registration of the press of the space bar sometimes idk why
            PlayerAction playerAction = Player.Update();
            if (playerAction == PlayerAction.Space) {
                SetHardDropTetronimo();
                return;
            }

            if (_mustClearLine) {
                if (HasFinishedPlayingBlinkAnimation()) {
                    CalculateScoreFromErasedLines();
                    OnScoreChange(_score);
                    EraseLines();
                }
                return;
            }


            if (!_tetrominoInGame) {
                AddNewTetrominoInGame();
                return;
            }

            bool hitBottom = TetrominoHitBottom();

            //kinda of a hack to trigger the movementAppliedOrHitBottom || HasAlreadyHitBottomOnce() condition.
            //this will help detect the game over when spawning a new tetromino and that all the tiles will be above the grid.
            bool movementAppliedOrHitBottom = playerAction != PlayerAction.None || hitBottom;

            if (((playerAction & PlayerAction.Fall) != 0 || !FallInCoolDown()) && !hitBottom) {
                _lastFallTimestamp = Core.TimeElapsed;
                movementAppliedOrHitBottom = true;
                Player.ApplyFall();
                _score++;
                OnScoreChange(_score);
            }

            if ((playerAction & PlayerAction.Rotation) != 0) {
                short wallKick = GetWallKick();
                Player.ApplyWallKick(wallKick);
                bool outOfBoundsOrColliding = TetronimoOutOfBoundsOrColliding();

                byte attempt = 0;
                //Even with the wall kick if it's still colliding with something, let's rotate to see if it's possible to
                //unlock it
                while (outOfBoundsOrColliding && attempt < 3) {
                    Player.ApplyRotation();
                    outOfBoundsOrColliding = TetronimoOutOfBoundsOrColliding();
                    attempt++;
                }

                //still out of bounds or colliding, undo the movement :(
                if (outOfBoundsOrColliding)
                    Player.ApplyWallKick((short)-wallKick);
                else Core.Audio.PlaySoundEffect(Core.Audio.soundEffects["player_rotation"]);

            }

            if (((playerAction & PlayerAction.Left) != 0 || (playerAction & PlayerAction.Right) != 0) && TetronimoOutOfBoundsOrColliding()) {
                Player.UndoSideMove();
            }

            if (movementAppliedOrHitBottom || HasAlreadyHitBottomOnce()) {
                if (TetrominoHitBottom()) {
                    _allowMovementT += (short)(Core.DeltaTime * 1000.0f);
                    if (_allowMovementT >= allowMovementOnGroundTimeoutMs) {
                        _tetrominoInGame = false;
                        //this method has found some rows to clear if it returns true
                        _mustClearLine = RetrieveIndexesOfRowToClear();
                        _allowMovementT = 0;
                        Core.Audio.PlaySoundEffect(Core.Audio.soundEffects["tetromino_lock"]);
                    }

                }

                CalculateGhostPieceYLevel();
                ErasePrevTetrominoPositionFromBoard();
                DrawTetrominoToBoard();
            }
        }

        public TetrominoType GetNextTetrominoType() {
            return _nextTetromino;
        }

        private bool FallInCoolDown() {
            return Core.TimeElapsed - _lastFallTimestamp < _fallTimeOutsMs[_indexFallTimeOut];
        }

        private void SetHardDropTetronimo() {
            CalculateGhostPieceYLevel();

            int diff = ghostPieceYLevel - Player.Position.Y;
            _score += diff * 2;
            OnScoreChange(_score);
            Player.SetYLevel(ghostPieceYLevel);
            _tetrominoInGame = false;
            _allowMovementT = 0;
            ErasePrevTetrominoPositionFromBoard();
            DrawTetrominoToBoard();
            _mustClearLine = RetrieveIndexesOfRowToClear();
            Core.Audio.PlaySoundEffect(Core.Audio.soundEffects["tetromino_lock"]);
        }

        private bool HasFinishedPlayingBlinkAnimation() {
            _blinkT += (short)(Core.DeltaTime * 1000.0f);
            if (_blinkCount < maxBlinkCount) {
                if (_blinkT >= elapsedBtwBlinkTimeoutMs) {
                    if (_lineOpacity == 0) {
                        Core.Audio.PlaySoundEffect(Core.Audio.soundEffects["tetromino_disappear"]);
                    }
                    _blinkT -= elapsedBtwBlinkTimeoutMs;
                    _blinkCount++;
                    _lineOpacity = (byte)(_blinkCount % 2);
                }
            } else {
                _lineOpacity = 0;

                if (_blinkT >= elapsedBtwBlinkTimeoutMs) {
                    if (_lineOpacity == 0) {
                        Core.Audio.PlaySoundEffect(Core.Audio.soundEffects["tetromino_disappear"]);
                    }
                    _blinkCount = 0;
                    _blinkT -= elapsedBtwBlinkTimeoutMs;
                    _mustClearLine = false;
                    return true;
                }
            }

            return false;
        }

        private void CalculateGhostPieceYLevel() {
            short y = Player.Position.Y;
            while (y < HEIGHT) {
                for (short i = 0; i < _tetrominoOffsets[Player.Rotation].Length; i++) {
                    short x = (short)(Player.Position.X + _tetrominoOffsets[Player.Rotation][i].X);
                    short yOffset = (short)(y + _tetrominoOffsets[Player.Rotation][i].Y);

                    bool inPrevPosition = IsInPreviousPosition(x, yOffset);
                    if (yOffset >= HEIGHT || (yOffset >= 0 && InBoardXBounds(x) && !inPrevPosition && _data[yOffset, x] != EMPTY)) {
                        ghostPieceYLevel = (short)(y - 1);
                        return;
                    }
                }

                y++;
            }
            ghostPieceYLevel = HEIGHT - 1;
        }

        private bool TetrominoHitBottom() {
            short playerPosition = (short)(Player.Position.Y + 1);
            for (short i = 0; i < _tetrominoOffsets[Player.Rotation].Length; i++) {

                short X = (short)(Player.Position.X + _tetrominoOffsets[Player.Rotation][i].X);
                short Y = (short)(playerPosition + _tetrominoOffsets[Player.Rotation][i].Y);

                bool inPrevPosition = IsInPreviousPosition(X, Y);
                if (Y >= HEIGHT || (Y >= 0 && InBoardXBounds(X) && !inPrevPosition && _data[Y, X] != EMPTY))
                    return true;
            }

            return false;
        }

        private short GetLineClearIndex(byte y) {
            for (short i = 0; i < _lineClearIndexes.Length; i++) {
                if (_lineClearIndexes[i] == y)
                    return i;
            }
            return -1;
        }

        private void AddNewTetrominoInGame() {
            _currentTetronimo = _nextTetromino;
            _nextTetromino = Tetromino.GetRandomType();
            while (_nextTetromino == _currentTetronimo) {
                _nextTetromino = Tetromino.GetRandomType();
            }

            OnNextTetrominoChange(_nextTetromino, Tetromino.GetOffsetsFromTypeAndRotation(_nextTetromino)[0]);

            Player.ResetPositionRotation();

            _tetrominoOffsets = Tetromino.GetOffsetsFromTypeAndRotation(_currentTetronimo);
            _tetrominoInGame = true;

            FixYPlayerPosition();
            if (AllTilesAboveGrid()) {
                OnGameOver();
                return;
            }

            DrawTetrominoToBoard();
            CalculateGhostPieceYLevel();
            _lastFallTimestamp = Core.TimeElapsed;
            Core.Audio.PlaySoundEffect(Core.Audio.soundEffects["tetromino_spawn"]);
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

                bool inPrevPosition = IsInPreviousPosition(X, Y);
                //Y >= HEIGHT -> only check if it's bellow the grid to give the player the possibility to rotate a piece
                // up the grid and be out of bounds
                //inPrevPosition -> Do not give a false positive if it was the previous position of the tetromino in play
                if (Y >= HEIGHT || !InBoardXBounds(X) || (InBoardYBounds(Y) && !inPrevPosition && _data[Y, X] != EMPTY))
                    return true;
            }

            return false;
        }

        private bool IsInPreviousPosition(short x, short y) {
            for (int i = 0; i < _previousTetrominoPosition.Length; i++) {
                if (_previousTetrominoPosition[i].X == x && _previousTetrominoPosition[i].Y == y)
                    return true;
            }
            return false;
        }

        private bool AllTilesAboveGrid() {
            byte tileCount = 0;
            for (short i = 0; i < _tetrominoOffsets[Player.Rotation].Length; i++) {

                short Y = (short)(Player.Position.Y + _tetrominoOffsets[Player.Rotation][i].Y);

                if (Y < 0) {
                    tileCount++;
                    if (tileCount == 4)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Will fix the Y player position if one of the offsets of the tetromino is colliding with 
        /// a tile already in place
        /// </summary>
        private void FixYPlayerPosition() {
            while (true) {
                bool neededToFixPosition = false;
                for (short i = 0; i < _tetrominoOffsets[Player.Rotation].Length; i++) {

                    short X = (short)(Player.Position.X + _tetrominoOffsets[Player.Rotation][i].X);
                    short Y = (short)(Player.Position.Y + _tetrominoOffsets[Player.Rotation][i].Y);

                    if (InBoardYBounds(Y) && _data[Y, X] != EMPTY) {
                        Player.UnApplyFall();
                        neededToFixPosition = true;
                        break;
                    }
                }
                if (!neededToFixPosition)
                    break;
            }
        }

        private void DrawGhostPiece() {
            if (_mustClearLine)
                return;

            for (short i = 0; i < _tetrominoOffsets[Player.Rotation].Length; i++) {

                short X = (short)(Player.Position.X + _tetrominoOffsets[Player.Rotation][i].X);
                short Y = (short)(ghostPieceYLevel + _tetrominoOffsets[Player.Rotation][i].Y);

                _destRect.Y = (Y + 1) * CELL_SIZE;
                _destRect.X = (X + 1) * CELL_SIZE;

                Core.SpriteBatch.Draw(_spikyTile, _destRect, Color.White * 0.33f);
            }
        }

        public void Draw() {

            DrawGhostPiece();

            for (byte y = 0; y < HEIGHT; y++) {
                float opacity = 1.0f;

                //this row need to change of opacity, it will be cleared
                if (_mustClearLine) {
                    short index = GetLineClearIndex(y);
                    if (index != -1) {
                        opacity = _lineOpacity;
                    }
                }

                _destRect.Y = (y + 1) * CELL_SIZE;
                for (byte x = 0; x < WIDTH; x++) {
                    _destRect.X = (x + 1) * CELL_SIZE;

                    switch (_data[y, x]) {
                        case EMPTY:
                            break;
                        default:
                            Core.SpriteBatch.Draw(_spikyTile, _destRect, Tetromino.GetColorBasedOnType((TetrominoType)_data[y, x]) * opacity);
                            break;


                    }
                }
            }

            DrawBorder();
        }

        private void DrawBorder() {
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

        private bool InBoardXBounds(short x) {
            return x >= 0 && x < WIDTH;
        }
        private bool InBoardYBounds(short y) {
            return y >= 0 && y < HEIGHT;
        }

        public Texture2D GetSpikyTileTexture() {
            return _spikyTile;
        }

        public void LoadContent() {
            _spikyTile = Core.Content.Load<Texture2D>("images/spiky_tiles");
        }

        public void ErasePrevTetrominoPositionFromBoard() {
            foreach (var item in _previousTetrominoPosition) {
                if (InBoardYBounds(item.Y) && InBoardXBounds(item.X))
                    _data[item.Y, item.X] = EMPTY;
            }
        }

        public void CalculateScoreFromErasedLines() {
            byte i = 0;
            while (_lineClearIndexes[i] != byte.MaxValue) {

                byte currentValue = _lineClearIndexes[i];
                byte j = (byte)(i + 1);
                byte count = 1;
                while (j < _lineClearIndexes.Length && count < 4 && currentValue == _lineClearIndexes[j] + 1) {
                    currentValue = _lineClearIndexes[j];
                    count++;
                    j++;
                }

                i = j;

                _nbrLinesCleared += count;
                if (_nbrLinesCleared >= 10) {
                    _nbrLinesCleared = 0;
                    _level++;
                    OnLevelChange(_level);
                    if (_indexFallTimeOut < _fallTimeOutsMs.Length) {
                        _indexFallTimeOut++;
                    }
                }

                _score += count switch {
                    1 => 100 * (_level + 1),
                    2 => 300 * (_level + 1),
                    3 => 500 * (_level + 1),
                    4 => 800 * (_level + 1),
                    _ => throw new NotSupportedException($"Count is at {count}"),
                };
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



        private void EraseLines() {
            short y = HEIGHT - 1;
            byte indexLineClear = 0;
            while (y >= 0) {
                short x = 0;
                bool foundEmptySpot = false;
                while (x < WIDTH && !foundEmptySpot) {
                    foundEmptySpot = _data[y, x] == EMPTY;
                    x++;
                }
                if (!foundEmptySpot) {
                    _lineClearIndexes[indexLineClear] = (byte)y;
                    indexLineClear++;
                    for (x = 0; x < WIDTH; x++) {
                        _data[y, x] = EMPTY;
                    }

                    for (x = 0; x < WIDTH; x++) {
                        short __y = y;
                        short _y = (short)(y - 1);
                        while (_y >= 0) {
                            _data[__y, x] = _data[_y, x];
                            _y--;
                            __y--;
                        }

                    }
                } else {
                    y--;
                }
            }
        }

        private bool RetrieveIndexesOfRowToClear() {
            bool foundLinesToClear = false;
            ResetLineClearIndexes();
            short y = HEIGHT - 1;
            byte indexLineClear = 0;

            while (y >= 0) {
                short x = 0;
                bool foundEmptySpot = false;
                while (x < WIDTH && !foundEmptySpot) {
                    foundEmptySpot = _data[y, x] == EMPTY;
                    x++;
                }
                if (!foundEmptySpot) {
                    foundLinesToClear = true;
                    _lineClearIndexes[indexLineClear] = (byte)y;
                    indexLineClear++;
                }

                y--;

            }

            return foundLinesToClear;
        }
        private bool HasAlreadyHitBottomOnce() {
            return _allowMovementT != 0;
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