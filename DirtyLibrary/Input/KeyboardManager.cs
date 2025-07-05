using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;

namespace DirtyLibrary.Input {
    public class KeyboardManager : IKeyboardManager {
        private KeyboardState _currentKeyboardState;
        private KeyboardState _prevKeyboardState;
        public Keys Down { get; set; } = Keys.Down;
        public Keys Up { get; set; } = Keys.Up;
        public Keys Left { get; set; } = Keys.Left;
        public Keys Right { get; set; } = Keys.Right;
        public Keys Jump { get; set; } = Keys.Space;
        public bool IsPressingDown() {
            return _currentKeyboardState.IsKeyDown(Down);
        }

        public bool IsPressingJump() {
            return _currentKeyboardState.IsKeyDown(Jump);
        }
        
        public bool IsReleasingJump() {
            return _prevKeyboardState.IsKeyDown(Jump) && _currentKeyboardState.IsKeyUp(Jump);
        }

        public bool IsPressingLeft() {
            return _currentKeyboardState.IsKeyDown(Left);
        }

        public bool IsPressingRight() {
            return _currentKeyboardState.IsKeyDown(Right);
        }

        public bool IsPressingUp() {
            return _currentKeyboardState.IsKeyDown(Up);
        }


        public bool IsReleasingUp() {
            return _prevKeyboardState.IsKeyDown(Up) && _currentKeyboardState.IsKeyUp(Up); 
        }

        public void Update() {
            _prevKeyboardState = _currentKeyboardState;
            _currentKeyboardState = Keyboard.GetState();
        }
    }
}