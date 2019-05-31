using Assets.Scrips.EXTRAS.STRUCTURES;
using Assets.Scrips.HELPERS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class CostGrid
{

    List<Vector3> turret_list;
    List<GameObject> turretObj_list;
    private RaycastHit hit;
    private NodeGrid nodegrid;
    public int width, height;
    public CostPoint[,] costMap;
    private Node[,] Map2D;
    public int[][,] costMaps;

    private void copyGrid(int index)
    {
        if (index >= turret_list.Count)
            return;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {

                costMaps[index][i, j] = costMap[i, j].numTurrets;
            }
        }
    }

    public CostGrid(GameManager gameManager, NodeGrid nodegrid)
    {
        width = nodegrid.width;
        height = nodegrid.height;
        Map2D = nodegrid.grid;
        costMap = new CostPoint[width, height];

        turretObj_list = gameManager.turret_list;
        turret_list = new List<Vector3>();
        this.nodegrid = nodegrid;
        foreach (GameObject turret in turretObj_list)
        {
            turret_list.Add(turret.transform.position);
        }

        costMaps = new int[turret_list.Count][,];
        for (int i = 0; i < turret_list.Count; i++)
        {
            costMaps[i] = new int[width, height];
        }

    }

    private bool checkHit(Vector3 nodePos, Vector3 tmpDirection, Vector3 turrPos)
    {
        int countHits = 0;
        Vector3 tmpPos = new Vector3(nodePos.x, nodePos.y, nodePos.z);
        if (Physics.Raycast(tmpPos, tmpDirection, out hit, Mathf.Infinity))
        {
            String hitname = hit.collider.gameObject.name;
            if (hitname.Equals("Cylinder"))
            {
                countHits++;
            }
            else
            {
            }
        }
        tmpPos.x += 0f;
        tmpPos.y += 1f;
        tmpDirection = (turrPos - tmpPos);
        if (Physics.Raycast(tmpPos, tmpDirection, out hit, Mathf.Infinity))
        {
            String hitname = hit.collider.gameObject.name;
            if (hitname.Equals("Cylinder"))
            {
                countHits++;
            }
            else
            {
            }
        }

        tmpPos.x += 1f;
        tmpPos.y -= 1f;
        tmpDirection = (turrPos - tmpPos);
        if (Physics.Raycast(tmpPos, tmpDirection, out hit, Mathf.Infinity))
        {
            String hitname = hit.collider.gameObject.name;
            if (hitname.Equals("Cylinder"))
            {
                countHits++;
            }
            else
            {
            }
        }

        return countHits == 3;
    }

    public CostPoint countHits(Vector3 nodePos)
    {
        int res = 0;
        nodePos.y = 3f;
        int counter = 0;
        CostPoint costpoint = new CostPoint();

        foreach (Vector3 turrPosTmp in turret_list)
        {
            var turrPos = turrPosTmp;
            turrPos.y = 3f;
            Vector3 tmpDirection = (turrPos - nodePos);

            if (checkHit(nodePos, tmpDirection, turrPos))
            {
                costpoint.turretsAtPoint.Add(counter);
                res++;
            }

            counter++;
        }

        costpoint.numTurrets = res;
        return costpoint;
    }

    public void removePoint(int x, int y)
    {
        CostPoint costTmp = costMap[x, y];
        List<int> turrRmv = new List<int>(costTmp.turretsAtPoint);
        //costTmp.numTurrets = 0;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                //scostMap[i, j].turretsAtPoint.Except(turrRmv).ToList();
                foreach (int id in turrRmv)
                {
                    costMap[i, j].turretsAtPoint.Remove(id);
                }
                costMap[i, j].numTurrets = costMap[i, j].turretsAtPoint.Count;
            }
        }

    }

    public void createTargets()
    {
        targets = new List<TargetPoint>();
        TargetPoint target = null;

        int counter = 0;
        while (true)
        {
            copyGrid(counter);
            counter++;
            //IOstuff iostuff = new IOstuff();
            //  iostuff.storeTextCosts("testOkokok"+counter.ToString(), this);

            target = getTarget();
            if (target == null)
                return;
            else
                targets.Add(target);
        }

    }

    TargetPoint res = null;
    public TargetPoint getTarget()
    {
        TargetPoint res = new TargetPoint();
        bool[,] visited = new bool[width, height];
        bestDistance = Int32.MaxValue;

        bfs(res, -1, visited, startX, startY);
        startX = res.mapX;
        startY = res.mapY;
        if (res.numEnemies == Int32.MaxValue)
            return null;
        removePoint(startX, startY);
        // Debug.Log("TARGET INFO: " + res.x + " , " + res.y + " dir: " + res.dir);
        return res;
    }
    private bool hasTurrets(CostPoint from, CostPoint to)
    {
        int counter = from.turretsAtPoint.Count;
        foreach (int id in to.turretsAtPoint)
        {
            if (from.turretsAtPoint.Contains(id))
                counter--;
        }

        return counter == 0;
    }

    private int calcNeighbors(int nx, int ny, int dir)
    {
        var nowP = costMap[nx, ny];
        int totalN = 0;
        if (dir == TargetPoint.UP || dir == TargetPoint.DOWN)
        {
            var upper = costMap[nx, ny + 1];
            var lower = costMap[nx, ny - 1];

            if (nowP.shareTurret(lower))
                totalN++;
            if (nowP.shareTurret(upper))
                totalN++;
        }
        else
        {
            var lefti = costMap[nx - 1, ny];
            var righti = costMap[nx + 1, ny];

            if (nowP.shareTurret(lefti))
                totalN++;
            if (nowP.shareTurret(righti))
                totalN++;
        }
        return totalN;
    }

    private int getDistance(int nx, int ny)
    {
        return (Math.Abs(startX - nx) + Math.Abs(startY - ny));
    }

    int bestDistance;

    private void bfs(TargetPoint t, int dir, bool[,] visited, int nx, int ny)
    {
        if (nx < 0 || ny < 0 || nx >= width || ny >= height || Map2D[nx, ny] == null || visited[nx, ny])
            return;
        visited[nx, ny] = true;
        var nowP = costMap[nx, ny];
        int turrets = costMap[nx, ny].numTurrets;
        List<int> turList = costMap[nx, ny].turretsAtPoint;
        if (costMap[nx + 1, ny].numTurrets + costMap[nx - 1, ny].numTurrets == 0 &&
            costMap[nx, ny + 1].numTurrets + costMap[nx, ny - 1].numTurrets == 0)
        {

        }
        else
        {
            if (turrets > 0 && turrets < t.numEnemies)
            {
                t.numEnemies = turrets;
                t.mapX = nx;
                t.mapY = ny;
                t.x = nodegrid.get_x_pos(nx);
                t.y = nodegrid.get_z_pos(ny);
                t.dir = dir;
                // bestDistance = getDistance(nx, ny);
                //  t.neighbors = calcNeighbors(nx,ny,dir);

            }
            /* else if(turrets > 0 && turrets == t.numEnemies && getDistance(nx,ny) < bestDistance)
             {
                 t.numEnemies = turrets;
                 t.mapX = nx;
                 t.mapY = ny;
                 t.x = nodegrid.get_x_pos(nx);
                 t.y = nodegrid.get_z_pos(ny);
                 t.dir = dir;
                 bestDistance = getDistance(nx, ny);
             }*/
            else if (turrets > 0)
                return;
        }

        if (dir == TargetPoint.UP)
            bfs(t, TargetPoint.UP, visited, nx, ny + 1);
        if (dir == TargetPoint.DOWN)
            bfs(t, TargetPoint.DOWN, visited, nx, ny - 1);
        if (dir == TargetPoint.LEFT)
            bfs(t, TargetPoint.LEFT, visited, nx - 1, ny);
        if (dir == TargetPoint.RIGHT)
            bfs(t, TargetPoint.RIGHT, visited, nx + 1, ny);

        bfs(t, TargetPoint.UP, visited, nx, ny + 1);
        bfs(t, TargetPoint.RIGHT, visited, nx + 1, ny);
        bfs(t, TargetPoint.DOWN, visited, nx, ny - 1);
        bfs(t, TargetPoint.LEFT, visited, nx - 1, ny);
    }

    public List<TargetPoint> targets;
    private int startX, startY;

    public void init(int startX, int startY)
    {
        this.startX = startX; this.startY = startY;

        Debug.Log(" POS: " + Map2D[startX, startY].position);
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Node n = Map2D[i, j];
                if (n == null)
                {
                    // float x = get_x_pos(i);
                    // float y = get_z_pos(j);
                    costMap[i, j] = new CostPoint();
                }
                else
                {
                    Vector3 nodePos = n.position;
                    //  Debug.Log("x " + i + " y " + j + " POS: " + nodePos);
                    costMap[i, j] = countHits(nodePos);
                }
            }

        }

        //IOstuff iostuff = new IOstuff();
        //iostuff.storeTextCosts("testOkokok", this);

        createTargets();

        //Debug.Log("IT worked, " + targets.Count);

    }

}

