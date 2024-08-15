using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class ElementList : RefCounted
{
	// We will make one instance of ElementList in the SandSimulation
	// Element IDs are governed by the index of each element instance in Elements, nothing else!
	// Element state data is stored separately from this list and the objects inheriting Element
	// TO DO: Element state data as array of object type ElementState

	// Contains instances of all the element types, once instantiated
	private static List<Element> elements;

	// Access point for the element list
	public static List<Element> Elements
	{
		get { return elements; }
	}

	// Instantiates the elements in the list
	public static void FillElementList()
	{
		elements = new List<Element>();
		elements.Capacity = 128;

		elements.Insert(0, new Air());
		elements.Insert(1, new Sand());
		elements.Insert(2, new Wall());
		elements.Insert(3, new Wood());
		elements.Insert(4, new Water());
		elements.Insert(5, new Fire());
		elements.Insert(6, new Smoke());

		for (int i = 1; i < elements.Count; i++) // Skip air, no need to generate offsets for it
		{
			elements[i].GenerateOffsets();
		}
	}

	// Accessor method for GDScript
	// They don't like C# properties and need a getter method 
	public static Element[] GetElementsArray()
	{
		return Elements.ToArray();
	}
}
