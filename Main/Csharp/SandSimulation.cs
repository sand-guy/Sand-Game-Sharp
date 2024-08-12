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
	// The element list
	private List<Element> Elements { get; set; }

	public void FillElements()
	{
		Elements = new List<Element>();
		Elements.Capacity = 128;

		Elements.Insert(0, new Void());
		Elements.Insert(1, new Sand());
		Elements.Insert(2, new Wall());
		Elements.Insert(3, new Water());
	}

	// An external access point for the element list
	public Element[] GetElements()
	{
		return Elements.ToArray();
	}

	[Export]
	int InternalSeed = 1234;

	Random rand;

	// Simulation dimensions
	private int Width = 400;
	private int Height = 240;

	// Contains the identity of all cells on the grid
	int[] Cells;

	// Drawing data to be used to generate an image once per frame
	byte[] DrawData;

	// Dimension of each chunk
	int ChunkSize = 16;
	// The chance that a sleeping chunk can randomly reawaken for a frame
	float AwakenChance = 0.68f;

	// Simulation dimensions in chunks
	// Since resizing is not currently supported, this is calculated just once in the constructor of this class
	int ChunkWidth;
	int ChunkHeight;

	// Stores the number of non-empty cells within each chunk
	// Could be a byte, but this would limit the maximum chunk size
	int[] AliveCount;
	// Stores whether a chunk is awake or not, 0 is asleep and 1-2 is awake (will process an extra 1-2 frames until sleeping again)
	byte[] AwakeChunk;
	// Stores whether a chunk should be awakened on the next frame
	bool[] ShouldAwakenChunk;
	// Stores whether a given chunk has had visual updates within this frame
	// Will need to be implemented into drawing functions, holding onto draw data between frames (not just image)
	bool[] ChunkUpdated;

	// Variables for multithreading
	int GlobalRowOffset;

	int FrameCount = 0;


	public override void _Ready()
	{
		base._Ready();

		GetWindow().ContentScaleSize = new Vector2I(Width, Height);
	}

	public SandSimulation()
	{
		FillElements();

		rand = new Random(InternalSeed);

		ChunkWidth = Width / ChunkSize;
		ChunkHeight = Height / ChunkSize;
		// GD.Print("INITIALIZATION - Array in chunks is " + ChunkWidth + "x" + ChunkHeight);

		Array.Resize(ref DrawData, Width * Height * 3);

		Array.Resize(ref Cells, Width * Height);

		Array.Resize(ref AliveCount, ChunkWidth * ChunkHeight);
		Array.Resize(ref AwakeChunk, ChunkWidth * ChunkHeight);
		Array.Resize(ref ShouldAwakenChunk, ChunkWidth * ChunkHeight);
		Array.Resize(ref ChunkUpdated, ChunkWidth * ChunkHeight);

		Array.Fill<bool>(ChunkUpdated, true); // Process all chunks on the first frame
	}

	// Advances the simulation
	public override void _Process(double delta)
	{
		// GD.Print("_PROCESS - Frame called: " + FrameCount);

		List<int> evenChunkRows = new List<int>();
		List<int> oddChunkRows = new List<int>();

		for (int chunkRow = 0; chunkRow < ChunkHeight; chunkRow++)
		{
			if (chunkRow % 2 == 0)
			{
				evenChunkRows.Add(chunkRow);
			} else {
				oddChunkRows.Add(chunkRow);
			}
		}

		Array.Fill<bool>(ShouldAwakenChunk, false);
		GlobalRowOffset++;
		if (GlobalRowOffset > 1)
		{
			GlobalRowOffset = -1;
		}

		// Simulate the grid by giving threads a row of chunks to process
		// Process all even rows, then all odd rows to prevent overlapping attempts at cell access

		//GD.Print("_PROCESS - Processing even chunks");
		// Parallel.ForEach(evenChunkRows, chunkRow => ThreadProcess(chunkRow));
		foreach (int chunkRow in evenChunkRows) ThreadProcess(chunkRow);
		// GD.Print("_PROCESS - Even chunks complete");

		// GD.Print("_PROCESS - Processing odd chunks");
		// Parallel.ForEach(oddChunkRows, chunkRow => ThreadProcess(chunkRow));
		foreach (int chunkRow in oddChunkRows) ThreadProcess(chunkRow);
		// GD.Print("_PROCESS - Odd chunks complete");

		for (int i = 0; i < AwakeChunk.Length; i++) // Iterate through all of the chunks
		{
			if (ShouldAwakenChunk[i]) // If the chunk should be awakened...
			{
				AwakeChunk[i] = 2; // ...set it to process for an extra two frames 
			}
			else
			{
				AwakeChunk[i] = (byte)Math.Max(0, AwakeChunk[i] - 1); // Otherwise subtract one from the number of extra frames left to process or keep it at zero
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
		for (int i = 0; i < ChunkSize * ChunkSize; i++)
		{
			ParticleOrder.Add(i);
		}

		for (int i = chunkRow * ChunkWidth; i < (chunkRow + 1) * ChunkWidth; i++)
		{
			if (AliveCount[i] == 0 || AwakeChunk[i] == 0 && Randf() > AwakenChance)
			{
				continue;
			}

			int RowOffset = i / ChunkWidth * ChunkSize + GlobalRowOffset;
			int ColOffset = i % ChunkWidth * ChunkSize;
			// Shuffle the particle order to avoid artifacts
			ParticleOrder = ParticleOrder.OrderBy(i => rand.Next()).ToList();

			foreach (int j in ParticleOrder)
			{
				int row = j / ChunkSize + RowOffset;
				if (row < 0 || row >= Height)
				{
					continue;
				}
				int col = j % ChunkSize + ColOffset;
				Elements[Cells[row * Width + col]].Process(this, row, col);
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
		return (float)rand.NextDouble();
	}

	public bool InBounds(int row, int col)
	{
		return row >= 0 && col >= 0 && row < Height && col < Width;
	}

	// Determine if the two elements could be swapped
	public bool IsSwappable(int row, int col, int row2, int col2)
	{
		if (!InBounds(row, col) || !InBounds(row2, col2))
		{
			return false;
		}
		if (Elements[GetCell(row2, col2)].GetState() == 0)
		{
			return false; // Do not allow a solid to be replaced
		}
		if (Elements[GetCell(row, col)].GetState() != 0 && Elements[GetCell(row, col)].GetDensity() <= Elements[GetCell(row2, col2)].GetDensity())
		{
			return false; // If the replacing element is not solid, and has a density less than or equal to what it is attempting to replace, do not replace
		}

		return true;
	}

	// Attempt to swap the elements at the two specified cells if the first cell has a higher density
	public void MoveAndSwap(int row, int col, int row2, int col2)
	{
		if (!InBounds(row, col) || !InBounds(row2, col2))
		{
			return;
		}
		if (GetCell(row, col) == 2 || GetCell(row2, col2) == 2)
		{
			return; // Do not move and swap, you've encountered or are trying to move a wall!
		}
		if (Elements[GetCell(row, col)].GetDensity() <= Elements[GetCell(row2, col2)].GetDensity())
		{
			return; // The replacing cell has the same or lower density, do not replace!
		}
		int old = GetCell(row, col);
		SetCell(row, col, GetCell(row2, col2));
		SetCell(row2, col2, old);
	}

	public void LiquidProcess(int row, int col, int fluidity)
	{
		for (int i = 0; i < fluidity; i++)
		{
			double random = Randf();
			int new_col = col + (random < (0.5 + 1.0 / 32) ? 1 : -1); // A 50% chance to move left or right, with a 1/32 chance bias to go toward the right
			if (random < 1.0 / 32) // A 1/32 chance that the horizontal movement will be applied
			{
				new_col = col;
			}

			int new_row = row + (IsSwappable(row, col, row + 1, new_col) ? 1 : 0); // If it is allowed, attempt to move down
			if (IsSwappable(row, col, new_row, new_col) && (Randf() < 0.6 || !IsSwappable(row, col, new_row + 1, new_col))) // If the horizontal and/or vertical moves are allowed, 
			{                                                                                                               // there is a 60% chance to apply the movement
				MoveAndSwap(row, col, new_row, new_col);                                                                    // but it will always apply if no more vertical moves can be done at that horizontal location
				row = new_row;
				col = new_col;
			}
		}
	}

	public int GetCell(int row, int col)
	{
		return Cells[row * Width + col];
	}

	private int GetChunkIndex(int row, int col)
	{
		if (!InBounds(row, col)) return 0;

		int index = row / ChunkSize * ChunkWidth + col / ChunkSize;
		if (index > ChunkWidth * ChunkHeight) return 0;

		return index;
	}

	private void AwakenChunk(int row, int col)
	{
		ShouldAwakenChunk[GetChunkIndex(row, col)] = true;
		int chunkRow = row % ChunkSize;
		int chunkCol = col % ChunkSize;

		// If we are awakening a chunk from a pixel at any of the edges of a chunk
		// We must also awaken the adjacent chunks
		if (row > 0 && chunkRow == 0)
		{
			ShouldAwakenChunk[GetChunkIndex(row - 1, col)] = true;
		}
		if (row < Height - 1 && chunkRow == ChunkSize - 1)
		{
			ShouldAwakenChunk[GetChunkIndex(row + 1, col)] = true;
		}
		if (col > 0 && chunkCol == 0)
		{
			ShouldAwakenChunk[GetChunkIndex(row, col - 1)] = true;
		}
		if (row < Width - 1 && chunkCol == ChunkSize - 1)
		{
			ShouldAwakenChunk[GetChunkIndex(row, col + 1)] = true;
		}
	}

	public void SetCell(int row, int col, int type)
	{
		if (GetCell(row, col) == 2 && type != 0)
		{
			return;
		}

		AwakenChunk(row, col);

		int oldType = GetCell(row, col);
		Cells[row * Width + col] = type;

		ChunkUpdated[GetChunkIndex(row, col)] = true;
		if (oldType == 0 && type != 0)
		{
			AliveCount[GetChunkIndex(row, col)]++;
		}
		else if (oldType != 0 && type == 0)
		{
			AliveCount[GetChunkIndex(row, col)]--;
		}
	}

	public void DrawCell(int row, int col, int type)
	{
		// GD.Print("Drawing new cell of type " + type + " at " + row + ", " + col);
		SetCell(row, col, type);
		
		// Tell the new cell to process for at least one frame
		AwakeChunk[GetChunkIndex(row, col)] = 1;
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

				byte[] color = Elements[type].GetColor();

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
