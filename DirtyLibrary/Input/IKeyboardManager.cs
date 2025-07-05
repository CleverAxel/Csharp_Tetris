using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DirtyLibrary.Input
{
    public interface IKeyboardManager
    {
        public void Update();
        public bool IsPressingLeft();
        public bool IsPressingRight();
        public bool IsPressingUp();
        public bool IsPressingJump();
        public bool IsReleasingJump();
        public bool IsReleasingUp();
        public bool IsPressingDown();
    }
}