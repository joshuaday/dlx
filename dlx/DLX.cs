using System;
using System.Collections;
using System.Collections.Generic;

// See arXiv:cs/0011047v1 [cs.DS]

namespace PDS
{
	public interface IDLXMove
	{
		void Mark(DLX solver);
	}
		
	public class DLX
	{
		public DLX ()
		{
			_root = new ColumnHeader(-1);
			_columns = new Dictionary<int, ColumnHeader>();
			
			_nrows = 0;
				
			_givens = new List<Link>();
		}
		
		public void AddRow(IDLXMove move)
		{
			StartRow(move);
			move.Mark(this);
		}
		
		public void AddRow(IDLXMove move, bool required)
		{
			StartRow(move);
			move.Mark(this);
			if (required) RequireRow();
		}
		
		public DLX Mark(int column)
		{
			return Mark(column, 1);
		}
		
		public DLX Mark(int column, int cost)
		{
			if (_O != null) {
				throw new System.Exception("Cannot mark new cells after starting search");
			}
			
			if (column < _insertColumn) {
				// todo: relax this constraint
				throw new System.Exception("Cells within a row must be marked in ascending order (i.e., left-to-right)");
			}
			
			if (!_columns.ContainsKey(column)) {
				// find the previous column and insert after it
				
				ColumnHeader right = _root;
				foreach (KeyValuePair<int, ColumnHeader> ch in _columns) {
					if (ch.Key > column && ch.Key < right.Name) {
						right = ch.Value;
					}
				}
				
				_columns[column] = new ColumnHeader(column);
				right.InsertAtLeft(_columns[column]);
			}
			
			ColumnHeader c = _columns[column];
			Link link = new Link();
			
			link.RowName = _insertRowName;
			
			c.InsertRightOf(link, _insertCursor); // handles the null case for _insertCursor -- no worries
			_insertCursor = link;
			
			return this;
		}

		public ArrayList Search() {
			foreach (Link row in _givens) {
				row.Column.Cover();
				for (Link j = row.Right; j != row; j = j.Right) {
					j.Column.Cover();
				}
			}
			
			
			// _O = new Link[_nrows];
			// ArrayList result = Search(0);
			
			foreach (ArrayList result in EnumerateSolutions()) {
				foreach (Link row in _givens) {
					result.Add(row.RowName);
				}
				
				return result;
			}
			
			return null;
		}
		
		private IEnumerable<ArrayList> EnumerateSolutions() {
			_O = new Link[_nrows];
			
			int searchDepth = 0;
			
			// walk down the rows that cover the column we've selected;
			// try taking the move represented by each one, one at a time,
			// and see whether that leads to a solution.
			
			ColumnHeader c;
			Link row;
			
			Search:
				if (_root.Right == _root) {
					ArrayList results = new ArrayList(searchDepth);
					
					// massage into a result!
					for (int i = 0; i < searchDepth; i++) {
						results.Add(_O[i].RowName);
					}
					
					yield return results;
				}
				
				c = LeftmostSmallestColumn();
				c.Cover();
				
				row = c.Down;
				
			NextRow:
				if (row == c) {
					c.Uncover();
					goto ReturnPoint;
				}
			
				_O[searchDepth] = row;
				
				for (Link j = row.Right; j != row; j = j.Right) {
					j.Column.Cover();
				}
				
				searchDepth++;
				goto Search;
				
			ReturnPoint:
				searchDepth--;
				if (searchDepth < 0) yield break;
				
				row = _O[searchDepth];
				c = row.Column;
				
				for (Link j = row.Left; j != row; j = j.Left) {
					j.Column.Uncover();
				}
				
				row = row.Down;
				goto NextRow;
		}
		
