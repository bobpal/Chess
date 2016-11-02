using System.Collections.Generic;
using static Chess.Logic;

namespace Chess
{
    public enum Color
    {
        Light,
        Dark
    }
    public enum Job
    {
        Pawn,
        Knight,
        Bishop,
        Rook,
        Queen,
        King
    }

    public abstract class Piece
    {
        public coordinate coor { get; set; }    //spot on board
        public Color color { get; set; }        //on dark or light team?
        public Job job { get; set; }            //piece's job
        public bool virgin { get; set; }        //has piece ever moved?
        public bool dead { get; set; }          //is piece dead?

        public List<move> getCheckRestrictedMoves(Piece[] board)
        {
            //returns list of moves that don't put piece's team in check

            bool inCheck;
            bool lostV;
            int index;
            List<move> allPossible = new List<move>();
            List<move> possibleWithoutCheck = new List<move>();

            allPossible = this.getMoves(board);

            foreach (move m in allPossible)
            {
                lostV = this.virgin;
                index = getIndex(m.end, board);

                //do moves
                board[index].dead = true;
                this.coor = m.end;
                this.virgin = false;

                //see if in check
                inCheck = isInCheck(this.color, board);

                if (inCheck == false)//if not in check
                {
                    possibleWithoutCheck.Add(m);
                }

                //reset pieces
                board[index].dead = false;
                this.coor = m.start;
                this.virgin = lostV;
            }
            return possibleWithoutCheck;
        }

        public void movePiece(coordinate newCell, Logic game)
        {
            game.pieceArray[getIndex(newCell, game.pieceArray)].dead = true;
            game.displayArray[this.coor.x, this.coor.y].top.Source = null;

            this.coor = newCell;
            this.virgin = false;
            game.displayArray[newCell.x, newCell.y].top.Source = game.matchPicture(this);

            game.movablePieceSelected = false;
        }

        public abstract List<move> getMoves(Piece[] board);

        public class coordinate
        {
            //a spot on the board

            public int x { get; set; }
            public int y { get; set; }

            public coordinate(int _x, int _y)
            {
                this.x = _x;
                this.y = _y;
            }

            public coordinate() { }
        }

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

