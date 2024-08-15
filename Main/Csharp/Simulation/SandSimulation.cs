using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[GlobalClass]
public partial class SandSimulation : RefCounted
{
	// Background color, override for air (ID = 0) to speed up rendering
	const byte BG_VAL_R = 70;
	const byte BG_VAL_G = 70;
	const byte BG_VAL_B = 70;

	// Shared random instance
	Random Rand;

	// Simulation dimensions
	private int Width = 400;
	private int Height = 240;

	// Contains the identity of cells on the grid.
	// Mutable information: Type, what cell is contained within that CellData
	// Immutable information: ColorOffset, generated once on instantiation
	CellData[] Cells;

	// Drawing data to be used to generate an image once per frame
	byte[] DrawData;

	// The chunk map, used for partitioning rendering tasks between threads
	ChunkMap ChunkMap;

	// MULTITHREADING VARIABLES

	// The random "row offset" (vertical shift) that will be applied each frame to the chunk processing to avoid artifacts along chunk borders
	int GlobalRowOffset;
	// The maximum row offset to use in a given frame, positive or negative
	// TODO - Find out why this causes stuck particles if set too high (eg 12)!
	//        This may indicate not all cells are being rendered every frame as intended!
	// TODO - Test out chunk rendering that does not rely on rows, dirty rects may play into this
	int MaxRowOffset = 2;

	
	int FrameCount = 0;

	public SandSimulation()
	{
		ElementList.FillElementList();

		Rand = new Random();

		ChunkMap = new ChunkMap(Width, Height);

		Array.Resize(ref DrawData, Width * Height * 3); // Must be 3x simulation size to store one byte per R/G/B channel

		Array.Resize(ref Cells, Width * Height);
		Array.Fill(Cells, new CellData(this)); // Populate with CellData, this pregenerates color offsets

		GetColorImage(); // Draw the first frame
	}

	// Advances the simulation, call this from Main and then call GetColorImage
	public void Step()
	{
		// Fill the chunk arrays to awaken and redraw ChunkMap with false, they get populated during a frame
		Array.Fill<bool>(ChunkMap.ShouldAwaken, false);
		Array.Fill<bool>(ChunkMap.Updated, false);

		GlobalRowOffset = Rand.Next(0, MaxRowOffset + 1) * (Randf() < 0.5 ? -1 : 1); // Generate a random row offset (-MaxRowOffset to MaxRowOffset) to apply to threaded chunk processing

		// Simulate the grid by giving threads a row of chunks to process
		// Process all even rows, then all odd rows to prevent overlapping attempts at cell access

		Parallel.ForEach(ChunkMap.EvenRows, chunkRow => ThreadProcess(chunkRow));
		Parallel.ForEach(ChunkMap.OddRows, chunkRow => ThreadProcess(chunkRow));

		// Debug, nonthreaded foreach statements to iterate over the rows sequentially
		//foreach (int chunkRow in ChunkMap.EvenRows) { ThreadProcess(chunkRow); }
		//foreach (int chunkRow in ChunkMap.OddRows) { ThreadProcess(chunkRow); }

		for (int i = 0; i < ChunkMap.WakeTime.Length; i++) // Iterate through all of the chunks
		{
			if (ChunkMap.ShouldAwaken[i]) // If the chunk should be awakened...
			{
				ChunkMap.WakeTime[i] = 2; // ...set it to process for an extra two frames 
			}
			else
			{
				ChunkMap.WakeTime[i] = (byte)Math.Max(0, ChunkMap.WakeTime[i] - 1); // Otherwise subtract one from the number of extra frames left to process or keep it at zero
			}
		}

		FrameCount++;
	}

	private void ThreadProcess(int chunkRow)
	{
		List<int> ParticleOrder = new List<int>();
		for (int i = 0; i < ChunkMap.ChunkSize * ChunkMap.ChunkSize; i++)
		{
			ParticleOrder.Add(i);
		}

		for (int i = chunkRow * ChunkMap.Width; i < (chunkRow + 1) * ChunkMap.Width; i++)
		{
			if (ChunkMap.CellCount[i] == 0 || (ChunkMap.WakeTime[i] == 0 && Randf() > ChunkMap.AwakenChance))
			{
				continue;
			}

			int RowOffset = i / ChunkMap.Width * ChunkMap.ChunkSize + GlobalRowOffset;
			int ColOffset = i % ChunkMap.Width * ChunkMap.ChunkSize;
			// Shuffle the particle order to avoid artifacts
			ParticleOrder = ParticleOrder.OrderBy(i => Rand.Next()).ToList();

			foreach (int j in ParticleOrder)
			{
				int row = j / ChunkMap.ChunkSize + RowOffset;
				if (row < 0 || row >= Height)
				{
					continue;
				}
				int col = j % ChunkMap.ChunkSize + ColOffset;

				ElementList.Elements[Cells[row * Width + col].Type].Process(this, row, col);
			}
		}
	}

