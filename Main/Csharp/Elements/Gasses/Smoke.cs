using Godot;
using System;

public partial class Smoke : Gas
{
	// The number of times the particle will attempt to spread out horizontally during a given frame, if it can't move up
	int Dispersion = 4;

	// The percent chance to apply a completely random movement to the particle
	float Volatility = 0.1f;

	// IMPLEMENT PROCESS FUNCTION
	public override void Process(SandSimulation sim, int row, int col)
	{
		base.GasProcess(sim, row, col, Volatility, Dispersion);
	}

	// PHYSICS VARIABLES
	
	public override double GetDensity
	{
		get { return 0.2; }
	}

	public override bool GetFlammable
	{
		get { return false; } 
	}

	// RENDERING VARIABLES

	byte r_val = 214;
	byte g_val = 214;
	byte b_val = 214;

	public override byte[] GetColor
	{
		get { return new byte[3] { r_val, g_val, b_val }; }
	}
}
