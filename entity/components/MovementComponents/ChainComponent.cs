using Godot;
using System;

[GlobalClass]
public partial class ChainComponent : Node2D
{
	[Export] public int ChainLenght = 800;
	[Export] public int SPEED = 150;
	[Export] public int chainPullForce = 60;
	[Export] public string HookInput = "hook";
	[Export] public CharacterBody2D Entity;
	[Export] public VelocityComponent velocityComponent;
	[Export] public DampedSpringJoint2D joint2D;
	public Vector2 direction = new Vector2(0, 0);
	public Vector2 tip = new Vector2(0, 0);
	
	public bool flying = false;
	public bool hooked = false;
	private Vector2 chainVelocity = new Vector2(0, 0);

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
		
		if (tip_loc.DistanceTo(GetNode<Line2D>("Line2D").GetPointPosition(1)) > ChainLenght)
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

	public void Hook()
	{
		HandleHook(HookInput);
	}

	private void HandleHook(string hookAction)
	{
		if (Input.IsActionJustPressed(hookAction) && !hooked && !flying)
		{
			var mouseViewportPos = GetViewport().GetMousePosition();
			shoot((mouseViewportPos - GetViewportRect().Size / 2).Normalized());
		}
		else if (Input.IsActionJustPressed(hookAction) || (Input.IsActionJustPressed("jump") && hooked))
		{
			Release();
			Entity.Velocity *= 1.2f;
		}
	}

	public void HookPhys()
	{
		HandleHookPhysics(ref chainVelocity);
		
	}

	private void HandleHookPhysics(ref Vector2 chainVelocity)
	{
		
		if (hooked)
		{	// gabriel guerra eu juro que vou me matar
			float distanceToHook = Entity.GlobalPosition.DistanceTo(tip);
            float pullForce = SPEED * Mathf.Clamp(distanceToHook / ChainLenght, 1.0f, chainPullForce);
            
            chainVelocity = ToLocal(tip).Normalized() * pullForce;
            chainVelocity.Y *= chainVelocity.Y > 0 ? 0.55f : 1.1f;
            if (Mathf.Sign(chainVelocity.X) != Mathf.Sign(velocityComponent.acceleration))
            {
                chainVelocity.X *= 0.3f;
            }
        }
        else
        {
            chainVelocity = Vector2.Zero;
        }
        
        Entity.Velocity += chainVelocity;
		
	}

}
