using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityStandardAssets.Vehicles.Car;

class CrashBacking
{
    private CarController m_Car;
    private CarAI car;
    private Grid grid;
    private Point startPoint, endPoint;
    private AStar aStar;
    private GameObject[] path;

    Stopwatch stopwatch;
    bool stopState;
    bool stopState2;

    private TerrainManager terrainManager;

    public void onEnterCollision()
    {
        if (stopState || stopState2)
            return;
        stopState = true;
        stopwatch.Start();
        UnityEngine.Debug.Log("First stop: " + car.transform.eulerAngles.y);
    }

    public CrashBacking(CarAI car, CarController m_Car, Grid grid, AStar aStar,
        Point endPoint, GameObject[] path, TerrainManager terrainManager)
    {
        this.terrainManager = terrainManager;
        steer = gas = 0f;
        this.path = path;
        this.car = car;
        this.m_Car = m_Car;
        this.grid = grid;
        this.aStar = aStar;
        this.endPoint = endPoint;

        stopwatch = new Stopwatch();
        stopState = false;
        stopState2 = false;
    }

    bool wallUp;
    bool wallRight;
    bool wallLeft;
    bool wallDown;

    int getClosestSide()
    {

        Point p = new Point((int)car.transform.position.x - grid.xlow, (int)car.transform.position.z - grid.zlow);
        int stepsUp = grid.getStepsClosestWall(1, p);
        int stepsDown = grid.getStepsClosestWall(2, p);
      //  int stepsLeft = grid.getStepsClosestWall(3, p);
      //  int stepsRight = grid.getStepsClosestWall(4, p);
        wallUp = false;
        wallRight = false;
        wallLeft = false;
        wallDown = false;


        if (stepsUp > stepsDown)
        {
            UnityEngine.Debug.Log("Wall to Up");
            wallUp = true;
        }
        else
        {
            wallDown = true;
            UnityEngine.Debug.Log("Wall to Down");
        }

        return 0;
    }

    private float steer, gas, angle;

    void up()
    {
        float angle = car.transform.eulerAngles.y;

        if(angle >= 270 || angle <= 90)
        {
            gas = -1f;
        }
        else
        {
            gas = 1f;
        }

    }

    void down()
    {
        if (angle >= 270 || angle <= 90)
        {
            gas = 1f;
        }
        else
        {
            gas = -1f;
        }
    }


    void backCarABit()
    {
        angle = car.transform.eulerAngles.y;
        if (wallUp)
            up();
        if (wallDown)
            down();

        if(gas == 1f)
            m_Car.Move(steer, gas, 1f, 0f);
        else
            m_Car.Move(steer, gas, gas, 0f);
        if (stopwatch.ElapsedMilliseconds > 3000)
        {
            stopState2 = false;
            stopwatch.Stop();
            stopwatch.Reset();
            startPoint = new Point((int)car.transform.position.x, (int)car.transform.position.z);
            car.generatePath(startPoint.x, startPoint.y);
            if(aStar.result.Count == 0)
            {
                grid = new Grid(terrainManager, 1);
                aStar.setGrid(grid);
                car.generatePath(startPoint.x, startPoint.y);
            }

            UnityEngine.Debug.Log("Third step: ");
        }
    }

    public bool activeCheck()
    {
        if (stopState2)
        {
            backCarABit();
            return true;
        }

        if (stopState)
        {
            postCollision();
            return true;
        }

        return false;
    }


    private void postCollision()
    {
        m_Car.Move(0f, 0f, 0f, 1f);

        if (stopwatch.ElapsedMilliseconds > 2500)
        {
            for (int i = 0; i < path.Length; i++)
            {
                GameObject.Destroy(path[i]);
            }
            getClosestSide();
            stopState = false;
            stopState2 = true;
            stopwatch.Stop();
            stopwatch.Reset();
            stopwatch.Start();
            UnityEngine.Debug.Log("Second stop: " + car.transform.eulerAngles.y);
        }
    }

}
