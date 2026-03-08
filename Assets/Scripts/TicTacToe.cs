using Unity.Netcode;
using UnityEngine;
using HelloWorld;
using TMPro;
using UnityEngine.UIElements;

public class TicTacToe : NetworkBehaviour{
    //elemenst to display on screen
    public TicTacToeGrid[] cells;
    public TextMeshProUGUI turnDisplay;
    public TextMeshProUGUI p1ScoreDisplay;
    public TextMeshProUGUI p2ScoreDisplay;
    public TextMeshProUGUI gameOverDisplay;

    //network vars changing based on game
    private NetworkVariable<int> currTurn = new NetworkVariable<int>(1);
    private NetworkVariable<int> p1Score = new NetworkVariable<int>(0);
    private NetworkVariable<int> p2Score = new NetworkVariable<int>(0);
    private NetworkVariable<bool> gameOn = new NetworkVariable<bool>(true);
    private NetworkList<int> boardState; 

    //find the elements to be displayed 
    private void Awake() {
        boardState = new NetworkList<int>(); 
        if (IsServer) {
            turnDisplay = GameObject.Find("turnDisplay").GetComponent<TextMeshProUGUI>();
            p1ScoreDisplay = GameObject.Find("p1ScoreDisplay").GetComponent<TextMeshProUGUI>();
            p2ScoreDisplay = GameObject.Find("p2ScoreDisplay").GetComponent<TextMeshProUGUI>();
            gameOverDisplay = GameObject.Find("gameOverDisplay").GetComponent<TextMeshProUGUI>();
        }
    }

    //object joins network and runs functions when data changes
    public override void OnNetworkSpawn() {
        if (IsServer) {
            InitializeBoard();
        }
        currTurn.OnValueChanged += OnTurnChanged;
        p1Score.OnValueChanged += OnScoreChanged;
        p2Score.OnValueChanged += OnScoreChanged;
        gameOn.OnValueChanged += OnGameOnChanged;
        boardState.OnListChanged += OnBoardStateChanged;
        UpdateUI();
    }

    //object leaves network and runs these functions
    public override void OnNetworkDespawn() {
        currTurn.OnValueChanged -= OnTurnChanged;
        p1Score.OnValueChanged -= OnScoreChanged;
        p2Score.OnValueChanged -= OnScoreChanged;
        gameOn.OnValueChanged -= OnGameOnChanged;
        boardState.OnListChanged -= OnBoardStateChanged;
    }

    //clear the board and hide the game over panel
    public void InitializeBoard() {
        boardState.Clear();
        for (int i = 0; i < 9; i++) {
            boardState.Add(0);
        }
        currTurn.Value = 1;
        gameOn.Value = true;
        HideGameOverPanelRpc();
    }

    //doesnt let wrong player make a move and only lets correct player do so by calling function
    public void OnCellClicked(int clickedIndex) {
        if (!gameOn.Value){
            return;
        }
        int playerNum = GetLocalPlayerNumber();
        if (playerNum != currTurn.Value) {
            return;
        }
        MakeMoveServerRpc(clickedIndex, playerNum);
    }

    //make the move the user requested to be seen on all screens
    [Rpc(SendTo.Server)] 
    private void MakeMoveServerRpc(int cellIndex, int playerNum) {
        if (NetworkManager.Singleton == null || !IsSpawned) {
            Debug.LogWarning("Cant make move");
            return;
        }
        if (!gameOn.Value || boardState[cellIndex] != 0 || currTurn.Value != playerNum) {
            return;
        }

        boardState[cellIndex] = playerNum; //updates board state with move

        //check for all conditions that happenwhen move is made
        if (checkWin(playerNum)) { //win
            gameOn.Value = false;
            if (playerNum == 1) {
                p1Score.Value++;
            } else {
                p2Score.Value++;
            }
            ShowGameOverPanelRpc(playerNum);
        } else if (IsBoardFull()) { //draw
            gameOn.Value = false;
            ShowGameOverPanelRpc(0);
        } else { //next turn
            if (currTurn.Value == 1) {
                currTurn.Value = 2;
            } else {
                currTurn.Value = 1;
            }
        }
    }

