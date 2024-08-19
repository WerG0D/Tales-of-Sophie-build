using Godot;
using System;

public partial class ParallaxBg : Line2D
{
	public Camera2D camera;
	[Export] public float parallaxvelocityX = 0.1f; //Tem q lembrar q essa budega ta invertida
	[Export] public float parallaxvelocityY = 0.1f;
	public Vector2 camerainit = new Vector2(0, 0);
	
	public override void _Ready()
	{
		camera = GetNode<Camera2D>("../../player/Camera2D");
		camerainit = camera.GlobalPosition;
	}

	
	public override void _Process(double delta)
	{
		while (camera == null) 
		{
			GD.Print("ParallaxBg.cs: Camera is null");
		}
		camera = GetNode<Camera2D>("../../player/Camera2D"); // cada dia mais perto da morte
		SetPointPosition(0, new Vector2(ToLocal(camera.GlobalPosition).X / parallaxvelocityX, ToLocal(camera.GlobalPosition).Y  / parallaxvelocityY -100));
		SetPointPosition(1, new Vector2(ToLocal(camera.GlobalPosition).X / parallaxvelocityX, ToLocal(camera.GlobalPosition).Y  / parallaxvelocityY + 100));
	}
}
