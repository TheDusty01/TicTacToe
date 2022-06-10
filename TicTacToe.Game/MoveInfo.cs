using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe.Game
{
    public struct MoveInfo
    {
        public byte Field { get; set; }
        public Piece Piece { get; set; }
    }
}
