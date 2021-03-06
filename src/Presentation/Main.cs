using Godot;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using IsometricGame.Presentation;

[SceneReference("Main.tscn")]
public partial class Main : Node
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        RemoveChild(this.dungeon);
        RemoveChild(this.lobby);
    }

    public void LoginSuccess()
    {
        this.menu.LoginSuccess();
    }

    public void IncorrectLogin()
    {
        this.menu.IncorrectLogin();
    }

    public void LobbyCreated(string lobbyId)
    {
        this.menu.LobbyCreated(lobbyId);
    }

    public void LobbyNotFound(string lobbyId)
    {
        this.menu.LobbyNotFound();
    }

    public void YouJoinedToLobby(bool creator, string lobbyId, string playerName)
    {
        RemoveChild(this.menu);
        AddChild(this.lobby);
        this.lobby.YouJoinedToLobby(creator, lobbyId, playerName);
    }

    public void PlayerJoinedToLobby(string playerName)
    {
        this.lobby.PlayerJoinedToLobby(playerName);
    }

    public void PlayerLeftLobby(string playerName)
    {
        this.lobby.PlayerLeftLobby(playerName);
    }

    public void YouLeftLobby()
    {
        RemoveChild(this.lobby);
        AddChild(this.menu);
    }

    public void GameStarted()
    {
        RemoveChild(this.lobby);
        AddChild(this.dungeon);

        this.dungeon.NewGame(this.menu.SelectedTeam);
    }

    public async void TurnDone(TransferTurnData turnData)
    {
        await this.dungeon.TurnDone(turnData);
        if (turnData.GameOverLoose || turnData.GameOverWin)
        {
            RemoveChild(this.dungeon);
            AddChild(this.menu);
            this.menu.GameOver();
        }
    }

    public void Initialize(TransferInitialData initialData)
    {
        this.dungeon.Initialize(initialData);
    }

    public void ConfigSynced(bool fullMapVisible, bool turnTimeoutEnaled, float turnTimeoutValue, MapGeneratingType mapType)
    {
        this.lobby.ConfigSynced(fullMapVisible, turnTimeoutEnaled, turnTimeoutValue, mapType);
    }
}
