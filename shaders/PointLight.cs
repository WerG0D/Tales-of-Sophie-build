using Godot;
using System;

public partial class PointLight : PointLight2D
{
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
		Position = GetGlobalMousePosition();
	}
}
