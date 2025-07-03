using System;
using System.Collections.Generic;
using System.Linq;
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


    public class Tetromino {
        private static Random _rand = new Random();

        public static TetrominoType GetRandomType() {
            Array values = Enum.GetValues(typeof(TetrominoType));
            // return TetrominoType.I;
            return (TetrominoType)values.GetValue(_rand.Next(values.Length));
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
            [ new(-1, 0), new(0, 0), new(1, 0), new(1, -1) ],
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