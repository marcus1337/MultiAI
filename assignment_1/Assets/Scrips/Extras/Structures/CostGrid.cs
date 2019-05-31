using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scrips.Extras.Structures
{
    class CostGrid
    {
        private readonly Grid maze;
        private readonly int[,] costGrid;
        private readonly int width, height;
        private readonly int[] costTable;

        public CostGrid(Grid maze)
        {
            this.maze = maze;
            width = maze.maze.GetLength(0);
            height = maze.maze.GetLength(1);
            costGrid = new int[width, height];
            costTable = new int[width * height];

            costTable[0] = 60;
            int price = 50;
            for(int i = 1; i < width; i++)
            {
                costTable[i] = price / i + (width -i);
            }

            init();
        }

        public void init()
        {
            HashSet<Point> closed = new HashSet<Point>();
            calcCosts(new Point(0,0),closed);
        }

        private bool isCloseToWall(Point p, int off)
        {
            for (int i = 1; i <= off; i++)
            {
                if (!maze.IsOnGrid(p.x + i, p.y))
                    return true;
                if (!maze.IsOnGrid(p.x - i, p.y))
                    return true;
                if (!maze.IsOnGrid(p.x, p.y+i))
                    return true;
                if (!maze.IsOnGrid(p.x, p.y-i))
                    return true;
                if (!maze.IsOnGrid(p.x+i, p.y + i))
                    return true;
                if (!maze.IsOnGrid(p.x - i, p.y - i))
                    return true;
                if (!maze.IsOnGrid(p.x - i, p.y + i))
                    return true;
                if (!maze.IsOnGrid(p.x + i, p.y - i))
                    return true;
            }

            return false;
        }

        private bool isCorner(int x, int y)
        {
            if (maze.IsOnGrid(x-1, y) && maze.IsOnGrid(x, y-1) && !maze.IsOnGrid(x-1,y-1))
            {
                return true;
            }
            if (maze.IsOnGrid(x - 1, y) && maze.IsOnGrid(x, y + 1) && !maze.IsOnGrid(x - 1, y + 1))
            {
                return true;
            }
            if (maze.IsOnGrid(x + 1, y) && maze.IsOnGrid(x, y + 1) && !maze.IsOnGrid(x + 1, y + 1))
            {
                return true;
            }
            if (maze.IsOnGrid(x + 1, y) && maze.IsOnGrid(x, y - 1) && !maze.IsOnGrid(x + 1, y - 1))
            {
                return true;
            }

            return false;
        }

        private int stepsClosest(Point p)
        {
            int tmpVal = int.MaxValue;
            int step = 0;

            for(int i = 0; i < width; i++)
            {
                if (!maze.IsOnGrid(p.x+i, p.y))
                    break;
                step++;
            }
            tmpVal = Math.Min(step, tmpVal);
            step = 0;
            for (int i = 0; i < width; i++)
            {
                if (!maze.IsOnGrid(p.x-i, p.y))
                    break;
                step++;
            }
            tmpVal = Math.Min(step, tmpVal);
            step = 0;
            for (int i = 0; i < width; i++)
            {
                if (!maze.IsOnGrid(p.x, p.y+i))
                    break;
                step++;
            }
            tmpVal = Math.Min(step, tmpVal);
            step = 0;
            for (int i = 0; i < width; i++)
            {
                if (!maze.IsOnGrid(p.x, p.y-i))
                    break;
                step++;
            }
            tmpVal = Math.Min(step, tmpVal);                       
            return tmpVal;
        }


        private void calcCosts(Point p, HashSet<Point> closed)
        {
            if (closed.Contains(p) || p.x >= width || p.y >= height)
                return;
            closed.Add(p);

            if (maze.IsOnGrid(p.x, p.y)) {
                // int steps = stepsClosest(p);
                // costGrid[p.x, p.y] = costTable[steps];
                if (isCloseToWall(p,1))
                {
                    costGrid[p.x, p.y] = 7;
                }else if (isCloseToWall(p, 2))
                {
                    costGrid[p.x, p.y] = 3;
                }

                if (isCorner(p.x, p.y))
                {
                    costGrid[p.x, p.y] = 1;
                    costGrid[p.x+1, p.y] = 1;
                    costGrid[p.x-1, p.y] = 1;
                    costGrid[p.x, p.y+1] = 1;
                    costGrid[p.x, p.y-1] = 1;
                }
                
                /*else if (isCloseToWall(p, 3))
                {
                    costGrid[p.x, p.y] = 3;
                }/*
                else if (isCloseToWall(p, 4))
                {
                    costGrid[p.x, p.y] = 5;
                }*/

            }

            calcCosts(new Point(p.x + 1, p.y), closed);
            calcCosts(new Point(p.x, p.y + 1), closed);
        }

        public int GetCost(int x, int y)
        {
            if (x >= 0 && y >= 0 && x < width && y < height)
                return costGrid[x,y];
            return 99999999;
        }

    }
}
