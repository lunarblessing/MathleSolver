using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathleSolver
{
	public class GuessResult
	{
		GuessColor[] Colors { get; set; }


		public GuessResult()
		{
			Colors = new GuessColor[Solver.GuessableCellsCount];
		}


		/// <summary>
		/// Creates as instance from specified colors if cells. Array length should be 
		/// <see cref="Solver.GuessableCellsCount"/>.
		/// </summary>
		/// <param name="colors"> Array of cell colors </param>
		/// <exception cref="ArgumentException"> If array length not equal to 
		/// <see cref="Solver.GuessableCellsCount"/> </exception>
		public GuessResult( GuessColor[] colors )
		{
			if (colors.Length != Solver.GuessableCellsCount)
			{
				Colors = new GuessColor[Solver.GuessableCellsCount];
				throw new ArgumentException( $"{nameof( colors )} should have a length of" +
					$" {nameof( Solver.GuessableCellsCount )}" );
			}
			Colors = colors;
		}


		/// <summary>
		/// Creates an instance from string representation of cell colors.
		/// </summary>
		/// <param name="stringRepresentation"> string containing abbreviations of cell colors:<br></br>
		/// B, b, Y, y, G, g.<br></br>
		/// Should have a length of <see cref="Solver.GuessableCellsCount"/> and have only those letters </param>
		/// <exception cref="ArgumentException"> If <paramref name="stringRepresentation"/> length is not
		/// <see cref="Solver.GuessableCellsCount"/>, or<br></br>
		/// any char doesn't represent a color. </exception>
		public GuessResult( string stringRepresentation )
		{
			if (stringRepresentation.Length != Solver.GuessableCellsCount)
			{
				Colors = new GuessColor[Solver.GuessableCellsCount];
				throw new ArgumentException( $"{nameof( stringRepresentation )} should have a length of" +
					$" {nameof( Solver.GuessableCellsCount )}" );
			}
			Colors = new GuessColor[stringRepresentation.Length];
			for (int i = 0; i < stringRepresentation.Length; i++)
			{
				if (!GetColorFromChar( stringRepresentation[i], out Colors[i] ))
				{
					throw new ArgumentException( $"{nameof( stringRepresentation )} should contain only chars" +
						$" representing a guess color" );
				}
			}
		}

		public GuessColor this[int index]
		{
			get => Colors[index];
			set => Colors[index] = value;
		}

		public bool IsFullyCorrect()
		{
			for (int i = 0; i < Colors.Length; i++)
			{
				if (Colors[i] != GuessColor.Green)
				{
					return false;
				}
			}
			return true;
		}

		bool GetColorFromChar( char ch, out GuessColor color )
		{
			switch (ch)
			{
				case 'B':
				case 'b':
					color = GuessColor.Black;
					return true;
				case 'Y':
				case 'y':
					color = GuessColor.Yellow;
					return true;
				case 'G':
				case 'g':
					color = GuessColor.Green;
					return true;
				default:
					color = GuessColor.Black;
					return false;
			}
		}
	}
}
