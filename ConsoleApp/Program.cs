using MathleSolver;
using System.Globalization;
using System.Diagnostics;
using System.IO;

namespace ConsoleApp
{
	class Program
	{
		static void Main( string[] args )
		{
			Solver solver = new Solver();
			var bestGuesses = TryLoadGuessesCache( solver.EquationsCount );
			if (bestGuesses == null)
			{
				bestGuesses = solver.GetBestGuesses();
				SaveBestGuessesCache( bestGuesses );
			}
			//CalculateAverage( solver, bestGuesses );
			while (true)
			{
				Console.WriteLine( $"Solutions remaining: {solver.SolutionsCount}, information =" +
					$" {solver.RemainingInformation}" );
				if (solver.SolutionsCount < 25)
				{
					Console.WriteLine( "Remaining solutions:" );
					foreach (var solution in solver.RemainingSolutions)
					{
						Console.WriteLine( $"{solution}; information =" +
							$" {solver.CalculateExpectedEntropy( solution )}" );
					}
				}
				Console.WriteLine( "Best guesses:" );
				for (int i = 0; i < bestGuesses.Length && i < 35; i++)
				{
					Console.WriteLine( bestGuesses[i] );
				}
				bool validInput = false;
				while (!validInput)
				{
					try
					{
						Equation guess = new Equation( Console.ReadLine() );
						GuessResult result = new GuessResult( Console.ReadLine() );
						validInput = true;
						solver.ProcessInput( guess, result );
						bestGuesses = solver.GetBestGuesses();
					}
					catch
					{
						validInput = false;
						Console.WriteLine( "Invalid input, please try again" );
					}
				}
			}
		}

		static void CalculateAverage( Solver solver, EquationWithEntropy[] firstBestGuesses )
		{
			int[] triesCount = new int[20];
			int solvedCount = 0;
			Stopwatch sw = Stopwatch.StartNew();
			foreach (var correctSolution in solver.AllPossibleEquations)
			{
				int currentSolutionTries = 0;
				var guess = firstBestGuesses[0].equation;
				bool guessedCorrectly = false;
				while (!guessedCorrectly)
				{
					currentSolutionTries++;
					var guessResult = solver.GetGuessResult( guess, correctSolution );
					guessedCorrectly = guessResult.IsFullyCorrect();
					if (!guessedCorrectly)
					{
						solver.ProcessInput( guess, guessResult );
						var newBestGuesses = solver.GetBestGuesses();
						guess = newBestGuesses[0].equation;
					}
				}
				triesCount[currentSolutionTries]++;
				solvedCount++;
				if (solvedCount % 1000 == 0)
				{
					Console.WriteLine( $"{solvedCount} at {sw.Elapsed.TotalSeconds}" );
				}
				solver.Reset();
			}
			int totalTries = 0;
			for (int i = 0; i < triesCount.Length; i++)
			{
				Console.WriteLine( $"{i} tries: {triesCount[i]} equations" );
				totalTries += i * triesCount[i];
			}
			double averagePerSoultion = (double)totalTries / solver.AllPossibleEquations.Count();
			Console.WriteLine( $"Average is {averagePerSoultion}" );
		}

		static EquationWithEntropy[]? TryLoadGuessesCache( int equationsCount )
		{
			if (!File.Exists( "mathle.txt" ))
			{
				return null;
			}
			try
			{
				string[] lines = File.ReadAllLines( "mathle.txt" );
				var bestGuesses = new EquationWithEntropy[equationsCount];
				if (lines.Length < bestGuesses.Length * 2)
				{
					throw new ArgumentException( "Guesses cache containt too few lines" );
				}
				for (int i = 0; i < bestGuesses.Length; i++)
				{
					Equation eq = new Equation( lines[i * 2] );
					double info = double.Parse( lines[i * 2 + 1], CultureInfo.InvariantCulture );
					bestGuesses[i] = new EquationWithEntropy( eq, info );
				}
				return bestGuesses;
			}
			catch (Exception ex)
			{
				Console.WriteLine( "Exception occured when trying to parse cached guesses" );
				Console.WriteLine( ex.Message );
				return null;
			}
		}

		static void SaveBestGuessesCache( EquationWithEntropy[] guesses )
		{
			try
			{
				string[] lines = new string[guesses.Length * 2];
				for (int i = 0; i < guesses.Length; i++)
				{
					lines[i * 2] = guesses[i].equation.ToString();
					lines[i * 2 + 1] = guesses[i].entropy.ToString( CultureInfo.InvariantCulture );
				}
				File.WriteAllLines( "mathle.txt", lines );
			}
			catch (Exception ex)
			{
				Console.WriteLine( "Exception occured when trying to save cache" );
				Console.WriteLine( ex.Message );
			}
		}
	}
}