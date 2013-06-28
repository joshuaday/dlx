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
			
			foreach (int i in board.SolveAll()) {
				//Console.Clear();
				Console.SetCursorPosition(0,0);
				board.WriteSolution();
				//Console.ReadKey();
			}
			
			Console.ReadKey();
			
			
			/*
			KnightsTour tour = new KnightsTour();
			tour.Solve();
			tour.WriteSolution();
			*/
			
			
			
		}
	}
}

