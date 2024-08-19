using Godot;
using System;

[GlobalClass]
public partial class HookChain : Line2D
{
	[Export] public Vector2 StartPos = new Vector2(0, 0);
	[Export] public Vector2 EndPos = new Vector2(0, 0);
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
		if (Visible)
		{
			AddPoint(StartPos);
			AddPoint(EndPos, 1);
			RemovePoint(0);
		}
		else {
			ClearPoints();
		}
	}
}
