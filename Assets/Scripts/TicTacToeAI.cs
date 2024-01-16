using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Threading.Tasks;

public enum TicTacToeState{none, cross, circle}

[System.Serializable]
public class WinnerEvent : UnityEvent<int>
{
}

public class TicTacToeAI : MonoBehaviour
{

	int _aiLevel;

    TicTacToeState[,] boardState;

    [SerializeField] 
	private bool _isPlayerTurn = true;
    
	[SerializeField]
	private int _gridSize = 3;

    [SerializeField]
	private TicTacToeState playerState = TicTacToeState.circle;
    TicTacToeState aiState = TicTacToeState.cross;

	[SerializeField]
	private GameObject _xPrefab;

	[SerializeField]
	private GameObject _oPrefab;

	public UnityEvent onGameStarted;

	//Call This event with the player number to denote the winner
    public WinnerEvent onPlayerWin;
	
	ClickTrigger[,] _triggers;
	
	private void Awake()
	{
        if(onPlayerWin == null){
            onPlayerWin = new WinnerEvent();
		}
	}

    public void StartAI(int AILevel){
		_aiLevel = AILevel;
		StartGame();
	}

	// Function to register the click trigger
	public void RegisterTransform(int myCoordX, int myCoordY, ClickTrigger clickTrigger)
	{
        _triggers[myCoordX, myCoordY] = clickTrigger;
	}

	private void StartGame()
	{
        _triggers = new ClickTrigger[3,3];
		onGameStarted.Invoke();

        InitializeBoard();

        if (!_isPlayerTurn){
            EasyAI();
        }
    }

	// Function to initialize the starting board
    private void InitializeBoard()
    {
        boardState = new TicTacToeState[_gridSize, _gridSize];

        for (int i = 0; i < _gridSize; i++)
        {
            for (int j = 0; j < _gridSize; j++)
            {
                boardState[i, j] = TicTacToeState.none;
            }
        }
    }

	// Function to set the visual of the square
	private void SetVisual(int coordX, int coordY, TicTacToeState targetState)
	{
		Instantiate(
			targetState == TicTacToeState.circle ? _oPrefab : _xPrefab,
            _triggers[coordX, coordY].transform.position,
			Quaternion.identity
		);
	}

	/*==================== THE PLAYER FUNCTIONS ====================*/
	// Function to select a square for the player
    public async void PlayerSelects(int coordX, int coordY){
        if (_isPlayerTurn){
            // Update the board state
            boardState[coordX, coordY] = playerState;

            // Disable the click trigger
            _triggers[coordX, coordY].SetInputEndabled(false);

            SetVisual(coordX, coordY, playerState);

            _isPlayerTurn = false;

            // Check for win
			// 0 for human
            if (CheckForWin(playerState)){
				await Task.Delay(500);
                onPlayerWin.Invoke(0); 
                Debug.Log("Player won!");
            }
			// -1 for a tie
            else if (!EmptySquaresExist()){
				await Task.Delay(500);
                onPlayerWin.Invoke(-1);
            }
            else{
                EasyAI();
            }
        }
    }

	/*==================== THE AI FUNCTIONS ====================*/
	// Logic: The AI will select a random square
	private void EasyAI() 
	{
		List<int> moves = GetAvailableMoves();
		
		// 50% (exclusive) chance to make a random move
		if (UnityEngine.Random.value < 0.5f) {
			int index = UnityEngine.Random.Range(0, moves.Count);
			int square = moves[index];
			AiSelects(square / _gridSize, square % _gridSize);
		}

		// 50% (inclusive) chance to make a strategic move
		else {
			MakeStrategicMove(); 
		}
	}

	// Returns a list of valid moves 
	public List<int> GetAvailableMoves()
    {
        List<int> moves = new List<int>();

        for (int i = 0; i < _gridSize * _gridSize; i++)
        {
            int row = i / _gridSize; // Get the row index
            int col = i % _gridSize; // Get the column index

            if (boardState[row, col] == TicTacToeState.none)
            {
                moves.Add(i);
            }
        }

        return moves;
    }

	// Function to select a square for the AI
    public async void AiSelects(int coordX, int coordY){
		// Simmulate AI thinking delay
        await Task.Delay(500); 

        boardState[coordX, coordY] = aiState;

        _triggers[coordX, coordY].SetInputEndabled(false);

        SetVisual(coordX, coordY, aiState);

        _isPlayerTurn = true;

        if (CheckForWin(aiState)){
			await Task.Delay(500);
            onPlayerWin.Invoke(1);
        }
        else if (!EmptySquaresExist()){
			await Task.Delay(500);
            onPlayerWin.Invoke(-1);
        }
	}

	// Function to make a strategic move
	private void MakeStrategicMove()
	{
		// Check if the AI can win in the next move
		List<int> moves = GetAvailableMoves();
		foreach (int move in moves) {
			int row = move / _gridSize;
			int col = move % _gridSize;
			boardState[row, col] = aiState;
			if (CheckForWin(aiState)) {
				AiSelects(row, col);
				return;
			}
			boardState[row, col] = TicTacToeState.none;
		}

		// Check if the player can be blocked in next move
		foreach (int move in moves) {
			int row = move / _gridSize;
			int col = move % _gridSize;
			boardState[row, col] = playerState;
			if (CheckForWin(playerState)) {
				AiSelects(row, col);
				return;
			}
			boardState[row, col] = TicTacToeState.none;
		}

		// Otherwise make a random move
		int index = UnityEngine.Random.Range(0, moves.Count);
		int square = moves[index];
		AiSelects(square / _gridSize, square % _gridSize);
	}

	/*==================== GAME FUNCTIONS ====================*/
	// Check if there are any empty squares
    public bool EmptySquaresExist()
    {
        int _gridSize = boardState.GetLength(0);

        for (int i = 0; i < _gridSize; i++)
        {
            for (int j = 0; j < _gridSize; j++)
            {
                if (boardState[i, j] == TicTacToeState.none)
                {
                    return true;
                }
            }
        }

        return false;
    }

    // Method to check if a player has won the game
    private bool CheckForWin(TicTacToeState playerState)
    {
        // Check rows
        for (int row = 0; row < _gridSize; row++)
        {
            if (boardState[row, 0] == playerState &&
                boardState[row, 1] == playerState &&
                boardState[row, 2] == playerState)
            {
                return true;
            }
        }

        // Check columns
        for (int col = 0; col < _gridSize; col++)
        {
            if (boardState[0, col] == playerState &&
                boardState[1, col] == playerState &&
                boardState[2, col] == playerState)
            {
                return true;
            }
        }

        // Check diagonals
        if (boardState[0, 0] == playerState &&
            boardState[1, 1] == playerState &&
            boardState[2, 2] == playerState)
        {
            return true;
        }

        if (boardState[0, 2] == playerState &&
            boardState[1, 1] == playerState &&
            boardState[2, 0] == playerState)
        {
            return true;
        }

        return false;
    }
}