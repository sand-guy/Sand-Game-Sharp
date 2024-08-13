using Godot;
using Godot.NativeInterop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

[GlobalClass]
public partial class SandSimulation : TextureRect
{
	// Shared random instance

	Random Rand;

	// Simulation dimensions
	private int Width = 400;
	private int Height = 240;

	// Contains the identity of all cells on the grid
	int[] Cells;

	// Drawing data to be used to generate an image once per frame
	byte[] DrawData;

	Chunks Chunks;

	// MULTITHREADING VARIABLES

	// The random "row offset" (vertical shift) that will be applied each frame to the chunk processing to avoid artifacts along chunk borders
	int GlobalRowOffset;
	// The maximum row offset to use in a given frame, positive or negative
	int MaxRowOffset = 8;

	int FrameCount = 0;


	public override void _Ready()
	{
		base._Ready();

		GetWindow().ContentScaleSize = new Vector2I(Width, Height);
	}

	public SandSimulation()
	{
		ElementList.FillElementList();

		Rand = new Random();

		Chunks = new Chunks(Width, Height);
		// GD.Print("INITIALIZATION - Array in chunks is " + ChunkWidth + "x" + ChunkHeight);

		Array.Resize(ref DrawData, Width * Height * 3);

		Array.Resize(ref Cells, Width * Height);
	}

	// Advances the simulation
	public override void _Process(double delta)
	{
		// GD.Print("_PROCESS - Frame called: " + FrameCount);

		List<int> evenChunkRows = new List<int>();
		List<int> oddChunkRows = new List<int>();

		for (int chunkRow = 0; chunkRow < Chunks.Height; chunkRow++)
		{
			if (chunkRow % 2 == 0)
			{
				evenChunkRows.Add(chunkRow);
			} else {
				oddChunkRows.Add(chunkRow);
			}
		}

		Array.Fill<bool>(Chunks.ShouldAwaken, false);
		GlobalRowOffset = Rand.Next(0, MaxRowOffset + 1); // Generate a random row offset to apply to threaded chunk processing
		GlobalRowOffset *= (Randf() < 0.5 ? -1 : 1); // 50/50 to make the offset positive or negative

		// Simulate the grid by giving threads a row of chunks to process
		// Process all even rows, then all odd rows to prevent overlapping attempts at cell access

		//GD.Print("_PROCESS - Processing even chunks");
		Parallel.ForEach(evenChunkRows, chunkRow => ThreadProcess(chunkRow));
		// foreach (int chunkRow in evenChunkRows) ThreadProcess(chunkRow);
		// GD.Print("_PROCESS - Even chunks complete");

		// GD.Print("_PROCESS - Processing odd chunks");
		Parallel.ForEach(oddChunkRows, chunkRow => ThreadProcess(chunkRow));
		// foreach (int chunkRow in oddChunkRows) ThreadProcess(chunkRow);
		// GD.Print("_PROCESS - Odd chunks complete");

		for (int i = 0; i < Chunks.WakeTime.Length; i++) // Iterate through all of the chunks
		{
			if (Chunks.ShouldAwaken[i]) // If the chunk should be awakened...
			{
				Chunks.WakeTime[i] = 2; // ...set it to process for an extra two frames 
			}
			else
			{
				Chunks.WakeTime[i] = (byte)Math.Max(0, Chunks.WakeTime[i] - 1); // Otherwise subtract one from the number of extra frames left to process or keep it at zero
			}
		}

		//GD.Print("_PROCESS - Frame " + FrameCount + " processed with frame time " + delta);

		// Debug wait timer, to minimize console spam and/or slow down the simulation
		// Thread.Sleep(100);


		// GD.Print("_PROCESS - Painting frame " + FrameCount);
		RepaintCanvas();

		FrameCount++;
	}

	private void ThreadProcess(int chunkRow)
	{
		// GD.Print("THREADPROCESS - Beginning queued task at chunk row " + chunkRow + " in thread " + Thread.CurrentThread.ManagedThreadId);

		List<int> ParticleOrder = new List<int>();
		for (int i = 0; i < Chunks.ChunkSize * Chunks.ChunkSize; i++)
		{
			ParticleOrder.Add(i);
		}

		for (int i = chunkRow * Chunks.Width; i < (chunkRow + 1) * Chunks.Width; i++)
		{
			if (Chunks.CellCount[i] == 0 || (Chunks.WakeTime[i] == 0 && Randf() > Chunks.AwakenChance))
			{
				continue;
			}

			int RowOffset = i / Chunks.Width * Chunks.ChunkSize + GlobalRowOffset;
			int ColOffset = i % Chunks.Width * Chunks.ChunkSize;
			// Shuffle the particle order to avoid artifacts
			ParticleOrder = ParticleOrder.OrderBy(i => Rand.Next()).ToList();

			foreach (int j in ParticleOrder)
			{
				int row = j / Chunks.ChunkSize + RowOffset;
				if (row < 0 || row >= Height)
				{
					continue;
				}
				int col = j % Chunks.ChunkSize + ColOffset;
				ElementList.Elements[Cells[row * Width + col]].Process(this, row, col);
			}
		}
		
		// GD.Print("THREADPROCESS - Queued task completed at " + chunkRow + " in thread " + Thread.CurrentThread.ManagedThreadId);
	}


	// A count of non-empty cells currently in the simulation
	// Give a type of 0 to count all cells instead of just a given element type
	public int DebugParticleCount(int type)
	{
		int count = 0;

		for (int row = 0; row < Height; row++)
		{
			for (int col = 0; col < Width; col++)
			{
				if (type == 0)
				{
					if (Cells[row * Width + col] != 0 && Cells[row * Width + col] != 2)
					{
						count++;
					}
				}
				else if (Cells[row * Width + col] == type)
				{
					count++;
				}
			}
		}

		return count;
	}

