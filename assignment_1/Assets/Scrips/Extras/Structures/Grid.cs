using Assets.Scrips.Extras.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Grid
{
    public readonly int[,] maze; //Assume 1 = obstacle, 0 = free road
    public readonly int width, height;
    public readonly int xlow, zlow;

    private readonly CostGrid costgrid;

    public int getCost(int x, int y)
    {
        return costgrid.GetCost(x, y);
    }

    public Grid(TerrainManager manager, int offset)
    {
        xlow = (int)manager.myInfo.x_low;
        zlow = (int)manager.myInfo.z_low;
        int xhigh = (int)manager.myInfo.x_high;
        int zhigh = (int)manager.myInfo.z_high;
        width = xhigh - xlow;
        height = zhigh - zlow;

        int[,] tmpMaze = new int[width, height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                int tmp_x = manager.myInfo.get_i_index(xlow + j);
                int tmp_z = manager.myInfo.get_j_index(zlow + i);
                int canTraverse = (int)manager.myInfo.traversability[tmp_x, tmp_z];
                tmpMaze[j, i] = canTraverse;
            }
        }

        //int i = manager.myInfo.get_i_index(xlow);
        //int j = manager.myInfo.get_j_index(zlow);
        maze = tmpMaze.Clone() as int[,];
        makeWallOffset(offset);
        blockImpossible();
        costgrid = new CostGrid(this);

    }

    private void blockImpossible()
    {
        HashSet<Point> closed = new HashSet<Point>();
        List<Point> wallPoints = new List<Point>();
        makeWallRecursive(new Point(0, 0), closed, wallPoints);
        foreach (Point p in wallPoints)
        {
            if (isImpossible(p))
                maze[p.x,p.y] = 1;
        }
    }

    private bool isImpossible(Point p)
    {
        if (IsOnGrid(p.x + 1, p.y) && IsOnGrid(p.x + 1, p.y + 1) && IsOnGrid(p.x, p.y + 1))
            return false;
        if (IsOnGrid(p.x - 1, p.y) && IsOnGrid(p.x - 1, p.y - 1) && IsOnGrid(p.x, p.y - 1))
            return false;
        if (IsOnGrid(p.x, p.y-1) && IsOnGrid(p.x + 1, p.y - 1) && IsOnGrid(p.x+1, p.y))
            return false;
        if (IsOnGrid(p.x-1, p.y + 1) && IsOnGrid(p.x - 1, p.y) && IsOnGrid(p.x, p.y+1))
            return false;

        return true;
    }


    private void makeWallOffset(int offset)
    {
        HashSet<Point> closed = new HashSet<Point>();
        List<Point> wallPoints = new List<Point>();
        makeWallRecursive(new Point(0, 0), closed, wallPoints);
        foreach (Point p in wallPoints)
        {
            for (int i = 0; i < offset; i++)
            {
                for (int j = 0; j < offset; j++)
                {
                    /*tryFill(p.x + i, p.y + j);
                    tryFill(p.x - i, p.y - j);
                    tryFill(p.x + i, p.y - j);
                    tryFill(p.x - i, p.y + j);*/
                    tryFill(p.x + i, p.y);
                    tryFill(p.x - i, p.y);
                    tryFill(p.x, p.y + j);
                    tryFill(p.x, p.y - j);
                }
            }
        }
    }

    private void tryFill(int x, int y)
    {
        if (IsOnGrid(x, y))
        {
            maze[x, y] = 1;
        }
    }

    private void makeWallRecursive(Point p, HashSet<Point> closed, List<Point> l)
    {
        if (closed.Contains(p) || p.x >= width || p.y >= height)
            return;
        closed.Add(p);

        if (IsOnGrid(p.x, p.y) && (!IsOnGrid(p.x + 1, p.y) || !IsOnGrid(p.x - 1, p.y)
            || !IsOnGrid(p.x, p.y + 1) || !IsOnGrid(p.x, p.y - 1)
            || !IsOnGrid(p.x + 1, p.y + 1) || !IsOnGrid(p.x - 1, p.y - 1)
            || !IsOnGrid(p.x - 1, p.y + 1) || !IsOnGrid(p.x + 1, p.y - 1)))
        {
            l.Add(p);
        }

        makeWallRecursive(new Point(p.x + 1, p.y), closed, l);
        makeWallRecursive(new Point(p.x, p.y + 1), closed, l);
    }

    public Grid(int[,] maze, int width, int height)
    {
        this.maze = maze;
        this.width = width;
        this.height = height;
    }

    public Node GetNode(Point location, int cost, int carDir = -1, int turns = 0)
    {
        if (IsOnGrid(location.x, location.y))
        {

               cost += costgrid.GetCost(location.x, location.y);
            return new Node(location, cost, this, carDir, turns);
        }

        return null;
    }

    //dir1 == UP, dir2 == DOWN, dir3 == LEFT, dir4 == RIGHT
    public int getStepsClosestWall(int dir, Point p)
    {
        int x1 = p.x;
        int y1 = p.y;
        int counter = 0;
        if (dir == 1)
        {
            while (IsOnGrid(x1, y1)) //y--
            {
                y1--;
                counter++;
            }
        }
        if (dir == 2)
        {
            while (IsOnGrid(x1, y1)) //y++
            {
                y1++;
                counter++;
            }
        }
        if (dir == 3)
        {
            while (IsOnGrid(x1, y1)) //x--
            {
                x1--;
                counter++;
            }
        }
        if (dir == 4)
        {
            while (IsOnGrid(x1, y1)) //x++
            {
                x1++;
                counter++;
            }
        }

        return counter;
    }

    public bool IsOnGrid(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height && maze[x, y] != 1;
    }

    private bool IsBlocked(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height && maze[x, y] == 1;
    }



}
