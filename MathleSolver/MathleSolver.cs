using System;
using System.Diagnostics;

namespace MathleSolver
{
	public class Solver
	{
		public const int GuessableCellsCount = 8;

		public IEnumerable<Equation> AllPossibleEquations => Equations;
		public IEnumerable<Equation> RemainingSolutions => PossibleSolutions;
		public int EquationsCount { get => Equations.Count; }
		public int SolutionsCount { get => PossibleSolutions.Count; }
		public double RemainingInformation { get => Math.Log2( PossibleSolutions.Count ); }

		List<Equation> Equations { get; set; }
		LinkedList<Equation> PossibleSolutions { get; set; }
		List<byte> ImpossibleToGuess { get; set; }

		/// <summary>
		/// Initializes an instance and prepares its inner state for calculation
		/// </summary>
		public Solver()
		{
			ImpossibleToGuess = new List<byte>( 12 );
			Equations = new( 100000 );
			MakeEquationsWithNDigits( Equations, 100, 100, 2, 2 );
			MakeEquationsWithNDigits( Equations, 1000, 10, 3, 1 );
			MakeEquationsWithNDigits( Equations, 10, 1000, 1, 3 );
			PossibleSolutions = new LinkedList<Equation>( Equations );

			void MakeEquationsWithNDigits( List<Equation> list, int leftMax, int rightMax, int leftDigits, int rightDigits )
			{
				string leftFormat = "D" + leftDigits;
				string rightFormat = "D" + rightDigits;
				for (int left = 0; left < leftMax; left++)
				{
					for (int right = 0; right < rightMax; right++)
					{
						var leftString = left.ToString( leftFormat );
						var rightString = right.ToString( rightFormat );
						if (left + right < 1000)
						{
							byte[] additionBytes = new byte[GuessableCellsCount];
							var answerString = (left + right).ToString( "D3" );
							FillDigits( additionBytes, leftString, rightString );
							additionBytes[leftDigits] = Equation.PlusSign;
							FillAnswerBytes( additionBytes, answerString );
							list.Add( new Equation( additionBytes ) );
						}
						if (left - right < 0)
						{
							continue;
						}
						byte[] subtractionBytes = new byte[GuessableCellsCount];
						FillDigits( subtractionBytes, leftString, rightString );
						subtractionBytes[leftDigits] = Equation.MinusSign;
						var subAnswerString = (left - right).ToString( "D3" );
						FillAnswerBytes( subtractionBytes, subAnswerString );
						list.Add( new Equation( subtractionBytes ) );
					}
				}
			}

			void FillDigits( byte[] array, string leftString, string rightString )
			{
				int index = 0;
				for (; index < leftString.Length; index++)
				{
					array[index] = (byte)(leftString[index] - '0');
				}
				index++; //skip sign char
				for (int i = 0; i < rightString.Length; i++)
				{
					array[index + i] = (byte)(rightString[i] - '0');
				}
			}

			void FillAnswerBytes( byte[] array, string answerString )
			{
				array[5] = (byte)(answerString[0] - '0');
				array[6] = (byte)(answerString[1] - '0');
				array[7] = (byte)(answerString[2] - '0');
			}
		}


		/// <summary>
		/// Changes inner state of solver according to user guess result, updates a list of possible solitions.
		/// </summary>
		/// <param name="guess"> User guess </param>
		/// <param name="result"> Results of guess </param>
		public void ProcessInput( Equation guess, GuessResult result )
		{
			var newPossibleGuesses = new LinkedList<Equation>();
			var solutionNode = PossibleSolutions.First;
			while (solutionNode != null)
			{
				var next = solutionNode.Next;
				if (IsSolutionSuitableForGuess( solutionNode.Value, guess, result ))
				{
					newPossibleGuesses.AddLast( new LinkedListNode<Equation>( solutionNode.Value ) );
				}
				solutionNode = next;
			}
			PossibleSolutions = newPossibleGuesses;
			for (int i = 0; i < GuessableCellsCount; i++)
			{
				if (result[i] == GuessColor.Black)
				{
					byte wrongSymbol = guess[i];
					bool addToImpossibleSymbols = true;
					for (int j = 0; j < GuessableCellsCount; j++)
					{
						if (i == j)
						{
							continue;
						}
						if (guess[j] == wrongSymbol && result[j] != GuessColor.Black)
						{
							addToImpossibleSymbols = false;
							break;
						}
					}
					if (addToImpossibleSymbols)
					{
						ImpossibleToGuess.Add( wrongSymbol );
					}
				}
			}
		}


