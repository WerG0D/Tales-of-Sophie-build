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

	private int maxSpeed = 1600;
	private int jumpForce = 500;

	private int acceleration = 300;
	private int jumpBufferTime = 15;
	private int jumpBufferCounter = 0;
	private float dashDuration = 0.2f;

	private Vector2 chainVelocity = new Vector2(0, 0);
	private Vector2 tempVelocity = new Vector2(0, 0);
	private int chainPullForce = 60;
	private float onGroundFriction = 0.1f;
	private float onAirFriction = 0.02f;
	private float gravityFactor = 0.2f;
	private Vector2 normal = new Vector2(0, 0);
	private bool isDead = false;
	private bool isTakingDamage = false;
	private bool isAttacking = false;
	private bool isHeadDismembered = false;
	private bool isRightArmDismembered = false;
	private bool isLeftArmDismembered = false;
	private bool isRightLegDismembered = false;
	private bool isLeftLegDismembered = false;
	private bool isGravity = true;
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
		Hook();
		HookPhys();
	}

	private bool IsHooked()
	{
		return GetNode<Chain>("Chain").hooked || GetNode<Chain>("Chain2").hooked;
	}


	private void Hook()
	{
		HandleHook("hook", "Chain");
		HandleHook("hook2", "Chain2");
	}

	private void HandleHook(string hookAction, string chainNodeName)
	{
		var chain = GetNode<Chain>(chainNodeName);
		if (Input.IsActionJustPressed(hookAction) && initialized && !chain.hooked && !chain.flying)
		{
			var mouseViewportPos = GetViewport().GetMousePosition();
			chain.shoot((mouseViewportPos - GetViewportRect().Size / 2).Normalized());
		}
		else if (Input.IsActionJustPressed(hookAction) || (Input.IsActionJustPressed("jump") && chain.hooked))
		{
			chain.Release();
			Velocity *= 1.2f;
		}
	}

	private void HookPhys()
	{
		HandleHookPhysics("Chain", ref chainVelocity);
		HandleHookPhysics("Chain2", ref chainVelocity);
	}

	private void HandleHookPhysics(string chainNodeName, ref Vector2 chainVelocity)
	{
		var chain = GetNode<Chain>(chainNodeName);
		if (chain.hooked)
		{
			var walk = (Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left")) * acceleration;
			chainVelocity = ToLocal(chain.tip).Normalized() * chainPullForce;
			chainVelocity.Y *= chainVelocity.Y > 0 ? 0.55f : 1.1f;
			if (Mathf.Sign(chainVelocity.X) != Mathf.Sign(walk))
			{
				chainVelocity.X *= 0.3f;
			}
		}
		else
		{
			chainVelocity = Vector2.Zero;
		}
		Velocity += chainVelocity;
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
				SetDismemberIconVisible("Dismember Icon Head", false);
				break;
			case "HealthComponentRightArm":
				if (isRightArmDismembered) return;
				isRightArmDismembered = true;
				GD.Print("Right Arm dismembered");
				SetDismemberIconVisible("Dismember Icon RightArm", false);
				break;
			case "HealthComponentLeftArm":
				if (isLeftArmDismembered) return;
				isLeftArmDismembered = true;
				GD.Print("Left Arm dismembered");
				SetDismemberIconVisible("Dismember Icon LeftArm", false);
				break;
			case "HealthComponentRightLeg":
				if (isRightLegDismembered) return;
				isRightLegDismembered = true;
				GD.Print("Right Leg dismembered");
				SetDismemberIconVisible("Dismember Icon RightLeg", false);
				break;
			case "HealthComponentLeftLeg":
				if (isLeftLegDismembered) return;
				isLeftLegDismembered = true;
				GD.Print("Left Leg dismembered");
				SetDismemberIconVisible("Dismember Icon LeftLeg", false);
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
			richTextLabel.Text = $"Velocity: {Velocity}\nNormal: {normal}\nIs Gravity: {isGravity}\nIs Input: {isInput}\nIs Restricted: {IsRestricted()}\nIs Jumping: {Input.IsActionJustPressed("jump")}\nCurrent Animation: {animPlayer.CurrentAnimation}";
		}
		else
		{
			richTextLabel.Text = "";
		}
		if (isDebug && Input.IsActionPressed("ctrl"))
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
}