	public float Randf()
	{
		return (float)Rand.NextDouble();
	}

	public bool InBounds(int row, int col)
	{
		return row >= 0 && col >= 0 && row < Height && col < Width;
	}

	// Determine if the two elements could be swapped
	public bool IsSwappable(int row, int col, int row2, int col2)
	{
		if (row2 < 0) { // If the element wants to fly off the top of the screen, let it!
			return true;
		}
		if (!InBounds(row, col) || !InBounds(row2, col2))
		{
			return false;
		}
		if (ElementList.Elements[GetCell(row2, col2)].GetState == 0)
		{
			return false; // Do not allow a solid to be replaced
		}
		if (ElementList.Elements[GetCell(row, col)].GetState != 0 && ElementList.Elements[GetCell(row, col)].GetDensity <= ElementList.Elements[GetCell(row2, col2)].GetDensity)
		{
			return false; // If the replacing element is not solid, and has a density less than or equal to what it is attempting to replace, do not replace
		}

		return true;
	}

	// Attempt to swap the elements at the two specified cells if the first cell has a higher density
	// By calling SetCell, this will awaken chunks
	public void MoveAndSwap(int row, int col, int row2, int col2)
	{
		if (row2 < 0) { // If the element is asking to be moved off the top of the screen, delete it!
			SetCell(row, col, 0);
		}
		if (!InBounds(row, col) || !InBounds(row2, col2) || (row == row2 && col == col2))
		{
			return; // Do not proceed if either coordinate is out of bounds, or they are exactly the same
		}
		if (GetCell(row, col) == 2 || GetCell(row2, col2) == 2)
		{
			return; // Do not move and swap, you've encountered or are trying to move a wall!
		}
		if (ElementList.Elements[GetCell(row, col)].GetDensity <= ElementList.Elements[GetCell(row2, col2)].GetDensity)
		{
			return; // The replacing cell has the same or lower density, do not replace!
		}
		int old = GetCell(row, col);
		SetCell(row, col, GetCell(row2, col2));
		SetCell(row2, col2, old);
	}


	public int GetCell(int row, int col)
	{
		return Cells[row * Width + col];
	}

	// CHUNKING METHODS

	private int GetChunkIndex(int row, int col)
	{
		if (!InBounds(row, col)) return 0;

		int index = row / Chunks.ChunkSize * Chunks.Width + col / Chunks.ChunkSize;
		if (index > Chunks.Width * Chunks.Height) return 0;

		return index;
	}

	public void DontSleepNextFrame(int row, int col)
	{
		if (Chunks.WakeTime[GetChunkIndex(row, col)] <= 1)
		{
			Chunks.WakeTime[GetChunkIndex(row, col)]++;
		}
	}

	private void AwakenChunk(int row, int col)
	{
		Chunks.ShouldAwaken[GetChunkIndex(row, col)] = true;
		int chunkRow = row % Chunks.ChunkSize;
		int chunkCol = col % Chunks.ChunkSize;

		// If we are awakening a chunk from a cell at any of the edges of a chunk
		// We must also awaken the adjacent chunks
		if (row > 0 && chunkRow == 0)
		{
			Chunks.ShouldAwaken[GetChunkIndex(row - 1, col)] = true;
		}
		if (row < Height - 1 && chunkRow == Chunks.ChunkSize - 1)
		{
			Chunks.ShouldAwaken[GetChunkIndex(row + 1, col)] = true;
		}
		if (col > 0 && chunkCol == 0)
		{
			Chunks.ShouldAwaken[GetChunkIndex(row, col - 1)] = true;
		}
		if (row < Width - 1 && chunkCol == Chunks.ChunkSize - 1)
		{
			Chunks.ShouldAwaken[GetChunkIndex(row, col + 1)] = true;
		}
	}

	public void SetCell(int row, int col, int type)
	{
		if (GetCell(row, col) == 2 && type != 0) // Only air can replace a wall, only called by erasing
		{
			return;
		}

		AwakenChunk(row, col);

		int oldType = GetCell(row, col);
		Cells[row * Width + col] = type;

		Chunks.Updated[GetChunkIndex(row, col)] = true;
		if (oldType == 0 && type != 0)
		{
			Chunks.CellCount[GetChunkIndex(row, col)]++;
		}
		else if (oldType != 0 && type == 0)
		{
			Chunks.CellCount[GetChunkIndex(row, col)]--;
		}
	}

	public void DrawCell(int row, int col, int type)
	{
		// GD.Print("Drawing new cell of type " + type + " at " + row + ", " + col);
		SetCell(row, col, type);
		
		// Tell the new cell to process for at least one frame
		Chunks.WakeTime[GetChunkIndex(row, col)] = 1;
	}

	public int GetWidth() { return Width; }

	public int GetHeight() { return Height; }

	public byte[] GetColorImage()
	{
		for (int row = 0; row < Height; row++)
		{
			for (int col = 0; col < Width; col++)
			{
				int idx = (row * Width + col) * 3;

				int type = Cells[row * Width + col];

				if (type == 0)
				{
					DrawData[idx] = 70;
					DrawData[idx + 1] = 70;
					DrawData[idx + 2] = 70;
					continue;
				}

				byte[] color = ElementList.Elements[type].GetColor;

				DrawData[idx] = color[0];
				DrawData[idx + 1] = color[1];
				DrawData[idx + 2] = color[2];
			}
		}

		return DrawData;
	}

	public void RepaintCanvas()
	{
		Texture = ImageTexture.CreateFromImage(Image.CreateFromData(Width, Height, false, Image.Format.Rgb8, GetColorImage()));
	}
}
