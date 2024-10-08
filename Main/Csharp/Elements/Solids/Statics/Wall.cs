using Godot;
using System;

public partial class Wall : Static
{
	// Statics do not implement a Process function!

	// PHYSICS VARIABLES
	
	public override double Density
	{
		get { return 2.0; }
	}

	public override bool Burning
	{
		get { return false; } 
	}

	// RENDERING VARIABLES

	public override byte[] A_Color { get { return new byte[] {112, 109, 100}; } }
	public override byte[] B_Color { get { return new byte[] {90, 90, 90}; } }
}
