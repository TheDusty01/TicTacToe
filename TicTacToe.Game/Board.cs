using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe.Game
{
    public class Board : BaseBoard
    {

        public override string ToString()
        {
            StringBuilder sb = new();

            for (byte i = 0; i < grid.Length; i++)
            {
                if (grid[i] == Piece.Circle)
                    sb.Append('o');
                else if (grid[i] == Piece.Cross)
                    sb.Append('x');
                else
                    sb.Append(' ');

                if ((i + 1) % 3 == 0 && i != grid.Length)
                    sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
