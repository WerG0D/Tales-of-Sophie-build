using Godot;
using System;

[GlobalClass]
public partial class ChainComponent : Node2D
{
    [Export] public int MaxChainLength = 800; // Comprimento máximo da corrente
    [Export] public int HookSpeed = 150; // Velocidade de movimento do gancho, basicamente o quão rápido ele vai ser disparado e se conectar a parede
    [Export] public int PullForce = 60; // Força de puxão do gancho, recomendo fortemente deixar em 60, quanto maior mais forte o personagem vai ser puxado
    [Export] public string HookInputAction = "hook"; // O input map pra disparar o gancho. Nn sei se tem uma maneira melhor de fzr isso mas blz
    [Export] public float SwingInfluence = 2f; // Influência do jogador no balanço
	[Export] public CharacterBody2D Entity; 
    [Export] public VelocityComponent VelocityComponent; 

    public Vector2 HookDirection = Vector2.Zero; 
    public Vector2 HookTipPosition = Vector2.Zero;

    public bool IsFlying = false; //dumb code boolean lolz isso com certeza vai dar algum pau no futuro mas fzr oq a gnt ama ficar trackeando booleano
    public bool IsHooked = false; 

    private Vector2 ChainVelocity = Vector2.Zero; 

    public override void _Ready()
    {

    }

    public override void _Process(double delta)
    {
        Visible = IsFlying || IsHooked;
        if (!Visible) return;

        var hookTipLocalPosition = ToLocal(HookTipPosition);

        // Alinha a rotação da ponta do gancho, pq tem que subtrair 90 graus? não sei, mas funciona
        GetNode<RigidBody2D>("Tip").Rotation = Position.AngleToPoint(hookTipLocalPosition) - Mathf.DegToRad(90);

        GetNode<Line2D>("Line2D").SetPointPosition(0, hookTipLocalPosition);

        // Libera o gancho se ele exceder o comprimento máximo da corrente
        if (hookTipLocalPosition.DistanceTo(GetNode<Line2D>("Line2D").GetPointPosition(1)) > MaxChainLength)
        {
            ReleaseHook();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        // Atualiza a posição da ponta do gancho
        GetNode<RigidBody2D>("Tip").GlobalPosition = HookTipPosition;

        if (IsFlying)
        {
            if (GetNode<RigidBody2D>("Tip").MoveAndCollide(HookDirection * HookSpeed) != null)
            {
                IsHooked = true;
                IsFlying = false;
            }
            HookTipPosition = GetNode<RigidBody2D>("Tip").GlobalPosition;
        }
    }

    public void ShootHook(Vector2 direction)
    {
        HookDirection = direction.Normalized();
        IsFlying = true;
        HookTipPosition = GlobalPosition;
    }

    public void ReleaseHook()
    {
        IsFlying = false;
        IsHooked = false;
    }

    public void ActivateHook()
    {
        HandleHookInput(HookInputAction);
        ApplyHookPhysics();
    }

    private void HandleHookInput(string hookAction)
    {
        if (Input.IsActionJustPressed(hookAction) && !IsHooked && !IsFlying)
        {
            var mouseViewportPosition = GetViewport().GetMousePosition();
            ShootHook((mouseViewportPosition - GetViewportRect().Size / 2).Normalized());
        }
        else if (Input.IsActionJustPressed(hookAction) || (Input.IsActionJustPressed("jump") && IsHooked))
        {
            ReleaseHook();
            Entity.Velocity *= 1.2f; // Adiciona impulso ao personagem ao liberar o gancho
        }
    }

    private void ApplyHookPhysics()
    {
        if (IsHooked)
        {
			Vector2 directionToHook = (HookTipPosition - Entity.GlobalPosition).Normalized();
            float distanceToHook = Entity.GlobalPosition.DistanceTo(HookTipPosition);
            
			float calculatedPullForce = HookSpeed * Mathf.Clamp(distanceToHook / MaxChainLength, 1.0f, PullForce);

            Vector2 pullDirection = ToLocal(HookTipPosition).Normalized();
            Entity.Velocity += pullDirection * calculatedPullForce;

			// Força de tensão da corda (para manter o personagem pendurado)
        	Vector2 tensionForce = directionToHook * PullForce * (distanceToHook / MaxChainLength);
			Vector2 gravityEffect = new Vector2(0, 9.8f);

            Vector2 swingForce = new Vector2();
            if (Input.IsActionPressed("move_right"))
            {
                swingForce = new Vector2(SwingInfluence, 0);
            }
            else if (Input.IsActionPressed("move_left"))
            {
                swingForce = new Vector2(-SwingInfluence, 0);
            }

            // A combinação de forças cria o movimento pendular
            Entity.Velocity += tensionForce + gravityEffect + swingForce;

        }
        else
        {
            ChainVelocity = Vector2.Zero;
        }
    }
}
