using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using static Chess.Piece;

namespace Chess
{
    [Serializable]
    public class Logic
    {
        [NonSerialized]
        public MainWindow mWindow;          //window with chess board on it
        public Piece[] pieceArray;          //32 element array of pieces
        public display[,] displayArray;     //8x8 array of display objects
        public bool onePlayer;              //versus computer
        public bool twoPlayer;              //2 player local
        public bool networkGame;            //playing over a network
        public Color opponent;              //color of computer or 2nd player
        public Color offensiveTeam;         //which side is on the offense
        public int difficulty;              //difficulty level
        public bool ready;                  //blocks functionality for unwanted circumstances
        private Piece prevSelected;         //where the cursor clicked previously
        public List<string> themeList;      //list of themes
        public int themeIndex;              //which theme is currently in use
        private List<string> ignore;        //list of which dll files to ignore
        [NonSerialized] public BitmapImage lKing;
        [NonSerialized] public BitmapImage lQueen;
        [NonSerialized] public BitmapImage lBishop;
        [NonSerialized] public BitmapImage lKnight;
        [NonSerialized] public BitmapImage lRook;
        [NonSerialized] public BitmapImage lPawn;
        [NonSerialized] public BitmapImage dKing;
        [NonSerialized] public BitmapImage dQueen;
        [NonSerialized] public BitmapImage dBishop;
        [NonSerialized] public BitmapImage dKnight;
        [NonSerialized] public BitmapImage dRook;
        [NonSerialized] public BitmapImage dPawn;
        [NonSerialized] public TcpClient client;
        [NonSerialized] public NetworkStream nwStream;
        public byte[] buffer = new byte[255];                   //buffer for tcp
        private static Random rnd = new Random();               //used for all random situations
        private move bestMove;                                  //final return for minimax()
        [NonSerialized] private Thinking think;                 //progress bar window for hardMode
        public bool rotate = true;                              //Rotate board between turns on 2Player mode?
        public bool lastMove = true;                            //is lastMove menu option checked?
        public bool saveGame = true;                            //Save game on exit?
        public Color onBottom = Color.Light;                    //which team is on the bottom of the screen
        public string IP = "127.0.0.1";                         //IP address of server
        public int port = 54321;                                //port of server
        public double rotationDuration = 3;                     //how long the rotation animation takes
        public bool movablePieceSelected = false;               //if true, the next click will move the selected piece if possible
        private List<move> possible = new List<move>();         //list of all possible moves
        public Stack<historyNode> history = new Stack<historyNode>();   //stores all moves on a stack
        private string pwd = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        [NonSerialized]
        private BitmapImage bmpTo = new BitmapImage(new Uri("pack://application:,,,/Resources/to.png"));
        [NonSerialized]
        private BitmapImage bmpFrom = new BitmapImage(new Uri("pack://application:,,,/Resources/from.png"));
        private string dirPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Chess";
        public string filePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Chess\\chess.sav";

        [Serializable]
        private class saveData
        {
            //contains all the information needed to save the game

            public Piece[] sBoard { get; private set; }
            public Color sOffensiveTeam { get; private set; }
            public Color sOpponent { get; private set; }
            public Color sOnBottom { get; set; }
            public string sTheme { get; private set; }
            public bool sOnePlayer { get; private set; }
            public bool sTwoPlayer { get; set; }
            public bool sNetwork { get; private set; }
            public int sDifficulty { get; private set; }
            public bool sLastMove { get; private set; }
            public bool sSaveGame { get; private set; }
            public bool sReady { get; private set; }
            public bool sRotate { get; private set; }
            public double sRduration { get; private set; }
            public int sSizeX { get; private set; }
            public int sSizeY { get; private set; }

            public saveData(Piece[] _board, Color _offensiveTeam, Color _opponent, Color _onBottom, string _theme, bool _onePlayer, bool _twoPlayer,
                bool _network, int _difficulty, bool _lastMove, bool _saveGame, bool _ready, bool _rotate, double _rDuration, int _sizeX, int _sizeY)
            {
                this.sBoard = _board;
                this.sOffensiveTeam = _offensiveTeam;
                this.sOpponent = _opponent;
                this.sOnBottom = _onBottom;
                this.sTheme = _theme;
                this.sOnePlayer = _onePlayer;
                this.sTwoPlayer = _twoPlayer;
                this.sNetwork = _network;
                this.sDifficulty = _difficulty;
                this.sLastMove = _lastMove;
                this.sSaveGame = _saveGame;
                this.sReady = _ready;
                this.sRotate = _rotate;
                this.sRduration = _rDuration;
                this.sSizeX = _sizeX;
                this.sSizeY = _sizeY;
            }
        }

        [Serializable]
        public class historyNode
        {
            //contains all information needed to undo move

            public move step { get; set; }               //move that happened previously
            public Piece captured { get; set; }          //piece that move captured, if no capture, use null
            public bool pawnTransform { get; set; }      //did a pawn transformation happen?
            public bool skip { get; set; }               //undo next move also if playing against computer
            public bool firstMove { get; set; }          //was this the piece's first move?

            public historyNode(move _step, Piece _captured, bool _pawnTransform, bool _skip, bool _firstMove)
            {
                this.step = _step;
                this.captured = _captured;
                this.pawnTransform = _pawnTransform;
                this.skip = _skip;
                this.firstMove = _firstMove;
            }
        }

        [Serializable]
        public class display
        {
            public Canvas tile { get; set; }    //black/white-background or blue-selectedCell or green/orange-possibleMove
            public Image bottom { get; set; }   //to or from indicators
            public Image top { get; set; }      //sprite layer

            public display(Canvas _tile, Image _bottom, Image _top)
            {
                this.tile = _tile;
                this.bottom = _bottom;
                this.top = _top;
            }
        }

        public Logic(MainWindow mw)
        {
            this.mWindow = mw;
        }

