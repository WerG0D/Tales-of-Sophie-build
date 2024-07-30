using Godot;
using System;

public partial class eldersap_collectable : StaticBody2D
{
	public const float SHINE_TIME = 1.0f;
	public override void _Ready()
	{
	}


	public async void FallFromTree()
	{
		GetNode<AnimatedSprite2D>("AnimatedSprite2D").Play("sapfall");
		await ToSignal(GetTree().CreateTimer(1.5), "timeout");
		GD.Print("ElderSap fell, +1 ElderSap");
	}
}
