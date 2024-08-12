using Godot;
using System;

public abstract partial class Powder : Solid
{
	// Default behavior for elements inheriting Powder
	// Call this within the overriding Process method of the inherting Element to utilize
	// Or don't! This is just an inherited default that gets used by Sand
	public void PowderProcess(SandSimulation sim, int row, int col, float powderSlowing)
	{
		if (sim.Randf() >= powderSlowing) {
			return;
		}	

		bool downLeft = sim.IsSwappable(row, col, row + 1, col - 1);
		bool down = sim.IsSwappable(row, col, row + 1, col);
		bool downRight = sim.IsSwappable(row, col, row + 1, col + 1);

		if (down)
		{
			sim.MoveAndSwap(row, col, row + 1, col);
		} else if (downLeft && downRight)
		{
			sim.MoveAndSwap(row, col, row + 1, col + (sim.Randf() < 0.5 ? 1 : -1));
		} else if (downLeft)
		{
			sim.MoveAndSwap(row, col, row + 1, col - 1);
		} else if (downRight)
		{
			sim.MoveAndSwap(row, col, row + 1, col + 1);
		}
	}
}
