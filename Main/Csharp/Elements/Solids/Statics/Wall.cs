using Godot;
using System;

public partial class Wall : Static
{
	// Statics do not implement a Process function!

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

	byte r_val = 112;
	byte g_val = 109;
	byte b_val = 100;

	public override byte[] GetColor
	{
		get { return new byte[3] { r_val, g_val, b_val }; }
	}
}
