using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using PDS;

namespace sudokusolver
{
	public class KnightsTour
	{
		public KnightsTour ()
		{
			// nothing to initialize?
		}
		
		public void Solve() {
			DLX solver = new DLX(); // one column per cell for leaving, one for arriving; and one special cell for start; one special cell for end
			
			foreach (ChessMove move in ChessMove.AllKnightsMoves()) {
				move.Mark(solver);
			}
			
			ArrayList answer = solver.Search();
			
			if (answer != null) {
				Dictionary<int, ChessMove> bysource = new Dictionary<int, ChessMove>();
				Dictionary<int, ChessMove> bydest = new Dictionary<int, ChessMove>();
				
				Console.Clear();
				foreach (object r in answer) {
					ChessMove move = r as ChessMove;
					
					bysource[move.C1] = move;
					bydest[move.C2] = move;
				}
				
				int start = ChessMove.OffTheBoard;
				int cell = start;
				
				List<ChessMove> moves = new List<ChessMove>();
				
				while (true) {
					ChessMove move = bysource[cell];
					moves.Add(move);
					cell = move.C2;
					
					if (cell == start) {
						break;
					}
				}
				
				Console.WriteLine("Moves in cycle:");
				Console.WriteLine(moves.Count);
				Console.ReadKey();
				
				_solution = moves;
			}
		}
		
		public void WriteSolution()
		{
			Console.Clear();
			
			foreach (ChessMove m in _solution) {
				Console.SetCursorPosition(m.X, m.Y);
				Console.Write("K");
				
				Console.SetCursorPosition(1, 20);
				Console.ReadKey();
				
				Console.SetCursorPosition(m.X, m.Y);
				Console.Write("*");
			}
		}
				          
		
		private class ChessMove : IDLXMove  {
			public int C1, C2;
			
			public static int OffTheBoard {
				get {return 64; }
			}
			
			public ChessMove(int x1, int y1, int x2, int y2) {
				C1 = x1 + (8 * y1);
				C2 = x2 + (8 * y2);
			}
			
			public int X {
				get { return C1 % 8; }
			}
			
			public int Y {
				get { return C1 / 8; }
			}
			
			public override string ToString() {
				return string.Concat(C1, " to ", C2);
			}
			
			static public ChessMove KnightMove(int x1, int y1, int n) {
				int x2, y2;
				
				switch (n) {
				case 0: x2 = x1 - 2; y2 = y1 - 1; break;
				case 1: x2 = x1 - 1; y2 = y1 - 2; break;
				case 2: x2 = x1 + 1; y2 = y1 - 2; break;
				case 3: x2 = x1 + 2; y2 = y1 - 1; break;
				case 4: x2 = x1 + 2; y2 = y1 + 1; break;
				case 5: x2 = x1 + 1; y2 = y1 + 2; break;
				case 6: x2 = x1 - 1; y2 = y1 + 2; break;
				case 7: x2 = x1 - 2; y2 = y1 + 1; break;
				default: return null;
				}
				
				if (x1 < 0 || x1 >= 8 || y1 < 0 || y1 >= 8 || x2 < 0 || x2 >= 8 || y2 < 0 || y2 >= 8) {
					return null;
				} else {
					return new ChessMove(x1, y1, x2, y2);
				}	
			}
			
			static public ChessMove StartAt(int x, int y) {
				ChessMove m = new ChessMove(x, y, x, y);
				m.C1 = OffTheBoard;
				return m;
			}
			
			static public ChessMove EndAt(int x, int y) {
				ChessMove m = new ChessMove(x, y, x, y);
				m.C2 = OffTheBoard;
				return m;
			}			
			
			public void Mark(DLX solver) {
				solver.Mark(C1).Mark(C2 + 65);
			}
			
			static public IEnumerable<ChessMove> AllKnightsMoves() { 
				for (int x1 = 0; x1 < 8; x1++) {
					for (int y1 = 0; y1 < 8; y1++) {
						
						yield return ChessMove.StartAt(x1, y1);
						yield return ChessMove.EndAt(x1, y1);
						
						for (int n = 0; n < 8; n++) {
							ChessMove move = ChessMove.KnightMove(x1, y1, n);
							
							if (move != null) {
								// add a row for this number in this row and column
								
								yield return move;
							}
						}
					}
				}
				
				yield break;
			}
		}
		
		private List<ChessMove> _solution;
	}
}

