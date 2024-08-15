using Godot;
using System;

public partial class Fire : Gas
{
	// Properties are explained in the parent class
	int Dispersion = 2;
	float Volatility = 0.28f;
	float UpwardPreference = 0.108f;

	// The percent chance every frame to turn into smoke
	float Lifespan = 0.0086f;

	// IMPLEMENT PROCESS FUNCTION
	public override void Process(SandSimulation sim, int row, int col)
	{
		if (sim.Randf() < Lifespan) { // Perform a random check to turn fire into smoke
			sim.SetCell(row, col, new CellData(sim, 6));
			return;
		}

		Physics.GasProcess(sim, row, col, Volatility, Dispersion, UpwardPreference);
	}

	// PHYSICS VARIABLES
	
	public override double Density
	{
		get { return 0.2; }
	}

	public override bool Burning
	{
		get { return true; } 
	}

	// RENDERING VARIABLES

	public override byte[] A_Color { get { return new byte[] {255, 94, 0}; } }
	public override byte[] B_Color { get { return new byte[] {255, 77, 0}; } }
}
