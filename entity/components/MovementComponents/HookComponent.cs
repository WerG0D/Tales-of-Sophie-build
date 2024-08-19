using Godot;
using System;

[GlobalClass]
public partial class HookComponent : Node2D
{
	[Export] public float Tension = 0.5f;
	[Export] public CharacterBody2D Entity;
	[Export] public DampedSpringJoint2D Hook;
	[Export] public RayCast2D HookRayCast;
	public HookChain HookChainLine;
	[Export] StaticBody2D Cursor;

	public override void _Ready()
	{
		HookChainLine = GetNode<HookChain>("HookChain");
		
	}

	public void HandleHook() 
	{
		Hook.GlobalPosition = Entity.GlobalPosition;
		HookRayCast.LookAt(GetGlobalMousePosition());

		if (Input.IsActionJustPressed("hook"))
		{
			if (HookRayCast.IsColliding())
			{
				Cursor.Position = HookRayCast.GetCollisionPoint();
				var DistanceLenght = Entity.Position.DistanceTo(Cursor.Position);
				Hook.Length = DistanceLenght;
				Hook.GlobalRotationDegrees = HookRayCast.GlobalRotationDegrees - 90;
				Hook.RestLength = DistanceLenght * Tension;
				HookChainLine.StartPos = Cursor.GlobalPosition;
				Hook.NodeB = Cursor.GetPath();
				

			}
		}

		if (Input.IsActionPressed("hook"))
		{
			if (Hook.NodeA != Hook.NodeB)
			{
				HookChainLine.Visible = true;
				HookChainLine.EndPos = Entity.GlobalPosition;
			}
		}

		else {
			HookChainLine.Visible = false;
			Hook.NodeB = Hook.NodeA;
		}
	}
}
