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
			
			foreach (ArrayList result in EnumerateSolutions()) {
				foreach (Link row in _givens) {
					result.Add(row.RowName);
				}
				
				return result;
			}
			
			return null;
		}
		
		private IEnumerable<ArrayList> EnumerateSolutions() {
			Link[] history = new Link[_nrows];
			
			int searchDepth = 0;
			
			// walk down the rows that cover the column we've selected;
			// try taking the move represented by each one, one at a time,
			// and see whether that leads to a solution.  This 
			
			ColumnHeader c;
			Link row;
			
			while (true) {
				// check whether we're at a solution
				if (_root.Right == _root) {
					ArrayList results = new ArrayList(searchDepth);
					
					// massage into a result!
					for (int i = 0; i < searchDepth; i++) {
						results.Add(history[i].RowName);
					}
					
					yield return results;
				}
				
				// if we aren't at a solution yet, find the column (of those that haven't been covered yet) 
				// that can be covered the fewest ways; since it must be covered eventually, it follows that
				// we'll get less branching by trying to deal with this one first.
				c = LeftmostSmallestColumn();
				c.Cover();
				
				row = c.Down;
			
				// here's an unintuitive bit: if the row we're looking at is a column header,
				// we must have checked all the rows already for solutions and not found any (or yielded
				// them).  So what we'll do is jump up to the previous search depth by popping the previous
				// link off of history, and if that one's done (i.e., at the header), too, we'll keep
				// ascending until we finally find one that isn't, or we'll break because we're done.
				while (row == c) {
					c.Uncover();
				
					if (searchDepth == 0) {
						// we've come back up to the top, which means we've tried every possibility
						yield break;
					}
				
					searchDepth--;
					
					row = history[searchDepth];
					c = row.Column;
					
					for (Link j = row.Left; j != row; j = j.Left) {
						j.Column.Uncover();
					}
					
					row = row.Down;
				}
			
				// now we'll take the link represented by row, and we'll make the move it indicates --
				// that is, we'll cover all the columns it has ones in, put it into our history, and continue our search.
				history[searchDepth] = row;
				
				for (Link j = row.Right; j != row; j = j.Right) {
					j.Column.Cover();
				}
				
				searchDepth++;
			}
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
			_givens.Add(_insertCursor);
		}
		
		private void StartRow(object RowName)
		{
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
		List<Link> _givens;
		
		// for the marking (i.e., insertion) phase:
		Link _insertCursor;
		object _insertRowName;
		int _insertColumn;		
	}
}

