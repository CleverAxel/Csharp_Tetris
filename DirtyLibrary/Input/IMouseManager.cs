using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace DirtyLibrary.Input
{
    public interface IMouseManager {
        public void Update();
        public bool HasLeftClicked();
        public bool HasDoubleLeftClicked();
        public bool HasClicked(ref Rectangle rectangle);
        public bool HasDoubleClicked(ref Rectangle rectangle);
        public Vector2 GetPosition();
        public bool InWindowBounds();
        public bool InPlayground();
    }
}