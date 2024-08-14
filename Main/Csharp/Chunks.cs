using Godot;
using System;

public class Chunks
{
	// Dimension of each chunk
	public int ChunkSize = 16;
	// The chance that a sleeping chunk can randomly reawaken for a frame
	public float AwakenChance = 0.146f;

	// Simulation dimensions in chunks
	// Since resizing is not currently supported, this is calculated just once in the constructor of this class
	public int Width = 0;
	public int Height = 0;

	// Stores the number of non-empty cells within each chunk
	// Could be a byte, but this would limit the maximum chunk size
	public int[] CellCount; // AliveCount
	// Stores whether a chunk is awake or not, 0 is asleep and 1-2 is awake (will process an extra 1-2 frames until sleeping again)
	public byte[] WakeTime; // AwakeChunk
	// Stores whether a chunk should be awakened on the next frame
	public bool[] ShouldAwaken; // ShouldAwakenChunk
	// Stores whether a given chunk has had visual updates within this frame
	// TODO - Implement into drawing functions
	public bool[] Updated; // ChunkUpdated

	public Chunks(int simWidthCells, int simHeightCells)
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
	}


}
