using Godot;
using System;

public partial class Sand : Powder
{
	// The percent chance that the particle will have diagonal movement applied, if possible
	float PowderSlowing = 1.0f;

	// Sand implements the default PowderProcess for its Process function
	public override void Process(SandSimulation sim, int row, int col)
	{
		Physics.PowderProcess(sim, row, col, PowderSlowing);
	}

	// PHYSICS VARIABLES
	
	public override double Density
	{
		get { return 2.0; }
	}

	public override bool Burning
	{
		get { return false; } 
	}

	// RENDERING VARIABLES

	public override byte[] A_Color { get { return new byte[] {212, 189, 108}; } }
	public override byte[] B_Color { get { return new byte[] {190, 170, 100}; } }
}
