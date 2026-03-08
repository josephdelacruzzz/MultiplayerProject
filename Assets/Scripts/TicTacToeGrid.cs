using Unity.Netcode;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TicTacToeGrid : NetworkBehaviour {
    public int cellIndex; //cell's pos on board
    public Button button; //button detecting clicks
    public TextMeshProUGUI cellText; //text for X O or empty
    private TicTacToe gameManager; //reference to logic controler

    private void Awake() {
        if (button == null) { //get button if not assigned
            button = GetComponent<Button>();
        }
        if (cellText == null) { //gets component to didsplay text
            cellText = GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    private void Start() {
        //find references for cell and button
        if (button == null) {
            button = GetComponent<Button>();
        }
        if (cellText == null) {
            cellText = GetComponentInChildren<TextMeshProUGUI>();
        }

        if (button == null) {
            Debug.LogError("no button found on " + gameObject.name);
            return; //return so program doesnt crash
        }
    
        //find the gamemanager and registers the click listener
        gameManager = Object.FindFirstObjectByType<TicTacToe>();
        button.onClick.AddListener(OnCellClicked);
    }

    //called when cell is clicked and tells gamemanager
    private void OnCellClicked() {
        if(gameManager != null) {
            gameManager.OnCellClicked(cellIndex);
        }
    }

    //change the text inside each cell based on input
    public void UpdateCell(int state) {
        switch (state) {
            case 0:
                cellText.text = ""; //blank and can be chosen
                button.interactable = true;
                break;
            case 1:
                cellText.text = "X"; //X P1 and cannot be chosen
                button.interactable = false;
                break;
            case 2:
                cellText.text = "O"; //O P2 and cannot be chosen
                button.interactable = false;
                break;
        }
    }

    //sets which part of baord corresponds to this cell
    public void setCellIndex(int index) {
        cellIndex = index;
    }

    //allows the button to be interactable once network is ready
    public override void OnNetworkSpawn() {
        if (button != null) {
            button.interactable = true;
        }
    }
}
