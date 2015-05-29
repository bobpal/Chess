using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Trinet.Core.IO.Ntfs;

namespace Chess
{
    [Serializable]
    public class Logic
    {
        public piece[,] pieceArray;         //8x8 array of pieces
        public display[,] displayArray;     //8x8 array of display objects
        public MainWindow mWindow;          //window with chess board on it
        public bool onePlayer;              //versus computer
        public bool networkGame;            //playing over a network
        public string opponent;             //color of computer or 2nd player
        public string offensiveTeam;        //which side is on the offense
        public bool medMode;                //difficulty level
        public bool hardMode;               //difficulty level
        public bool ready;                  //blocks functionality for unwanted circumstances
        private coordinate prevSelected;    //where the cursor clicked previously
        public List<string> themeList;      //list of themes
        public int themeIndex;              //which theme is currently in use
        private List<string> ignore;        //list of which dll files to ignore
        public BitmapImage lKing;
        public BitmapImage lQueen;
        public BitmapImage lBishop;
        public BitmapImage lKnight;
        public BitmapImage lRook;
        private BitmapImage lPawn;
        private BitmapImage dKing;
        public BitmapImage dQueen;
        public BitmapImage dBishop;
        public BitmapImage dKnight;
        public BitmapImage dRook;
        private BitmapImage dPawn;
        public TcpClient client;
        public NetworkStream nwStream;
        public byte[] buffer = new byte[255];                   //buffer for tcp
        private static Random rnd = new Random();               //used for all random situations
        public int bottom;                                      //how many levels deep should comp look?
        private move bestMove;                                  //ultimate return for evaluator()
        private Thinking think;                                 //progress bar window for hardMode
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
        private BitmapImage bmpTo = new BitmapImage(new Uri("pack://application:,,,/Resources/to.png"));
        private BitmapImage bmpFrom = new BitmapImage(new Uri("pack://application:,,,/Resources/from.png"));
        private string dirPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Chess";
        public string filePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Chess\\chess.sav";

        [Serializable]
        private class saveData
        {
            //contains all the information needed to save the game

            public piece[,] sBoard { get; private set; }
            public string sOffensiveTeam { get; private set; }
            public string sOpponent { get; private set; }
            public string sTheme { get; private set; }
            public bool sOnePlayer { get; private set; }
            public bool sNetwork { get; private set; }
            public bool sMedMode { get; private set; }
            public bool sHardMode { get; private set; }
            public bool sLastMove { get; private set; }
            public bool sSaveGame { get; private set; }
            public bool sReady { get; private set; }
            public bool sRotate { get; private set; }
            public double sRduration { get; private set; }

            public saveData(piece[,] p1, string p2, string p3, string p4, bool p5,
                bool p6, bool p7, bool p8, bool p9, bool p10, bool p11, bool p12, double p13)
            {
                this.sBoard = p1;
                this.sOffensiveTeam = p2;
                this.sOpponent = p3;
                this.sTheme = p4;
                this.sOnePlayer = p5;
                this.sNetwork = p6;
                this.sMedMode = p7;
                this.sHardMode = p8;
                this.sLastMove = p9;
                this.sSaveGame = p10;
                this.sReady = p11;
                this.sRotate = p12;
                this.sRduration = p13;
            }
        }

        [Serializable]
        public struct piece
        {
            //represents a movable piece

            public string color { get; set; }   //on dark or light team?
            public string job { get; set; }     //piece's job
            public bool virgin { get; set; }    //has piece never moved?
        }

        [Serializable]
        public struct coordinate
        {
            //a spot on the board

            public int x { get; set; }
            public int y { get; set; }

            public coordinate(int p1, int p2)
                : this()
            {
                this.x = p1;
                this.y = p2;
            }
        }

        [Serializable]
        public class move
        {
            //represents a move that a piece can do

            public coordinate pieceSpot { get; set; }   //starting position
            public coordinate moveSpot { get; set; }    //ending position

            public move(coordinate p1, coordinate p2)
            {
                this.pieceSpot = p1;
                this.moveSpot = p2;
            }

            public move() { }
        }

        [Serializable]
        public class historyNode
        {
            //contains all information needed to undo move

            public move step;               //move that happened previously
            public piece captured;          //piece that move captured, if no capture, use null
            public bool pawnTransform;      //did a pawn transformation happen?
            public bool skip;               //undo next move also if playing against computer
            public bool firstMove;          //was this the piece's first move?

            public historyNode(move p1, piece p2, bool p3, bool p4, bool p5)
            {
                this.step = p1;
                this.captured = p2;
                this.pawnTransform = p3;
                this.skip = p4;
                this.firstMove = p5;
            }
        }

        [Serializable]
        public class display
        {
            public Canvas tile { get; set; }
            public Image bottom { get; set; }
            public Image top { get; set; }

            public display(Canvas c, Image b, Image t)
            {
                this.tile = c;
                this.bottom = b;
                this.top = t;
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
                        pieceArray[x, 0].color = "light";
                    }

