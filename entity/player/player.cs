using Godot;
using System;

public partial class Player : CharacterBody2D
{
	// ####################### ONREADY VAR #######################
	[Export] public float JumpHeight;
	[Export] public float JumpTimeToPeak;
	[Export] public float JumpTimeToDescent;

	private Camera2D _camera;
	private AnimationPlayer _animPlayer;
	private bool _initialized = false;

	private HealthComponent _healthCompHead;
    private HealthComponent _healthCompBody;
    private HealthComponent _healthCompRightArm;
    private HealthComponent _healthCompLeftArm;
    private HealthComponent _healthCompRightLeg;
    private HealthComponent _healthCompLeftLeg;

	// ####################### VARIABLES #########################

	private int _maxSpeed = 1600;
	private int _jumpForce = 500;
	private float _jumpVelocity;
	private float _jumpGravity;
	private float _fallGravity;

	private int _acceleration = 300;
	private int _jumpBufferTime = 15;
	private int _jumpBufferCounter = 0;
	private float _dashDuration = 0.2f;
	private int _wallJumpTimer = 0;
	private int _wallJumpCooldown = 20;

	private Vector2 _chainVelocity = new Vector2(0, 0);
	private Vector2 _tempVelocity = new Vector2(0, 0);
	private int _chainPullForce = 60;
	private float _onGroundFriction = 0.01f;
	private float _onAirFriction = 0.002f;
	private float _unsignedSpeed = 0.0f;
	private float _gravityFactor = 0.02f;
	private Vector2 _normal = new Vector2(0, 0);
	private bool _isDead = false;
	private bool _isTakingDamage = false;
	private bool _isAttacking = false;
	private bool _isDash = false;
	private bool _isWallJump = false;
	private bool _isHeadDismembered = false;
	private bool _isRightArmDismembered = false;
	private bool _isLeftArmDismembered = false;
	private bool _isRightLegDismembered = false;
	private bool _isLeftLegDismembered = false;
	private bool _isGravity = true;
	private bool _isInput = true;
	private bool _isDebug = false;

	// ####################### SIGNALS #########################

	[Signal] public delegate void DieEventHandler();
	[Signal] public delegate void DismemberEventHandler();

	// ####################### METHODS #########################

	public override void _Ready()
	{
		InitializeComponents();
		CalculateJumpPhysics();

		_initialized = true;
	}

	private void InitializeComponents()
	{
		_camera = GetNode<Camera2D>("Camera2D");
		_animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

		_healthCompHead = SetupHealthComponent("Sprite2D/HurtBoxHead/HealthComponentHead");
		_healthCompBody = SetupHealthComponent("Sprite2D/HurtBoxBody/HealthComponentBody");
		_healthCompRightArm = SetupHealthComponent("Sprite2D/HurtBoxRArm/HealthComponentRightArm");
		_healthCompLeftArm = SetupHealthComponent("Sprite2D/HurtBoxLArm/HealthComponentLeftArm");
		_healthCompRightLeg = SetupHealthComponent("Sprite2D/HurtBoxRLeg/HealthComponentRightLeg");
		_healthCompLeftLeg = SetupHealthComponent("Sprite2D/HurtBoxLleg/HealthComponentLeftLeg");
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
		_jumpVelocity = ((2.0f * JumpHeight) / JumpTimeToPeak) * -1;
		_jumpGravity = -((2.0f * JumpHeight) / Mathf.Pow(JumpTimeToPeak, 2)) * -1;
		_fallGravity = -((2.0f * JumpHeight) / Mathf.Pow(JumpTimeToDescent, 2)) * -1;
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
		_unsignedSpeed = Mathf.Abs(Velocity.X);
		_normal = GetNode<RayCast2D>("RayCastFloor").GetCollisionNormal();
		MoveHorizontal();
		Jump();
		WallJump();
		Dash();
	}

	private void MoveHorizontal()
	{
		float friction = IsOnFloor() ? _onGroundFriction : _onAirFriction;
		_tempVelocity = Velocity;

		if (_isDash) friction *= 4;

		if (Input.IsActionPressed("move_right") && !IsHooked() && _isInput)
		{
			ApplyMovement(friction, _acceleration);
		}
		else if (Input.IsActionPressed("move_left") && !IsHooked() && _isInput)
		{
			ApplyMovement(friction, -_acceleration);
		}
		else if (!Input.IsActionPressed("move_left") && !Input.IsActionPressed("move_right"))
		{
			if (!IsHooked())
			{
				_tempVelocity.X = 0;
				Velocity = _tempVelocity;
			}
		}
		_tempVelocity.X = Mathf.Clamp(Velocity.X, -_maxSpeed, _maxSpeed);
		Velocity = _tempVelocity;
	}

