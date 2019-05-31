using Assets.Scrips.HELPERS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scrips.EXTRAS.STRUCTURES
{
    /*public class NodeGrid
    {
        private TerrainManager manager;
        public Node[,] grid;

        public int width, height;
        public int xlow, zlow;
        int spacing;

        public NodeGrid(TerrainManager manager)
        {
            spacing = 4;
            this.manager = manager;
            xlow = (int)manager.myInfo.x_low;
            zlow = (int)manager.myInfo.z_low;
            int xhigh = (int)manager.myInfo.x_high;
            int zhigh = (int)manager.myInfo.z_high;
            width = xhigh;
            height = zhigh;
            grid = new Node[width, height];

            int maxY = Int32.MinValue;
            int maxX = Int32.MinValue;
            int minY = Int32.MaxValue;
            int minX = Int32.MaxValue;

            for (int i = zlow; i < zhigh; i++)
            {
                for (int j = xlow; j < xhigh; j++)
                {
                    if (areaFree(j, i, spacing))
                    {
                        maxY = Math.Max(i, maxY);
                        minY = Math.Min(i, minY);
                        maxX = Math.Max(j, maxX);
                        minX = Math.Min(j, minX);
                        int tmpH = i + spacing / 2;
                        int tmpW = j + spacing / 2;
                        //tmpH = height - tmpH;
                        grid[j - xlow, i - zlow] = new Node(tmpW, tmpH, 0, j - xlow, i - zlow);
                    }
                    else
                    {
                        grid[j - xlow, i - zlow] = null;
                    }
                }
            }

            // Debug.Log("TESTAAA  " + grid[(minX - xlow)/2, (minY - zlow)/2].x + " " + grid[(minX - xlow)/2, (minY - zlow)/2].y);
            width = maxX - minX + 1;
            height = maxY - minY + 1;
            Node[,] tmpNodes = new Node[maxX - minX + 1, maxY - minY + 1];
            // Debug.Log("MIN X Y " + minX + "," + minY  +" , " + maxX + " , " + maxY);
            // Debug.Log("minvalue: " + grid[minX-xlow, minY-zlow].x + " _ " +  grid[minX-xlow, minY-zlow].y);

            int maxY2 = Int32.MinValue;
            int maxX2 = Int32.MinValue;
            int minY2 = Int32.MaxValue;
            //int minX2 = Int32.MaxValue;

            int ex = -1;
            for (int i = minY; i + ex <= maxY; i++)
            {
                ex++;
                int ey = -1;
                for (int j = minX; j + ey <= maxX; j++)
                {
                    ey++;
                    tmpNodes[j - minX, (maxY - minY) - (i - minY)] = grid[j - xlow + ey, i - zlow + ex];
                    if (tmpNodes[j - minX, (maxY - minY) - (i - minY)] != null)
                    {
                        //tmpNodes[j - minX, (maxY - minY) - (i - minY)].setMapXY(j - minX, (maxY - minY) - (i - minY));
                        maxY2 = Math.Max(maxY2, (maxY - minY) - (i - minY));
                        maxX2 = Math.Max(maxX2, j - minX);
                        minY2 = Math.Min(minY2, (maxY - minY) - (i - minY));
                    }
                }
            }

            int realMaxX = -1;
            int realMaxY = -1;
            // Debug.Log("EYy: " + minY2 + "  -- " + maxY + " " + height);
            for (int i = minY2; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    tmpNodes[j, i - minY2] = tmpNodes[j, i];
                    if (tmpNodes[j, i - minY2] != null)
                    {
                        tmpNodes[j, i - minY2].setMapXY(j, i - minY2);
                        realMaxX = Math.Max(realMaxX, j);
                        realMaxY = Math.Max(realMaxY, i - minY2);
                    }
                }
            }
            width = realMaxX + 1;
            height = realMaxY + 1;
            grid = tmpNodes;
            grid = ResizeArray(grid, height, width);

        }

        private bool areaFree(int y, int x, int size)
        {
            for (int i = x; i <= size + x; i++)
            {
                for (int j = y; j <= size + y; j++)
                {
                    int tmp_x = manager.myInfo.get_i_index(j);
                    int tmp_z = manager.myInfo.get_j_index(i);
                    int canTraverse = (int)manager.myInfo.traversability[tmp_x, tmp_z];
                    if (canTraverse == 1)
                        return false;
                }
                for (int j = y; j >= y - size; j--)
                {
                    int tmp_x = manager.myInfo.get_i_index(j);
                    int tmp_z = manager.myInfo.get_j_index(i);
                    int canTraverse = (int)manager.myInfo.traversability[tmp_x, tmp_z];
                    if (canTraverse == 1)
                        return false;
                }
            }

            for (int i = x; i >= x - size; i--)
            {
                for (int j = y; j <= size + y; j++)
                {
                    int tmp_x = manager.myInfo.get_i_index(j);
                    int tmp_z = manager.myInfo.get_j_index(i);
                    int canTraverse = (int)manager.myInfo.traversability[tmp_x, tmp_z];
                    if (canTraverse == 1)
                        return false;
                }
                for (int j = y; j >= y - size; j--)
                {
                    int tmp_x = manager.myInfo.get_i_index(j);
                    int tmp_z = manager.myInfo.get_j_index(i);
                    int canTraverse = (int)manager.myInfo.traversability[tmp_x, tmp_z];
                    if (canTraverse == 1)
                        return false;
                }
            }

            return true;
        }

        public Node getNode(int x, int y)
        {
            Node res = null;
            // Debug.Log("BEFORE " + grid[73,102].x + "_" + grid[73, 102].y);
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (grid[i, j] == null)
                        continue;
                    int off = spacing*2;
                    if (grid[i, j].x - off <= x && grid[i, j].x + off >= x &&
                        grid[i, j].y - off <= y && grid[i, j].y + off >= y)
                    {
                        // Debug.Log("Something was returned..... " + i + "_"+j);
                        return grid[i, j];
                    }
                }
            }
            return res;
        }

        public void refine()
        {
            Node[,] tmpGrid = new Node[width, height];
            int newMaxX = Int32.MinValue;
            int newMaxY = Int32.MinValue;

            for (int i = 1; i < height; i+=2)
            {
                for (int j = 1; j < width; j+=2)
                {
                    Node nLU = grid[j - 1, i - 1];
                    Node nRU = grid[j, i - 1];
                    Node nLD = grid[j - 1, i];
                    Node nRD = grid[j, i];
                    if (nLU != null && nRU != null && nLD != null && nRD != null)
                    {
                        int avgX = (nLU.x + nRU.x)/2;
                        int avgY = (nLU.y + nLD.y)/2;
                        Node node = new Node(avgX,avgY,0);
                        node.mapX = j/2;
                        node.mapY = i/2;
                        newMaxX = Math.Max(newMaxX, node.mapX);
                        newMaxY = Math.Max(newMaxY, node.mapY);
                        tmpGrid[node.mapX,node.mapY] = node;
                    }
                }
            }
            width = newMaxX;
            height = newMaxY;
            tmpGrid = ResizeArray(tmpGrid, height, width);
            grid = tmpGrid;

        }

        private static T[,] ResizeArray<T>(T[,] original, int rows, int cols)
        {
            var newArray = new T[rows, cols];
            int minRows = Math.Min(rows, original.GetLength(0));
            int minCols = Math.Min(cols, original.GetLength(1));
            for (int i = 0; i < minRows; i++)
                for (int j = 0; j < minCols; j++)
                    newArray[i, j] = original[i, j];
            return newArray;
        }

    }*/








    public class NodeGrid
    {
        private TerrainManager manager;
        public Node[,] grid;

        public int width, height;
        public float xlow, zlow, xhigh, zhigh;
        public float xstep, zstep;
        //int spacing;

        public NodeGrid(TerrainManager manager)
        {
            //spacing = 4;
            this.manager = manager;
            xlow = manager.myInfo.x_low;
            zlow = manager.myInfo.z_low;
            xhigh = manager.myInfo.x_high;
            zhigh = manager.myInfo.z_high;
            width = manager.myInfo.x_N * 2;
            height = manager.myInfo.z_N * 2;
            xstep = (xhigh - xlow) / width;
            zstep = (zhigh - zlow) / height;
            grid = new Node[width, height];

            //initiate all available nodes with ID = -1, obstacle node is null
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    float x = get_x_pos(i);
                    float y = get_z_pos(j);
                    //check if the four cells contain obstacles
                    if (ObstacleFound(x, y))
                    {
                        grid[i, j] = null;
                    }
                    else
                    {
                        grid[i, j] = new Node((int)x, (int)y, 0, i, j);
                    }
                }
            }
        }

        private bool ObstacleFound(float x, float y)
        {
            int i = get_i_index(x, false);
            int j = get_j_index(y, false);
            if (manager.myInfo.traversability[i, j] > 0.5f) { return true; }
            else { return false; }
        }

        public Node getNode(int x, int y)
        {
            int i = get_i_index(x, true);
            int j = get_j_index(y, true);
            return grid[i, j];
        }

        public Node getClosestNode(int x, int y)
        {
            Node n = getNode(x, y);
            if (n != null)
                return n;
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    int off = 7;
                    float gx = grid[i, j].position.x;
                    float gy = grid[i, j].position.y;

                    if (grid[i,j] != null && 
                        gx-off < x &&  x < gx + off &&
                        gy - off < y && y < gy + off)
                    {
                        return grid[i, j];
                    }
                }
            }

            return null;
        }



        // get index of given coordinate
        //maptype == true: 2D map, false: terrain D map
        public int get_i_index(float x, bool maptype)
        {
            int x_N = width;
            if (!maptype)
            {
                x_N = manager.myInfo.x_N;
            }
            int index = (int)Mathf.Floor(x_N * (x - xlow) / (xhigh - xlow));
            if (index < 0)
            {
                index = 0;
            }
            else if (index > x_N - 1)
            {
                index = x_N - 1;
            }
            return index;
        }

        public int get_j_index(float z, bool maptype)
        {
            int z_N = height;
            if (!maptype)
            {
                z_N = manager.myInfo.z_N;
            }
            int index = (int)Mathf.Floor(z_N * (z - zlow) / (zhigh - zlow));
            if (index < 0)
            {
                index = 0;
            }
            else if (index > z_N - 1)
            {
                index = z_N - 1;
            }
            return index;
        }

        public float get_x_pos(int i) { return (xlow + xstep / 2 + xstep * i); }

        public float get_z_pos(int j) { return (zlow + zstep / 2 + zstep * j); }




        public ANode GetNodeStar(ANode prev, int x, int y, int cost, int carDir = -1, int turns = 0)
        {
            if (IsOnGrid(x, y))
            {
                ANode node = new ANode(x,y, cost + 1, this, carDir,turns);
                /*if(!IsOnGrid(x + 1, y+1) || !IsOnGrid(x - 1, y-1))
                {
                    node.cost += 150;
                }*/
                int px = prev.x;
                int py = prev.y;
                if(carDir == ANode.DOWNLEFT)
                {
                    if (!IsOnGrid(px - 1, py) || !IsOnGrid(px, py + 1))
                        return null;
                }
                if (carDir == ANode.DOWNRIGHT)
                {
                    if (!IsOnGrid(px + 1, py) || !IsOnGrid(px, py + 1))
                        return null;
                }
                if (carDir == ANode.UPLEFT)
                {
                    if (!IsOnGrid(px - 1, py) || !IsOnGrid(px, py - 1))
                        return null;
                }
                if (carDir == ANode.UPRIGHT)
                {
                    if (!IsOnGrid(px + 1, py) || !IsOnGrid(px, py - 1))
                        return null;
                }

                return node;
            }
            return null;
        }

        public bool IsOnGrid(int x, int y)
        {
            if (x < 0 || y < 0 || x >= width || y >= height)
                return false;
            if (grid[x, y] == null)
                return false;
            return true;
        }


    }

}