            public move()
            {

            }
        }
    }

    public class Pawn : Piece
    {
        public Pawn(coordinate _coor, Color _color)
        {
            this.coor = _coor;
            this.color = _color;
            this.job = Job.Pawn;
            this.virgin = true;
            this.dead = false;
        }

        public Pawn(coordinate _coor, Color _color, bool _virgin)
        {
            this.coor = _coor;
            this.color = _color;
            this.job = Job.Pawn;
            this.virgin = _virgin;
            this.dead = false;
        }

        public override List<move> getMoves(Piece[] board)
        {
            //does not account for check restrictions
            //returns list of possible moves for that piece

            move availableMove;
            int availableX = this.coor.x;
            int availableY = this.coor.y;
            List<move> availableList = new List<move>();
            Color oppositeColor = switchTeam(this.color);

            if (this.color == Color.Dark)
            {
                availableY--;
                //search first move
                if (this.virgin == true)
                {
                    if (getIndex(new coordinate(availableX, 5), board) == 32)
                    {
                        availableMove = new move(this.coor, new coordinate(availableX, 5));
                        availableList.Add(availableMove);

                        if (getIndex(new coordinate(availableX, 4), board) == 32)
                        {
                            availableMove = new move(this.coor, new coordinate(availableX, 4));
                            availableList.Add(availableMove);
                        }
                    }
                }

                //search down
                else if (availableY >= 0)
                {
                    if (getIndex(new coordinate(availableX, availableY), board) == 32)
                    {
                        availableMove = new move(this.coor, new coordinate(availableX, availableY));
                        availableList.Add(availableMove);
                    }
                }

                //search lower right
                availableX++;
                if (availableY >= 0 && availableX < 8)
                {
                    if (board[getIndex(new coordinate(availableX, availableY), board)].color == oppositeColor)
                    {
                        availableMove = new move(this.coor, new coordinate(availableX, availableY));
                        availableList.Add(availableMove);
                    }
                }

                //search lower left
                availableX -= 2;
                if (availableY >= 0 && availableX >= 0)
                {
                    if (board[getIndex(new coordinate(availableX, availableY), board)].color == oppositeColor)
                    {
                        availableMove = new move(this.coor, new coordinate(availableX, availableY));
                        availableList.Add(availableMove);
                    }
                }
            }

            else
            {
                availableY++;
                //search first move
                if (board[getIndex(this.coor, board)].virgin == true)
                {
                    if (getIndex(new coordinate(availableX, 2), board) == 32)
                    {
                        availableMove = new move(this.coor, new coordinate(availableX, 2));
                        availableList.Add(availableMove);

                        if (getIndex(new coordinate(availableX, 3), board) == 32)
                        {
                            availableMove = new move(this.coor, new coordinate(availableX, 3));
                            availableList.Add(availableMove);
                        }
                    }
                }

                //search up
                else if (availableY < 8)
                {
                    if (getIndex(new coordinate(availableX, availableY), board) == 32)
                    {
                        availableMove = new move(this.coor, new coordinate(availableX, availableY));
                        availableList.Add(availableMove);
                    }
                }

                //search upper right
                availableX++;
                if (availableY < 8 && availableX < 8)
                {
                    if (board[getIndex(new coordinate(availableX, availableY), board)].color == oppositeColor)
                    {
                        availableMove = new move(this.coor, new coordinate(availableX, availableY));
                        availableList.Add(availableMove);
                    }
                }

                //search upper left
                availableX -= 2;
                if (availableY < 8 && availableX >= 0)
                {
                    if (board[getIndex(new coordinate(availableX, availableY), board)].color == oppositeColor)
                    {
                        availableMove = new move(this.coor, new coordinate(availableX, availableY));
                        availableList.Add(availableMove);
                    }
                }
            }
            return availableList;
        }
    }

    public class Rook : Piece
    {
        public Rook(coordinate _coor, Color _color)
        {
            this.coor = _coor;
            this.color = _color;
            this.job = Job.Rook;
            this.virgin = true;
            this.dead = false;
        }

        public Rook(coordinate _coor, Color _color, bool _virgin)
        {
            this.coor = _coor;
            this.color = _color;
            this.job = Job.Rook;
            this.virgin = _virgin;
            this.dead = false;
        }

        public override List<move> getMoves(Piece[] board)
        {
            //does not account for check restrictions
            //returns list of possible moves for that piece

            move availableMove;
            int availableX = this.coor.x;             //put coordinate in temp variable to manipulate while preserving original
            int availableY = this.coor.y;
            List<move> availableList = new List<move>();
            Color oppositeColor = switchTeam(this.color);

            //search up
            availableY++;
            while (availableY < 8)
            {
                if (board[getIndex(new coordinate(availableX, availableY), board)].color == this.color)           //if same team
                {
                    break;                                                      //can't go past
                }

                else if (board[getIndex(new coordinate(availableX, availableY), board)].color == oppositeColor)   //if enemy
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);       //add to list
                    break;                                  //can't go past
                }

                else                                        //if unoccupied
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);       //add to list
                    availableY++;                           //try next spot
                }
            }

            //search left
            availableX = this.coor.x;
            availableY = this.coor.y;
            availableX--;
            while (availableX >= 0)
            {
                if (board[getIndex(new coordinate(availableX, availableY), board)].color == this.color)
                {
                    break;
                }

                else if (board[getIndex(new coordinate(availableX, availableY), board)].color == oppositeColor)
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);
                    break;
                }

                else
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);
                    availableX--;
                }
            }

            //search down
            availableX = this.coor.x;
            availableY = this.coor.y;
            availableY--;
            while (availableY >= 0)
            {
                if (board[getIndex(new coordinate(availableX, availableY), board)].color == this.color)
                {
                    break;
                }

                else if (board[getIndex(new coordinate(availableX, availableY), board)].color == oppositeColor)
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);
                    break;
                }

                else
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);
                    availableY--;
                }
            }

            //search right
            availableX = this.coor.x;
            availableY = this.coor.y;
            availableX++;
            while (availableX < 8)
            {
                if (board[getIndex(new coordinate(availableX, availableY), board)].color == this.color)
                {
                    break;
                }

                else if (board[getIndex(new coordinate(availableX, availableY), board)].color == oppositeColor)
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);
                    break;
                }

                else
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);
                    availableX++;
                }
            }
            return availableList;
        }
    }

    public class Knight : Piece
    {
        public Knight(coordinate _coor, Color _color)
        {
            this.coor = _coor;
            this.color = _color;
            this.job = Job.Knight;
            this.dead = false;
        }

        public override List<move> getMoves(Piece[] board)
        {
            //does not account for check restrictions
            //returns list of possible moves for that piece

            move availableMove;
            int availableX = this.coor.x;
            int availableY = this.coor.y;
            List<move> availableList = new List<move>();

            //search up.right
            availableY += 2;
            availableX++;
            if (availableY < 8 && availableX < 8)
            {
                if (board[getIndex(new coordinate(availableX, availableY), board)].color != this.color)
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);
                }
            }

            //search up.left
            availableX = this.coor.x;
            availableY = this.coor.y;
            availableY += 2;
            availableX--;
            if (availableY < 8 && availableX >= 0)
            {
                if (board[getIndex(new coordinate(availableX, availableY), board)].color != this.color)
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);
                }
            }

            //search left.up
            availableX = this.coor.x;
            availableY = this.coor.y;
            availableY++;
            availableX -= 2;
            if (availableY < 8 && availableX >= 0)
            {
                if (board[getIndex(new coordinate(availableX, availableY), board)].color != this.color)
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);
                }
            }

            //search left.down
            availableX = this.coor.x;
            availableY = this.coor.y;
            availableY--;
            availableX -= 2;
            if (availableY >= 0 && availableX >= 0)
            {
                if (board[getIndex(new coordinate(availableX, availableY), board)].color != this.color)
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);
                }
            }

            //search down.left
            availableX = this.coor.x;
            availableY = this.coor.y;
            availableY -= 2;
            availableX--;
            if (availableY >= 0 && availableX >= 0)
            {
                if (board[getIndex(new coordinate(availableX, availableY), board)].color != this.color)
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);
                }
            }

            //search down.right
            availableX = this.coor.x;
            availableY = this.coor.y;
            availableY -= 2;
            availableX++;
            if (availableY >= 0 && availableX < 8)
            {
                if (board[getIndex(new coordinate(availableX, availableY), board)].color != this.color)
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);
                }
            }

            //search right.down
            availableX = this.coor.x;
            availableY = this.coor.y;
            availableY--;
            availableX += 2;
            if (availableY >= 0 && availableX < 8)
            {
                if (board[getIndex(new coordinate(availableX, availableY), board)].color != this.color)
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);
                }
            }

            //search right.up
            availableX = this.coor.x;
            availableY = this.coor.y;
            availableY++;
            availableX += 2;
            if (availableY < 8 && availableX < 8)
            {
                if (board[getIndex(new coordinate(availableX, availableY), board)].color != this.color)
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);
                }
            }
            return availableList;
        }
    }

    public class Bishop : Piece
    {
        public Bishop(coordinate _coor, Color _color)
        {
            this.coor = _coor;
            this.color = _color;
            this.job = Job.Bishop;
            this.dead = false;
        }

        public override List<move> getMoves(Piece[] board)
        {
            //does not account for check restrictions
            //returns list of possible moves for that piece

            move availableMove;
            int availableX = this.coor.x;
            int availableY = this.coor.y;
            List<move> availableList = new List<move>();
            Color oppositeColor = switchTeam(this.color);

            //search upper right
            availableX++;
            availableY++;
            while (availableX < 8 && availableY < 8)
            {
                if (board[getIndex(new coordinate(availableX, availableY), board)].color == this.color)
                {
                    break;
                }

                else if (board[getIndex(new coordinate(availableX, availableY), board)].color == oppositeColor)
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);
                    break;
                }

                else
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);
                    availableX++;
                    availableY++;
                }
            }

            //search upper left
            availableX = this.coor.x;
            availableY = this.coor.y;
            availableX--;
            availableY++;
            while (availableX >= 0 && availableY < 8)
            {
                if (board[getIndex(new coordinate(availableX, availableY), board)].color == this.color)
                {
                    break;
                }

                else if (board[getIndex(new coordinate(availableX, availableY), board)].color == oppositeColor)
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);
                    break;
                }

                else
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);
                    availableX--;
                    availableY++;
                }
            }

            //search lower left
            availableX = this.coor.x;
            availableY = this.coor.y;
            availableX--;
            availableY--;
            while (availableX >= 0 && availableY >= 0)
            {
                if (board[getIndex(new coordinate(availableX, availableY), board)].color == this.color)
                {
                    break;
                }

                else if (board[getIndex(new coordinate(availableX, availableY), board)].color == oppositeColor)
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);
                    break;
                }

                else
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);
                    availableX--;
                    availableY--;
                }
            }

            //search lower right
            availableX = this.coor.x;
            availableY = this.coor.y;
            availableX++;
            availableY--;
            while (availableX < 8 && availableY >= 0)
            {
                if (board[getIndex(new coordinate(availableX, availableY), board)].color == this.color)
                {
                    break;
                }

                else if (board[getIndex(new coordinate(availableX, availableY), board)].color == oppositeColor)
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);
                    break;
                }

                else
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);
                    availableX++;
                    availableY--;
                }
            }
            return availableList;
        }
    }

    public class Queen : Piece
    {
        public Queen(coordinate _coor, Color _color)
        {
            this.coor = _coor;
            this.color = _color;
            this.job = Job.Queen;
            this.dead = false;
        }

        public override List<move> getMoves(Piece[] board)
        {
            //does not account for check restrictions
            //returns list of possible moves for that piece

            move availableMove;
            int availableX = this.coor.x;
            int availableY = this.coor.y;
            List<move> availableList = new List<move>();
            Color pieceColor = this.color;
            Color oppositeColor = switchTeam(pieceColor);

            //search upper right
            availableX++;
            availableY++;
            while (availableX < 8 && availableY < 8)
            {
                if (board[getIndex(new coordinate(availableX, availableY), board)].color == pieceColor)
                {
                    break;
                }

                else if (board[getIndex(new coordinate(availableX, availableY), board)].color == oppositeColor)
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);
                    break;
                }

                else
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);
                    availableX++;
                    availableY++;
                }
            }

            //search upper left
            availableX = this.coor.x;
            availableY = this.coor.y;
            availableX--;
            availableY++;
            while (availableX >= 0 && availableY < 8)
            {
                if (board[getIndex(new coordinate(availableX, availableY), board)].color == pieceColor)
                {
                    break;
                }

                else if (board[getIndex(new coordinate(availableX, availableY), board)].color == oppositeColor)
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);
                    break;
                }

                else
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);
                    availableX--;
                    availableY++;
                }
            }

            //search lower left
            availableX = this.coor.x;
            availableY = this.coor.y;
            availableX--;
            availableY--;
            while (availableX >= 0 && availableY >= 0)
            {
                if (board[getIndex(new coordinate(availableX, availableY), board)].color == pieceColor)
                {
                    break;
                }

                else if (board[getIndex(new coordinate(availableX, availableY), board)].color == oppositeColor)
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);
                    break;
                }

                else
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);
                    availableX--;
                    availableY--;
                }
            }

            //search lower right
            availableX = this.coor.x;
            availableY = this.coor.y;
            availableX++;
            availableY--;
            while (availableX < 8 && availableY >= 0)
            {
                if (board[getIndex(new coordinate(availableX, availableY), board)].color == pieceColor)
                {
                    break;
                }

                else if (board[getIndex(new coordinate(availableX, availableY), board)].color == oppositeColor)
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);
                    break;
                }

                else
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);
                    availableX++;
                    availableY--;
                }
            }

            //search up
            availableY++;
            while (availableY < 8)
            {
                if (board[getIndex(new coordinate(availableX, availableY), board)].color == pieceColor)  //if same team
                {
                    break;                                              //can't go past
                }

                else if (board[getIndex(new coordinate(availableX, availableY), board)].color == oppositeColor)   //if enemy
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);       //add to list
                    break;                                  //can't go past
                }

                else                                        //if unoccupied
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);       //add to list
                    availableY++;                           //try next spot
                }
            }

            //search left
            availableX = this.coor.x;
            availableY = this.coor.y;
            availableX--;
            while (availableX >= 0)
            {
                if (board[getIndex(new coordinate(availableX, availableY), board)].color == pieceColor)
                {
                    break;
                }

                else if (board[getIndex(new coordinate(availableX, availableY), board)].color == oppositeColor)
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);
                    break;
                }

                else
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);
                    availableX--;
                }
            }

            //search down
            availableX = this.coor.x;
            availableY = this.coor.y;
            availableY--;
            while (availableY >= 0)
            {
                if (board[getIndex(new coordinate(availableX, availableY), board)].color == pieceColor)
                {
                    break;
                }

                else if (board[getIndex(new coordinate(availableX, availableY), board)].color == oppositeColor)
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);
                    break;
                }

                else
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);
                    availableY--;
                }
            }

            //search right
            availableX = this.coor.x;
            availableY = this.coor.y;
            availableX++;
            while (availableX < 8)
            {
                if (board[getIndex(new coordinate(availableX, availableY), board)].color == pieceColor)
                {
                    break;
                }

                else if (board[getIndex(new coordinate(availableX, availableY), board)].color == oppositeColor)
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);
                    break;
                }

                else
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);
                    availableX++;
                }
            }

            return availableList;
        }
    }

    public class King : Piece
    {
        public King(coordinate _coor, Color _color)
        {
            this.coor = _coor;
            this.color = _color;
            this.job = Job.King;
            this.virgin = true;
            this.dead = false;
        }

        public King(coordinate _coor, Color _color, bool _virgin)
        {
            this.coor = _coor;
            this.color = _color;
            this.job = Job.King;
            this.virgin = _virgin;
            this.dead = false;
        }

        public override List<move> getMoves(Piece[] board)
        {
            //does not account for check restrictions
            //returns list of possible moves for that piece

            move availableMove;
            int availableX = this.coor.x;
            int availableY = this.coor.y;
            List<move> availableList = new List<move>();
            Color pieceColor = this.color;

            //search up
            availableY++;
            if (availableY < 8)
            {
                if (board[getIndex(new coordinate(availableX, availableY), board)].color != pieceColor)
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);
                }
            }

            //search upper left
            availableX = this.coor.x;
            availableY = this.coor.y;
            availableY++;
            availableX--;
            if (availableY < 8 && availableX >= 0)
            {
                if (board[getIndex(new coordinate(availableX, availableY), board)].color != pieceColor)
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);
                }
            }

            //search left
            availableX = this.coor.x;
            availableY = this.coor.y;
            availableX--;
            if (availableX >= 0)
            {
                if (board[getIndex(new coordinate(availableX, availableY), board)].color != pieceColor)
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);
                }
            }

            //search lower left
            availableX = this.coor.x;
            availableY = this.coor.y;
            availableY--;
            availableX--;
            if (availableY >= 0 && availableX >= 0)
            {
                if (board[getIndex(new coordinate(availableX, availableY), board)].color != pieceColor)
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);
                }
            }

            //search down
            availableX = this.coor.x;
            availableY = this.coor.y;
            availableY--;
            if (availableY >= 0)
            {
                if (board[getIndex(new coordinate(availableX, availableY), board)].color != pieceColor)
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);
                }
            }

            //search lower right
            availableX = this.coor.x;
            availableY = this.coor.y;
            availableY--;
            availableX++;
            if (availableY >= 0 && availableX < 8)
            {
                if (board[getIndex(new coordinate(availableX, availableY), board)].color != pieceColor)
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);
                }
            }

            //search right
            availableX = this.coor.x;
            availableY = this.coor.y;
            availableX++;
            if (availableX < 8)
            {
                if (board[getIndex(new coordinate(availableX, availableY), board)].color != pieceColor)
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);
                }
            }

            //search upper right
            availableX = this.coor.x;
            availableY = this.coor.y;
            availableY++;
            availableX++;
            if (availableY < 8 && availableX < 8)
            {
                if (board[getIndex(new coordinate(availableX, availableY), board)].color != pieceColor)
                {
                    availableMove = new move(this.coor, new coordinate(availableX, availableY));
                    availableList.Add(availableMove);
                }
            }

            //search for castling opportunity
            if (board[getIndex(new coordinate(availableX, availableY), board)].virgin == true)//if king's first move
            {
                if (pieceColor == Color.Dark)
                {
                    if (board[getIndex(new coordinate(), board)].virgin == true)//if left rook's first move
                    {
                        //if clear path from rook to king
                        if (getIndex(new coordinate(1, 7), board) == 32 &&
                            getIndex(new coordinate(2, 7), board) == 32 &&
                            getIndex(new coordinate(3, 7), board) == 32)
                        {
                            availableMove = new move(this.coor, new coordinate(2, 7));
                            availableList.Add(availableMove);
                        }
                    }

                    if (board[getIndex(new coordinate(7, 7), board)].virgin == true)//if right rook's first move
                    {
                        if (getIndex(new coordinate(6, 7), board) == 32 &&
                            getIndex(new coordinate(5, 7), board) == 32)
                        {
                            availableMove = new move(this.coor, new coordinate(6, 7));
                            availableList.Add(availableMove);
                        }
                    }
                }

                else
                {
                    if (board[getIndex(new coordinate(0, 0), board)].virgin == true)//if left rook's first move
                    {
                        if (getIndex(new coordinate(1, 0), board) == 32 &&
                            getIndex(new coordinate(2, 0), board) == 32 &&
                            getIndex(new coordinate(3, 0), board) == 32)
                        {
                            availableMove = new move(this.coor, new coordinate(2, 0));
                            availableList.Add(availableMove);
                        }
                    }

                    if (board[getIndex(new coordinate(7, 0), board)].virgin == true)//if right rook's first move
                    {
                        if (getIndex(new coordinate(6, 0), board) == 32 &&
                            getIndex(new coordinate(5, 0), board) == 32)
                        {
                            availableMove = new move(this.coor, new coordinate(6, 0));
                            availableList.Add(availableMove);
                        }
                    }
                }
            }
            return availableList;
        }
    }
}
