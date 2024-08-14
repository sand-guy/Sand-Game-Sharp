using Godot;
using System;

public abstract partial class Powder : Solid
{
	// Default behavior for elements inheriting Powder
	// Call this within the overriding Process method of the inherting Element to utilize
	// Or don't! This is just an inherited default that gets used by Sand
	public void PowderProcess(SandSimulation sim, int row, int col, float powderSlowing)
	{
		bool down = sim.IsSwappable(row, col, row + 1, col);

		if (down) { // Always attempt to move straight down
			sim.MoveAndSwap(row, col, row + 1, col);
			return;
		} else if (sim.Randf() >= powderSlowing) { // powderSlowing can only stop the powder from moving diagonally in a given frame
			sim.DontSleepNextFrame(row, col); // If the powder was not allowed to move this frame, make sure its chunk is not put to sleep for the next frame to avoid artifacts
			return;
		}

		// If we cannot move down, and powderSlowing did not prevent us from moving this frame
		// Attempt to move diagonally down
		bool downLeft = sim.IsSwappable(row, col, row + 1, col - 1);
		bool downRight = sim.IsSwappable(row, col, row + 1, col + 1);

		if (downLeft && downRight) {
			sim.MoveAndSwap(row, col, row + 1, col + (sim.Randf() < 0.5 ? 1 : -1));
		} else if (downLeft) {
			sim.MoveAndSwap(row, col, row + 1, col - 1);
		} else if (downRight) {
			sim.MoveAndSwap(row, col, row + 1, col + 1);
		}
	}
}
