using Godot;
using System;

public abstract partial class Static : Solid
{
	// Statics do not move, thus they have an empty Process method
	// Things like burning and explosions happen in global calculations
	//     - Utilize things like GetFlammable within the ThreadProcess in the SandSimulation
	public override void Process(SandSimulation sim, int row, int col)
	{
		return;
	}

	// PHYSICS VARIABLES

	// Override the default value of false for GetStatic
	// Anything that inherits Static cannot be replaced no matter what its density is
	public override bool GetStatic
	{
		get { return true; }
	}
}
