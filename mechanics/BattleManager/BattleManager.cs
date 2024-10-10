using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[GlobalClass]
public partial class BattleManager : Node
{
    private enum BattleState { START, PLAYERTURN, ENEMYTURN, WIN, LOSS }
    private BattleState state;

    private Player player;
    private List<skeleton> enemies;

    public override void _Ready()
    {
        // Inicializando o combate
        state = BattleState.START;
        SetupBattle();
    }

    private void SetupBattle()
    {
        GD.Print("Iniciando o combate...");
        player = GetNode<Player>("Player");
        enemies = new List<skeleton>(GetNode<Node2D>("Enemies").GetChildren().OfType<skeleton>());


        state = BattleState.PLAYERTURN;
        PlayerTurn();
    }

    private void PlayerTurn()
    {
        GD.Print("Turno do jogador...");
        // Aqui o jogador escolhe entre ataque, defesa, etc.
        // Implementação de escolha de ação
        // Quando uma ação for selecionada:
        // ExecutePlayerAction();
    }

    private void ExecutePlayerAction(string action)
    {
        // Exemplo de ataque
        if (action == "attack")
        {
            StartAttackMinigame();
        }
        else if (action == "defend")
        {
            StartDefenseMinigame();
        }
    }

    private void EnemyTurn()
    {
        GD.Print("Turno do inimigo...");
        // Implementar lógica de ataque do inimigo e chamar minigame de defesa.
        StartDefenseMinigame();
    }

    private void EndTurn()
    {
        // Lógica de alternância de turnos
        if (state == BattleState.PLAYERTURN)
        {
            state = BattleState.ENEMYTURN;
            EnemyTurn();
        }
        else if (state == BattleState.ENEMYTURN)
        {
            state = BattleState.PLAYERTURN;
            PlayerTurn();
        }
    }

    private void StartAttackMinigame()
    {
        // Chama o minigame de ataque (a ser implementado)
        GD.Print("Iniciando o minigame de ataque...");
        //GetNode<AttackMinigame>("AttackMinigame").StartMinigame();
    }

    private void StartDefenseMinigame()
    {
        // Chama o minigame de defesa (a ser implementado)
        GD.Print("Iniciando o minigame de defesa...");
        //GetNode<DefenseMinigame>("DefenseMinigame").StartMinigame();
    }
}
