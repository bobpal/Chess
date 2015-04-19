using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Chess.Properties;
using Trinet.Core.IO.Ntfs;

namespace Chess
{
    [Serializable]
    public class Logic
    {
        public piece[,] pieceArray;         //8x8 array of pieces
        public display[,] displayArray;     //8x8 array of display objects
        public MainWindow mWindow;          //window with chess board on it
        public bool onePlayer;              //versus computer or human
        public string opponent;             //color of computer or 2nd player
        public string offensiveTeam;        //which side is on the offense
        public string baseOnBottom;         //which side is currently on bottom, going up
        public bool medMode;                //difficulty level
        public bool hardMode;               //difficulty level
        public bool ready;                  //blocks functionality for unwanted circumstances
        private coordinate prevSelected;    //where the cursor clicked previously
        private coordinate toCoor;          //where to.png is located
        private coordinate fromCoor;        //where from.png is located
        public List<Assembly> themeList;    //list of themes
        public int themeIndex;              //which theme is currently in use
        public int tick;                    //timestep for board rotation
        public Image lKing;
        public Image lQueen;
        public Image lBishop;
        public Image lKnight;
        public Image lRook;
        private Image lPawn;
        private Image dKing;
        public Image dQueen;
        public Image dBishop;
        public Image dKnight;
        public Image dRook;
        private Image dPawn;
        public Stack<historyNode> history = new Stack<historyNode>();   //stores all moves on a stack
        private List<move> possible = new List<move>();                 //list of all possible moves
        public bool gameOverExit = false;                               //Did player exit from game over screen?
        public bool lastMove = true;                                    //is lastMove menu option checked?
        public bool saveGame = true;                                    //Save game on exit?
        public bool rotate = true;                                      //Rotate board between turns on 2Player mode?
        public bool movablePieceSelected = false;                       //if true, the next click will move the selected piece if possible
        private string dirPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Chess";
        public string filePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Chess\\chess.sav";
        private string pwd = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static Random rnd = new Random();

        [Serializable]
        private class saveData
        {
            //contains all the information needed to save the game

            public piece[,] sBoard { get; private set; }
            public string sOffensiveTeam { get; private set; }
            public string sOpponent { get; private set; }
            public string sTheme { get; private set; }
            public bool sOnePlayer { get; private set; }
            public bool sMedMode { get; private set; }
            public bool sHardMode { get; private set; }
            public bool sLastMove { get; private set; }
            public bool sSaveGame { get; private set; }
            public bool sGameOverExit { get; private set; }
            public bool sRotate { get; private set; }

