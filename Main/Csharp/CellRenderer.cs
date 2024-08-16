using Godot;
using System;

[GlobalClass]
public partial class CellRenderer : RefCounted
{
	// Background color, override for air (ID = 0) to speed up rendering
	const byte BG_VAL_R = 70;
	const byte BG_VAL_G = 70;
	const byte BG_VAL_B = 70;

	// Drawing data to be used to generate an image once per frame
	private byte[] DrawData;

	public void FitToSim(int simWidth, int simHeight)
	{
		Array.Resize(ref DrawData, simWidth * simHeight * 3); // Must be 3x simulation size to store one byte per R/G/B channel
	}

	public byte[] GetColorImage(SandSimulation sim)
	{
		for (int row = 0; row < sim.GetHeight(); row++)
		{
			for (int col = 0; col < sim.GetWidth(); col++)
			{
				if (!sim.ChunkMap.WasUpdated(row, col)) { // If the chunk was not updated this frame, leave the draw data as it was last frame
					continue;
				}
				int idx = (row * sim.GetWidth() + col) * 3;

				int type = sim.GetCell(row, col).Type;

				if (type == 0) // Override for air, use background color defined in the sim
				{
					DrawData[idx] = BG_VAL_R;
					DrawData[idx + 1] = BG_VAL_B;
					DrawData[idx + 2] = BG_VAL_G;
					continue;
				}

				byte[] color = ElementList.Elements[type].LerpColor(sim.GetCell(row, col).ColorOffset);

				DrawData[idx] = color[0];
				DrawData[idx + 1] = color[1];
				DrawData[idx + 2] = color[2];
			}
		}

		return DrawData;
	}
}
