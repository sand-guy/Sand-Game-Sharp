using Godot;
using System;

public partial class Wood : Static
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

	byte r_val = 110;
	byte g_val = 77;
	byte b_val = 34;

	public override byte[] A_Color { get { return new byte[] {110, 77, 34}; } }
	public override byte[] B_Color { get { return new byte[] {84, 58, 24}; } }
}
