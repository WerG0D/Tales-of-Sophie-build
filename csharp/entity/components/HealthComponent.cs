using Godot;
using System;
[GlobalClass]
public partial class HealthComponent : Node2D
{
    [Export] public int MAX_HEALTH { get; set; }
    public float CurrentHP;

    [Signal] public delegate void DismemberHeadEventHandler(string compname);
    [Signal] public delegate void DismemberRARMEventHandler(string compname);
    [Signal] public delegate void DismemberLARMEventHandler(string compname);
    [Signal] public delegate void DismemberRLEGEventHandler(string compname);
    [Signal] public delegate void DismemberLLEGEventHandler(string compname);

    [Signal] public delegate void DeathEventHandler();
    [Signal] public delegate void DamagedEventHandler(float amount, Vector2 knockback);

    public override void _Ready()
    {
        CurrentHP = MAX_HEALTH;
    }

    public void HealthReduce(float amount, Vector2 knockback)
    {
        if (Name == "HealthComponentHead")
        {
            CurrentHP -= 2.5f * amount;
            CurrentHP = Math.Max(CurrentHP, 0.0f);
        }
        else if (Name == "HealthComponentBody")
        {
            CurrentHP -= 0.2f * amount;
            CurrentHP = Math.Max(CurrentHP, 0.0f);
        }
        else
        {
            CurrentHP -= amount;
            CurrentHP = Math.Max(CurrentHP, 0.0f);
        }

        if (CurrentHP <= 0.0f)
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