	// A count of non-empty cells currently in the simulation, excluding walls
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
					if (Cells[row * Width + col].Type != 0 && Cells[row * Width + col].Type != 2)
					{
						count++;
					}
				}
				else if (Cells[row * Width + col].Type == type)
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

	// Returns the count of cells of "type" touching the cell at row, col in all eight adjacent cells
	public int TouchCount(int row, int col, int type)
	{
		int touching = 0;
		for (int rowOffset = -1; rowOffset <= 1; rowOffset++) {
			for (int colOffset = -1; colOffset <= 1; colOffset++) {
				if (!InBounds(row + rowOffset, col + colOffset)) {
					continue;
				}
				if (GetCell(row + rowOffset, col + colOffset).Type == type && (rowOffset != 0 && colOffset != 0)) {
					touching++;
				}
			}
		}
		return touching;
	}

	// Returns whether any cell of "type" are in the eight cells adjacent to the cell at row, col
	public bool IsTouching(int row, int col, int type)
	{
		for (int rowOffset = -1; rowOffset <= 1; rowOffset++) {
			for (int colOffset = -1; colOffset <= 1; colOffset++) {
				if (!InBounds(row + rowOffset, col + colOffset)) {
					continue;
				}
				if (GetCell(row + rowOffset, col + colOffset).Type == type && (rowOffset != 0 && colOffset != 0)) {
					return true;
				}
			}
		}
		return false;
	}

	// Determine if the two elements could be swapped
	public bool IsSwappable(int row, int col, int row2, int col2)
	{
		if (row2 < 0) { // If the element wants to move off the top of the screen, let it!
			return true;
		}
		if (!InBounds(row, col) || !InBounds(row2, col2))
		{
			return false;
		}
		if (ElementList.Elements[GetCell(row2, col2).Type].State == 0)
		{
			return false; // Do not allow a solid to be replaced
		}
		if (ElementList.Elements[GetCell(row, col).Type].State != 0 && ElementList.Elements[GetCell(row, col).Type].Density <= ElementList.Elements[GetCell(row2, col2).Type].Density)
		{
			return false; // If the replacing element is not solid, and has a density less than or equal to what it is attempting to replace, do not replace
		}

		return true;
	}

	// Attempt to swap the elements at the two specified cells if the first cell has a higher density
	// By calling SetCell, this will awaken chunks
	public void MoveAndSwap(int row, int col, int row2, int col2)
	{
		if (row2 < 0) { // If the element wants to move off the top of the screen, replace it with a fresh empty cell
			SetCell(row, col, new CellData());
		}
		if (!InBounds(row, col) || !InBounds(row2, col2) || (row == row2 && col == col2))
		{
			return; // Do not proceed if either coordinate is out of bounds, or they are exactly the same
		}
		if (GetCell(row, col).Type == 2 || GetCell(row2, col2).Type == 2)
		{
			return; // Do not move and swap, you've encountered or are trying to move a wall!
		}
		if (ElementList.Elements[GetCell(row, col).Type].Density <= ElementList.Elements[GetCell(row2, col2).Type].Density)
		{
			return; // The replacing cell has the same or lower density, do not replace!
		}
		CellData old = GetCell(row, col);
		SetCell(row, col, GetCell(row2, col2));
		SetCell(row2, col2, old);
	}

	// Accessor method for GDScript, it doesn't like structs I guess!
	public int GetCellType(int row, int col)
	{
		return Cells[row * Width + col].Type;
	}

	public CellData GetCell(int row, int col)
	{
		return Cells[row * Width + col];
	}

	// CHUNKING METHODS

	private int GetChunkIndex(int row, int col)
	{
		if (!InBounds(row, col)) return 0;

		int index = row / ChunkMap.ChunkSize * ChunkMap.Width + col / ChunkMap.ChunkSize;
		if (index > ChunkMap.Width * ChunkMap.Height) return 0;

		return index;
	}

	public void DontSleepNextFrame(int row, int col)
	{
		if (ChunkMap.WakeTime[GetChunkIndex(row, col)] <= 1)
		{
			ChunkMap.WakeTime[GetChunkIndex(row, col)]++;
		}
	}

	// This is the unified method for waking up a chunk in the next frame
	// If you want to awaken a chunk, for example because you moved a cell within it:
	//      ALWAYS CALL THIS!
	private void AwakenChunk(int row, int col)
	{
		ChunkMap.ShouldAwaken[GetChunkIndex(row, col)] = true; // Wake up this chunk
		ChunkMap.Updated[GetChunkIndex(row, col)] = true; // Rerender this chunk this frame
		int chunkRow = row % ChunkMap.ChunkSize;
		int chunkCol = col % ChunkMap.ChunkSize;

		// If we are awakening a chunk from a cell at any of the edges of a chunk
		// We must also awaken the adjacent chunks
		if (row > 0 && chunkRow == 0)
		{
			ChunkMap.ShouldAwaken[GetChunkIndex(row - 1, col)] = true;
			ChunkMap.Updated[GetChunkIndex(row - 1, col)] = true;
		}
		if (row < Height - 1 && chunkRow == ChunkMap.ChunkSize - 1)
		{
			ChunkMap.ShouldAwaken[GetChunkIndex(row + 1, col)] = true;
			ChunkMap.Updated[GetChunkIndex(row + 1, col)] = true;
		}
		if (col > 0 && chunkCol == 0)
		{
			ChunkMap.ShouldAwaken[GetChunkIndex(row, col - 1)] = true;
			ChunkMap.Updated[GetChunkIndex(row, col - 1)] = true;
		}
		if (row < Width - 1 && chunkCol == ChunkMap.ChunkSize - 1)
		{
			ChunkMap.ShouldAwaken[GetChunkIndex(row, col + 1)] = true;
			ChunkMap.Updated[GetChunkIndex(row, col + 1)] = true;
		}
	}

	public void SetCell(int row, int col, CellData newCell)
	{
		if (GetCell(row, col).Type == 2 && newCell.Type != 0) // Walls can only be replaced by erasing them (placing air)
		{ 
			return;
		}

		AwakenChunk(row, col);
		
		ChunkMap.Updated[GetChunkIndex(row, col)] = true; // Set this chunk to rerender in the next frame, as it has had at least one cell update

		CellData oldCell = GetCell(row, col);
		Cells[row * Width + col] = newCell;

		if (oldCell.Type == 0 && newCell.Type != 0)
		{
			ChunkMap.CellCount[GetChunkIndex(row, col)]++;
		}
		else if (oldCell.Type != 0 && newCell.Type == 0)
		{
			ChunkMap.CellCount[GetChunkIndex(row, col)]--;
		}
	}

	public void SetCellType(int row, int col, int type)
	{
		if (GetCell(row, col).Type == 2 && type != 0) // Walls can only be replaced by erasing them (placing air)
		{ 
			return;
		}

		AwakenChunk(row, col);
		
		ChunkMap.Updated[GetChunkIndex(row, col)] = true; // Set this chunk to rerender in the next frame, as it has had at least one cell update

		int oldCellType = GetCell(row, col).Type;
		Cells[row * Width + col].Type = type;

		if (oldCellType == 0 && type != 0)
		{
			ChunkMap.CellCount[GetChunkIndex(row, col)]++;
		}
		else if (oldCellType != 0 && type == 0)
		{
			ChunkMap.CellCount[GetChunkIndex(row, col)]--;
		}
	}

	public void DrawCell(int row, int col, int type)
	{
		AwakenChunk(row, col); // Wake this chunk up for the next frame
		//SetCellType(row, col, type);
		SetCell(row, col, new CellData(this, type));
	}

	public int GetWidth() { return Width; }

	public int GetHeight() { return Height; }

	public byte[] GetColorImage()
	{
		for (int row = 0; row < Height; row++)
		{
			for (int col = 0; col < Width; col++)
			{
				if (!ChunkMap.Updated[GetChunkIndex(row, col)]) { // If the chunk was not updated this frame, leave the draw data as it was last frame
					continue;
				}
				int idx = (row * Width + col) * 3;

				int type = Cells[row * Width + col].Type;

				if (type == 0) // Override for air, use background color defined in the sim
				{
					DrawData[idx] = BG_VAL_R;
					DrawData[idx + 1] = BG_VAL_B;
					DrawData[idx + 2] = BG_VAL_G;
					continue;
				}

				byte[] color = ElementList.Elements[type].LerpColor(GetCell(row, col).ColorOffset);

				DrawData[idx] = color[0];
				DrawData[idx + 1] = color[1];
				DrawData[idx + 2] = color[2];
			}
		}

		return DrawData;
	}
}
