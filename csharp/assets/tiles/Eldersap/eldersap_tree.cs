using Godot;
using System;

public partial class eldersap_tree : Node2D
{
	public string state = "no sap";
	public bool player_in_area = false;
	public PackedScene eldersap = GD.Load<PackedScene>("res://assets/tiles/Eldersap/eldersap_collectable.tscn");
	
	public override void _Ready()
	{
		if (state == "no sap")
		{
			GetNode<Timer>("grow_timer").Start();
		}
	}

	public override void _Process(double delta)
	{
		if (state == "sap")
		{
			GetNode<AnimatedSprite2D>("AnimatedSprite2D").Play("default");
			if (player_in_area && Input.IsActionJustPressed("collect"))
			{
				state = "no sap";
				DropElderSap();
			}
		}
		else
		{
			GetNode<AnimatedSprite2D>("AnimatedSprite2D").Play("nosap");
		}
	}
	public async void DropElderSap()
	{
		var instance = eldersap.Instantiate<StaticBody2D>();
		instance.GlobalPosition = GetNode<Marker2D>("Marker2D").GlobalPosition;
		GetParent().AddChild(instance);

		await ToSignal(GetTree().CreateTimer(3), "timeout");
		GetNode<Timer>("grow_timer").Start();
	}
	public void OnGrowthTimerTimeout() 
	{
		if (state == "no sap") {state = "sap";}
	}

	public void OnPickableAreaEntered(CharacterBody2D body)
	{
		if (body.HasMethod("player"))
		{
			player_in_area = true;
		}
		else {return;}
	}
	public void OnPickableAreaExited(CharacterBody2D body)
	{
		if (body.HasMethod("player"))
		{
			player_in_area = false;
		}
		else {return;}
	}
}
