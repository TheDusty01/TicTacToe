using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace TicTacToe.Game
{
    public class BaseBoard
    {
        private static readonly WinIndices[] winIndices = new WinIndices[8]
        {
            new(0, 1, 2), new(3, 4, 5), new(6, 7, 8),
            new(0, 3, 6), new(1, 4, 7), new(2, 5, 8),
            new(0, 4, 8), new(2, 4, 6)
        };

        private readonly Random rng = new();
        protected Piece[] grid;
        private List<MoveInfo> moves;

        public Piece CurrentTurn { get; set; }
        public Piece Winner { get; private set; }

        public BaseBoard()
        {
            Reset();
        }

        public void Reset()
        {
            grid = new Piece[9];
            moves = new List<MoveInfo>();
            Winner = Piece.Empty;

            if (rng.Next(2) == 1)
            {
                CurrentTurn = Piece.Circle;
            }
            else
            {
                CurrentTurn = Piece.Cross;
            }
        }

        public Piece GetLastTurn()
        {
            if (CurrentTurn == Piece.Cross)
            {
                return Piece.Circle;
            }
            else
            {
                return Piece.Cross;
            }
        }

        private void SwitchCurrentTurn()
        {
            if (CurrentTurn == Piece.Cross)
            {
                CurrentTurn = Piece.Circle;
            }
            else
            {
                CurrentTurn = Piece.Cross;
            }
        }

        public bool IsEmpty(byte index)
        {
            return grid[index] == Piece.Empty;
        }

        public bool IsGameOver()
        {
            return moves.Count == 9 || Winner != Piece.Empty;
        }

        public bool MakeMove(byte index)
        {
            // Is game over
            if (IsGameOver())
                return false;

            // Invalid turn
            if (IsEmpty(index))
            {
                grid[index] = CurrentTurn;
                moves.Add(new MoveInfo { Field = index, Piece = CurrentTurn });
                CalcWinner();
                SwitchCurrentTurn();

                return true;
            }

            return false;
        }

        public bool RevokeLastMove()
        {
            if (moves.Count < 1)
                return false;

            if (Winner != Piece.Empty)
                Winner = Piece.Empty;

            int lastIndex = moves.Count - 1;
            grid[moves[lastIndex].Field] = Piece.Empty;
            moves.RemoveAt(lastIndex);
            SwitchCurrentTurn();

            return true;            
        }

        public Piece GetPiece(byte index)
        {
            return grid[index];
        }

        public IReadOnlyList<byte> GetPossibleMoves()
        {
            List<byte> possibleMoves = new();
            for (byte i = 0; i < grid.Length; i++)
            {
                if (grid[i] == Piece.Empty)
                    possibleMoves.Add(i);
            }

            return possibleMoves;
        }

        private void CalcWinner()
        {
            foreach (WinIndices probableWinIndices in winIndices)
            {
                if (grid[probableWinIndices.First] == CurrentTurn &&
                    grid[probableWinIndices.Second] == CurrentTurn &&
                    grid[probableWinIndices.Third] == CurrentTurn)
                {
                    Winner = CurrentTurn;
                    return;
                }
            }
        }

    }
}
