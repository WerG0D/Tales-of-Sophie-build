using Godot;
using System;

public partial class Menu : Godot.Control
{
    PackedScene packedScene;
    public override void _Ready()
    { 
        GetNode<Button>("VBoxContainer/StartButton").GrabFocus();
        packedScene = (PackedScene)ResourceLoader.Load("res://levels/tests/tests.tscn");
    }


    public void OnStartPressed()
    {
        GetTree().ChangeSceneToPacked(packedScene);
    }

    public void _on_OptionsButton_pressed()
    {
    }

    public void _on_QuitButton_pressed()
    {
        GetTree().Quit();
    }
}