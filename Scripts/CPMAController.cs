using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CharacterController))]
public class CPMAController : BaseController
{
    float baseSpeed = 10.0f;

    float groundAcceleration = 15f;
    float groundStopSpeed = 3f;

    float airAcceleration = 1.0f;
    float airStopAcceleration = 3.0f;
    float airStrafeSpeed = 1.0f;
    float airStrafeAcceleration = 70.0f;
    float airControl = 150.0f;

    float friction = 6.0f;
    float gravity = 20.0f;

    float jumpHeight = 1f;

    Vector2 inputDirection;

    Vector2 SnapDirection(Vector2 direction)
    {
        if(direction == Vector2.zero)
            return Vector2.zero;
        
        Vector2 snapDirection = direction.normalized;
        float angle = Vector3.Angle(snapDirection, Vector3.up);

        if(angle < 22.5f)
            return Vector2.up;
        if(angle > 157.5f)
            return Vector2.down;

        float t = Mathf.Round(angle / 45f);
        float deltaAngle = (t * 45f) - angle;

        Vector3 axis = Vector3.Cross(Vector3.up, snapDirection);
        Quaternion rotation = Quaternion.AngleAxis(deltaAngle, axis);

        snapDirection = rotation * snapDirection;

        if(snapDirection.x != 0)
            snapDirection.x = Mathf.Sign(snapDirection.x);
        if(snapDirection.y != 0)
            snapDirection.y = Mathf.Sign(snapDirection.y);
        
        return snapDirection;
    }

    protected override void Move()
    {
        // Snaps input to 45 degrees then locks it to -1, 0, and 1 per axis
        inputDirection = SnapDirection(input.MoveDirection);

        if(isGrounded)
        {
            localVelocity.y = 0f;
            GroundMove();
        } else
        {
            AirMove();
        }
    }

    void GroundMove()
    {
        if(input.JumpHeld)
        {
            Jump();
            return;
        }

        Gravity();
        Friction();

        Accelerate(MoveDirection, baseSpeed * MoveMagnitude, groundAcceleration);
    }

    void Jump()
    {
        localVelocity.y = Mathf.Sqrt(gravity * 2f * jumpHeight);
    }

    void AirMove()
    {
        float moveSpeed = baseSpeed;

        /* CPM START */
        float acceleration = airAcceleration;
        if(Vector3.Dot(localVelocity, MoveDirection) < 0)
            acceleration = airStopAcceleration;
        
        if(inputDirection.x != 0 && inputDirection.y == 0)
        {
            if(moveSpeed > airStrafeSpeed)
                moveSpeed = airStrafeSpeed;
            acceleration = airStrafeAcceleration;
        }
        /* CPM END */

        Gravity();
        Accelerate(MoveDirection, moveSpeed * MoveMagnitude, acceleration);

        /* CPM START */
        AirControl(MoveDirection, baseSpeed * MoveMagnitude);
        /* CPM END */
    }

    /* CPM START */
    void AirControl(Vector3 direction, float speed)
    {
        if(inputDirection.x != 0 || speed == 0)
            return;
        float ySpeed = localVelocity.y;
        localVelocity.y = 0;
        float currentSpeed = localVelocity.magnitude;
        localVelocity = localVelocity.normalized;

        float dot = Vector3.Dot(localVelocity, MoveDirection);
        float k = 32.0f;
        k *= airControl * dot * dot * Time.deltaTime;

        if(dot > 0)
        {
            localVelocity = localVelocity * speed + MoveDirection * k;
            localVelocity = localVelocity.normalized;
        }

        localVelocity *= currentSpeed;
        localVelocity.y = ySpeed;
    }
    /* CPM END */

    void Accelerate(Vector3 direction, float moveSpeed, float acceleration)
    {
        float currentSpeed = Vector3.Dot(direction, localVelocity);
        float addSpeed = moveSpeed - currentSpeed;
        if(addSpeed <= 0)
		    return;
        
        float accelerationSpeed = acceleration * Time.deltaTime * moveSpeed;

        if(accelerationSpeed > addSpeed)
            accelerationSpeed = addSpeed;
        
        localVelocity += direction * accelerationSpeed;
    }

    void Friction()
    {
        float control = Speed < groundStopSpeed ? groundStopSpeed : Speed;
        float drop = control * friction * Time.deltaTime;

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
        localVelocity.y -= gravity * Time.deltaTime;
    }
}