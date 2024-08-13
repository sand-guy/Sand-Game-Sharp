using Godot;
using System;

public class CellData
{
	private int simulationWidth;
	private int simulationHeight;

	private int[] cellType;
	private int[] cellTemp;

	public CellData(int newWidth, int newHeight)
	{
		simulationWidth = newWidth;
		simulationHeight = newHeight;

		Array.Resize(ref cellType, simulationWidth * simulationHeight);
		Array.Resize(ref cellTemp, simulationWidth * simulationHeight);
	}

	private int getCellIndex(int row, int col)
	{ 
		return row * simulationWidth + col;
	}

	public int getType(int row, int col)
	{
		return cellType[getCellIndex(row, col)];
	}

	public void setType(int row, int col, int type)
	{
		cellType[getCellIndex(row, col)] = type;
	}


}
