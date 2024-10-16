using Godot;
using System;

public partial class Player : CharacterBody2D
{
	// ####################### ONREADY VAR #######################

	[Export] public VelocityComponent velocityComponent;
	[Export] public WallJumpComponent wallJumpComponent;
	[Export] public DashComponent dashComponent;

	private Camera2D camera;
	private AnimationPlayer animPlayer;
	private bool initialized = false;

	private HealthComponent healthCompHead;
	private HealthComponent healthCompBody;
	private HealthComponent healthCompRightArm;
	private HealthComponent healthCompLeftArm;
	private HealthComponent healthCompRightLeg;
	private HealthComponent healthCompLeftLeg;

	// ####################### VARIABLES #########################

	private Vector2 tempVelocity = new Vector2(0, 0);
	private Vector2 normal = new Vector2(0, 0);
	private bool isDead = false;
	private bool isTakingDamage = false;
	private bool isAttacking = false;
	private bool isHeadDismembered = false;
	private bool isRightArmDismembered = false;
	private bool isLeftArmDismembered = false;
	private bool isRightLegDismembered = false;
	private bool isLeftLegDismembered = false;
	
	private bool isInput = true;
	private bool isDebug = false;

	// ####################### SIGNALS #########################

	[Signal] public delegate void DieEventHandler();
	[Signal] public delegate void DismemberEventHandler();

	// ####################### METHODS #########################

	public override void _Ready()
	{
		InitializeComponents();

		initialized = true;
		
	}

	private void InitializeComponents()
	{
		camera = GetNode<Camera2D>("Camera2D");
		animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

		healthCompHead = SetupHealthComponent("Sprite2D/HurtBoxHead/HealthComponentHead");
		healthCompBody = SetupHealthComponent("Sprite2D/HurtBoxBody/HealthComponentBody");
		healthCompRightArm = SetupHealthComponent("Sprite2D/HurtBoxRArm/HealthComponentRightArm");
		healthCompLeftArm = SetupHealthComponent("Sprite2D/HurtBoxLArm/HealthComponentLeftArm");
		healthCompRightLeg = SetupHealthComponent("Sprite2D/HurtBoxRLeg/HealthComponentRightLeg");
		healthCompLeftLeg = SetupHealthComponent("Sprite2D/HurtBoxLleg/HealthComponentLeftLeg");
	}

	private HealthComponent SetupHealthComponent(string path)
	{
		var healthComponent = GetNode<HealthComponent>(path);
		healthComponent.DismemberHead += DismemberBodyPart;
		healthComponent.DismemberLARM += DismemberBodyPart;
		healthComponent.DismemberLLEG += DismemberBodyPart;
		healthComponent.DismemberRARM += DismemberBodyPart;
		healthComponent.DismemberRLEG += DismemberBodyPart;
		healthComponent.Damaged += Damage;
		healthComponent.Death += Death;
		return healthComponent;
	}


	public override void _Process(double delta)
	{
		Debug();
		velocityComponent.ActivateMove();
		velocityComponent.HandleVelocity(delta);
		wallJumpComponent.HandleWallJump();
		dashComponent.HandleDash();
		AnimatePlayer();
	}

	
	private async void Damage(float amount, Vector2 knockback)
	{
		isTakingDamage = true;
		await ToSignal(animPlayer, AnimationPlayer.SignalName.AnimationFinished);
		isTakingDamage = false;
	}

	private async void Death()
	{
		if (isDead) return;

		await ToSignal(animPlayer, AnimationPlayer.SignalName.AnimationFinished);
		EmitSignal(nameof(DieEventHandler));
		isDead = true;
		DisableCollisionShapes();
	}

	private void DisableCollisionShapes()
	{
		GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred("disabled", true);
		GetNode<CollisionShape2D>("CollisionShape2D2").SetDeferred("disabled", true);
	}

	private void DismemberBodyPart(string bodyPart)
	{
		EmitSignal(nameof(DismemberEventHandler));

		switch (bodyPart)
		{
			case "HealthComponentHead":
				if (isHeadDismembered) return;
				isHeadDismembered = true;
				GD.Print("Head dismembered");
				SetDismemberIconVisible("Dismember Icon Head", true);
				break;
			case "HealthComponentRightArm":
				if (isRightArmDismembered) return;
				isRightArmDismembered = true;
				GD.Print("Right Arm dismembered");
				SetDismemberIconVisible("Dismember Icon RightArm", true);
				break;
			case "HealthComponentLeftArm":
				if (isLeftArmDismembered) return;
				isLeftArmDismembered = true;
				GD.Print("Left Arm dismembered");
				SetDismemberIconVisible("Dismember Icon LeftArm", true);
				break;
			case "HealthComponentRightLeg":
				if (isRightLegDismembered) return;
				isRightLegDismembered = true;
				GD.Print("Right Leg dismembered");
				SetDismemberIconVisible("Dismember Icon RightLeg", true);
				break;
			case "HealthComponentLeftLeg":
				if (isLeftLegDismembered) return;
				isLeftLegDismembered = true;
				GD.Print("Left Leg dismembered");
				SetDismemberIconVisible("Dismember Icon LeftLeg", true);
				break;
		}
	}

