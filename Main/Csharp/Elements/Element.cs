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
	public virtual bool GetStatic
	{
		get { return false; } 
	}

	// ABSTRACT PROPERTIES

	// Get the physical state of the element: solid, liquid or gas
	// 0 = SOLID
	// 1 = LIQUID
	// 2 = GAS
	public abstract int GetState
	{
		get;
	}

	// Density can be as low as zero, such as for air, and as high as an integer can store
	public abstract double GetDensity
	{
		get;
	}

	// Flammable things may burn, have fire spread to them, and be destroyed by it
	// True = flammable, let it burn!
	// False = nonflammable, will never burn!
	public abstract bool GetFlammable
	{
		get;
	}

	// RENDERING VARIABLES

	// Returns three values in the array, [0] = r [1] = g [2] = b | 8-bit color depth
	public abstract byte[] GetColor
	{
		get;
	}
}