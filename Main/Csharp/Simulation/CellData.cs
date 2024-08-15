using Godot;
using System;


public struct CellData
{
	float colorOffset;

	int type;

	public CellData(SandSimulation sim)
	{
		type = 0;
		colorOffset = sim.Randf();
	}

	public CellData(SandSimulation sim, int newType)
	{
		type = newType;
		colorOffset = sim.Randf();
	}

	public float ColorOffset
	{
		get { return colorOffset; }
	}

	public int Type
	{
		get { return type; }

		set { type = value; }
	}
}
