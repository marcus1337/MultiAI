
public abstract class PIDController
{
    protected float previousError;
    protected float integralError;
    protected float kp;
    protected float ki;
    protected float kd;
    protected float timeSlice;

    public abstract float GetOutput(float currentValue, float targetValue);
}
