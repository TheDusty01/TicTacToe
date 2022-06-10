using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicTacToe.Game;

namespace TicTacToe.Discord
{
    public class DiscordBoard : BaseBoard
    {
        public const string CrossEmoji = "❎";
        public const string CircleEmoji = "⏺️";
        public const string EmptyEmoji = "⬜";

        public override string ToString()
        {
            StringBuilder sb = new();

            for (byte i = 0; i < grid.Length; i++)
            {
#pragma warning disable CA1834 // Consider using 'StringBuilder.Append(char)' when applicable
                if (grid[i] == Piece.Circle)
                    sb.Append(CircleEmoji);
                else if (grid[i] == Piece.Cross)
                    sb.Append(CrossEmoji);
                else
                    sb.Append(EmptyEmoji);
#pragma warning restore CA1834 // Consider using 'StringBuilder.Append(char)' when applicable

                if ((i + 1) % 3 == 0 && i != grid.Length)
                    sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
