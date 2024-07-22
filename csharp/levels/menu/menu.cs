using Godot;
using System;

public partial class Menu : Godot.Control
{
    PackedScene packedScene;
    public override void _Ready()
    {
        GetNode<Button>("VBoxContainer/StartButton").GrabFocus();
        packedScene = (PackedScene)GD.Load("res://levels/tests/tests.tscn");
    }

    // Called when the StartButton is pressed
    public void _on_start_button_pressed()
    {
        Node node = packedScene.Instantiate();
        GetTree().Root.AddChild(node); 
    }

    // Called when the OptionsButton is pressed
    public void _on_OptionsButton_pressed()
    {
        // Do nothing (equivalent to 'pass' in GDScript)
    }

    // Called when the QuitButton is pressed
    public void _on_QuitButton_pressed()
    {
        GetTree().Quit();
    }
}