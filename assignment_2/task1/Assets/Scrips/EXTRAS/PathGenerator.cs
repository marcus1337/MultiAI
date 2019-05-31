using Assets.Scrips.EXTRAS.STRUCTURES;
using Assets.Scrips.HELPERS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;


//*NB! This class must be added as a component on Unity for Awake() to be called.*//
public class PathGenerator : MonoBehaviour
{
    public GameObject terrain_manager_game_object;
    private TerrainManager terrain_manager;
    public NodeGrid grid;

    //for problem 3 to get the turrets positions
    public GameObject game_manager_object;
    private GameManager game_manager;

    private Vector3[] start_pos;

    public Robot robot1, robot2, robot3;
    public List<Robot> robots;

    public int ProblemIndex = 2;

    //for problem 1
    public TreeGenerator treeGenerator;
    private Node[,] Map2D;

    //for problem 3
    public EdgeGenerator edgegenerator;
    public GAGenerator gaGenerator;

    private static bool runOnce = false;

    private void Start()
    {

        terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager>();
        game_manager = game_manager_object.GetComponent<GameManager>();

        Initialize();

        //must be called after initialize done
        switch (ProblemIndex)
        {
            case 1:
                Problem1();
                break;
            case 2:
                Problem2();
                break;
            default:
                Console.WriteLine("there is no such problem!");
                break;
        }

    }

    //initialize must be called before any problem func start
    public void Initialize()
    {
        //initialize the map
        grid = new NodeGrid(terrain_manager);

        //initialize robots
        robots = new List<Robot>();
        start_pos = new Vector3[3]{new Vector3(220,0.1f,230),
            new Vector3(209.5f,0.1f,230), new Vector3(201.3f,0.1f,230)};

        for (int i = 0; i < 3; i++)
        {
            Vector3 startPosition = start_pos[i];
            Node start_node = grid.getNode((int)startPosition.x, (int)startPosition.z);
            start_node.ID = -1;
            Robot robot = new Robot(startPosition, i + 1);
            robot.subtree.AddFirst(start_node);
            //robot.path.AddFirst(new Point(startPosition));
            robots.Add(robot);
        }

        robot1 = robots[0];
        robot2 = robots[1];
        robot3 = robots[2];

        //        if (startnode == null)
        //        {
        //            startnode = new Node(startX, startY, -1, grid.get_i_index(startX,true),grid.get_j_index(startY,true));
        //        }
    }

    private void Problem1()
    {
        Map2D = grid.grid;
        treeGenerator = new TreeGenerator(Map2D, grid, robots);
        treeGenerator.GenerateTree();
    }

    private void Problem2()
    {
        List<GameObject> turretObj_list = game_manager.turret_list;
        List<Vector3> turret_list = new List<Vector3>();
        foreach (GameObject turret in turretObj_list)
        {
            turret_list.Add(turret.transform.position);
            //Debug.Log("turret position:" + turret.transform.position);
        }

        List<Vector3> all_list = new List<Vector3>();
        all_list.AddRange(start_pos);
        all_list.AddRange(turret_list);

        Debug.Log("TEST: " + all_list.Count);
        //build path
        edgegenerator = new EdgeGenerator(all_list, grid);


        //build GA
        gaGenerator = new GAGenerator(turret_list, robots, grid, edgegenerator);
        gaGenerator.GeneratePath();

    }

}