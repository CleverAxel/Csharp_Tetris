using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DirtyLibrary;


namespace NotTetris {
    public enum PlayerAction {
        None = 0,
        Space = 1 << 0,
        Fall = 1 << 1,
        Rotation = 1 << 2,
        Left = 1 << 3,
        Right = 1 << 4,
    }
    public class Player {
        private bool _prevActionWasLeft = false;
        private bool _prevActionWasRight = false;
        private const short defaultInputTimeoutMs = 100;
        private bool _upKeyDown = false;
        private bool _spaceKeyDown = false;
        private int lastInputTimestamp = 0;

        private TetrominoPosition _position;
        public TetrominoPosition Position {
            get => _position;
            set => _position = value;
        }

        private TetrominoPosition _prevPosition;
        public TetrominoPosition PrevPosition {
            get => _prevPosition;
            set => _prevPosition = value;
        }

        public byte Rotation { get; set; }
        private Random _r = new Random();


        public PlayerAction Update() {
            PlayerAction action = PlayerAction.None;

            if (!_spaceKeyDown && Core.InputManager.IsPressingJump()) {
                _spaceKeyDown = true;
                 lastInputTimestamp = Core.TimeElapsed + defaultInputTimeoutMs;
                return PlayerAction.Space;
            } else if (_spaceKeyDown && Core.InputManager.IsReleasingJump()) {
                _spaceKeyDown = false;
            }

            if (!_upKeyDown && Core.InputManager.IsPressingUp()) {
                ApplyRotation();
                action |= PlayerAction.Rotation;
                _upKeyDown = true;
            }

            if (_upKeyDown && Core.InputManager.IsReleasingUp()) {
                _upKeyDown = false;
            }

            if (!InputInCoolDown()) {
                _prevPosition.X = _position.X;
                _prevPosition.Y = _position.Y;

                bool isPressingLeft = Core.InputManager.IsPressingLeft();
                bool isPressingRight = Core.InputManager.IsPressingRight();
                bool isNotPressingOnBothLeftRight = isPressingLeft ^ isPressingRight;

                if (isNotPressingOnBothLeftRight) {
                    if (isPressingLeft) {
                        if (!_prevActionWasLeft) {
                            Core.Audio.PlaySoundEffect(Core.Audio.soundEffects["player_movement"], 1.0f, (float)(_r.NextDouble() * 0.2 - 0.1), 0.0f, false);
                        }
                        lastInputTimestamp = Core.TimeElapsed;
                        _position.X--;
                        action |= PlayerAction.Left;

                        _prevActionWasLeft = true;
                        _prevActionWasRight = false;
                    }

                    if (isPressingRight) {
                        if (!_prevActionWasRight) {
                            Core.Audio.PlaySoundEffect(Core.Audio.soundEffects["player_movement"], 1.0f, (float)(_r.NextDouble() * 0.2 - 0.1), 0.0f, false);
                        }
                        lastInputTimestamp = Core.TimeElapsed;
                        _position.X++;
                        action |= PlayerAction.Right;

                        _prevActionWasLeft = false;
                        _prevActionWasRight = true;
                    }
                }

                if (!isPressingLeft && !isPressingRight) {
                    _prevActionWasLeft = false;
                    _prevActionWasRight = false;
                }

                if (Core.InputManager.IsPressingDown()) {
                    // _position.Y++;
                    lastInputTimestamp = Core.TimeElapsed;
                    action |= PlayerAction.Fall;
                }

            }
            return action;
        }

        public void SetYLevel(short y) {
            _position.Y = y;
        }

        public void UndoSideMove() {
            _position.X = _prevPosition.X;
        }

        public void ResetPositionRotation() {
            Rotation = 0;
            _position.X = Board.WIDTH / 2;
            _position.Y = 0;
        }

        public void ApplyFall() {
            _position.Y += 1;
        }

        public void UnApplyFall() {
            _position.Y -= 1;
        }

        public void ApplyRotation() {
            Rotation = (byte)((Rotation + 1) % 4);
        }

        public void ApplyWallKick(short offset) {
            _position.X += offset;
        }



        private bool InputInCoolDown() {
            return Core.TimeElapsed - lastInputTimestamp < defaultInputTimeoutMs;
        }

    }
}