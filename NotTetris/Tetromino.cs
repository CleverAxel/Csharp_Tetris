using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;

namespace NotTetris {
    public enum TetrominoType {
        I = 2, O, T, S, Z, J, L
    }

    public struct TetrominoPosition {
        public short X;
        public short Y;
        public TetrominoPosition(short x = 0, short y = 0) {
            X = x;
            Y = y;
        }

        public void Set(short x = 0, short y = 0) {
            X = x;
            Y = y;
        }
    }


    public static class Tetromino {
        private static Random _rand = new Random();
        private static byte _count = 0;
        public static TetrominoType GetRandomType() {
            //if you return a specific tetromino, be sure to disable the loop preventing to have the same tetronimo back to back in the Board Script
            Array values = Enum.GetValues(typeof(TetrominoType));
            var tetronimo = (TetrominoType)values.GetValue(_rand.Next(values.Length));

            if (tetronimo == TetrominoType.I)
                _count = 0;
            else
                if (_count++ >= 7) {
                tetronimo = TetrominoType.I;
                _count = 0;
            }
            return tetronimo;
        }

        public static TetrominoPosition[][] GetOffsetsFromTypeAndRotation(TetrominoType type) {
            return type switch {
                TetrominoType.I => IOffsets,
                TetrominoType.O => OOffsets,
                TetrominoType.T => TOffsets,
                TetrominoType.S => SOffsets,
                TetrominoType.Z => ZOffsets,
                TetrominoType.J => JOffsets,
                TetrominoType.L => LOffsets,
                _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unknown tetromino type: {type}")
            };
        }
        public static readonly TetrominoPosition[][] IOffsets = [
            [ new(-1, 0), new(0, 0), new(1, 0), new(2, 0) ],
            [ new(1, -1), new(1, 0), new(1, 1), new(1, 2) ],
            [ new(-1, 1), new(0, 1), new(1, 1), new(2, 1) ],
            [ new(0, -1), new(0, 0), new(0, 1), new(0, 2) ]
        ];

        public static readonly TetrominoPosition[][] OOffsets = [
            [ new(0, 0), new(1, 0), new(0, 1), new(1, 1) ],
            [ new(0, 0), new(1, 0), new(0, 1), new(1, 1) ],
            [ new(0, 0), new(1, 0), new(0, 1), new(1, 1) ],
            [ new(0, 0), new(1, 0), new(0, 1), new(1, 1) ]
        ];

        public static readonly TetrominoPosition[][] TOffsets = [
            [ new(-1, 0), new(0, 0), new(1, 0), new(0, 1) ],
            [ new(0, -1), new(0, 0), new(1, 0), new(0, 1) ],
            [ new(-1, 0), new(0, 0), new(1, 0), new(0, -1) ],
            [ new(0, -1), new(0, 0), new(-1, 0), new(0, 1) ]
        ];

        public static readonly TetrominoPosition[][] SOffsets = [
            [ new(0, 0), new(1, 0), new(-1, 1), new(0, 1) ],
            [ new(0, -1), new(0, 0), new(1, 0), new(1, 1) ],
            [ new(0, 0), new(1, 0), new(-1, 1), new(0, 1) ],
            [ new(0, -1), new(0, 0), new(1, 0), new(1, 1) ]
        ];

        public static readonly TetrominoPosition[][] ZOffsets = [
            [ new(-1, 0), new(0, 0), new(0, 1), new(1, 1) ],
            [ new(1, -1), new(0, 0), new(1, 0), new(0, 1) ],
            [ new(-1, 0), new(0, 0), new(0, 1), new(1, 1) ],
            [ new(1, -1), new(0, 0), new(1, 0), new(0, 1) ]
        ];

        public static readonly TetrominoPosition[][] JOffsets = [
            [ new(-1, 0), new(-1, 1), new(0, 1), new(1, 1) ],
            [ new(0, -1), new(1, -1), new(0, 0), new(0, 1) ],
            [ new(-1, 0), new(0, 0), new(1, 0), new(1, 1) ],
            [ new(0, -1), new(0, 0), new(-1, 1), new(0, 1) ]
        ];

        public static readonly TetrominoPosition[][] LOffsets = [
            [ new(1, 0), new(-1, 1), new(0, 1), new(1, 1) ],
            [ new(0, -1), new(0, 0), new(0, 1), new(1, 1) ],
            [ new(-1, 0), new(0, 0), new(1, 0), new(-1, 1) ],
            [ new(-1, -1), new(0, -1), new(0, 0), new(0, 1) ]
        ];


    }
}