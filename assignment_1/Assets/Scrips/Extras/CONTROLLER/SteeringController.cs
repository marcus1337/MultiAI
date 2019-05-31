using UnityEngine;

public class SteeringController: PIDController
{

    public SteeringController(float timeSlice)
    {
        this.previousError = 0;
        this.integralError = 0;
        this.kp = 1; //0.001f;
        this.ki = 0f; //0.00001f;
        this.kd = 0f; //0.003f;
        this.timeSlice = timeSlice;

    }

    public override float GetOutput(float currentValue, float targetValue)
    {
        float error = targetValue - Mathf.DeltaAngle(0f, currentValue);
        error = Mathf.DeltaAngle(0f, error);
        integralError += error * timeSlice;
        float derivative = (error - previousError) /timeSlice;
        previousError = error;
        return + kp * error + ki * integralError + kd * derivative; // just make all signs negative here if you want to move backwards.
    }
}
