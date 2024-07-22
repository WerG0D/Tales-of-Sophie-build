using Godot;
using System;

public partial class Control : Godot.Control
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GetNode<Button>("VBoxContainer/StartButton").GrabFocus();
        Level levelScene = GD.Load<tests>("res://levels/tests/tests.tscn");
    }

    // Called when the StartButton is pressed
    public void _on_StartButton_pressed()
    {
        var menu = levelScene = levelScene.Instantiate();  
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