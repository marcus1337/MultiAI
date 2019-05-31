using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;
using Assets.Scrips.Extras;
using Assets.Scrips.Extras.Structures;

namespace UnityStandardAssets.Vehicles.Car
{
    [RequireComponent(typeof(CarController))]
    public class CarAI : MonoBehaviour
    {
        private CarController m_Car; // the car controller we want to use

        public GameObject terrain_manager_game_object;
        TerrainManager terrain_manager;
        
        private Grid grid;
        private Point startPoint, endPoint;
        private AStar aStar;

        private float m_car_length = 2.87f;
        private float m_car_realAxleOffset = 1.59f;
        public GameObject[] path;

        int currentGoalIndex = 0;
        GameObject emptyGO;

        private PIDController velocityController;
        private PIDController steeringController;
        private MotionSimulator motionSimulator;
        private CrashBacking crashHandler;


        private void OnCollisionEnter(Collision collision)
        {
            crashHandler.onEnterCollision();
        }

        public void SetGrid(Grid grid)
        {
            this.grid = grid;
        }

        public void generatePath(int startX, int startY)
        {
            startPoint = new Point(startX, startY);
            currentGoalIndex = 0;
            //Car orientation, 360 = up, 270 = left, 90 = right, 180 = down
            float carangle = transform.eulerAngles.y;
            aStar.init(startPoint.x - grid.xlow, startPoint.y - grid.zlow, endPoint.x - grid.xlow, endPoint.y - grid.zlow, carangle);
            aStar.findPath();
            path = new GameObject[aStar.result.Count];
            aStar.result.Reverse();
            int i = 0;
            foreach (Node n in aStar.result)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.position = new Vector3(startPoint.x + (n.location.x - aStar.result[0].location.x), 0.5f, startPoint.y + (n.location.y - aStar.result[0].location.y));
                cube.GetComponent<BoxCollider>().enabled = false;
                path[i] = cube;
                i++;
            }
        }

        private void InitGridPath()
        {
            int wallOffset = 8;
            grid = new Grid(terrain_manager, wallOffset);
            aStar = new AStar(grid);
            generatePath(startPoint.x, startPoint.y);
            while (wallOffset > 0)
            {
                if (aStar.result.Count != 0)
                    break;
                wallOffset--;
                grid = new Grid(terrain_manager, wallOffset);
                aStar.setGrid(grid);
                generatePath(startPoint.x, startPoint.y);
            }
            UnityEngine.Debug.Log("Chosen Offset: " + wallOffset);
        }

        private void Awake()
        {

            /// get the car controller
            m_Car = GetComponent<CarController>();
            terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager>();
    
            startPoint = new Point((int)terrain_manager.myInfo.start_pos.x,(int)terrain_manager.myInfo.start_pos.z);
            endPoint = new Point((int)terrain_manager.myInfo.goal_pos.x, (int)terrain_manager.myInfo.goal_pos.z);

            InitGridPath();

            emptyGO = new GameObject();

            /// Instantiating simple controllers that control velocity and steering angle.
            /// Instantiating simple controllers that control velocity and steering angle.
            velocityController = new VelocityController(Time.fixedDeltaTime);
            steeringController = new SteeringController(Time.fixedDeltaTime);
            Vector2 realAxleStartPosition = RearAxleMidPosition(terrain_manager.myInfo.start_pos.x, terrain_manager.myInfo.start_pos.z, transform.eulerAngles.y);
            motionSimulator = new MotionSimulator(new Vector3(realAxleStartPosition.x, realAxleStartPosition.y, transform.eulerAngles.y), Time.deltaTime);

            crashHandler = new CrashBacking(this,m_Car, grid, aStar, endPoint, path, terrain_manager);
            /*
            IOstuff iostuff = new IOstuff();
            iostuff.storeText("datainfo", grid, terrain_manager, aStar.result);

            Debug.Log("WIDTH: " + grid.width + " HEIGHT: " + grid.height + " startx: " + startPoint.x + " startz: " + startPoint.y);
            Debug.Log("endx: " + endPoint.x + " endZ: " + endPoint.y);
            // Debug.Log("lowZ: " + terrain_manager.myInfo.z_low + " _ " + terrain_manager.myInfo.z_high); */
           
        }


