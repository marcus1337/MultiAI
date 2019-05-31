
public class VelocityController : PIDController
{
    public VelocityController(float timeSlice)
    {
        this.previousError = 0;
        this.integralError = 0;
        this.kp = 1f;
        this.ki = 0.01f;
        this.kd = 0.001f;
        this.timeSlice = timeSlice;

    }

    public override float GetOutput(float currentValue, float targetValue)
    {
        float error = targetValue - currentValue;
        integralError += error * timeSlice;
        float derivative = (error - previousError) / timeSlice;
        previousError = error;
        return kp * error + ki * integralError + kd * derivative;
    }
}
