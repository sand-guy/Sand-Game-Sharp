using Godot;
using System;

public abstract partial class Liquid : Element
{
	// Default behavior for elements inheriting Liquid
	// Call this within the overriding Process method of the inherting Element to utilize
	// Or don't! This is just an inherited default that gets used by Water
	public void LiquidProcess(SandSimulation sim, int row, int col, int dispersion, float viscosity)
	{
		int newRow = row;
		int newCol = col;

		bool downLeft = sim.IsSwappable(row, col, row + 1, col - 1);
		bool down = sim.IsSwappable(row, col, row + 1, col);
		bool downRight = sim.IsSwappable(row, col, row + 1, col + 1);

		// First attempt sand movement
		if (down) {
			newRow++; // Move down if possible
		} else if (downLeft && downRight) {
			newRow++;
			newCol += (sim.Randf() < 0.5 ? 1 : -1); // Pick randomly left or right if both are an option
		} else if (downLeft) {
			newRow++;
			newCol--;
		} else if (downRight) {
			newRow++;
			newCol++;
		} else { // If we can't move down or diagonal, attempt to move horizontally
			int sign = (sim.Randf() < 0.5 ? 1 : -1); // 50% chance to attempt moves left or right this frame
			int colChange = 0;
			for (int i = 0; i < dispersion; i++) // Attempt to move horizontally "dispersion" times
			{
				colChange += sign; // Add one horizontal movement
				if (!sim.IsSwappable(row, col, row, col + colChange)) { // If we can't make the current horizontal move, undo it and stop trying to move further
					colChange -= sign;
					break;
				}
			}
			newCol = col + colChange; // Apply the horizontal changes 
		}

		if (sim.Randf() >= viscosity) { // Viscosity is expressed as a percent chance to not apply ANY movement in a given frame (higher > more viscous)
			sim.MoveAndSwap(row, col, newRow, newCol);
		} else if (row != newRow || col != newCol){
			sim.DontSleepNextFrame(row, col); // If the liquid did not move this frame, but could have, make sure its chunk is not put to sleep next frame to avoid artifacts
		}
	}

	public override int GetState
	{
		get { return 1; }
	}
}