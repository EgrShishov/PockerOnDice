using System.Xml.Linq;
using System;

namespace SignalRGame.Domain.Models
{
	public class Combination : IComparable
	{
		public int CombinationLevel { get; set; }
		public int Score { get; set; }

		public Combination()
		{
			CombinationLevel = 0;
			Score = 0;
		}

		public Combination(int combo_level, int score = 0)
		{
			CombinationLevel = combo_level;
			Score = score;
		}
		public void Reset()
		{
			CombinationLevel = 0;
			Score = 0;
		}

		public override bool Equals(object? obj)
		{
			return obj is Combination combination&&
				   CombinationLevel==combination.CombinationLevel&&
				   Score==combination.Score;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(CombinationLevel, Score);
		}

		public static bool operator >(Combination left, Combination right)
		{
			if (left.CombinationLevel > right.CombinationLevel)
			{
				return true;
			}
			if (left.CombinationLevel < right.CombinationLevel)
			{
				return false;
			}
			return left.Score > right.Score;
		}

		public static bool operator >=(Combination left, Combination right)
		{
			if (left.CombinationLevel > right.CombinationLevel)
			{
				return true;
			}
			if (left.CombinationLevel < right.CombinationLevel)
			{
				return false;
			}
			return left.Score >= right.Score;
		}

		public static bool operator <(Combination left, Combination right)
		{
			if (left.CombinationLevel < right.CombinationLevel)
			{
				return true;
			}
			if (left.CombinationLevel > right.CombinationLevel)
			{
				return false;
			}
			return left.Score < right.Score;
		}

		public static bool operator <=(Combination left, Combination right)
		{
			if (left.CombinationLevel < right.CombinationLevel)
			{
				return true;
			}
			if (left.CombinationLevel > right.CombinationLevel)
			{
				return false;
			}
			return left.Score <= right.Score;
		}

		public static bool operator ==(Combination left, Combination right)
		{
			return left.CombinationLevel == right.CombinationLevel && left.Score == right.Score;
		}

		public static bool operator !=(Combination left, Combination right)
		{
			return left.CombinationLevel != right.CombinationLevel || left.Score != right.Score;
		}
		public int CompareTo(object? o)
		{
			if (o is Combination combo)
			{
				if (this < combo)
					return -1;
				if (this > combo)
					return 1;
				return 0;
			}
			else
			{
				throw new ArgumentException("Некорректное значение параметра");
			}
		}
	}
}
