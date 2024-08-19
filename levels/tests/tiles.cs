using Godot;
using System;

public partial class Tiles : Node2D
{
	
	public override void _Ready()
	{
		PlayAnimationsInNode(this);
	}

	
	public void PlayAnimationsInNode(CanvasItem node)
	{
    	foreach (Node child in node.GetChildren())
    	{
        	if (child is AnimatedSprite2D animatedSprite)
        	{
            	animatedSprite.Play("default");
        	}
        	else if (child is Node)
        	{
            PlayAnimationsInNode(child as CanvasItem);
        	}
    	}
	}
}
