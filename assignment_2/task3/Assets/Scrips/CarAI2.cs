using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Assets.Scrips.HELPERS;

namespace UnityStandardAssets.Vehicles.Car
{
    [RequireComponent(typeof(CarController))]
    public class CarAI2 : MonoBehaviour
    {
        private CarController m_Car; // the car controller we want to use

        public GameObject terrain_manager_game_object;
        TerrainManager terrain_manager;

        public GameObject[] friends;
        public GameObject[] enemies;

        private void Start()
        {
            // get the car controller
            m_Car = GetComponent<CarController>();
            terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager>();
            pathgen = terrain_manager_game_object.GetComponent<PathGenerator>();

            // note that both arrays will have holes when objects are destroyed
            // but for initial planning they should work
            friends = GameObject.FindGameObjectsWithTag("Player");
            enemies = GameObject.FindGameObjectsWithTag("Enemy");


            drawInfo();
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
        //public GameObject[] path;
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
              //  points[i] = new Vector3(n.x, 0.1f, n.y);
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

        

        private PathGenerator pathgen;




        private void FixedUpdate()
        {


            // Execute your path here
            // ...

            Vector3 avg_pos = Vector3.zero;

            foreach (GameObject friend in friends)
            {
                avg_pos += friend.transform.position;
            }
            avg_pos = avg_pos / friends.Length;
            Vector3 direction = (avg_pos - transform.position).normalized;

            bool is_to_the_right = Vector3.Dot(direction, transform.right) > 0f;
            bool is_to_the_front = Vector3.Dot(direction, transform.forward) > 0f;

            float steering = 0f;
            float acceleration = 0;

            if (is_to_the_right && is_to_the_front)
            {
                steering = 1f;
                acceleration = 1f;
            }
            else if (is_to_the_right && !is_to_the_front)
            {
                steering = -1f;
                acceleration = -1f;
            }
            else if (!is_to_the_right && is_to_the_front)
            {
                steering = -1f;
                acceleration = 1f;
            }
            else if (!is_to_the_right && !is_to_the_front)
            {
                steering = 1f;
                acceleration = -1f;
            }

            // this is how you access information about the terrain
            int i = terrain_manager.myInfo.get_i_index(transform.position.x);
            int j = terrain_manager.myInfo.get_j_index(transform.position.z);
            float grid_center_x = terrain_manager.myInfo.get_x_pos(i);
            float grid_center_z = terrain_manager.myInfo.get_z_pos(j);

            Debug.DrawLine(transform.position, new Vector3(grid_center_x, 0f, grid_center_z));


            // this is how you control the car
          //  Debug.Log("Steering:" + steering + " Acceleration:" + acceleration);
            m_Car.Move(steering, acceleration, acceleration, 0f);
            //m_Car.Move(0f, -1f, 1f, 0f);


        }
    }
}
