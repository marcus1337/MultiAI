using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionSimulator : MonoBehaviour {

    private float m_car_length = 2.87f;
    private float m_car_realAxleOffset = 1.59f;

    private Vector3 currentSimulatedPosition; // (x position, z position, y angle relative game world)
    private float timeSlice;

    public MotionSimulator(Vector3 startPosition, float timeSlice)
    {
        this.currentSimulatedPosition = startPosition;
        this.timeSlice = timeSlice;
    }

    public Vector3 GetSimulatedPosition(float velocityMagnitude, float steeringAngle)
    {
        float directionAngle = currentSimulatedPosition.z;
        float xDiff = velocityMagnitude * Mathf.Cos((90 - directionAngle) * Mathf.Deg2Rad) * timeSlice;
        float zDiff = velocityMagnitude * Mathf.Sin((90 - directionAngle) * Mathf.Deg2Rad) * timeSlice;
        float steeringAngleDiff = (velocityMagnitude / m_car_length) * Mathf.Tan(steeringAngle * Mathf.Deg2Rad) * Mathf.Rad2Deg * timeSlice;
        currentSimulatedPosition = new Vector3(currentSimulatedPosition.x + xDiff, currentSimulatedPosition.y + zDiff, directionAngle + steeringAngleDiff);
        return currentSimulatedPosition;
    }
}
