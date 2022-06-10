using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe.Game
{
    internal struct WinIndices
    {
        public WinIndices(byte first, byte second, byte third)
        {
            First = first;
            Second = second;
            Third = third;
        }

        public byte First { get; }
        public byte Second { get; }
        public byte Third { get; }
    
    
    }
}
