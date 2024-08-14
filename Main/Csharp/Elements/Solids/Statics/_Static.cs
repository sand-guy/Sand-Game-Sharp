using Godot;
using System;

public abstract partial class Static : Solid
{
	// Override this process for flammable statics, like wood
	public override void Process(SandSimulation sim, int row, int col)
	{
		return;
	}

	// PHYSICS VARIABLES

	// Override the default value of false for GetStatic
	// Anything that inherits Static cannot be replaced no matter what its density is
	public override bool IsStatic
	{
		get { return true; }
	}
}