		private ArrayList Search(int searchDepth) {
			// this loop is "search" in Knuth's paper
			
			if (_root.Right == _root) {
				ArrayList results = new ArrayList(searchDepth);
				
				// massage into a result!
				for (int i = 0; i < searchDepth; i++) {
					results.Add(_O[i].RowName);
				}
				
				return results;
			}
			
			ColumnHeader c = LeftmostSmallestColumn();
			
			c.Cover();
			
			for (Link row = c.Down; row != c; row = row.Down) {
				_O[searchDepth] = row;
				
				for (Link j = row.Right; j != row; j = j.Right) {
					j.Column.Cover();
				}
				
				ArrayList result = Search(searchDepth + 1); // this should not actually be recursive; this actually maintains its own stack, so why bother?
				if (result != null) {
					// if we  yield  instead, we can actually enumerate all possible solutions!
					return result;
				}
				
				row = _O[searchDepth];
				c = row.Column;
				
				for (Link j = row.Left; j != row; j = j.Left) {
					j.Column.Uncover();
				}
			}
			c.Uncover();
			
			return null;
		}
		
		// Links and ColumnHeaders are used as plain data -- this is not, perhaps, ideal!
		private class Link
		{
			public Link Left, Right, Up, Down;
			public ColumnHeader Column;
			public object RowName;
			
			public Link()
			{
				Left = Right = Up = Down = this;
				Column = null;
				RowName = null;
			}
			
			public void InsertAtLeft(Link toInsert)
			{
				toInsert.Right = this;
				toInsert.Left = this.Left;
				
				toInsert.Left.Right = toInsert;
				this.Left = toInsert;
			}
		}

		private void RequireRow()
		{
			if (_insertCursor == null) {
				throw new System.Exception("You must mark at least one column before requiring a row"); // todo: relax this restriction
			}
			_givens.Add(_insertCursor);
		}
		
		private void StartRow(object RowName)
		{
			if (_O != null) {
				throw new System.Exception("Cannot insert new rows after starting search");
			}
			
			_insertCursor = null;
			_insertRowName = RowName;
			_insertColumn = 0;
			_nrows++;
		}		
				
		
		private class ColumnHeader : Link
		{
			public int Name;
			public int Size;

			public ColumnHeader(int columnName) {
				// in the longer term, the "name" of the column will be an arbitrary object that serves as a convenient annotation
				Name = columnName;
				Size = 0;
				Column = this;
			}
			
			public void InsertRightOf(Link toInsert, Link left)
			{
				toInsert.Column = this;
				
				// put it at the bottom of this column
				toInsert.Down = this;
				toInsert.Up = this.Up;
				
				toInsert.Up.Down = toInsert;
				this.Up = toInsert;
				
				// and put it to the right of the link just left of it
				if (left != null) {
					toInsert.Left = left;
					toInsert.Right = left.Right;
					
					left.Right = toInsert;
					toInsert.Right.Left = toInsert;
				}
				
				Size++;
			}
			
			public void Cover()
			{
				Right.Left = Left;
				Left.Right = Right;

				for (Link i = this.Down; i != this; i = i.Down) {
					for (Link j = i.Right; j != i; j = j.Right) {
						j.Column.Size--;
						
						// notice that j is not modified here -- only its neighbors are! -- no need for a temp j.Down
						j.Down.Up = j.Up;
						j.Up.Down = j.Down;
					}
				}
			}
			
			public void Uncover()
			{
				/*
				 * This exactly undoes what Cover does; the search process makes sure that the order
				 * is the same.
				 * (My apologies for using i and j here -- I wanted to keep Knuth's usage for clarity.)
				 */
				
				for (Link i = this.Up; i != this; i = i.Up) {
					for (Link j = i.Left; j != i; j = j.Left) {
						j.Column.Size++;
						
						j.Down.Up = j;
						j.Up.Down = j;
					}
				}
				
				
				Right.Left = this;
				Left.Right = this;
			}
		}
		
		private ColumnHeader LeftmostSmallestColumn()
		{
			ColumnHeader best_column = null;
			int best_size = int.MaxValue;
			
			for (ColumnHeader i = _root.Right as ColumnHeader; i != _root; i = i.Right as ColumnHeader) {
				if (i.Size < best_size) {
					best_column = i;
					best_size = i.Size;
				}
			}
			
			return best_column;
		}
				
		int _nrows;
		Dictionary<int, ColumnHeader> _columns;
		ColumnHeader _root;
		
		// for the searching phase:
		Link[] _O;
		List<Link> _givens;
		
		// for the marking (i.e., insertion) phase:
		Link _insertCursor;
		object _insertRowName;
		int _insertColumn;		
	}
}

