using Godot;
using System;

public partial class Tiles : Node
{
	
	public override void _Ready()
	{
		
	}

	
	public void PlayAnimationsInNode(CanvasItem node)
	{
    	foreach (Node child in node.GetChildren())
    	{
        	if (child is AnimatedSprite2D animatedSprite)
        	{
            	animatedSprite.Play("default");  // Play the default animation of the AnimatedSprite2D
        	}
        	else if (child is Node)  // If the child is a node, we call the function recursively
        	{
            PlayAnimationsInNode(child as CanvasItem);
        	}
    	}
	}
}