        private void FixedUpdate()
        {
            //UnityEngine.Debug.Log("Second stop: " + transform.eulerAngles.y);
            if (crashHandler.activeCheck())
                return;

            //UnityEngine.Debug.Log("WAT: " + aStar.result.Count + " , " + path.Length + " currentgoal: "+ currentGoalIndex);
            /// If we are 5 points within the path object it is considered visited. Dangerous tactic, not a good soluion, just to test. Works good for now thought.
            if (Vector3.Distance(path[currentGoalIndex].transform.position, transform.position) <= 5f)
            {
                currentGoalIndex = FindNearestPoint(currentGoalIndex, currentGoalIndex + 1);
            }
    
            /// Lazy to calculate angle between two objects, using empty object and invoking LookAt function to do that for me.
            emptyGO.transform.position = transform.position;
            emptyGO.transform.rotation = transform.rotation;
            Transform t = emptyGO.transform;
            t.LookAt(path[currentGoalIndex].transform);

            float x = t.rotation.eulerAngles.y;
            /// x - 180 for oposite ange. Moving backwards.
            float angle = Mathf.DeltaAngle(0f, x);
            
            /// Setting target velocity and sendind negative to footbrake to move backwards.
            float velocity = velocityController.GetOutput(GetComponent<Rigidbody>().velocity.magnitude, 3f);
            float steering = steeringController.GetOutput(transform.rotation.eulerAngles.y, angle);
            m_Car.Move(steering, velocity , 0f, 0f);


            /*

            /// Simulating position, right now it is not going to work while going backwards. Velocity is getting, not calculating since I've not manage to approximate it with higher velocities.
            Vector2 realAxlePosition = RearAxleMidPosition(transform.position.x, transform.position.z, transform.eulerAngles.y);
            Debug.Log("Actual Position: x:" + realAxlePosition.x + "  z:" + realAxlePosition.y + " angle:" + transform.eulerAngles.y);
            Vector3 simulatedPosition = motionSimulator.GetSimulatedPosition(GetComponent<Rigidbody>().velocity.magnitude, m_Car.CurrentSteerAngle);
            Debug.Log("Simulated Position: x:" + simulatedPosition.x + "  z:" + simulatedPosition.y + " angle:" + simulatedPosition.z);
            Debug.Log("---------------------------"); */

        }


        /// <summary>
        /// Finding the closest cube object relative the current car position along the path between start and end idnex. Probably not needed, this was just testing some stuff.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private int FindNearestPoint(int start, int end)
        {
            int nearestIndex = start + 1;
            float nearestDistance = Vector2.Distance(new Vector2(transform.position.x, transform.position.y), new Vector2(aStar.result[nearestIndex].location.x, aStar.result[nearestIndex].location.y));
            for(int i = nearestIndex + 1; i < end; i++)
            {
                float distance = Vector2.Distance(new Vector2(transform.position.x, transform.position.y), new Vector2(aStar.result[i].location.x, aStar.result[i].location.y));
                if (distance <= nearestDistance)
                {
                    nearestDistance = distance;
                    nearestIndex = i;
                }
            }
            return nearestIndex;
        }

        /// <summary>
        /// Calculate rear axle mid position given the transform position and y angle relative game world.
        /// </summary>
        /// <param name="xPos"></param>
        /// <param name="zPos"></param>
        /// <param name="carAngle"></param>
        /// <returns></returns>
        private Vector2 RearAxleMidPosition(float xPos, float zPos, float carAngle)
        {
            float newXPos = xPos + m_car_realAxleOffset * Mathf.Sin(carAngle * Mathf.Deg2Rad);
            float newZpos = zPos + m_car_realAxleOffset * Mathf.Cos(carAngle * Mathf.Deg2Rad);
            return new Vector2(newXPos, newZpos);
        }
    }
}
