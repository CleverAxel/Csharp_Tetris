using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DirtyLibrary.Input {
    public class MouseManager : IMouseManager {
        private byte _clickCounter = 0;

        public Vector2 position = Vector2.Zero;
        public Vector2 mouseDownPosition = Vector2.Zero;
        public Vector2 mouseUpPosition;
        public bool _isMouseDown = false;
        public bool _isMouseUp = false;

        /// <summary>
        /// Click timeout for a double click between click
        /// </summary>
        private const ushort _clickTimeout = 350;
        private MouseState _currentMouseState;
        private MouseState _prevMouseState;
        private int _timeOfFirstClickMs = 0;

        public void Update() {
            _prevMouseState = _currentMouseState;
            _currentMouseState = Mouse.GetState();

            position.X = _currentMouseState.X;
            position.Y = _currentMouseState.Y;

            ManageClickTimeOut();
        }

        /// <summary>
        /// If used with the method HasDoubleClicked, this method must be called first
        /// </summary>
        /// <returns>True if the clicked occured before the timeout</returns>
        public bool HasLeftClicked() {
            if (_clickCounter == 0 && !IsClickInTimeOut() && IsClickAction()) {
                _timeOfFirstClickMs = GetCurrentTimestamp();
                _clickCounter++;
                return true;
            }

            return false;
        }

        /// <summary>
        /// If used with the method HasClicked, HasClicked must be called first.
        /// </summary>
        /// <returns>True if the double-clicked occured before the timeout</returns>
        public bool HasDoubleLeftClicked() {
            if (IsClickAction()) {
                //HasClicked not called before HasDoubleClicked
                if (_clickCounter == 0 && _timeOfFirstClickMs == 0) {
                    _timeOfFirstClickMs = GetCurrentTimestamp();
                    _clickCounter++;
                    return false;
                }

                if (_clickCounter >= 2 && IsClickInTimeOut()) {
                    Reset();
                    return true;
                }

                _clickCounter++;
            }

            return false;
        }

        public bool HasClicked(ref Rectangle rectangle) {
            throw new NotImplementedException();
        }

        public bool HasDoubleClicked(ref Rectangle rectangle) {
            throw new NotImplementedException();
        }

        private void ManageClickTimeOut() {
            if (_clickCounter == 0)
                return;

            if (!IsClickInTimeOut()) {
                Reset();
            }
        }

        private void Reset() {
            _timeOfFirstClickMs = 0;
            _clickCounter = 0;
        }

        private int GetCurrentTimestamp() {
            return Core.TimeElapsed;
        }

        private bool IsClickAction() {
            return _prevMouseState.LeftButton == ButtonState.Pressed && _currentMouseState.LeftButton == ButtonState.Released;
        }

        private int TimeElapsedBetweenClick() {
            return GetCurrentTimestamp() - _timeOfFirstClickMs;
        }

        private bool IsClickInTimeOut() {
            return TimeElapsedBetweenClick() < _clickTimeout;
        }

        public Vector2 GetPosition() {
            return position;
        }

        public bool InWindowBounds() {
            int windowWidth = Core.Instance.Window.ClientBounds.Width;
            int windowHeight = Core.Instance.Window.ClientBounds.Height;
            return !(position.X < 0 || position.X > windowWidth || position.Y < 0 || position.Y > windowHeight);

        }

        public bool InPlayground() {
            ref Rectangle rectangle = ref Core.s_instance.GetRenderDestRect();
            return !(position.X < rectangle.Left || position.X > rectangle.Width + rectangle.Left || position.Y < rectangle.Top || position.Y > rectangle.Height + rectangle.Top);
        }


    }


}