            public saveData(piece[,] p1, string p2, string p3, string p4, bool p5, bool p6, bool p7, bool p8, bool p9, bool p10, bool p11)
            {
                this.sBoard = p1;
                this.sOffensiveTeam = p2;
                this.sOpponent = p3;
                this.sTheme = p4;
                this.sOnePlayer = p5;
                this.sMedMode = p6;
                this.sHardMode = p7;
                this.sLastMove = p8;
                this.sSaveGame = p9;
                this.sGameOverExit = p10;
                this.sRotate = p11;
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

            public move(coordinate p1, coordinate p2, int p3)
            {
                this.pieceSpot = p1;
                this.moveSpot = p2;
                this.value = p3;
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
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 1:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 2:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 3:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 4:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 5:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 6:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 7:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                            }
                            break;
                        case 1:
                            switch (x)
                            {
                                case 0:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 1:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 2:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 3:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 4:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 5:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 6:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 7:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                            }
                            break;
                        case 2:
                            switch (x)
                            {
                                case 0:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 1:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 2:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 3:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 4:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 5:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 6:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 7:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                            }
                            break;
                        case 3:
                            switch (x)
                            {
                                case 0:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 1:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 2:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 3:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 4:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 5:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 6:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 7:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                            }
                            break;
                        case 4:
                            switch (x)
                            {
                                case 0:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 1:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 2:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 3:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 4:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 5:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 6:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 7:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                            }
                            break;
                        case 5:
                            switch (x)
                            {
                                case 0:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 1:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 2:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 3:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 4:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 5:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 6:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 7:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                            }
                            break;
                        case 6:
                            switch (x)
                            {
                                case 0:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 1:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 2:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 3:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 4:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 5:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 6:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 7:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                            }
                            break;
                        case 7:
                            switch (x)
                            {
                                case 0:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 1:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 2:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 3:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 4:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 5:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 6:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
                                    break;
                                case 7:
                                    displayArray[x, y] = new display(mWindow.zero_zero, mWindow.bottom_zero_zero, mWindow.top_zero_zero);
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

            string defensiveTeam;
            pieceArray = new piece[8, 8];

            if (offensiveTeam == "light")
            {
                defensiveTeam = "dark";
            }
            else
            {
                defensiveTeam = "light";
            }

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    if (y == 0)
                    {
                        pieceArray[x, 0].color = offensiveTeam;
                    }

                    else if (y == 1)
                    {
                        pieceArray[x, 1].color = offensiveTeam;
                        pieceArray[x, 1].job = "Pawn";
                        pieceArray[x, 1].virgin = true;
                        displayArray[x, 1].top = matchPicture(pieceArray[x, 1]);
                    }

                    else if (y == 6)
                    {
                        pieceArray[x, 6].color = defensiveTeam;
                        pieceArray[x, 6].job = "Pawn";
                        pieceArray[x, 6].virgin = true;
                        displayArray[x, 6].top = matchPicture(pieceArray[x, 6]);
                    }

                    else if (y == 7)
                    {
                        pieceArray[x, 7].color = defensiveTeam;
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

            displayArray[0, 0].top = matchPicture(pieceArray[0, 0]);
            displayArray[1, 0].top = matchPicture(pieceArray[1, 0]);
            displayArray[2, 0].top = matchPicture(pieceArray[2, 0]);
            displayArray[3, 0].top = matchPicture(pieceArray[3, 0]);
            displayArray[4, 0].top = matchPicture(pieceArray[4, 0]);
            displayArray[5, 0].top = matchPicture(pieceArray[5, 0]);
            displayArray[6, 0].top = matchPicture(pieceArray[6, 0]);
            displayArray[7, 0].top = matchPicture(pieceArray[7, 0]);
            displayArray[0, 7].top = matchPicture(pieceArray[0, 7]);
            displayArray[1, 7].top = matchPicture(pieceArray[1, 7]);
            displayArray[2, 7].top = matchPicture(pieceArray[2, 7]);
            displayArray[3, 7].top = matchPicture(pieceArray[3, 7]);
            displayArray[4, 7].top = matchPicture(pieceArray[4, 7]);
            displayArray[5, 7].top = matchPicture(pieceArray[5, 7]);
            displayArray[6, 7].top = matchPicture(pieceArray[6, 7]);
            displayArray[7, 7].top = matchPicture(pieceArray[7, 7]);
        }

        private List<coordinate> getDarkPieces()
        {
            //searches through board and returns list of coordinates where all dark pieces are located

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
            //searches through board and returns list of coordinates where all light pieces are located

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
            //searches through board and returns list of coordinates where all pieces are located

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

                    switch (pieceArray[humanMoves[j].pieceSpot.x, humanMoves[j].pieceSpot.y].job)    //what piece does the capturing
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
                    if (humanMoves[j].value != humanMoves[0].value)    //find all moves with highest value
                    {
                        break;
                    }
                    bestMovesList.Add(humanMoves[j]);  //add them to list
                }
                pos[i].value -= bestMovesList[rnd.Next(0, bestMovesList.Count)].value; //score of computer move - human reaction move

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
                if (pieceArray[m.moveSpot.x, m.moveSpot.y].job == "King" && pieceArray[m.moveSpot.x, m.moveSpot.y].color == teamInQuestion)
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

            if (offensiveTeam == baseOnBottom)
            {
                yCoor = 0;
            }
            else
            {
                yCoor = 7;
            }

            if (fromSpot.x == 4 && fromSpot.y == yCoor)  //if moving from King default position
            {
                if (toSpot.x == 2 && toSpot.y == yCoor)  //if moving two spaces to the left
                {
                    coordinate newCastleCoor = new coordinate(3, yCoor);
                    coordinate oldCastleCoor = new coordinate(0, yCoor);
                    castleMove.moveSpot = newCastleCoor;
                    castleMove.pieceSpot = oldCastleCoor;
                    node = new historyNode(castleMove, pieceArray[3, yCoor], false, true, true);
                    history.Push(node);
                    movePiece(newCastleCoor, pieceArray[0, yCoor], oldCastleCoor);
                }

                else if (toSpot.x == 6 && toSpot.y == yCoor) //if moving two spaces to the right
                {
                    coordinate newCastleCoor = new coordinate(5, yCoor);
                    coordinate oldCastleCoor = new coordinate(7, yCoor);
                    castleMove.moveSpot = newCastleCoor;
                    castleMove.pieceSpot = oldCastleCoor;
                    node = new historyNode(castleMove, pieceArray[5, yCoor], false, true, true);
                    history.Push(node);
                    movePiece(newCastleCoor, pieceArray[7, yCoor], oldCastleCoor);
                }
            }
        }

        public void clicker(coordinate currentCell)
        {
            //human player's turn, gets called when player clicks on spot

            if (ready == true)  //blocks functionality if game hasn't started yet
            {
                piece currentPiece = pieceArray[currentCell.x, currentCell.y];

                if (displayArray[currentCell.x, currentCell.y].tile.Background == Brushes.DeepSkyBlue)//if selected same piece
                {
                    movablePieceSelected = false;
                    clearSelectedAndPossible();
                }

                else if (currentPiece.color == offensiveTeam)//if selected own piece
                {
                    string defensiveTeam;
                    movablePieceSelected = true;
                    clearSelectedAndPossible();
                    displayArray[currentCell.x, currentCell.y].tile.Background = Brushes.DeepSkyBlue;
                    prevSelected = currentCell;
                    possible.Clear();
                    possible.AddRange(getCheckRestrictedMoves(currentCell));

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

                else if (movablePieceSelected == true)//if previously selected own piece
                {
                    move curTurn = new move();
                    bool movableSpot = false;

                    foreach (move m in possible)
                    {
                        if (currentCell.Equals(m.moveSpot))//if selected spot is in possible move list
                        {
                            movableSpot = true;
                            curTurn = m;
                        }
                    }

                    if (movableSpot == true)
                    {
                        historyNode node;
                        piece captured = pieceArray[currentCell.x, currentCell.y];
                        bool virginMove = pieceArray[prevSelected.x, prevSelected.y].virgin;
                        movePiece(currentCell, pieceArray[prevSelected.x, prevSelected.y], prevSelected);
                        clearSelectedAndPossible();

                        if (pieceArray[currentCell.x, currentCell.y].job == "Pawn")
                        {
                            string baseOnTop;

                            if (baseOnBottom == "light")
                            {
                                baseOnTop = "dark";
                            }
                            else
                            {
                                baseOnTop = "light";
                            }

                            if (pieceArray[currentCell.x, currentCell.y].color == baseOnBottom && currentCell.y == 7)
                            {
                                PawnTransformation transform = new PawnTransformation(currentCell, this);
                                transform.ShowDialog();
                                node = new historyNode(curTurn, captured, true, false, false);
                            }

                            else if (pieceArray[currentCell.x, currentCell.y].color == baseOnTop && currentCell.y == 0)
                            {
                                PawnTransformation transform = new PawnTransformation(currentCell, this);
                                transform.ShowDialog();
                                node = new historyNode(curTurn, captured, true, false, false);
                            }

                            else    //if pawn, but no transform
                            {
                                node = new historyNode(curTurn, captured, false, false, virginMove);
                            }
                        }

                        else    //not pawn
                        {
                            node = new historyNode(curTurn, captured, false, false, virginMove);
                        }

                        history.Push(node);
                        mWindow.undoMenu.IsEnabled = true;

                        if (lastMove == true)
                        {
                            clearToAndFrom();
                            displayArray[curTurn.pieceSpot.x, curTurn.pieceSpot.y].bottom.Source = Resources.from;
                            coordinateToDisplay(curTurn.pieceSpot).BackgroundImage = Resources.from;
                            coordinateToDisplay(curTurn.moveSpot).BackgroundImage = Resources.to;
                            toCoor = curTurn.moveSpot;
                            fromCoor = curTurn.pieceSpot;
                        }

                        if (pieceArray[currentCell.x, currentCell.y].job == "King")
                        {
                            castling(curTurn);//check if move is a castling
                        }
                        betweenTurns();
                    }
                    else    //if didn't select movable spot
                    {
                        clearSelectedAndPossible();
                        movablePieceSelected = false;
                    }
                }
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
                    isInCheckmate("light", getLightPieces()); //did computer turn put player in checkmate?
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
                    isInCheckmate("dark", getDarkPieces()); //did computer turn put player in checkmate?
                    offensiveTeam = "dark";
                }
            }

            if (onePlayer == false && rotate == true)    //rotate
            {
                rotateBoard();
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
            movePiece(newSpot, pieceArray[oldSpot.x, oldSpot.y], oldSpot);

            if (pieceArray[newSpot.x, newSpot.y].job == "Pawn" && newSpot.y == 0)//if pawn makes it to last row
            {
                r = rnd.Next(0, 4); //choose random piece to transform into

                if (opponent == "dark")
                {
                    switch (r)
                    {
                        case 0:
                            pieceArray[newSpot.x, newSpot.y].job = "Queen";
                            coordinateToDisplay(newSpot).Image = dQueen;
                            break;
                        case 1:
                            pieceArray[newSpot.x, newSpot.y].job = "Rook";
                            coordinateToDisplay(newSpot).Image = dRook;
                            break;
                        case 2:
                            pieceArray[newSpot.x, newSpot.y].job = "Bishop";
                            coordinateToDisplay(newSpot).Image = dBishop;
                            break;
                        case 3:
                            pieceArray[newSpot.x, newSpot.y].job = "Knight";
                            coordinateToDisplay(newSpot).Image = dKnight;
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
                            coordinateToDisplay(newSpot).Image = lQueen;
                            break;
                        case 1:
                            pieceArray[newSpot.x, newSpot.y].job = "Rook";
                            coordinateToDisplay(newSpot).Image = lRook;
                            break;
                        case 2:
                            pieceArray[newSpot.x, newSpot.y].job = "Bishop";
                            coordinateToDisplay(newSpot).Image = lBishop;
                            break;
                        case 3:
                            pieceArray[newSpot.x, newSpot.y].job = "Knight";
                            coordinateToDisplay(newSpot).Image = lKnight;
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

            history.Push(node);

            if (lastMove == true)
            {
                clearToAndFrom();
                coordinateToDisplay(bestMove.pieceSpot).BackgroundImage = Resources.from;
                coordinateToDisplay(bestMove.moveSpot).BackgroundImage = Resources.to;
                toCoor = bestMove.moveSpot;
                fromCoor = bestMove.pieceSpot;
            }

            if (pieceArray[newSpot.x, newSpot.y].job == "King")
            {
                castling(bestMove);//check if move is a castling
            }
        }

        private void rotateBoard()
        {
            //performs rotate animation

            if (ready == true)
            {
                ready = false;
                tick = 0;
                clearToAndFrom();
                mainForm.timer.Start();
                while (tick < 42)
                {
                    Application.DoEvents();
                }

                mainForm.timer.Stop();
                rotatePieces();
                rotateToAndFrom();

                if (baseOnBottom == "light")
                {
                    baseOnBottom = "dark";
                }
                else
                {
                    baseOnBottom = "light";
                }
                ready = true;
            }
        }

        private void rotatePieces()
        {
            //rotates pieces in array, not pictures

            piece[,] bufferBoard = new piece[8, 8];
            int newX;
            int newY;

            foreach (coordinate piece in getAllPieces())
            {
                newX = 7 - piece.x;
                newY = 7 - piece.y;
                bufferBoard[newX, newY] = pieceArray[piece.x, piece.y];
            }
            pieceArray = bufferBoard;
        }

        private void rotateToAndFrom()
        {
            //rotate last move indicators

            coordinate temp;
            temp = new coordinate(7 - toCoor.x, 7 - toCoor.y);
            displayArray[temp.x, temp.y].tile.Background = Resources.to;
            coordinateToDisplay(temp).BackgroundImage = Resources.to;
            toCoor = temp;
            temp = new coordinate(7 - fromCoor.x, 7 - fromCoor.y);
            coordinateToDisplay(temp).BackgroundImage = Resources.from;
            fromCoor = temp;
        }

        public void rotateRing(int min)
        {
            //takes one ring and rotates all images

            int max = 7 - min;
            string direction;
            Image next = displayArray[min, min].top;    //first image moved

            direction = "right";
            for (int x = min; x < max; x++)
            {
                //takes previous image and puts it in next spot while returning next image
                next = rotateImage(x, min, direction, next);
            }

            direction = "up";
            for (int y = min; y < max; y++)
            {
                next = rotateImage(max, y, direction, next);
            }

            direction = "left";
            for (int x = max; x > min; x--)
            {
                next = rotateImage(x, max, direction, next);
            }

            direction = "down";
            for (int y = max; y > min; y--)
            {
                next = rotateImage(min, y, direction, next);
            }
        }

        private Image rotateImage(int fromX, int fromY, string dir, Image overwrite)
        {
            //moves one image to next cell for rotate animation

            Image replace;
            coordinate toCoor;
            coordinate fromCoor = new coordinate(fromX, fromY);

            if (dir == "down")
            {
                toCoor = new coordinate(fromX, fromY - 1);
            }
            else if (dir == "right")
            {
                toCoor = new coordinate(fromX + 1, fromY);
            }
            else if (dir == "up")
            {
                toCoor = new coordinate(fromX, fromY + 1);
            }
            else//left
            {
                toCoor = new coordinate(fromX - 1, fromY);
            }

            replace = displayArray[toCoor.x, toCoor.y].top;
            displayArray[toCoor.x, toCoor.y].top = overwrite;
            return replace;
        }

        private void movePiece(coordinate newCell, piece pPiece, coordinate oldCell)
        {
            //does standard piece move

            //overwrite current cell
            pieceArray[newCell.x, newCell.y].color = pPiece.color;
            pieceArray[newCell.x, newCell.y].job = pPiece.job;

            //delete prev cell
            pieceArray[oldCell.x, oldCell.y].color = null;
            pieceArray[oldCell.x, oldCell.y].job = null;

            //overwrite current image
            //take previous piece picture and put it in current cell picture box
            displayArray[newCell.x, newCell.y].top = matchPicture(pPiece);

            //delete prev image
            displayArray[oldCell.x, oldCell.y].top = null;

            movablePieceSelected = false;
            pieceArray[newCell.x, newCell.y].virgin = false;
        }

        public void undo()
        {
            //completely undo previous move

            Image pawnPic;
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

            if (rotate == true && onePlayer == false)
            {
                rotateBoard();
            }

            to = pieceArray[xMove, yMove];
            from = pieceArray[xPiece, yPiece];
            offensiveTeam = to.color;

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
                displayArray[xPiece, yPiece].top = pawnPic;
            }

            else
            {
                pieceArray[xPiece, yPiece].job = to.job;
                displayArray[xPiece, yPiece].top = matchPicture(to);
            }

            pieceArray[xPiece, yPiece].color = to.color;
            pieceArray[xPiece, yPiece].virgin = node.firstMove;

            //put captured piece back
            pieceArray[xMove, yMove].job = node.captured.job;
            displayArray[xMove, yMove].top = matchPicture(node.captured);
            pieceArray[xMove, yMove].color = node.captured.color;
            pieceArray[xMove, yMove].virgin = node.captured.virgin;


            if (node.skip == true)
            {
                undo(); //call function again to undo another move
            }

            else if (history.Count == 0)    //if stack is empty, disable button; skip and empty stack can't both happen
            {
                mWindow.undoMenu.IsEnabled = false;
            }
            clearToAndFrom();
            clearSelectedAndPossible();
        }

        private Image matchPicture(piece figure)
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
                    if(sum % 2 == 0)    //if even number
                    {
                        displayArray[x, y].tile.Background = Brushes.Black;
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

            displayArray[toCoor.x, toCoor.y].bottom = null;
            displayArray[fromCoor.x, fromCoor.y].bottom = null;
        }

        public void changeThemeVisually()
        {
            //calls matchPicture() on each piece and puts image in PictureBox

            foreach (coordinate spot in getAllPieces())
            {
                displayArray[spot.x, spot.y].top = matchPicture(pieceArray[spot.x, spot.y]);
            }
        }

        public void changeThemeInternally()
        {
            //sets image variables based on themeIndex

            string themeName = themeList[themeIndex].GetName().Name;

            Stream lKingFile = themeList[themeIndex].GetManifestResourceStream(themeName + ".lKing.png");
            Stream lQueenFile = themeList[themeIndex].GetManifestResourceStream(themeName + ".lQueen.png");
            Stream lBishopFile = themeList[themeIndex].GetManifestResourceStream(themeName + ".lBishop.png");
            Stream lKnightFile = themeList[themeIndex].GetManifestResourceStream(themeName + ".lKnight.png");
            Stream lRookFile = themeList[themeIndex].GetManifestResourceStream(themeName + ".lRook.png");
            Stream lPawnFile = themeList[themeIndex].GetManifestResourceStream(themeName + ".lPawn.png");
            Stream dKingFile = themeList[themeIndex].GetManifestResourceStream(themeName + ".dKing.png");
            Stream dQueenFile = themeList[themeIndex].GetManifestResourceStream(themeName + ".dQueen.png");
            Stream dBishopFile = themeList[themeIndex].GetManifestResourceStream(themeName + ".dBishop.png");
            Stream dKnightFile = themeList[themeIndex].GetManifestResourceStream(themeName + ".dKnight.png");
            Stream dRookFile = themeList[themeIndex].GetManifestResourceStream(themeName + ".dRook.png");
            Stream dPawnFile = themeList[themeIndex].GetManifestResourceStream(themeName + ".dPawn.png");

            try
            {
                lKing = Image.FromStream(lKingFile);
                lQueen = Image.FromStream(lQueenFile);
                lBishop = Image.FromStream(lBishopFile);
                lKnight = Image.FromStream(lKnightFile);
                lRook = Image.FromStream(lRookFile);
                lPawn = Image.FromStream(lPawnFile);
                dKing = Image.FromStream(dKingFile);
                dQueen = Image.FromStream(dQueenFile);
                dBishop = Image.FromStream(dBishopFile);
                dKnight = Image.FromStream(dKnightFile);
                dRook = Image.FromStream(dRookFile);
                dPawn = Image.FromStream(dPawnFile);
            }
            catch (ArgumentException)
            {
                themeList.RemoveAt(themeIndex);
            }
        }

        public void tryDlls()
        {
            //calls loadDlls() and setTheme() till found all dlls

            bool dllsFound = false;
            int originalSize;
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

            while (dllsFound == false)
            {
                loadDlls();
                originalSize = themeList.Count;

                for (int i = 0; i < originalSize; i++)
                {
                    themeIndex = originalSize - i - 1;
                    changeThemeInternally();
                }

                if (themeList.Count < 1)
                {
                    result = MessageBox.Show(
                        "No themes found.\n\nPlace theme dll in directory containing 'Chess.exe'\n\nTry Again?",
                        "Missing Dll", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);

                    if(result == MessageBoxResult.No)
                    {
                        Environment.Exit(1);
                    }
                }

                else
                {
                    dllsFound = true;
                }
            }
            //find default theme
            themeIndex = themeList.FindIndex(x => x.GetName().Name == "Figure");

            if (themeIndex == -1)    //if can't find default
            {
                themeIndex = 0;
            }
            changeThemeInternally();
        }

        private void loadDlls()
        {
            //searches dlls in working directory and loads themes

            AssemblyName an;
            Assembly assembly;
            themeList = new List<Assembly>();
            string[] dllFilePathArray = null;
            List<string> ignore = new List<string>();
            bool themesFound = false;

            while (themesFound == false)
            {
                try
                {
                    dllFilePathArray = Directory.GetFiles(pwd, "*.dll");

                    foreach (string dllFilePath in dllFilePathArray.Except(ignore))
                    {
                        FileInfo file = new FileInfo(dllFilePath);
                        file.DeleteAlternateDataStream("Zone.Identifier");
                        an = AssemblyName.GetAssemblyName(dllFilePath);
                        assembly = Assembly.Load(an);

                        if (!themeList.Contains(assembly))
                        {
                            themeList.Add(assembly);
                        }
                    }
                }
                catch (BadImageFormatException ex)
                {
                    ignore.Add(pwd + "\\" + ex.FileName);
                    themeList.Clear();
                }

                if (themeList.Count < 1)    //if 0 themes or 1 bad file before added to ignore
                {
                    //if 0 bad files <OR> bad files = total dll files
                    if (ignore.Count < 1 || ignore.Count == dllFilePathArray.Count())
                    {
                        MessageBoxResult result = MessageBox.Show(
                        "No themes found.\n\nPlace theme dll in directory containing 'Chess.exe'\n\nTry Again?",
                        "Missing Dll", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);

                        if (result == MessageBoxResult.No)
                        {
                            Environment.Exit(1);
                        }
                        ignore.Clear();
                    }
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
            //if save game unchecked, still saves, but takes note not to load current game next time

            if (ready == true)   //if a game is being or has been played
            {
                string theme = themeList[themeIndex].GetName().Name;
                saveData sData = new saveData(pieceArray, offensiveTeam, opponent, theme, onePlayer, medMode, hardMode,
                    lastMove, saveGame, gameOverExit, rotate);

                System.IO.Directory.CreateDirectory(dirPath);
                BinaryFormatter writer = new BinaryFormatter();
                FileStream saveStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                writer.Serialize(saveStream, sData);
                saveStream.Close();
            }
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
                mWindow.lastMoveMenu.IsChecked = lastMove;
                rotate = lData.sRotate;
                mWindow.rotateMenu.IsChecked = rotate;
                string theme = lData.sTheme;

                if (lData.sSaveGame == true)
                {
                    if (lData.sGameOverExit == false)
                    {
                        pieceArray = new piece[8, 8];
                        ready = true;
                        movablePieceSelected = false;
                        pieceArray = lData.sBoard;
                        offensiveTeam = lData.sOffensiveTeam;
                        opponent = lData.sOpponent;
                        onePlayer = lData.sOnePlayer;
                        medMode = lData.sMedMode;
                        hardMode = lData.sHardMode;
                        string goodTeam;

                        if (opponent == "light")
                        {
                            goodTeam = "dark";
                        }
                        else
                        {
                            goodTeam = "light";
                        }

                        if (onePlayer == true)
                        {
                            baseOnBottom = offensiveTeam;
                            mWindow.rotateMenu.IsEnabled = false;
                        }
                        else
                        {
                            if (rotate == true)
                            {
                                baseOnBottom = offensiveTeam;
                            }
                            else
                            {
                                baseOnBottom = goodTeam;
                            }
                            mWindow.rotateMenu.IsEnabled = true;
                        }
                    }
                    else    //Exit on Game Over
                    {
                        newGame();
                    }
                }
                else    //Exit with saveGame set to false
                {
                    saveGame = false;
                    mWindow.saveMenu.IsChecked = false;
                    newGame();
                }

                for (int i = 0; i < themeList.Count(); i++)
                {
                    if (themeList[i].GetName().Name == theme)
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

        public void rotateMenuOption()
        {
            //when rotate option is toggled

            if (onePlayer == false)
            {
                if (offensiveTeam == opponent)  //if opponent's turn
                {
                    clearSelectedAndPossible();
                    rotateBoard();
                    clearToAndFrom();
                }
            }
            rotate = mWindow.rotateMenu.IsChecked;
        }

        public void newGame()
        {
            //call newGame window

            NewGame play = new NewGame(this, mainForm);
            play.ShowDialog();
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
                    availableMove = new move(current, moveCoor, 0);
                    availableList.Add(availableMove);       //add to list
                    break;                                  //can't go past
                }

                else                                        //if unoccupied
                {
                    moveCoor.x = availableX;
                    moveCoor.y = availableY;
                    availableMove = new move(current, moveCoor, 0);
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
                    availableMove = new move(current, moveCoor, 0);
                    availableList.Add(availableMove);
                    break;
                }

                else
                {
                    moveCoor.x = availableX;
                    moveCoor.y = availableY;
                    availableMove = new move(current, moveCoor, 0);
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
                    availableMove = new move(current, moveCoor, 0);
                    availableList.Add(availableMove);
                    break;
                }

                else
                {
                    moveCoor.x = availableX;
                    moveCoor.y = availableY;
                    availableMove = new move(current, moveCoor, 0);
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
                    availableMove = new move(current, moveCoor, 0);
                    availableList.Add(availableMove);
                    break;
                }

                else
                {
                    moveCoor.x = availableX;
                    moveCoor.y = availableY;
                    availableMove = new move(current, moveCoor, 0);
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
                    availableMove = new move(current, moveCoor, 0);
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
                    availableMove = new move(current, moveCoor, 0);
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
                    availableMove = new move(current, moveCoor, 0);
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
                    availableMove = new move(current, moveCoor, 0);
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
                    availableMove = new move(current, moveCoor, 0);
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
                    availableMove = new move(current, moveCoor, 0);
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
                    availableMove = new move(current, moveCoor, 0);
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
                    availableMove = new move(current, moveCoor, 0);
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
                    availableMove = new move(current, moveCoor, 0);
                    availableList.Add(availableMove);
                    break;
                }

                else
                {
                    moveCoor.x = availableX;
                    moveCoor.y = availableY;
                    availableMove = new move(current, moveCoor, 0);
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
                    availableMove = new move(current, moveCoor, 0);
                    availableList.Add(availableMove);
                    break;
                }

                else
                {
                    moveCoor.x = availableX;
                    moveCoor.y = availableY;
                    availableMove = new move(current, moveCoor, 0);
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
                    availableMove = new move(current, moveCoor, 0);
                    availableList.Add(availableMove);
                    break;
                }

                else
                {
                    moveCoor.x = availableX;
                    moveCoor.y = availableY;
                    availableMove = new move(current, moveCoor, 0);
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
                    availableMove = new move(current, moveCoor, 0);
                    availableList.Add(availableMove);
                    break;
                }

                else
                {
                    moveCoor.x = availableX;
                    moveCoor.y = availableY;
                    availableMove = new move(current, moveCoor, 0);
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
                    availableMove = new move(current, moveCoor, 0);
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
                    availableMove = new move(current, moveCoor, 0);
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
                    availableMove = new move(current, moveCoor, 0);
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
                    availableMove = new move(current, moveCoor, 0);
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
                    availableMove = new move(current, moveCoor, 0);
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
                    availableMove = new move(current, moveCoor, 0);
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
                    availableMove = new move(current, moveCoor, 0);
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
                    availableMove = new move(current, moveCoor, 0);
                    availableList.Add(availableMove);
                }
            }

            //search for castleing opportunity
            if (pieceArray[current.x, current.y].virgin == true)//if king's first move
            {
                if (pieceColor == baseOnBottom)
                {
                    if (pieceArray[0, 0].virgin == true)//if left rook's first move
                    {
                        if (pieceArray[1, 0].job == null && pieceArray[2, 0].job == null && pieceArray[3, 0].job == null)
                        {
                            moveCoor.x = 2;
                            moveCoor.y = 0;
                            availableMove = new move(current, moveCoor, 0);
                            availableList.Add(availableMove);
                        }
                    }

                    if (pieceArray[7, 0].virgin == true)//if right rook's first move
                    {
                        if (pieceArray[6, 0].job == null && pieceArray[5, 0].job == null)
                        {
                            moveCoor.x = 6;
                            moveCoor.y = 0;
                            availableMove = new move(current, moveCoor, 0);
                            availableList.Add(availableMove);
                        }
                    }
                }

                else
                {
                    if (pieceArray[0, 7].virgin == true)//if left rook's first move
                    {
                        //if clear path from rook to king
                        if (pieceArray[1, 7].job == null && pieceArray[2, 7].job == null && pieceArray[3, 7].job == null)
                        {
                            moveCoor.x = 2;
                            moveCoor.y = 7;
                            availableMove = new move(current, moveCoor, 0);
                            availableList.Add(availableMove);
                        }
                    }

                    if (pieceArray[7, 7].virgin == true)//if right rook's first move
                    {
                        if (pieceArray[6, 7].job == null && pieceArray[5, 7].job == null)
                        {
                            moveCoor.x = 6;
                            moveCoor.y = 7;
                            availableMove = new move(current, moveCoor, 0);
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

            if (pieceColor == baseOnBottom)
            {
                //search up
                availableY++;
                if (availableY < 8)
                {
                    if (pieceArray[availableX, availableY].color == null)
                    {
                        moveCoor.x = availableX;
                        moveCoor.y = availableY;
                        availableMove = new move(current, moveCoor, 0);
                        availableList.Add(availableMove);

                        //search first move
                        availableY++;
                        if (availableY < 8 && pieceArray[current.x, current.y].virgin == true)
                        {
                            if (pieceArray[availableX, availableY].color == null)
                            {
                                moveCoor.x = availableX;
                                moveCoor.y = availableY;
                                availableMove = new move(current, moveCoor, 0);
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
                        availableMove = new move(current, moveCoor, 0);
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
                        availableMove = new move(current, moveCoor, 0);
                        availableList.Add(availableMove);
                    }
                }
            }

            else
            {
                //search down
                availableY--;
                if (availableY >= 0)
                {
                    if (pieceArray[availableX, availableY].color == null)
                    {
                        moveCoor.x = availableX;
                        moveCoor.y = availableY;
                        availableMove = new move(current, moveCoor, 0);
                        availableList.Add(availableMove);

                        //search first move
                        availableY--;
                        if (availableY >= 0 && pieceArray[current.x, current.y].virgin == true)
                        {
                            if (pieceArray[availableX, availableY].color == null)
                            {
                                moveCoor.x = availableX;
                                moveCoor.y = availableY;
                                availableMove = new move(current, moveCoor, 0);
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
                        availableMove = new move(current, moveCoor, 0);
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
                        availableMove = new move(current, moveCoor, 0);
                        availableList.Add(availableMove);
                    }
                }
            }
            return availableList;
        }
    }
}
