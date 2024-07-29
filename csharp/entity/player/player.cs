using Godot;
using System;
using System.ComponentModel;

public partial class player : CharacterBody2D
{
	// ####################### ONREADY VAR #######################
	public Camera2D camera;
	public  HealthComponent healthcomphead;
    public  HealthComponent healthcompbody;
    public  HealthComponent healthcompRightArm;
    public  HealthComponent healthcompLeftArm;
    public  HealthComponent healthcompRightLeg;
    public  HealthComponent healthcompLeftLeg;
    public  AnimationPlayer animplayer;
    public  bool initialized = false;

	// ####################### ONREADY VAR #######################

	// ####################### VARS #########################
	
	public int max_speed = 1600;
	public int jump_force = 500;
	public int acceleration = 300;
	public int jump_buffer_time = 15;
	public int jump_buffer_counter = 0;
	public float dash_duration = 0.2f;
	public int walljmp_timer = 0;
	public int walljmp_cd = 20;
	public bool is_debug = false;
	public Vector2 chain_velocity = new Vector2(0, 0);
	public Vector2 chain2_velocity = new Vector2(0, 0);
	public Vector2 tempVelocity = new Vector2(0, 0);
	public int chain_pull_force = 60;
	public float on_ground_friction = 0.01f;
	public float on_air_friction = 0.002f;
	public float unsigned_speed = 0.0f;
	public float gravityfactor = 0.02f;
	public Vector2 normal = new Vector2(0, 0);
	public bool is_dead = false;
	public bool is_taking_damage = false;
	public bool is_attacking = false;
	public bool is_dash = false;
	public bool is_walljmp = false;
	public bool is_head_dismembered = false;
	public bool is_RARM_dismembered = false;
	public bool is_LARM_dismembered = false;
	public bool is_RLEG_dismembered = false;
	public bool is_LLEG_dismembered = false;
	public bool is_gravity = true;
	public bool is_input = true; 
	
	// ####################### VARS #########################

	
	public override void _Ready()
	{
		camera = GetNode<Camera2D>("Camera2D");
		healthcomphead = GetNode<HealthComponent>("Sprite2D/HurtboxHead/HealthComponentHead");
		healthcompbody = GetNode<HealthComponent>("Sprite2D/HurtboxBody/HealthComponentBody");
		healthcompRightArm = GetNode<HealthComponent>("Sprite2D/HurtboxRarm/HealthComponentRightArm");
		healthcompLeftArm = GetNode<HealthComponent>("Sprite2D/HurtboxLarm/HealthComponentLeftArm");
		healthcompRightLeg = GetNode<HealthComponent>("Sprite2D/HurtboxRLeg/HealthComponentRightLeg");
		healthcompLeftLeg = GetNode<HealthComponent>("Sprite2D/HurtboxLLeg/HealthComponentLeftLeg");
		animplayer = GetNode<AnimationPlayer>("AnimationPlayer");
		initialized = true;
		
	}


