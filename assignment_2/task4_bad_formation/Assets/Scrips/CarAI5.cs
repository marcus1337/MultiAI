using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Assets.Scrips.HELPERS;

namespace UnityStandardAssets.Vehicles.Car
{
    [RequireComponent(typeof(CarController))]
    public class CarAI5 : MonoBehaviour
    {
        private CarController m_Car; // the car controller we want to use

        public GameObject terrain_manager_game_object;
        TerrainManager terrain_manager;

        public GameObject[] friends;
        public GameObject[] enemies;
        private Rigidbody rigidbody;

        private PathGenerator pathgen;

        //car move
        private float maxSteerAngle;
        private float footBrake;
        private float steerAngle;
        private float acceleration;

        private GameObject replayCar;
        private Vector3 prePos;
        private Vector3 CurPos;
        private Quaternion preRotation;

        private float vMax = 7f;

        private float start_time;
        private GameManager game_manager;
        public GameObject game_manager_object;

        private Node currNode;
        private bool ifCollided;
        private bool cantCollide;

        float totalTime, checkTime;

        List<LinkedList<Node>> allPaths;
        LinkedList<Node> currentPath;

        private void Start()
        {
            state = MOVING;
            targetIndex = 0;
            cantCollide = false;
            rigidbody = GetComponent<Rigidbody>();
            game_manager = game_manager_object.GetComponent<GameManager>();
            // get the car controller
            m_Car = GetComponent<CarController>();
            maxSteerAngle = m_Car.m_MaximumSteerAngle;
            terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager>();
            friends = GameObject.FindGameObjectsWithTag("Player");
            enemies = GameObject.FindGameObjectsWithTag("Enemy");
            pathgen = terrain_manager_game_object.GetComponent<PathGenerator>();

            replayCar = friends[0];

            start_time = Time.time;
            allPaths = new List<LinkedList<Node>>();
            foreach (LinkedList<Node> lln in pathgen.listpaths)
            {
                allPaths.Add(new LinkedList<Node>(lln));
            }
            currentPath = new LinkedList<Node>(allPaths[0]);
            //allPaths = new List<LinkedList<Node>>(allPaths);
        }

        void makePath()
        {
            var oldPath = new LinkedList<Node>(allPaths[targetIndex]);
            lastPosInPath = oldPath.Last.Value.position;
            pathgen.grid.setCostGrid(pathgen.costGrid.costMaps[targetIndex]);
            AStar astar = new AStar(pathgen.grid);
            int tmpX = pathgen.grid.get_i_index(transform.position.x, true);
            int tmpY = pathgen.grid.get_j_index(transform.position.z, true);


            Node n = pathgen.grid.getClosestNode((int)transform.position.x, (int)transform.position.z);
            tmpX = n.mapX;
            tmpY = n.mapY;


            int tmpEndX = pathgen.grid.get_i_index(lastPosInPath.x, true);
            int tmpEndY = pathgen.grid.get_j_index(lastPosInPath.z, true);

            
            astar.init(tmpX, tmpY, tmpEndX, tmpEndY, -1);
            astar.findPath();
            List<ANode> ares = astar.result;
            LinkedList<Node> realResult = new LinkedList<Node>();
            foreach (ANode an in ares)
            {
                Node tmpNode = pathgen.grid.grid[an.x, an.y];
                realResult.AddFirst(tmpNode);
            }
            currentPath = realResult;
        }

        void OnCollisionEnter(Collision collisionInfo)
        {
            if (collisionInfo.collider.GetType() == typeof(BoxCollider))
            {
                if (!cantCollide)
                    ifCollided = true;
            }
        }
        int counter_obstacle = 0;

        void avoidObstacles(Vector3 targetPos)
        {
            steerAngle = GetSteerAngle(transform.position, targetPos);
            m_Car.Move(-steerAngle, 0, -1, 0);

            counter_obstacle++;
            if (counter_obstacle > 50)
            {
                counter_obstacle = 0;
                ifCollided = false;
            }
        }

        private Vector3 lastPosInPath;

        void moveNormally(Vector3 targetPos)
        {
            if (lastPosInPath == targetPos)
            {
                breakCar();
                currentPath.RemoveFirst();
                return;
            }
            lastPosInPath = currentPath.Last.Value.position;
            Debug.DrawLine(transform.position, targetPos);
            SetAccCruise(transform.position, targetPos);
            steerAngle = GetSteerAngle(transform.position, targetPos);
            m_Car.Move(steerAngle, acceleration, footBrake, 0);
            //bool in_front = Vector3.Dot(direction, transform.forward) > 0f;
            if ((targetPos - transform.position).magnitude < 10f)
            {
                currentPath.RemoveFirst();
            }
        }

        static int ATTACK = 1, SAFETY = 2, MOVING = 3, WAIT = 4;
        int state;
        int targetIndex;

        bool timerStarted = false;

        void startTimer()
        {
            if (!timerStarted)
            {
                timerStarted = true;
                start_time = totalTime;
            }
        }

        bool hasTimeElapsed(float seconds)
        {
            // Debug.Log("INFO " + (totalTime - start_time - seconds));
            if (timerStarted)
            {
                return totalTime - start_time - seconds > 0f;
            }
            return false;
        }
        /*void stopTimer()
        {
            timerStarted = false;
        }*/

        void changeState()
        {
            if (hasTimeElapsed(2.5f) && state == MOVING)
            {
                timerStarted = false;
                state = ATTACK;
            }

            if (hasTimeElapsed(7f) && state == ATTACK)
            {
                timerStarted = false;
                state = SAFETY;
            }

            if (hasTimeElapsed(4f) && state == SAFETY)
            {
                targetIndex = (targetIndex + 1) % 8;
                timerStarted = false;
                state = MOVING;
                makePath();
            }
        }


