using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Trinet.Core.IO.Ntfs;

namespace Chess
{
    [Serializable]
    public class Logic
    {
        public piece[,] board;              //8x8 array of pieces
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

        public Logic(MainWindow mw)
        {
            this.mWindow = mw;
        }
    }
}
