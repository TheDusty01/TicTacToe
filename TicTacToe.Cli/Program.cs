using System;
using System.Collections.Generic;
using TicTacToe.Game;

namespace TicTacToe.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Board board = new();

            Random rng = new();

            while (!board.IsGameOver())
            {
                Console.Write("Your turn (0-8): ");
                byte index = byte.Parse(Console.ReadLine());

                if (!board.MakeMove(index))
                {
                    Console.WriteLine("Move is invalid!");
                    continue;
                }

                if (board.IsGameOver())
                {

                    Console.WriteLine(board.ToString());
                    Console.WriteLine("============");
                    break;
                }

                // AI turn
                IReadOnlyList<byte> moves = board.GetPossibleMoves();

                board.MakeMove(moves[rng.Next(moves.Count)]);

                // Print
                Console.WriteLine(board.ToString());
                Console.WriteLine("============");
            }

            Console.WriteLine("Winner: " + board.Winner.ToString());
        }
    }
}
