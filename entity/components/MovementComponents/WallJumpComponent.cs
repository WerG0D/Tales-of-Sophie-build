using Godot;
using Godot.Collections;

[GlobalClass]
public partial class WallJumpComponent: Node2D 
{
    [Export] CharacterBody2D Entity;
    [Export] public float wallJumpCooldown = 20;
    public float wallJumpTimer = 0.0f;

    public bool isWallJump { get; private set; }

    public void HandleWallJump()
    {
        	
		if (Entity.Velocity.Y >= 10 && wallJumpTimer < wallJumpCooldown && Entity.IsOnWallOnly())
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

}    
    