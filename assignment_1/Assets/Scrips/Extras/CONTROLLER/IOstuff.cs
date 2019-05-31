using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;

public class IOstuff : MonoBehaviour
{

    GameObject terrain_manager_game_object;
    private int width, height, zlow, zhigh, xlow, xhigh;
    private int[,] maze;

    private int getLowestZ()
    {
        for (int i = 0; i < height; i++)
            for (int j = 0; j < width; j++)
                if (maze[j, i] == 0 && i < (height - 3))
                    return i - 1;
        return 0;
    }

    private int geHighestZ()
    {
        for (int i = 0; i < height; i++)
            for (int j = 0; j < width; j++)
                if (maze[j, height - i - 1] == 0 && height - i > 2)
                    return i - 1;
        return 0;
    }

    private int getLowestX()
    {
        for (int i = 0; i < height; i++)
            for (int j = 0; j < width; j++)
                if (maze[j, i] == 0 && j > 2)
                    return j - 1;
        return 0;
    }

    private int geHighestX()
    {
        for (int i = 0; i < height; i++)
            for (int j = 0; j < width; j++)
                if (maze[width - j - 1, i] == 0 && width - j > 3)
                    return j - 1;
        return 0;
    }

    internal void storeText(string filename, Grid grid, TerrainManager manager, List<Node> bestpath)
    {
        string path = Application.dataPath + "/" + filename + ".txt";
        width = grid.width;
        height = grid.height;
        maze = grid.maze;
        xlow = (int)manager.myInfo.x_low;
        zlow = (int)manager.myInfo.z_low;
        xhigh = (int)manager.myInfo.x_high;
        zhigh = (int)manager.myInfo.z_high;

        string gridinfo = "width: " + width + " height: " + height + "\n";


        gridinfo += getMazeStr(bestpath);

        File.WriteAllText(path, gridinfo);
    }


    private string getMazeStr(List<Node> bestpath)
    {
        string res = "";
        int[,] tmpMaze = maze.Clone() as int[,];
        if (bestpath != null)
            foreach (Node n in bestpath)
            {
                tmpMaze[n.location.x, n.location.y] = 5;
            }

        int offsetZlow = getLowestZ();
        int offsetZhigh = geHighestZ();
        int offsetXlow = getLowestX();
        int offsetXhigh = geHighestX();

        for (int i = offsetZlow; i < height - offsetZhigh; i++)
        {
            for (int j = offsetXlow; j < width - offsetXhigh; j++)
            {
                if (tmpMaze[j, (height - 1) - i] == 0)
                {
                    res += ' ';
                }
                else
                    res += tmpMaze[j, (height - 1) - i];

            }
            res += "\n";
        }
        return res;
    }
}

