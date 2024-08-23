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
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
		Rope.GlobalPosition = Entity.GlobalPosition;
		HookRayCast.LookAt(GetGlobalMousePosition());
	}

	public void Hook()
	{
		if (Input.IsActionJustPressed("hook")) 
		{
			if (HookRayCast.IsColliding())
			{
				Cursor.Position = HookRayCast.GetCollisionPoint();
				if (Rope.HasMethod("_set_length")) {
					Rope.Call("_set_length", (Cursor.GlobalPosition - Entity.GlobalPosition).Length());
					Rope.GlobalRotationDegrees = HookRayCast.GlobalRotationDegrees - 90;
										
				}

			}
		}
	}
}