                    else if (y == 1)
                    {
                        pieceArray[x, 1].color = "light";
                        pieceArray[x, 1].job = "Pawn";
                        pieceArray[x, 1].virgin = true;
                        displayArray[x, 1].top.Source = matchPicture(pieceArray[x, 1]);
                    }

                    else if (y == 6)
                    {
                        pieceArray[x, 6].color = "dark";
                        pieceArray[x, 6].job = "Pawn";
                        pieceArray[x, 6].virgin = true;
                        displayArray[x, 6].top.Source = matchPicture(pieceArray[x, 6]);
                    }

                    else if (y == 7)
                    {
                        pieceArray[x, 7].color = "dark";
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

            pieceArray[0, 0].job = "Rook";
            pieceArray[1, 0].job = "Knight";
            pieceArray[2, 0].job = "Bishop";
            pieceArray[3, 0].job = "Queen";
            pieceArray[4, 0].job = "King";
            pieceArray[5, 0].job = "Bishop";
            pieceArray[6, 0].job = "Knight";
            pieceArray[7, 0].job = "Rook";
            pieceArray[0, 7].job = "Rook";
            pieceArray[1, 7].job = "Knight";
            pieceArray[2, 7].job = "Bishop";
            pieceArray[3, 7].job = "Queen";
            pieceArray[4, 7].job = "King";
            pieceArray[5, 7].job = "Bishop";
            pieceArray[6, 7].job = "Knight";
            pieceArray[7, 7].job = "Rook";

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

        private List<coordinate> getDarkPieces(piece[,] board)
        {
            //searches through pieceArray and returns list of coordinates where all dark pieces are located

            coordinate temp = new coordinate();
            List<coordinate> darkPieces = new List<coordinate>();

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    if (board[x, y].color == "dark")
                    {
                        temp.x = x;
                        temp.y = y;
                        darkPieces.Add(temp);
                    }
                }
            }
            return darkPieces;
        }

