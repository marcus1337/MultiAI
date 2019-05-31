using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Assets.Scrips.HELPERS;
using Assets.Scrips.EXTRAS.STRUCTURES;

namespace UnityStandardAssets.Vehicles.Car
{
    [RequireComponent(typeof(CarController))]
    public class CarAI1 : MonoBehaviour
    {
        private CarController m_Car; // the car controller we want to use

        public GameObject terrain_manager_game_object;
        TerrainManager terrain_manager;
        private Rigidbody rigidbody;
        private Collider collider;

        public GameObject[] friends;
        public GameObject[] enemies;

        //tree
        private PathGenerator pathgen;
        private LinkedList<Node> subtree;
        private bool treeComplete;
        private bool ifCollided;
        private Node currNode;
        private NodeGrid nodegrid;
        private Node[,] grid;

        //car move
        private int counter_obstacle;
        private float maxSteerAngle;
        private float footBrake;
        private float steerAngle;
        private float acceleration;
        public float vMax = 10f;
        private Vector3 collision_point;

        private void Start()
        {
            Debug.Log("CAR 1 AI START");
            // get the car controller
            m_Car = GetComponent<CarController>();
            maxSteerAngle = m_Car.m_MaximumSteerAngle;
            terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager>();
            rigidbody = GetComponent<Rigidbody>();
            pathgen = terrain_manager_game_object.GetComponent<PathGenerator>();
            nodegrid = pathgen.grid;
            grid = nodegrid.grid;
            // note that both arrays will have holes when objects are destroyed
            // but for initial planning they should work
            friends = GameObject.FindGameObjectsWithTag("Player");
            enemies = GameObject.FindGameObjectsWithTag("Enemy");

            counter_obstacle = 0;

            if (m_Car.name == "ArmedCar1")
            {
                //draw only called once
                drawInfo();
                subtree = pathgen.robot1.subtree;
                Debug.Log("Car" + pathgen.robot1.ID + "begins!");

                PathInfo[,] M = pathgen.edgegenerator.M;
                /* int numTurrets = pathgen.edgegenerator.numTurrets;

                 for (int i = 0; i < numTurrets; i++)
                 {
                     for (int j = i + 1; j < numTurrets; j++)
                     {
                         int blue = (i * 7 + 10) % 255;
                         int red = (j * 11 + 50) % 255;
                         int green = ((i + j) * 3) % 255;
                         Color color = new Color32((byte)red, (byte)green, (byte)blue, 1);
                         drawPath(M[i, j].path, color);
                         //   Debug.Log("PATH["+i+","+j+"]: " + M[i,j].path.Count);
                     }
                 }*/

                //drawPath(M[0, 7].path, Color.red, 1.0f,1.0f);
                //drawPath(M[7, 5].path, Color.yellow, -1.0f, -1.0f);
               // drawPath(M[5, 10].path, Color.red, 2.0f, 2.0f);
                //drawPath(M[10, 12].path, Color.blue, -2.0f, -2.0f);
                //drawPath(M[12, 8].path, Color.magenta);

                // foreach(ANode n in M[0, 3].path)
                // {
                //     Debug.Log("WAT: " + n.x + " " + n.y);
                //  }



            }
            else if (m_Car.name == "ArmedCar2")
            {
                subtree = pathgen.robot2.subtree;
                Debug.Log("Car" + pathgen.robot2.ID + "begins!");
            }
            else if (m_Car.name == "ArmedCar3")
            {
                subtree = pathgen.robot3.subtree;
                Debug.Log("Car" + pathgen.robot3.ID + "begins!");
            }
            subtree.RemoveFirst();

        }

        private void FixedUpdate()
        {

            if (subtree.Count > 0)
            {
                currNode = subtree.First();
                Vector3 position = new Vector3(currNode.position.x, 0.1f, currNode.position.z);
                if (!ifCollided)
                {
                    Debug.DrawLine(transform.position, position);

                    //set the speed and footbrake
                    SetAcceleration(transform.position, position);
                    //set the steer angle
                    steerAngle = GetSteerAngle(transform.position, position);

                    m_Car.Move(steerAngle, acceleration, footBrake, 0);

                    //check if the car has got to the current node
                    //Vector3 direction = (currNode.position - transform.position).normalized;
                    //bool in_front = Vector3.Dot(direction, transform.forward) > 0f;
                    if ((position - transform.position).magnitude < 8f)
                    {
                        //  print("Current Position: " + transform.position + "Current Node: " + position);
                        subtree.RemoveFirst();
                        //   Debug.Log("total number of nodes " + subtree.Count);
                    }

                }
                else
                {
                    //  Debug.Log("Collision:" + counter_obstacle);

                    steerAngle = GetSteerAngle(transform.position, position);
                    m_Car.Move(-steerAngle, 0, -1, 0);

                    //AvoidCollision();
                    //m_Car.Move(steerAngle, acceleration, footBrake, 0);

                    counter_obstacle++;
                    if (counter_obstacle > 40)
                    {
                        counter_obstacle = 0;
                        ifCollided = false;
                    }
                }
            }
            else
            {
                if (rigidbody.velocity.magnitude > 0.1f)
                    m_Car.Move(0f, 0f, -1f, 0f);
                else
                {
                    print("path of " + m_Car.name + "complete!");
                    enabled = false;
                }
            }
        }

