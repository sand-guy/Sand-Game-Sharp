using Godot;
using System;
using System.Collections.Generic;

// The ChunkMap is used exclusively for coordinating and improving the performance of the physical simulation (sleeping chunks)
// CellData are stored in an array and hold the granular information about each cell
// Cells don't know what chunk they're in and chunks don't know what's in them (it doesn't necessarily stay the same each frame)
// Bottom line: chunks are a fiction of how the game iterates through cells and decides what to process
public class ChunkMap
{
	// TODO - These arrays all store the data that's used for chunks at the same indices,
	//			Make a "Chunk" class that can hold this data to clean this up a bit in future
	// TODO - Implement dirty rects to minimize the area within a chunk that needs to be rendered/simulated (these two things are tied together)
	//          This may benefit from a larger chunk size
	// TODO - Implement a method of visualizing (active/rendered) chunks on screen
	// TODO - Make all of this data private! SandSimulation shouldn't be touching it directly

	
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

		// Generate odd and even column indices to feed to the threaded simulation
		// These would need to be regenerated if the simulation was resized
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

	// This is the unified method for waking up a chunk in the next frame
	// If you want to awaken a chunk, ALWAYS CALL THIS!
	// This method accepts simulation coordinates, and awakens the chunk that contains that cell
	// It also awakens any adjacent chunks if the cell lands on a chunk border
	public void AwakenChunk(int simRow, int simCol)
	{
		ShouldAwaken[GetChunkIndex(simRow, simCol)] = true; // Wake up this chunk
		Updated[GetChunkIndex(simRow, simCol)] = true; // Rerender this chunk this frame
		int chunkRow = simRow % ChunkSize;
		int chunkCol = simCol % ChunkSize;

		// If we are awakening a chunk from a cell at any of the edges of a chunk
		// We must also awaken the adjacent chunks
		if (simRow > 0 && chunkRow == 0)
		{
			ShouldAwaken[GetChunkIndex(simRow - 1, simCol)] = true;
			Updated[GetChunkIndex(simRow - 1, simCol)] = true;
		}
		if (simRow < Height - 1 && chunkRow == ChunkSize - 1)
		{
			ShouldAwaken[GetChunkIndex(simRow + 1, simCol)] = true;
			Updated[GetChunkIndex(simRow + 1, simCol)] = true;
		}
		if (simCol > 0 && chunkCol == 0)
		{
			ShouldAwaken[GetChunkIndex(simRow, simCol - 1)] = true;
			Updated[GetChunkIndex(simRow, simCol - 1)] = true;
		}
		if (simRow < Width - 1 && chunkCol == ChunkSize - 1)
		{
			ShouldAwaken[GetChunkIndex(simRow, simCol + 1)] = true;
			Updated[GetChunkIndex(simRow, simCol + 1)] = true;
		}
	}

	// A (potentially) slightly more performant version of using AwakenChunk
	// This may only be called within an Element's process function
	//      For example: when an element could have moved (according to IsSwappable), but was made not to (perhaps by a random check)
	//      Call this to make sure it tries that check again in the next frame!
	// Calls of this nature in the SandSimulation class should be made using AwakenChunk to prevent artifacts
	public void DontSleepNextFrame(int simRow, int simCol)
	{
		if (WakeTime[GetChunkIndex(simRow, simCol)] <= 1)
		{
			WakeTime[GetChunkIndex(simRow, simCol)]++;
		}
	}

	// Update the CellCount based on the replacing and replaced cells
	public void CellChanged(int simRow, int simCol, int oldCellType, int newCellType)
	{
		if (oldCellType == 0 && newCellType != 0) // Air is being replaced by a live cell
		{
			CellCount[GetChunkIndex(simRow, simCol)]++;
		}
		else if (oldCellType != 0 && newCellType == 0) // A live cell is being replaced by air (erased)
		{
			CellCount[GetChunkIndex(simRow, simCol)]--;
		}
	}

	private int GetChunkIndex(int simRow, int simCol)
	{
		int index = simRow / ChunkSize * Width + simCol / ChunkSize;

		if (index > Width * Height) return 0;

		return index;
	}
	public bool WasUpdated(int simRow, int simCol)
	{
		return Updated[GetChunkIndex(simRow, simCol)];
	}

	public int GetCellCount(int chunkRow, int chunkCol)
	{
		return CellCount[chunkRow * ChunkSize + chunkCol];
	}

	public bool IsSleeping(int chunkRow, int chunkCol)
	{
		return WakeTime[chunkRow * ChunkSize + chunkCol] == 0;
	}

	// Returns the index of the first cell in a given chunk
	public int GetStartingCellIndex(int chunkRow, int chunkCol)
	{
		return chunkRow * (Width * ChunkSize) + chunkCol * ChunkSize - 1;
	}
}
