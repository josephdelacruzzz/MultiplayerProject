using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;
using TMPro;

namespace HelloWorld {
    public class HelloWorldManager : MonoBehaviour{

        public GameObject ticTacToeGamePrefab; //prefab for board
        public TextMeshProUGUI statusLabel; //text showing if p1 or p2
        public GameObject gameOverPanel; //panel displayed when game ends
        public GameObject buttonsPanel; //panel for selecting p1 or p2
        public GameObject startScreenBackground; //background for when game starts
        private bool gameOnScreen = false; //if game was spawned

        //Player1 button is clicked and sets up address and port
        public void StartHost() {
            var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
            transport.ConnectionData.Address = "0.0.0.0"; 
            transport.ConnectionData.Port = 7777;
            transport.ConnectionData.ServerListenAddress = "0.0.0.0";
            if (NetworkManager.Singleton.StartHost()) { //remove buttons and start screen 
                if (buttonsPanel != null) { 
                    buttonsPanel.SetActive(false);
                }
                if (startScreenBackground != null) {
                    startScreenBackground.SetActive(false); 
                }
            }
        }

        //Player2 button is clicked  and sets up address and port
        public void StartClient() {
            var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
            transport.ConnectionData.Address = "192.168.1.213"; //hard coded ip
            transport.ConnectionData.Port = 7777;
            if (NetworkManager.Singleton.StartClient()) { //remove buttons and start screen 
                if (buttonsPanel != null) {
                    buttonsPanel.SetActive(false);
                }
                if (startScreenBackground != null) {
                    startScreenBackground.SetActive(false); 
                }
            }
        }

        //hides game over panel
        void Start(){
            if (gameOverPanel != null) {
                gameOverPanel.SetActive(false);
            }
        }

        //essential checks before spawing game
        void Update() {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer 
                && !gameOnScreen && ticTacToeGamePrefab != null) {
                spawnTicTacToeGame(); 
            }
            UpdateStatusText();
        }

        //spawns the game
        void spawnTicTacToeGame() {
            GameObject game = Instantiate(ticTacToeGamePrefab); //instantiate game
            NetworkObject networkObject = game.GetComponent<NetworkObject>(); //instantiate gameobj

            //spawn the game and show on screen
            if (networkObject != null){
                networkObject.Spawn(); 
                gameOnScreen = true;
            }
        }

        void UpdateStatusText() {
            if (NetworkManager.Singleton == null) {
                return;
            }

            //determine which player
            string playingMode;
            if (NetworkManager.Singleton.IsHost) {
                playingMode = "Player 1 (X)";
            } else if (NetworkManager.Singleton.IsClient) {
                playingMode = "Player 2 (O)";
            } else {
                playingMode = "Server";
            }

            //display current status of player or not connected
            if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer) {
                statusLabel.text = $"Playing as: {playingMode}";
            } else {
                statusLabel.text = "Not connected";
            }
        }

        //display the game over panel with correct endgame message
        public void ShowGameOver(string message) {
            if (gameOverPanel != null) {
                gameOverPanel.SetActive(true); //show the panel
                var textComponent = gameOverPanel.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null) {
                    textComponent.text = message;  //set the message
                }
            }
        }

        public void RestartGame() {
            //first hide the game over panel
            if (gameOverPanel != null) {
                gameOverPanel.SetActive(false);
            }
            
            //retstart the game
            TicTacToe game = Object.FindAnyObjectByType<TicTacToe>();
            if (game != null) {
                game.RestartGameServerRpc();
            }
        }
    }
}