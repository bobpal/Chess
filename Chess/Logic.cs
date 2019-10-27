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

namespace Chess
{
    public enum Color
    {
        None,
        Light,
        Dark
    }

    public enum Job
    {
        None,
        Pawn,
        Knight,
        Bishop,
        Rook,
        Queen,
        King
    }

    [Serializable]
    public class Logic
    {
        [NonSerialized]
        public MainWindow mWindow;                  //window with chess board on it
        public piece[,] pieceArray;                 //8x8 array of pieces
        public display[,] displayArray;             //8x8 array of display objects
        public bool onePlayer;                      //versus computer
        public bool twoPlayer;                      //2 player local
        public bool networkGame;                    //playing over a network
        public Color opponentColor;                 //color of computer or 2nd player
        public Color offensiveColor;                //which side is on the offense
        public Color onBottomColor = Color.Light;   //which team is on the bottom of the screen
        public int difficulty;                      //difficulty level
        public bool ready;                          //blocks functionality for unwanted circumstances
        private coordinate prevSelected;            //where the cursor clicked previously
        public List<string> themeList;              //list of themes
        public int themeIndex;                      //which theme is currently in use
        private List<string> ignore;                //list of which dll files to ignore
        [NonSerialized] public BitmapImage lKing;
        [NonSerialized] public BitmapImage lQueen;
        [NonSerialized] public BitmapImage lBishop;
        [NonSerialized] public BitmapImage lKnight;
        [NonSerialized] public BitmapImage lRook;
        [NonSerialized] private BitmapImage lPawn;
        [NonSerialized] private BitmapImage dKing;
        [NonSerialized] public BitmapImage dQueen;
        [NonSerialized] public BitmapImage dBishop;
        [NonSerialized] public BitmapImage dKnight;
        [NonSerialized] public BitmapImage dRook;
        [NonSerialized] private BitmapImage dPawn;
        [NonSerialized] public TcpClient client;
        [NonSerialized] public NetworkStream nwStream;
        public byte[] buffer = new byte[255];                   //buffer for tcp
        private static Random rnd = new Random();               //used for all random situations
        private move bestMove;                                  //final return for minimax()
        [NonSerialized] private Thinking think;                 //progress bar window for hardMode
        public bool rotate = true;                              //Rotate board between turns on 2Player mode?
        public bool lastMove = true;                            //is lastMove menu option checked?
        public bool saveGame = true;                            //Save game on exit?
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

            public piece[,] sBoard { get; private set; }
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

            public saveData(piece[,] _board, Color _offensiveTeam, Color _opponent, Color _onBottom, string _theme, bool _onePlayer, bool _twoPlayer,
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
        public struct piece
        {
            //represents a movable piece

            public Color color { get; set; }    //on dark or light team?
            public Job job { get; set; }        //piece's job
            public bool virgin { get; set; }    //has piece never moved?
        }

        [Serializable]
        public struct coordinate
        {
            //a spot on the board

            public int x { get; set; }
            public int y { get; set; }

            public coordinate(int _x, int _y)
                : this()
            {
                this.x = _x;
                this.y = _y;
            }
        }

        [Serializable]
        public class move
        {
            //represents a move that a piece can do

            public coordinate start { get; set; }   //starting position
            public coordinate end { get; set; }    //ending position

            public move(coordinate _start, coordinate _end)
            {
                this.start = _start;
                this.end = _end;
            }

            public move() { }
        }

        [Serializable]
        public class historyNode
        {
            //contains all information needed to undo move

            public move step { get; set; }               //move that happened previously
            public piece captured { get; set; }          //piece that move captured, if no capture, use null
            public bool pawnTransform { get; set; }      //did a pawn transformation happen?
            public bool skip { get; set; }               //undo next move also if playing against computer
            public bool firstMove { get; set; }          //was this the piece's first move?

            public historyNode(move _step, piece _captured, bool _pawnTransform, bool _skip, bool _firstMove)
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
            public Canvas tile { get; set; }
            public Image bottom { get; set; }
            public Image top { get; set; }

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

            pieceArray = new piece[8, 8];

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    if (y == 0)
                    {
                        pieceArray[x, 0].color = Color.Light;
                    }

                    else if (y == 1)
                    {
                        pieceArray[x, 1].color = Color.Light;
                        pieceArray[x, 1].job = Job.Pawn;
                        pieceArray[x, 1].virgin = true;
                        displayArray[x, 1].top.Source = matchPicture(pieceArray[x, 1]);
                    }

                    else if (y == 6)
                    {
                        pieceArray[x, 6].color = Color.Dark;
                        pieceArray[x, 6].job = Job.Pawn;
                        pieceArray[x, 6].virgin = true;
                        displayArray[x, 6].top.Source = matchPicture(pieceArray[x, 6]);
                    }

                    else if (y == 7)
                    {
                        pieceArray[x, 7].color = Color.Dark;
                    }

                    else
                    {
                        displayArray[x, y].top.Source = null;
                    }
                }
            }
            pieceArray[0, 0].virgin = true;
            pieceArray[4, 0].virgin = true;
            pieceArray[7, 0].virgin = true;
            pieceArray[0, 7].virgin = true;
            pieceArray[4, 7].virgin = true;
            pieceArray[7, 7].virgin = true;

            pieceArray[0, 0].job = Job.Rook;
            pieceArray[1, 0].job = Job.Knight;
            pieceArray[2, 0].job = Job.Bishop;
            pieceArray[3, 0].job = Job.Queen;
            pieceArray[4, 0].job = Job.King;
            pieceArray[5, 0].job = Job.Bishop;
            pieceArray[6, 0].job = Job.Knight;
            pieceArray[7, 0].job = Job.Rook;
            pieceArray[0, 7].job = Job.Rook;
            pieceArray[1, 7].job = Job.Knight;
            pieceArray[2, 7].job = Job.Bishop;
            pieceArray[3, 7].job = Job.Queen;
            pieceArray[4, 7].job = Job.King;
            pieceArray[5, 7].job = Job.Bishop;
            pieceArray[6, 7].job = Job.Knight;
            pieceArray[7, 7].job = Job.Rook;

            displayArray[0, 0].top.Source = lRook;
            displayArray[1, 0].top.Source = lKnight;
            displayArray[2, 0].top.Source = lBishop;
            displayArray[3, 0].top.Source = lQueen;
            displayArray[4, 0].top.Source = lKing;
            displayArray[5, 0].top.Source = lBishop;
            displayArray[6, 0].top.Source = lKnight;
            displayArray[7, 0].top.Source = lRook;
            displayArray[0, 7].top.Source = dRook;
            displayArray[1, 7].top.Source = dKnight;
            displayArray[2, 7].top.Source = dBishop;
            displayArray[3, 7].top.Source = dQueen;
            displayArray[4, 7].top.Source = dKing;
            displayArray[5, 7].top.Source = dBishop;
            displayArray[6, 7].top.Source = dKnight;
            displayArray[7, 7].top.Source = dRook;
        }

