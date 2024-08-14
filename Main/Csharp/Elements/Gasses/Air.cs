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
	
	public override double Density
	{
		get { return 0.0; }
	}

	public override bool Burning
	{
		get { return false; } 
	}

	public override byte[] A_Color { get { return null; } }
	public override byte[] B_Color { get { return null; } }
}
