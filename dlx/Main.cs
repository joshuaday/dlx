using System;
using PDS;

namespace sudokusolver
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			string filename = args.Length < 1 ? "samurai1.txt" : args[0];
			
			SamuraiSudokuBoard board = new SamuraiSudokuBoard(filename);
			
			board.Solve();
			
			Console.Clear();
			board.WriteSolution();
			
			/*
			KnightsTour tour = new KnightsTour();
			tour.Solve();
			tour.WriteSolution();
			*/
			
			Console.ReadKey();
			
			
		}
	}
}

