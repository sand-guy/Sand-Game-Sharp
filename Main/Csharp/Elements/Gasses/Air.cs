using Godot;
using System;

public partial class Air : Gas
{
	// IMPLEMENT PROCESS FUNCTION
	public override void Process(SandSimulation sim, int row, int col)
	{
		return;
	}

	// PHYSICS VARIABLES
	
	public override double GetDensity
	{
		get { return 0.0; }
	}

	public override bool GetFlammable
	{
		get { return false; } 
	}

	// RENDERING VARIABLES

	byte r_val = 70;
	byte g_val = 70;
	byte b_val = 70;

	public override byte[] GetColor
	{
		get { return new byte[3] { r_val, g_val, b_val }; }
	}
}