		/// <summary>
		/// Returns array of possible guesses sorted by expected gained information
		/// on the current state of solving process.
		/// </summary>
		/// <returns> Array sorted by descending expected gained information. </returns>
		public EquationWithEntropy[] GetBestGuesses()
		{
			var equationsGainedInfo = BestGuessesMainWork( true, out double _ );
			if (PossibleSolutions.Count < 25)
			{
				double minEntropyToConsider = equationsGainedInfo[0].entropy * 0.99;
				int index = 0;
				int substitutionsCount = 0;
				while (index < equationsGainedInfo.Length && equationsGainedInfo[index].entropy >= minEntropyToConsider)
				{
					if (PossibleSolutions.Contains( equationsGainedInfo[index].equation ))
					{
						(equationsGainedInfo[substitutionsCount], equationsGainedInfo[index]) =
							(equationsGainedInfo[index], equationsGainedInfo[substitutionsCount]);
						substitutionsCount++;
					}
					index++;
				}
			}
			return equationsGainedInfo;
		}


		/// <summary>
		/// Returns an expected entropy when <paramref name="equation"/> is guessed
		/// on current list of possible solutions.
		/// </summary>
		/// <param name="equation"> Equation we check expected information for. </param>
		/// <returns> Expected remaining entropy after specified guess. </returns>
		public double CalculateExpectedEntropy( Equation equation )
		{
			for (int i = 0; i < GuessableCellsCount; i++)
			{
				if (ImpossibleToGuess.Contains( equation[i] ))
				{
					return 0;
				}
			}
			int[] frequencies = new int[81 * 81];
			double probabilityPerSolution = 1.0 / PossibleSolutions.Count;
			foreach (var solution in PossibleSolutions)
			{
				GuessResult guessResult = GetGuessResult( equation, solution );
				int arrayIndex = (int)(guessResult[0]) * (81 * 27) +
								 (int)(guessResult[1]) * (27 * 27) +
								 (int)(guessResult[2]) * (27 * 9) +
								 (int)(guessResult[3]) * (9 * 9) +
								 (int)(guessResult[4]) * (9 * 3) +
								 (int)(guessResult[5]) * (3 * 3) +
								 (int)(guessResult[6]) * 3 +
								 (int)(guessResult[7]);
				frequencies[arrayIndex]++;
			}
			double infoGained = 0;
			for (int i = 0; i < frequencies.Length; i++)
			{
				if (frequencies[i] == 0)
				{
					continue;
				}
				double currentProb = frequencies[i] * probabilityPerSolution;
				infoGained -= currentProb * Math.Log2( currentProb );
			}
			return infoGained;

			int ColorToInt( GuessColor color )
			{
				if (color == GuessColor.Green)
				{
					return 0;
				}
				if (color == GuessColor.Yellow)
				{
					return 1;
				}
				return 2;
			}
		}


		/// <summary>
		/// Resets state to default.
		/// </summary>
		public void Reset()
		{
			ImpossibleToGuess.Clear();
			PossibleSolutions = new LinkedList<Equation>( Equations );
		}


