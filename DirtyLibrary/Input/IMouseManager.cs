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
        public bool HasLeftClicked(ref Rectangle rectangle);
        public bool HasDoubleLeftClicked();
        public bool HasDoubleClicked(ref Rectangle rectangle);
        public ref Vector2 GetPosition();
        public bool IsDown();
        public bool IsUp();
        public bool InWindowBounds();
        public bool InPlayground();
    }
}