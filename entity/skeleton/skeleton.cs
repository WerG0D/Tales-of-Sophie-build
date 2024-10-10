using Godot;
using System;

public partial class skeleton : CharacterBody2D
{
	[Signal] public delegate void DieEventHandler();

	// ############## VARS ##############
	public const float SPEED = 300.0f;
	public const float JUMP_VELOCITY = -400.0f;
	public bool is_roaming = false;
	public int skeletondmg = 10;
	public float skeletonstuntime = 0.5f;
	public float skeletonknockbackforce = 0.5f;
	public bool player_in_area = false;
	public bool is_dead = false;
	public bool is_attacking = false;
	public bool is_taking_damage = false;
	public CharacterBody2D player;
	public int dir = 1;
	public AnimationPlayer animplayer;
	public HealthComponent healthcomp;
	public HitBoxComponent hitBoxComponent;
	public float gravity = (float)ProjectSettings.GetSetting("physics/2d/default_gravity");
	public override void _Ready()
	{
		animplayer = GetNode<AnimationPlayer>("Sprite2D/AnimationPlayer");
		healthcomp = GetNode<HealthComponent>("HealthCompGeneral");

	}

	public Vector2 LerpVector2(Vector2 from, Vector2 to, float weight)
	{
    return new Vector2(
        Mathf.Lerp(from.X, to.X, weight),
        Mathf.Lerp(from.Y, to.Y, weight)
    );
	}

	private void Death()
	{
	    if (is_dead) { return; }
		EmitSignal("DieEventHandler");
		is_dead = true;
		animplayer.Play("die");
		GetNode<CollisionShape2D>("DetectionArea/CollisionShape2D").Disabled = true;
		animplayer.Play("death");
	 }

    public override void _Process(double delta)
	{
		if (!IsOnFloor()) 
		{	var tempVelocity = Velocity;
			tempVelocity.Y += (float)(gravity * delta);
			Velocity = tempVelocity;
		}
		if (!is_dead)
		{
			GetNode<CollisionShape2D>("DetectionArea/CollisionShape2D").Disabled = false;
			if (player_in_area)
			{
				var tempPosition = Position;
				tempPosition.X = Mathf.Lerp(player.Position.X, Position.X, 0.1f);
				Position = tempPosition;
			}
				
		}
		MoveAndSlide();
		AnimateSkeleton();
		Debug();
		Attack();

	}

	public async void Damaged(float amount, Vector2 knockback)
	{
		ApplyKnockback(knockback);
		is_taking_damage = true;
		if (GetNode<Sprite2D>("Sprite2D").FlipH == true)
		{
			animplayer.Play("hurt_left");
			await ToSignal(animplayer, AnimationPlayer.SignalName.AnimationFinished);
			is_taking_damage = false;
		}
		else
		{
			animplayer.Play("hurt_right");
			await ToSignal(animplayer, AnimationPlayer.SignalName.AnimationFinished);
			is_taking_damage = false;
		}
	}

	public async void ApplyKnockback(Vector2 knockback, int frames = 10)
	{
		if (knockback.IsZeroApprox()) { return; }
		for (int i = 0; i < frames; i++)
		{
			Velocity += LerpVector2(Velocity, knockback, 0.2f);
			await ToSignal(GetTree(), "physics_frame");
		}
	}

	public float GetHealth()
	{
		return healthcomp.CurrentHP;
	}

	public void AnimateSkeleton() 
	{ 
		if (!is_taking_damage && !is_dead && !is_roaming) {
			animplayer.Play("skeleton1_idle");
		}
		if (Input.IsActionJustPressed("roaming")) {
			is_roaming = is_roaming ? false : true;
			GetNode<CollisionShape2D>("DetectionArea/CollisionShape2D").Disabled = is_roaming;
		}
		if (is_roaming) {
			is_attacking = true;

			if (is_attacking) {
				animplayer.Play("attack");
				GetNode<CollisionShape2D>("DetectionArea/CollisionShape2D").Disabled = false;
			}
			
		}
	}

	public void Move() 
	{
		if (IsOnWall() && IsOnFloor())
		{
			dir *= -1;
			var tempVelocity = Velocity;
			tempVelocity.X = dir * SPEED;
			Velocity = tempVelocity;
		}
	}

	private void Attack() {

		if (Input.IsActionJustPressed("Skeleton")) {
			is_attacking = true;
			GetNode<CollisionShape2D>("Sprite2D/HitboxComponent/CollisionShape2D").SetDeferred("disabled", false);


		}

	}

	public void OnDetectionAreaEntered(object body)
	{
		if (body is CharacterBody2D)
		{
			player = (CharacterBody2D)body;
			player_in_area = true;
		}
	}

	public void OnAnimationPlayerAnimationFinished(string anim_name)
	{
		if (anim_name == "hurt")
		{
			is_taking_damage = false;
		}

		if (anim_name == "attack")
		{
			is_attacking = false;
		}
	}

	private void Debug()
{
    var richTextLabel = GetNodeOrNull<RichTextLabel>("RichTextLabel");
    
    if (richTextLabel != null)
    {
        richTextLabel.Text = $"Velocity: {Velocity}\nHealth: {healthcomp.CurrentHP}";
    }
    else
    {
        GD.PrintErr("RichTextLabel not found in the scene.");
    }
}

}
