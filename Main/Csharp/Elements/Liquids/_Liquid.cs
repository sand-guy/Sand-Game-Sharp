using Godot;
using System;

public abstract partial class Liquid : Element
{
	// Default behavior for elements inheriting Liquid
	// Call this within the overriding Process method of the inherting Element to utilize
	// Or don't! This is just an inherited default that gets used by Water
	public void LiquidProcess(SandSimulation sim, int row, int col, int fluidity)
	{
		int sign = (sim.Randf() < 0.5 ? 1 : -1); // 50% chance to attempt moves left or right this frame
		int colChange = sign;
		for (int i = 0; i < fluidity; i++)
		{
			if (sim.Randf() < 0.85) // 85% chance to attempt horizontal movement in a given loop
			{
				if (sim.IsSwappable(row, col, row, col + colChange) && !sim.IsSwappable(row, col, row + 1, col + colChange)) { // If we can make this horizontal move and could not move to the cell below it, apply it
					colChange += sign;
				} else { // Otherwise move on to the next part of the process, stop checking
					break;
				}
			}
		}
		int newCol = col;
		if (sim.IsSwappable(row, col, row, col + colChange)) {
			newCol += colChange;
		}

		int newRow = row + (sim.IsSwappable(row, col, row + 1, newCol) ? 1 : 0); // If it is allowed, attempt to move down

		sim.MoveAndSwap(row, col, newRow, newCol);
	}

	public override int GetState
	{
		get { return 1; }
	}
}