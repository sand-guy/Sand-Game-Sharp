using System;

// TODO - Add gravity and velocity to CellData struct, implement it here
public static class Physics
{
	// A process for elements to behave like a liquid
	// Call this when you want something to look like water
	// TODO  - Simplify this and make the water spread out faster
	public static void LiquidProcess(SandSimulation sim, int row, int col, int dispersion, float viscosity)
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
			bool first = true;
			int sign = (sim.Randf() < 0.5 ? 1 : -1); // 50% chance to attempt moves left or right this frame
			int colChange = 0;
			for (int i = 0; i < dispersion; i++) // Attempt to move horizontally "dispersion" times
			{
				colChange += sign; // Add one horizontal movement
				if (!sim.IsSwappable(row, col, row, col + colChange) && first) { // If we can't make the first horizontal move, try moving the other direction
					colChange -= sign;
					first = false;
					break;
				} else if (first) {
					first = false;
				}
				if (!sim.IsSwappable(row, col, row, col + colChange) && !first) { // If we can't make any subsequent horizontal move, undo it and stop trying
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

	// Default behavior for elements inheriting Powder
	// Call this within the overriding Process method of the inherting Element to utilize
	// Or don't! This is just an inherited default that gets used by Sand
	public static void PowderProcess(SandSimulation sim, int row, int col, float powderSlowing)
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

	
	// Default behavior for a gas that floats up and away, disappearing after it reaches the top of the simulation
	// Volatility is a chance to move randomly, instead of attempting to move up
	// Dispersion is the number of time the element will attempt to move horizontally
	// upwardPreference is the percent chance that upward movement will be called as the preferential first
	public static void GasProcess(SandSimulation sim, int row, int col, float volatility, int dispersion, float upwardPreference)
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

		if (sim.Randf() > upwardPreference) {
			return;
		}

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
}
