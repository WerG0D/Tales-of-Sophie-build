using Godot;
using System;
[GlobalClass]
public partial class HealthComponent : Node2D
{
    [Export] public int MAX_HEALTH { get; set; }
    private float _current;

    [Signal] delegate void DismemberHead(string compname);
    [Signal] delegate void DismemberRARM(string compname);
    [Signal] delegate void DismemberLARM(string compname);
    [Signal] delegate void DismemberRLEG(string compname);
    [Signal] delegate void DismemberLLEG(string compname);

    [Signal] delegate void Death();
    [Signal] delegate void Damaged(float amount, Vector2 knockback);

    public override void _Ready()
    {
        _current = MAX_HEALTH;
    }

    public void HealthReduce(float amount, Vector2 knockback)
    {
        if (Name == "HealthComponentHead")
        {
            _current -= 2.5f * amount;
            _current = Math.Max(_current, 0.0f);
        }
        else if (Name == "HealthComponentBody")
        {
            _current -= 0.2f * amount;
            _current = Math.Max(_current, 0.0f);
        }
        else
        {
            _current -= amount;
            _current = Math.Max(_current, 0.0f);
        }

        if (_current <= 0.0f)
        {
            if (Name == "HealthComponentHead")
            {
                EmitSignal(nameof(DismemberHead), Name);
            }
            else if (Name == "HealthComponentLeftArm")
            {
                EmitSignal(nameof(DismemberLARM), Name);
            }
            else if (Name == "HealthComponentRightArm")
            {
                EmitSignal(nameof(DismemberRARM), Name);
            }
            else if (Name == "HealthComponentLeftLeg")
            {
                EmitSignal(nameof(DismemberLLEG), Name);
            }
            else if (Name == "HealthComponentRightLeg")
            {
                EmitSignal(nameof(DismemberRLEG), Name);
            }
            else
            {
                EmitSignal(nameof(Death));
            }
        }
        else
        {
            EmitSignal(nameof(Damaged), amount, knockback);
        }
    }
}