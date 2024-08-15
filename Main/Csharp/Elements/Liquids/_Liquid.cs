using Godot;
using System;

public abstract partial class Liquid : Element
{
	public override int State
	{
		get { return 1; }
	}
}