    //check for wins
    private bool checkWin(int playerNum) {
        //rows
        if (boardState[0] == playerNum && boardState[1] == playerNum && boardState[2] == playerNum) return true; //top
        if (boardState[3] == playerNum && boardState[4] == playerNum && boardState[5] == playerNum) return true; //middle
        if (boardState[6] == playerNum && boardState[7] == playerNum && boardState[8] == playerNum) return true; //bottom
        //columns
        if (boardState[0] == playerNum && boardState[3] == playerNum && boardState[6] == playerNum) return true; // left
        if (boardState[1] == playerNum && boardState[4] == playerNum && boardState[7] == playerNum) return true; //middle
        if (boardState[2] == playerNum && boardState[5] == playerNum && boardState[8] == playerNum) return true; //right 
        //diagonals
        if (boardState[0] == playerNum && boardState[4] == playerNum && boardState[8] == playerNum) return true; // topleft to bottomright
        if (boardState[2] == playerNum && boardState[4] == playerNum && boardState[6] == playerNum) return true; // topright to bottomleft
        return false;
    }

    //check all cells and see if full board
    private bool IsBoardFull() {
        for (int i = 0; i < 9; i++) {
            if (boardState[i] == 0) {
                return false;
            }
        }
        return true;
    }

    //show game over panel to users
    [Rpc(SendTo.ClientsAndHost)]
    private void ShowGameOverPanelRpc(int winner) {
        string message;
        if (winner == 0) {
            message = "Draw!";
        } else {
            message = $"Player {winner} wins!";
        }    
        HelloWorldManager manager = Object.FindFirstObjectByType<HelloWorldManager>();
        if (manager != null) { //display correct message in the panel
            manager.ShowGameOver(message);
        }
    }

    //get current player
    private int GetLocalPlayerNumber() {
        if (NetworkManager.Singleton.LocalClientId == 0) {
            return 1;
        } else {
            return 2;
        }
    }

    //callback when turn changes
    private void OnTurnChanged(int prevVal, int newVal) {
        UpdateUI();
    }

    //ccallback when score changes
    private void OnScoreChanged(int prevVal, int newVal) {
        UpdateUI();
    }

    //callback when game state changes
    private void OnGameOnChanged(bool prevVal, bool newVal) {
        UpdateUI();
    }

    //callbcak when board state changes
    private void OnBoardStateChanged(NetworkListEvent<int> changeEvent) {
        UpdateUI();
    }

    //show correct ui to player
    private void UpdateUI() {
        int localPlayer = GetLocalPlayerNumber();
        if (gameOn.Value) { //change player turns
            if (currTurn.Value == localPlayer) {
                turnDisplay.text = "Your Turn!";
            } else {
                turnDisplay.text = $"Player {currTurn.Value}'s turn";
            }
        } else {
            turnDisplay.text = "";
        }

        //display correct scores
        p1ScoreDisplay.text = $"Player 1: {p1Score.Value}";
        p2ScoreDisplay.text = $"Player 2: {p2Score.Value}";

        //update cells correctly
        for (int i = 0; i < cells.Length && i < boardState.Count; i++) { 
            cells[i].UpdateCell(boardState[i]);
        }
    }
    
    //make sure netwrokmanager is active before restarting game 
    [Rpc(SendTo.Server)]
    public void RestartGameServerRpc() {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer) {
            InitializeBoard();
        }
    }

    //hide the game over panel 
    [Rpc(SendTo.ClientsAndHost)]
    private void HideGameOverPanelRpc(){
        HelloWorld.HelloWorldManager manager = Object.FindAnyObjectByType<HelloWorld.HelloWorldManager>();
        if (manager != null && manager.gameOverPanel != null) {
            manager.gameOverPanel.SetActive(false);
        }
    }
}
