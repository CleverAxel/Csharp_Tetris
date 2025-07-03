using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotTetris {
    public class GameManager {
        private Board _board;
        private Player _player;
        public GameManager(Board board, Player player) {
            _board = board;
            _player = player;
        }

        public void Update() {
            _board.Update();
        }

        public void Draw() {
            _board.Draw();
        }

        public void LoadContent() {
            _board.LoadContent();
        }
    }
}