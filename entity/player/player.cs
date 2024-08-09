using Godot;
using System;

public partial class Player : CharacterBody2D
{
	// ####################### ONREADY VAR #######################
	[Export] public float JumpHeight;
	[Export] public float JumpTimeToPeak;
	[Export] public float JumpTimeToDescent;

	public Camera2D camera;
	public AnimationPlayer animPlayer;
	public AnimationTree animationTree;
	public bool initialized = false;

	public HealthComponent healthCompHead;
    public HealthComponent healthCompBody;
    public HealthComponent healthCompRightArm;
    public HealthComponent healthCompLeftArm;
    public HealthComponent healthCompRightLeg;
    public HealthComponent healthCompLeftLeg;

	// ####################### VARIABLES #########################

	private int maxSpeed = 1600;
	private int jumpForce = 500;
	private float jumpVelocity;
	private float jumpGravity;
	private float fallGravity;

	private int acceleration = 300;
	private int jumpBufferTime = 15;
	private int jumpBufferCounter = 0;
	private float dashDuration = 0.2f;
	private int wallJumpTimer = 0;
	private int wallJumpCooldown = 20;

	private Vector2 chainVelocity = new Vector2(0, 0);
	private Vector2 tempVelocity = new Vector2(0, 0);
	private int chainPullForce = 60;
	private float onGroundFriction = 0.1f;
	private float onAirFriction = 0.02f;
	private float unsignedSpeed = 0.0f;
	private float gravityFactor = 0.2f;
	private Vector2 normal = new Vector2(0, 0);
	private bool isDead = false;
	private bool isTakingDamage = false;
	private bool isAttacking = false;
	private bool isDash = false;
	private bool isWallJump = false;
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
		CalculateJumpPhysics();

