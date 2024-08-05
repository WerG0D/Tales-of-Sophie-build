// TODO 
// FAZER O ANIMPLAYER FUNCIONAR DE VERDADE
// FAZER O HOOK FUNCIONAR DE VERDADE
// FAZER O HOOKPHYS FUNCIONAR DE VERDADE
// MUDAR VELOCIDADE DE PULO E RUN ETC 
// OTIMIZAR MUDANDO A LINHA 

using Godot;
using System;

public partial class Player : CharacterBody2D
{
	// ####################### ONREADY VAR #######################
	public  Camera2D camera;
	public  HealthComponent healthcomphead;
    public  HealthComponent healthcompbody;
    public  HealthComponent healthcompRightArm;
    public  HealthComponent healthcompLeftArm;
    public  HealthComponent healthcompRightLeg;
    public  HealthComponent healthcompLeftLeg;
    public  AnimationPlayer animplayer;
    public  bool initialized = false;

	// ####################### ONREADY VAR #######################

	// ####################### EXPORT VAR ########################

	[Export] public float JumpHeight;
	[Export] public float JumpTimeToPeak;
	[Export] public float JumpTimeToDescent;





	// ####################### EXPORT VAR ########################

	// ####################### SIGNALS #########################

	[Signal] public delegate void DieEventHandler();
	[Signal] public delegate void DismemberEventHandler();

	// ####################### SIGNALS #########################
	
	// ####################### VARS #########################
	
	public int max_speed = 1600;
	public int jump_force = 500;
	public float JumpVelocity;
	public float JumpGravity;
	public float FallGravity;


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
		
		healthcomphead = GetNode<HealthComponent>("Sprite2D/HurtBoxHead/HealthComponentHead"); 
		healthcomphead.DismemberHead += DismemberBodyPart;
		healthcomphead.Damaged += Damage;
		healthcomphead.Death += Death;
		
		healthcompbody = GetNode<HealthComponent>("Sprite2D/HurtBoxBody/HealthComponentBody"); 
		healthcompbody.Damaged += Damage;
		healthcompbody.Death += Death;

		healthcompRightArm = GetNode<HealthComponent>("Sprite2D/HurtBoxRArm/HealthComponentRightArm");
		healthcompRightArm.DismemberRARM += DismemberBodyPart;
		healthcompRightArm.Damaged += Damage;
		healthcompRightArm.Death += Death;

		healthcompLeftArm = GetNode<HealthComponent>("Sprite2D/HurtBoxLArm/HealthComponentLeftArm"); 
		healthcompLeftArm.DismemberLARM += DismemberBodyPart;
		healthcompLeftArm.Damaged += Damage;
		healthcompLeftArm.Death += Death;	

		healthcompRightLeg = GetNode<HealthComponent>("Sprite2D/HurtBoxRLeg/HealthComponentRightLeg"); 
		healthcompRightLeg.DismemberRLEG += DismemberBodyPart;
		healthcompRightLeg.Damaged += Damage;
		healthcompRightLeg.Death += Death;

		healthcompLeftLeg = GetNode<HealthComponent>("Sprite2D/HurtBoxLleg/HealthComponentLeftLeg");
		healthcompLeftLeg.DismemberLLEG += DismemberBodyPart;
		healthcompLeftLeg.Damaged += Damage;
		healthcompLeftLeg.Death += Death;

		JumpVelocity = ((2.0f * JumpHeight) / JumpTimeToPeak) * -1;
		JumpGravity = -((2.0f * JumpHeight) / Mathf.Pow(JumpTimeToPeak, 2)) * -1;
		FallGravity = -((2.0f * JumpHeight) / Mathf.Pow(JumpTimeToDescent, 2)) * -1;

