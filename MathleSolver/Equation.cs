using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathleSolver
{
	public readonly struct Equation
	{

		public const byte PlusSign = 10;
		public const byte MinusSign = 11;

		readonly byte[] elements;

		public Equation( byte[] elements )
		{
			if (elements.Length != Solver.GuessableCellsCount)
			{
				throw new ArgumentException( $"{nameof( elements )} should have a length" +
					$" of at least {nameof( Solver.GuessableCellsCount )}" );
			}
			this.elements = elements;
		}

		public Equation( string str )
		{
			if (str.Length < Solver.GuessableCellsCount)
			{
				throw new ArgumentException( $"{nameof( str )} should have a length" +
					$" of {nameof( Solver.GuessableCellsCount )}" );
			}
			elements = new byte[Solver.GuessableCellsCount];
			int index = 0;
			for (int i = 0; i < str.Length && index < Solver.GuessableCellsCount; i++)
			{
				if (str[i] == '+' || str[i] == '-' || (str[i] >= '0' && str[i] <= '9'))
				{
					if (index >= elements.Length)
					{
						throw new ArgumentException( $"{nameof( str )} was too long" );
					}
					if (str[i] == '+')
					{
						elements[index] = PlusSign;
					}
					else if (str[i] == '-')
					{
						elements[index] = MinusSign;
					}
					else if (str[i] >= '0' && str[i] <= '9')
					{
						elements[index] = (byte)(str[i] - '0');
					}
					index++;
				}
			}
			if (index < elements.Length)
			{
				throw new ArgumentException( $"{nameof( str )} should have" +
					$" {nameof( Solver.GuessableCellsCount )} valid chars" );
			}
		}

		public byte this[int index]
		{
			get => elements[index];
		}

		public override string ToString()
		{
			char[] strChars = new char[11];
			for (int i = 0; i < 5; i++)
			{
				strChars[i] = ElementToChar( elements[i] );
			}
			strChars[5] = ' ';
			strChars[6] = '=';
			strChars[7] = ' ';
			for (int i = 8; i < 11; i++)
			{
				strChars[i] = ElementToChar( elements[i - 3] );
			}
			return new string( strChars );

			char ElementToChar( byte element )
			{
				if (element == PlusSign)
				{
					return '+';
				}
				if (element == MinusSign)
				{
					return '-';
				}
				return (char)(element + '0');
			}
		}

		public static bool operator ==( Equation a, Equation b )
		{
			if (a.elements.Length != b.elements.Length)
			{
				return false;
			}
			for (int i = 0; i < a.elements.Length; i++)
			{
				if (a.elements[i] != b.elements[i])
				{
					return false;
				}
			}
			return true;
		}

		public static bool operator !=( Equation a, Equation b )
		{
			return !(a == b);
		}
	}
}
