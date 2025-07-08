using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DirtyLibrary.Input.Components {
    public class Button {
        private Rectangle _destRect = Rectangle.Empty;
        private Rectangle _SrcRect = Rectangle.Empty;

        private Vector2 _mouseDownPosition;
        private bool _isMouseDown;

        public Action<Button> OnMouseEnter;
        public Action<Button> OnMouseLeave;
        public Action<Button> OnClick;

        public void Update() {
            if (!_isMouseDown && Core.InputManager.IsMouseDown()) {
                _isMouseDown = true;
                _mouseDownPosition.X = Core.InputManager.GetMousePosition().X;
                _mouseDownPosition.Y = Core.InputManager.GetMousePosition().Y;

            } else if (_isMouseDown && Core.InputManager.IsMouseUp()) {
                _isMouseDown = false;
                if (_destRect.Contains(Core.InputManager.GetMousePosition()) && _destRect.Contains(_mouseDownPosition)) {
                    OnClick(this);
                }
            }

            if (_destRect.Contains(Core.InputManager.GetMousePosition())) {
                OnMouseEnter(this);
            } else {
                OnMouseLeave(this);
            }
        }

        public void Draw(Texture2D uiAtlas) {
            Core.SpriteBatch.Draw(uiAtlas, _destRect, _SrcRect, Color.White);
        }

        public void SetDestRect(int x, int y, int w, int h) {
            _destRect.X = x;
            _destRect.Y = y;
            _destRect.Width = w;
            _destRect.Height = h;
        }

        public void SetSrcRect(int x, int y, int w, int h) {
            _SrcRect.X = x;
            _SrcRect.Y = y;
            _SrcRect.Width = w;
            _SrcRect.Height = h;
        }
        public void SetSrcRect(int x, int y) {
            _SrcRect.X = x;
            _SrcRect.Y = y;
        }


        public void AddOnMouseEnter(Action<Button> action) {
            OnMouseEnter = action;
        }
        public void AddOnMouseLeave(Action<Button> action) {
            OnMouseLeave = action;
        }
        public void AddOnClick(Action<Button> action) {
            OnClick = action;
        }

    }
}