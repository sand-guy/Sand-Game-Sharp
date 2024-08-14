using Godot;
using System;

public partial class Smoke : Gas
{
	// Properties are explained in the parent class
	int Dispersion = 3;
	float Volatility = 0.22f;
	float UpwardPreference = 0.8f;

	// Percent chance to disappear during a frame
	float Dissipation = 0.0066f;

	// IMPLEMENT PROCESS FUNCTION
	public override void Process(SandSimulation sim, int row, int col)
	{	
		if (sim.Randf() < Dissipation) {
			sim.SetCell(row, col, new CellData(sim, 0));
			return;
		}
		GasProcess(sim, row, col, Volatility, Dispersion, UpwardPreference);
	}

	// PHYSICS VARIABLES
	
	public override double Density
	{
		get { return 0.2; }
	}

	public override bool Burning
	{
		get { return false; } 
	}

	// RENDERING VARIABLES

	public override byte[] A_Color { get { return new byte[] {214, 214, 214}; } }
	public override byte[] B_Color { get { return new byte[] {200, 200, 200}; } }
}
