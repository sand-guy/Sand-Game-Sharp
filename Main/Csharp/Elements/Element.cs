using Godot;
using System;

// Element is inherited by the four base classes of elements: solids, powders, liquids and gasses
// Getter methods are used to access constant information about a type of element
// Abstract properties must be defined by inheriting base classes or elements
abstract public partial class Element : RefCounted
{
	// UNIVERSAL TO ALL ELEMENTS
	
	// Gets the name of the element type
	// Expect a return like "Air" or "Sand"
	public string GetName
	{
		get { return this.GetType().Name; }
	}

	// PHYSICS PROCESS

	// A process function to be called each frame that the element is considered active by the simulation
	// This acts on the state data of a given element via SandSimulation, but elements DO NOT contain state data
	// State data is stored in a separate type of object, so only one copy of each element exists and is referenced
	public abstract void Process(SandSimulation sim, int row, int col);

	// PHYSICS PROPERTIES

	// VIRTUAL PROPERTIES

	// Things that are static cannot be replaced, for any reason
	// Only things that inherit Static will return true for this property
	// No need to touch this, unless you wanna make something static temporarily
	public virtual bool IsStatic
	{
		get { return false; } 
	}

	// Whether or not this block is burning, and thus if it can turn nearby blocks into fire
	public virtual bool Burning
	{
		get { return false; }
	}

	// How flammable this block is 

	// ABSTRACT PROPERTIES - NO DEFAULT, SET PER ELEMENT

	// Get the physical state of the element: solid, liquid or gas
	// 0 = SOLID
	// 1 = LIQUID
	// 2 = GAS
	public abstract int State
	{
		get;
	}

	// Density can be as low as zero, such as for air, and as high as an integer can store
	public abstract double Density
	{
		get;
	}

	// RENDERING VARIABLES

	public abstract byte[] A_Color { get; }
	public abstract byte[] B_Color { get; }

	byte[][] ColorOffsetPregen;

	// Returns the A color as a fallback
	// Three values in the array, [0] = r [1] = g [2] = b
	// 8-bit color depth
	public byte[] Color
	{
		get { return A_Color; }
	}

	public void GenerateOffsets()
	{
		ColorOffsetPregen = new byte[256][];

		for (int i = 0; i <= 255; i++)
		{
			byte[] newOffsetPregen = new byte[3];
			float colorOffset = (float)i / 255f;
			newOffsetPregen[0] = (byte) (A_Color[0] + (B_Color[0] - A_Color[0]) * colorOffset);
			newOffsetPregen[1] = (byte) (A_Color[1] + (B_Color[1] - A_Color[1]) * colorOffset);
			newOffsetPregen[2] = (byte) (A_Color[2] + (B_Color[2] - A_Color[2]) * colorOffset);

			ColorOffsetPregen[i] = newOffsetPregen;
		}
	}

	public byte[] LerpColor(float colorOffset)
	{
		int offsetIndex = (int)(colorOffset * 255);
		return ColorOffsetPregen[offsetIndex];
	}
}