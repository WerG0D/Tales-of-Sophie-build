using Godot;
using System;

public partial class player : CharacterBody2D
{
	public Camera2D camera;
	public  HealthComponent healthcomphead;
    public  HealthComponent healthcompbody;
    public  HealthComponent healthcompRightArm;
    public  HealthComponent healthcompLeftArm;
    public  HealthComponent healthcompRightLeg;
    public  HealthComponent healthcompLeftLeg;
    public  AnimationPlayer animplayer;
    public  bool initialized = false;
	
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

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
