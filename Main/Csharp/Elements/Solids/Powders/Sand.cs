using Godot;
using System;

public partial class Sand : Powder
{
	// A percent chance to not move the particle
	float PowderSlowing = 0.95f;

	// Sand implements the default PowderProcess for its Process function
	public override void Process(SandSimulation sim, int row, int col)
	{
		base.PowderProcess(sim, row, col, PowderSlowing);
	}

	// PHYSICS VARIABLES
	
	public override double GetDensity
	{
		get { return 2.0; }
	}

	public override bool GetFlammable
	{
		get { return false; } 
	}

	// RENDERING VARIABLES

	byte r_val = 244;
	byte g_val = 218;
	byte b_val = 128;

	public override byte[] GetColor
	{
		get { return new byte[3] { r_val, g_val, b_val }; }
	}
}
