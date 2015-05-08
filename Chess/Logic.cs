using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
        public int bytesRead;
        public byte[] buffer = new byte[512];
        private static Random rnd = new Random();
        public bool rotate = true;                          //Rotate board between turns on 2Player mode?
        public bool lastMove = true;                        //is lastMove menu option checked?
        public bool saveGame = true;                        //Save game on exit?
        public string IP = "127.0.0.1";                     //IP address of server
        public int port = 54321;                            //port of server
        public double rotationDuration = 3;                 //how long the rotation animation takes
        public bool movablePieceSelected = false;           //if true, the next click will move the selected piece if possible
        private List<move> possible = new List<move>();     //list of all possible moves
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
            public int value { get; set; }              //how good the move is

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

        private List<coordinate> getDarkPieces()
        {
            //searches through pieceArray and returns list of coordinates where all dark pieces are located

            coordinate temp = new coordinate();
            List<coordinate> darkPieces = new List<coordinate>();

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    if (pieceArray[x, y].color == "dark")
                    {
                        temp.x = x;
                        temp.y = y;
                        darkPieces.Add(temp);
                    }
                }
            }
            return darkPieces;
        }

        private List<coordinate> getLightPieces()
        {
            //searches through pieceArray and returns list of coordinates where all light pieces are located

            coordinate temp = new coordinate();
            List<coordinate> lightPieces = new List<coordinate>();

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    if (pieceArray[x, y].color == "light")
                    {
                        temp.x = x;
                        temp.y = y;
                        lightPieces.Add(temp);
                    }
                }
            }
            return lightPieces;
        }

        private List<coordinate> getAllPieces()
        {
            //searches through pieceArray and returns list of coordinates where all pieces are located

            coordinate temp = new coordinate();
            List<coordinate> allPieces = new List<coordinate>();

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    if (pieceArray[x, y].color != null)
                    {
                        temp.x = x;
                        temp.y = y;
                        allPieces.Add(temp);
                    }
                }
            }
            return allPieces;
        }

        private List<move> getMoves(coordinate spot)
        {
            //returns all possible moves of spot disregarding check restrictions
            //determines job of piece and calls apropriate function to get correct move list

            List<move> temp = new List<move>();

            switch (pieceArray[spot.x, spot.y].job)
            {
                case "Rook":
                    return rookMoves(spot);
                case "Knight":
                    return knightMoves(spot);
                case "Bishop":
                    return bishopMoves(spot);
                case "Queen":
                    temp.AddRange(bishopMoves(spot));
                    temp.AddRange(rookMoves(spot));
                    return temp;
                case "King":
                    return kingMoves(spot);
                case "Pawn":
                    return pawnMoves(spot);
                default:
                    return temp;    //temp should be empty
            }
        }

        private move medLogic(List<move> pos)
        {
            //gets executed if player selects medium mode

            List<move> bestMovesList = new List<move>();

            for (int i = 0; i < pos.Count; i++)
            {
                switch (pieceArray[pos[i].moveSpot.x, pos[i].moveSpot.y].job)    //what piece can you capture
                {
                    case "Queen":
                        pos[i].value = 30;
                        break;
                    case "Rook":
                        pos[i].value = 24;
                        break;
                    case "Bishop":
                        pos[i].value = 18;
                        break;
                    case "Knight":
                        pos[i].value = 12;
                        break;
                    case "Pawn":
                        pos[i].value = 6;
                        break;
                    default:    //empty cell
                        pos[i].value = 0;
                        break;
                }

                switch (pieceArray[pos[i].pieceSpot.x, pos[i].pieceSpot.y].job)    //what piece does that capturing
                {
                    case "King":
                        pos[i].value -= 5;
                        break;
                    case "Queen":
                        pos[i].value -= 4;
                        break;
                    case "Rook":
                        pos[i].value -= 3;
                        break;
                    case "Bishop":
                        pos[i].value -= 2;
                        break;
                    case "Knight":
                        pos[i].value -= 1;
                        break;
                    default:    //pawn
                        break;
                }
            }
            pos.Sort((x, y) => y.value.CompareTo(x.value)); //descending order sort

            for (int j = 0; j < pos.Count; j++)
            {
                if (pos[j].value != pos[0].value)    //find all moves with highest value
                {
                    break;
                }
                bestMovesList.Add(pos[j]);  //add them to list
            }
            return bestMovesList[rnd.Next(0, bestMovesList.Count)]; //choose random move from bestMovesList
        }

        private move hardLogic(List<move> pos)
        {
            //gets executed if player selects hard mode

            List<move> humanMoves = new List<move>();
            List<move> bestMovesList = new List<move>();
            List<coordinate> humanPiecesAfterMove = new List<coordinate>();
            coordinate to;
            coordinate from;
            string fromColor;
            string toColor;
            string toJob;

            for (int i = 0; i < pos.Count; i++) //go through all moves
            {
                switch (pieceArray[pos[i].moveSpot.x, pos[i].moveSpot.y].job)    //find value of computer move
                {
                    case "Queen":
                        pos[i].value = 60;
                        break;
                    case "Rook":
                        pos[i].value = 48;
                        break;
                    case "Bishop":
                        pos[i].value = 36;
                        break;
                    case "Knight":
                        pos[i].value = 24;
                        break;
                    case "Pawn":
                        pos[i].value = 12;
                        break;
                    default:    //empty cell
                        pos[i].value = 0;
                        break;
                }

                switch (pieceArray[pos[i].pieceSpot.x, pos[i].pieceSpot.y].job)    //what piece does the capturing
                {
                    case "King":
                        pos[i].value -= 5;
                        break;
                    case "Queen":
                        pos[i].value -= 4;
                        break;
                    case "Rook":
                        pos[i].value -= 3;
                        break;
                    case "Bishop":
                        pos[i].value -= 2;
                        break;
                    case "Knight":
                        pos[i].value -= 1;
                        break;
                    default:    //pawn
                        break;
                }
                to = pos[i].moveSpot;
                toColor = pieceArray[to.x, to.y].color;
                toJob = pieceArray[to.x, to.y].job;
                from = pos[i].pieceSpot;
                fromColor = pieceArray[from.x, from.y].color;

                //do move
                pieceArray[to.x, to.y].color = fromColor;
                pieceArray[to.x, to.y].job = pieceArray[from.x, from.y].job.ToString();
                pieceArray[from.x, from.y].color = null;
                pieceArray[from.x, from.y].job = null;

                //human turn
                if (opponent == "dark")
                {
                    humanPiecesAfterMove = getLightPieces();
                }
                else
                {
                    humanPiecesAfterMove = getDarkPieces();
                }

                foreach (coordinate c in humanPiecesAfterMove)   //go through each human piece
                {
                    humanMoves.AddRange(getCheckRestrictedMoves(c));
                }

                for (int j = 0; j < humanMoves.Count; j++)
                {
                    switch (pieceArray[humanMoves[j].moveSpot.x, humanMoves[j].moveSpot.y].job)
                    {
                        case "Queen":
                            humanMoves[j].value = 30;
                            break;
                        case "Rook":
                            humanMoves[j].value = 24;
                            break;
                        case "Bishop":
                            humanMoves[j].value = 18;
                            break;
                        case "Knight":
                            humanMoves[j].value = 12;
                            break;
                        case "Pawn":
                            humanMoves[j].value = 6;
                            break;
                        default:    //empty cell
                            humanMoves[j].value = 0;
                            break;
                    }
                    //what piece does the capturing
                    switch (pieceArray[humanMoves[j].pieceSpot.x, humanMoves[j].pieceSpot.y].job)
                    {
                        case "King":
                            humanMoves[j].value -= 5;
                            break;
                        case "Queen":
                            humanMoves[j].value -= 4;
                            break;
                        case "Rook":
                            humanMoves[j].value -= 3;
                            break;
                        case "Bishop":
                            humanMoves[j].value -= 2;
                            break;
                        case "Knight":
                            humanMoves[j].value -= 1;
                            break;
                        default:    //pawn
                            break;
                    }
                }
                humanMoves.Sort((x, y) => x.value.CompareTo(y.value));  //sort ascending

                for (int j = 0; j < humanMoves.Count; j++)
                {
                    if (humanMoves[j].value != humanMoves[0].value)    //find all moves with lowest value
                    {
                        break;
                    }
                    bestMovesList.Add(humanMoves[j]);  //add them to list
                }
                //score of computer move - human reaction move
                pos[i].value -= bestMovesList[rnd.Next(0, bestMovesList.Count)].value;

                //reset pieces
                pieceArray[from.x, from.y].color = pieceArray[to.x, to.y].color;
                pieceArray[from.x, from.y].job = pieceArray[to.x, to.y].job;
                pieceArray[to.x, to.y].color = toColor;
                pieceArray[to.x, to.y].job = toJob;
            }
            pos.Sort((x, y) => y.value.CompareTo(x.value)); //descending order sort
            bestMovesList.Clear();

            for (int i = 0; i < pos.Count; i++)
            {
                if (pos[i].value != pos[0].value)    //find all moves with highest value
                {
                    break;
                }
                bestMovesList.Add(pos[i]);  //add them to list
            }
            return bestMovesList[rnd.Next(0, bestMovesList.Count)]; //choose random move from bestMovesList
        }

        private bool isInCheck(string teamInQuestion)
        {
            //returns whether or not team in question is in check

            List<coordinate> spots;
            List<move> poss = new List<move>();

            if (teamInQuestion == "dark")
            {
                spots = getLightPieces();//get all opposing team's pieces
            }

            else
            {
                spots = getDarkPieces();//get all opposing team's pieces
            }

            foreach (coordinate c in spots)
            {
                //get possible moves of opposing team,
                //doesn't matter if opposing team move gets them in check,
                //still a valid move for current team
                poss.AddRange(getMoves(c));
            }

            foreach (move m in poss)
            {
                //if opposing team's move can capture your king, you're in check
                if (pieceArray[m.moveSpot.x, m.moveSpot.y].job == "King" &&
                    pieceArray[m.moveSpot.x, m.moveSpot.y].color == teamInQuestion)
                {
                    return true;
                }
            }
            return false;
        }

        private List<move> getCheckRestrictedMoves(coordinate aPiece)
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

            allPossible = getMoves(aPiece);

            foreach (move m in allPossible)
            {
                to = m.moveSpot;
                toColor = pieceArray[to.x, to.y].color;
                toJob = pieceArray[to.x, to.y].job;
                from = m.pieceSpot;
                fromColor = pieceArray[from.x, from.y].color;

                //do moves
                pieceArray[to.x, to.y].color = fromColor;
                pieceArray[to.x, to.y].job = pieceArray[from.x, from.y].job.ToString();
                pieceArray[from.x, from.y].color = null;
                pieceArray[from.x, from.y].job = null;

                //see if in check
                inCheck = isInCheck(fromColor);

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
                allPossible = getMoves(aPiece);

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
                    inCheck = isInCheck(teamInQuestion);

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
                
                foreach(display d in displayArray)
                {
                    d.tile.Cursor = Cursors.Arrow;
                }

                if (onePlayer == false)
                {
                    string winningTeam;

                    if (teamInQuestion == "light")
                    {
                        winningTeam = "dark";
                    }

                    else
                    {
                        winningTeam = "light";
                    }

                    message = "The " + winningTeam + " army has slain the " + teamInQuestion + " army's king in battle";
                }

                else
                {
                    if (teamInQuestion == opponent)
                    {
                        message = "Congratulations!\n\nYou have slain the evil king\n and saved the princess!";
                    }

                    else
                    {
                        message = "Sorry\n\nYou gave a valiant effort,\nbut you have been bested in battle by the enemy army";
                    }
                }

                if(networkGame == true)
                {
                    byte[] gameOver = new byte[1] {1};
                    nwStream.Write(gameOver, 0, 1);
                }

                MessageBoxResult result = MessageBox.Show(message + "\n\nTry Again?", "Game Over",
                        MessageBoxButton.YesNo, MessageBoxImage.None, MessageBoxResult.Yes);

                if (result == MessageBoxResult.Yes)
                {
                    newGame();
                }
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

        public void clicker(coordinate currentCell)
        {
            //human player's turn, gets called when player clicks on spot

            //check if still connected to server
            if (networkGame == true && client.Connected == false)
            {
                MessageBox.Show("You have been disconnected from the server",
                    "Disconnected", MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.OK);
                ready = false;
            }

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
                    movableCellSelected(currentCell);
                }
            }
        }

        private void ownCellSelected(coordinate cCell)
        {
            string defensiveTeam;
            movablePieceSelected = true;
            clearSelectedAndPossible();
            displayArray[cCell.x, cCell.y].tile.Background = Brushes.DeepSkyBlue;
            prevSelected = cCell;
            possible.Clear();
            possible.AddRange(getCheckRestrictedMoves(cCell));

            if (offensiveTeam == "light")
            {
                defensiveTeam = "dark";
            }
            else
            {
                defensiveTeam = "light";
            }

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

        private void movableCellSelected(coordinate cCell)
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
                betweenTurns();
            }
            else    //if didn't select movable spot
            {
                clearSelectedAndPossible();
                movablePieceSelected = false;
            }
        }

        private void betweenTurns()
        {
            //In between light and dark's turns

            List<move> possibleWithoutCheck = new List<move>();
            bool endOfGame;

            //change teams
            if (offensiveTeam == "light")
            {
                offensiveTeam = "dark";
                endOfGame = isInCheckmate(offensiveTeam, getDarkPieces());  //did previous turn put other team in checkmate?

                if (endOfGame == false && onePlayer == true)
                {
                    foreach (coordinate cell in getDarkPieces())
                    {
                        possibleWithoutCheck.AddRange(getCheckRestrictedMoves(cell));
                    }

                    compTurn(possibleWithoutCheck);
                    endOfGame = isInCheckmate("light", getLightPieces()); //did computer turn put player in checkmate?
                    offensiveTeam = "light";
                }
            }
            else
            {
                offensiveTeam = "light";
                endOfGame = isInCheckmate(offensiveTeam, getLightPieces()); //did previous turn put other team in checkmate?

                if (endOfGame == false && onePlayer == true)
                {
                    foreach (coordinate cell in getLightPieces())
                    {
                        possibleWithoutCheck.AddRange(getCheckRestrictedMoves(cell));
                    }

                    compTurn(possibleWithoutCheck);
                    endOfGame = isInCheckmate("dark", getDarkPieces()); //did computer turn put player in checkmate?
                    offensiveTeam = "dark";
                }
            }

            if(networkGame == true)
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

        private void compTurn(List<move> poss)
        {
            //computer's turn

            historyNode node;
            move bestMove;
            int r;

            if (medMode == true)
            {
                bestMove = medLogic(poss);
            }

            else if (hardMode == true)
            {
                bestMove = hardLogic(poss);
            }

            else
            {
                r = rnd.Next(0, poss.Count);//choose random move
                bestMove = poss[r];
            }

            coordinate newSpot = new coordinate(bestMove.moveSpot.x, bestMove.moveSpot.y);
            coordinate oldSpot = new coordinate(bestMove.pieceSpot.x, bestMove.pieceSpot.y);

            piece captured = pieceArray[newSpot.x, newSpot.y];
            bool virginMove = pieceArray[oldSpot.x, oldSpot.y].virgin;
            movePiece(oldSpot, newSpot);

            if (pieceArray[newSpot.x, newSpot.y].job == "Pawn" && newSpot.y == 0)//if pawn makes it to last row
            {
                r = rnd.Next(0, 4); //choose random piece to transform into

                if (opponent == "dark")
                {
                    switch (r)
                    {
                        case 0:
                            pieceArray[newSpot.x, newSpot.y].job = "Queen";
                            displayArray[newSpot.x, newSpot.y].top.Source = dQueen;
                            break;
                        case 1:
                            pieceArray[newSpot.x, newSpot.y].job = "Rook";
                            displayArray[newSpot.x, newSpot.y].top.Source = dRook;
                            break;
                        case 2:
                            pieceArray[newSpot.x, newSpot.y].job = "Bishop";
                            displayArray[newSpot.x, newSpot.y].top.Source = dBishop;
                            break;
                        case 3:
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
                            pieceArray[newSpot.x, newSpot.y].job = "Queen";
                            displayArray[newSpot.x, newSpot.y].top.Source = lQueen;
                            break;
                        case 1:
                            pieceArray[newSpot.x, newSpot.y].job = "Rook";
                            displayArray[newSpot.x, newSpot.y].top.Source = lRook;
                            break;
                        case 2:
                            pieceArray[newSpot.x, newSpot.y].job = "Bishop";
                            displayArray[newSpot.x, newSpot.y].top.Source = lBishop;
                            break;
                        case 3:
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

            //delete prev cell
            pieceArray[oldCell.x, oldCell.y].color = null;
            pieceArray[oldCell.x, oldCell.y].job = null;

            //overwrite current image
            //take previous piece picture and put it in current cell picture box
            displayArray[newCell.x, newCell.y].top.Source = matchPicture(pPiece);

            //delete prev image
            displayArray[oldCell.x, oldCell.y].top.Source = null;

            movablePieceSelected = false;
            pieceArray[newCell.x, newCell.y].virgin = false;
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

            foreach (coordinate spot in getAllPieces())
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

            NewGame play = new NewGame(this);
            play.ShowDialog();
        }

        public async void continuousReader()
        {
            while(true)
            {
                bytesRead = await nwStream.ReadAsync(buffer, 0, 512);

                if(bytesRead == 0)
                {
                    break;
                }
                else if (bytesRead == 1)
                {
                    if(buffer[0] == 3)
                    {
                        ready = false;

                        string message = "Your opponent has forfeited the match";

                        MessageBoxResult result = MessageBox.Show(message + "\n\nTry Again?", "Game Over",
                        MessageBoxButton.YesNo, MessageBoxImage.None, MessageBoxResult.Yes);

                        if (result == MessageBoxResult.Yes)
                        {
                            newGame();
                        }
                    }
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

            if(offensiveTeam == "dark")
            {
                offensiveTeam = "light";
            }
            else
            {
                offensiveTeam = "dark";
            }
            ready = true;
        }

        //the next few functions define the rules for what piece can move where in any situation
        //does not account for check restrictions
        //takes coordinate and returns list of possible moves for that piece

        private List<move> rookMoves(coordinate current)
        {
            string oppositeColor;
            move availableMove = new move();
            int availableX = current.x;             //put coordinate in temp variable to manipulate while preserving original
            int availableY = current.y;
            List<move> availableList = new List<move>();
            coordinate moveCoor = new coordinate(); //when found possible move, put in this variable to add to list
            string pieceColor = pieceArray[current.x, current.y].color;

            if (pieceColor == "light")
            {
                oppositeColor = "dark";
            }

            else
            {
                oppositeColor = "light";
            }

            //search up
            availableY++;
            while (availableY < 8)
            {
                if (pieceArray[availableX, availableY].color == pieceColor)  //if same team
                {
                    break;                                              //can't go past
                }

                else if (pieceArray[availableX, availableY].color == oppositeColor)   //if enemy
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
                if (pieceArray[availableX, availableY].color == pieceColor)
                {
                    break;
                }

                else if (pieceArray[availableX, availableY].color == oppositeColor)
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
                if (pieceArray[availableX, availableY].color == pieceColor)
                {
                    break;
                }

                else if (pieceArray[availableX, availableY].color == oppositeColor)
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
                if (pieceArray[availableX, availableY].color == pieceColor)
                {
                    break;
                }

                else if (pieceArray[availableX, availableY].color == oppositeColor)
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

        private List<move> knightMoves(coordinate current)
        {
            move availableMove = new move();
            int availableX = current.x;
            int availableY = current.y;
            List<move> availableList = new List<move>();
            coordinate moveCoor = new coordinate();
            string pieceColor = pieceArray[current.x, current.y].color;

            //search up.right
            availableY += 2;
            availableX++;
            if (availableY < 8 && availableX < 8)
            {
                if (pieceArray[availableX, availableY].color != pieceColor)
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
                if (pieceArray[availableX, availableY].color != pieceColor)
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
                if (pieceArray[availableX, availableY].color != pieceColor)
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
                if (pieceArray[availableX, availableY].color != pieceColor)
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
                if (pieceArray[availableX, availableY].color != pieceColor)
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
                if (pieceArray[availableX, availableY].color != pieceColor)
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
                if (pieceArray[availableX, availableY].color != pieceColor)
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
                if (pieceArray[availableX, availableY].color != pieceColor)
                {
                    moveCoor.x = availableX;
                    moveCoor.y = availableY;
                    availableMove = new move(current, moveCoor);
                    availableList.Add(availableMove);
                }
            }
            return availableList;
        }

        private List<move> bishopMoves(coordinate current)
        {
            string oppositeColor;
            move availableMove = new move();
            int availableX = current.x;
            int availableY = current.y;
            List<move> availableList = new List<move>();
            coordinate moveCoor = new coordinate();
            string pieceColor = pieceArray[current.x, current.y].color;

            if (pieceColor == "light")
            {
                oppositeColor = "dark";
            }

            else
            {
                oppositeColor = "light";
            }

            //search upper right
            availableX++;
            availableY++;
            while (availableX < 8 && availableY < 8)
            {
                if (pieceArray[availableX, availableY].color == pieceColor)
                {
                    break;
                }

                else if (pieceArray[availableX, availableY].color == oppositeColor)
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
                if (pieceArray[availableX, availableY].color == pieceColor)
                {
                    break;
                }

                else if (pieceArray[availableX, availableY].color == oppositeColor)
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
                if (pieceArray[availableX, availableY].color == pieceColor)
                {
                    break;
                }

                else if (pieceArray[availableX, availableY].color == oppositeColor)
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
                if (pieceArray[availableX, availableY].color == pieceColor)
                {
                    break;
                }

                else if (pieceArray[availableX, availableY].color == oppositeColor)
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

        private List<move> kingMoves(coordinate current)
        {
            move availableMove = new move();
            int availableX = current.x;
            int availableY = current.y;
            List<move> availableList = new List<move>();
            coordinate moveCoor = new coordinate();
            string pieceColor = pieceArray[current.x, current.y].color;

            //search up
            availableY++;
            if (availableY < 8)
            {
                if (pieceArray[availableX, availableY].color != pieceColor)
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
                if (pieceArray[availableX, availableY].color != pieceColor)
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
                if (pieceArray[availableX, availableY].color != pieceColor)
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
                if (pieceArray[availableX, availableY].color != pieceColor)
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
                if (pieceArray[availableX, availableY].color != pieceColor)
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
                if (pieceArray[availableX, availableY].color != pieceColor)
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
                if (pieceArray[availableX, availableY].color != pieceColor)
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
                if (pieceArray[availableX, availableY].color != pieceColor)
                {
                    moveCoor.x = availableX;
                    moveCoor.y = availableY;
                    availableMove = new move(current, moveCoor);
                    availableList.Add(availableMove);
                }
            }

            //search for castleing opportunity
            if (pieceArray[current.x, current.y].virgin == true)//if king's first move
            {
                if (pieceColor == "dark")
                {
                    if (pieceArray[0, 7].virgin == true)//if left rook's first move
                    {
                        //if clear path from rook to king
                        if (pieceArray[1, 7].job == null && pieceArray[2, 7].job == null && pieceArray[3, 7].job == null)
                        {
                            moveCoor.x = 2;
                            moveCoor.y = 7;
                            availableMove = new move(current, moveCoor);
                            availableList.Add(availableMove);
                        }
                    }

                    if (pieceArray[7, 7].virgin == true)//if right rook's first move
                    {
                        if (pieceArray[6, 7].job == null && pieceArray[5, 7].job == null)
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
                    if (pieceArray[0, 0].virgin == true)//if left rook's first move
                    {
                        if (pieceArray[1, 0].job == null && pieceArray[2, 0].job == null && pieceArray[3, 0].job == null)
                        {
                            moveCoor.x = 2;
                            moveCoor.y = 0;
                            availableMove = new move(current, moveCoor);
                            availableList.Add(availableMove);
                        }
                    }

                    if (pieceArray[7, 0].virgin == true)//if right rook's first move
                    {
                        if (pieceArray[6, 0].job == null && pieceArray[5, 0].job == null)
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

        private List<move> pawnMoves(coordinate current)
        {
            string oppositeColor;
            move availableMove = new move();
            int availableX = current.x;
            int availableY = current.y;
            List<move> availableList = new List<move>();
            coordinate moveCoor = new coordinate();
            string pieceColor = pieceArray[current.x, current.y].color;

            if (pieceColor == "light")
            {
                oppositeColor = "dark";
            }
            else
            {
                oppositeColor = "light";
            }

            if (pieceColor == "dark")
            {
                //search down
                availableY--;
                if (availableY >= 0)
                {
                    if (pieceArray[availableX, availableY].color == null)
                    {
                        moveCoor.x = availableX;
                        moveCoor.y = availableY;
                        availableMove = new move(current, moveCoor);
                        availableList.Add(availableMove);

                        //search first move
                        availableY--;
                        if (availableY >= 0 && pieceArray[current.x, current.y].virgin == true)
                        {
                            if (pieceArray[availableX, availableY].color == null)
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
                    if (pieceArray[availableX, availableY].color == oppositeColor)
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
                    if (pieceArray[availableX, availableY].color == oppositeColor)
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
                    if (pieceArray[availableX, availableY].color == null)
                    {
                        moveCoor.x = availableX;
                        moveCoor.y = availableY;
                        availableMove = new move(current, moveCoor);
                        availableList.Add(availableMove);

                        //search first move
                        availableY++;
                        if (availableY < 8 && pieceArray[current.x, current.y].virgin == true)
                        {
                            if (pieceArray[availableX, availableY].color == null)
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
                    if (pieceArray[availableX, availableY].color == oppositeColor)
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
                    if (pieceArray[availableX, availableY].color == oppositeColor)
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
