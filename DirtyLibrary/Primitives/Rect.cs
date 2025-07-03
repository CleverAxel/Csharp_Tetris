using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DirtyLibrary.Primitives {
    public struct Rect {
        private Microsoft.Xna.Framework.Rectangle _rect = Microsoft.Xna.Framework.Rectangle.Empty;
        public Texture2D Color { get; set; } = TextureColor.Blue;
        public float Opacity { get; set; } = 1.0f;

        public Rect() {
        }

        public Rect(int x, int y, int w, int h) {
            _rect = new Microsoft.Xna.Framework.Rectangle(x, y, w, h);
        }

        public int W {
            get => _rect.Width;
            set {
                _rect.Width = value;
            }
        }

        public int H {
            get => _rect.Height;
            set {
                _rect.Height = value;
            }
        }

        public int X {
            get => _rect.X;
            set {
                _rect.X = value;
            }
        }

        public int Y {
            get => _rect.Y;
            set {
                _rect.Y = value;
            }
        }

        public void Draw() {
            Core.SpriteBatch.Draw(Color, _rect, Microsoft.Xna.Framework.Color.White * Opacity);
        }
    }
}