		/// <summary>
		/// Returns results of guess against specified correct solution.
		/// </summary>
		/// <param name="guess"> Guess. </param>
		/// <param name="solution"> Correct solution. </param>
		/// <returns> Results of guess containing information about each guessed cell. </returns>
		public GuessResult GetGuessResult( Equation guess, Equation solution )
		{
			GuessResult result = new GuessResult();
			for (int i = 0; i < GuessableCellsCount; i++)
			{
				var currentCell = guess[i];
				var solutionCell = solution[i];
				if (currentCell == solutionCell)
				{
					result[i] = GuessColor.Green;
					continue;
				}
				int misplacedTotal = 0;
				for (int j = 0; j < GuessableCellsCount; j++)
				{
					if (solution[j] == currentCell && solution[j] != guess[j])
					{
						misplacedTotal++;
					}
				}
				if (misplacedTotal == 0)
				{
					result[i] = GuessColor.Black;
					continue;
				}
				int misplacedBeforeCurrent = 0;
				for (int j = 0; j < i; j++)
				{
					if (guess[j] == currentCell && guess[j] != solution[j])
					{
						misplacedBeforeCurrent++;
					}
				}
				if (misplacedBeforeCurrent < misplacedTotal)
				{
					result[i] = GuessColor.Yellow;
				}
				else
				{
					result[i] = GuessColor.Black;
				}
			}
			return result;
		}


		/// <summary>
		/// Returns entropy of the best guess.
		/// </summary>
		/// <returns></returns>
		public double GetBestGuessEntropy()
		{
			BestGuessesMainWork( false, out double entropy );
			return entropy;
		}


		EquationWithEntropy[]? BestGuessesMainWork( bool returnArray, out double bestEntropy )
		{
			if (PossibleSolutions.Count < 3)
			{
				bestEntropy = 1;
				return PossibleSolutions.Select( eq => new EquationWithEntropy( eq, 1 ) ).ToArray();
			}
			int iterations = Equations.Count;
			EquationWithEntropy[] equationsGainedInfo = new EquationWithEntropy[iterations];
			Parallel.For( 0, iterations, ( i ) =>
			{
				Equation eq = Equations[i];
				double gainedInfo = CalculateExpectedEntropy( eq );
				equationsGainedInfo[i] = new EquationWithEntropy( eq, gainedInfo );
			} );
			if (returnArray)
			{
				Array.Sort( equationsGainedInfo, ( eq1, eq2 ) => -eq1.entropy.CompareTo( eq2.entropy ) );
				bestEntropy = equationsGainedInfo[0].entropy;
				return equationsGainedInfo;
			}
			else
			{
				bestEntropy = equationsGainedInfo[0].entropy;
				for (int i = 0; i < equationsGainedInfo.Length; i++)
				{
					if (equationsGainedInfo[i].entropy > bestEntropy)
					{
						bestEntropy = equationsGainedInfo[i].entropy;
					}
				}
				return null;
			}
		}


		/// <summary>
		/// Checks whether specified solution is the possible answer, 
		/// if processed guess returned specified result
		/// </summary>
		/// <param name="solution"> Solution which is being checked for being possible </param>
		/// <param name="guess"> Equation which was guessed </param>
		/// <param name="result"> Result of checking <paramref name="guess"/></param>
		/// <returns></returns>
		bool IsSolutionSuitableForGuess( Equation solution, Equation guess, GuessResult result )
		{
			for (int i = 0; i < GuessableCellsCount; i++)
			{
				byte currentCell = guess[i];
				byte solutionCell = solution[i];
				if (result[i] == GuessColor.Green)
				{
					if (currentCell == solutionCell)
					{
						continue;
					}
					else
					{
						return false;
					}
				}
				if (currentCell == solutionCell)
				{
					return false;
				}
				int misplacedTotal = 0;
				for (int j = 0; j < GuessableCellsCount; j++)
				{
					if (solution[j] == currentCell && solution[j] != guess[j])
					{
						misplacedTotal++;
					}
				}
				int misplacedBeforeCurrent = 0;
				for (int j = 0; j < i; j++)
				{
					if (guess[j] == currentCell && guess[j] != solution[j])
					{
						misplacedBeforeCurrent++;
					}
				}
				if (misplacedBeforeCurrent < misplacedTotal)
				{
					if (result[i] == GuessColor.Yellow)
					{
						continue;
					}
					else
					{
						return false;
					}
				}
				else
				{
					if (result[i] == GuessColor.Black)
					{
						continue;
					}
					else
					{
						return false;
					}
				}
			}
			return true;
		}

	}
}