        private List<coordinate> getLightPieces(piece[,] board)
        {
            //searches through pieceArray and returns list of coordinates where all light pieces are located

            coordinate temp = new coordinate();
            List<coordinate> lightPieces = new List<coordinate>();

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    if (board[x, y].color == "light")
                    {
                        temp.x = x;
                        temp.y = y;
                        lightPieces.Add(temp);
                    }
                }
            }
            return lightPieces;
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
                    if (board[x, y].color != null)
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
                case "Rook":
                    return rookMoves(spot, grid);
                case "Knight":
                    return knightMoves(spot, grid);
                case "Bishop":
                    return bishopMoves(spot, grid);
                case "Queen":
                    temp.AddRange(bishopMoves(spot, grid));
                    temp.AddRange(rookMoves(spot, grid));
                    return temp;
                case "King":
                    return kingMoves(spot, grid);
                case "Pawn":
                    return pawnMoves(spot, grid);
                default:
                    return temp;    //temp should be empty
            }
        }

        private int evaluator(
            piece[,] board, string attacking, int level, bool computerTurn, int alpha, int beta, IProgress<int> progress)
        {
            //is called recursively for comp to look ahead and return the best move

            int val;
            int bestVal;
            int indexOfBest = 0;
            List<move> offensiveMoves = new List<move>();

            level++;
            //if not on bottom level, go down
            if(level < bottom)
            {
                piece[,] newBoard;

                string nextTurn = switchTeam(attacking);

                if (attacking == "dark")
                {
                    foreach (coordinate cell in getDarkPieces(board))
                    {
                        offensiveMoves.AddRange(getCheckRestrictedMoves(cell, board));
                    }
                }
                else
                {
                    foreach (coordinate cell in getLightPieces(board))
                    {
                        offensiveMoves.AddRange(getCheckRestrictedMoves(cell, board));
                    }
                }

                if (computerTurn == true)
                {
                    bestVal = -30;

                    //if checkmate
                    if (offensiveMoves.Count == 0){}
                    else
                    {
                        if (level == 1)
                        {
                            for (int i = 0; i < offensiveMoves.Count; i++)
                            {
                                newBoard = deepCopy(board);
                                newBoard = silentMove(newBoard, offensiveMoves[i]);
                                val = evaluator(newBoard, nextTurn, level, false, alpha, beta, null);
                                if (val > bestVal)
                                {
                                    bestVal = val;
                                    indexOfBest = i;
                                }
                                //null = medMode
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
                                newBoard = silentMove(newBoard, offensiveMoves[i]);
                                val = evaluator(newBoard, nextTurn, level, false, alpha, beta, null);
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
                            newBoard = silentMove(newBoard, offensiveMoves[i]);
                            val = evaluator(newBoard, nextTurn, level, true, alpha, beta, null);
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
                bestVal = getValue(board);
            }
            return bestVal;
        }

        private piece[,] silentMove(piece[,] grid, move mv)
        {
            //does a move without visuals on a private board

            int fromX = mv.pieceSpot.x;
            int fromY = mv.pieceSpot.y;
            int toX = mv.moveSpot.x;
            int toY = mv.moveSpot.y;
            string offense = grid[fromX, fromY].color;

            //move to new cell
            grid[toX, toY].color = offense;
            grid[toX, toY].job = grid[fromX, fromY].job;
            grid[toX, toY].virgin = false;

            //clear old cell
            grid[fromX, fromY].color = null;
            grid[fromX, fromY].job = null;
            grid[fromX, fromY].virgin = false;

            //check for pawnTransform
            if (grid[toX, toY].job == "Pawn")
            {
                if (offense == "dark" && toY == 0 || offense == "light" && toY == 7)
                {
                    int r = rnd.Next(0, 10);

                    switch (r)
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                            grid[toX, toY].job = "Queen";
                            break;
                        case 5:
                        case 6:
                            grid[toX, toY].job = "Rook";
                            break;
                        case 7:
                        case 8:
                            grid[toX, toY].job = "Bishop";
                            break;
                        case 9:
                            grid[toX, toY].job = "Knight";
                            break;
                        default:
                            break;
                    }
                }
            }
            //check for castling
            else if (grid[toX, toY].job == "King")
            {
                int yCoor;

                if (offense == "dark")
                {
                    yCoor = 7;
                }
                else
                {
                    yCoor = 0;
                }

                if (fromX == 4 && fromY == yCoor)  //if moving from King default position
                {
                    if (toX == 2 && toY == yCoor)  //if moving two spaces to the left
                    {
                        //move to new cell
                        grid[3, yCoor].color = offense;
                        grid[3, yCoor].job = "Rook";

                        //clear old cell
                        grid[0, yCoor].color = null;
                        grid[0, yCoor].job = null;
                    }

                    else if (toX == 6 && toY == yCoor) //if moving two spaces to the right
                    {
                        //move to new cell
                        grid[5, yCoor].color = offense;
                        grid[5, yCoor].job = "Rook";

                        //clear old cell
                        grid[7, yCoor].color = null;
                        grid[7, yCoor].job = null;
                    }
                }
            }
            return grid;
        }

        private int getValue(piece[,] grid)
        {
            //looks at board and returns a value that indicates how good the computer is doing

            int total = 0;
            List<coordinate> compPieces = new List<coordinate>();
            List<coordinate> humanPieces = new List<coordinate>();

            if(opponent == "light")
            {
                compPieces = getLightPieces(grid);
                humanPieces = getDarkPieces(grid);
            }
            else
            {
                compPieces = getDarkPieces(grid);
                humanPieces = getLightPieces(grid);
            }

            foreach(coordinate c in compPieces)
            {
                switch(grid[c.x, c.y].job)
                {
                    case "Queen":
                        total += 5;
                        break;
                    case "Rook":
                        total += 3;
                        break;
                    case "Bishop":
                        total += 3;
                        break;
                    case "Knight":
                        total += 2;
                        break;
                    case "Pawn":
                        total += 1;
                        break;
                    default:
                        break;
                }
            }

            foreach (coordinate c in humanPieces)
            {
                switch (grid[c.x, c.y].job)
                {
                    case "Queen":
                        total -= 5;
                        break;
                    case "Rook":
                        total -= 3;
                        break;
                    case "Bishop":
                        total -= 3;
                        break;
                    case "Knight":
                        total -= 2;
                        break;
                    case "Pawn":
                        total -= 1;
                        break;
                    default:
                        break;
                }
            }
            return total;
        }

        private piece[,] deepCopy(piece[,] source)
        {
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

        private bool isInCheck(string teamInQuestion, piece[,] pArray)
        {
            //returns whether or not team in question is in check

            List<coordinate> spots;
            List<move> poss = new List<move>();

            if (teamInQuestion == "dark")
            {
                spots = getLightPieces(pArray);//get all opposing team's pieces
            }

            else
            {
                spots = getDarkPieces(pArray);//get all opposing team's pieces
            }

            foreach (coordinate c in spots)
            {
                //get possible moves of opposing team,
                //doesn't matter if opposing team move gets them in check,
                //still a valid move for current team
                poss.AddRange(getMoves(c, pArray));
            }

            foreach (move m in poss)
            {
                //if opposing team's move can capture your king, you're in check
                if (pArray[m.moveSpot.x, m.moveSpot.y].job == "King" &&
                    pArray[m.moveSpot.x, m.moveSpot.y].color == teamInQuestion)
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
            coordinate to;
            coordinate from;
            string fromColor;
            string toColor;
            string toJob;
            bool inCheck;

            allPossible = getMoves(aPiece, grid);

            foreach (move m in allPossible)
            {
                to = m.moveSpot;
                toColor = grid[to.x, to.y].color;
                toJob = grid[to.x, to.y].job;
                from = m.pieceSpot;
                fromColor = grid[from.x, from.y].color;

                //do moves
                grid[to.x, to.y].color = fromColor;
                grid[to.x, to.y].job = grid[from.x, from.y].job.ToString();
                grid[from.x, from.y].color = null;
                grid[from.x, from.y].job = null;

                //see if in check
                inCheck = isInCheck(fromColor, grid);

                if (inCheck == false)//if not in check
                {
                    possibleWithoutCheck.Add(m);
                }

                //reset pieces
                grid[from.x, from.y].color = grid[to.x, to.y].color;
                grid[from.x, from.y].job = grid[to.x, to.y].job;
                grid[to.x, to.y].color = toColor;
                grid[to.x, to.y].job = toJob;
            }
            return possibleWithoutCheck;
        }

        private bool isInCheckmate(string teamInQuestion, List<coordinate> availablePieces)
        {
            //takes list of pieces and returns whether or not player is in checkmate

            List<move> allPossible = new List<move>();
            List<move> possibleWithoutCheck = new List<move>();
            coordinate to;
            coordinate from;
            string toColor;
            string toJob;
            bool inCheck;

            //find all moves that can be done without going into check
            foreach (coordinate aPiece in availablePieces)
            {
                allPossible = getMoves(aPiece, pieceArray);

                foreach (move m in allPossible)
                {
                    to = m.moveSpot;
                    toColor = pieceArray[to.x, to.y].color;
                    toJob = pieceArray[to.x, to.y].job;
                    from = m.pieceSpot;

                    //do moves
                    pieceArray[to.x, to.y].color = teamInQuestion;
                    pieceArray[to.x, to.y].job = pieceArray[from.x, from.y].job.ToString();
                    pieceArray[from.x, from.y].color = null;
                    pieceArray[from.x, from.y].job = null;

                    //see if in check
                    inCheck = isInCheck(teamInQuestion, pieceArray);

                    if (inCheck == false)//if not in check
                    {
                        possibleWithoutCheck.Add(m);
                    }

                    //reset pieces
                    pieceArray[from.x, from.y].color = pieceArray[to.x, to.y].color;
                    pieceArray[from.x, from.y].job = pieceArray[to.x, to.y].job;
                    pieceArray[to.x, to.y].color = toColor;
                    pieceArray[to.x, to.y].job = toJob;
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

                if (onePlayer == false)
                {
                    string winningTeam = switchTeam(teamInQuestion);
                    message = "The " + winningTeam + " army has slain the " + teamInQuestion + " army's king in battle";
                }

                else
                {
                    if (teamInQuestion == opponent)
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
            coordinate toSpot = shift.moveSpot;
            coordinate fromSpot = shift.pieceSpot;

            if (offensiveTeam == "dark")
            {
                yCoor = 7;
            }
            else
            {
                yCoor = 0;
            }

            if (fromSpot.x == 4 && fromSpot.y == yCoor)  //if moving from King default position
            {
                if (toSpot.x == 2 && toSpot.y == yCoor)  //if moving two spaces to the left
                {
                    coordinate newCastleCoor = new coordinate(3, yCoor);
                    coordinate oldCastleCoor = new coordinate(0, yCoor);
                    castleMove.moveSpot = newCastleCoor;
                    castleMove.pieceSpot = oldCastleCoor;

                    if(networkGame == false)
                    {
                        node = new historyNode(castleMove, pieceArray[3, yCoor], false, true, true);
                        history.Push(node);
                    }
                    
                    movePiece(oldCastleCoor, newCastleCoor);
                }

                else if (toSpot.x == 6 && toSpot.y == yCoor) //if moving two spaces to the right
                {
                    coordinate newCastleCoor = new coordinate(5, yCoor);
                    coordinate oldCastleCoor = new coordinate(7, yCoor);
                    castleMove.moveSpot = newCastleCoor;
                    castleMove.pieceSpot = oldCastleCoor;

                    if(networkGame == false)
                    {
                        node = new historyNode(castleMove, pieceArray[5, yCoor], false, true, true);
                        history.Push(node);
                    }

                    movePiece(oldCastleCoor, newCastleCoor);
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
                else if (currentPiece.color == offensiveTeam)
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
                    MessageBox.Show("It is currently the other player's turn", "Wait",
                    MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.OK);
                }
            }
        }

        private void ownCellSelected(coordinate cCell)
        {
            movablePieceSelected = true;
            clearSelectedAndPossible();
            displayArray[cCell.x, cCell.y].tile.Background = Brushes.DeepSkyBlue;
            prevSelected = cCell;
            possible.Clear();
            possible.AddRange(getCheckRestrictedMoves(cCell, pieceArray));
            string defensiveTeam = switchTeam(offensiveTeam);

            foreach (move m in possible)
            {
                if (pieceArray[m.moveSpot.x, m.moveSpot.y].color == defensiveTeam)
                {
                    displayArray[m.moveSpot.x, m.moveSpot.y].tile.Background = Brushes.DarkOrange;
                }
                else
                {
                    displayArray[m.moveSpot.x, m.moveSpot.y].tile.Background = Brushes.LawnGreen;
                }
            }
        }

        private async Task movableCellSelected(coordinate cCell)
        {
            move curTurn = new move();
            bool movableSpot = false;

            foreach (move m in possible)
            {
                if (cCell.Equals(m.moveSpot))//if selected spot is in possible move list
                {
                    movableSpot = true;
                    curTurn = m;
                }
            }

            if (movableSpot == true)
            {
                historyNode node;
                string pawnTrans = "Pawn";
                piece captured = pieceArray[cCell.x, cCell.y];
                bool virginMove = pieceArray[prevSelected.x, prevSelected.y].virgin;
                movePiece(prevSelected, cCell);
                clearSelectedAndPossible();

                if (pieceArray[cCell.x, cCell.y].job == "Pawn")
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
                    byte pawnT;

                    switch(pawnTrans)
                    {
                        case "Queen":
                            pawnT = 2;
                            break;
                        case "Rook":
                            pawnT = 3;
                            break;
                        case "Bishop":
                            pawnT = 4;
                            break;
                        case "Knight":
                            pawnT = 5;
                            break;
                        default:
                            pawnT = 6;
                            break;
                    }
                    buffer[0] = 7;

                    if(opponent == "light")
                    {
                        buffer[1] = (byte)(curTurn.pieceSpot.x);
                        buffer[2] = (byte)(curTurn.pieceSpot.y);
                        buffer[3] = (byte)(curTurn.moveSpot.x);
                        buffer[4] = (byte)(curTurn.moveSpot.y);
                    }
                    else
                    {
                        buffer[1] = (byte)(curTurn.pieceSpot.x);
                        buffer[2] = (byte)(7 - curTurn.pieceSpot.y);
                        buffer[3] = (byte)(curTurn.moveSpot.x);
                        buffer[4] = (byte)(7 - curTurn.moveSpot.y);
                    }
                    buffer[5] = pawnT;
                    nwStream.Write(buffer, 0, 6);
                }

                if (lastMove == true)
                {
                    clearToAndFrom();
                    displayArray[curTurn.pieceSpot.x, curTurn.pieceSpot.y].bottom.Source = bmpFrom;
                    displayArray[curTurn.moveSpot.x, curTurn.moveSpot.y].bottom.Source = bmpTo;
                }

                if (pieceArray[cCell.x, cCell.y].job == "King")
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
            if (offensiveTeam == "light")
            {
                offensiveTeam = "dark";
                endOfGame = isInCheckmate(offensiveTeam, getDarkPieces(pieceArray));  //did previous turn put other team in checkmate?

                if (endOfGame == false && onePlayer == true)
                {
                    await compTurn();
                    endOfGame = isInCheckmate("light", getLightPieces(pieceArray)); //did computer turn put player in checkmate?
                    offensiveTeam = "light";
                }
            }
            else
            {
                offensiveTeam = "light";
                endOfGame = isInCheckmate(offensiveTeam, getLightPieces(pieceArray)); //did previous turn put other team in checkmate?

                if (endOfGame == false && onePlayer == true)
                {
                    await compTurn();
                    endOfGame = isInCheckmate("dark", getDarkPieces(pieceArray)); //did computer turn put player in checkmate?
                    offensiveTeam = "dark";
                }
            }

            if(networkGame == true && endOfGame == false)
            {
                ready = false;
                //wait for response move
            }
            //if 2Player local, rotate is on, and didn't end game
            else if (onePlayer == false && rotate == true && endOfGame == false)
            {
                if(offensiveTeam == "dark")
                {
                    rotateBoard(true, rotationDuration);
                }
                else
                {
                    rotateBoard(false, rotationDuration);
                }
            }
        }

        private async Task compTurn()
        {
            //computer's turn
            
            historyNode node;
            int r;
            List<move> possibleWithoutCheck = new List<move>();

            if (hardMode == true || medMode == true)
            {
                Progress<int> percent = null;

                if (hardMode == true)
                {
                    percent = new Progress<int>();
                    percent.ProgressChanged += (sender, e) => { think.update(e); };
                    think = new Thinking(rnd);
                    think.Owner = mWindow;
                    think.Show();
                }
                
                ready = false;
                await Task.Run(() => evaluator(pieceArray, offensiveTeam, 0, true, -30, 30, percent));
                ready = true;
            }

            else
            {
                if (offensiveTeam == "dark")
                {
                    foreach (coordinate cell in getDarkPieces(pieceArray))
                    {
                        possibleWithoutCheck.AddRange(getCheckRestrictedMoves(cell, pieceArray));
                    }
                }
                else
                {
                    foreach (coordinate cell in getLightPieces(pieceArray))
                    {
                        possibleWithoutCheck.AddRange(getCheckRestrictedMoves(cell, pieceArray));
                    }
                }

                r = rnd.Next(0, possibleWithoutCheck.Count);//choose random move
                bestMove = possibleWithoutCheck[r];
            }

            coordinate newSpot = new coordinate(bestMove.moveSpot.x, bestMove.moveSpot.y);
            coordinate oldSpot = new coordinate(bestMove.pieceSpot.x, bestMove.pieceSpot.y);

            piece captured = pieceArray[newSpot.x, newSpot.y];
            bool virginMove = pieceArray[oldSpot.x, oldSpot.y].virgin;
            movePiece(oldSpot, newSpot);

            if (pieceArray[newSpot.x, newSpot.y].job == "Pawn" && newSpot.y == 0)//if pawn makes it to last row
            {
                r = rnd.Next(0, 10); //choose random piece to transform into

                if (opponent == "dark")
                {
                    switch (r)
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                            pieceArray[newSpot.x, newSpot.y].job = "Queen";
                            displayArray[newSpot.x, newSpot.y].top.Source = dQueen;
                            break;
                        case 5:
                        case 6:
                            pieceArray[newSpot.x, newSpot.y].job = "Rook";
                            displayArray[newSpot.x, newSpot.y].top.Source = dRook;
                            break;
                        case 7:
                        case 8:
                            pieceArray[newSpot.x, newSpot.y].job = "Bishop";
                            displayArray[newSpot.x, newSpot.y].top.Source = dBishop;
                            break;
                        case 9:
                            pieceArray[newSpot.x, newSpot.y].job = "Knight";
                            displayArray[newSpot.x, newSpot.y].top.Source = dKnight;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    switch (r)
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                            pieceArray[newSpot.x, newSpot.y].job = "Queen";
                            displayArray[newSpot.x, newSpot.y].top.Source = lQueen;
                            break;
                        case 5:
                        case 6:
                            pieceArray[newSpot.x, newSpot.y].job = "Rook";
                            displayArray[newSpot.x, newSpot.y].top.Source = lRook;
                            break;
                        case 7:
                        case 8:
                            pieceArray[newSpot.x, newSpot.y].job = "Bishop";
                            displayArray[newSpot.x, newSpot.y].top.Source = lBishop;
                            break;
                        case 9:
                            pieceArray[newSpot.x, newSpot.y].job = "Knight";
                            displayArray[newSpot.x, newSpot.y].top.Source = lKnight;
                            break;
                        default:
                            break;
                    }
                }
                node = new historyNode(bestMove, captured, true, true, false);
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
                displayArray[bestMove.pieceSpot.x, bestMove.pieceSpot.y].bottom.Source = bmpFrom;
                displayArray[bestMove.moveSpot.x, bestMove.moveSpot.y].bottom.Source = bmpTo;
            }

            if (pieceArray[newSpot.x, newSpot.y].job == "King")
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
                }
                
                else
                {
                    rotation = new DoubleAnimation(180, 360, TimeSpan.FromSeconds(time));
                    ft.ScaleY = 1;
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
            pieceArray[oldCell.x, oldCell.y].color = null;
            pieceArray[oldCell.x, oldCell.y].job = null;
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
            int xMove;
            int yMove;
            int xPiece;
            int yPiece;

            historyNode node = history.Pop();

            xMove = node.step.moveSpot.x;
            yMove = node.step.moveSpot.y;
            xPiece = node.step.pieceSpot.x;
            yPiece = node.step.pieceSpot.y;

            to = pieceArray[xMove, yMove];
            from = pieceArray[xPiece, yPiece];
            offensiveTeam = to.color;

            if (rotate == true && onePlayer == false)
            {
                if(offensiveTeam == "dark")
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
                if (to.color == "light")
                {
                    pawnPic = lPawn;
                }

                else
                {
                    pawnPic = dPawn;
                }

                pieceArray[xPiece, yPiece].job = "Pawn";
                displayArray[xPiece, yPiece].top.Source = pawnPic;
            }

            else
            {
                pieceArray[xPiece, yPiece].job = to.job;
                displayArray[xPiece, yPiece].top.Source = matchPicture(to);
            }

            pieceArray[xPiece, yPiece].color = to.color;
            pieceArray[xPiece, yPiece].virgin = node.firstMove;

            //put captured piece back
            pieceArray[xMove, yMove].job = node.captured.job;
            displayArray[xMove, yMove].top.Source = matchPicture(node.captured);
            pieceArray[xMove, yMove].color = node.captured.color;
            pieceArray[xMove, yMove].virgin = node.captured.virgin;


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

            if (figure.color == "dark")
            {
                switch (figure.job)
                {
                    case "King":
                        return dKing;
                    case "Queen":
                        return dQueen;
                    case "Bishop":
                        return dBishop;
                    case "Knight":
                        return dKnight;
                    case "Rook":
                        return dRook;
                    case "Pawn":
                        return dPawn;
                    default:
                        return null;
                }
            }
            else
            {
                switch (figure.job)
                {
                    case "King":
                        return lKing;
                    case "Queen":
                        return lQueen;
                    case "Bishop":
                        return lBishop;
                    case "Knight":
                        return lKnight;
                    case "Rook":
                        return lRook;
                    case "Pawn":
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

            string themeName = themeList[themeIndex];
            try
            {
                lKing = new BitmapImage(new Uri("pack://application:,,,/" + themeName + ";component/lKing.png"));
                lQueen = new BitmapImage(new Uri("pack://application:,,,/" + themeName + ";component/lQueen.png"));
                lBishop = new BitmapImage(new Uri("pack://application:,,,/" + themeName + ";component/lBishop.png"));
                lKnight = new BitmapImage(new Uri("pack://application:,,,/" + themeName + ";component/lKnight.png"));
                lRook = new BitmapImage(new Uri("pack://application:,,,/" + themeName + ";component/lRook.png"));
                lPawn = new BitmapImage(new Uri("pack://application:,,,/" + themeName + ";component/lPawn.png"));
                dKing = new BitmapImage(new Uri("pack://application:,,,/" + themeName + ";component/dKing.png"));
                dQueen = new BitmapImage(new Uri("pack://application:,,,/" + themeName + ";component/dQueen.png"));
                dBishop = new BitmapImage(new Uri("pack://application:,,,/" + themeName + ";component/dBishop.png"));
                dKnight = new BitmapImage(new Uri("pack://application:,,,/" + themeName + ";component/dKnight.png"));
                dRook = new BitmapImage(new Uri("pack://application:,,,/" + themeName + ";component/dRook.png"));
                dPawn = new BitmapImage(new Uri("pack://application:,,,/" + themeName + ";component/dPawn.png"));
            }
            catch (IOException)
            {
                themeList.RemoveAt(themeIndex);
                themeIndex--;
            }
        }

        public void initializeDlls()
        {
            //Initializes all dlls

            MessageBoxResult result;

            while (!File.Exists(pwd + "//Trinet.Core.IO.Ntfs.dll"))
            {
                result = MessageBox.Show(
                    "Required dll not found.\n\nPlace 'Trinet.Core.IO.Ntfs.dll'\nin directory containing 'Chess.exe'\n\nTry Again?",
                    "Missing Dll", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);

                if(result == MessageBoxResult.No)
                {
                    Environment.Exit(1);
                }
            }

            addDllsToList();

            //find default theme
            themeIndex = themeList.FindIndex(x => x == "Figure");

            if (themeIndex == -1)    //if can't find default
            {
                themeIndex = 0;
            }
            changeThemeInternally();
        }

        private void addDllsToList()
        {
            //searches dlls in working directory and puts in themeList

            AssemblyName an;
            FileInfo file;
            string name;
            themeList = new List<string>();
            ignore = new List<string>();
            string[] dllFilePathArray = null;
            bool themesFound = false;

            while (themesFound == false)
            {
                try
                {
                    dllFilePathArray = Directory.GetFiles(pwd, "*.dll");
                    themeIndex = 0;

                    foreach (string dllFilePath in dllFilePathArray.Except(ignore))
                    {
                        file = new FileInfo(dllFilePath);
                        file.DeleteAlternateDataStream("Zone.Identifier");
                        an = AssemblyName.GetAssemblyName(dllFilePath);
                        name = an.Name;
                        Assembly.Load(an);

                        if (!themeList.Contains(name))  //check for duplicates
                        {
                            themeList.Add(name);
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

                if (themeList.Count == 0)    //if 0 good themes
                {
                    MessageBoxResult result = MessageBox.Show(
                    "No themes found.\n\nPlace theme dll in directory containing 'Chess.exe'\n\nTry Again?",
                    "Missing Dll", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);

                    if (result == MessageBoxResult.No)
                    {
                        Environment.Exit(1);
                    }
                    ignore.Clear();
                    themeList.Clear();
                }
                else
                {
                    themesFound = true;
                }
            }
        }

        public void saveState()
        {
            //saves game on exit
            //if save game unchecked, saves preferences, but not game state

            string theme = themeList[themeIndex];
            saveData sData = new saveData(pieceArray, offensiveTeam, opponent, theme, onePlayer, networkGame, medMode, hardMode,
                lastMove, saveGame, ready, rotate, rotationDuration);

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

                if (lData.sSaveGame == true)
                {
                    if (lData.sReady == true && lData.sNetwork == false)
                    {
                        ready = true;
                        pieceArray = new piece[8, 8];
                        movablePieceSelected = false;
                        pieceArray = lData.sBoard;
                        offensiveTeam = lData.sOffensiveTeam;
                        opponent = lData.sOpponent;
                        onePlayer = lData.sOnePlayer;
                        medMode = lData.sMedMode;
                        hardMode = lData.sHardMode;
                        //if 2Player local, rotate is on, and opponent's turn
                        if(rotate == true && offensiveTeam == opponent && onePlayer == false)
                        {
                            if(offensiveTeam == "dark")
                            {
                                rotateBoard(true, 0);
                            }
                            else
                            {
                                rotateBoard(false, 0);
                            }
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
            catch (InvalidCastException) //Error loading data
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
                    if(offensiveTeam == "dark")
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
                    if (offensiveTeam == "dark")
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
                    MessageBox.Show("You have been disconnected from the Server", "Disconnected",
                    MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.OK);

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
                    MessageBox.Show("Your opponent has forfeited the match", "Game Over",
                    MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.OK);

                    ready = false;
                    break;
                }
                else if (buffer[0] == 4)
                {
                    MessageBox.Show("Sorry\n\nYou gave a valiant effort,\nbut you have been bested in battle by the enemy army",
                        "Game Over", MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.OK);

                    ready = false;
                    break;
                }
                //move
                else if (buffer[0] == 7)
                {
                    coordinate p;
                    coordinate m;

                    if(opponent == "light")
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
                    doMove(opponentsMove, buffer[5]);
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
            removeChat();
            clearToAndFrom();
            clearSelectedAndPossible();

            foreach (display d in displayArray)
            {
                d.tile.Cursor = Cursors.Arrow;
            }
        }

        private void doMove(move m, byte pawn)
        {
            int xMove = m.moveSpot.x;
            int yMove = m.moveSpot.y;

            //movePiece
            movePiece(m.pieceSpot, m.moveSpot);
            //pawnTransform
            if (pieceArray[xMove, yMove].job == "Pawn")
            {
                if (yMove == 0)
                {
                    string pawnT;

                    switch (pawn)
                    {
                        case 2:
                            pawnT = "Queen";
                            break;
                        case 3:
                            pawnT = "Rook";
                            break;
                        case 4:
                            pawnT = "Bishop";
                            break;
                        case 5:
                            pawnT = "Knight";
                            break;
                        default:
                            pawnT = null;
                            break;
                    }

                    pieceArray[xMove, yMove].job = pawnT;
                    displayArray[xMove, yMove].top.Source = matchPicture(pieceArray[xMove, yMove]);
                }
            }
            //clear then set to and from
            if (lastMove == true)
            {
                clearToAndFrom();
                displayArray[m.pieceSpot.x, m.pieceSpot.y].bottom.Source = bmpFrom;
                displayArray[xMove, yMove].bottom.Source = bmpTo;
            }
            //check for castling
            if (pieceArray[xMove, yMove].job == "King")
            {
                castling(m);
            }

            offensiveTeam = switchTeam(offensiveTeam);
            ready = true;
        }

        public void removeChat()
        {
            mWindow.Board.Width -= 300;
            mWindow.chat.Visibility = Visibility.Hidden;
            mWindow.split.Visibility = Visibility.Hidden;
            mWindow.space.ColumnDefinitions.RemoveAt(2);
            mWindow.space.ColumnDefinitions.RemoveAt(1);
        }

        public string switchTeam(string input)
        {
            if(input == "light")
            {
                return "dark";
            }
            else
            {
                return "light";
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
            string pieceColor = pArray[current.x, current.y].color;
            string oppositeColor = switchTeam(pieceColor);

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
            string pieceColor = pArray[current.x, current.y].color;

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
            string pieceColor = pArray[current.x, current.y].color;
            string oppositeColor = switchTeam(pieceColor);

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
            string pieceColor = pArray[current.x, current.y].color;

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
                if (pieceColor == "dark")
                {
                    if (pArray[0, 7].virgin == true)//if left rook's first move
                    {
                        //if clear path from rook to king
                        if (pArray[1, 7].job == null && pArray[2, 7].job == null && pArray[3, 7].job == null)
                        {
                            moveCoor.x = 2;
                            moveCoor.y = 7;
                            availableMove = new move(current, moveCoor);
                            availableList.Add(availableMove);
                        }
                    }

                    if (pArray[7, 7].virgin == true)//if right rook's first move
                    {
                        if (pArray[6, 7].job == null && pArray[5, 7].job == null)
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
                        if (pArray[1, 0].job == null && pArray[2, 0].job == null && pArray[3, 0].job == null)
                        {
                            moveCoor.x = 2;
                            moveCoor.y = 0;
                            availableMove = new move(current, moveCoor);
                            availableList.Add(availableMove);
                        }
                    }

                    if (pArray[7, 0].virgin == true)//if right rook's first move
                    {
                        if (pArray[6, 0].job == null && pArray[5, 0].job == null)
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
            string pieceColor = pArray[current.x, current.y].color;
            string oppositeColor = switchTeam(pieceColor);

            if (pieceColor == "dark")
            {
                //search down
                availableY--;
                if (availableY >= 0)
                {
                    if (pArray[availableX, availableY].color == null)
                    {
                        moveCoor.x = availableX;
                        moveCoor.y = availableY;
                        availableMove = new move(current, moveCoor);
                        availableList.Add(availableMove);

                        //search first move
                        availableY--;
                        if (availableY >= 0 && pArray[current.x, current.y].virgin == true)
                        {
                            if (pArray[availableX, availableY].color == null)
                            {
                                moveCoor.x = availableX;
                                moveCoor.y = availableY;
                                availableMove = new move(current, moveCoor);
                                availableList.Add(availableMove);
                            }
                        }
                        availableY++;
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
                //search up
                availableY++;
                if (availableY < 8)
                {
                    if (pArray[availableX, availableY].color == null)
                    {
                        moveCoor.x = availableX;
                        moveCoor.y = availableY;
                        availableMove = new move(current, moveCoor);
                        availableList.Add(availableMove);

                        //search first move
                        availableY++;
                        if (availableY < 8 && pArray[current.x, current.y].virgin == true)
                        {
                            if (pArray[availableX, availableY].color == null)
                            {
                                moveCoor.x = availableX;
                                moveCoor.y = availableY;
                                availableMove = new move(current, moveCoor);
                                availableList.Add(availableMove);
                            }
                        }
                        availableY--;
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
