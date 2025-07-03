// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.Xna.Framework;
// using Microsoft.Xna.Framework.Input;

// namespace DirtyLibrary {
//     public class InputManager {

//         #region MOUSE REGION
//         private const short _clickTimeout = 1000;
//         private byte _clickCounter = 0;
//         private bool _firstClickSetByDblClick = false;
//         private MouseState _currentMouseState;
//         private MouseState _prevMouseState;
//         private int _timeOfClick = 0;
//         #endregion

//         private GameTime _gameTime;

//         public Keys Up { get; set; } = Keys.Up;
//         public Keys Down { get; set; } = Keys.Down;
//         public Keys Left { get; set; } = Keys.Left;
//         public Keys Right { get; set; } = Keys.Right;

//         public void Update(GameTime gameTime) {
//             _prevMouseState = _currentMouseState;
//             _currentMouseState = Mouse.GetState();
//             _gameTime = gameTime;

//             ManageClickTimeOut();
//         }


//         /// <summary>
//         /// If used with the method HasDoubleClicked, this method must be called first
//         /// </summary>
//         /// <returns>True if the clicked occured before the timeout</returns>
//         public bool HasClicked() {
//             if (_clickCounter == 0 && !IsClickInTimeOut() && IsClickAction()) {
//                 _timeOfClick = (int)_gameTime.TotalGameTime.TotalMilliseconds;
//                 _clickCounter++;
//                 return true;
//             }

//             return false;
//         }

//         /// <summary>
//         /// If used with the method HasClicked, HasClicked must be called first.
//         /// </summary>
//         /// <returns>True if the double-clicked occured before the timeout</returns>
//         public bool HasDoubleClicked() {
//             if (IsClickAction()) {

//                 //HasClicked not called before HasDoubleClicked
//                 if (_clickCounter == 0 && _timeOfClick == 0) {
//                     _timeOfClick = (int)_gameTime.TotalGameTime.TotalMilliseconds;
//                     _clickCounter++;
//                     _firstClickSetByDblClick = true;
//                     return false;
//                 }

//                 if ((_firstClickSetByDblClick || _clickCounter == 2) && IsClickInTimeOut()) {
//                     ResetMouseStatus();
//                     return true;
//                 }

//                 _clickCounter++;
//             }

//             return false;
//         }

//         private void ResetMouseStatus() {
//             _timeOfClick = 0;
//             _clickCounter = 0;
//             _firstClickSetByDblClick = false;
//         }

//         private int TimeElapsedBetweenClick() {
//             return (int)_gameTime.TotalGameTime.TotalMilliseconds - _timeOfClick;
//         }

//         private void ManageClickTimeOut() {
//             if (_timeOfClick != 0 && !IsClickInTimeOut()) {
//                 ResetMouseStatus();
//             }
//         }

//         private bool IsClickInTimeOut() {
//             return TimeElapsedBetweenClick() < _clickTimeout;
//         }

//         private bool IsClickAction() {
//             return _prevMouseState.LeftButton == ButtonState.Pressed && _currentMouseState.LeftButton == ButtonState.Released;
//         }
//     }
// }