        void attack()
        {
            if (state != ATTACK)
                return;
            lastPosInPath.y = 0.1f;

            SetAccAttack(transform.position, lastPosInPath);
            steerAngle = GetSteerAngle(transform.position, lastPosInPath);
            m_Car.Move(steerAngle, acceleration, footBrake, 0);
            startTimer();

        }

        private void moveCar()
        {

            //Debug.Log("Time test: " + totalTime);
            //Debug.Log("Counter: " + allPaths[0].Count);
            //Debug.Log("Steer: " + steerAngle + " acc: " + acceleration + " brake " + footBrake);
            if (state == MOVING)
            {
                if (currentPath.Count > 0)
                {
                    cantCollide = false;
                    currNode = currentPath.First.Value;
                    Vector3 position = new Vector3(currNode.position.x, 0.1f, currNode.position.z);
                    if (!ifCollided)
                    {
                        moveNormally(position);
                    }
                    else
                    {
                        avoidObstacles(position);
                    }
                }
                else//Target reached
                {
                    startTimer();
                    cantCollide = true;
                    breakCar();
                    m_Car.Move(steerAngle, acceleration, footBrake, 0);
                }

            }
            if (state == SAFETY)
            {
                startTimer();
                cantCollide = true;
                m_Car.Move(0f, 1f, -1f, 0);

            }

        }

        private void breakCar()
        {
            float vel = rigidbody.velocity.magnitude;
            float diffVel = (rigidbody.velocity.normalized - rigidbody.transform.forward).magnitude;
            if (vel > 0.2f && diffVel < 0.8)
            {
                acceleration = 1f;
                footBrake = -1f;
            }
            else
            {
                acceleration = 0f;
                footBrake = 0f;
            }
        }

        private void FixedUpdate()
        {
            totalTime += Time.deltaTime;
            changeState();

            if (m_Car.name == "ArmedCar1")
            {
                moveCar();
                attack();

            }


            // m_Car.Move(steerAngle, acceleration, footBrake, 0);

        }

        TargetPoint target = null;

        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            //Gizmos.DrawSphere(tmpvec, 7);
            // Debug.Log("tmpvec: " + tmpvec);

            Gizmos.color = Color.red;
            int counting = 0;
            foreach (TargetPoint t in pathgen.targets)
            {
                counting++;
                if (counting == 2)
                    Gizmos.color = Color.magenta;
                if (counting == 3)
                    Gizmos.color = Color.blue;
                if (counting == 4)
                    Gizmos.color = Color.white;
                if (counting == 5)
                    Gizmos.color = Color.yellow;
                if (counting == 6)
                    Gizmos.color = Color.cyan;
                if (counting == 7)
                    Gizmos.color = Color.black;
                if (counting == 8)
                    Gizmos.color = Color.green;

                Vector3 pos = new Vector3(t.x, 2f, t.y);
                Gizmos.DrawSphere(pos, 3);
            }

            Gizmos.color = Color.red;
            /*  List<LinkedList<Node>> listpaths = pathgen.listpaths;
              foreach (LinkedList<Node> l in listpaths)
              {
                  foreach (Node an in l)
                  {
                      Vector3 tmpPos = an.position;
                      Gizmos.DrawCube(tmpPos, new Vector3(1, 1, 1));
                  }

              }*/
            Gizmos.color = Color.cyan;
            foreach (Node an in currentPath)
            {
                Vector3 tmpPos = an.position;
                Gizmos.DrawCube(tmpPos, new Vector3(1, 1, 1));
            }

            //   Gizmos.DrawSphere(new Vector3(target.x, 3f, target.y), 8);
            //Debug.Log("TESTAA: " + target.x + " _ " + target.y);

        }

        void SetAccAttack(Vector3 currentCarPosition, Vector3 destinationPosition)
        {
            float distance = (destinationPosition - currentCarPosition).magnitude;
            float vel = rigidbody.velocity.magnitude;

            Vector3 direction = (destinationPosition - transform.position).normalized;
            bool in_front = Vector3.Dot(direction, transform.forward) > 0f;
            if (distance < 5)
            {
                breakCar();
                steerAngle = 0f;
                return;
            }

            if (in_front && distance >= 10f)
            {
                acceleration = 0.5f;
                footBrake = 0;
            }
            else if (in_front && distance < 10)
            {
                acceleration = 0.3f;
                footBrake = 0f;
            }
            else
            {
                breakCar();
                steerAngle = 0f;
            }
        }

        void SetAccCruise(Vector3 currentCarPosition, Vector3 destinationPosition)//slow but safe
        {
            float distance = (destinationPosition - currentCarPosition).magnitude;
            float vel = rigidbody.velocity.magnitude;
            if (distance > 50 && vel < vMax)
            {
                acceleration = 1f;
                footBrake = 0;
            }
            else
            if (distance <= 50f && vel <= vMax)
            {
                acceleration = 0.5f;
                footBrake = 0;
            }
            else
            {
                acceleration = 0f;
                footBrake = 0f;
            }
        }

        void SetAcceleration(Vector3 currentCarPosition, Vector3 destinationPosition)
        {
            float distance = (destinationPosition - currentCarPosition).magnitude;
            float vel = rigidbody.velocity.magnitude;
            if (distance > 10f && vel <= vMax)
            {
                acceleration = 0.5f;
                footBrake = 0;
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


    }
}