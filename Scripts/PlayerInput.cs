using UnityEngine;

/* Placeholder Class, revamp to be more modular and utilize Rewired */
public class PlayerInput
{
    EventHelper updateHelper;

    public Vector2 MoveDirection { get; private set; }
    public Vector2 LookDirection { get; private set; }

    public bool JumpPress { get; private set; }
    public bool JumpHeld { get; private set; }

    public bool PrimaryPress { get; private set; }
    public bool PrimaryHeld { get; private set; }

    public bool SecondaryPress { get; private set; }
    public bool SecondaryHeld { get; private set; }

    Vector2 mouseSensitivity = new Vector2(4f, 4f);

    bool useGamepad = true;
    Vector2 stickSensitivity = new Vector2(5, 5);
    Vector2 stickDeadzone = new Vector2(0.1f, 0.1f);

    public PlayerInput(EventHelper updateHelper)
    {
        this.updateHelper = updateHelper;
        this.updateHelper.Add(Update);
    }

    void Update()
    {
        MoveUpdate();
        LookUpdate();
    }

    void MoveUpdate()
    {
        bool Jump = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.JoystickButton4);
        JumpPress = Jump && !JumpHeld;
        JumpHeld = Jump;

        Vector2 keyDirection = new Vector2();

        if(Input.GetKey(KeyCode.W))
            keyDirection.y++;
        if(Input.GetKey(KeyCode.A))
            keyDirection.x--;
        if(Input.GetKey(KeyCode.S))
            keyDirection.y--;
        if(Input.GetKey(KeyCode.D))
            keyDirection.x++;

        MoveDirection = keyDirection.normalized;

        if(Input.GetKey(KeyCode.LeftShift))
            MoveDirection *= 0.5f;

        if(useGamepad)
        {
            Vector2 stickDirection = new Vector2(Input.GetAxisRaw("Joy1Axis1"), -Input.GetAxisRaw("Joy1Axis2"));
            
            stickDirection.x = stickDirection.x != 0 && Mathf.Abs(stickDirection.x) >= stickDeadzone.x ? stickDirection.x : 0;
            stickDirection.y = stickDirection.y != 0 && Mathf.Abs(stickDirection.y) >= stickDeadzone.y ? stickDirection.y : 0;

            stickDirection = Vector2.ClampMagnitude(stickDirection, 1f);

            MoveDirection = Vector2.ClampMagnitude(MoveDirection + stickDirection, 1f);
        }
    }

    void LookUpdate()
    {
        /*  Mouse Axis returns delta movement at a scale of 1/20 per pixel, so it's multiplied by 20 for pixel movement.
            Intended pixels per degree is 0.022, so pixel movement is multiplied by it.
            Multiplication has been simplified for optimization */
        
        Vector2 mouseDirection = new Vector2();

        mouseDirection.x = Input.GetAxisRaw("Mouse X") * 0.44f * mouseSensitivity.x;
        mouseDirection.y = Input.GetAxisRaw("Mouse Y") * 0.44f * mouseSensitivity.y;

        LookDirection = mouseDirection;

        /*  Stick Axis returns how far it's tilted, with a max value of 1.
            Intended base sensitivity is 60 degrees per second, so stick axis is multiplied by the relation of time
            between frames and 1/60th of a second. */

        if(useGamepad)
        {
            Vector2 stickDirection = new Vector2();
            
            float timeDeltaScale = Time.unscaledDeltaTime / (1f / 60f);

            stickDirection.x = Input.GetAxisRaw("Joy1Axis4") * timeDeltaScale * stickSensitivity.x;
            stickDirection.y = -Input.GetAxisRaw("Joy1Axis5") * timeDeltaScale * stickSensitivity.y;

            stickDirection.x = stickDirection.x != 0 && Mathf.Abs(stickDirection.x) >= stickDeadzone.x ? stickDirection.x : 0;
            stickDirection.y = stickDirection.y != 0 && Mathf.Abs(stickDirection.y) >= stickDeadzone.y ? stickDirection.y : 0;

            LookDirection += stickDirection;
        }
    }
}
