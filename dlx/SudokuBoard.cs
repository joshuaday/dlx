using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using PDS;

namespace sudokusolver
{
	public class SudokuBoard
	{
		public SudokuBoard(string FileName)
		{
			_givens = new int[9, 9];
			_solution = new int[9, 9];
			
			ReadFile(FileName);
		}
		
		private void ReadFile(string FileName)
		{
			
			using (StreamReader reader = new StreamReader(FileName)) {
				int x = 0, y = 0;
				
				while (y < 9) {
					string line = reader.ReadLine();
					
					for (int i = 0; i < line.Length; i++) {
						if (line[i] >= '1' && line[i] <= '9') {
							_givens[x, y] = line[i] - '0';
							x++;
						}
						
						if (line[i] == 'X') {
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
				bool given = (_givens[move.C, move.R] == move.N + 1);
				
				solver.AddRow(move, given);
			}
			
			ArrayList answer = solver.Search();
			
			foreach (object r in answer) {
				SudokuMove move = r as SudokuMove;
				_solution[move.C, move.R] = move.N + 1;
			}
		}
		
		public void WriteSolution()
		{
			Console.WriteLine("Solution:");
			for (int y = 0; y < 9; y++) {
				for (int x = 0; x < 9; x++) {
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
			public int N, R, C;
			public SudokuMove(int n, int r, int c) {
				N = n;
				R = r;
				C = c;
			}
			
			public override string ToString() {
				return string.Concat(N + 1, " at (", R + 1, ", ", C + 1, ")");
			}
			
			public void Plot() {
				Console.SetCursorPosition(1 + C, 1 + R);
				Console.Write((1 + N).ToString());
			}
			
			public void Mark(DLX solver) {
				// add a row for this number in this row and column
				int block = (R / 3) + 3 * (C / 3); // relying on truncation, of course
				
				// mark the cell as being used
				solver.Mark(0 * 81 + 9 * R + C);
				
				// mark the column as containing n
				solver.Mark(1 * 81 + 9 * C + N);
				
				// mark the row as containing n
				solver.Mark(2 * 81 + 9 * R + N);
				
				// mark the block as containing n
				solver.Mark(3 * 81 + 9 * block + N);
			}
			
			static public IEnumerable<SudokuMove> AllMoves() {
				for (int n = 0; n < 9; n++) {
					for (int r = 0; r < 9; r++) {
						for (int c = 0; c < 9; c++) {
							yield return new SudokuMove(n, r, c);
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