        private List<coordinate> getPieces(piece[,] board, Color color)
        {
            //searches through pieceArray and returns list of coordinates where all color specified pieces are located

            coordinate temp = new coordinate();
            List<coordinate> foundPieces = new List<coordinate>();

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    if (board[x, y].color == color)
                    {
                        temp.x = x;
                        temp.y = y;
                        foundPieces.Add(temp);
                    }
                }
            }
            return foundPieces;
        }

        private List<coordinate> getAllPieces(piece[,] board)
        {
            //searches through pieceArray and returns list of coordinates where all pieces are located

            coordinate temp = new coordinate();
            List<coordinate> allPieces = new List<coordinate>();

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    if (board[x, y].color != Color.None)
                    {
                        temp.x = x;
                        temp.y = y;
                        allPieces.Add(temp);
                    }
                }
            }
            return allPieces;
        }

        private List<move> getMoves(coordinate spot, piece[,] grid)
        {
            //returns all possible moves of spot disregarding check restrictions
            //determines job of piece and calls apropriate function to get correct move list

            List<move> temp = new List<move>();

            switch (grid[spot.x, spot.y].job)
            {
                case Job.Rook:
                    return rookMoves(spot, grid);
                case Job.Knight:
                    return knightMoves(spot, grid);
                case Job.Bishop:
                    return bishopMoves(spot, grid);
                case Job.Queen:
                    temp.AddRange(bishopMoves(spot, grid));
                    temp.AddRange(rookMoves(spot, grid));
                    return temp;
                case Job.King:
                    return kingMoves(spot, grid);
                case Job.Pawn:
                    return pawnMoves(spot, grid);
                default:
                    return temp;    //temp should be empty
            }
        }

        public int minimax(piece[,] board, Color attacking, int level, bool computerTurn, int alpha, int beta, IProgress<int> progress)
        {
            //is called recursively for comp to look ahead and return the best move
            //initial call parameters:
            //board = current board
            //attacking = computer color
            //level = -1
            //computerTurn = true
            //alpha = -99
            //beta = 99

            int bestVal;
            List<move> offensiveMoves = new List<move>();

            level++;
            //if not on bottom level, go down
            if(level < difficulty)
            {
                int val;
                piece[,] newBoard;
                int indexOfBest = 0;

                Color nextTurn = switchTeam(attacking);

                foreach (coordinate cell in getPieces(board, attacking))
                {
                    offensiveMoves.AddRange(getCheckRestrictedMoves(cell, board));
                }

                if (computerTurn == true)
                {
                    bestVal = -99;

                    //if not checkmate
                    if (offensiveMoves.Count > 0)
                    {
                        if (level == 0)
                        {
                            //the only difference from the else statement is the progress updates
                            for (int i = 0; i < offensiveMoves.Count; i++)
                            {
                                newBoard = deepCopy(board);
                                newBoard = privateMove(newBoard, offensiveMoves[i]);
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
                                newBoard = privateMove(newBoard, offensiveMoves[i]);
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
                    bestVal = 99;

                    //if checkmate
                    if (offensiveMoves.Count == 0){}
                    else
                    {
                        for (int i = 0; i < offensiveMoves.Count; i++)
                        {
                            newBoard = deepCopy(board);
                            newBoard = privateMove(newBoard, offensiveMoves[i]);
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

        private piece[,] privateMove(piece[,] grid, move mv)
        {
            //does a move without visuals on a private board for computer

            Color offense = grid[mv.start.x, mv.start.y].color;

            //move to new cell
            grid[mv.end.x, mv.end.y].color = offense;
            grid[mv.end.x, mv.end.y].job = grid[mv.start.x, mv.start.y].job;
            grid[mv.end.x, mv.end.y].virgin = false;

            //clear old cell
            grid[mv.start.x, mv.start.y].color = Color.None;
            grid[mv.start.x, mv.start.y].job = Job.None;
            grid[mv.start.x, mv.start.y].virgin = false;

            //check for pawnTransform
            if (grid[mv.end.x, mv.end.y].job == Job.Pawn)
            {
                if (offense == Color.Dark && mv.end.y == 0 || offense == Color.Light && mv.end.y == 7)
                {
                    grid[mv.end.x, mv.end.y].job = Job.Queen;
                }
            }
            //check for castling
            else if (grid[mv.end.x, mv.end.y].job == Job.King)
            {
                int castleY;

                if (offense == Color.Dark)
                {
                    castleY = 7;
                }
                else
                {
                    castleY = 0;
                }

                if (mv.start.x == 4 && mv.start.y == castleY)  //if moving from King default position
                {
                    if (mv.end.x == 2 && mv.end.y == castleY)  //if moving two spaces to the left
                    {
                        //move to new cell
                        grid[3, castleY].color = offense;
                        grid[3, castleY].job = Job.Rook;
                        grid[3, castleY].virgin = false;

                        //clear old cell
                        grid[0, castleY].color = Color.None;
                        grid[0, castleY].job = Job.None;
                        grid[0, castleY].virgin = false;
                    }

                    else if (mv.end.x == 6 && mv.end.y == castleY) //if moving two spaces to the right
                    {
                        //move to new cell
                        grid[5, castleY].color = offense;
                        grid[5, castleY].job = Job.Rook;
                        grid[5, castleY].virgin = false;

                        //clear old cell
                        grid[7, castleY].color = Color.None;
                        grid[7, castleY].job = Job.None;
                        grid[7, castleY].virgin = false;
                    }
                }
            }
            return grid;
        }

        private int evaluator(piece[,] grid)
        {
            //looks at board and returns a value that indicates how good the computer is doing

            int total = 0;

            if (opponentColor == Color.Light)
            {
                for (int y = 0; y < 8; y++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        if (grid[x, y].color == Color.Light)
                        {
                            //computer pieces
                            total += evalAdd(grid[x, y].job);
                        }
                        else if (grid[x, y].color == Color.Dark)
                        {
                            //human pieces
                            total -= evalAdd(grid[x, y].job);
                        }
                    }
                }
            }
            else
            {
                for (int y = 0; y < 8; y++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        if (grid[x, y].color == Color.Light)
                        {
                            //human pieces
                            total -= evalAdd(grid[x, y].job);
                        }
                        else if (grid[x, y].color == Color.Dark)
                        {
                            //computer pieces
                            total += evalAdd(grid[x, y].job);
                        }
                    }
                }
            }
            return total;
        }

        private int evalAdd(Job j)
        {
            switch (j)
            {
                case Job.Queen:
                    return 9;
                case Job.Rook:
                    return 5;
                case Job.Bishop:
                    return 3;
                case Job.Knight:
                    return 3;
                case Job.Pawn:
                    return 1;
                default:
                    return 0;
            }
        }

        private piece[,] deepCopy(piece[,] source)
        {
            //Creates a deep copy of board

            piece[,] copy = new piece[8,8];

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    copy[x, y].job = source[x, y].job;
                    copy[x, y].color = source[x, y].color;
                    copy[x, y].virgin = source[x, y].virgin;
                }
            }
            return copy;
        }

        private bool isInCheck(Color teamInQuestion, piece[,] board)
        {
            //returns whether or not team in question is in check

            List<move> poss = new List<move>();

            //get all of opposing team's pieces
            //get possible moves of opposing team,
            //doesn't matter if opposing team move gets them in check,
            //still a valid move for current team
            foreach (coordinate c in getPieces(board, teamInQuestion))
            {
                poss.AddRange(getMoves(c, board));
            }

            foreach (move m in poss)
            {
                //if opposing team's move can capture your king, you're in check
                if (board[m.end.x, m.end.y].job == Job.King &&
                    board[m.end.x, m.end.y].color == teamInQuestion)
                {
                    return true;
                }
            }
            return false;
        }

        private List<move> getCheckRestrictedMoves(coordinate aPiece, piece[,] grid)
        {
            //takes single piece and returns list of moves that don't put player in check

            List<move> allPossible = new List<move>();
            List<move> possibleWithoutCheck = new List<move>();
            move rookMove = null;
            coordinate rookStart = new coordinate();
            coordinate rookEnd = new coordinate();
            int castleY;
            Color fromColor;
            bool fromVirgin;
            Color toColor;
            Job toJob;
            bool toVirgin;
            bool inCheck;

            if (offensiveColor == Color.Dark)
            {
                castleY = 7;
            }
            else
            {
                castleY = 0;
            }

            allPossible = getMoves(aPiece, grid);

            foreach (move m in allPossible)
            {
                toColor = grid[m.end.x, m.end.y].color;
                toJob = grid[m.end.x, m.end.y].job;
                toVirgin = grid[m.end.x, m.end.y].virgin;
                fromColor = grid[m.start.x, m.start.y].color;
                fromVirgin = grid[m.start.x, m.start.y].virgin;

                if (grid[m.start.x, m.start.y].job == Job.King)
                {
                    if (m.start.x == 4 && m.start.y == castleY)  //if moving from King default position
                    {
                        if (m.end.x == 2 && m.end.y == castleY)  //if moving two spaces to the left
                        {
                            rookStart.x = 0;
                            rookStart.y = castleY;
                            rookEnd.x = 3;
                            rookEnd.y = castleY;
                            rookMove = new move(rookStart, rookEnd);
                        }

                        else if (m.end.x == 6 && m.end.y == castleY) //if moving two spaces to the right
                        {
                            rookStart.x = 7;
                            rookStart.y = castleY;
                            rookEnd.x = 5;
                            rookEnd.y = castleY;
                            rookMove = new move(rookStart, rookEnd);
                        }
                    }
                }

                //do moves
                //main
                grid[m.end.x, m.end.y].color = fromColor;
                grid[m.end.x, m.end.y].job = grid[m.start.x, m.start.y].job;
                grid[m.end.x, m.end.y].virgin = false;
                grid[m.start.x, m.start.y].color = Color.None;
                grid[m.start.x, m.start.y].job = Job.None;
                grid[m.start.x, m.start.y].virgin = false;
                //rook
                if (rookMove != null)
                {
                    grid[rookEnd.x, rookEnd.y].color = grid[rookStart.x, rookStart.y].color;
                    grid[rookEnd.x, rookEnd.y].job = Job.Rook;
                    grid[rookEnd.x, rookEnd.y].virgin = false;
                    grid[rookStart.x, rookStart.y].color = Color.None;
                    grid[rookStart.x, rookStart.y].job = Job.None;
                    grid[rookStart.x, rookStart.y].virgin = false;
                }

                //see if in check
                inCheck = isInCheck(fromColor, grid);

                if (inCheck == false)//if not in check
                {
                    possibleWithoutCheck.Add(m);
                }

                //reset pieces
                //main
                grid[m.start.x, m.start.y].color = fromColor;
                grid[m.start.x, m.start.y].job = grid[m.end.x, m.end.y].job;
                grid[m.start.x, m.start.y].virgin = fromVirgin;
                grid[m.end.x, m.end.y].color = toColor;
                grid[m.end.x, m.end.y].job = toJob;
                grid[m.end.x, m.end.y].virgin = toVirgin;
                //rook
                if (rookMove != null)
                {
                    grid[rookStart.x, rookStart.y].color = grid[rookEnd.x, rookEnd.y].color;
                    grid[rookStart.x, rookStart.y].job = Job.Rook;
                    grid[rookStart.x, rookStart.y].virgin = true;
                    grid[rookEnd.x, rookEnd.y].color = Color.None;
                    grid[rookEnd.x, rookEnd.y].job = Job.None;
                    grid[rookEnd.x, rookEnd.y].virgin = false;
                }
                rookMove = null;
            }
            return possibleWithoutCheck;
        }

        private bool isInCheckmate(Color teamInQuestion, List<coordinate> availablePieces)
        {
            //takes list of pieces and returns whether or not player is in checkmate

            List<move> allPossible = new List<move>();
            List<move> possibleWithoutCheck = new List<move>();
            move rookMove = null;
            coordinate rookStart = new coordinate();
            coordinate rookEnd = new coordinate();
            int castleY;
            Color fromColor;
            bool fromVirgin;
            Color toColor;
            Job toJob;
            bool toVirgin;
            bool inCheck;

            if (offensiveColor == Color.Dark)
            {
                castleY = 7;
            }
            else
            {
                castleY = 0;
            }

            //find all moves that can be done without going into check
            foreach (coordinate aPiece in availablePieces)
            {
                allPossible = getMoves(aPiece, pieceArray);

                foreach (move m in allPossible)
                {
                    toColor = pieceArray[m.end.x, m.end.y].color;
                    toJob = pieceArray[m.end.x, m.end.y].job;
                    toVirgin = pieceArray[m.end.x, m.end.y].virgin;
                    fromColor = pieceArray[m.start.x, m.start.y].color;
                    fromVirgin = pieceArray[m.start.x, m.start.y].virgin;

                    if (pieceArray[m.start.x, m.start.y].job == Job.King)
                    {
                        if (m.start.x == 4 && m.start.y == castleY)  //if moving from King default position
                        {
                            if (m.end.x == 2 && m.end.y == castleY)  //if moving two spaces to the left
                            {
                                rookStart.x = 0;
                                rookStart.y = castleY;
                                rookEnd.x = 3;
                                rookEnd.y = castleY;
                                rookMove = new move(rookStart, rookEnd);
                            }

                            else if (m.end.x == 6 && m.end.y == castleY) //if moving two spaces to the right
                            {
                                rookStart.x = 7;
                                rookStart.y = castleY;
                                rookEnd.x = 5;
                                rookEnd.y = castleY;
                                rookMove = new move(rookStart, rookEnd);
                            }
                        }
                    }

                    //do moves
                    //main
                    pieceArray[m.end.x, m.end.y].color = fromColor;
                    pieceArray[m.end.x, m.end.y].job = pieceArray[m.start.x, m.start.y].job;
                    pieceArray[m.end.x, m.end.y].virgin = false;
                    pieceArray[m.start.x, m.start.y].color = Color.None;
                    pieceArray[m.start.x, m.start.y].job = Job.None;
                    pieceArray[m.start.x, m.start.y].virgin = false;
                    //rook
                    if (rookMove != null)
                    {
                        pieceArray[rookEnd.x, rookEnd.y].color = pieceArray[rookStart.x, rookStart.y].color;
                        pieceArray[rookEnd.x, rookEnd.y].job = Job.Rook;
                        pieceArray[rookEnd.x, rookEnd.y].virgin = false;
                        pieceArray[rookStart.x, rookStart.y].color = Color.None;
                        pieceArray[rookStart.x, rookStart.y].job = Job.None;
                        pieceArray[rookStart.x, rookStart.y].virgin = false;
                    }

                    //see if in check
                    inCheck = isInCheck(teamInQuestion, pieceArray);

                    if (inCheck == false)//if not in check
                    {
                        possibleWithoutCheck.Add(m);
                    }

                    //reset pieces
                    //main
                    pieceArray[m.start.x, m.start.y].color = fromColor;
                    pieceArray[m.start.x, m.start.y].job = pieceArray[m.end.x, m.end.y].job;
                    pieceArray[m.start.x, m.start.y].virgin = fromVirgin;
                    pieceArray[m.end.x, m.end.y].color = toColor;
                    pieceArray[m.end.x, m.end.y].job = toJob;
                    pieceArray[m.end.x, m.end.y].virgin = toVirgin;
                    //rook
                    if (rookMove != null)
                    {
                        pieceArray[rookStart.x, rookStart.y].color = pieceArray[rookEnd.x, rookEnd.y].color;
                        pieceArray[rookStart.x, rookStart.y].job = Job.Rook;
                        pieceArray[rookStart.x, rookStart.y].virgin = true;
                        pieceArray[rookEnd.x, rookEnd.y].color = Color.None;
                        pieceArray[rookEnd.x, rookEnd.y].job = Job.None;
                        pieceArray[rookEnd.x, rookEnd.y].virgin = false;
                    }
                    rookMove = null;
                }
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
                    Color winningTeam = switchTeam(teamInQuestion);
                    message = "The " + winningTeam.ToString().ToLower() + " army has slain the " + teamInQuestion.ToString().ToLower() + " army's king in battle";
                }

                else
                {
                    if (teamInQuestion == opponentColor)
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
            move castleMove = new move();
            int yCoor;                              //which row the move is being conducted in

            if (offensiveColor == Color.Dark)
            {
                yCoor = 7;
            }
            else
            {
                yCoor = 0;
            }

            if (shift.start.x == 4 && shift.start.y == yCoor)  //if moving from King default position
            {
                if (shift.end.x == 2 && shift.end.y == yCoor)  //if moving two spaces to the left
                {
                    castleMove.end = new coordinate(3, yCoor);
                    castleMove.start = new coordinate(0, yCoor);

                    if(networkGame == false)
                    {
                        node = new historyNode(castleMove, pieceArray[3, yCoor], false, true, true);
                        history.Push(node);
                    }
                    
                    movePiece(castleMove.start, castleMove.end);
                }

                else if (shift.end.x == 6 && shift.end.y == yCoor) //if moving two spaces to the right
                {
                    castleMove.end = new coordinate(5, yCoor);
                    castleMove.start = new coordinate(7, yCoor);

                    if(networkGame == false)
                    {
                        node = new historyNode(castleMove, pieceArray[5, yCoor], false, true, true);
                        history.Push(node);
                    }

                    movePiece(castleMove.start, castleMove.end);
                }
            }
        }

        public async Task clicker(coordinate currentCell)
        {
            //human player's turn, gets called when player clicks on spot

            if (ready == true)  //blocks functionality if game hasn't started yet
            {
                piece currentPiece = pieceArray[currentCell.x, currentCell.y];
                //if selected same piece
                if (displayArray[currentCell.x, currentCell.y].tile.Background == Brushes.DeepSkyBlue)
                {
                    movablePieceSelected = false;
                    clearSelectedAndPossible();
                }
                //if selected own piece
                else if (currentPiece.color == offensiveColor)
                {
                    ownCellSelected(currentCell);
                }
                //if selected movable spot
                else if (movablePieceSelected == true)
                {
                    await movableCellSelected(currentCell);
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

        private void ownCellSelected(coordinate cCell)
        {
            //When click on your own team the first time, get possible moves

            movablePieceSelected = true;
            clearSelectedAndPossible();
            displayArray[cCell.x, cCell.y].tile.Background = Brushes.DeepSkyBlue;
            prevSelected = cCell;
            possible.Clear();
            possible.AddRange(getCheckRestrictedMoves(cCell, pieceArray));
            Color defensiveTeam = switchTeam(offensiveColor);

            foreach (move m in possible)
            {
                if (pieceArray[m.end.x, m.end.y].color == defensiveTeam)
                {
                    displayArray[m.end.x, m.end.y].tile.Background = Brushes.DarkOrange;
                }
                else
                {
                    displayArray[m.end.x, m.end.y].tile.Background = Brushes.LawnGreen;
                }
            }
        }

        private async Task movableCellSelected(coordinate cCell)
        {
            //If previously clicked on a movable piece

            move curTurn = new move();
            bool movableSpot = false;

            //Checks if current click is in possible move list
            foreach (move m in possible)
            {
                if (cCell.Equals(m.end))
                {
                    movableSpot = true;
                    curTurn = m;
                }
            }

            if (movableSpot == true)
            {
                historyNode node;
                Job pawnTrans = Job.Pawn;
                piece captured = pieceArray[cCell.x, cCell.y];
                bool virginMove = pieceArray[prevSelected.x, prevSelected.y].virgin;
                movePiece(prevSelected, cCell);
                clearSelectedAndPossible();

                if (pieceArray[cCell.x, cCell.y].job == Job.Pawn)
                {
                    if (cCell.y == 0 || cCell.y == 7)
                    {
                        PawnTransformation transform = new PawnTransformation(cCell, this);
                        transform.ShowDialog();
                        node = new historyNode(curTurn, captured, true, false, false);
                        pawnTrans = pieceArray[cCell.x, cCell.y].job;
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

                    if(opponentColor == Color.Light)
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

                if (pieceArray[cCell.x, cCell.y].job == Job.King)
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
            offensiveColor = switchTeam(offensiveColor);
            endOfGame = isInCheckmate(offensiveColor, getPieces(pieceArray, offensiveColor));  //did previous turn put other team in checkmate?

            if (endOfGame == false && onePlayer == true)
            {
                await compTurn();
                var oppositeColor = switchTeam(offensiveColor);
                endOfGame = isInCheckmate(oppositeColor, getPieces(pieceArray, oppositeColor)); //did computer turn put player in checkmate?
                offensiveColor = oppositeColor;
            }

            if (networkGame == true && endOfGame == false)
            {
                ready = false;
                //wait for response move
            }
            //if 2Player local, rotate is on, and didn't end game
            else if (twoPlayer == true && rotate == true && endOfGame == false)
            {
                if(offensiveColor == Color.Dark)
                {
                    rotateBoard(true, rotationDuration);
                }
                else
                {
                    rotateBoard(false, rotationDuration);
                }
                onBottomColor = offensiveColor;
            }
        }

        public async Task compTurn()
        {
            //computer's turn
            
            historyNode node;
            ready = false;

            if (difficulty > 3)
            {
                think = new Thinking(this, rnd.Next(0, 41));
                think.Owner = mWindow;
                think.ShowDialog();
            }
            else
            {
                await Task.Run(() => minimax(pieceArray, offensiveColor, -1, true, -99, 99, null));
            }

            ready = true;
            coordinate newSpot = new coordinate(bestMove.end.x, bestMove.end.y);
            coordinate oldSpot = new coordinate(bestMove.start.x, bestMove.start.y);

            piece captured = pieceArray[newSpot.x, newSpot.y];
            bool virginMove = pieceArray[oldSpot.x, oldSpot.y].virgin;
            movePiece(oldSpot, newSpot);

            if (pieceArray[newSpot.x, newSpot.y].job == Job.Pawn)
            {
                if (newSpot.y == 0)
                {
                    pieceArray[newSpot.x, newSpot.y].job = Job.Queen;
                    displayArray[newSpot.x, newSpot.y].top.Source = dQueen;
                    node = new historyNode(bestMove, captured, true, true, false);
                }
                else if (newSpot.y == 7)
                {
                    pieceArray[newSpot.x, newSpot.y].job = Job.Queen;
                    displayArray[newSpot.x, newSpot.y].top.Source = lQueen;
                    node = new historyNode(bestMove, captured, true, true, false);
                }
                node = new historyNode(bestMove, captured, false, true, virginMove);
            }
            else
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

            if (pieceArray[newSpot.x, newSpot.y].job == Job.King)
            {
                castling(bestMove);//check if move is a castling
            }
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

                if(mWindow.space.ActualHeight > mWindow.space.ActualWidth)
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

                if(bottomLightToDark == true)
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
                foreach(display cell in displayArray)
                {
                    cell.top.RenderTransformOrigin = middle;
                    cell.top.RenderTransform = ft;
                }
                ready = true;
            }
        }

        private void movePiece(coordinate oldCell, coordinate newCell)
        {
            //does standard piece move

            piece pPiece = pieceArray[oldCell.x, oldCell.y];

            //overwrite current cell
            pieceArray[newCell.x, newCell.y].color = pPiece.color;
            pieceArray[newCell.x, newCell.y].job = pPiece.job;
            pieceArray[newCell.x, newCell.y].virgin = false;
            displayArray[newCell.x, newCell.y].top.Source = matchPicture(pPiece);

            //delete prev cell
            pieceArray[oldCell.x, oldCell.y].color = Color.None;
            pieceArray[oldCell.x, oldCell.y].job = Job.None;
            pieceArray[oldCell.x, oldCell.y].virgin = false;
            displayArray[oldCell.x, oldCell.y].top.Source = null;

            movablePieceSelected = false;
        }

        public void undo()
        {
            //completely undo previous move

            BitmapImage pawnPic = new BitmapImage();
            piece to;
            piece from;
            int xEnd;
            int yEnd;
            int xStart;
            int yStart;

            historyNode node = history.Pop();

            xEnd = node.step.end.x;
            yEnd = node.step.end.y;
            xStart = node.step.start.x;
            yStart = node.step.start.y;

            to = pieceArray[xEnd, yEnd];
            from = pieceArray[xStart, yStart];
            offensiveColor = to.color;

            if (rotate == true && twoPlayer == true)
            {
                if(offensiveColor == Color.Dark)
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

                pieceArray[xStart, yStart].job = Job.Pawn;
                displayArray[xStart, yStart].top.Source = pawnPic;
            }

            else
            {
                pieceArray[xStart, yStart].job = to.job;
                displayArray[xStart, yStart].top.Source = matchPicture(to);
            }

            pieceArray[xStart, yStart].color = to.color;
            pieceArray[xStart, yStart].virgin = node.firstMove;

            //put captured piece back
            pieceArray[xEnd, yEnd].job = node.captured.job;
            displayArray[xEnd, yEnd].top.Source = matchPicture(node.captured);
            pieceArray[xEnd, yEnd].color = node.captured.color;
            pieceArray[xEnd, yEnd].virgin = node.captured.virgin;


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

        private BitmapImage matchPicture(piece figure)
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

            foreach (coordinate spot in getAllPieces(pieceArray))
            {
                displayArray[spot.x, spot.y].top.Source = matchPicture(pieceArray[spot.x, spot.y]);
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
            saveData sData = new saveData(pieceArray, offensiveColor, opponentColor, onBottomColor, theme, onePlayer, twoPlayer, networkGame, difficulty,
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
                        pieceArray = new piece[8, 8];
                        movablePieceSelected = false;
                        pieceArray = lData.sBoard;
                        offensiveColor = lData.sOffensiveTeam;
                        opponentColor = lData.sOpponent;
                        onBottomColor = lData.sOnBottom;
                        onePlayer = lData.sOnePlayer;
                        twoPlayer = lData.sTwoPlayer;
                        difficulty = lData.sDifficulty;

                        if (onBottomColor == Color.Dark)
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

            if (offensiveColor == opponentColor)  //if opponent's turn
            {
                clearSelectedAndPossible();
                //if toggled on
                if(rotate == false)
                {
                    if(offensiveColor == Color.Dark)
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
                    if (offensiveColor == Color.Dark)
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
                    coordinate p;
                    coordinate m;

                    if(opponentColor == Color.Light)
                    {
                        p = new coordinate(buffer[1], 7 - buffer[2]);
                        m = new coordinate(buffer[3], 7 - buffer[4]);
                    }
                    else
                    {
                        p = new coordinate(buffer[1], buffer[2]);
                        m = new coordinate(buffer[3], buffer[4]);
                    }

                    move opponentsMove = new move(p, m);
                    networkMove(opponentsMove, (int)buffer[5]);
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

        private void networkMove(move m, int pawn)
        {
            //Executes move locally sent from other client

            int xMove = m.end.x;
            int yMove = m.end.y;

            //movePiece
            movePiece(m.start, m.end);
            //pawnTransform
            if (pieceArray[xMove, yMove].job == Job.Pawn)
            {
                if (yMove == 0 || yMove == 7)
                {
                    pieceArray[xMove, yMove].job = (Job)pawn;
                    displayArray[xMove, yMove].top.Source = matchPicture(pieceArray[xMove, yMove]);
                }
            }
            //clear then set to and from
            if (lastMove == true)
            {
                clearToAndFrom();
                displayArray[m.start.x, m.start.y].bottom.Source = bmpFrom;
                displayArray[xMove, yMove].bottom.Source = bmpTo;
            }
            //check for castling
            if (pieceArray[xMove, yMove].job == Job.King)
            {
                castling(m);
            }

            offensiveColor = switchTeam(offensiveColor);
            ready = true;
        }

        public Color switchTeam(Color input)
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

        //the next few functions define the rules for what piece can move where in any situation
        //does not account for check restrictions
        //takes coordinate and returns list of possible moves for that piece

        private List<move> rookMoves(coordinate current, piece[,] pArray)
        {
            move availableMove = new move();
            int availableX = current.x;             //put coordinate in temp variable to manipulate while preserving original
            int availableY = current.y;
            List<move> availableList = new List<move>();
            coordinate moveCoor = new coordinate(); //when found possible move, put in this variable to add to list
            Color pieceColor = pArray[current.x, current.y].color;
            Color oppositeColor = switchTeam(pieceColor);

            //search up
            availableY++;
            while (availableY < 8)
            {
                if (pArray[availableX, availableY].color == pieceColor)  //if same team
                {
                    break;                                              //can't go past
                }

                else if (pArray[availableX, availableY].color == oppositeColor)   //if enemy
                {
                    moveCoor.x = availableX;
                    moveCoor.y = availableY;
                    availableMove = new move(current, moveCoor);
                    availableList.Add(availableMove);       //add to list
                    break;                                  //can't go past
                }

                else                                        //if unoccupied
                {
                    moveCoor.x = availableX;
                    moveCoor.y = availableY;
                    availableMove = new move(current, moveCoor);
                    availableList.Add(availableMove);       //add to list
                    availableY++;                           //try next spot
                }
            }

            //search left
            availableX = current.x;
            availableY = current.y;
            availableX--;
            while (availableX >= 0)
            {
                if (pArray[availableX, availableY].color == pieceColor)
                {
                    break;
                }

                else if (pArray[availableX, availableY].color == oppositeColor)
                {
                    moveCoor.x = availableX;
                    moveCoor.y = availableY;
                    availableMove = new move(current, moveCoor);
                    availableList.Add(availableMove);
                    break;
                }

                else
                {
                    moveCoor.x = availableX;
                    moveCoor.y = availableY;
                    availableMove = new move(current, moveCoor);
                    availableList.Add(availableMove);
                    availableX--;
                }
            }

            //search down
            availableX = current.x;
            availableY = current.y;
            availableY--;
            while (availableY >= 0)
            {
                if (pArray[availableX, availableY].color == pieceColor)
                {
                    break;
                }

                else if (pArray[availableX, availableY].color == oppositeColor)
                {
                    moveCoor.x = availableX;
                    moveCoor.y = availableY;
                    availableMove = new move(current, moveCoor);
                    availableList.Add(availableMove);
                    break;
                }

                else
                {
                    moveCoor.x = availableX;
                    moveCoor.y = availableY;
                    availableMove = new move(current, moveCoor);
                    availableList.Add(availableMove);
                    availableY--;
                }
            }

            //search right
            availableX = current.x;
            availableY = current.y;
            availableX++;
            while (availableX < 8)
            {
                if (pArray[availableX, availableY].color == pieceColor)
                {
                    break;
                }

                else if (pArray[availableX, availableY].color == oppositeColor)
                {
                    moveCoor.x = availableX;
                    moveCoor.y = availableY;
                    availableMove = new move(current, moveCoor);
                    availableList.Add(availableMove);
                    break;
                }

                else
                {
                    moveCoor.x = availableX;
                    moveCoor.y = availableY;
                    availableMove = new move(current, moveCoor);
                    availableList.Add(availableMove);
                    availableX++;
                }
            }
            return availableList;
        }

        private List<move> knightMoves(coordinate current, piece[,] pArray)
        {
            move availableMove = new move();
            int availableX = current.x;
            int availableY = current.y;
            List<move> availableList = new List<move>();
            coordinate moveCoor = new coordinate();
            Color pieceColor = pArray[current.x, current.y].color;

            //search up.right
            availableY += 2;
            availableX++;
            if (availableY < 8 && availableX < 8)
            {
                if (pArray[availableX, availableY].color != pieceColor)
                {
                    moveCoor.x = availableX;
                    moveCoor.y = availableY;
                    availableMove = new move(current, moveCoor);
                    availableList.Add(availableMove);
                }
            }

            //search up.left
            availableX = current.x;
            availableY = current.y;
            availableY += 2;
            availableX--;
            if (availableY < 8 && availableX >= 0)
            {
                if (pArray[availableX, availableY].color != pieceColor)
                {
                    moveCoor.x = availableX;
                    moveCoor.y = availableY;
                    availableMove = new move(current, moveCoor);
                    availableList.Add(availableMove);
                }
            }

            //search left.up
            availableX = current.x;
            availableY = current.y;
            availableY++;
            availableX -= 2;
            if (availableY < 8 && availableX >= 0)
            {
                if (pArray[availableX, availableY].color != pieceColor)
                {
                    moveCoor.x = availableX;
                    moveCoor.y = availableY;
                    availableMove = new move(current, moveCoor);
                    availableList.Add(availableMove);
                }
            }

            //search left.down
            availableX = current.x;
            availableY = current.y;
            availableY--;
            availableX -= 2;
            if (availableY >= 0 && availableX >= 0)
            {
                if (pArray[availableX, availableY].color != pieceColor)
                {
                    moveCoor.x = availableX;
                    moveCoor.y = availableY;
                    availableMove = new move(current, moveCoor);
                    availableList.Add(availableMove);
                }
            }

            //search down.left
            availableX = current.x;
            availableY = current.y;
            availableY -= 2;
            availableX--;
            if (availableY >= 0 && availableX >= 0)
            {
                if (pArray[availableX, availableY].color != pieceColor)
                {
                    moveCoor.x = availableX;
                    moveCoor.y = availableY;
                    availableMove = new move(current, moveCoor);
                    availableList.Add(availableMove);
                }
            }

            //search down.right
            availableX = current.x;
            availableY = current.y;
            availableY -= 2;
            availableX++;
            if (availableY >= 0 && availableX < 8)
            {
                if (pArray[availableX, availableY].color != pieceColor)
                {
                    moveCoor.x = availableX;
                    moveCoor.y = availableY;
                    availableMove = new move(current, moveCoor);
                    availableList.Add(availableMove);
                }
            }

            //search right.down
            availableX = current.x;
            availableY = current.y;
            availableY--;
            availableX += 2;
            if (availableY >= 0 && availableX < 8)
            {
                if (pArray[availableX, availableY].color != pieceColor)
                {
                    moveCoor.x = availableX;
                    moveCoor.y = availableY;
                    availableMove = new move(current, moveCoor);
                    availableList.Add(availableMove);
                }
            }

            //search right.up
            availableX = current.x;
            availableY = current.y;
            availableY++;
            availableX += 2;
            if (availableY < 8 && availableX < 8)
            {
                if (pArray[availableX, availableY].color != pieceColor)
                {
                    moveCoor.x = availableX;
                    moveCoor.y = availableY;
                    availableMove = new move(current, moveCoor);
                    availableList.Add(availableMove);
                }
            }
            return availableList;
        }

        private List<move> bishopMoves(coordinate current, piece[,] pArray)
        {
            move availableMove = new move();
            int availableX = current.x;
            int availableY = current.y;
            List<move> availableList = new List<move>();
            coordinate moveCoor = new coordinate();
            Color pieceColor = pArray[current.x, current.y].color;
            Color oppositeColor = switchTeam(pieceColor);

            //search upper right
            availableX++;
            availableY++;
            while (availableX < 8 && availableY < 8)
            {
                if (pArray[availableX, availableY].color == pieceColor)
                {
                    break;
                }

                else if (pArray[availableX, availableY].color == oppositeColor)
                {
                    moveCoor.x = availableX;
                    moveCoor.y = availableY;
                    availableMove = new move(current, moveCoor);
                    availableList.Add(availableMove);
                    break;
                }

                else
                {
                    moveCoor.x = availableX;
                    moveCoor.y = availableY;
                    availableMove = new move(current, moveCoor);
                    availableList.Add(availableMove);
                    availableX++;
                    availableY++;
                }
            }

            //search upper left
            availableX = current.x;
            availableY = current.y;
            availableX--;
            availableY++;
            while (availableX >= 0 && availableY < 8)
            {
                if (pArray[availableX, availableY].color == pieceColor)
                {
                    break;
                }

                else if (pArray[availableX, availableY].color == oppositeColor)
                {
                    moveCoor.x = availableX;
                    moveCoor.y = availableY;
                    availableMove = new move(current, moveCoor);
                    availableList.Add(availableMove);
                    break;
                }

                else
                {
                    moveCoor.x = availableX;
                    moveCoor.y = availableY;
                    availableMove = new move(current, moveCoor);
                    availableList.Add(availableMove);
                    availableX--;
                    availableY++;
                }
            }

            //search lower left
            availableX = current.x;
            availableY = current.y;
            availableX--;
            availableY--;
            while (availableX >= 0 && availableY >= 0)
            {
                if (pArray[availableX, availableY].color == pieceColor)
                {
                    break;
                }

                else if (pArray[availableX, availableY].color == oppositeColor)
                {
                    moveCoor.x = availableX;
                    moveCoor.y = availableY;
                    availableMove = new move(current, moveCoor);
                    availableList.Add(availableMove);
                    break;
                }

                else
                {
                    moveCoor.x = availableX;
                    moveCoor.y = availableY;
                    availableMove = new move(current, moveCoor);
                    availableList.Add(availableMove);
                    availableX--;
                    availableY--;
                }
            }

            //search lower right
            availableX = current.x;
            availableY = current.y;
            availableX++;
            availableY--;
            while (availableX < 8 && availableY >= 0)
            {
                if (pArray[availableX, availableY].color == pieceColor)
                {
                    break;
                }

                else if (pArray[availableX, availableY].color == oppositeColor)
                {
                    moveCoor.x = availableX;
                    moveCoor.y = availableY;
                    availableMove = new move(current, moveCoor);
                    availableList.Add(availableMove);
                    break;
                }

                else
                {
                    moveCoor.x = availableX;
                    moveCoor.y = availableY;
                    availableMove = new move(current, moveCoor);
                    availableList.Add(availableMove);
                    availableX++;
                    availableY--;
                }
            }
            return availableList;
        }

        private List<move> kingMoves(coordinate current, piece[,] pArray)
        {
            move availableMove = new move();
            int availableX = current.x;
            int availableY = current.y;
            List<move> availableList = new List<move>();
            coordinate moveCoor = new coordinate();
            Color pieceColor = pArray[current.x, current.y].color;

            //search up
            availableY++;
            if (availableY < 8)
            {
                if (pArray[availableX, availableY].color != pieceColor)
                {
                    moveCoor.x = availableX;
                    moveCoor.y = availableY;
                    availableMove = new move(current, moveCoor);
                    availableList.Add(availableMove);
                }
            }

            //search upper left
            availableX = current.x;
            availableY = current.y;
            availableY++;
            availableX--;
            if (availableY < 8 && availableX >= 0)
            {
                if (pArray[availableX, availableY].color != pieceColor)
                {
                    moveCoor.x = availableX;
                    moveCoor.y = availableY;
                    availableMove = new move(current, moveCoor);
                    availableList.Add(availableMove);
                }
            }

            //search left
            availableX = current.x;
            availableY = current.y;
            availableX--;
            if (availableX >= 0)
            {
                if (pArray[availableX, availableY].color != pieceColor)
                {
                    moveCoor.x = availableX;
                    moveCoor.y = availableY;
                    availableMove = new move(current, moveCoor);
                    availableList.Add(availableMove);
                }
            }

            //search lower left
            availableX = current.x;
            availableY = current.y;
            availableY--;
            availableX--;
            if (availableY >= 0 && availableX >= 0)
            {
                if (pArray[availableX, availableY].color != pieceColor)
                {
                    moveCoor.x = availableX;
                    moveCoor.y = availableY;
                    availableMove = new move(current, moveCoor);
                    availableList.Add(availableMove);
                }
            }

            //search down
            availableX = current.x;
            availableY = current.y;
            availableY--;
            if (availableY >= 0)
            {
                if (pArray[availableX, availableY].color != pieceColor)
                {
                    moveCoor.x = availableX;
                    moveCoor.y = availableY;
                    availableMove = new move(current, moveCoor);
                    availableList.Add(availableMove);
                }
            }

            //search lower right
            availableX = current.x;
            availableY = current.y;
            availableY--;
            availableX++;
            if (availableY >= 0 && availableX < 8)
            {
                if (pArray[availableX, availableY].color != pieceColor)
                {
                    moveCoor.x = availableX;
                    moveCoor.y = availableY;
                    availableMove = new move(current, moveCoor);
                    availableList.Add(availableMove);
                }
            }

            //search right
            availableX = current.x;
            availableY = current.y;
            availableX++;
            if (availableX < 8)
            {
                if (pArray[availableX, availableY].color != pieceColor)
                {
                    moveCoor.x = availableX;
                    moveCoor.y = availableY;
                    availableMove = new move(current, moveCoor);
                    availableList.Add(availableMove);
                }
            }

            //search upper right
            availableX = current.x;
            availableY = current.y;
            availableY++;
            availableX++;
            if (availableY < 8 && availableX < 8)
            {
                if (pArray[availableX, availableY].color != pieceColor)
                {
                    moveCoor.x = availableX;
                    moveCoor.y = availableY;
                    availableMove = new move(current, moveCoor);
                    availableList.Add(availableMove);
                }
            }

            //search for castleing opportunity
            if (pArray[current.x, current.y].virgin == true)//if king's first move
            {
                if (pieceColor == Color.Dark)
                {
                    if (pArray[0, 7].virgin == true)//if left rook's first move
                    {
                        //if clear path from rook to king
                        if (pArray[1, 7].job == Job.None && pArray[2, 7].job == Job.None && pArray[3, 7].job == Job.None)
                        {
                            moveCoor.x = 2;
                            moveCoor.y = 7;
                            availableMove = new move(current, moveCoor);
                            availableList.Add(availableMove);
                        }
                    }

                    if (pArray[7, 7].virgin == true)//if right rook's first move
                    {
                        if (pArray[6, 7].job == Job.None && pArray[5, 7].job == Job.None)
                        {
                            moveCoor.x = 6;
                            moveCoor.y = 7;
                            availableMove = new move(current, moveCoor);
                            availableList.Add(availableMove);
                        }
                    }
                }

                else
                {
                    if (pArray[0, 0].virgin == true)//if left rook's first move
                    {
                        if (pArray[1, 0].job == Job.None && pArray[2, 0].job == Job.None && pArray[3, 0].job == Job.None)
                        {
                            moveCoor.x = 2;
                            moveCoor.y = 0;
                            availableMove = new move(current, moveCoor);
                            availableList.Add(availableMove);
                        }
                    }

                    if (pArray[7, 0].virgin == true)//if right rook's first move
                    {
                        if (pArray[6, 0].job == Job.None && pArray[5, 0].job == Job.None)
                        {
                            moveCoor.x = 6;
                            moveCoor.y = 0;
                            availableMove = new move(current, moveCoor);
                            availableList.Add(availableMove);
                        }
                    }
                }
            }
            return availableList;
        }

        private List<move> pawnMoves(coordinate current, piece[,] pArray)
        {
            move availableMove = new move();
            int availableX = current.x;
            int availableY = current.y;
            List<move> availableList = new List<move>();
            coordinate moveCoor = new coordinate();
            Color pieceColor = pArray[current.x, current.y].color;
            Color oppositeColor = switchTeam(pieceColor);

            if (pieceColor == Color.Dark)
            {
                availableY--;
                //search first move
                if (pArray[current.x, current.y].virgin == true)
                {
                    if (pArray[availableX, 5].color == Color.None)
                    {
                        moveCoor.x = availableX;
                        moveCoor.y = 5;
                        availableMove = new move(current, moveCoor);
                        availableList.Add(availableMove);

                        if (pArray[availableX, 4].color == Color.None)
                        {
                            moveCoor.x = availableX;
                            moveCoor.y = 4;
                            availableMove = new move(current, moveCoor);
                            availableList.Add(availableMove);
                        }
                    }
                }

                //search down
                else if (availableY >= 0)
                {
                    if (pArray[availableX, availableY].color == Color.None)
                    {
                        moveCoor.x = availableX;
                        moveCoor.y = availableY;
                        availableMove = new move(current, moveCoor);
                        availableList.Add(availableMove);
                    }
                }

                //search lower right
                availableX++;
                if (availableY >= 0 && availableX < 8)
                {
                    if (pArray[availableX, availableY].color == oppositeColor)
                    {
                        moveCoor.x = availableX;
                        moveCoor.y = availableY;
                        availableMove = new move(current, moveCoor);
                        availableList.Add(availableMove);
                    }
                }

                //search lower left
                availableX -= 2;
                if (availableY >= 0 && availableX >= 0)
                {
                    if (pArray[availableX, availableY].color == oppositeColor)
                    {
                        moveCoor.x = availableX;
                        moveCoor.y = availableY;
                        availableMove = new move(current, moveCoor);
                        availableList.Add(availableMove);
                    }
                }
            }

            else
            {
                availableY++;
                //search first move
                if (pArray[current.x, current.y].virgin == true)
                {
                    if (pArray[availableX, 2].color == Color.None)
                    {
                        moveCoor.x = availableX;
                        moveCoor.y = 2;
                        availableMove = new move(current, moveCoor);
                        availableList.Add(availableMove);

                        if (pArray[availableX, 3].color == Color.None)
                        {
                            moveCoor.x = availableX;
                            moveCoor.y = 3;
                            availableMove = new move(current, moveCoor);
                            availableList.Add(availableMove);
                        }
                    }
                }

                //search up
                else if (availableY < 8)
                {
                    if (pArray[availableX, availableY].color == Color.None)
                    {
                        moveCoor.x = availableX;
                        moveCoor.y = availableY;
                        availableMove = new move(current, moveCoor);
                        availableList.Add(availableMove);
                    }
                }

                //search upper right
                availableX++;
                if (availableY < 8 && availableX < 8)
                {
                    if (pArray[availableX, availableY].color == oppositeColor)
                    {
                        moveCoor.x = availableX;
                        moveCoor.y = availableY;
                        availableMove = new move(current, moveCoor);
                        availableList.Add(availableMove);
                    }
                }

                //search upper left
                availableX -= 2;
                if (availableY < 8 && availableX >= 0)
                {
                    if (pArray[availableX, availableY].color == oppositeColor)
                    {
                        moveCoor.x = availableX;
                        moveCoor.y = availableY;
                        availableMove = new move(current, moveCoor);
                        availableList.Add(availableMove);
                    }
                }
            }
            return availableList;
        }
    }
}
