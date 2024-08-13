using Godot;
using System;

public abstract partial class Gas : Element
{
	// Default behavior for a gas that floats up and away, disappearing after it reaches the top of the simulation
	// Call this within the overriding Process method of the inherting Element to utilize
	// Or don't! This is just an inherited default that gets used by Smoke
	public void GasProcess(SandSimulation sim, int row, int col, float volatility, int dispersion)
	{
		if (volatility >= sim.Randf()) // Random chance to attempt a move into any of the 8 nearby cells instead of following normal logic
		{
			
			int colChange = 0;
			switch (Math.Ceiling(sim.Randf() * 3)) {
				case 1:
					colChange++;
					break;
				case 2:
					colChange--;
					break;
				default:
					break;
			}

			int rowChange = 0;
			switch (Math.Ceiling(sim.Randf() * (3 - (rowChange == 0 ? 1 : 0)))) { // If no rowChange, force a column change
				case 1:
					rowChange++;
					break;
				case 2:
					rowChange--;
					break;
				default:
					break;
			}
			
			// If we can can apply this random movement, don't try to do anything else this frame
			if (sim.IsSwappable(row, col, row + rowChange, col + colChange)) {
				sim.MoveAndSwap(row, col, row + rowChange, col + colChange);
				return;
			}
		}

		bool upLeft = sim.IsSwappable(row, col, row - 1, col - 1);
		bool up = sim.IsSwappable(row, col, row - 1, col);
		bool upRight = sim.IsSwappable(row, col, row - 1, col + 1);

		int newRow = row;
		int newCol = col;

		// First attempt sand movement, flipped vertically
		if (up) {
			newRow--; // Move up if possible
		} else if (upLeft && upRight) {
			newRow--;
			newCol += (sim.Randf() < 0.5 ? 1 : -1); // Pick randomly left or right if both are an option
		} else if (upLeft) {
			newRow--;
			newCol--;
		} else if (upRight) {
			newRow--;
			newCol++;
		} else { // If we can't move up or diagonal, attempt to move horizontally
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

		sim.MoveAndSwap(row, col, newRow, newCol);
	}

	// PHYSICS VARIABLES

	// For any element inheriting Gas, return 2 for GetState
	// This indicates that it's a gas for physics calculations
	public override int GetState
	{
		get { return 2; }
	}
}