	private void ApplyMovement(float friction, int acceleration)
	{
		if (!(Velocity.X >= -acceleration && Velocity.X <= acceleration))
		{
			_tempVelocity.X = Mathf.Lerp(_tempVelocity.X, (float)acceleration, 1);
			Velocity = _tempVelocity;
		}
		else
		{
			_tempVelocity.X = Mathf.Lerp(_tempVelocity.X, (float)acceleration, 1);
			Velocity = _tempVelocity;
		}
		_tempVelocity.X = Velocity.X * (_normal.X + 0.9f);
		Velocity = _tempVelocity;
	}

	private bool IsHooked()
	{
		return GetNode<Chain>("Chain").hooked || GetNode<Chain>("Chain2").hooked;
	}

	private void Jump()
	{
		if (Input.IsActionJustPressed("jump") && IsOnFloor() && _isInput)
		{
			_jumpBufferCounter = _jumpBufferTime;
		}
		if (_jumpBufferCounter > 0)
		{
			_jumpBufferCounter--;
		}
		if (_jumpBufferCounter > 0)
		{
			_tempVelocity.Y = _jumpVelocity;
			Velocity = _tempVelocity;
			_jumpBufferCounter = 0;
		}
		if (Input.IsActionJustReleased("jump") && Velocity.Y < 0)
		{
			_tempVelocity.Y *= 0.5f;
			Velocity = _tempVelocity;
		}
	}

	private void WallJump()
	{
		if (Velocity.Y >= 10 && _wallJumpTimer < _wallJumpCooldown && IsOnWallOnly())
		{
			_wallJumpTimer++;
			_isWallJump = true;
		}
		else
		{
			_wallJumpTimer = 0;
			_isWallJump = false;
		}
	}

	private void Dash()
	{
		var timer = GetNode<Timer>("DashTimer");
		if (Input.IsActionJustPressed("dash") && timer.IsStopped())
		{
			StartTimer(timer, _dashDuration);
		}
		if (!timer.IsStopped())
		{
			_isDash = true;
			_isGravity = false;
			_isInput = false;
			ApplyDashSpeed();
		}
		else
		{
			_isDash = false;
			_isGravity = true;
			_isInput = true;
		}
	}

	private void ApplyDashSpeed()
	{
		int dashSpeed = 600;
		_tempVelocity = Velocity;

		_tempVelocity.X = GetNode<Sprite2D>("Sprite2D").FlipH ? -dashSpeed : dashSpeed;
		_tempVelocity.Y = 0;
		Velocity = _tempVelocity;
	}

	private void Hook()
	{
		HandleHook("hook", "Chain");
		HandleHook("hook2", "Chain2");
	}

	private void HandleHook(string hookAction, string chainNodeName)
	{
		var chain = GetNode<Chain>(chainNodeName);
		if (Input.IsActionJustPressed(hookAction) && _initialized && !chain.hooked && !chain.flying)
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
		HandleHookPhysics("Chain", ref _chainVelocity);
		HandleHookPhysics("Chain2", ref _chainVelocity);
	}