        public void createDisplayArray()
        {
            //Creates array that handles all visuals

            displayArray = new display[8, 8];
            
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    switch (y)
                    {
                        case 0:
                            switch (x)
                            {
                                case 0:
                                    displayArray[x, y] =
                                        new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 1:
                                    displayArray[x, y] =
                                        new display(mWindow.one_zero, mWindow.bottom_one_zero, mWindow.top_one_zero);
                                    break;
                                case 2:
                                    displayArray[x, y] =
                                        new display(mWindow.two_zero, mWindow.bottom_two_zero, mWindow.top_two_zero);
                                    break;
                                case 3:
                                    displayArray[x, y] =
                                        new display(mWindow.three_zero, mWindow.bottom_three_zero, mWindow.top_three_zero);
                                    break;
                                case 4:
                                    displayArray[x, y] =
                                        new display(mWindow.four_zero, mWindow.bottom_four_zero, mWindow.top_four_zero);
                                    break;
                                case 5:
                                    displayArray[x, y] =
                                        new display(mWindow.five_zero, mWindow.bottom_five_zero, mWindow.top_five_zero);
                                    break;
                                case 6:
                                    displayArray[x, y] =
                                        new display(mWindow.six_zero, mWindow.bottom_six_zero, mWindow.top_six_zero);
                                    break;
                                case 7:
                                    displayArray[x, y] =
                                        new display(mWindow.seven_zero, mWindow.bottom_seven_zero, mWindow.top_seven_zero);
                                    break;
                            }
                            break;
                        case 1:
                            switch (x)
                            {
                                case 0:
                                    displayArray[x, y] =
                                        new display(mWindow.zero_one, mWindow.bottom_zero_one, mWindow.top_zero_one);
                                    break;
                                case 1:
                                    displayArray[x, y] =
                                        new display(mWindow.one_one, mWindow.bottom_one_one, mWindow.top_one_one);
                                    break;
                                case 2:
                                    displayArray[x, y] =
                                        new display(mWindow.two_one, mWindow.bottom_two_one, mWindow.top_two_one);
                                    break;
                                case 3:
                                    displayArray[x, y] =
                                        new display(mWindow.three_one, mWindow.bottom_three_one, mWindow.top_three_one);
                                    break;
                                case 4:
                                    displayArray[x, y] =
                                        new display(mWindow.four_one, mWindow.bottom_four_one, mWindow.top_four_one);
                                    break;
                                case 5:
                                    displayArray[x, y] =
                                        new display(mWindow.five_one, mWindow.bottom_five_one, mWindow.top_five_one);
                                    break;
                                case 6:
                                    displayArray[x, y] =
                                        new display(mWindow.six_one, mWindow.bottom_six_one, mWindow.top_six_one);
                                    break;
                                case 7:
                                    displayArray[x, y] =
                                        new display(mWindow.seven_one, mWindow.bottom_seven_one, mWindow.top_seven_one);
                                    break;
                            }
                            break;
                        case 2:
                            switch (x)
                            {
                                case 0:
                                    displayArray[x, y] =
                                        new display(mWindow.zero_two, mWindow.bottom_zero_two, mWindow.top_zero_two);
                                    break;
                                case 1:
                                    displayArray[x, y] =
                                        new display(mWindow.one_two, mWindow.bottom_one_two, mWindow.top_one_two);
                                    break;
                                case 2:
                                    displayArray[x, y] =
                                        new display(mWindow.two_two, mWindow.bottom_two_two, mWindow.top_two_two);
                                    break;
                                case 3:
                                    displayArray[x, y] =
                                        new display(mWindow.three_two, mWindow.bottom_three_two, mWindow.top_three_two);
                                    break;
                                case 4:
                                    displayArray[x, y] =
                                        new display(mWindow.four_two, mWindow.bottom_four_two, mWindow.top_four_two);
                                    break;
                                case 5:
                                    displayArray[x, y] =
                                        new display(mWindow.five_two, mWindow.bottom_five_two, mWindow.top_five_two);
                                    break;
                                case 6:
                                    displayArray[x, y] =
                                        new display(mWindow.six_two, mWindow.bottom_six_two, mWindow.top_six_two);
                                    break;
                                case 7:
                                    displayArray[x, y] =
                                        new display(mWindow.seven_two, mWindow.bottom_seven_two, mWindow.top_seven_two);
                                    break;
                            }
                            break;
                        case 3:
                            switch (x)
                            {
                                case 0:
                                    displayArray[x, y] =
                                        new display(mWindow.zero_three, mWindow.bottom_zero_three, mWindow.top_zero_three);
                                    break;
                                case 1:
                                    displayArray[x, y] =
                                        new display(mWindow.one_three, mWindow.bottom_one_three, mWindow.top_one_three);
                                    break;
                                case 2:
                                    displayArray[x, y] =
                                        new display(mWindow.two_three, mWindow.bottom_two_three, mWindow.top_two_three);
                                    break;
                                case 3:
                                    displayArray[x, y] =
                                        new display(mWindow.three_three, mWindow.bottom_three_three, mWindow.top_three_three);
                                    break;
                                case 4:
                                    displayArray[x, y] =
                                        new display(mWindow.four_three, mWindow.bottom_four_three, mWindow.top_four_three);
                                    break;
                                case 5:
                                    displayArray[x, y] =
                                        new display(mWindow.five_three, mWindow.bottom_five_three, mWindow.top_five_three);
                                    break;
                                case 6:
                                    displayArray[x, y] =
                                        new display(mWindow.six_three, mWindow.bottom_six_three, mWindow.top_six_three);
                                    break;
                                case 7:
                                    displayArray[x, y] =
                                        new display(mWindow.seven_three, mWindow.bottom_seven_three, mWindow.top_seven_three);
                                    break;
                            }
                            break;
                        case 4:
                            switch (x)
                            {
                                case 0:
                                    displayArray[x, y] =
                                        new display(mWindow.zero_four, mWindow.bottom_zero_four, mWindow.top_zero_four);
                                    break;
                                case 1:
                                    displayArray[x, y] =
                                        new display(mWindow.one_four, mWindow.bottom_one_four, mWindow.top_one_four);
                                    break;
                                case 2:
                                    displayArray[x, y] =
                                        new display(mWindow.two_four, mWindow.bottom_two_four, mWindow.top_two_four);
                                    break;
                                case 3:
                                    displayArray[x, y] =
                                        new display(mWindow.three_four, mWindow.bottom_three_four, mWindow.top_three_four);
                                    break;
                                case 4:
                                    displayArray[x, y] =
                                        new display(mWindow.four_four, mWindow.bottom_four_four, mWindow.top_four_four);
                                    break;
                                case 5:
                                    displayArray[x, y] =
                                        new display(mWindow.five_four, mWindow.bottom_five_four, mWindow.top_five_four);
                                    break;
                                case 6:
                                    displayArray[x, y] =
                                        new display(mWindow.six_four, mWindow.bottom_six_four, mWindow.top_six_four);
                                    break;
                                case 7:
                                    displayArray[x, y] =
                                        new display(mWindow.seven_four, mWindow.bottom_seven_four, mWindow.top_seven_four);
                                    break;
                            }
                            break;
                        case 5:
                            switch (x)
                            {
                                case 0:
                                    displayArray[x, y] =
                                        new display(mWindow.zero_five, mWindow.bottom_zero_five, mWindow.top_zero_five);
                                    break;
                                case 1:
                                    displayArray[x, y] =
                                        new display(mWindow.one_five, mWindow.bottom_one_five, mWindow.top_one_five);
                                    break;
                                case 2:
                                    displayArray[x, y] =
                                        new display(mWindow.two_five, mWindow.bottom_two_five, mWindow.top_two_five);
                                    break;
                                case 3:
                                    displayArray[x, y] =
                                        new display(mWindow.three_five, mWindow.bottom_three_five, mWindow.top_three_five);
                                    break;
                                case 4:
                                    displayArray[x, y] =
                                        new display(mWindow.four_five, mWindow.bottom_four_five, mWindow.top_four_five);
                                    break;
                                case 5:
                                    displayArray[x, y] =
                                        new display(mWindow.five_five, mWindow.bottom_five_five, mWindow.top_five_five);
                                    break;
                                case 6:
                                    displayArray[x, y] =
                                        new display(mWindow.six_five, mWindow.bottom_six_five, mWindow.top_six_five);
                                    break;
                                case 7:
                                    displayArray[x, y] =
                                        new display(mWindow.seven_five, mWindow.bottom_seven_five, mWindow.top_seven_five);
                                    break;
                            }
                            break;
                        case 6:
                            switch (x)
                            {
                                case 0:
                                    displayArray[x, y] =
                                        new display(mWindow.zero_six, mWindow.bottom_zero_six, mWindow.top_zero_six);
                                    break;
                                case 1:
                                    displayArray[x, y] =
                                        new display(mWindow.one_six, mWindow.bottom_one_six, mWindow.top_one_six);
                                    break;
                                case 2:
                                    displayArray[x, y] =
                                        new display(mWindow.two_six, mWindow.bottom_two_six, mWindow.top_two_six);
                                    break;
                                case 3:
                                    displayArray[x, y] =
                                        new display(mWindow.three_six, mWindow.bottom_three_six, mWindow.top_three_six);
                                    break;
                                case 4:
                                    displayArray[x, y] =
                                        new display(mWindow.four_six, mWindow.bottom_four_six, mWindow.top_four_six);
                                    break;
                                case 5:
                                    displayArray[x, y] =
                                        new display(mWindow.five_six, mWindow.bottom_five_six, mWindow.top_five_six);
                                    break;
                                case 6:
                                    displayArray[x, y] =
                                        new display(mWindow.six_six, mWindow.bottom_six_six, mWindow.top_six_six);
                                    break;
                                case 7:
                                    displayArray[x, y] =
                                        new display(mWindow.seven_six, mWindow.bottom_seven_six, mWindow.top_seven_six);
                                    break;
                            }
                            break;
                        case 7:
                            switch (x)
                            {
                                case 0:
                                    displayArray[x, y] =
                                        new display(mWindow.zero_seven, mWindow.bottom_zero_seven, mWindow.top_zero_seven);
                                    break;
                                case 1:
                                    displayArray[x, y] =
                                        new display(mWindow.one_seven, mWindow.bottom_one_seven, mWindow.top_one_seven);
                                    break;
                                case 2:
                                    displayArray[x, y] =
                                        new display(mWindow.two_seven, mWindow.bottom_two_seven, mWindow.top_two_seven);
                                    break;
                                case 3:
                                    displayArray[x, y] =
                                        new display(mWindow.three_seven, mWindow.bottom_three_seven, mWindow.top_three_seven);
                                    break;
                                case 4:
                                    displayArray[x, y] =
                                        new display(mWindow.four_seven, mWindow.bottom_four_seven, mWindow.top_four_seven);
                                    break;
                                case 5:
                                    displayArray[x, y] =
                                        new display(mWindow.five_seven, mWindow.bottom_five_seven, mWindow.top_five_seven);
                                    break;
                                case 6:
                                    displayArray[x, y] =
                                        new display(mWindow.six_seven, mWindow.bottom_six_seven, mWindow.top_six_seven);
                                    break;
                                case 7:
                                    displayArray[x, y] =
                                        new display(mWindow.seven_seven, mWindow.bottom_seven_seven, mWindow.top_seven_seven);
                                    break;
                            }
                            break;
                    }
                }
            }
        }

        public void setBoardForNewGame()
        {
            //Sets arrays in starting position

            pieceArray = new Piece[33];

            for (int i = 0; i < 32; i++)
            {
                switch (i)
                {
                    case 0:
                        pieceArray[i] = new Rook(new coordinate(0, 0), Color.Light);
                        displayArray[0, 0].top.Source = lRook;
                        break;
                    case 1:
                        pieceArray[i] = new Knight(new coordinate(1, 0), Color.Light);
                        displayArray[1, 0].top.Source = lKnight;
                        break;
                    case 2:
                        pieceArray[i] = new Bishop(new coordinate(2, 0), Color.Light);
                        displayArray[2, 0].top.Source = lBishop;
                        break;
                    case 3:
                        pieceArray[i] = new Queen(new coordinate(3, 0), Color.Light);
                        displayArray[3, 0].top.Source = lQueen;
                        break;
                    case 4:
                        pieceArray[i] = new King(new coordinate(4, 0), Color.Light);
                        displayArray[4, 0].top.Source = lKing;
                        break;
                    case 5:
                        pieceArray[i] = new Bishop(new coordinate(5, 0), Color.Light);
                        displayArray[5, 0].top.Source = lBishop;
                        break;
                    case 6:
                        pieceArray[i] = new Knight(new coordinate(6, 0), Color.Light);
                        displayArray[6, 0].top.Source = lKnight;
                        break;
                    case 7:
                        pieceArray[i] = new Rook(new coordinate(7, 0), Color.Light);
                        displayArray[7, 0].top.Source = lRook;
                        break;
                    case 8:
                    case 9:
                    case 10:
                    case 11:
                    case 12:
                    case 13:
                    case 14:
                    case 15:
                        pieceArray[i] = new Pawn(new coordinate(i - 8, 1), Color.Light);
                        displayArray[i - 8, 1].top.Source = lPawn;
                        break;
                    case 16:
                    case 17:
                    case 18:
                    case 19:
                    case 20:
                    case 21:
                    case 22:
                    case 23:
                        pieceArray[i] = new Pawn(new coordinate(i - 16, 6), Color.Dark);
                        displayArray[i - 16, 6].top.Source = dPawn;
                        break;
                    case 24:
                        pieceArray[i] = new Rook(new coordinate(0, 7), Color.Dark);
                        displayArray[0, 7].top.Source = dRook;
                        break;
                    case 25:
                        pieceArray[i] = new Knight(new coordinate(1, 7), Color.Dark);
                        displayArray[1, 7].top.Source = dKnight;
                        break;
                    case 26:
                        pieceArray[i] = new Bishop(new coordinate(2, 7), Color.Dark);
                        displayArray[2, 7].top.Source = dBishop;
                        break;
                    case 27:
                        pieceArray[i] = new Queen(new coordinate(3, 7), Color.Dark);
                        displayArray[3, 7].top.Source = dQueen;
                        break;
                    case 28:
                        pieceArray[i] = new King(new coordinate(4, 7), Color.Dark);
                        displayArray[4, 7].top.Source = dKing;
                        break;
                    case 29:
                        pieceArray[i] = new Bishop(new coordinate(5, 7), Color.Dark);
                        displayArray[5, 7].top.Source = dBishop;
                        break;
                    case 30:
                        pieceArray[i] = new Knight(new coordinate(6, 7), Color.Dark);
                        displayArray[6, 7].top.Source = dKnight;
                        break;
                    case 31:
                        pieceArray[i] = new Rook(new coordinate(7, 7), Color.Dark);
                        displayArray[7, 7].top.Source = dRook;
                        break;
                }
            }
        }

        private List<Piece> getLightPieces(Piece[] board)
        {
            List<Piece> light = new List<Piece>();

            for (int i = 0; i < 16; i++)
            {
                if (board[i].dead == false)
                {
                    light.Add(board[i]);
                }
            }
            return light;
        }

        private List<Piece> getDarkPieces(Piece[] board)
        {
            List<Piece> dark = new List<Piece>();

            for (int i = 16; i < 32; i++)
            {
                if (board[i].dead == false)
                {
                    dark.Add(board[i]);
                }
            }
            return dark;
        }

        public int minimax(
            Piece[] board, Color attacking, int level, bool computerTurn, int alpha, int beta, IProgress<int> progress)
        {
            //is called recursively for comp to look ahead and return the best move

            int bestVal;
            List<move> offensiveMoves = new List<move>();

            level++;
            //if not on bottom level, go down
            if(level < difficulty)
            {
                int val;
                Piece[] newBoard;
                int indexOfBest = 0;

                Color nextTurn = switchTeam(attacking);

                if (attacking == Color.Light)
                {
                    for (int i = 0; i < 16; i++)
                    {
                        if (board[i].dead == false)
                        {
                            offensiveMoves.AddRange(board[i].getCheckRestrictedMoves(board));
                        }
                    }
                }
                else
                {
                    for (int i = 16; i < 32; i++)
                    {
                        if (board[i].dead == false)
                        {
                            offensiveMoves.AddRange(board[i].getCheckRestrictedMoves(board));
                        }
                    }
                }

                if (computerTurn == true)
                {
                    bestVal = -30;

                    //if checkmate
                    if (offensiveMoves.Count == 0){}
                    else
                    {
                        if (level == 0)
                        {
                            for (int i = 0; i < offensiveMoves.Count; i++)
                            {
                                newBoard = deepCopy(board);
                                newBoard = privateMove(offensiveMoves[i], newBoard);
                                val = minimax(newBoard, nextTurn, level, false, alpha, beta, null);
                                if (val > bestVal)
                                {
                                    bestVal = val;
                                    indexOfBest = i;
                                }
                                //null = easyMode
                                if (progress != null)
                                {
                                    progress.Report((i * 100) / offensiveMoves.Count);
                                }

                                alpha = Math.Max(alpha, bestVal);
                                if (beta <= alpha)
                                {
                                    break;
                                }
                            }
                            bestMove = offensiveMoves[indexOfBest];
                            if (progress != null)
                            {
                                progress.Report(100);
                            }
                        }
                        else
                        {
                            for (int i = 0; i < offensiveMoves.Count; i++)
                            {
                                newBoard = deepCopy(board);
                                newBoard = privateMove(offensiveMoves[i], newBoard);
                                val = minimax(newBoard, nextTurn, level, false, alpha, beta, null);
                                bestVal = Math.Max(bestVal, val);
                                alpha = Math.Max(alpha, bestVal);
                                if (beta <= alpha)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    bestVal = 30;

                    //if checkmate
                    if (offensiveMoves.Count == 0){}
                    else
                    {
                        for (int i = 0; i < offensiveMoves.Count; i++)
                        {
                            newBoard = deepCopy(board);
                            newBoard = privateMove(offensiveMoves[i], newBoard);
                            val = minimax(newBoard, nextTurn, level, true, alpha, beta, null);
                            bestVal = Math.Min(bestVal, val);
                            beta = Math.Min(beta, bestVal);
                            if(beta <= alpha)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            //bottom level
            else
            {
                bestVal = evaluator(board);
            }
            return bestVal;
        }

        private Piece[] privateMove(move m, Piece[] board)
        {
            //does a move without visuals on a private board for computer

            int rookIndex;
            int kingIndex = getIndex(m.start, board);

            //move to new cell
            board[getIndex(m.end, board)].dead = true;
            board[kingIndex].coor = m.end;
            board[kingIndex].virgin = false;
            Color offense = board[kingIndex].color;

            //check for pawnTransform
            if (board[kingIndex].job == Job.Pawn)
            {
                if (m.end.y == 0)
                {
                    board[kingIndex] = new Queen(m.end, Color.Dark);
                }

                else if (m.end.y == 7)
                {
                    board[kingIndex] = new Queen(m.end, Color.Light);
                }
            }
            //check for castling
            else if (board[kingIndex].job == Job.King)
            {
                int yCoor;

                if (offense == Color.Dark)
                {
                    yCoor = 7;
                }
                else
                {
                    yCoor = 0;
                }

                if (m.start.x == 4 && m.start.y == yCoor)  //if moving from King default position
                {
                    if (m.end.x == 2 && m.end.y == yCoor)  //if moving two spaces to the left
                    {
                        rookIndex = getIndex(new coordinate(0, yCoor), board);

                        //Move Rook
                        board[rookIndex].coor = new coordinate(3, yCoor);
                        board[rookIndex].virgin = false;
                    }

                    else if (m.end.x == 6 && m.end.y == yCoor) //if moving two spaces to the right
                    {
                        rookIndex = getIndex(new coordinate(7, yCoor), board);

                        //Move Rook
                        board[rookIndex].coor = new coordinate(5, yCoor);
                        board[rookIndex].virgin = false;
                    }
                }
            }
            return board;
        }

        private int evaluator(Piece[] board)
        {
            //looks at board and returns a value that indicates how good the computer is doing

            int total = 0;

            if(opponent == Color.Light)
            {
                for (int i = 0; i < 16; i++)
                {
                    if (board[i].dead == false)
                    {
                        //compPieces
                        total += evalAdd(board[i]);
                    }
                }

                for (int i = 16; i < 32; i++)
                {
                    if (board[i].dead == false)
                    {
                        //humanPieces
                        total -= evalAdd(board[i]);
                    }
                }
            }
            else
            {
                for (int i = 0; i < 16; i++)
                {
                    if (board[i].dead == false)
                    {
                        //humanPieces
                        total -= evalAdd(board[i]);
                    }
                }

                for (int i = 16; i < 32; i++)
                {
                    if (board[i].dead == false)
                    {
                        //compPieces
                        total += evalAdd(board[i]);
                    }
                }
            }
            return total;
        }

        private int evalAdd(Piece p)
        {
            switch (p.job)
            {
                case Job.Queen:
                    return 5;
                case Job.Rook:
                    return 3;
                case Job.Bishop:
                    return 3;
                case Job.Knight:
                    return 2;
                case Job.Pawn:
                    return 1;
                default:
                    return 0;
            }
        }

        private Piece[] deepCopy(Piece[] source)
        {
            //Creates a deep copy of board

            Piece[] copy = new Piece[33];

            for (int i = 0; i < 32; i++)
            {
                switch(source[i].job)
                {
                    case Job.King:
                        copy[i] = new King(
                            source[i].coor,
                            source[i].color,
                            source[i].virgin);
                        break;
                    case Job.Queen:
                        copy[i] = new Queen(
                            source[i].coor,
                            source[i].color);
                        break;
                    case Job.Rook:
                        copy[i] = new Rook(
                            source[i].coor,
                            source[i].color,
                            source[i].virgin);
                        break;
                    case Job.Bishop:
                        copy[i] = new Bishop(
                            source[i].coor,
                            source[i].color);
                        break;
                    case Job.Knight:
                        copy[i] = new Knight(
                            source[i].coor,
                            source[i].color);
                        break;
                    case Job.Pawn:
                        copy[i] = new Pawn(
                            source[i].coor,
                            source[i].color,
                            source[i].virgin);
                        break;
                }
                copy[i].dead = source[i].dead;
            }
            return copy;
        }

        public static bool isInCheck(Color teamInQuestion, Piece[] board)
        {
            //returns whether or not team in question is in check

            int captured;
            List<move> poss = new List<move>();

            //get possible moves of opposing team,
            //doesn't matter if opposing team move gets them in check,
            //still a valid move for current team
            if (teamInQuestion == Color.Dark)
            {
                for (int i = 0; i < 16; i++)
                {
                    if (board[i].dead == false)
                    {
                        poss.AddRange(board[i].getMoves(board));
                    }
                }
            }

            else
            {
                for (int i = 16; i < 32; i++)
                {
                    if (board[i].dead == false)
                    {
                        poss.AddRange(board[i].getMoves(board));
                    }
                }
            }

            foreach (move m in poss)
            {
                captured = getIndex(m.end, board);
                //if opposing team's move can capture your king, you're in check
                if (board[captured].job == Job.King &&
                    board[captured].color == teamInQuestion)
                {
                    return true;
                }
            }
            return false;
        }

        private bool isInCheckmate(List<Piece> teamInQuestion, Color teamColor)
        {
            //takes list of pieces and returns whether or not player is in checkmate

            List<move> possibleWithoutCheck = new List<move>();

            //find all moves that can be done without going into check
            foreach (Piece p in teamInQuestion)
            {
                possibleWithoutCheck.AddRange(p.getCheckRestrictedMoves(pieceArray));
            }

            if (possibleWithoutCheck.Count == 0)//if no moves available that don't go into check
            {
                string message;
                ready = false;
                mWindow.undoMenu.IsEnabled = false;

                if(networkGame == true)
                {
                    byte[] winner = new byte[1] { 2 };
                    nwStream.Write(winner, 0, 1);
                    nwStream.Close();
                    client.Close();
                }
                
                foreach(display d in displayArray)
                {
                    d.tile.Cursor = Cursors.Arrow;
                }

                if (twoPlayer == true)
                {
                    Color winningTeam = switchTeam(teamColor);
                    message = "The " + winningTeam + " army has slain the " + teamColor + " army's king in battle";
                }

                else
                {
                    if (teamColor == opponent)
                    {
                        message = "Congratulations!\n\nYou have slain the evil king\nand saved the princess!";
                    }

                    else
                    {
                        message = "Sorry\n\nYou gave a valiant effort,\nbut you have been bested in battle by the enemy army";
                    }
                }

                MessageBoxResult result = MessageBox.Show(message, "Game Over",
                        MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.OK);

                return true;
            }
            return false;
        }

        private void castling(move shift)
        {
            //if selected move is a castling move, move Rook in this function

            historyNode node;
            int yCoor;                                          //which row the move is being conducted in
            move castleMove = new move();

            if (offensiveTeam == Color.Dark)
            {
                yCoor = 7;
            }
            else
            {
                yCoor = 0;
            }

            if (shift.start.x == 4 && shift.start.y == yCoor)   //if moving from King default position
            {
                if (shift.end.x == 2 && shift.end.y == yCoor)   //if moving two spaces to the left
                {
                    castleMove.end = new coordinate(3, yCoor);
                    castleMove.start = new coordinate(0, yCoor);

                    if(networkGame == false)
                    {
                        node = new historyNode(castleMove, pieceArray[getIndex(new coordinate(3, yCoor), pieceArray)], false, true, true);
                        history.Push(node);
                    }
                    
                    pieceArray[getIndex(castleMove.start, pieceArray)].movePiece(castleMove.end, this);
                }

                else if (shift.end.x == 6 && shift.end.y == yCoor) //if moving two spaces to the right
                {
                    castleMove.end = new coordinate(5, yCoor);
                    castleMove.start = new coordinate(7, yCoor);

                    if (networkGame == false)
                    {
                        node = new historyNode(castleMove, pieceArray[getIndex(new coordinate(5, yCoor), pieceArray)], false, true, true);
                        history.Push(node);
                    }

                    pieceArray[getIndex(castleMove.start, pieceArray)].movePiece(castleMove.end, this);
                }
            }
        }

        public async Task clicker(coordinate currentCell)
        {
            //human player's turn, gets called when player clicks on spot

            if (ready == true)  //blocks functionality if game hasn't started yet
            {
                Piece currentPiece = pieceArray[getIndex(currentCell, pieceArray)];
                //if selected same piece
                if (displayArray[currentCell.x, currentCell.y].tile.Background == Brushes.DeepSkyBlue)
                {
                    movablePieceSelected = false;
                    clearSelectedAndPossible();
                }
                //if selected own piece
                else if (currentPiece.color == offensiveTeam)
                {
                    ownCellSelected(currentPiece);
                }
                //if selected movable spot
                else if (movablePieceSelected == true)
                {
                    await movableCellSelected(currentPiece);
                }
            }
            else if(networkGame == true && client != null)
            {
                if(client.Connected == true)
                {
                    MessageBox.Show(Application.Current.MainWindow, "It is currently the other player's turn", "Wait",
                    MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.OK);
                }
            }
        }

        private void ownCellSelected(Piece cPiece)
        {
            //When click on your own team the first time, get possible moves

            movablePieceSelected = true;
            clearSelectedAndPossible();
            displayArray[cPiece.coor.x, cPiece.coor.y].tile.Background = Brushes.DeepSkyBlue;
            prevSelected = cPiece;
            possible.Clear();
            possible.AddRange(cPiece.getCheckRestrictedMoves(pieceArray));
            Color defensiveTeam = switchTeam(offensiveTeam);

            foreach (move m in possible)
            {
                if (pieceArray[getIndex(m.end, pieceArray)].color == defensiveTeam)
                {
                    displayArray[m.end.x, m.end.y].tile.Background = Brushes.DarkOrange;
                }
                else
                {
                    displayArray[m.end.x, m.end.y].tile.Background = Brushes.LawnGreen;
                }
            }
        }

        private async Task movableCellSelected(Piece captured)
        {
            //If previously clicked on a movable piece

            move curTurn = new move();
            bool movableSpot = false;

            //Checks if current click is in possible move list
            foreach (move m in possible)
            {
                if (captured.coor == m.end)
                {
                    movableSpot = true;
                    curTurn = m;
                }
            }

            if (movableSpot == true)
            {
                historyNode node;
                Job pawnTrans = 0;
                bool virginMove = prevSelected.virgin;
                prevSelected.movePiece(captured.coor, this);
                clearSelectedAndPossible();

                if (captured.job == Job.Pawn)
                {
                    if (captured.coor.y == 0 || captured.coor.y == 7)
                    {
                        PawnTransformation transform = new PawnTransformation(captured.coor, this);
                        transform.ShowDialog();
                        node = new historyNode(curTurn, captured, true, false, false);
                        pawnTrans = captured.job;
                    }
                    //if pawn, but no transform
                    else
                    {
                        node = new historyNode(curTurn, captured, false, false, virginMove);
                    }
                }

                else    //not pawn
                {
                    node = new historyNode(curTurn, captured, false, false, virginMove);
                }

                if (networkGame == false)
                {
                    history.Push(node);
                    mWindow.undoMenu.IsEnabled = true;
                }
                //send move to server
                else
                {
                    buffer[0] = 7;

                    if(opponent == Color.Light)
                    {
                        buffer[1] = (byte)(curTurn.start.x);
                        buffer[2] = (byte)(curTurn.start.y);
                        buffer[3] = (byte)(curTurn.end.x);
                        buffer[4] = (byte)(curTurn.end.y);
                    }
                    else
                    {
                        buffer[1] = (byte)(curTurn.start.x);
                        buffer[2] = (byte)(7 - curTurn.start.y);
                        buffer[3] = (byte)(curTurn.end.x);
                        buffer[4] = (byte)(7 - curTurn.end.y);
                    }
                    buffer[5] = (byte)pawnTrans;
                    nwStream.Write(buffer, 0, 6);
                }

                if (lastMove == true)
                {
                    clearToAndFrom();
                    displayArray[curTurn.start.x, curTurn.start.y].bottom.Source = bmpFrom;
                    displayArray[curTurn.end.x, curTurn.end.y].bottom.Source = bmpTo;
                }

                if (captured.job == Job.King)
                {
                    castling(curTurn);//check if move is a castling
                }

                foreach (display d in displayArray)
                {
                    d.tile.Cursor = Cursors.Arrow;
                }
                await betweenTurns();
            }
            else    //if didn't select movable spot
            {
                clearSelectedAndPossible();
                movablePieceSelected = false;
            }
        }

        private async Task betweenTurns()
        {
            //In between light and dark's turns

            bool endOfGame;

            //change teams
            if (offensiveTeam == Color.Light)
            {
                offensiveTeam = Color.Dark;
                endOfGame = isInCheckmate(getDarkPieces(pieceArray), offensiveTeam);  //did previous turn put other team in checkmate?

                if (endOfGame == false && onePlayer == true)
                {
                    await compTurn();
                    offensiveTeam = Color.Light;
                    endOfGame = isInCheckmate(getLightPieces(pieceArray), offensiveTeam); //did computer turn put player in checkmate?
                }
            }
            else
            {
                offensiveTeam = Color.Light;
                endOfGame = isInCheckmate(getLightPieces(pieceArray), offensiveTeam); //did previous turn put other team in checkmate?

                if (endOfGame == false && onePlayer == true)
                {
                    await compTurn();
                    offensiveTeam = Color.Dark;
                    endOfGame = isInCheckmate(getDarkPieces(pieceArray), offensiveTeam); //did computer turn put player in checkmate?
                }
            }

            if(networkGame == true && endOfGame == false)
            {
                ready = false;
                //wait for response move
            }
            //if 2Player local, rotate is on, and didn't end game
            else if (twoPlayer == true && rotate == true && endOfGame == false)
            {
                if(offensiveTeam == Color.Dark)
                {
                    rotateBoard(true, rotationDuration);
                }
                else
                {
                    rotateBoard(false, rotationDuration);
                }
                onBottom = offensiveTeam;
            }
        }

        public async Task compTurn()
        {
            //computer's turn
            
            historyNode node;
            int index;

            ready = false;

            if (difficulty > 3)
            {
                think = new Thinking(this, rnd.Next(0, 41));
                think.Owner = mWindow;
                think.ShowDialog();
            }
            else
            {
                await Task.Run(() => minimax(pieceArray, offensiveTeam, -1, true, -30, 30, null));
            }

            ready = true;

            bool virginMove = pieceArray[getIndex(bestMove.start, pieceArray)].virgin;
            index = getIndex(bestMove.end, pieceArray);
            Piece captured = pieceArray[index];
            pieceArray[index].movePiece(bestMove.end, this);

            if (pieceArray[index].job == Job.Pawn)
            {
                if (bestMove.end.y == 0)
                {
                    pieceArray[index] = new Queen(bestMove.end, Color.Dark);
                    displayArray[bestMove.end.x, 0].top.Source = dQueen;
                    node = new historyNode(bestMove, captured, true, true, false);
                }
                else if (bestMove.end.y == 7)
                {
                    pieceArray[index] = new Queen(bestMove.end, Color.Light);
                    displayArray[bestMove.end.x, 7].top.Source = lQueen;
                    node = new historyNode(bestMove, captured, true, true, false);
                }
                //Pawn, but not transform
                node = new historyNode(bestMove, captured, false, true, virginMove);
            }
            else //not pawn
            {
                node = new historyNode(bestMove, captured, false, true, virginMove);
            }

            if(networkGame == false)
            {
                history.Push(node);
            }

            if (lastMove == true)
            {
                clearToAndFrom();
                displayArray[bestMove.start.x, bestMove.start.y].bottom.Source = bmpFrom;
                displayArray[bestMove.end.x, bestMove.end.y].bottom.Source = bmpTo;
            }

            if (pieceArray[index].job == Job.King)
            {
                castling(bestMove);//check if move is a castling
            }
        }

        public void undo()
        {
            //completely undo previous move

            BitmapImage pawnPic = new BitmapImage();
            Piece to;
            int toIndex;
            int fromIndex;

            historyNode node = history.Pop();

            fromIndex = getIndex(node.step.start, pieceArray);
            toIndex = getIndex(node.step.end, pieceArray);
            to = pieceArray[toIndex];
            offensiveTeam = to.color;

            if (rotate == true && twoPlayer == true)
            {
                if(offensiveTeam == Color.Dark)
                {
                    rotateBoard(true, rotationDuration);
                }
                else
                {
                    rotateBoard(false, rotationDuration);
                }
            }

            if (node.pawnTransform == true)
            {
                if (to.color == Color.Light)
                {
                    pawnPic = lPawn;
                }

                else
                {
                    pawnPic = dPawn;
                }

                pieceArray[fromIndex] = new Pawn(node.step.start, offensiveTeam, false);
                displayArray[node.step.start.x, node.step.start.y].top.Source = pawnPic;
            }

            else
            {
                pieceArray[fromIndex] = to;
                pieceArray[fromIndex].virgin = node.firstMove;
                displayArray[node.step.start.x, node.step.start.y].top.Source = matchPicture(to);
            }

            //put captured piece back
            pieceArray[toIndex] = node.captured;
            displayArray[node.step.end.x, node.step.end.y].top.Source = matchPicture(node.captured);

            if (node.skip == true)
            {
                undo(); //call function again to undo another move
            }
            //if stack is empty, disable button; skip and empty stack can't both happen
            else if (history.Count == 0)
            {
                mWindow.undoMenu.IsEnabled = false;
            }
            clearToAndFrom();
            clearSelectedAndPossible();
        }

        public async void rotateBoard(bool bottomLightToDark, double time)
        {
            //performs rotate animation

            if (ready == true)
            {
                ready = false;

                DoubleAnimation rotation;
                double spaceSize;
                RotateTransform rt = new RotateTransform();
                ScaleTransform ft = new ScaleTransform();
                double gridHeight = mWindow.uGrid.ActualHeight;
                double gridWidth = mWindow.uGrid.ActualWidth;
                Point middle = new Point(.5, .5);
                mWindow.uGrid.RenderTransformOrigin = middle;

                if (mWindow.space.ActualHeight > mWindow.space.ActualWidth)
                {
                    spaceSize = mWindow.space.ActualWidth;
                }
                else
                {
                    spaceSize = mWindow.space.ActualHeight;
                }

                spaceSize *= .66;
                DoubleAnimation shrink = new DoubleAnimation(gridHeight, spaceSize, TimeSpan.FromSeconds(time * .15));
                DoubleAnimation expand =
                    new DoubleAnimation(spaceSize, gridHeight, TimeSpan.FromSeconds(time * .15), FillBehavior.Stop);

                if (bottomLightToDark == true)
                {
                    rotation = new DoubleAnimation(0, 180, TimeSpan.FromSeconds(time));
                    ft.ScaleY = -1;
                    ft.ScaleX = -1;
                }

                else
                {
                    rotation = new DoubleAnimation(180, 360, TimeSpan.FromSeconds(time));
                    ft.ScaleY = 1;
                    ft.ScaleX = 1;
                }

                //shrink
                mWindow.uGrid.BeginAnimation(Grid.HeightProperty, shrink);
                mWindow.uGrid.BeginAnimation(Grid.WidthProperty, shrink);

                //rotate
                mWindow.uGrid.RenderTransform = rt;
                rt.BeginAnimation(RotateTransform.AngleProperty, rotation);

                await Task.Delay((int)(time * 850));

                //expand
                mWindow.uGrid.BeginAnimation(Grid.HeightProperty, expand);
                mWindow.uGrid.BeginAnimation(Grid.WidthProperty, expand);

                await Task.Delay((int)(time * 150));

                //flip
                foreach (display cell in displayArray)
                {
                    cell.top.RenderTransformOrigin = middle;
                    cell.top.RenderTransform = ft;
                }
                ready = true;
            }
        }

        public BitmapImage matchPicture(Piece figure)
        {
            //returns image based on what piece it is

            if (figure.color == Color.Dark)
            {
                switch (figure.job)
                {
                    case Job.King:
                        return dKing;
                    case Job.Queen:
                        return dQueen;
                    case Job.Bishop:
                        return dBishop;
                    case Job.Knight:
                        return dKnight;
                    case Job.Rook:
                        return dRook;
                    case Job.Pawn:
                        return dPawn;
                    default:
                        return null;
                }
            }
            else
            {
                switch (figure.job)
                {
                    case Job.King:
                        return lKing;
                    case Job.Queen:
                        return lQueen;
                    case Job.Bishop:
                        return lBishop;
                    case Job.Knight:
                        return lKnight;
                    case Job.Rook:
                        return lRook;
                    case Job.Pawn:
                        return lPawn;
                    default:
                        return null;
                }
            }
        }

        public void clearSelectedAndPossible()
        {
            //resets Background to get rid of green, blue, and red squares

            int sum;

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    sum = x + y;
                    if(sum % 2 == 0)
                    {
                        displayArray[x, y].tile.Background = Brushes.DarkGray;
                    }

                    else
                    {
                        displayArray[x, y].tile.Background = Brushes.White;
                    }
                }
            }
        }

        public void clearToAndFrom()
        {
            //clears last move indicators from board

            foreach(display cell in displayArray)
            {
                cell.bottom.Source = null;
            }
        }

        public void changeThemeVisually()
        {
            //calls matchPicture() on each piece and puts image in displayArray

            for (int i = 0; i < 32; i++)
            {
                if (pieceArray[i].dead == false)
                {
                    displayArray[pieceArray[i].coor.x, pieceArray[i].coor.y].top.Source = matchPicture(pieceArray[i]);
                }
            }
        }

        public void changeThemeInternally()
        {
            //sets image variables based on themeIndex

            string uriPath;
            string themeName = themeList[themeIndex];

            if (themeIndex == 0)
            {
                uriPath = "pack://application:,,,/Resources/";
            }
            else
            {
                uriPath = "pack://application:,,,/" + themeName + ";component/";
            }
            
            try
            {
                lKing = new BitmapImage(new Uri(uriPath + "lKing.png"));
                lQueen = new BitmapImage(new Uri(uriPath + "lQueen.png"));
                lBishop = new BitmapImage(new Uri(uriPath + "lBishop.png"));
                lKnight = new BitmapImage(new Uri(uriPath + "lKnight.png"));
                lRook = new BitmapImage(new Uri(uriPath + "lRook.png"));
                lPawn = new BitmapImage(new Uri(uriPath + "lPawn.png"));
                dKing = new BitmapImage(new Uri(uriPath + "dKing.png"));
                dQueen = new BitmapImage(new Uri(uriPath + "dQueen.png"));
                dBishop = new BitmapImage(new Uri(uriPath + "dBishop.png"));
                dKnight = new BitmapImage(new Uri(uriPath + "dKnight.png"));
                dRook = new BitmapImage(new Uri(uriPath + "dRook.png"));
                dPawn = new BitmapImage(new Uri(uriPath + "dPawn.png"));
            }
            catch (IOException)
            {
                themeList.RemoveAt(themeIndex);
                themeIndex--;
            }
        }

        public void populateThemeList()
        {
            //searches dlls in working directory and puts in themeList

            AssemblyName an;
            FileInfo file;
            themeList = new List<string>();
            ignore = new List<string>();
            string[] dllFilePathArray = null;
            bool themesFound = false;

            while (themesFound == false)
            {
                try
                {
                    themeList.Add("Figure");
                    dllFilePathArray = Directory.GetFiles(pwd, "*.dll");
                    themeIndex = 1;

                    foreach (string dllFilePath in dllFilePathArray.Except(ignore))
                    {
                        file = new FileInfo(dllFilePath);
                        Unblock.DeleteAlternateDataStream(file, "Zone.Identifier");
                        an = AssemblyName.GetAssemblyName(dllFilePath);
                        Assembly.Load(an);

                        if (!themeList.Contains(an.Name))  //check for duplicates
                        {
                            themeList.Add(an.Name);
                            changeThemeInternally();    //check to see if can set to variable
                            themeIndex++;
                        }
                    }
                }
                catch (BadImageFormatException ex)
                {
                    ignore.Add(pwd + "\\" + ex.FileName);
                    themeList.Clear();
                    continue;
                }

                themesFound = true;
                themeIndex = 0;
                changeThemeInternally();
            }
        }

        public void saveState()
        {
            //saves game on exit
            //if save game unchecked, saves preferences, but not game state

            int sizeX = (int)mWindow.ActualWidth;
            int sizeY = (int)mWindow.ActualHeight;

            if(networkGame == true && client.Connected == true)
            {
                sizeX -= 300;
            }

            string theme = themeList[themeIndex];
            saveData sData = new saveData(pieceArray, offensiveTeam, opponent, onBottom, theme, onePlayer, twoPlayer, networkGame, difficulty,
                lastMove, saveGame, ready, rotate, rotationDuration, sizeX, sizeY);

            System.IO.Directory.CreateDirectory(dirPath);
            BinaryFormatter writer = new BinaryFormatter();
            FileStream saveStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            writer.Serialize(saveStream, sData);
            saveStream.Close();
        }

        public void loadState()
        {
            //look for a game save and load it

            BinaryFormatter reader = new BinaryFormatter();
            FileStream loadStream = new FileStream(filePath, FileMode.Open);

            try
            {
                saveData lData = (saveData)reader.Deserialize(loadStream);  //load file
                loadStream.Close();

                //load preferences regardless of whether saveGame was enabled
                lastMove = lData.sLastMove;
                rotate = lData.sRotate;
                rotationDuration = lData.sRduration;
                string theme = lData.sTheme;
                mWindow.Width = lData.sSizeX;
                mWindow.Height = lData.sSizeY;

                if (lData.sSaveGame == true)
                {
                    if (lData.sReady == true && lData.sNetwork == false)
                    {
                        ready = true;
                        pieceArray = new Piece[33];
                        movablePieceSelected = false;
                        pieceArray = lData.sBoard;
                        offensiveTeam = lData.sOffensiveTeam;
                        opponent = lData.sOpponent;
                        onBottom = lData.sOnBottom;
                        onePlayer = lData.sOnePlayer;
                        twoPlayer = lData.sTwoPlayer;
                        difficulty = lData.sDifficulty;

                        if (onBottom == Color.Dark)
                        {
                            rotateBoard(true, 0);
                        }

                    }
                    else    //Exit while game not being played
                    {
                        newGame();
                    }
                }
                else    //Exit with saveGame set to false
                {
                    saveGame = false;
                    newGame();
                }

                for (int i = 0; i < themeList.Count(); i++)
                {
                    if (themeList[i] == theme)
                    {
                        themeIndex = i;
                    }
                }
                changeThemeInternally();

                if (ready == true)
                {
                    changeThemeVisually();
                }
            }
            catch (Exception) //Error loading data
            {
                newGame();
            }
        }

        public void rotateOptionToggled()
        {
            //when rotate option is toggled

            if (offensiveTeam == opponent)  //if opponent's turn
            {
                clearSelectedAndPossible();
                //if toggled on
                if(rotate == false)
                {
                    if(offensiveTeam == Color.Dark)
                    {
                        rotateBoard(true, 0);
                    }
                    else
                    {
                        rotateBoard(false, 0);
                    }
                }
                //toggled off
                else
                {
                    if (offensiveTeam == Color.Dark)
                    {
                        rotateBoard(false, 0);
                    }
                    else
                    {
                        rotateBoard(true, 0);
                    }
                }
                clearToAndFrom();
            }
        }

        public void newGame()
        {
            //call newGame window

            NewGame play;
            MessageBoxResult result = MessageBoxResult.Yes;

            //if closing a network game
            if (networkGame == true)
            {
                if (client.Connected == true)
                {
                    result = MessageBox.Show("Online game in progress\nAre you sure you want to quit?",
                        "Quit Online Game", MessageBoxButton.YesNo, MessageBoxImage.None, MessageBoxResult.No);

                    if (result == MessageBoxResult.Yes)
                    {
                        byte[] newLocalGame = new byte[1] { 1 };
                        nwStream.Write(newLocalGame, 0, 1);
                        nwStream.Close();
                        client.Close();
                    }
                }
            }
            if (result == MessageBoxResult.Yes)
            {
                play = new NewGame(this);
                play.ShowDialog();
            }
        }

        public async void continuousReader()
        {
            //Runs constantly in the background for network games to intercept server messages

            int bytesRead;

            while(true)
            {
                try
                {
                    bytesRead = await nwStream.ReadAsync(buffer, 0, 255);
                }
                //Server crashes during play
                catch(System.IO.IOException)
                {
                    MessageBox.Show(Application.Current.MainWindow, "You have been disconnected from the Server",
                        "Disconnected", MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.OK);

                    ready = false;
                    break;
                }
                //You quit a network game
                catch(ObjectDisposedException)
                {
                    ready = false;
                    break;
                }

                
                if (buffer[0] == 3)
                {
                    MessageBox.Show(Application.Current.MainWindow, "Your opponent has forfeited the match",
                        "Game Over", MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.OK);

                    ready = false;
                    break;
                }
                else if (buffer[0] == 4)
                {
                    MessageBox.Show(Application.Current.MainWindow,
                        "Sorry\n\nYou gave a valiant effort,\nbut you have been bested in battle by the enemy army",
                        "Game Over", MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.OK);

                    ready = false;
                    break;
                }
                //move
                else if (buffer[0] == 7)
                {
                    coordinate s;
                    coordinate e;

                    if(opponent == Color.Light)
                    {
                        s = new coordinate(buffer[1], 7 - buffer[2]);
                        e = new coordinate(buffer[3], 7 - buffer[4]);
                    }
                    else
                    {
                        s = new coordinate(buffer[1], buffer[2]);
                        e = new coordinate(buffer[3], buffer[4]);
                    }

                    move opponentsMove = new move(new coordinate(s.x, s.y), new coordinate(e.x, e.y));
                    networkMove(opponentsMove, buffer[5]);
                }
                //message
                else
                {
                    string chatMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    mWindow.respone(chatMessage);
                }
            }
            nwStream.Close();
            client.Close();
            mWindow.removeChat();
            clearToAndFrom();
            clearSelectedAndPossible();

            foreach (display d in displayArray)
            {
                d.tile.Cursor = Cursors.Arrow;
            }
        }

        private void networkMove(move m, byte pawn)
        {
            //Executes move locally sent from other client

            int index = getIndex(m.end, pieceArray);

            //movePiece
            pieceArray[getIndex(m.start, pieceArray)].movePiece(m.end, this);

            //pawnTransform
            if (pieceArray[index].job == Job.Pawn)
            {
                if (m.end.y == 0)
                {
                    switch (pawn)
                    {
                        case 4:
                            pieceArray[index] = new Queen(m.end, Color.Dark);
                            displayArray[m.end.x, 0].top.Source = dQueen;
                            break;
                        case 3:
                            pieceArray[index] = new Rook(m.end, Color.Dark, false);
                            displayArray[m.end.x, 0].top.Source = dRook;
                            break;
                        case 2:
                            pieceArray[index] = new Bishop(m.end, Color.Dark);
                            displayArray[m.end.x, 0].top.Source = dBishop;
                            break;
                        case 1:
                            pieceArray[index] = new Knight(m.end, Color.Dark);
                            displayArray[m.end.x, 0].top.Source = dKnight;
                            break;
                    }
                }
                else if(m.end.y == 7)
                {
                    switch (pawn)
                    {
                        case 4:
                            pieceArray[index] = new Queen(m.end, Color.Light);
                            displayArray[m.end.x, 7].top.Source = lQueen;
                            break;
                        case 3:
                            pieceArray[index] = new Rook(m.end, Color.Light, false);
                            displayArray[m.end.x, 7].top.Source = lRook;
                            break;
                        case 2:
                            pieceArray[index] = new Bishop(m.end, Color.Light);
                            displayArray[m.end.x, 7].top.Source = lBishop;
                            break;
                        case 1:
                            pieceArray[index] = new Knight(m.end, Color.Light);
                            displayArray[m.end.x, 7].top.Source = lKnight;
                            break;
                    }
                }
            }
            //clear then set to and from
            if (lastMove == true)
            {
                clearToAndFrom();
                displayArray[m.start.x, m.start.y].bottom.Source = bmpFrom;
                displayArray[m.end.x, m.end.y].bottom.Source = bmpTo;
            }
            //check for castling
            if (pieceArray[index].job == Job.King)
            {
                castling(m);
            }

            offensiveTeam = switchTeam(offensiveTeam);
            ready = true;
        }

        public static Color switchTeam(Color input)
        {
            //Returns opposite of team given

            if(input == Color.Light)
            {
                return Color.Dark;
            }
            else
            {
                return Color.Light;
            }
        }

        public static int getIndex(coordinate coor, Piece[] board)
        {
            for (int i = 0; i < 32; i++)
            {
                if (board[i].coor == coor)
                {
                    return i;
                }
            }
            return 32;
        }
    }
}