	private void SetDismemberIconVisible(string iconName, bool isVisible)
	{
		GetNode<Sprite2D>($"Sprite2D/{iconName}").Visible = isVisible;
	}

	private async void ApplyKnockback(Vector2 knockback, int frames = 10)
	{
		if (knockback.IsZeroApprox()) return;

		for (int i = 0; i < frames; i++)
		{
			tempVelocity += knockback;
			Velocity = tempVelocity;
			await ToSignal(GetTree(), "physics_frame");
		}
	}

	private void AnimatePlayer()
	{
		HandleHorizontalFlip();
		HandleSpriteRotation();

		if (IsJumping())
		{
			PlayAnimation("jump", "jump_left");
		}
		else if (IsFalling())
		{
			PlayAnimation("fall", "fall_left");
		}
		else if (IsOnFloor())
		{
			if (IsIdle())
			{
				PlayAnimation("idle", "idle_left");
				
			}
			else if (IsMoving())
			{
				PlayAnimation("run", "run_left");
			}
		}
		if (dashComponent.isDash)
		{
			PlayAnimation("dash", "dash_left");
		}
		if (wallJumpComponent.isWallJump)
		{
			PlayAnimation("walljmp", "walljmp_left");
		}
		if (isTakingDamage)
		{
			PlayAnimation("hurt", "hurt_left");
		}
		if (isDead)
		{
			PlayAnimation("death", "death_left");
		}
		if (Input.IsActionJustPressed("attack") && !isAttacking && !IsRestricted())
		{
			PlayAttackAnimation();
		}
	}

	private void PlayAnimation(string anim, string animLeft)
	{
		animPlayer.Play(!GetNode<Sprite2D>("Sprite2D").FlipH ? anim : animLeft);
	}

	private bool IsJumping()
	{
		return Velocity.Y < 1 && !IsOnFloor() && Input.IsActionPressed("jump") && !IsRestricted();
	}

	private bool IsFalling()
	{
		return Velocity.Y >= 0 && !IsOnFloor() && !IsRestricted();
	}

	private bool IsIdle()
	{
		return Velocity.X < 20 && Velocity.X > -20 && Velocity.Y < 10 && !IsRestricted();
	}

	private bool IsMoving()
	{
		return Velocity.X != 0 && (Input.IsActionPressed("move_right") || Input.IsActionPressed("move_left")) && !IsRestricted();
	}

	private async void PlayAttackAnimation()
	{
		isAttacking = true;
		PlayAnimation("attack", "attack_left");
		await ToSignal(animPlayer, AnimationPlayer.SignalName.AnimationFinished);
		isAttacking = false;
	}

	private void HandleHorizontalFlip()
	{
		if (isInput)
		{
			if (Input.IsActionPressed("move_left"))
			{
				GetNode<Sprite2D>("Sprite2D").FlipH = true;
			}
			else if (Input.IsActionPressed("move_right"))
			{
				GetNode<Sprite2D>("Sprite2D").FlipH = false;
			}
		}
	}

	private void HandleSpriteRotation()
	{	
		if (GetNode<RayCast2D>("RayCastFloor").IsColliding())
		{
			GetNode<Sprite2D>("Sprite2D").Rotation = velocityComponent.normal.Angle() + Mathf.DegToRad(90);
		}
		else
		{
			GetNode<Sprite2D>("Sprite2D").Rotation = Mathf.Lerp(GetNode<Sprite2D>("Sprite2D").Rotation, 0.0f, 0.08f);
		}
	}

	private bool IsRestricted()
	{
		return isAttacking || isTakingDamage || dashComponent.isDash || wallJumpComponent.isWallJump;
	}

	private void StartTimer(Timer timer, float duration)
	{
		timer.WaitTime = duration;
		timer.OneShot = true;
		timer.Start();
	}

	private void Debug()
	{
		RichTextLabel richTextLabel = GetNode<RichTextLabel>("RichTextLabel");
		if (Input.IsActionJustPressed("debug"))
		{
			isDebug = !isDebug;
			GD.Print("Debug mode: " + isDebug);
		}
		if (isDebug)
		{
			richTextLabel.Text = $"Velocity: {Velocity}\nHead: {healthCompHead.CurrentHP}";
			GD.Print(richTextLabel.Text);	
		}
		else
		{
			richTextLabel.Text = "";
		}
		if (isDebug && Input.IsActionPressed("ctrl"))
		{
			if (Input.IsActionJustPressed("scroll_up"))
			{
				camera.Zoom *= new Vector2(1.2f, 1.2f);
				GD.Print("Zoom: " + camera.Zoom);
			}

			if (Input.IsActionJustPressed("scroll_down"))
			{
				camera.Zoom /= new Vector2(1.2f, 1.2f);
				GD.Print("Zoom: " + camera.Zoom);
			}
		}
	}
}