		initialized = true;
	}

	private void InitializeComponents()
	{
		camera = GetNode<Camera2D>("Camera2D");
		animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		animationTree = GetNode<AnimationTree>("AnimationTree");

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

	private void CalculateJumpPhysics()
	{
		jumpVelocity = ((2.0f * JumpHeight) / JumpTimeToPeak) * -1;
		jumpGravity = -((2.0f * JumpHeight) / Mathf.Pow(JumpTimeToPeak, 2)) * -1;
		fallGravity = -((2.0f * JumpHeight) / Mathf.Pow(JumpTimeToDescent, 2)) * -1;
	}

	public override void _Process(double delta)
	{
		Debug();
		MoveAndSlide();
		MovePlayer(delta);
		AnimatePlayer();
		ApplyGravity(delta);
		Hook();
		HookPhys();
	}

	private void MovePlayer(double delta)
	{
		unsignedSpeed = Mathf.Abs(Velocity.X);
		normal = GetNode<RayCast2D>("RayCastFloor").GetCollisionNormal();
		MoveHorizontal();
		Jump();
		WallJump();
		Dash();
	}

	private void MoveHorizontal()
	{
		float friction = IsOnFloor() ? onGroundFriction : onAirFriction;
		tempVelocity = Velocity;

		if (isDash) friction *= 4;

		if (Input.IsActionPressed("move_right") && !IsHooked() && isInput)
		{
			ApplyMovement(friction, acceleration);
		}
		else if (Input.IsActionPressed("move_left") && !IsHooked() && isInput)
		{
			ApplyMovement(friction, -acceleration);
		}
		else if (!Input.IsActionPressed("move_left") && !Input.IsActionPressed("move_right"))
		{
			if (!IsHooked())
			{
				tempVelocity.X = 0;
				Velocity = tempVelocity;
			}
		}
		tempVelocity.X = Mathf.Clamp(Velocity.X, -maxSpeed, maxSpeed);
		Velocity = tempVelocity;
	}

	private void ApplyMovement(float friction, int acceleration)
	{
		if (!(Velocity.X >= -acceleration && Velocity.X <= acceleration))
		{
			tempVelocity.X = Mathf.Lerp(tempVelocity.X, (float)acceleration, 1);
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

	private bool IsHooked()
	{
		return GetNode<Chain>("Chain").hooked || GetNode<Chain>("Chain2").hooked;
	}

	private void Jump()
	{
		if (Input.IsActionJustPressed("jump") && IsOnFloor() && isInput)
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
			Velocity = tempVelocity;
			jumpBufferCounter = 0;
		}
		if (Input.IsActionJustReleased("jump") && Velocity.Y < 0)
		{
			tempVelocity.Y *= 0.5f;
			Velocity = tempVelocity;
		}
	}

	private void WallJump()
	{
		if (Velocity.Y >= 10 && wallJumpTimer < wallJumpCooldown && IsOnWallOnly())
		{
			wallJumpTimer++;
			isWallJump = true;
		}
		else
		{
			wallJumpTimer = 0;
			isWallJump = false;
		}
	}

	private void Dash()
	{
		var timer = GetNode<Timer>("DashTimer");
		if (Input.IsActionJustPressed("dash") && timer.IsStopped())
		{
			StartTimer(timer, dashDuration);
		}
		if (!timer.IsStopped())
		{
			isDash = true;
			isGravity = false;
			isInput = false;
			ApplyDashSpeed();
		}
		else
		{
			isDash = false;
			isGravity = true;
			isInput = true;
		}
	}

	private void ApplyDashSpeed()
	{
		int dashSpeed = 600;
		tempVelocity = Velocity;

		tempVelocity.X = GetNode<Sprite2D>("Sprite2D").FlipH ? -dashSpeed : dashSpeed;
		tempVelocity.Y = 0;
		Velocity = tempVelocity;
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

	private void FlipAnimationTree() 
	{
		
		
		
		
		
		
		
		
	}

	private void AnimatePlayer()
	{
		HandleHorizontalFlip();
		HandleSpriteRotation();
		FlipAnimationTree();

		if (IsJumping())
		{
			animationTree.Set("parameters/Jump/blend_position", GetNode<Sprite2D>("Sprite2D").FlipH ? -1 : 1);
		}
		else if (IsFalling())
		{
			animationTree.Set("parameters/Fall/blend_position", GetNode<Sprite2D>("Sprite2D").FlipH ? -1 : 1);
		}
		else if (IsOnFloor())
		{
			if (IsIdle())
			{
				animationTree.Set("parameters/Idle/blend_position", GetNode<Sprite2D>("Sprite2D").FlipH ? -1 : 1);
			}
			else if (IsMoving())
			{
				animationTree.Set("parameters/Run/blend_position", GetNode<Sprite2D>("Sprite2D").FlipH ? -1 : 1);
				animPlayer.SpeedScale = unsignedSpeed / 200;
			}
		}
		if (isDash)
		{
			animationTree.Set("parameters/Dash/blend_position", GetNode<Sprite2D>("Sprite2D").FlipH ? -1 : 1);
		}
		if (isWallJump)
		{
			animationTree.Set("parameters/WallJump/blend_position", GetNode<Sprite2D>("Sprite2D").FlipH ? -1 : 1);
		}
		if (isTakingDamage)
		{
			animationTree.Set("parameters/Hurt/blend_position", GetNode<Sprite2D>("Sprite2D").FlipH ? -1 : 1);
		}
		if (isDead)
		{
			animationTree.Set("parameters/Death/blend_position", GetNode<Sprite2D>("Sprite2D").FlipH ? -1 : 1);
		}
		if (Input.IsActionJustPressed("attack") && !isAttacking && !IsRestricted())
		{
			animationTree.Set("parameters/Attack/blend_position", GetNode<Sprite2D>("Sprite2D").FlipH ? -1 : 1);
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
			GetNode<Sprite2D>("Sprite2D").Rotation = normal.Angle() + Mathf.DegToRad(90);
		}
		else
		{
			GetNode<Sprite2D>("Sprite2D").Rotation = Mathf.Lerp(GetNode<Sprite2D>("Sprite2D").Rotation, 0.0f, 0.08f);
		}
	}

	private bool IsRestricted()
	{
		return isAttacking || isTakingDamage || isDash || isWallJump;
	}

	private void StartTimer(Timer timer, float duration)
	{
		timer.WaitTime = duration;
		timer.OneShot = true;
		timer.Start();
	}

	private void ApplyGravity(double delta)
	{
		if (!IsOnFloor() && isGravity && !isWallJump)
		{
			tempVelocity.Y += ReturnGravity() * (float)delta;
			tempVelocity.Y = Mathf.Clamp(tempVelocity.Y, -maxSpeed, maxSpeed);
			Velocity = tempVelocity;
		}
		else if (Velocity.Y > 0)
		{
			tempVelocity.Y = 0;
			Velocity = tempVelocity;
		}
	}

	private float ReturnGravity()
	{
		return Velocity.Y < 0.0 ? jumpGravity : fallGravity;
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
