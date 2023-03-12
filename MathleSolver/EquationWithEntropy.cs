namespace MathleSolver
{
	public readonly struct EquationWithEntropy
	{
		public readonly Equation equation;
		public readonly double entropy;

		public EquationWithEntropy( Equation equation, double entropy )
		{
			this.equation = equation;
			this.entropy = entropy;
		}

		public override string ToString()
		{
			return $"{equation}; entropy = {entropy}";
		}
	}
}