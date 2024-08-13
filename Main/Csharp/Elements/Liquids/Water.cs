using Godot;
using System;

public partial class Water : Liquid
{
	// The number of times the particle will attempt to spread out horizontally during a given frame, if it can't move down
	int Dispersion = 5;

	// The percent chance that, even if it is possible, the cell will not move in a given frame
	float Viscosity = 0.0f;

	// Sand implements the default PowderProcess for its Process function
	public override void Process(SandSimulation sim, int row, int col)
	{
		base.LiquidProcess(sim, row, col, Dispersion, Viscosity);
	}

	// PHYSICS VARIABLES
	
	public override double GetDensity
	{
		get { return 1.0; }
	}

	public override bool GetFlammable
	{
		get { return false; } 
	}

	// RENDERING VARIABLES

	byte r_val = 66;
	byte g_val = 135;
	byte b_val = 245;
	public override byte[] GetColor
	{
		get { return new byte[3] { r_val, g_val, b_val }; }
	}
}
