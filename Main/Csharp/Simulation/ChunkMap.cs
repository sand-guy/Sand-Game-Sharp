using Godot;
using System;
using System.Collections.Generic;

// The ChunkMap is used exclusively for coordinating and improving the performance of the physical simulation (sleeping chunks)
// CellData are stored in an array and hold the granular information about each cell
// Cells don't know what chunk they're in and chunks don't know what's in them (it doesn't necessarily stay the same each frame)
// Bottom line: chunks are a fiction of how the game iterates through cells and decides what to process
public class ChunkMap
{
	// Dimension of each chunk
	public int ChunkSize = 16;
	// The chance that a sleeping chunk can randomly reawaken for a frame
	public float AwakenChance = 0.146f;

	// Simulation dimensions in chunks
	// Since resizing is not currently supported, this is calculated just once in the constructor of this class
	public int Width = 0;
	public int Height = 0;

	// TODO - These arrays all store the data that's used for chunks at the same indices,
	//			Make a "Chunk" class that can hold this data to clean this up a bit in future
	// TODO - Implement dirty rects to minimize the area within a chunk that needs to be rendered/simulated (these two things are tied together)
	//          This may benefit from a larger chunk size
	// TODO - Implement a method of visualizing (active/rendered) chunks on screen

	// Stores the number of non-empty cells within each chunk
	// Could be a byte, but this would limit the maximum chunk size
	public int[] CellCount;
	// The amount of frames a chunk should be processed
	// 0 indicates a sleeping chunk
	// >1 indicates a chunk that should be checked again next frame
	public byte[] WakeTime;
	// Stores whether a chunk should be awakened on the next frame
	public bool[] ShouldAwaken;
	// Stores whether a given chunk has had visual updates within this frame
	public bool[] Updated;

	public List<int> EvenRows = new List<int>();
	public List<int> OddRows = new List<int>();

	public ChunkMap(int simWidthCells, int simHeightCells)
	{
		if (simWidthCells % ChunkSize != 0 || simHeightCells % ChunkSize != 0)
		{
			GD.PrintErr("Simulation width and/or height not divisible by chunk size!");
			throw new ArgumentException();
		}

		Width = simWidthCells / ChunkSize;
		Height = simHeightCells / ChunkSize;

		CellCount = new int[Width * Height];
		WakeTime = new byte[Width * Height];
		ShouldAwaken = new bool[Width * Height];
		Updated = new bool[Width * Height];

		Array.Fill(Updated, true); // Ensure that all chunks are updated on the first frame

		// Generate odd and even row indices to feed to the threaded simulation
		// Must be edited if the simulation is resized
		for (int chunkRow = 0; chunkRow < Height; chunkRow++)
		{
			if (chunkRow % 2 == 0)
			{
				EvenRows.Add(chunkRow);
			} else {
				OddRows.Add(chunkRow);
			}
		}
	}


}
