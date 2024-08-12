using Godot;
using System;

public abstract partial class Solid : Element
{
	// PHYSICS VARIABLES

	// For any element inheriting Solid, return 0 for GetState
	// This indicates that it's a solid for physics calculations
	public override int GetState
	{
		get { return 0; }
	}
}
