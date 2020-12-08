using UnityEngine;
using UnityEngine.Events;

public class CustomController : BaseController
{
    float baseSpeed = 10.0f;
    float baseAccel = 15.0f;

    float groundStopSpeed = 3f;

    float airControl = 150.0f;

    float friction = 6.0f;
    float gravity = 20.0f;

    float jumpHeight = 1f;

    protected override void Move()
    {
        // Snaps input to 45 degrees then locks it to -1, 0, and 1 per axis
        if(isGrounded)
        {
            localVelocity.y = 0f;
            GroundMove();
        } else// if(groundNormal == Vector3.up)
        {
            AirMove();
        }/* else
        {
            SlideMove();
        }*/
    }

    void GroundMove()
    {
        if(input.JumpHeld)
        {
            Jump();
        } else
        {
            Gravity();
            Friction();
        }

        if(MoveMagnitude <= 0)
            return;
        
        float moveSpeed = baseSpeed * BoxWarpMagnitude(input.MoveDirection);
        Accelerate(MoveDirection, moveSpeed, baseAccel);
        
        LimitStrafe();
    }

    float BoxWarpMagnitude(Vector2 direction)
    {
        if(direction.x == 0 || direction.y == 0)
            return direction.magnitude;
        
        Vector2 normalDirection = direction.normalized;
        float slope = Mathf.Abs(normalDirection.y / normalDirection.x);
        float inverseSlope = 1f / slope;

        // Finds the smallest slope and gets the distance from (0, 0) to where that slope intersects 1 (can be x or y position)
        // Multiplies the magnitude by the distance found, the closer to a 45 degree angle the more warped it becomes
        return direction.magnitude * (inverseSlope <= slope ? Mathf.Sqrt(1f + inverseSlope * inverseSlope) : Mathf.Sqrt(1f + slope * slope));
    }

    void Jump()
    {
        localVelocity.y = Mathf.Sqrt(gravity * 2f * jumpHeight);
    }

    void AirMove()
    {
        Gravity();

        Accelerate(MoveDirection, baseSpeed * MoveMagnitude, baseAccel);

        LimitStrafe();

        AirControl(MoveDirection, baseSpeed * MoveMagnitude);
    }

    void AirControl(Vector3 direction, float speed)
    {
        /*if(input.MoveDirection.y == 0 || speed == 0)
            return;
        
        float angle = Vector2.Angle(input.MoveDirection, Vector2.up);
        float controlScale = 0;

        // Linearly increases scale from 0 to 1 from 40 degrees to 5 degrees
        if(angle < 40f)
            controlScale = Mathf.Min(-angle / 35f + 8f / 7f, 1f);
        else if(angle > 140f)
            controlScale = Mathf.Min((angle - 180f) / 35f + 8f / 7f, 1f);
        else
            return;*/

        if(MoveMagnitude == 0)
            return;

        float ySpeed = localVelocity.y;
        localVelocity.y = 0;
        float currentSpeed = localVelocity.magnitude;
        localVelocity = localVelocity.normalized;

        Vector2 inputDirection = input.MoveDirection.normalized;
        float cardinalPercent = (1f - Mathf.Cos(Mathf.PI * Mathf.Abs(Mathf.Abs(inputDirection.x) - Mathf.Abs(inputDirection.y)))) / 2f;
        float speedPercent = Mathf.Min(currentSpeed / baseSpeed, 1f);

        float controlScale = MoveMagnitude * cardinalPercent * Mathf.Pow(speedPercent, 2f / speedPercent);

        // FIX THIS SOME HOW
        float dot = Vector3.Dot(localVelocity, MoveDirection);
        float k = 32.0f;
        k = airControl * dot * dot * Time.fixedDeltaTime * controlScale;

        if(dot > 0)
        {
            localVelocity = localVelocity * speed + MoveDirection * k;
            localVelocity = localVelocity.normalized;
        }

        localVelocity *= currentSpeed;
        localVelocity.y = ySpeed;
    }

    void Accelerate(Vector3 direction, float moveSpeed, float moveAccel)
    {
        float currentSpeed = Vector3.Dot(direction, localVelocity);
        float addSpeed = moveSpeed - currentSpeed;
        if(addSpeed <= 0)
		    return;
        
        float accelSpeed = moveAccel * Time.fixedDeltaTime * moveSpeed;

        if(accelSpeed > addSpeed)
            accelSpeed = addSpeed;
        
        localVelocity += direction * accelSpeed;
    }

    void Friction()
    {
        float control = Speed < groundStopSpeed ? groundStopSpeed : Speed;
        float drop = control * friction * Time.fixedDeltaTime;

        float newSpeed = Speed - drop;
        if(newSpeed < 0)
            newSpeed = 0;
        if(newSpeed > 0)
            newSpeed /= Speed;
        
        localVelocity.x *= newSpeed;
        localVelocity.z *= newSpeed;
    }

    void Gravity()
    {
        localVelocity.y -= gravity * Time.fixedDeltaTime;
    }

    void LimitStrafe()
    {
        Vector2 flatVelocity = new Vector2(localVelocity.x, localVelocity.z);
        float newSpeed = flatVelocity.magnitude;
        if(newSpeed > Speed && newSpeed > baseSpeed)
        {
            float oldSpeed = Mathf.Max(Speed, baseSpeed);
            float maxAccel = baseSpeed / oldSpeed * Mathf.Sqrt(baseAccel);
            float accelScale = maxAccel / baseSpeed / 2f;

            if(isGrounded)
                newSpeed = oldSpeed + (newSpeed - oldSpeed) * accelScale;
            else
                newSpeed = oldSpeed + Sigmoid((newSpeed - oldSpeed) / Time.fixedDeltaTime / maxAccel * accelScale) * maxAccel * Time.fixedDeltaTime;

            flatVelocity = flatVelocity.normalized * newSpeed;

            localVelocity.x = flatVelocity.x;
            localVelocity.z = flatVelocity.y;
        }
    }
    
    float Sigmoid(float value)
    {
        // Modified sigmoid curve with origin set to 0 and max value set to 1
        return 2f / (1f + Mathf.Exp(-value)) - 1f;
    }
    
    ////////////////////////////

    void SlideMove()
    {
        /*Gravity();
        float slideFriction = 0;
        Vector3 normal = velocityRotation * groundNormal;
        localVelocity.x += (1f - normal.y) * normal.x * (1f - slideFriction);
        localVelocity.z += (1f - normal.y) * normal.z * (1f - slideFriction);*/

        Vector3 slopeParallel = Vector3.Cross(Vector3.Cross(Vector3.up, groundNormal), groundNormal).normalized;
        Quaternion slopeRotation = Quaternion.LookRotation(-slopeParallel);
        localVelocity = slopeRotation * localVelocity;
        Friction();
        Gravity();
        localVelocity = Quaternion.Inverse(slopeRotation) * localVelocity;
        //localVelocity = Quaternion.Inverse(velocityRotation) * localVelocity;

        localVelocity = slopeParallel * 10f;
        localVelocity += -groundNormal * 1f;

        //localVelocity += slopeParallel * gravity * Time.fixedDeltaTime;
    }
}
