using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DirtyLibrary;


namespace NotTetris {
    public enum PlayerAction {
        None = 0,
        LeftRight = 1 << 0,
        Fall = 1 << 1,
        Rotation = 1 << 2,
    }
    public class Player {
        private const short defaultInputTimeoutMs = 100;
        private bool _upKeyDown = false;
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


        public PlayerAction Update() {
            PlayerAction action = PlayerAction.None;

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
                if (Core.InputManager.IsPressingLeft()) {
                    lastInputTimestamp = Core.TimeElapsed;
                    _position.X--;
                    action |= PlayerAction.LeftRight;
                }

                if (Core.InputManager.IsPressingRight()) {
                    lastInputTimestamp = Core.TimeElapsed;
                    _position.X++;
                    action |= PlayerAction.LeftRight;
                }

                if (Core.InputManager.IsPressingDown()) {
                    // _position.Y++;
                    lastInputTimestamp = Core.TimeElapsed;
                    action |= PlayerAction.Fall;
                }


            }

            return action;
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