using Godot;
using Godot.Collections;

[GlobalClass]
public partial class VelocityComponent: Node2D 
{
    [Export] CharacterBody2D Entity;
    [Export] public float JumpHeight;
	[Export] public float JumpTimeToPeak;
	[Export] public float JumpTimeToDescent;
    [Export] public int maxSpeed = 1600;	
    public float jumpVelocity;
	public float jumpGravity;
	public float fallGravity;
    [Export] public float acceleration = 300.0f;
    public float friction;
	[Export] public int jumpBufferTime = 15;
	public int jumpBufferCounter = 0;
    [Export] public float onGroundFriction = 0.1f;
	[Export] public float onAirFriction = 0.1f;
    public Vector2 normal = new Vector2(0, 0);

    public Vector2 tempVelocity = new Vector2(0, 0);

    public void HandleVelocity(double delta) 
    {
        normal = Entity.GetNode<RayCast2D>("RayCastFloor").GetCollisionNormal();
        ApplyGravity(delta);
        MoveHorizontal();   
        Jump();

    }

    
    public void ActivateMove()
    {
        Entity.MoveAndSlide();
    }

    public void CalculateJumpPhysics()
	{
		jumpVelocity = ((2.0f * JumpHeight) / JumpTimeToPeak) * -1;
		jumpGravity = -((2.0f * JumpHeight) / Mathf.Pow(JumpTimeToPeak, 2)) * -1;
		fallGravity = -((2.0f * JumpHeight) / Mathf.Pow(JumpTimeToDescent, 2)) * -1;
	}

    private void Jump()
	{
        CalculateJumpPhysics();
		if (Input.IsActionJustPressed("jump") && Entity.IsOnFloor())
		{
			jumpBufferCounter = jumpBufferTime;
		}
		if (jumpBufferCounter > 0)
		{
			jumpBufferCounter--;
		}
		if (jumpBufferCounter > 0)
		{
			tempVelocity.Y = jumpVelocity;
			Entity.Velocity = tempVelocity;
			jumpBufferCounter = 0;
		}
		if (Input.IsActionJustReleased("jump") && Entity.Velocity.Y < 0)
		{
			tempVelocity.Y *= 0.5f;
			Entity.Velocity = tempVelocity;
		}
	}

    public void CalculateSpeed(Array<float> addModifiers, Array<float> multiplyModifiers)
    {
     

        for (int i = 0; i < addModifiers.Count; i++)
        {
            acceleration += addModifiers[i];
        }

        for (int i = 0; i < multiplyModifiers.Count; i++)
        {
            acceleration *= multiplyModifiers[i];
        }
    }

    private void MoveHorizontal()
	{
		friction = Entity.IsOnFloor() ? onGroundFriction : onAirFriction;

		tempVelocity = Entity.Velocity;


		if (Input.IsActionPressed("move_right") && !IsHooked())
		{
			ApplyMovement(friction, acceleration);
		}
		else if (Input.IsActionPressed("move_left") && !IsHooked())
		{
			ApplyMovement(friction, -acceleration);
		}
		else if (!Input.IsActionPressed("move_left") && !Input.IsActionPressed("move_right"))
		{
			if (!IsHooked())
			{
                tempVelocity = Entity.Velocity;
				tempVelocity.X = 0;
				Entity.Velocity = tempVelocity;
			}
		}
		
	}

	public void ApplyMovement(float friction, float acceleration)
	{
        
        tempVelocity = Entity.Velocity;
        tempVelocity.X = Mathf.Lerp(tempVelocity.X, acceleration, friction);
        Entity.Velocity = tempVelocity;
        tempVelocity.X = Entity.Velocity.X * (normal.X + 0.9f);
        tempVelocity.X = Mathf.Clamp(Entity.Velocity.X, -maxSpeed, maxSpeed);
        Entity.Velocity = tempVelocity;
    }



    public void ApplyGravity(double delta)
    {
        if (!Entity.IsOnFloor())
        {   tempVelocity = Entity.Velocity;
            tempVelocity.Y += ReturnGravity() * (float)delta;
            tempVelocity.Y = Mathf.Clamp(tempVelocity.Y, -maxSpeed, maxSpeed);
            Entity.Velocity = tempVelocity;
        }
        else if (Entity.Velocity.Y > 0)
        {
            tempVelocity = Entity.Velocity;
            tempVelocity.Y = 0;
            Entity.Velocity = tempVelocity;
        }
    }

    private float ReturnGravity()
	{
		return Entity.Velocity.Y < 0.0 ? jumpGravity : fallGravity;
	}

    private bool IsHooked()
	{
        if (Owner.HasNode("Chain") || Owner.HasNode("Chain2"))
        {
            return Owner.GetNode<ChainComponent>("Chain").hooked || Owner.GetNode<ChainComponent>("Chain2").hooked;
        }
        else
        {
            return false;
        }
	}


}