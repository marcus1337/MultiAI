using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SocialPlatforms.GameCenter;


namespace UnityStandardAssets.Vehicles.Car
{
    [RequireComponent(typeof(CarController))]
    public class CarAI4 : MonoBehaviour
    {
        private CarController m_Car; // the car controller we want to use
        private Rigidbody rigidbody;

        public GameObject terrain_manager_game_object;
        TerrainManager terrain_manager;

        public GameObject[] friends;
        public GameObject[] enemies;


        //car move
        private GameObject replayCar;
        private Vector3 prePos;
        //   private float RCvelocity;
        private Vector3 curdirection;

        //private int counter_obstacle;
        private float maxSteerAngle;
        private float footBrake;
        private float steerAngle;
        private float acceleration;

        private float vMax = 0;// 30f;
        private float space = 2f;
        private float start_time;


        private void Start()
        {
            // get the car controller
            m_Car = GetComponent<CarController>();
            maxSteerAngle = m_Car.m_MaximumSteerAngle;
            terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager>();
            rigidbody = GetComponent<Rigidbody>();

            // note that both arrays will have holes when objects are destroyed
            // but for initial planning they should work
            friends = GameObject.FindGameObjectsWithTag("Player");
            enemies = GameObject.FindGameObjectsWithTag("Enemy");

            replayCar = friends[0];
            prePos = replayCar.transform.position;
            //  RCvelocity = 0f;
            start_time = Time.time;
            prevCarStep = new ReplayInfo(replayCar.transform.position, replayCar.transform.eulerAngles.y, totalTime);
            latestCarStep = new ReplayInfo(replayCar.transform.position, replayCar.transform.eulerAngles.y, totalTime);

        }

        private class ReplayInfo
        {
            public float timer = 0f;
            public float angle;
            public Vector3 position;

            public ReplayInfo(Vector3 position, float angle, float timer)
            {
                this.position = position;
                this.angle = angle;
                this.timer = timer;
            }
        }
        float totalTime = 0f;
        float replayVelocity = 0f;
        float replayAngularVelocity = 0f;
        ReplayInfo prevCarStep = null;
        ReplayInfo latestCarStep = null;
        private static float modulo(float a, float n)
        {
            return (float)(a - Math.Floor(a / n) * n);
        }

