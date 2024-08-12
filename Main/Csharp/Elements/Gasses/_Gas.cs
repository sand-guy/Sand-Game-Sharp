using Godot;
using System;

public abstract partial class Gas : Element
{
	// PHYSICS VARIABLES

	// For any element inheriting Gas, return 2 for GetState
	// This indicates that it's a gas for physics calculations
	public override int GetState
	{
		get { return 2; }
	}
}