	public override void _Process(double delta)
	{
		Debug();
		MoveAndSlide();
	}

	
	public void MovePlayer(double delta) 
	{
		unsigned_speed = Velocity.X < 0 ? Velocity.X * -1 : Velocity.X;
		normal = GetNode<RayCast2D>("RayCastFloor").GetCollisionNormal();
		ApplyGravity();
		MoveRL();
		Jump();
		Walljmp();
	}
	public void MoveRL()
	{
		var friction = IsOnFloor() ? on_ground_friction : on_air_friction;
		tempVelocity = Velocity;
		
		if (is_dash) friction *= 4;
		if (Input.IsActionPressed("move_right") && GetNode<Chain>("Chain").hooked == false && GetNode<Chain>("Chain2").hooked == false && is_input) 
		{
			if (!(Velocity.X >= -acceleration && Velocity.X <= acceleration)) 
			{
				
				tempVelocity.X = Mathf.Lerp(tempVelocity.X, (float)acceleration, friction);
				Velocity = tempVelocity;
					
			}
			else 
			{
				tempVelocity.X = Mathf.Lerp(tempVelocity.X, (float)acceleration, 1);
				Velocity = tempVelocity;
			}
			tempVelocity.X = Velocity.X * (normal.X + 0.9f);
			Velocity = tempVelocity;

		}

		if (Input.IsActionPressed("move_left") && GetNode<Chain>("Chain").hooked == false && GetNode<Chain>("Chain2").hooked == false && is_input)
		{
			if (!(Velocity.X >= -acceleration && Velocity.X <= acceleration)) 
			{
				tempVelocity.X = Mathf.Lerp(tempVelocity.X, (float)-acceleration, friction);
				Velocity = tempVelocity;
			}
			else 
			{
				tempVelocity.X = Mathf.Lerp(tempVelocity.X, (float)-acceleration, 1);
				Velocity = tempVelocity;
			}
			tempVelocity.X = Velocity.X * (normal.X + 0.9f);
			Velocity = tempVelocity;
			
		}

		if (!Input.IsActionPressed("move_left") && !Input.IsActionPressed("move_right") || Input.IsActionJustPressed("move_right") && Input.IsActionJustPressed("move_left")) 
		{
			if (!(GetNode<Chain>("Chain").hooked == true && GetNode<Chain>("Chain2").hooked == true)) 
			{
				tempVelocity.X = 0;
				Velocity = tempVelocity;
			}
		}
		tempVelocity.X = Mathf.Clamp(Velocity.X, -max_speed, max_speed);
		Velocity = tempVelocity;

	}
	public void Jump() 
	{
		if (Input.IsActionJustPressed("jump") && IsOnFloor() && is_input) 
		{
			jump_buffer_counter = jump_buffer_time;
		}
		if (jump_buffer_counter > 0) 
		{
			jump_buffer_counter--;
		}
		if (jump_buffer_counter > 0) 
		{
			Velocity +=  GetFloorNormal() * jump_force;
			jump_buffer_counter = 0;
		}
		if (Input.IsActionJustReleased("jump")) 
		{
			if (Velocity.Y < 0) 
			{
				tempVelocity = Velocity;
				tempVelocity.Y = Velocity.Y * 0.2f;
				Velocity = tempVelocity;
			}
		}



	}
	public void Walljmp()
	{
		if (Velocity.Y >= 10 && walljmp_timer < walljmp_cd && IsOnWallOnly()) 
		{
			walljmp_timer += 1;
			is_walljmp = true;
		}
		else 
		{
			walljmp_timer = 0;
			is_walljmp = false;
		}
	}
	public void Dash()
	{
		var timer = GetNode<Timer>("DashTimer");
		if (Input.IsActionJustPressed("dash") && timer.IsStopped()) 
		{
			if (timer.IsStopped()) 
			{
				StartTimer(timer, dash_duration);
				GD.Print("Timer");
			}
		}
	}
	public void StartTimer(Timer timer, float duration)
	{
		timer.WaitTime = duration;
		timer.OneShot = true;
		timer.Start();
	}
	public void ApplyGravity()
	{
		if (!IsOnFloor() && is_gravity && !is_walljmp) 
		{
			Vector2 tempVelocity = Velocity;
			tempVelocity.Y += gravityfactor;
			Velocity = tempVelocity;
		}
	}
	public void Debug()
	 
	{
		RichTextLabel richTextLabel = GetNode<RichTextLabel>("RichTextLabel");
		if (Input.IsActionJustPressed("debug")) 
		{
			is_debug = !is_debug;
			GD.Print("Debug mode: " + is_debug);
		}
		if (is_debug) 
		{
			richTextLabel.Text = "Velocity: " + Velocity + 
			"\nNormal: " + normal + 
			"\nIs Dead: " + is_dead + 
			"\nIs Taking Damage: " + is_taking_damage + 
			"\nIs Attacking: " + is_attacking + 
			"\nIs Dashing: " + is_dash + 
			"\nIs Wall Jumping: " + is_walljmp + 
			"\nIs Head Dismembered: " + is_head_dismembered + 
			"\nIs Right Arm Dismembered: " + is_RARM_dismembered + 
			"\nIs Left Arm Dismembered: " + is_LARM_dismembered + 
			"\nIs Right Leg Dismembered: " + is_RLEG_dismembered + 
			"\nIs Left Leg Dismembered: " + is_LLEG_dismembered + 
			"\nIs Gravity: " + is_gravity + 
			"\nIs Input: " + is_input;
		}
		else {richTextLabel.Text = "";}
		if (is_debug && Input.IsActionPressed("ctrl")) 
		{
			if (Input.IsActionPressed("scroll_up"))
			{
				camera.Zoom *= new Vector2(1.2f, 1.2f);
				GD.Print("Zoom: " + camera.Zoom);
			}

			if (Input.IsActionPressed("scroll_down"))
			{
				camera.Zoom /= new Vector2(1.2f, 1.2f);
				GD.Print("Zoom: " + camera.Zoom);
			}
		}

	}
	public void Player() {}
}
