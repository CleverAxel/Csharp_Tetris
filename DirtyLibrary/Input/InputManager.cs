using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace DirtyLibrary.Input {
    public class InputManager {
        private readonly IMouseManager _mouse;
        private readonly IKeyboardManager _keyboard;

        public InputManager(IMouseManager mouseManager, IKeyboardManager keyboardManager) {
            _mouse = mouseManager;
            _keyboard = keyboardManager;
        }

        public void Update() {
            _mouse.Update();
            _keyboard.Update();
        }


        /// <summary>
        /// If used with the method HasDoubleLeftClicked, this method must be called first
        /// </summary>
        /// <returns>True if the clicked occured before the timeout</returns>
        public bool HasLeftClicked() {
            return Core.WindowHasFocus() && _mouse.HasLeftClicked();
        }

        public Vector2 GetMousePosition() {
            return _mouse.GetPosition();
        }

        public bool IsMouseInWindow() {
            return Core.WindowHasFocus() && _mouse.InWindowBounds();
        }

        public bool IsMouseInPlayGround() {
            return Core.WindowHasFocus() && _mouse.InPlayground();
        }

        public bool IsPressingUp() {
            return Core.WindowHasFocus() && _keyboard.IsPressingUp();
        }

        public bool IsReleasingUp() {
            return Core.WindowHasFocus() && _keyboard.IsReleasingUp();
        }

        public bool IsPressingDown() {
            return Core.WindowHasFocus() && _keyboard.IsPressingDown();
        }

        public bool IsPressingLeft() {
            return Core.WindowHasFocus() && _keyboard.IsPressingLeft();
        }

        public bool IsPressingRight() {
            return Core.WindowHasFocus() && _keyboard.IsPressingRight();
        }

        public bool IsPressingJump() {
            return Core.WindowHasFocus() && _keyboard.IsPressingJump();
        }

        public bool IsReleasingJump(){
            return Core.WindowHasFocus() && _keyboard.IsReleasingJump();
        }


        /// <summary>
        /// If used with the method HasLeftClicked, HasClicked must be called first.
        /// </summary>
        /// <returns>True if the double-clicked occured before the timeout</returns>
        public bool HasDoubleLeftClicked() {
            return Core.WindowHasFocus() && _mouse.HasDoubleLeftClicked();
        }
    }
}