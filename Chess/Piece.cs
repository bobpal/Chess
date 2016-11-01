using System;
using System.Collections.Generic;
using static Chess.Logic;

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

    public abstract class Piece
    {
        public coordinate coor { get; set; }    //spot on board
        public Color color { get; set; }        //on dark or light team?
        public Job job { get; set; }            //piece's job
        public bool virgin { get; set; }        //has piece ever moved?

        public List<move> getCheckRestrictedMoves(Piece[,] grid)
        {
            //returns list of moves that don't put piece's team in check

            bool inCheck;

            List<move> allPossible = new List<move>();
            List<move> possibleWithoutCheck = new List<move>();

            allPossible = this.getMoves(grid);

            foreach (move m in allPossible)
            {
                //do moves
                grid[m.end.coor.x, m.end.coor.y] = m.start;
                grid[m.end.coor.x, m.end.coor.y].coor = m.end.coor;
                grid[m.end.coor.x, m.end.coor.y].virgin = false;
                grid[m.start.coor.x, m.start.coor.y] = new Empty(m.start.coor);

                //see if in check
                inCheck = isInCheck(this.color, grid);

                if (inCheck == false)//if not in check
                {
                    possibleWithoutCheck.Add(m);
                }

                //reset pieces
                grid[m.start.coor.x, m.start.coor.y] = m.start;
                grid[m.end.coor.x, m.end.coor.y] = m.end;
            }
            return possibleWithoutCheck;
        }

        public abstract List<move> getMoves(Piece[,] grid);

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

            public Piece start { get; set; }   //starting position
            public Piece end { get; set; }    //ending position

            public move(Piece _start, Piece _end)
            {
                this.start = _start;
                this.end = _end;
            }

            public move() { }
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
        }

        public Pawn(coordinate _coor, Color _color, bool _virgin)
        {
            this.coor = _coor;
            this.color = _color;
            this.job = Job.Pawn;
            this.virgin = _virgin;
        }

        public override List<move> getMoves(Piece[,] grid)
        {
            //does not account for check restrictions
            //returns list of possible moves for that piece

            move availableMove = new move();
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
                    if (grid[availableX, 5].color == Color.None)
                    {
                        availableMove = new move(this, grid[availableX, 5]);
                        availableList.Add(availableMove);

                        if (grid[availableX, 4].color == Color.None)
                        {
                            availableMove = new move(this, grid[availableX, 4]);
                            availableList.Add(availableMove);
                        }
                    }
                }

                //search down
                else if (availableY >= 0)
                {
                    if (grid[availableX, availableY].color == Color.None)
                    {
                        availableMove = new move(this, grid[availableX, availableY]);
                        availableList.Add(availableMove);
                    }
                }

                //search lower right
                availableX++;
                if (availableY >= 0 && availableX < 8)
                {
                    if (grid[availableX, availableY].color == oppositeColor)
                    {
                        availableMove = new move(this, grid[availableX, availableY]);
                        availableList.Add(availableMove);
                    }
                }

                //search lower left
                availableX -= 2;
                if (availableY >= 0 && availableX >= 0)
                {
                    if (grid[availableX, availableY].color == oppositeColor)
                    {
                        availableMove = new move(this, grid[availableX, availableY]);
                        availableList.Add(availableMove);
                    }
                }
            }

            else
            {
                availableY++;
                //search first move
                if (grid[this.coor.x, this.coor.y].virgin == true)
                {
                    if (grid[availableX, 2].color == Color.None)
                    {
                        availableMove = new move(this, grid[availableX, 2]);
                        availableList.Add(availableMove);

                        if (grid[availableX, 3].color == Color.None)
                        {
                            availableMove = new move(this, grid[availableX, 3]);
                            availableList.Add(availableMove);
                        }
                    }
                }

                //search up
                else if (availableY < 8)
                {
                    if (grid[availableX, availableY].color == Color.None)
                    {
                        availableMove = new move(this, grid[availableX, availableY]);
                        availableList.Add(availableMove);
                    }
                }

                //search upper right
                availableX++;
                if (availableY < 8 && availableX < 8)
                {
                    if (grid[availableX, availableY].color == oppositeColor)
                    {
                        availableMove = new move(this, grid[availableX, availableY]);
                        availableList.Add(availableMove);
                    }
                }

                //search upper left
                availableX -= 2;
                if (availableY < 8 && availableX >= 0)
                {
                    if (grid[availableX, availableY].color == oppositeColor)
                    {
                        availableMove = new move(this, grid[availableX, availableY]);
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
        }

        public Rook(coordinate _coor, Color _color, bool _virgin)
        {
            this.coor = _coor;
            this.color = _color;
            this.job = Job.Rook;
            this.virgin = _virgin;
        }

        public override List<move> getMoves(Piece[,] grid)
        {
            //does not account for check restrictions
            //returns list of possible moves for that piece

            move availableMove = new move();
            int availableX = this.coor.x;             //put coordinate in temp variable to manipulate while preserving original
            int availableY = this.coor.y;
            List<move> availableList = new List<move>();
            Color oppositeColor = switchTeam(this.color);

            //search up
            availableY++;
            while (availableY < 8)
            {
                if (grid[availableX, availableY].color == this.color)           //if same team
                {
                    break;                                                      //can't go past
                }

                else if (grid[availableX, availableY].color == oppositeColor)   //if enemy
                {
                    availableMove = new move(this, grid[availableX, availableY]);
                    availableList.Add(availableMove);       //add to list
                    break;                                  //can't go past
                }

                else                                        //if unoccupied
                {
                    availableMove = new move(this, grid[availableX, availableY]);
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
                if (grid[availableX, availableY].color == this.color)
                {
                    break;
                }

                else if (grid[availableX, availableY].color == oppositeColor)
                {
                    availableMove = new move(this, grid[availableX, availableY]);
                    availableList.Add(availableMove);
                    break;
                }

                else
                {
                    availableMove = new move(this, grid[availableX, availableY]);
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
                if (grid[availableX, availableY].color == this.color)
                {
                    break;
                }

                else if (grid[availableX, availableY].color == oppositeColor)
                {
                    availableMove = new move(this, grid[availableX, availableY]);
                    availableList.Add(availableMove);
                    break;
                }

                else
                {
                    availableMove = new move(this, grid[availableX, availableY]);
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
                if (grid[availableX, availableY].color == this.color)
                {
                    break;
                }

                else if (grid[availableX, availableY].color == oppositeColor)
                {
                    availableMove = new move(this, grid[availableX, availableY]);
                    availableList.Add(availableMove);
                    break;
                }

                else
                {
                    availableMove = new move(this, grid[availableX, availableY]);
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
        }

        public override List<move> getMoves(Piece[,] grid)
        {
            //does not account for check restrictions
            //returns list of possible moves for that piece

            move availableMove = new move();
            int availableX = this.coor.x;
            int availableY = this.coor.y;
            List<move> availableList = new List<move>();

            //search up.right
            availableY += 2;
            availableX++;
            if (availableY < 8 && availableX < 8)
            {
                if (grid[availableX, availableY].color != this.color)
                {
                    availableMove = new move(this, grid[availableX, availableY]);
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
                if (grid[availableX, availableY].color != this.color)
                {
                    availableMove = new move(this, grid[availableX, availableY]);
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
                if (grid[availableX, availableY].color != this.color)
                {
                    availableMove = new move(this, grid[availableX, availableY]);
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
                if (grid[availableX, availableY].color != this.color)
                {
                    availableMove = new move(this, grid[availableX, availableY]);
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
                if (grid[availableX, availableY].color != this.color)
                {
                    availableMove = new move(this, grid[availableX, availableY]);
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
                if (grid[availableX, availableY].color != this.color)
                {
                    availableMove = new move(this, grid[availableX, availableY]);
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
                if (grid[availableX, availableY].color != this.color)
                {
                    availableMove = new move(this, grid[availableX, availableY]);
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
                if (grid[availableX, availableY].color != this.color)
                {
                    availableMove = new move(this, grid[availableX, availableY]);
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
        }

        public override List<move> getMoves(Piece[,] grid)
        {
            //does not account for check restrictions
            //returns list of possible moves for that piece

            move availableMove = new move();
            int availableX = this.coor.x;
            int availableY = this.coor.y;
            List<move> availableList = new List<move>();
            Color oppositeColor = switchTeam(this.color);

            //search upper right
            availableX++;
            availableY++;
            while (availableX < 8 && availableY < 8)
            {
                if (grid[availableX, availableY].color == this.color)
                {
                    break;
                }

                else if (grid[availableX, availableY].color == oppositeColor)
                {
                    availableMove = new move(this, grid[availableX, availableY]);
                    availableList.Add(availableMove);
                    break;
                }

                else
                {
                    availableMove = new move(this, grid[availableX, availableY]);
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
                if (grid[availableX, availableY].color == this.color)
                {
                    break;
                }

                else if (grid[availableX, availableY].color == oppositeColor)
                {
                    availableMove = new move(this, grid[availableX, availableY]);
                    availableList.Add(availableMove);
                    break;
                }

                else
                {
                    availableMove = new move(this, grid[availableX, availableY]);
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
                if (grid[availableX, availableY].color == this.color)
                {
                    break;
                }

                else if (grid[availableX, availableY].color == oppositeColor)
                {
                    availableMove = new move(this, grid[availableX, availableY]);
                    availableList.Add(availableMove);
                    break;
                }

                else
                {
                    availableMove = new move(this, grid[availableX, availableY]);
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
                if (grid[availableX, availableY].color == this.color)
                {
                    break;
                }

                else if (grid[availableX, availableY].color == oppositeColor)
                {
                    availableMove = new move(this, grid[availableX, availableY]);
                    availableList.Add(availableMove);
                    break;
                }

                else
                {
                    availableMove = new move(this, grid[availableX, availableY]);
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
        }

        public override List<move> getMoves(Piece[,] grid)
        {
            //does not account for check restrictions
            //returns list of possible moves for that piece

            move availableMove = new move();
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
                if (grid[availableX, availableY].color == pieceColor)
                {
                    break;
                }

                else if (grid[availableX, availableY].color == oppositeColor)
                {
                    availableMove = new move(this, grid[availableX, availableY]);
                    availableList.Add(availableMove);
                    break;
                }

                else
                {
                    availableMove = new move(this, grid[availableX, availableY]);
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
                if (grid[availableX, availableY].color == pieceColor)
                {
                    break;
                }

                else if (grid[availableX, availableY].color == oppositeColor)
                {
                    availableMove = new move(this, grid[availableX, availableY]);
                    availableList.Add(availableMove);
                    break;
                }

                else
                {
                    availableMove = new move(this, grid[availableX, availableY]);
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
                if (grid[availableX, availableY].color == pieceColor)
                {
                    break;
                }

                else if (grid[availableX, availableY].color == oppositeColor)
                {
                    availableMove = new move(this, grid[availableX, availableY]);
                    availableList.Add(availableMove);
                    break;
                }

                else
                {
                    availableMove = new move(this, grid[availableX, availableY]);
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
                if (grid[availableX, availableY].color == pieceColor)
                {
                    break;
                }

                else if (grid[availableX, availableY].color == oppositeColor)
                {
                    availableMove = new move(this, grid[availableX, availableY]);
                    availableList.Add(availableMove);
                    break;
                }

                else
                {
                    availableMove = new move(this, grid[availableX, availableY]);
                    availableList.Add(availableMove);
                    availableX++;
                    availableY--;
                }
            }

            //search up
            availableY++;
            while (availableY < 8)
            {
                if (grid[availableX, availableY].color == pieceColor)  //if same team
                {
                    break;                                              //can't go past
                }

                else if (grid[availableX, availableY].color == oppositeColor)   //if enemy
                {
                    availableMove = new move(this, grid[availableX, availableY]);
                    availableList.Add(availableMove);       //add to list
                    break;                                  //can't go past
                }

                else                                        //if unoccupied
                {
                    availableMove = new move(this, grid[availableX, availableY]);
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
                if (grid[availableX, availableY].color == pieceColor)
                {
                    break;
                }

                else if (grid[availableX, availableY].color == oppositeColor)
                {
                    availableMove = new move(this, grid[availableX, availableY]);
                    availableList.Add(availableMove);
                    break;
                }

                else
                {
                    availableMove = new move(this, grid[availableX, availableY]);
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
                if (grid[availableX, availableY].color == pieceColor)
                {
                    break;
                }

                else if (grid[availableX, availableY].color == oppositeColor)
                {
                    availableMove = new move(this, grid[availableX, availableY]);
                    availableList.Add(availableMove);
                    break;
                }

                else
                {
                    availableMove = new move(this, grid[availableX, availableY]);
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
                if (grid[availableX, availableY].color == pieceColor)
                {
                    break;
                }

                else if (grid[availableX, availableY].color == oppositeColor)
                {
                    availableMove = new move(this, grid[availableX, availableY]);
                    availableList.Add(availableMove);
                    break;
                }

                else
                {
                    availableMove = new move(this, grid[availableX, availableY]);
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
        }

        public King(coordinate _coor, Color _color, bool _virgin)
        {
            this.coor = _coor;
            this.color = _color;
            this.job = Job.King;
            this.virgin = _virgin;
        }

        public override List<move> getMoves(Piece[,] grid)
        {
            //does not account for check restrictions
            //returns list of possible moves for that piece

            move availableMove = new move();
            int availableX = this.coor.x;
            int availableY = this.coor.y;
            List<move> availableList = new List<move>();
            Color pieceColor = this.color;

            //search up
            availableY++;
            if (availableY < 8)
            {
                if (grid[availableX, availableY].color != pieceColor)
                {
                    availableMove = new move(this, grid[availableX, availableY]);
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
                if (grid[availableX, availableY].color != pieceColor)
                {
                    availableMove = new move(this, grid[availableX, availableY]);
                    availableList.Add(availableMove);
                }
            }

            //search left
            availableX = this.coor.x;
            availableY = this.coor.y;
            availableX--;
            if (availableX >= 0)
            {
                if (grid[availableX, availableY].color != pieceColor)
                {
                    availableMove = new move(this, grid[availableX, availableY]);
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
                if (grid[availableX, availableY].color != pieceColor)
                {
                    availableMove = new move(this, grid[availableX, availableY]);
                    availableList.Add(availableMove);
                }
            }

            //search down
            availableX = this.coor.x;
            availableY = this.coor.y;
            availableY--;
            if (availableY >= 0)
            {
                if (grid[availableX, availableY].color != pieceColor)
                {
                    availableMove = new move(this, grid[availableX, availableY]);
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
                if (grid[availableX, availableY].color != pieceColor)
                {
                    availableMove = new move(this, grid[availableX, availableY]);
                    availableList.Add(availableMove);
                }
            }

            //search right
            availableX = this.coor.x;
            availableY = this.coor.y;
            availableX++;
            if (availableX < 8)
            {
                if (grid[availableX, availableY].color != pieceColor)
                {
                    availableMove = new move(this, grid[availableX, availableY]);
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
                if (grid[availableX, availableY].color != pieceColor)
                {
                    availableMove = new move(this, grid[availableX, availableY]);
                    availableList.Add(availableMove);
                }
            }

            //search for castling opportunity
            if (grid[this.coor.x, this.coor.y].virgin == true)//if king's first move
            {
                if (pieceColor == Color.Dark)
                {
                    if (grid[0, 7].virgin == true)//if left rook's first move
                    {
                        //if clear path from rook to king
                        if (grid[1, 7].job == Job.None && grid[2, 7].job == Job.None && grid[3, 7].job == Job.None)
                        {
                            availableMove = new move(this, grid[2, 7]);
                            availableList.Add(availableMove);
                        }
                    }

                    if (grid[7, 7].virgin == true)//if right rook's first move
                    {
                        if (grid[6, 7].job == Job.None && grid[5, 7].job == Job.None)
                        {
                            availableMove = new move(this, grid[6, 7]);
                            availableList.Add(availableMove);
                        }
                    }
                }

                else
                {
                    if (grid[0, 0].virgin == true)//if left rook's first move
                    {
                        if (grid[1, 0].job == Job.None && grid[2, 0].job == Job.None && grid[3, 0].job == Job.None)
                        {
                            availableMove = new move(this, grid[2, 0]);
                            availableList.Add(availableMove);
                        }
                    }

                    if (grid[7, 0].virgin == true)//if right rook's first move
                    {
                        if (grid[6, 0].job == Job.None && grid[5, 0].job == Job.None)
                        {
                            availableMove = new move(this, grid[6, 0]);
                            availableList.Add(availableMove);
                        }
                    }
                }
            }
            return availableList;
        }
    }

    public class Empty : Piece
    {
        public Empty(coordinate _coor)
        {
            this.coor = _coor;
            this.color = Color.None;
            this.job = Job.None;
            this.virgin = false;
        }

        public override List<move> getMoves(Piece[,] grid)
        {
            return null;
        }
    }
}