		animplayer = GetNode<AnimationPlayer>("AnimationPlayer");
		initialized = true;
		
	}


	public override void _Process(double delta){
		FloorSnapLength = 20.0f;
		Debug();
		MoveAndSlide();
		MovePlayer(delta);
		ApplyGravity(delta);
		Hook();
		HookPhys();
	}

	public Vector2 LerpVector2(Vector2 from, Vector2 to, float weight)
	{
    return new Vector2(
        Mathf.Lerp(from.X, to.X, weight),
        Mathf.Lerp(from.Y, to.Y, weight)
    );
	}


	public void MovePlayer(double delta) 
	{
		unsigned_speed = Velocity.X < 0 ? Velocity.X * -1 : Velocity.X;
		normal = GetNode<RayCast2D>("RayCastFloor").GetCollisionNormal();
		MoveRL();
		Jump();
		Walljmp();
		Dash();
	}
	public void AnimatePlayer()
	{
		HandleHorizontalFlip();
		HandleSpriteRotation();
		PlayJumpOrFallAnimation();
		PlayMovementAnimation();
		PlayDashAnimation();
		PlayWalljmpAnimation();
		PlayHurtAnimation();
		PlayDeathAnimation();

	}
	// ####################### HANDLE MOVEMENT #########################
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
		var floor = GetFloorNormal();
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
			tempVelocity.Y = JumpVelocity;
			Velocity = tempVelocity;
			jump_buffer_counter = 0;
		}
		if (Input.IsActionJustReleased("jump") && Velocity.Y < 0) 
		{
			tempVelocity = Velocity;
			tempVelocity.Y = Velocity.Y * 0.5f;
			Velocity = tempVelocity;
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
		if (timer.IsStopped() == false) 
		{
			is_dash = true;
			is_gravity = false;
			is_input = false;
			var dash_speed = 600;

			if (GetNode<Sprite2D>("Sprite2D").FlipH == true) 
			{
				tempVelocity = Velocity;
				tempVelocity.X = -dash_speed;
				Velocity = tempVelocity;
			}
			else 
			{
				tempVelocity = Velocity;
				tempVelocity.X = dash_speed;
				Velocity = tempVelocity;
			}
			tempVelocity = Velocity;
			tempVelocity.Y = 0;
			Velocity = tempVelocity;
		}
		else 
		{
			is_dash = false;
			is_gravity = true;
			is_input = true;
		}
	}
	public void Hook()
	{
		if (Input.IsActionJustPressed("hook") && initialized && !GetNode<Chain>("Chain").hooked && !GetNode<Chain>("Chain").flying) 
		{
			var mouse_viewport_pos = GetViewport().GetMousePosition();
			GetNode<Chain>("Chain").shoot((mouse_viewport_pos - GetViewportRect().Size / 2).Normalized()); //Talvez GetViewportRect().Size * 0.5f
		}
		else if (Input.IsActionJustPressed("hook") || Input.IsActionJustPressed("jump") && GetNode<Chain>("Chain").hooked) 
		{
			GetNode<Chain>("Chain").Release();
			Velocity = Velocity * 1.2f;
		}
		if (Input.IsActionJustPressed("scroll_up")) 
		{
			chain_pull_force = chain_pull_force + 10;
		}
		if (Input.IsActionJustPressed("scroll_down")) 
		{
			chain_pull_force = chain_pull_force - 10;
		}
		if (Input.IsActionJustPressed("hook2") && initialized && !GetNode<Chain>("Chain2").hooked && !GetNode<Chain>("Chain2").flying) 
		{
			var mouse_viewport_pos = GetViewport().GetMousePosition();
			GetNode<Chain>("Chain2").shoot((mouse_viewport_pos - GetViewportRect().Size / 2).Normalized()); //Talvez GetViewportRect().Size * 0.5f
		}
		else if (Input.IsActionJustPressed("hook2") || Input.IsActionJustPressed("jump") && GetNode<Chain>("Chain2").hooked) 
		{
			GetNode<Chain>("Chain2").Release();
			Velocity = Velocity * 1.2f;
		}
	}
	public void HookPhys()
	{
		if (GetNode<Chain>("Chain").hooked) 
		{
			var walk = (Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left")) * acceleration;
			chain_velocity = ToLocal(GetNode<Chain>("Chain").tip).Normalized() * chain_pull_force;
			if (chain_velocity.Y > 0) 
			{
				chain_velocity.Y *= 0.55f;

			}
			else 
			{
				chain_velocity.Y *= 1.1f;
			}
			if (Mathf.Sign(chain_velocity.X) != Mathf.Sign(walk)) 
			{
				chain_velocity.X *= 0.3f;
			}
		}
		else 
		{
			chain_velocity = new Vector2(0, 0);
		}
		Velocity += chain_velocity;

		if (GetNode<Chain>("Chain2").hooked) 
		{
			var walk = (Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left")) * acceleration;
			chain2_velocity = ToLocal(GetNode<Chain>("Chain2").tip).Normalized() * chain_pull_force;
			if (chain2_velocity.Y > 0) 
			{
				chain2_velocity.Y *= 0.55f;

			}
			else 
			{
				chain2_velocity.Y *= 1.1f;
			}
			if (Mathf.Sign(chain2_velocity.X) != Mathf.Sign(walk)) 
			{
				chain2_velocity.X *= 0.3f;
			}
		}
		else 
		{
			chain2_velocity = new Vector2(0, 0);
		}
		Velocity += chain2_velocity;
	}
	
	// ####################### HANDLE COMBAT & DISMEMBERS #########################
	public async void Damage(float amount, Vector2 knockback)
	{
		is_taking_damage = true;
		await ToSignal(animplayer, AnimationPlayer.SignalName.AnimationFinished);
		is_taking_damage = false;
	}

	public async void Death()
	{
		if (is_dead) {return;}
		
		await ToSignal(animplayer, AnimationPlayer.SignalName.AnimationFinished);
		EmitSignal(nameof(DieEventHandler));
		is_dead = true;
		GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred("disabled", true);
		GetNode<CollisionShape2D>("CollisionShape2D2").SetDeferred("disabled", true);

	}

	public void DismemberBodyPart(string bodypart)
	{
		EmitSignal(nameof(DismemberEventHandler));
		
		if (bodypart == "HealthComponentHead") 
		{
			if (is_head_dismembered) {return;}
			is_head_dismembered = true;
			GD.Print("Head dismembered");
			GetNode<Sprite2D>("Sprite2D/Dismember Icon Head").Visible = false;
		}
		if (bodypart == "HealthComponentRightArm") 
		{
			if (is_RARM_dismembered) {return;}
			is_RARM_dismembered = true;
			GD.Print("Right Arm dismembered");
			GetNode<Sprite2D>("Sprite2D/Dismember Icon RightArm").Visible = false;
		}
		if (bodypart == "HealthComponentLeftArm") 
		{
			if (is_LARM_dismembered) {return;}
			is_LARM_dismembered = true;
			GD.Print("Left Arm dismembered");
			GetNode<Sprite2D>("Sprite2D/Dismember Icon LeftArm").Visible = false;
		}
		if (bodypart == "HealthComponentRightLeg") 
		{
			if (is_RLEG_dismembered) {return;}
			is_RLEG_dismembered = true;
			GD.Print("Right Leg dismembered");
			GetNode<Sprite2D>("Sprite2D/Dismember Icon RightLeg").Visible = false;
		}
		if (bodypart == "HealthComponentLeftLeg") 
		{
			if (is_LLEG_dismembered) {return;}
			is_LLEG_dismembered = true;
			GD.Print("Left Leg dismembered");
			GetNode<Sprite2D>("Sprite2D/Dismember Icon LeftLeg").Visible = false;
		}
	}

	public async void ApplyKnockback(Vector2 knockback, int frames = 10)
	{
		if (knockback.IsZeroApprox()) {return;}
		for (int i = 0; i < frames; i++) 
		{
			tempVelocity = Velocity;
			tempVelocity += knockback;
			Velocity = tempVelocity;
			await ToSignal(GetTree(), "physics_frame");
		}
		
	}

	// ####################### HANDLE ANIMATIONS #########################
	
	public void PlayJumpOrFallAnimation()
	{
		if (Velocity.Y < 1 && !IsOnFloor() && Input.IsActionJustPressed("jump") && !IsRestricted())  
		{
			animplayer.Play(!GetNode<Sprite2D>("Sprite2D").FlipH ? "jump" : "jump_left");
		}
		else if (Velocity.Y >= 0 && !IsOnFloor() && !IsRestricted())
		{
			animplayer.Play(!GetNode<Sprite2D>("Sprite2D").FlipH ? "fall" : "fall_left");
		}
	}
	public void PlayMovementAnimation()
	{
		if (IsOnFloor()) 
		{
			if (Velocity.X < 20  && Velocity.X > -20 && Velocity.Y <10 && !IsRestricted()) 
			{
				animplayer.Play(!GetNode<Sprite2D>("Sprite2D").FlipH ? "idle" : "idle_left");
			}
			else if (Velocity.X != 0 && Input.IsActionPressed("move_right") || Input.IsActionJustPressed("move_left") && !IsRestricted()) 
			{
				animplayer.Play(!GetNode<Sprite2D>("Sprite2D").FlipH ? "run" : "run_left");
				animplayer.SpeedScale = unsigned_speed / 200;
			}
		}
	}
	public void PlayDashAnimation()
	{
		if (is_dash) 
		{
			animplayer.Play(!GetNode<Sprite2D>("Sprite2D").FlipH ? "dash" : "dash_left");
		}
	}
	public void PlayWalljmpAnimation()
	{
		if (is_walljmp) 
		{
			animplayer.Play(!GetNode<Sprite2D>("Sprite2D").FlipH ? "walljmp" : "walljmp_left");
		}
	}
	public void PlayHurtAnimation()
	{
		if (is_taking_damage) 
		{
			animplayer.Play(!GetNode<Sprite2D>("Sprite2D").FlipH ? "hurt" : "hurt_left");
		}
	}
	public void PlayDeathAnimation()
	{
		if (is_dead) 
		{
			animplayer.Play(!GetNode<Sprite2D>("Sprite2D").FlipH ? "death" : "death_left");
		}
	}
	public void OnAnimationFinished(StringName anim_name)
	{
	}

	// ####################### UTILS #########################
	public void HandleHorizontalFlip() 
	{
		if (is_input) 
		{ 
			if (Input.IsActionPressed("move_right")) 
			{
				GetNode<Sprite2D>("Sprite2D").FlipH = false;
			}
			if (Input.IsActionPressed("move_left")) 
			{
				GetNode<Sprite2D>("Sprite2D").FlipH = true;
			}
		}
	}
	public void HandleSpriteRotation()
	{
		if (GetNode<RayCast2D>("RayCastFloor").IsColliding()) 
		{
			GetNode<Sprite2D>("Sprite2D").Rotation = normal.Angle() + Mathf.DegToRad(90);
		}
		else 
		{
			GetNode<Sprite2D>("Sprite2D").Rotation = Mathf.Lerp(GetNode<Sprite2D>("Sprite2D").Rotation, 0.0f, 0.08f);
		}
	}
	public bool IsRestricted()
	{
		return is_attacking || is_taking_damage || is_dash || is_walljmp;
	}
	public void StartTimer(Timer timer, float duration)
	{
		timer.WaitTime = duration;
		timer.OneShot = true;
		timer.Start();
	}
	public void ApplyGravity(double delta)
	{
		if (!IsOnFloor() && is_gravity && !is_walljmp) 
		{
			tempVelocity = Velocity;
			tempVelocity.Y += ReturnGravity() * (float)delta;
			tempVelocity.Y = Mathf.Clamp(tempVelocity.Y, -max_speed, max_speed);
			Velocity = tempVelocity;

			//velocity.y = lerp(velocity.y,  float(max_speed),0.02)
		    //velocity.y = clamp(velocity.y, -max_speed+100, max_speed+100)	#dallingspeed should be faster than walking
		}
		else if (Velocity.Y > 0){
			tempVelocity = Velocity;
			tempVelocity.Y = 0;
			Velocity = tempVelocity;
		}
	}

	public float ReturnGravity()
	{
		return Velocity.Y < 0.0 ? JumpGravity : FallGravity;
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
	public void player() {}
}