        private void FixedUpdate()
        {

            totalTime += Time.deltaTime;
            latestCarStep = new ReplayInfo(replayCar.transform.position, replayCar.transform.eulerAngles.y, totalTime);
            float timeDiff = latestCarStep.timer - prevCarStep.timer;
            if(timeDiff > 0)
            {
                float x1 = latestCarStep.position.x;
                float x2 = prevCarStep.position.x;
                float y1 = latestCarStep.position.z;
                float y2 = prevCarStep.position.z;
                float c = (float) Math.Sqrt(Math.Pow((x1 - x2),2.0f) + Math.Pow((y1 - y2),2.0f));
                float angle1 = latestCarStep.angle;
                float angle2 = prevCarStep.angle;
                float angleDiff = Math.Min((2 * 360) - Math.Abs(angle1 - angle2), Math.Abs(angle1 - angle2));
                replayVelocity = c / timeDiff;
                replayAngularVelocity = angleDiff / timeDiff;
              //  Debug.Log("NY DIST: " + c +" TIME: " + timeDiff + " velocity: " + (c/timeDiff) + " angleDiff " + (angleDiff/ timeDiff));
            }
            prevCarStep = latestCarStep;            

            if (Time.time - start_time > 3)
            {

                Vector3 CurPos = replayCar.transform.position;
                curdirection = (CurPos - prePos).normalized;
                //  RCvelocity = (transform.position - prePos).magnitude / Time.deltaTime;
                prePos = transform.position;
               // curdirection = (CurPos - (CurPos + replayCar.transform.forward)).normalized;

                Vector3 goalPos = new Vector3();
                Vector3 leftDirection = -curdirection + LeftDir(curdirection);
                Vector3 rightDirection = -curdirection + RightDir(curdirection);             

                Vector3 tmpDir = (CurPos - (CurPos + replayCar.transform.forward)).normalized;
                Vector3 testLeft = LeftDir(curdirection);
                Vector3 testRight = RightDir(curdirection);

                Vector3 goalPos1 = CurPos - curdirection * 10 + 10 * testLeft;
                Vector3 goalPos2 = CurPos - curdirection * 40 + 40 * testLeft;
                Vector3 goalPos3 = CurPos - curdirection * 10 + 10 * testRight;
                Vector3 goalPos4 = CurPos - curdirection * 40 + 40 * testRight;
                goalPos1 = goalPos2 = goalPos3 = goalPos4 = CurPos + 20*tmpDir;

                if (m_Car.name == "ArmedCar1")
                {
                   // goalPos = CurPos; //leftDirection * space + CurPos;
                    //Vector3 testLeft = LeftDir(curdirection);
                    goalPos = goalPos1;  //+ curdirection * 40 + 40 * testLeft;
                    
                    Debug.DrawLine(transform.position, goalPos, Color.magenta);
                    SetAcceleration(transform.position, goalPos);
                    steerAngle = GetSteerAngle(transform.position, goalPos);
                    m_Car.Move(steerAngle, acceleration, footBrake, 0);

                }
                else if (m_Car.name == "ArmedCar2")
                {
                    //  goalPos = leftDirection * space * 2 + CurPos;
                    goalPos = goalPos2;
                    Debug.DrawLine(transform.position, goalPos);
                    SetAcceleration(transform.position, goalPos);
                    steerAngle = GetSteerAngle(transform.position, goalPos);
                    m_Car.Move(steerAngle, acceleration, footBrake, 0);

                }
                else if (m_Car.name == "ArmedCar3")
                {
                    // goalPos = rightDirection * space + CurPos;
                    goalPos = goalPos3;
                    Debug.DrawLine(transform.position, goalPos);
                    SetAcceleration(transform.position, goalPos);
                    steerAngle = GetSteerAngle(transform.position, goalPos);
                    m_Car.Move(steerAngle, acceleration, footBrake, 0);

                }
                else //if(m_Car.name == "ArmedCar4")
                {
                    //  goalPos = rightDirection * space * 2 + CurPos;
                    goalPos = goalPos4;
                    Debug.DrawLine(transform.position, goalPos);
                    SetAcceleration(transform.position, goalPos);
                    steerAngle = GetSteerAngle(transform.position, goalPos);
                    m_Car.Move(steerAngle, acceleration, footBrake, 0);
                }

            }

        }

        //both input and output is normalised
        //clockwise
        Vector3 RightDir(Vector3 UnitDir)
        {
            return new Vector3(UnitDir.z, UnitDir.y, -UnitDir.x);
        }
        // counter clockwise
        Vector3 LeftDir(Vector3 UnitDir)
        {
            return new Vector3(-UnitDir.z, UnitDir.y, UnitDir.x);
        }

        void SetAcceleration(Vector3 currentCarPosition, Vector3 destinationPosition)
        {
            float distance = (destinationPosition - currentCarPosition).magnitude;
            if(m_Car.name == "ArmedCar1"){
          //      Debug.Log("TEST: " + distance);
            }

            bool is_to_the_front = Vector3.Dot(destinationPosition - currentCarPosition, curdirection) > 0f;
            float vel = rigidbody.velocity.magnitude;

            if (distance > 50f && is_to_the_front && vel <= vMax)
            {
                acceleration = 2f;
                footBrake = 0;
            }
            else if (distance > 40f && is_to_the_front && vel < vMax)
            {
                acceleration = 0.5f;
                footBrake = 0;
            }
            //else if (distance > 2.5f && vel < RCvelocity)
            else if (distance < 40f && is_to_the_front && vel > vMax)
            {
                steerAngle = 0;
                acceleration = 0f;
                footBrake = -1f;
            }
            else if (!is_to_the_front || vel > vMax)
            {
                steerAngle = 0;
                acceleration = 0f;
                footBrake = -1f;
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