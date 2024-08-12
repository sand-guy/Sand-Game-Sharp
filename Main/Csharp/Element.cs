using Godot;
using System;

abstract public partial class Element : RefCounted
{
	abstract public void Process(SandSimulation sim, int row, int col);

	// Returns three values in the array, [0] = r [1] = g [2] = b | 8-bit color depth
	abstract public byte[] GetColor();

	abstract public double GetDensity();

	// 0 if the element is solid, 1 if it is liquid, 2 if it is gaseous
	abstract public int GetState();

	public string GetName()
	{
		return this.GetType().Name;
	}
}

// ID = 0
public partial class Void : Element
{
	public Void()
	{
		
	}
	
	public override void Process(SandSimulation sim, int row, int col)
	{
		return;
	}

	public override byte[] GetColor()
	{
		byte r = 70;
		byte g = 70;
		byte b = 70;

		byte[] color = { r, g, b };

		return color;
	}

	public override double GetDensity()
	{
		return 0.0;
	}

	public override int GetState()
	{
		return 2;
	}
}

// ID = 1
public partial class Sand : Element
{
	// Constants which govern the non-universal physical properties of this element
	const double POWDER = 1 / 1.05;
	
	public override void Process(SandSimulation sim, int row, int col)
	{
		if (sim.Randf() >= POWDER) {
			return;
		}	

		bool downLeft = sim.IsSwappable(row, col, row + 1, col - 1);
		bool down = sim.IsSwappable(row, col, row + 1, col);
		bool downRight = sim.IsSwappable(row, col, row + 1, col + 1);

		if (down)
		{
			sim.MoveAndSwap(row, col, row + 1, col);
		} else if (downLeft && downRight)
		{
			sim.MoveAndSwap(row, col, row + 1, col + (sim.Randf() < 0.5 ? 1 : -1));
		} else if (downLeft)
		{
			sim.MoveAndSwap(row, col, row + 1, col - 1);
		} else if (downRight)
		{
			sim.MoveAndSwap(row, col, row + 1, col + 1);
		}
	}

	public override byte[] GetColor()
	{
		byte r = 244;
		byte g = 218;
		byte b = 128;

		byte[] color = { r, g, b };

		return color;
	}

	public override double GetDensity() {
		return 2.0;
	}

	public override int GetState()
	{
		return 0;
	}
}

// ID = 2
public partial class Wall : Element
{
	public override void Process(SandSimulation sim, int row, int col)
	{
		return;
	}

	public override byte[] GetColor()
	{
		byte r = 212;
		byte g = 212;
		byte b = 212;

		byte[] color = { r, g, b };

		return color;
	}

	public override double GetDensity() {
		return 1.0;
	}

	public override int GetState()
	{
		return 0;
	}
}

// ID = 3
public partial class Water : Element
{
	// Constants which govern the non-universal physical properties of this element
	const int FLUIDITY = 2;

	public override void Process(SandSimulation sim, int row, int col)
	{
		sim.LiquidProcess(row, col, FLUIDITY);
	}

	public override byte[] GetColor()
	{
		byte r = 66;
		byte g = 135;
		byte b = 245;

		byte[] color = { r, g, b };

		return color;
	}

	public override double GetDensity() {
		return 1.0;
	}

	public override int GetState()
	{
		return 1;
	}
}