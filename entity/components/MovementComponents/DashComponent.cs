using Godot;
using Godot.Collections;

[GlobalClass]
public partial class DashComponent: Node2D 
{
    [Export] CharacterBody2D Entity;
    [Export] VelocityComponent VelocityComponent;
    [Export] public float DashSpeed = 100f;
    [Export] public float DashDuration = 0.2f;
    public float wallJumpTimer = 0.0f;

    public bool isWallJump { get; private set; }
    public bool isDash { get; private set; }

    public void HandleDash()
    {
        var timer = Entity.GetNode<Timer>("DashTimer");

        if (Input.IsActionJustPressed("dash") && timer.IsStopped())
		{
			timer.WaitTime = DashDuration;
		    timer.OneShot = true;
		    timer.Start();
		}
		if (!timer.IsStopped())
		{
			isDash = true;

			VelocityComponent.ApplyDashSpeed(DashSpeed);
		}
		else
		{
			isDash = false;
		}
    }
    



}    
    