        void OnCollisionEnter(Collision collisionInfo)
        {
            if (collisionInfo.collider.GetType() == typeof(BoxCollider))
            {
                ifCollided = true;
                collision_point = collisionInfo.contacts[0].point;
                print("Collided:" + collision_point);
            }
        }

        //set the car_move parameter when there is a collision
        void AvoidCollision()
        {
            Vector3 direction = (collision_point - transform.position).normalized;
            Debug.Log("collision point:" + collision_point);
            Debug.Log("transform point:" + transform.position);

            bool collison_on_right = Vector3.Dot(direction, transform.right) > 0f;
            bool collison_in_front = Vector3.Dot(direction, transform.forward) > 0f;


            if (collison_on_right && collison_in_front)
            {
                steerAngle = -1f;
                acceleration = 0f;
                footBrake = -1f;
                Debug.Log("turn left back!");
            }
            else if (collison_on_right && !collison_in_front)
            {
                steerAngle = -1f;
                acceleration = 1f;
                footBrake = 0f;
                Debug.Log("go left ahead!");
            }
            else if (!collison_on_right && collison_in_front)
            {
                steerAngle = -1f;
                acceleration = 0f;
                footBrake = -1f;
                Debug.Log("turn left back!");
            }
            else if (!collison_on_right && !collison_in_front)
            {
                steerAngle = -1f;
                acceleration = 0f;
                footBrake = -1f;
                Debug.Log("go right ahead!");
            }
        }

        void SetAcceleration(Vector3 currentCarPosition, Vector3 destinationPosition)
        {
            float distance = (destinationPosition - currentCarPosition).magnitude;
            float vel = rigidbody.velocity.magnitude;
            if (distance > 10f && vel <= vMax)
            {
                acceleration = 1f;
                footBrake = 0;
            }
            else if (distance > 5f && vel < vMax)
            {
                acceleration = 0.5f;
                footBrake = 0f;
            }
            else
            {
                acceleration = 0f;
                footBrake = 0f;
            }

        }
        float GetSteerAngle(Vector3 currentCarPosition, Vector3 destinationPosition)
        {
            Vector3 directionVector = destinationPosition - currentCarPosition;
            float turnAngle = Vector3.Angle(directionVector, transform.forward);
            if (Vector3.Cross(directionVector, transform.forward).y > 0)
            {
                turnAngle = -turnAngle;
            }
            turnAngle = Mathf.Clamp(turnAngle, (-1) * maxSteerAngle, maxSteerAngle) / maxSteerAngle;

            return turnAngle;
        }

        private void drawInfo()
        {
            //BEGIN DRAWING
            LinkedList<Node> subtree1 = pathgen.robot1.subtree;
            LinkedList<Node> subtree2 = pathgen.robot2.subtree;
            LinkedList<Node> subtree3 = pathgen.robot3.subtree;
            drawTree(subtree1, Color.magenta);
            drawTree(subtree2, Color.red);
            drawTree(subtree3, Color.black);
        }

        void drawTree(LinkedList<Node> subtree, Color color)
        {
            Debug.Log("SIZE OF SUB: " + subtree.Count);
            GameObject treeDraw = new GameObject();
            LineRenderer lineRenderer = treeDraw.AddComponent<LineRenderer>();
            lineRenderer.material.color = color;
            lineRenderer.widthMultiplier = 1f;
            lineRenderer.useWorldSpace = true;
            lineRenderer.SetVertexCount(subtree.Count);
            var points = new Vector3[subtree.Count];

            int i = 0;
            foreach (Node n in subtree)
            {
                points[i] = new Vector3(n.position.x, 0.1f, n.position.z);
                i++;
            }

            lineRenderer.SetPositions(points);

            /* foreach (Node curr_node in subtree)
             {
                 if (skipFirst)
                 {
                     skipFirst = false;
                     continue;
                 }
                 lineRenderer.SetPosition(counter, new Vector3(firstNode.x, 0.1f, firstNode.y));
                 lineRenderer.SetPosition(counter+1, new Vector3(curr_node.x, 0.1f, curr_node.y));
                 firstNode = curr_node;
                 counter++;
             }*/


        }

        void drawPath(List<ANode> subtree, Color color, float offX = 0, float offY = 0)
        {
            // Debug.Log("SIZE OF SUB: " + subtree.Count);
            GameObject treeDraw = new GameObject();
            LineRenderer lineRenderer = treeDraw.AddComponent<LineRenderer>();
            lineRenderer.material.color = color;
            lineRenderer.widthMultiplier = 1f;
            lineRenderer.useWorldSpace = true;
            lineRenderer.SetVertexCount(subtree.Count);
            var points = new Vector3[subtree.Count];
            int i = 0;
            foreach (ANode n in subtree)
            {
                Node node = grid[n.x, n.y];
                // Debug.Log("TEST: " + node.position.x + "," + node.position.z);
                points[i] = new Vector3(node.position.x + offX, 0.1f, node.position.z + offY);
                i++;
            }
            lineRenderer.SetPositions(points);
        }
    }
}