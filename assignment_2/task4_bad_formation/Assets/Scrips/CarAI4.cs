using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine.SocialPlatforms.GameCenter;


namespace UnityStandardAssets.Vehicles.Car
{
    [RequireComponent(typeof(CarController))]
    public class CarAI4 : MonoBehaviour
    {
        public GameObject terrain_manager_game_object;
        TerrainManager terrain_manager;

        public GameObject[] friends;
        public GameObject[] enemies;
        private Rigidbody[] rigidbody;
        private CarController[] m_Car; // the car controller we want to use
        private int[] leaderIndex;
        private int[] relativeDir;
        private float[] waitTime;

        //car move
        private float maxSteerAngle;
        private float footBrake;
        private float steerAngle;
        private float acceleration;

        //replay car
        private GameObject replayCar;
        private Vector3 preRCPos;


        //formation parameter
        private float edgeLength;
        private float start_time;


        private void Start()
        {
            terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager>();

            // note that both arrays will have holes when objects are destroyed
            // but for initial planning they should work
            friends = GameObject.FindGameObjectsWithTag("Player");
            enemies = GameObject.FindGameObjectsWithTag("Enemy");

            // initialization
            // car name: replay, 3, 1, 4, 2 
            replayCar = friends[0];
            rigidbody = new Rigidbody[friends.Length];
            m_Car = new CarController[friends.Length];
            leaderIndex = new int[] { 0, 2, 0, 0, 3 };
            relativeDir = new int[] { 0, -180, -120, 120, 180 }; // -1: left to leader, 1: right to leader
            for (int i = 0; i < friends.Length; i++)
            {
                rigidbody[i] = friends[i].GetComponent<Rigidbody>();
                if (i == 0)
                {
                    m_Car[i] = null;
                }
                else
                {
                    m_Car[i] = friends[i].GetComponent<CarController>();
                }
            }
            waitTime = new float[] { 0, 1.1f, 0.8f, 0.5f, 0.8f };

            maxSteerAngle = m_Car[1].m_MaximumSteerAngle;
            edgeLength = 12f;
            preRCPos = replayCar.transform.position;
            start_time = Time.time;



        }


        private void FixedUpdate()
        {
            // update replay car velocity and previous pos
            Vector3 curRCPos = replayCar.transform.position;
            Vector3 replayVelocity = (curRCPos - preRCPos) / Time.fixedDeltaTime;
            rigidbody[0].velocity = replayVelocity;
            preRCPos = curRCPos;

            //Debug.Log("replay car velocity: " + rigidbody[0].velocity.magnitude);

            for (int i = 1; i < m_Car.Length; i++)
            {
                Transform leaderTrans = friends[leaderIndex[i]].transform;
                Vector3 leaderPos = leaderTrans.position;
                Vector3 followPos = m_Car[i].transform.position;
                Vector3 nextPos = getFormationPos(leaderTrans, relativeDir[i]);
                float followVel = rigidbody[i].velocity.magnitude;
                float leaderVel = rigidbody[leaderIndex[i]].velocity.magnitude;

                Debug.DrawLine(leaderPos, nextPos, Color.blue);

                acceleration = 0f;
                footBrake = 0f;

                float distance = (nextPos - followPos).magnitude;
                //float distance2 = (leaderPos - followPos).magnitude;
                steerAngle = GetSteerAngle(followPos, nextPos, m_Car[i].transform.forward);

                // set acceleration
                if (followVel < leaderVel && Time.time > 3f)
                {
                    acceleration = 1f;
                }

                if (followVel >= leaderVel && Time.time > 3f)
                {
                    if (distance < edgeLength * 2)
                    {
                        if (Math.Abs(steerAngle) * Mathf.Rad2Deg < 45)
                        {
                            if (followVel - leaderVel < 1f)
                            {
                                acceleration = 0.3f;
                            }
                            else
                            {
                                footBrake = -0.3f;
                            }
                        }
                        else
                        {
                            footBrake = -1f;
                        }
                    }
                    else
                    {
                        if (followVel - leaderVel < 1f)
                        {
                            acceleration = 1f;
                        }
                        else
                        {
                            acceleration = 0f;
                        }
                    }
                }


                if (Time.time > waitTime[i])
                {
                    m_Car[i].Move(steerAngle, acceleration, footBrake, 0f);
                }

            }

        }


        Vector3 getFormationPos(Transform leaderTrans, int steerAngle)
        {
            Vector3 dir = Quaternion.AngleAxis(steerAngle, Vector3.up) * leaderTrans.forward;
            return dir * edgeLength + leaderTrans.position;
        }


        void SetAcceleration(Vector3 goalPos, float time)
        {
            //Vector3 goalPos = path.Peek();
            //Vector3 curleaderPos = leaderpath.Peek();
            Vector3 curPos = transform.position;
            //bool is_to_the_front = Vector3.Dot(transform.forward, goalPos - curPos) > 0f;
            float distance = (curPos - goalPos).magnitude;
            Vector3 velocity = rigidbody[0].velocity;
            //float distance1 = (curPos - centre).magnitude;
            //float distance2 = (curPos - goalPos).magnitude;

            //bool is_to_the_front1 = Vector3.Dot(curTrans.forward, curPos - centre) > 0f;
            //bool is_to_the_front2 = Vector3.Dot(curTrans.forward, curleaderPos - curPos) > 0f;

            acceleration = 0f;
            footBrake = 0f;

            if (distance > 5f)
            {
                acceleration = 1f;
            }
            else if (velocity.magnitude <= distance)
            {
                acceleration = 0.5f;
            }
            else if (velocity.magnitude > distance)
            {
                footBrake = -1f;
            }

        }


        float GetSteerAngle(Vector3 currentPos, Vector3 destination, Vector3 forwardDirection)
        {
            Vector3 directionVector = destination - currentPos;
            float turnAngle = Vector3.Angle(directionVector, forwardDirection);
            if (Vector3.Cross(directionVector, forwardDirection).y > 0)
            {
                turnAngle = -turnAngle;
            }
            turnAngle = Mathf.Clamp(turnAngle, (-1) * maxSteerAngle, maxSteerAngle) / maxSteerAngle;

            return turnAngle;
        }
    }
}