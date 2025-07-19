using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.VFX;
using static UnityEngine.Rendering.DebugUI.Table;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static void ClearMoveTrails()
    {
        foreach (GameObject trailObj in trailList)
        {
            if (trailObj != null)
            {
                Destroy(trailObj);
            }
        }
        trailList.Clear();
    }

    // En passant tracking
    public static int enPassantCol = -1;  // Column of pawn that just moved two squares
    public static int enPassantRow = -1;  // Row where capturing pawn should move to
    public static bool enPassantPossible = false;

    // Piece values for evaluation
    private static readonly Dictionary<char, int> pieceCosts = new Dictionary<char, int>
    {
        {'p', -1},   // Black pawn
        {'n', -3},   // Black knight
        {'b', -3},   // Black bishop
        {'r', -5},   // Black rook
        {'q', -9},   // Black queen
        {'k', -10000}, // Black king (practically infinite)
        {'P', 1},    // White pawn
        {'N', 3},    // White knight
        {'B', 3},    // White bishop
        {'R', 5},    // White rook
        {'Q', 9},    // White queen
        {'K', 10000}  // White king (practically infinite)
    };

    public bool playAsWhite = true;
    public static bool playAsWhiteStatic;

    public int minimaxDepth = 5;

    public Color lightCol;
    public Color darkCol;

    public Color lightPiece;
    public Color darkPiece;
    public Color placeColor;
    public static Color StaticPlaceColor;
    public Color captureColor;
    public static Color StaticCaptureColor;
    public GameObject tile;
    public GameObject button;
    private SpriteRenderer sr;
    public static int move = 0;

    public TextMeshProUGUI message;

    // Timer-related variables
    public TextMeshPro whiteTime;
    public TextMeshPro blackTime;
    public int gameTime = 10; // Initial time in minutes
    private float whiteTimeRemaining;
    private float blackTimeRemaining;
    private bool isTimerRunning = false;
    private Coroutine whiteTimerCoroutine;
    private Coroutine blackTimerCoroutine;

    public GameObject darkSquare;

    //private Camera cam;
    //public float rotTime = 1.5f;
    //private Vector3 rotVelocity = Vector2.zero;
    //float camRot = 0f;
    private char[,] staleBoard = {
        {' ', ' ', ' ', ' ', ' ', ' ', 'k', 'r'},

        {'p', ' ', ' ', ' ', ' ', ' ', ' ', 'p'},

        {' ', ' ', ' ', ' ', ' ', ' ', ' ', ' '},

        {' ', 'P', ' ', ' ', ' ', 'p', 'q', ' '},

        {' ', ' ', ' ', ' ', ' ', ' ', ' ', ' '},

        {' ', ' ', ' ', ' ', ' ', 'K', ' ', ' '},

        {' ', 'r', ' ', ' ', ' ', ' ', ' ', ' '},

        {' ', ' ', ' ', ' ', ' ', ' ', ' ', ' '},
    };

    private char[,] enPassantBoard = {
        { ' ', 'r', 'b', ' ', 'k', 'b', ' ', 'r'},

        { 'p', ' ', ' ', 'p', 'q', 'p', 'p', 'p'},

        { ' ', ' ', 'p', ' ', ' ', ' ', 'n', ' '},

        { ' ', ' ', 'P', ' ', ' ', ' ', ' ', ' '},

        { ' ', 'p', ' ', ' ', ' ', 'P', ' ', ' '},

        { ' ', ' ', ' ', 'Q', ' ', ' ', ' ', ' '},

        { 'P', ' ', 'P', ' ', ' ', ' ', 'P', 'P'},

        { 'R', 'N', 'B', ' ', ' ', 'R', ' ', 'K'},
    };
    public static char[,] chessBoard = {
        { 'r', 'n', 'b', 'q', 'k', 'b', 'n', 'r'},

        { 'p', 'p', 'p', 'p', 'p', 'p', 'p', 'p'},

        { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' '},

        { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' '},

        { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' '},

        { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' '},

        { 'P', 'P', 'P', 'P', 'P', 'P', 'P', 'P'},

        { 'R', 'N', 'B', 'Q', 'K', 'B', 'N', 'R'}

    };
    // Store the initial board state for opening detection
    private static readonly char[,] initialBoardWhite = {
        { 'r', 'n', 'b', 'q', 'k', 'b', 'n', 'r'},

        { 'p', 'p', 'p', 'p', 'p', 'p', 'p', 'p'},

        { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' '},

        { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' '},

        { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' '},

        { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' '},

        { 'P', 'P', 'P', 'P', 'P', 'P', 'P', 'P'},

        { 'R', 'N', 'B', 'Q', 'K', 'B', 'N', 'R'}
    };

    private static readonly char[,] initialBoardBlack = {
        { 'r', 'n', 'b', 'k', 'q', 'b', 'n', 'r'},

        { 'p', 'p', 'p', 'p', 'p', 'p', 'p', 'p'},

        { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' '},

        { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' '},

        { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' '},

        { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' '},

        { 'P', 'P', 'P', 'P', 'P', 'P', 'P', 'P'},

        { 'R', 'N', 'B', 'K', 'Q', 'B', 'N', 'R'}
    };

    public GameObject pawn;
    public GameObject rook;
    public GameObject knight;
    public GameObject bishop;
    public GameObject queen;
    public GameObject king;

    public static GameObject placeButton;
    public static GameObject currentButton;
    public static GameObject trail;

    public GameObject placeButtonSprite;
    public GameObject currentButtonSprite;
    public GameObject trailSprite;


    public static List<GameObject> trailList = new List<GameObject>();
    public static List<GameObject> positionList = new List<GameObject>();

    public static int prexY;
    public static int prevX;
    public static GameObject[,] pieceList = new GameObject[8, 8];

    public GameObject elseSquare;
    public static bool isMoving = false;

    public static bool whiteKingMoved = false;
    public static bool blackKingMoved = false;
    public static bool whiteLeftRookMoved = false;
    public static bool blackLeftRookMoved = false;
    public static bool whiteRightRookMoved = false;
    public static bool blackRightRookMoved = false;

    // Add pool for place buttons
    private Queue<GameObject> placeButtonPool = new Queue<GameObject>();
    private const int INITIAL_POOL_SIZE = 32;  // Maximum possible moves for any piece

    public static bool gameOver = false;
    private static string winnerMessage = "";
    public GameObject checkSquare;
    private GameObject currentCheckSquare;
    private bool lastWhiteInCheck = false;
    private bool lastBlackInCheck = false;

    public AudioClip placeSound;
    public AudioClip captureSound;
    public AudioClip notifySound;

    private void Awake()
    {
        placeButton = placeButtonSprite;
        currentButton = currentButtonSprite;
        trail = trailSprite;
        Application.targetFrameRate = 50;
    }
    void Start()
    {
        playAsWhite = MenuManager.playAsWhite;
        gameTime = MenuManager.gameTime;
        message.text = winnerMessage;
        darkSquare.SetActive(false);
        elseSquare.SetActive(false);
        playAsWhiteStatic = playAsWhite;
        StaticCaptureColor = captureColor;
        StaticPlaceColor = placeColor;
        System.Array.Copy(playAsWhite ? initialBoardWhite : initialBoardBlack, chessBoard, initialBoardWhite.Length);
        sr = tile.GetComponent<SpriteRenderer>();
        InitializePlaceButtonPool();
        CreateBoard();
        InitPieces();
        InitializePlaceButtonPool();

        Restart();
    }
    private bool isBotThinking = false;
    private bool waitingForNextFrame = false;
    private void Update()
    {
        if (gameOver)
        {
            // When game is over, deactivate the selection square and return
            message.text = winnerMessage;
            darkSquare.SetActive(true);
            if (elseSquare.activeSelf)
            {
                elseSquare.SetActive(false);
            }
            return;

        }

        if (isMoving)
        {
            if (elseSquare.activeSelf == false)
            {
                elseSquare.SetActive(true);
            }
        }
        else if (elseSquare.activeSelf == true)
        {
            elseSquare.SetActive(false);
        }

        // Bot's turn (black pieces)
        if ((move % 2 == 1 == playAsWhite) && !isBotThinking)
        {
            Refresh();
            if (!waitingForNextFrame)
            {
                // First frame after player's move, let UI updates complete
                waitingForNextFrame = true;
                StartCoroutine(WaitForUIUpdate());
                return;
            }
        }
        else if ((move % 2 == 0 == playAsWhite) && !isBotThinking && !isMoving) // Player's turn (white pieces)
        {
            // Check if player has any legal moves
            bool hasLegalMoves = false;
            for (int row = 0; row < 8 && !hasLegalMoves; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (char.IsUpper(chessBoard[row, col])) // White piece
                    {
                        var moves = GetLegalMoves(row, col, chessBoard);
                        foreach (var move in moves)
                        {
                            // Create a temporary board and make the move
                            char[,] tempBoard = new char[8, 8];
                            System.Array.Copy(chessBoard, tempBoard, chessBoard.Length);
                            MakeMove(move.fromRow, move.fromCol, move.toRow, move.toCol, tempBoard);

                            // Check if the move is legal (doesn't leave king in check)
                            if (!IsInCheck(true, tempBoard))
                            {
                                hasLegalMoves = true;
                                break;
                            }
                        }
                        if (hasLegalMoves) break;
                    }
                }
            }

            if (!hasLegalMoves)
            {
                if (IsInCheck(true, chessBoard))
                {
                    gameOver = true;
                    winnerMessage = playAsWhite ? "Checkmate!\nBlack wins!" : "Checkmate!\nWhite wins!";
                }
                else
                {
                    gameOver = true;
                    winnerMessage = "Stalemate!\nGame is a draw.";
                }
                return;
            }
        }
        else
        {
            waitingForNextFrame = false;
        }

        ManageCheckIndicator();
    }

    private Coroutine botMoveCoroutine;

    private IEnumerator WaitForUIUpdate()
    {
        // Wait for two frames to ensure all UI elements are updated
        yield return null;
        yield return null;

        // Force layout rebuild
        var canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(canvas.transform as RectTransform);
        }

        waitingForNextFrame = false;
        isBotThinking = true;
        botMoveCoroutine = StartCoroutine(MakeBotMoveCoroutine());
    }


    private IEnumerator MakeBotMoveCoroutine()
    {
        // Wait for end of frame to ensure all previous updates are complete
        yield return new WaitForEndOfFrame();

        // Clear existing trails

        float startTime = Time.realtimeSinceStartup;

        // Create a copy of the current board for evaluation
        char[,] boardCopy = new char[8, 8];
        System.Array.Copy(chessBoard, boardCopy, chessBoard.Length);

        // Check for book moves first
        var bookMove = GetBookMove(boardCopy);
        if (bookMove.HasValue)
        {
            // Add delay before making the book move
            yield return new WaitForSeconds(0.1f);

            ClearMoveTrails();
            var fromTrail = Instantiate(trail, new Vector3(bookMove.Value.fromCol, 7 - bookMove.Value.fromRow, 0), Quaternion.identity);
            var toTrail = Instantiate(trail, new Vector3(bookMove.Value.toCol, 7 - bookMove.Value.toRow, 0), Quaternion.identity);
            trailList.Add(fromTrail);
            trailList.Add(toTrail);
            AudioSource.PlayClipAtPoint(placeSound, Camera.main.transform.position);
            MakeMove(bookMove.Value.fromRow, bookMove.Value.fromCol, bookMove.Value.toRow, bookMove.Value.toCol, chessBoard);
            move++; // Increment move counter
            Refresh(); // Update the visual board
            IsKingCaptured(); // Check for game over
            isBotThinking = false;
            float timeSpent = Time.realtimeSinceStartup - startTime;
            //Debug.Log($"Bot played book move in {timeSpent:F3} seconds");
            yield break;
        }
        var allMoves = new List<(int fromRow, int fromCol, int toRow, int toCol)>();
        var boards = new List<char[,]>();        // Debug current position if in check
        if (IsInCheck(false, boardCopy))
        {
            //Debug.Log("Black king is in check, searching for moves to escape check");
        }



        // Collect all possible moves first
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                if (char.IsLower(boardCopy[row, col]))
                {
                    var moves = GetLegalMoves(row, col, boardCopy);
                    foreach (var move in moves)
                    {
                        // Create a temporary board and make the move
                        char[,] tempBoard = new char[8, 8];
                        System.Array.Copy(boardCopy, tempBoard, boardCopy.Length);
                        MakeMove(move.fromRow, move.fromCol, move.toRow, move.toCol, tempBoard);                        // Only add moves that don't leave the king in check
                        bool isLegalMove = !IsInCheck(false, tempBoard);
                        if (isLegalMove)
                        {
                            // Debug when we find a legal move that escapes check
                            if (IsInCheck(false, boardCopy))
                            {
                                //Debug.Log($"Found legal move to escape check: {boardCopy[move.fromRow, move.fromCol]} from {GetSquareNotation(move.fromRow, move.fromCol)} to {GetSquareNotation(move.toRow, move.toCol)}");
                            }
                            allMoves.Add(move);
                            boards.Add(tempBoard);
                        }
                    }
                }
            }
        }

        // Debug the number of legal moves found
        //Debug.Log($"Found {allMoves.Count} legal moves for black");

        if (allMoves.Count > 0)
        {
            //Debug.Log("Legal moves found are:");
            foreach (var move in allMoves)
            {
                //Debug.Log($"{boardCopy[move.fromRow, move.fromCol]} from {GetSquareNotation(move.fromRow, move.fromCol)} to {GetSquareNotation(move.toRow, move.toCol)}");
            }
        }

        // If no legal moves are available, it's checkmate
        if (allMoves.Count == 0)
        {
            // Only declare checkmate if the king is in check
            if (IsInCheck(false, boardCopy))
            {
                gameOver = true;
                winnerMessage = playAsWhite ? "Checkmate!\nWhite wins!" : "Checkmate!\nBlack wins!";
                isBotThinking = false;
                yield break;
            }
            else
            {
                // Stalemate
                gameOver = true;
                winnerMessage = "Stalemate!\nGame is a draw.";
                isBotThinking = false;
                yield break;
            }
        }

        int totalMoves = allMoves.Count;
        int bestScore = int.MaxValue;
        (int fromRow, int fromCol, int toRow, int toCol) bestMove = (-1, -1, -1, -1);        // If there's only one legal move, just make it
        if (totalMoves == 1)
        {
            //Debug.Log("Only one legal move available - playing it immediately");
            bestMove = allMoves[0];
        }
        else
        {
            // Move calculation to background task
            int totalPieces = 0;
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (chessBoard[row, col] != ' ' &&
                        chessBoard[row, col] != 'K' &&
                        char.IsUpper(chessBoard[row, col]))
                    {
                        totalPieces++;
                    }
                }
            }
            var calculationTask = Task.Run(() =>
            {
                var localBestScore = int.MaxValue;
                var localBestMove = (-1, -1, -1, -1);

                // Process all moves in parallel
                Parallel.For(0, totalMoves, i =>
                {
                    try
                    {
                        int score = Minimax(boards[i], minimaxDepth, int.MinValue, int.MaxValue, true, totalPieces);
                        lock (this)
                        {
                            if (score < localBestScore)
                            {
                                localBestScore = score;
                                localBestMove = allMoves[i];
                            }
                        }
                    }
                    catch
                    {
                        // Skip failed evaluations
                    }
                });

                return (localBestScore, localBestMove);
            });

            // Wait for calculation while keeping UI responsive
            while (!calculationTask.IsCompleted)
            {
                yield return null;
            }

            var result = calculationTask.Result;
            bestScore = result.localBestScore;
            bestMove = result.localBestMove;
        }

        // If we haven't found a good move but have legal moves, just pick the first one
        if (bestMove.fromRow == -1 && allMoves.Count > 0)
        {
            //Debug.Log("Falling back to first legal move due to evaluation issues");
            bestMove = allMoves[0];
        }
        // Make the best move on the actual board
        if (bestMove.fromRow != -1)
        {
            bool isCapture = chessBoard[bestMove.toRow, bestMove.toCol] != ' ';
        
            // Check for en passant capture
            bool isEnPassantCapture = false;
            char piece = chessBoard[bestMove.fromRow, bestMove.fromCol];
            if (char.ToLower(piece) == 'p' && bestMove.fromCol != bestMove.toCol && chessBoard[bestMove.toRow, bestMove.toCol] == ' ')
            {
                if (enPassantPossible && bestMove.toCol == enPassantCol && bestMove.toRow == enPassantRow)
                {
                    isEnPassantCapture = true;
                    isCapture = true; // En passant is also a capture
                }
            }

            ClearMoveTrails();
            var fromTrail = Instantiate(trail, new Vector3(bestMove.fromCol, 7 - bestMove.fromRow, 0), Quaternion.identity);
            var toTrail = Instantiate(trail, new Vector3(bestMove.toCol, 7 - bestMove.toRow, 0), Quaternion.identity);
            trailList.Add(fromTrail);
            trailList.Add(toTrail);


            if (isCapture)
            {
                // You'll need to add these public AudioClip variables to your GameManager class
                if (captureSound != null)
                    AudioSource.PlayClipAtPoint(captureSound, Camera.main.transform.position);
            }
            else
            {
                if (placeSound != null)
                    AudioSource.PlayClipAtPoint(placeSound, Camera.main.transform.position);
            }

            MakeGameMove(bestMove.fromRow, bestMove.fromCol, bestMove.toRow, bestMove.toCol);

            IsKingCaptured(); // Check for game over

            float timeSpent = Time.realtimeSinceStartup - startTime;
            //Debug.Log($"Bot calculated move in {timeSpent:F3} seconds (evaluated {totalMoves} positions at depth {minimaxDepth})");
        }
        else
        {
            //Debug.Log("Bot couldn't find a valid move!");
        }

        isBotThinking = false;
    }
    public int Minimax(char[,] board, int depth, int alpha, int beta, bool isMaximizing, int totalPieces)
    {
        // Base cases
        if (depth == 0)
        {
            return EvaluatePosition(board);
        }
        bool isEndgame = totalPieces <= 3;
        if (isEndgame)
        {
            bool hasLegalMoves = false;
            bool isInCheckNow = IsInCheck(isMaximizing, board);

            // Quick check for any legal moves
            for (int row = 0; row < 8 && !hasLegalMoves; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if ((isMaximizing && char.IsUpper(board[row, col])) ||
                        (!isMaximizing && char.IsLower(board[row, col])))
                    {
                        var moves = GetLegalMoves(row, col, board);
                        foreach (var move in moves)
                        {
                            char[,] tempBoard = new char[8, 8];
                            System.Array.Copy(board, tempBoard, board.Length);
                            MakeMove(move.fromRow, move.fromCol, move.toRow, move.toCol, tempBoard);
                            if (!IsInCheck(isMaximizing, tempBoard))
                            {
                                hasLegalMoves = true;
                                break;
                            }
                        }
                    }
                    if (hasLegalMoves) break;
                }
            }

            if (!hasLegalMoves)
            {
                if (isInCheckNow)
                {
                    // Checkmate: return a score that prefers faster mates
                    return isMaximizing ? (-100000 + depth) : (100000 - depth);
                }
                else
                {
                    // Stalemate: heavily penalize
                    return 0;
                }
            }
        }

        // Check for king capture (immediate win/loss)
        bool whiteKingExists = false;
        bool blackKingExists = false;
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                char piece = board[i, j];
                if (piece == 'K') whiteKingExists = true;
                if (piece == 'k') blackKingExists = true;
            }
        }

        // Use large but safe values for mate
        const int MATE_SCORE = 100000;
        if (!whiteKingExists) return -MATE_SCORE + depth; // Black wins (practically infinite score)
        if (!blackKingExists) return MATE_SCORE - depth;  // White wins (practically infinite score)

        if (isMaximizing)
        {
            int maxScore = int.MinValue;
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (char.IsUpper(board[row, col]))
                    { // White piece
                        var moves = GetLegalMoves(row, col, board);
                        foreach (var move in moves)
                        {
                            char[,] tempBoard = new char[8, 8];
                            System.Array.Copy(board, tempBoard, board.Length);

                            MakeMove(move.fromRow, move.fromCol, move.toRow, move.toCol, tempBoard);
                            int score = Minimax(tempBoard, depth - 1, alpha, beta, false, totalPieces);

                            maxScore = Mathf.Max(maxScore, score);
                            alpha = Mathf.Max(alpha, score);

                            if (beta <= alpha)
                            {
                                return maxScore; // Beta cut-off
                            }
                        }
                    }
                }
            }
            return maxScore;
        }
        else
        {
            int minScore = int.MaxValue;
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (char.IsLower(board[row, col]))
                    { // Black piece
                        var moves = GetLegalMoves(row, col, board);
                        foreach (var move in moves)
                        {
                            char[,] tempBoard = new char[8, 8];
                            System.Array.Copy(board, tempBoard, board.Length);

                            MakeMove(move.fromRow, move.fromCol, move.toRow, move.toCol, tempBoard);
                            int score = Minimax(tempBoard, depth - 1, alpha, beta, true, totalPieces);
                            minScore = Mathf.Min(minScore, score);
                            beta = Mathf.Min(beta, score);

                            if (beta <= alpha)
                            {
                                return minScore; // Alpha cut-off
                            }
                        }
                    }
                }
            }
            return minScore;
        }
    }    // Position tables to encourage better piece placement
    private static readonly int[,] pawnPositionBonus = {
        { 0,  0,  0,  0,  0,  0,  0,  0},
        {50, 50, 50, 50, 50, 50, 50, 50},
        {10, 10, 20, 30, 30, 20, 10, 10},
        { 5,  5, 10, 25, 25, 10,  5,  5},
        { 0,  0,  0, 20, 20,  0,  0,  0},
        { 5, -5,-10,  0,  0,-10, -5,  5},
        { 5, 10, 10,-20,-20, 10, 10,  5},
        { 0,  0,  0,  0,  0,  0,  0,  0}
    };

    private static readonly int[,] knightPositionBonus = {
        {-50,-40,-30,-30,-30,-30,-40,-50},
        {-40,-20,  0,  0,  0,  0,-20,-40},
        {-30,  0, 10, 15, 15, 10,  0,-30},
        {-30,  5, 15, 20, 20, 15,  5,-30},
        {-30,  0, 15, 20, 20, 15, 0,-30},
        {-30,  5, 10, 15, 15, 10,  5,-30},
        {-40,-20,  0,  5,  5,  0,-20,-40},
        {-50,-40,-30,-30,-30,-30,-40,-50}
    };

    private static readonly int[,] bishopPositionBonus = {
        {-20,-10,-10,-10,-10,-10,-10,-20},
        {-10,  0,  0,  0,  0,  0,  0,-10},
        {-10,  0,  5, 10, 10,  5,  0,-10},
        {-10,  5,  5, 10, 10,  5,  5,-10},
        {-10,  0, 10, 10, 10, 10,  0,-10},
        {-10, 10, 10, 10, 10, 10, 10,-10},
        {-10,  5,  0,  0,  0,  0,  5,-10},
        {-20,-10,-10,-10,-10,-10,-10,-20}
    };

    private static readonly int[,] rookPositionBonus = {
        { 0,  0,  0,  0,  0,  0,  0,  0},
        { 5, 10, 10, 10, 10, 10, 10,  5},
        {-5,  0,  0,  0,  0,  0,  0, -5},
        {-5,  0,  0,  0,  0,  0,  0, -5},
        {-5,  0,  0,  0,  0,  0,  0, -5},
        {-5,  0,  0,  0,  0,  0,  0, -5},
        {-5,  0,  0,  0,  0,  0,  0, -5},
        { 0,  0,  0,  5,  5,  0,  0,  0}
    };

    private static readonly int[,] queenPositionBonus = {
        {-20,-10,-10, -5, -5,-10,-10,-20},
        {-10,  0,  0,  0,  0,  0,  0,-10},
        {-10,  0,  5,  5,  5,  5,  0,-10},
        { -5,  0,  5,  5,  5,  5,  0, -5},
        {  0,  0,  5,  5,  5,  5,  0, -5},
        {-10,  5,  5,  5,  5,  5,  0,-10},
        {-10,  0,  5,  0,  0,  0,  0,-10},
        {-20,-10,-10, -5, -5,-10,-10,-20}
    };

    private static readonly int[,] kingMiddleGamePositionBonus = {
        {-30,-40,-40,-50,-50,-40,-40,-30},
        {-30,-40,-40,-50,-50,-40,-40,-30},
        {-30,-40,-40,-50,-50,-40,-40,-30},
        {-30,-40,-40,-50,-50,-40,-40,-30},
        {-20,-30,-30,-40,-40,-30,-30,-20},
        {-10,-20,-20,-20,-20,-20,-20,-10},
        { 20, 20,  0,  0,  0,  0, 20, 20},
        { 20, 30, 10,  0,  0, 10, 30, 20}
    };

    private int GetPositionBonus(char piece, int row, int col)
    {
        // Flip coordinates for black pieces to use the same tables
        int evalRow = char.IsUpper(piece) ? row : (7 - row);
        int evalCol = col;

        switch (char.ToLower(piece))
        {
            case 'p': return pawnPositionBonus[evalRow, evalCol];
            case 'n': return knightPositionBonus[evalRow, evalCol];
            case 'b': return bishopPositionBonus[evalRow, evalCol];
            case 'r': return rookPositionBonus[evalRow, evalCol];
            case 'q': return queenPositionBonus[evalRow, evalCol];
            case 'k': return kingMiddleGamePositionBonus[evalRow, evalCol];
            default: return 0;
        }
    }
    private int EvaluatePosition(char[,] board)
    {
        int score = 0;

        // Arrays to track pawn columns for both colors
        int[] whitePawnColumns = new int[8];
        int[] blackPawnColumns = new int[8];

        // Track king positions
        int whiteKingCol = -1, blackKingCol = -1;
        int whiteKingRow = -1, blackKingRow = -1;

        // Find kings' positions
        int enemyKingRow = -1, enemyKingCol = -1;
        bool isMajorPieceEndgame = false;
        int majorPieceCount = 0;
        int pieceCount = 0;
        // Count material and positional scores
        for (int col = 0; col < 8; col++)
        {
            for (int row = 0; row < 8; row++)
            {
                if (board[row, col] != ' ')
                {
                    char piece = board[row, col];
                    // Material score
                    score += pieceCosts[piece];

                    // Track pawn columns
                    if (piece == 'P')
                    {
                        whitePawnColumns[col]++;
                    }
                    else if (piece == 'p')
                    {
                        blackPawnColumns[col]++;
                    }

                    // Track king positions
                    if (piece == 'K')
                    {
                        whiteKingRow = row;
                        whiteKingCol = col;
                    }
                    else if (piece == 'k')
                    {
                        blackKingRow = row;
                        blackKingCol = col;
                    }

                    // Positional score (scaled down to not overwhelm material value)
                    int posBonus = GetPositionBonus(board[row, col], row, col);
                    score += char.IsUpper(board[row, col]) ? posBonus / 20 : -posBonus / 20;

                    if (piece != ' ')
                    {
                        score += pieceCosts[piece];

                        if (piece == 'K' || piece == 'k')
                        {
                            if (char.IsLower(piece))
                            {
                                enemyKingRow = row;
                                enemyKingCol = col;
                            }
                        }
                        // Count major pieces (queen and rook)
                        if (piece == 'Q' || piece == 'R')
                        {
                            majorPieceCount++;
                        }

                        if (piece != 'k' &&
                        piece != 'K')
                        {
                            pieceCount++;
                        }
                    }
                }
            }

            if (whitePawnColumns[col] > 1)
                score -= whitePawnColumns[col] - 1;
            if (blackPawnColumns[col] > 1)
                score += blackPawnColumns[col] - 1;

            // Penalize isolated pawns
            bool isIsolated = true;
            if (col > 0 && col < 7)
            {
                if (whitePawnColumns[col] > 0)
                {
                    if (whitePawnColumns[col - 1] > 0 || whitePawnColumns[col + 1] > 0)
                        isIsolated = false;
                    if (isIsolated)
                        score -= 1;
                }
                if (blackPawnColumns[col] > 0)
                {
                    if (blackPawnColumns[col - 1] > 0 || blackPawnColumns[col + 1] > 0)
                        isIsolated = false;
                    if (isIsolated)
                        score += 1;
                }
            }
        }
        // Evaluate king safety
        if (whiteKingCol != -1)
        {
            // Penalize exposed king
            if (whiteKingRow < 6) // King has moved away from back rank
                score -= 2;
            // Bonus for pawn shield
            for (int col = Math.Max(0, whiteKingCol - 1); col <= Math.Min(7, whiteKingCol + 1); col++)
            {
                if (whitePawnColumns[col] > 0)
                    score += 1;
            }
        }

        if (blackKingCol != -1)
        {
            // Penalize exposed king
            if (blackKingRow > 1) // King has moved away from back rank
                score += 2;
            // Bonus for pawn shield
            for (int col = Math.Max(0, blackKingCol - 1); col <= Math.Min(7, blackKingCol + 1); col++)
            {
                if (blackPawnColumns[col] > 0)
                    score -= 1;
            }
        }



        // Check if we're in a major piece endgame
        isMajorPieceEndgame = majorPieceCount > 0 && pieceCount <= 8;

        if (isMajorPieceEndgame && enemyKingRow != -1)
        {
            // Distance from edge bonus (simpler calculation)
            score -= 5 * (Math.Min(enemyKingRow, 7 - enemyKingRow) +
                        Math.Min(enemyKingCol, 7 - enemyKingCol));

            // Corner bonus
            if ((enemyKingRow == 0 || enemyKingRow == 7) &&
                (enemyKingCol == 0 || enemyKingCol == 7))
            {
                score -= 20;
            }

            bool hasBlackMoves = HasAnyLegalMoves(board, false);
            bool isBlackInCheck = IsInCheck(false, board);

            if (!hasBlackMoves)
            {
                if (isBlackInCheck)
                {
                    // Checkmate is better than any other position
                    return int.MaxValue;
                }
                else
                {
                    // Heavily penalize stalemate
                    return int.MinValue / 2;
                }
            }
        }

        return score;
    }



    // Quick check if side has any legal moves
    private bool HasAnyLegalMoves(char[,] board, bool isWhite)
    {
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                char piece = board[row, col];
                if (piece != ' ' && char.IsUpper(piece) == isWhite)
                {
                    var moves = GetLegalMoves(row, col, board);
                    foreach (var move in moves)
                    {
                        char[,] tempBoard = new char[8, 8];
                        Array.Copy(board, tempBoard, board.Length);
                        MakeMove(move.fromRow, move.fromCol, move.toRow, move.toCol, tempBoard);
                        if (!IsInCheck(isWhite, tempBoard))
                            return true;
                    }
                }
            }
        }
        return false;
    }

    void CreateBoard()
    {
        for (int row = 7; row >= 0; row--)
        {
            for (int col = 0; col < 8; col++)
            {
                bool isDark = (row + col) % 2 == 0;
                Color color = isDark ? darkCol : lightCol;
                Vector2 position = new Vector3(col, row, 0);
                sr.color = color;
                Instantiate(tile, position, Quaternion.identity);
                //GameObject newButton = Instantiate(button, position, Quaternion.identity);
                //newButton.GetComponent<SpriteButton>().row = 7 - row;
                //newButton.GetComponent<SpriteButton>().col = col;

            }
        }
    }

    public void InitPieces()
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                int row = 7 - i;
                int col = j;
                GameObject piece = ToPiece(chessBoard[i, j]);
                if (piece != null)
                {
                    pieceList[i, j] = Instantiate(piece, new Vector3(col, row, -2 + (((float)row) / 8)), Quaternion.identity);
                }
            }
        }
    }

    private void InitializePlaceButtonPool()
    {
        for (int i = 0; i < INITIAL_POOL_SIZE; i++)
        {
            GameObject button = Instantiate(placeButtonSprite);
            button.SetActive(false);
            placeButtonPool.Enqueue(button);
        }
    }

    public GameObject GetPlaceButton()
    {
        GameObject button;
        if (placeButtonPool.Count > 0)
        {
            button = placeButtonPool.Dequeue();
        }
        else
        {
            button = Instantiate(placeButtonSprite);
        }
        button.SetActive(true);
        return button;
    }
    public void ReturnPlaceButtonToPool(GameObject button)
    {
        if (button != null)
        {
            DestroyImmediate(button);
        }
    }

    public void ClearActivePlaceButtons()
    {
        foreach (GameObject place in positionList)
        {
            ReturnPlaceButtonToPool(place);
        }
        positionList.Clear();
        isMoving = false;
    }

    public GameObject ToPiece(char ch)
    {
        GameObject piece = null;
        switch (char.ToLower(ch))
        {
            case 'p':
                piece = pawn;
                break;
            case 'r':
                piece = rook;
                break;
            case 'n':
                piece = knight;
                break;
            case 'b':
                piece = bishop;
                break;
            case 'q':
                piece = queen;
                break;
            case 'k':
                piece = king;
                break;
            default:
                piece = null;
                break;
        }
        if (char.IsLower(ch))
        {
            piece.GetComponent<SpriteRenderer>().color = playAsWhite ? darkPiece : lightPiece;
            piece.GetComponent<PieceColor>().isWhite = false;
        }
        else if (char.IsUpper(ch))
        {
            piece.GetComponent<SpriteRenderer>().color = playAsWhite ? lightPiece : darkPiece;
            piece.GetComponent<PieceColor>().isWhite = true;
        }
        return piece;
    }
    public void RemoveAllPieces()
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (pieceList[i, j] != null)
                {
                    DestroyImmediate(pieceList[i, j]);  // Immediate destruction instead of delayed
                    pieceList[i, j] = null;
                }
            }
        }
    }

    public void Refresh()
    {
        RemoveAllPieces();
        InitPieces();
        //Debug.Log("Game refreshed");
    }

    private static List<(int fromRow, int fromCol, int toRow, int toCol)> GetLegalMoves(int row, int col, char[,] board, bool updateCastling = true)
    {
        List<(int, int, int, int)> moves = new List<(int, int, int, int)>();
        char piece = board[row, col];
        bool isBlack = char.IsLower(piece);

        switch (char.ToLower(piece))
        {
            case 'p':
                // Pawn moves
                int direction = isBlack ? 1 : -1;  // Black pawns move down, white moves up
                int startRow = isBlack ? 1 : 6;
                // Forward one square
                if (IsInBoard(row + direction, col) && board[row + direction, col] == ' ')
                {
                    moves.Add((row, col, row + direction, col)); // Add single-step move

                    // Initial two-square move
                    if (row == startRow &&
                        IsInBoard(row + 2 * direction, col) &&
                        board[row + 2 * direction, col] == ' ')
                    {
                        moves.Add((row, col, row + 2 * direction, col));
                    }
                }

                // Regular captures and en passant
                for (int colOffset = -1; colOffset <= 1; colOffset += 2)
                {
                    if (IsInBoard(row + direction, col + colOffset))
                    {
                        // Regular captures
                        char targetPiece = board[row + direction, col + colOffset];
                        if (targetPiece != ' ' && IsOpponentPiece(piece, targetPiece))
                        {
                            if (row + direction == 7 || row + direction == 0)
                            {
                                moves.Add((row, col, row + direction, col + colOffset)); // Promotion capture
                            }
                            else
                            {
                                moves.Add((row, col, row + direction, col + colOffset));
                            }
                        }
                        // En passant
                        else if (enPassantPossible &&
                                row == (isBlack ? 4 : 3) && // Correct rank for en passant
                                col + colOffset == enPassantCol && // Adjacent to pawn that moved
                                row + direction == enPassantRow) // Capture square matches
                        {
                            moves.Add((row, col, row + direction, col + colOffset));
                        }
                    }
                }
                break;

            case 'r':
                // Rook moves (horizontal and vertical)
                AddSlidingMoves(row, col, new[] { (-1, 0), (1, 0), (0, -1), (0, 1) }, board, moves);
                break;

            case 'n':
                // Knight moves
                int[,] knightOffsets = { { -2, -1 }, { -2, 1 }, { -1, -2 }, { -1, 2 }, { 1, -2 }, { 1, 2 }, { 2, -1 }, { 2, 1 } };
                for (int i = 0; i < 8; i++)
                {
                    int newRow = row + knightOffsets[i, 0];
                    int newCol = col + knightOffsets[i, 1];
                    if (IsInBoard(newRow, newCol))
                    {
                        if (board[newRow, newCol] == ' ' || IsOpponentPiece(piece, board[newRow, newCol]))
                        {
                            moves.Add((row, col, newRow, newCol));
                        }
                    }
                }
                break;

            case 'b':
                // Bishop moves (diagonals)
                AddSlidingMoves(row, col, new[] { (-1, -1), (-1, 1), (1, -1), (1, 1) }, board, moves);
                break;
            case 'q':
                // Queen moves (combination of rook and bishop)
                AddSlidingMoves(row, col, new[] {
                    (-1,-1), (-1,0), (-1,1),
                    (0,-1),          (0,1),
                    (1,-1),  (1,0),  (1,1)
                }, board, moves);
                break;
            case 'k':
                // Basic king moves
                int[,] kingOffsets = { { -1, -1 }, { -1, 0 }, { -1, 1 }, { 0, -1 }, { 0, 1 }, { 1, -1 }, { 1, 0 }, { 1, 1 } };
                for (int i = 0; i < 8; i++)
                {
                    int newRow = row + kingOffsets[i, 0];
                    int newCol = col + kingOffsets[i, 1];
                    if (IsInBoard(newRow, newCol))
                    {
                        if (board[newRow, newCol] == ' ' || IsOpponentPiece(piece, board[newRow, newCol]))
                        {
                            moves.Add((row, col, newRow, newCol));
                        }
                    }
                }

                // Only check castling if this is a king in its starting position
                if (updateCastling)
                {
                    if (isBlack && !blackKingMoved && row == 0 && col == 4)
                    {
                        // Kingside castling
                        if (!blackRightRookMoved &&
                            board[0, 5] == ' ' &&
                            board[0, 6] == ' ' &&
                            board[0, 7] == 'r' &&
                            CanCastle(0, 4, 6, board, true))
                        {
                            moves.Add((0, 4, 0, 6)); // King's move in castling
                        }

                        // Queenside castling
                        if (!blackLeftRookMoved &&
                            board[0, 3] == ' ' &&
                            board[0, 2] == ' ' &&
                            board[0, 1] == ' ' &&
                            board[0, 0] == 'r' &&
                            CanCastle(0, 4, 2, board, true))
                        {
                            moves.Add((0, 4, 0, 2)); // King's move in castling
                        }
                    }
                    else if (!isBlack && !whiteKingMoved && row == 7 && col == 4)
                    {
                        // Kingside castling
                        if (!whiteRightRookMoved &&
                            board[7, 5] == ' ' &&
                            board[7, 6] == ' ' &&
                            board[7, 7] == 'R' &&
                            CanCastle(7, 4, 6, board, false))
                        {
                            moves.Add((7, 4, 7, 6));
                        }

                        // Queenside castling
                        if (!whiteLeftRookMoved &&
                            board[7, 3] == ' ' &&
                            board[7, 2] == ' ' &&
                            board[7, 1] == ' ' &&
                            board[7, 0] == 'R' &&
                            CanCastle(7, 4, 2, board, false))
                        {
                            moves.Add((7, 4, 7, 2));
                        }
                    }
                }
                break;
        }

        return moves;
    }

    private static void AddSlidingMoves(int row, int col, (int dRow, int dCol)[] directions, char[,] board,
                                List<(int fromRow, int fromCol, int toRow, int toCol)> moves)
    {
        char piece = board[row, col];

        foreach (var (dRow, dCol) in directions)
        {
            int newRow = row + dRow;
            int newCol = col + dCol;

            while (IsInBoard(newRow, newCol))
            {
                if (board[newRow, newCol] == ' ')
                {
                    moves.Add((row, col, newRow, newCol));
                }
                else
                {
                    if (IsOpponentPiece(piece, board[newRow, newCol]))
                    {
                        moves.Add((row, col, newRow, newCol));
                    }
                    break; // Stop after capture or blocking piece
                }
                newRow += dRow;
                newCol += dCol;
            }
        }
    }

    private static bool IsInBoard(int row, int col)
    {
        return row >= 0 && row < 8 && col >= 0 && col < 8;
    }

    private static bool IsOpponentPiece(char piece1, char piece2)
    {
        return char.IsUpper(piece1) != char.IsUpper(piece2);
    }

    private static void UpdateCastlingRights(int fromRow, int fromCol, char piece)
    {
        if (piece == 'k' && fromRow == 0 && fromCol == 4)
        {
            blackKingMoved = true;
        }
        else if (piece == 'r')
        {
            if (fromRow == 0 && fromCol == 0)
            {
                blackLeftRookMoved = true;
            }
            else if (fromRow == 0 && fromCol == 7)
            {
                blackRightRookMoved = true;
            }
        }
    }

    private static void HandleCastling(int fromRow, int fromCol, int toRow, int toCol, char[,] board)
    {
        // If it's a king moving two squares, it's castling
        if (char.ToLower(board[fromRow, fromCol]) == 'k' && Mathf.Abs(toCol - fromCol) == 2)
        {
            // Kingside castling
            if (toCol > fromCol)
            {
                // Move the rook
                board[toRow, 5] = board[toRow, 7];
                board[toRow, 7] = ' ';
            }
            // Queenside castling
            else
            {
                // Move the rook
                board[toRow, 3] = board[toRow, 0];
                board[toRow, 0] = ' ';
            }
        }
    }

    private static void HandlePawnPromotion(int toRow, int toCol, char[,] board)
    {
        // If a black pawn reaches the bottom rank, promote to queen
        if (toRow == 7 && board[toRow, toCol] == 'p')
        {
            board[toRow, toCol] = 'q';
        }
        // If a white pawn reaches the top rank, promote to queen
        else if (toRow == 0 && board[toRow, toCol] == 'P')
        {
            board[toRow, toCol] = 'Q';
        }
    }
    public static void MakeMove(int fromRow, int fromCol, int toRow, int toCol, char[,] board)
    {
        char piece = board[fromRow, fromCol];

        // Handle en passant capture
        if (char.ToLower(piece) == 'p' && fromCol != toCol && board[toRow, toCol] == ' ')
        {
            // This is an en passant capture, remove the captured pawn
            board[fromRow, toCol] = ' ';
        }

        // Update castling rights
        UpdateCastlingRights(fromRow, fromCol, piece);

        // Make the move
        board[toRow, toCol] = board[fromRow, fromCol];
        board[fromRow, fromCol] = ' ';

        // Handle special moves
        HandleCastling(fromRow, fromCol, toRow, toCol, board);
        HandlePawnPromotion(toRow, toCol, board);
    }

    public static void ReplacePiece(int row, int col, GameObject newPiece)
    {
        if (pieceList[row, col] != null)
        {
            DestroyImmediate(pieceList[row, col]);
        }
        pieceList[row, col] = newPiece;
        // Force immediate UI refresh
        if (UnityEngine.UI.GraphicRegistry.instance != null)
        {
            var canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(canvas.transform as RectTransform);
            }
        }
    }

    public bool IsKingCaptured()
    {
        bool whiteKingExists = false;
        bool blackKingExists = false;
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (chessBoard[i, j] == 'K') whiteKingExists = true;
                if (chessBoard[i, j] == 'k') blackKingExists = true;
            }
        }

        if (!whiteKingExists)
        {
            gameOver = true;
            winnerMessage = "Checkmate! Black wins!";
            isTimerRunning = false;  // Add this line
            return true;
        }
        if (!blackKingExists)
        {
            gameOver = true;
            winnerMessage = "Checkmate! White wins!";
            isTimerRunning = false;  // Add this line
            return true;
        }
        return false;
    }

    /*void OnGUI()
    {
        if (gameOver)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 24;
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = Color.white;

            // Create a semi-transparent background
            GUI.color = new Color(0, 0, 0, 0.7f);
            GUI.Box(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 25, 200, 50), "");

            // Reset color for text
            GUI.color = Color.white;
            GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 25, 200, 50), winnerMessage, style);
        }
    }*/

    private string GetSquareNotation(int row, int col)
    {
        char file = (char)('a' + col);
        char rank = (char)('8' - row);
        return $"{file}{rank}";
    }

    private (int row, int col) ParseSquareNotation(string square)
    {
        int col = square[0] - 'a';
        int row = '8' - square[1];
        return (row, col);
    }
    private readonly Dictionary<string, string> openingBook = new Dictionary<string, string>
    {
        // Main Lines - Classical Responses
        {"e2e4", "e7e5"},     // King's Pawn Game (symmetrical, classical)
        {"d2d4", "d7d5"},     // Queen's Pawn Game (symmetrical, solid)
        {"c2c4", "e7e6"},     // English Opening (safe, prepares d5)
        
        // Common White First Moves - Safe Responses
        {"g1f3", "g8f6"},     // Reti Opening (symmetrical development)
        {"b2b3", "d7d5"},     // Larsen's Opening (control center)
        {"f2f4", "d7d5"},     // Bird's Opening (solid center control)
        {"g2g3", "d7d5"},     // King's Fianchetto (claim the center)
        
        // Common Second Moves - Natural Development
        {"b1c3", "b8c6"},     // Natural knight development
        {"f1c4", "g8f6"},     // Italian Game (solid defense)
        {"f1b5", "g8f6"},     // Ruy Lopez (solid knight development)
        
        // Irregular Openings - Simple Responses
        {"a2a3", "d7d5"},     // Against Anderssen's (claim center)
        {"a2a4", "d7d5"},     // Against Ware (claim center)
        {"h2h3", "d7d5"},     // Against Clemenz (claim center)
        {"h2h4", "d7d5"},     // Against Grob (claim center)
        {"b2b4", "d7d5"},     // Against Polish (claim center)
        {"d2d3", "d7d5"}      // Against Colle-style setup (claim center)
    }; private (int fromRow, int fromCol, int toRow, int toCol)? GetBookMove(char[,] board)
    {        // If we're white (not playAsWhite) and it's move 0, make a random first move
        if (!playAsWhite && move == 0)
        {            // When playing as black, the board is flipped, so we need to use flipped coordinates
            var weightedMoves = new (string move, int weight)[] {
                ("d7d5", 45),  // King's Pawn Opening (flipped) - Most common and aggressive
                ("e7e5", 30),  // Queen's Pawn Opening (flipped) - Second most common
                ("f7f5", 7),  // Sicilian Defense (flipped) - More aggressive
                ("b8c6", 15),   // Knight Defense (flipped) - Less common but solid
                ("b8c6", 3)    // Knight Development (flipped) - Rare but interesting
            };

            // Calculate total weight
            int totalWeight = 0;
            foreach (var move in weightedMoves)
                totalWeight += move.weight;

            // Get a random number within the total weight
            int randomNum = UnityEngine.Random.Range(0, totalWeight);

            // Find the chosen move based on weights
            string chosenMove = weightedMoves[0].move; // default to first move
            int currentWeight = 0;
            foreach (var move in weightedMoves)
            {
                currentWeight += move.weight;
                if (randomNum < currentWeight)
                {
                    chosenMove = move.move;
                    break;
                }
            }
            //Debug.Log($"Bot chose opening move: {chosenMove}");
            var from = ParseSquareNotation(chosenMove.Substring(0, 2));
            var to = ParseSquareNotation(chosenMove.Substring(2, 2));
            return (from.row, from.col, to.row, to.col);
        }

        // Only use book responses when it's move 1
        if (move != 1)
        {
            return null;
        }

        // Find white's first move
        string whiteMove = "";
        // Compare current board with initial board to find the moved piece
        bool moveFound = false;
        for (int r = 0; r < 8 && !moveFound; r++)
        {
            for (int c = 0; c < 8 && !moveFound; c++)
            {
                // Found where a piece moved from - empty in current board but had piece in initial board
                if (board[r, c] == ' ' && (playAsWhite ? initialBoardWhite[r, c] : initialBoardBlack[r, c]) != ' ' && char.IsUpper(playAsWhite ? initialBoardWhite[r, c] : initialBoardBlack[r, c]))
                {
                    char movedPiece = playAsWhite ? initialBoardWhite[r, c] : initialBoardBlack[r, c];

                    // Look for where it went in the current board
                    for (int r2 = 0; r2 < 8 && !moveFound; r2++)
                    {
                        for (int c2 = 0; c2 < 8 && !moveFound; c2++)
                        {
                            if ((r2 != r || c2 != c) && board[r2, c2] == movedPiece)
                            {
                                whiteMove = GetSquareNotation(r, c) + GetSquareNotation(r2, c2);
                                moveFound = true;
                            }
                        }
                    }
                }
            }
        }        // If we found the move and have a book response
        if (!string.IsNullOrEmpty(whiteMove))
        {
            if (openingBook.ContainsKey(whiteMove))
            {
                string response = openingBook[whiteMove];
                //Debug.Log($"Found book response: {response}");
                var from = ParseSquareNotation(response.Substring(0, 2));
                var to = ParseSquareNotation(response.Substring(2, 2));
                return (from.row, from.col, to.row, to.col);
            }
            else
            {
                //Debug.Log("Move not found in opening book");
            }
        }
        else
        {
            //Debug.Log("Could not determine white's move");
        }

        return null;
    }

    public static bool IsInCheck(bool isWhiteKing, char[,] board)
    {
        // Find king's position
        char kingChar = isWhiteKing ? 'K' : 'k';
        int kingRow = -1, kingCol = -1;
        // Find the king
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                if (board[row, col] == kingChar)
                {
                    kingRow = row;
                    kingCol = col;
                    break;
                }
            }
            if (kingRow != -1) break;
        }

        if (kingRow == -1) return false; // King not found (shouldn't happen in normal play)

        // Check for attacks from each piece type
        char[] enemyPieces = isWhiteKing
            ? new char[] { 'p', 'r', 'n', 'b', 'q', 'k' }  // Black pieces that could attack white king
            : new char[] { 'P', 'R', 'N', 'B', 'Q', 'K' }; // White pieces that could attack black king

        // Check each square on the board
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                char piece = board[row, col];
                if (Array.IndexOf(enemyPieces, piece) != -1)
                {
                    // Get all possible moves for this enemy piece
                    var moves = GetLegalMoves(row, col, board, false);  // false to prevent infinite recursion

                    // Check if any move can capture the king
                    foreach (var move in moves)
                    {
                        if (move.toRow == kingRow && move.toCol == kingCol)
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    public void Restart()
    {
        // Reset en passant state
        enPassantPossible = false;
        enPassantCol = -1;
        enPassantRow = -1;
        // Stop all running coroutines
        moveHistory.Clear();

        StopAllCoroutines();

        // Reset game state
        moveHistory.Clear();
        move = 0;
        gameOver = false;
        winnerMessage = "";
        message.text = winnerMessage;
        isMoving = false;
        isBotThinking = false;
        waitingForNextFrame = false;

        // Initialize timers
        whiteTimeRemaining = gameTime * 60f;
        blackTimeRemaining = gameTime * 60f;
        UpdateTimerDisplay();
        StopTimers();
        isTimerRunning = true;
        StartPlayerTimer();

        darkSquare.SetActive(false);

        // Reset castling flags
        whiteKingMoved = false;
        blackKingMoved = false;
        whiteLeftRookMoved = false;
        blackLeftRookMoved = false;
        whiteRightRookMoved = false;
        blackRightRookMoved = false;

        // Clear trails and buttons
        foreach (GameObject trail in trailList)
        {
            if (trail != null)
                Destroy(trail);
        }

        if (currentCheckSquare != null)
        {
            Destroy(currentCheckSquare);
            currentCheckSquare = null;
        }
        lastWhiteInCheck = false;
        lastBlackInCheck = false;

        trailList.Clear();
        ClearActivePlaceButtons();

        // Reset board to initial position
        chessBoard = new char[8, 8];
        playAsWhiteStatic = playAsWhite;
        System.Array.Copy(playAsWhite ? initialBoardWhite : initialBoardBlack, chessBoard, initialBoardWhite.Length);

        // Recreate pieces
        RemoveAllPieces();
        InitPieces();

        //Debug.Log("Game restarted");
    }

    public void MakeGameMove(int fromRow, int fromCol, int toRow, int toCol)
    {
        char piece = chessBoard[fromRow, fromCol];

        // Track en passant opportunity for actual game moves
        if (char.ToLower(piece) == 'p' && Math.Abs(toRow - fromRow) == 2)
        {
            enPassantCol = toCol;
            enPassantRow = toRow + (char.IsUpper(piece) ? 1 : -1);
            enPassantPossible = true;
            Debug.Log($"En passant possible at col {enPassantCol}, row {enPassantRow}");
        }
        else
        {
            // Reset en passant on any other move since it must be taken immediately
            enPassantPossible = false;
            enPassantCol = -1;
            enPassantRow = -1;
        }

        // Make the actual move
        MakeMove(fromRow, fromCol, toRow, toCol, chessBoard);
        move++; // Increment move counter
        
        // Switch active timer
        if (!gameOver)
        {
            StartPlayerTimer();
        }
        
        Refresh(); // Update the visual board
    }

    private static bool CanCastle(int row, int col, int targetCol, char[,] board, bool isBlack)
    {
        // Can't castle while in check
        if (IsInCheck(isBlack, board))
        {
            return false;
        }

        // Check squares the king passes through
        int direction = targetCol > col ? 1 : -1;
        char[,] tempBoard = new char[8, 8];

        // Check each square the king moves through
        for (int c = col; c != targetCol; c += direction)
        {
            Array.Copy(board, tempBoard, board.Length);
            // Simulate king on this square
            tempBoard[row, c + direction] = board[row, col];
            tempBoard[row, col] = ' ';

            // If any square is under attack, castling is illegal
            if (IsInCheck(isBlack, tempBoard))
            {
                return false;
            }
        }

        return true;
    }

    // Move history for undo functionality
    private class MoveState
    {
        public char[,] boardState;
        public bool wasEnPassantPossible;
        public int lastEnPassantCol;
        public int lastEnPassantRow;
        public GameObject[,] piecesState;
        public bool wasWhiteKingMoved;
        public bool wasBlackKingMoved;
        public bool wasWhiteLeftRookMoved;
        public bool wasBlackLeftRookMoved;
        public bool wasWhiteRightRookMoved;
        public bool wasBlackRightRookMoved;
        public int moveNumber;

        public MoveState(char[,] board, GameObject[,] pieces)
        {
            boardState = new char[8, 8];
            piecesState = new GameObject[8, 8];
            System.Array.Copy(board, boardState, board.Length);

            // Deep copy of piece references
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    piecesState[i, j] = pieces[i, j];

            // Copy current game state
            wasEnPassantPossible = enPassantPossible;
            lastEnPassantCol = enPassantCol;
            lastEnPassantRow = enPassantRow;
            wasWhiteKingMoved = whiteKingMoved;
            wasBlackKingMoved = blackKingMoved;
            wasWhiteLeftRookMoved = whiteLeftRookMoved;
            wasBlackLeftRookMoved = blackLeftRookMoved;
            wasWhiteRightRookMoved = whiteRightRookMoved;
            wasBlackRightRookMoved = blackRightRookMoved;
            moveNumber = move;
        }
    }

    private static Stack<MoveState> moveHistory = new Stack<MoveState>();

    // Store the current state before making a move
    public static void SaveState()
    {
        moveHistory.Push(new MoveState(chessBoard, pieceList));
    }

    public void Undo()
    {
        if (moveHistory.Count == 0)
            return;

        // Stop the bot's move coroutine if it's running
        if (isBotThinking && botMoveCoroutine != null)
        {
            StopCoroutine(botMoveCoroutine);
            botMoveCoroutine = null;
            isBotThinking = false;
            waitingForNextFrame = false;
        }

        // Get the previous state
        MoveState previousState = moveHistory.Pop();

        // Clear current trails
        ClearMoveTrails();

        RemoveAllPieces();

        foreach (GameObject place in GameManager.positionList)
        {
            Destroy(place);
        }

        // Restore the board and pieces state
        chessBoard = previousState.boardState;
        pieceList = previousState.piecesState;

        // Restore game state
        enPassantPossible = previousState.wasEnPassantPossible;
        enPassantCol = previousState.lastEnPassantCol;
        enPassantRow = previousState.lastEnPassantRow;
        whiteKingMoved = previousState.wasWhiteKingMoved;
        blackKingMoved = previousState.wasBlackKingMoved;
        whiteLeftRookMoved = previousState.wasWhiteLeftRookMoved;
        blackLeftRookMoved = previousState.wasBlackLeftRookMoved;
        whiteRightRookMoved = previousState.wasWhiteRightRookMoved;
        blackRightRookMoved = previousState.wasBlackRightRookMoved;
        move = previousState.moveNumber;

        // Visual update
        Refresh();
        //.Log("Move undone");

        // When undoing to before a checkmate/stalemate
        if (gameOver)
        {
            gameOver = false;
            winnerMessage = "";
        }
    }

    private void UpdateTimerDisplay()
    {
        if (whiteTime != null)
        {
            int whiteMinutes = Mathf.FloorToInt(whiteTimeRemaining / 60f);
            int whiteSeconds = Mathf.FloorToInt(whiteTimeRemaining % 60f);
            int whiteCentiseconds = Mathf.FloorToInt((whiteTimeRemaining * 100f) % 100f);
            whiteTime.text = string.Format("{0:00}:{1:00}.{2:00}", whiteMinutes, whiteSeconds, whiteCentiseconds);
            if (whiteMinutes <= 0 && whiteSeconds <= 9)
            {
                whiteTime.color = new Color(1f, 0.5f, 0.5f, 0.5f);
            }
            else
            {
                whiteTime.color = new Color(1f, 1f, 1f, 0.5f);
            }
        }

        if (blackTime != null)
        {
            int blackMinutes = Mathf.FloorToInt(blackTimeRemaining / 60f);
            int blackSeconds = Mathf.FloorToInt(blackTimeRemaining % 60f);
            int blackCentiseconds = Mathf.FloorToInt((blackTimeRemaining * 100f) % 100f);
            blackTime.text = string.Format("{0:00}:{1:00}.{2:00}", blackMinutes, blackSeconds, blackCentiseconds);
            if (blackMinutes <= 0 && blackSeconds <= 9)
            {
                blackTime.color = new Color(1f, 0.5f, 0.5f, 0.5f);
            }
            else
            {
                blackTime.color = new Color(1f, 1f, 1f, 0.5f);
            }
        }
    }

    private void StartPlayerTimer()
    {
        if (!isTimerRunning) return;

        StopTimers();
        if ((move % 2 == 0 && playAsWhite) || (move % 2 == 1 && !playAsWhite))
        {
            whiteTimerCoroutine = StartCoroutine(RunTimer(true));
        }
        else
        {
            blackTimerCoroutine = StartCoroutine(RunTimer(false));
        }
    }

    private void StopTimers()
    {
        if (whiteTimerCoroutine != null)
            StopCoroutine(whiteTimerCoroutine);
        if (blackTimerCoroutine != null)
            StopCoroutine(blackTimerCoroutine);
    }

    private IEnumerator RunTimer(bool isWhite)
    {
        Debug.Log($"Starting timer for {(isWhite ? "White" : "Black")}");
        float lastUpdateTime = Time.time;

        while (isTimerRunning && !gameOver)  // Add gameOver check here
        {
            float currentTime = Time.time;
            float deltaTime = currentTime - lastUpdateTime;
            lastUpdateTime = currentTime;

            if (move == 0) yield return null; // Don't run timer on first move
            else if (!isBotThinking)
            {
                whiteTimeRemaining -= deltaTime;
                if (whiteTimeRemaining <= 0)
                {
                    whiteTimeRemaining = 0;
                    gameOver = true;
                    winnerMessage = playAsWhite ? "Black wins on time!" : "White wins on time!";
                    message.text = winnerMessage;
                    isTimerRunning = false;
                    StopTimers();  // Add this line to stop all timers
                }
            }
            else
            {
                blackTimeRemaining -= deltaTime;
                if (blackTimeRemaining <= 0)
                {
                    blackTimeRemaining = 0;
                    gameOver = true;
                    winnerMessage = playAsWhite ? "White wins on time!" : "Black wins on time!";
                    message.text = winnerMessage;
                    isTimerRunning = false;
                    StopTimers();  // Add this line to stop all timers
                }
            }

            UpdateTimerDisplay();
            yield return null;
        }
    }
    
    private void ManageCheckIndicator()
    {
        // Check current state
        bool whiteInCheck = IsInCheck(true, chessBoard);
        bool blackInCheck = IsInCheck(false, chessBoard);

        // Only update if the check state has changed
        if (whiteInCheck != lastWhiteInCheck || blackInCheck != lastBlackInCheck)
        {
            // Destroy existing check square if it exists
            if (currentCheckSquare != null)
            {
                Destroy(currentCheckSquare);
                currentCheckSquare = null;
            }

            // Create new check square if needed
            if (whiteInCheck || blackInCheck)
            {
                // Find the king that's in check
                for (int row = 0; row < 8; row++)
                {
                    for (int col = 0; col < 8; col++)
                    {
                        char piece = chessBoard[row, col];
                        if ((whiteInCheck && piece == 'K') || (blackInCheck && piece == 'k'))
                        {
                            // Instantiate check square at the king's position
                            Vector3 position = new Vector3(col, 7 - row, -0.5f);
                            currentCheckSquare = Instantiate(checkSquare, position, Quaternion.identity);
                            AudioSource.PlayClipAtPoint(notifySound, position);
                            break;
                        }
                    }
                    if (currentCheckSquare != null) break;
                }
            }

            // Update the cached state
            lastWhiteInCheck = whiteInCheck;
            lastBlackInCheck = blackInCheck;
        }
    }
}
