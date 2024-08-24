using Godot;
using System;
using System.ComponentModel.Design;
using System.Transactions;

public partial class HookComponent : Node2D
{
	[Export] public Node2D Rope;
	[Export] public RayCast2D HookRayCast;
	[Export] public CharacterBody2D Entity;
	[Export] public StaticBody2D Cursor;
	[Export] public Node RopeInteractionEnd;
	[Export] public Node RopeInteractionBegin;
	[Export] public Line2D RopeLine;

	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
		//Rope.GlobalPosition = Entity.GlobalPosition;
		HookRayCast.Rotation = GetAngleTo(GetGlobalMousePosition());
	}

	public void Hook()
	{
		if (Input.IsActionJustPressed("hook"))
		{
			
			Rope.Call("_unregister_server()");
			Rope.Visible = false;

												
			
		}
	}
}
