using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using PDS;

namespace sudokusolver
{
	public class SamuraiSudokuBoard
	{
		public SamuraiSudokuBoard(string FileName)
		{
			_givens = new int[21, 21];
			_solution = new int[21, 21];
			
			ReadFile(FileName);
		}
		
		private void ReadFile(string FileName)
		{	
			using (StreamReader reader = new StreamReader(FileName)) {
				int x = 0, y = 0;
				
				while (y < 21) {
					string line = reader.ReadLine();
					
					for (int i = 0; i < line.Length; i++) {
						if (line[i] >= '1' && line[i] <= '9') {
							_givens[x, y] = line[i] - '0';
							x++;
						}
						
						if (line[i] == 'X' || line[i] == '.') {
							x++;
						}
					}
					
					if (x > 0) {
						x = 0;
						y++;
					}
				}
			}
		}

		public void Solve() {
			DLX solver = new DLX();

			foreach (SudokuMove move in SudokuMove.AllMoves()) {
				bool required = _givens[move.X, move.Y] == move.N + 1;
				solver.AddRow(move, required);
			}
			
			ArrayList answer = solver.Search();
			
			if (answer != null) {
				foreach (object r in answer) {
					SudokuMove move = r as SudokuMove;
					_solution[move.X, move.Y] = move.N + 1;
				}
			}
		}
		
		public IEnumerable<int> SolveAll() {
			DLX solver = new DLX();

			foreach (SudokuMove move in SudokuMove.AllMoves()) {
				bool required = _givens[move.X, move.Y] == move.N + 1;
				solver.AddRow(move, required);
			}
			
			foreach (ArrayList answer in solver.EnumerateSolutions()) {
				foreach (object r in answer) {
					SudokuMove move = r as SudokuMove;
					_solution[move.X, move.Y] = move.N + 1;
				}
				
				yield return 1; // hack
			}
			
			yield break;
		}
		
		public void WriteSolution()
		{
			Console.WriteLine("Solution:");
			
			for (int y = 0; y < 21; y++) {
				for (int x = 0; x < 21; x++) {
					if (_solution[x, y] > 0) {
						Console.Write(_solution[x, y].ToString());
					} else {
						Console.Write(" ");
					}
				}
				Console.WriteLine();
			}
		}
				          

		private class SudokuMove : IDLXMove {
			public int N, R, C, Block, Board;
			public SudokuMove(int n, int r, int c, int block, int board) {
				N = n;
				R = r;
				C = c;
				Block = block;
				Board = board;
			}
			
			public int X {
				get {
					int x = C;
					if (Board == 1 || Board == 4) x += 12;
					if (Board == 2) x += 6;
					
					x += 3 * (Block % 3);
					
					return x;	
				}
			}
			public int Y {
				get {
					int y = R;
					if (Board == 3 || Board == 4) y += 12;
					if (Board == 2) y += 6;
					
					y += 3 * (Block / 3);
					
					return y;
				}
			}
				
			
			public override string ToString() {
				return string.Concat(N + 1, " at (", R + 1, ", ", C + 1, ")");
			}
			
			public void Plot() {
				Console.SetCursorPosition(1 + X, 1 + Y);
				Console.Write((1 + N).ToString());
			}
			
			public void Mark(DLX solver) {
				// mark the cell as being used
				solver.Mark((0 * 5 * 81) + (Board * 81) + (Block * 9) + (3 * R) + C);
				
				// mark the column as containing n
				solver.Mark((1 * 5 * 81) + Board * 81 + 27 * (Block % 3) + 9 * C + N);
				
				// mark the row as containing n
				solver.Mark((2 * 5 * 81) + Board * 81 + 27 * (Block / 3) + 9 * R + N);
				
				// mark the block as containing n
				solver.Mark((3 * 5 * 81) + 9 * (Board * 9 + Block) + N);

				if ((Board == 0 && Block == 8) || (Board == 1 && Block == 6) || (Board == 3 && Block == 2) || (Board == 4 && Block == 0)) {
					// also mark the middle board
					
					int MiddleBlock = 8 - Block;
					int MiddleBoard = 2;
					
					// mark the column as containing n
					solver.Mark((1 * 5 * 81) + MiddleBoard * 81 + 27 * (MiddleBlock % 3) + 9 * C + N);
					
					// mark the row as containing n
					solver.Mark((2 * 5 * 81) + MiddleBoard * 81 + 27 * (MiddleBlock / 3) + 9 * R + N);
				}
			}
			
			static public IEnumerable<SudokuMove> AllMoves() {
				for (int n = 0; n < 9; n++) {
					for (int board = 0; board < 5; board++) {
						for (int block = 0; block < 9; block++) {
							if (board == 2 && (block == 0 || block == 2 || block == 6 || block == 8)) {
								continue;
							}
							
							for (int r = 0; r < 3; r++) {
								for (int c = 0; c < 3; c++) {
									yield return new SudokuMove(n, r, c, block, board);
								}
							}
						}
					}
				}
				
				yield break;
			}
		}				          
				          
		private int[,] _givens;
		private int[,] _solution;
	}
}

