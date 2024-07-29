using Godot;
using System;

public partial class Chain : Node2D
{
	public int chain_len = 800;
	public Vector2 direction = new Vector2(0, 0);
	public Vector2 tip = new Vector2(0, 0);
	public const int SPEED = 150;
	public bool flying = false;
	public bool hooked = false;

	public override void _Ready()
	{
	}


	public override void _Process(double delta)
	{
		Visible = flying || hooked;
		if (!Visible) return;
		var tip_loc = ToLocal(tip);
		GetNode<RigidBody2D>("Tip").Rotation = Position.AngleToPoint(tip_loc) - Mathf.DegToRad(90);
		GetNode<Line2D>("Line2D").SetPointPosition(0, tip_loc);
		if (tip_loc.DistanceTo(GetNode<Line2D>("Line2D").GetPointPosition(1)) > chain_len)
		{
			Release();
		}

	}
	public override void _PhysicsProcess(double delta)
	{
		GetNode<RigidBody2D>("Tip").GlobalPosition = tip;
		if (flying)
		{
			if (GetNode<RigidBody2D>("Tip").MoveAndCollide(direction * SPEED) != null) 
			{
				hooked = true;
				flying = false;
			}
			tip = GetNode<RigidBody2D>("Tip").GlobalPosition;
		}
	}

		public void shoot(Vector2 dir)
	{
		direction = dir.Normalized();
		flying = true;
		tip =  GlobalPosition;
	}
	public void Release() 
	{
		flying = false;
		hooked = false;
	}

}
