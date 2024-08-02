using Godot;
using System;

public partial class PlayerCamera2D : Camera2D
{
    public float DEAD_ZONE { get; private set; }


    public override void _Ready()
    {
        DEAD_ZONE = 1;
        base._Ready();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion eventMouseMotion)
        {
            Viewport viewport = GetViewport();
            Vector2 _target = eventMouseMotion.Position - viewport.GetVisibleRect().Size * 0.5f;
            if (_target.Length() < DEAD_ZONE)
            {
                Position = new Vector2(0, 0);
            }
            else
            {
                Position = _target.Normalized() * (_target.Length() - DEAD_ZONE) * 0.1f;
            }
        }
    }
}