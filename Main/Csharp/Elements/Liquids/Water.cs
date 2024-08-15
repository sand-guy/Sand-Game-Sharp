using Godot;
using System;

public partial class Water : Liquid
{
	// The number of times the particle will attempt to spread out horizontally during a given frame, if it can't move down
	int Dispersion = 5;

	// The percent chance that, even if it is possible, the cell will not move in a given frame (other than directly down)
	// TODO - Remove this? The effect isn't very good and it has to be set awkwardly high to have any noticeable effect because of how it operates
	float Viscosity = 0.0f;

	// Sand implements the default PowderProcess for its Process function
	public override void Process(SandSimulation sim, int row, int col)
	{
		Physics.LiquidProcess(sim, row, col, Dispersion, Viscosity);
	}

	// PHYSICS VARIABLES
	
	public override double Density
	{
		get { return 1.0; }
	}

	public override bool Burning
	{
		get { return false; } 
	}

	// RENDERING VARIABLES

	public override byte[] A_Color { get { return new byte[] {66, 135, 245}; } }
	public override byte[] B_Color { get { return new byte[] {38, 114, 237}; } }
}