	private void HandleHookPhysics(string chainNodeName, ref Vector2 chainVelocity)
	{
		var chain = GetNode<Chain>(chainNodeName);
		if (chain.hooked)
		{
			var walk = (Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left")) * _acceleration;
			chainVelocity = ToLocal(chain.tip).Normalized() * _chainPullForce;
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
		_isTakingDamage = true;
		await ToSignal(_animPlayer, AnimationPlayer.SignalName.AnimationFinished);
		_isTakingDamage = false;
	}

	private async void Death()
	{
		if (_isDead) return;

		await ToSignal(_animPlayer, AnimationPlayer.SignalName.AnimationFinished);
		EmitSignal(nameof(DieEventHandler));
		_isDead = true;
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
				if (_isHeadDismembered) return;
				_isHeadDismembered = true;
				GD.Print("Head dismembered");
				SetDismemberIconVisible("Dismember Icon Head", false);
				break;
			case "HealthComponentRightArm":
				if (_isRightArmDismembered) return;
				_isRightArmDismembered = true;
				GD.Print("Right Arm dismembered");
				SetDismemberIconVisible("Dismember Icon RightArm", false);
				break;
			case "HealthComponentLeftArm":
				if (_isLeftArmDismembered) return;
				_isLeftArmDismembered = true;
				GD.Print("Left Arm dismembered");
				SetDismemberIconVisible("Dismember Icon LeftArm", false);
				break;
			case "HealthComponentRightLeg":
				if (_isRightLegDismembered) return;
				_isRightLegDismembered = true;
				GD.Print("Right Leg dismembered");
				SetDismemberIconVisible("Dismember Icon RightLeg", false);
				break;
			case "HealthComponentLeftLeg":
				if (_isLeftLegDismembered) return;
				_isLeftLegDismembered = true;
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
			_tempVelocity += knockback;
			Velocity = _tempVelocity;
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
				_animPlayer.SpeedScale = _unsignedSpeed / 200;
			}
		}
		if (_isDash)
		{
			PlayAnimation("dash", "dash_left");
		}
		if (_isWallJump)
		{
			PlayAnimation("walljmp", "walljmp_left");
		}
		if (_isTakingDamage)
		{
			PlayAnimation("hurt", "hurt_left");
		}
		if (_isDead)
		{
			PlayAnimation("death", "death_left");
		}
		if (Input.IsActionJustPressed("attack") && !_isAttacking && !IsRestricted())
		{
			PlayAttackAnimation();
		}
	}

	private void PlayAnimation(string anim, string animLeft)
	{
		_animPlayer.Play(!GetNode<Sprite2D>("Sprite2D").FlipH ? anim : animLeft);
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
		return Velocity.X != 0 && Input.IsActionPressed("move_right") || Input.IsActionJustPressed("move_left") && !IsRestricted();
	}

	private async void PlayAttackAnimation()
	{
		_isAttacking = true;
		PlayAnimation("attack", "attack_left");
		await ToSignal(_animPlayer, AnimationPlayer.SignalName.AnimationFinished);
		_isAttacking = false;
	}

	private void HandleHorizontalFlip()
	{
		if (_isInput)
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
			GetNode<Sprite2D>("Sprite2D").Rotation = _normal.Angle() + Mathf.DegToRad(90);
		}
		else
		{
			GetNode<Sprite2D>("Sprite2D").Rotation = Mathf.Lerp(GetNode<Sprite2D>("Sprite2D").Rotation, 0.0f, 0.08f);
		}
	}

	private bool IsRestricted()
	{
		return _isAttacking || _isTakingDamage || _isDash || _isWallJump;
	}

	private void StartTimer(Timer timer, float duration)
	{
		timer.WaitTime = duration;
		timer.OneShot = true;
		timer.Start();
	}

	private void ApplyGravity(double delta)
	{
		if (!IsOnFloor() && _isGravity && !_isWallJump)
		{
			_tempVelocity.Y += ReturnGravity() * (float)delta;
			_tempVelocity.Y = Mathf.Clamp(_tempVelocity.Y, -_maxSpeed, _maxSpeed);
			Velocity = _tempVelocity;
		}
		else if (Velocity.Y > 0)
		{
			_tempVelocity.Y = 0;
			Velocity = _tempVelocity;
		}
	}

	private float ReturnGravity()
	{
		return Velocity.Y < 0.0 ? _jumpGravity : _fallGravity;
	}

	private void Debug()
	{
		RichTextLabel richTextLabel = GetNode<RichTextLabel>("RichTextLabel");
		if (Input.IsActionJustPressed("debug"))
		{
			_isDebug = !_isDebug;
			GD.Print("Debug mode: " + _isDebug);
		}
		if (_isDebug)
		{
			richTextLabel.Text = $"Velocity: {Velocity}\nNormal: {_normal}\nIs Gravity: {_isGravity}\nIs Input: {_isInput}\nIs Restricted: {IsRestricted()}\nIs Jumping: {Input.IsActionJustPressed("jump")}\nCurrent Animation: {_animPlayer.CurrentAnimation}";
		}
		else
		{
			richTextLabel.Text = "";
		}
		if (_isDebug && Input.IsActionPressed("ctrl"))
		{
			if (Input.IsActionPressed("scroll_up"))
			{
				_camera.Zoom *= new Vector2(1.2f, 1.2f);
				GD.Print("Zoom: " + _camera.Zoom);
			}

			if (Input.IsActionPressed("scroll_down"))
			{
				_camera.Zoom /= new Vector2(1.2f, 1.2f);
				GD.Print("Zoom: " + _camera.Zoom);
			}
		}
	}
}
