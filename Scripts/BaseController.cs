using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public abstract class BaseController : MonoBehaviour
{
    CharacterController _controller = null;
    public CharacterController Controller
    {
        get
        {
            if(_controller == null)
                _controller = transform.GetComponent<CharacterController>();
            return _controller;
        }
    }

    [SerializeField] new Camera camera = null;
    
    public float MoveMagnitude { get; private set; }
    public Vector3 MoveDirection { get; private set; }
    
    public Vector3 Velocity { get; private set; }
    protected Vector3 localVelocity;

    public float Speed { get; private set; }
    public float Accel { get; private set; }

    void Update()
    {
        Input();
        Look(input.LookDirection);
    }

    void FixedUpdate()
    {
        MoveMagnitude = input.MoveDirection.magnitude;
        if(MoveMagnitude > 0)
            MoveDirection = playerRotation * new Vector3(input.MoveDirection.x, 0, input.MoveDirection.y).normalized;
        else
            MoveDirection = Vector3.zero;
        
        // Lerps local Velocity to actual Velocity
        localVelocity = Vector3.Lerp(Velocity, transform.InverseTransformDirection(Controller.velocity), Time.fixedDeltaTime * 4f);

        // Undos ground normal aligning
        localVelocity = Quaternion.Inverse(Quaternion.LookRotation(Vector3.forward, groundNormal)) * localVelocity;

        CheckGround();
        Move();

        float newSpeed = new Vector2(localVelocity.x, localVelocity.z).magnitude;
        Accel = (newSpeed - Speed) / Time.fixedDeltaTime;
        Speed = newSpeed;

        // Applies and aligns Velocity to ground normal
        Velocity = Quaternion.LookRotation(Vector3.forward, groundNormal) * localVelocity;

        camera.transform.localRotation = cameraRotation;
        Controller.Move(Velocity * Time.fixedDeltaTime);
    }

    protected abstract void Move();

    void OnGUI()
    {
        GUIStyle textStyle = new GUIStyle();
        textStyle.fontSize = 32;
        textStyle.fontStyle = FontStyle.Bold;
        textStyle.normal.textColor = Color.white;

        GUI.Label(new Rect(10, 10, 100, 20), "Speed : " + ((int)(Speed * 32f)).ToString() + "\nAccel : " + ((int)(Accel * 32f)).ToString(), textStyle);
    }

    ////////////////////////////

    EventHelper inputUpdate = new EventHelper();
    PlayerInput _input = null;
    public PlayerInput input
    {
        get
        {
            if(_input == null)
                _input = new PlayerInput(inputUpdate);
            return _input;
        }
    }

    void Input()
    {
        Cursor.lockState = CursorLockMode.Locked;

        if(_input == null)
            _input = new PlayerInput(inputUpdate);
        inputUpdate.Invoke();
    }

    ////////////////////////////

    public Quaternion cameraRotation { get; private set; }
    public Quaternion playerRotation { get; private set; }

    protected void Look(Vector2 lookDirection, float lookTilt = 0, float tiltTime = 0.25f)
    {
        Vector3 eulerAngles = cameraRotation.eulerAngles;

        // Wrap X and Y Axises around for easier use
        eulerAngles.x -= Mathf.Floor((eulerAngles.x + 180f) / 360f) * 360f; 
        eulerAngles.z -= Mathf.Floor((eulerAngles.z + 180f) / 360f) * 360f;

        // Applies look input while clamping vertical rotation
        eulerAngles.x = Mathf.Clamp(eulerAngles.x - lookDirection.y, -90f, 90f); 

        eulerAngles.y += lookDirection.x;
        
        // Tilts camera over time given
        eulerAngles.z = Mathf.Lerp(eulerAngles.z, lookTilt, Time.deltaTime / tiltTime);

        cameraRotation = Quaternion.Euler(eulerAngles);
        playerRotation = Quaternion.Euler(0, eulerAngles.y, 0);
    }

    ////////////////////////////

    public bool isGrounded { get; private set; }
    public Vector3 groundNormal { get; private set; }

    protected void CheckGround()
    {
        isGrounded = false;
        Controller.enabled = false;

        //Get single collider
        if(Physics.SphereCast(transform.position, Controller.radius, Vector3.down, out var raycastHit, Controller.height / 2f - Controller.radius + 0.1f))
        {
            groundNormal = raycastHit.normal;
            float angle = Vector3.Angle(Vector3.up, groundNormal);
            isGrounded = angle < Controller.slopeLimit - 0.1f && !Mathf.Approximately(angle, Controller.slopeLimit - 0.1f);
        } else
        {
            groundNormal = Vector3.up;
        }

        //Get all colliders
        /*RaycastHit[] raycastHits = Physics.SphereCastAll(transform.position, Controller.radius, Vector3.down, Controller.height / 2f - Controller.radius + 0.1f);
        if(raycastHits.Length > 0)
        {
            groundNormal = Vector3.zero;
            foreach(RaycastHit raycastHit in raycastHits)
                groundNormal += raycastHit.normal;
            groundNormal /= raycastHits.Length;

            float angle = Vector3.Angle(Vector3.up, groundNormal);
            isGrounded = angle < Controller.slopeLimit - 0.1f && !Mathf.Approximately(angle, Controller.slopeLimit - 0.1f);
        } else
        {
            groundNormal = Vector3.up;
        }*/

        Controller.enabled = true;

        /*Debug.DrawRay(transform.position + cameraRotation * Vector3.forward, groundNormal, Color.red, 0, false);
        Vector3 perpNormal = Vector3.Cross(Vector3.Cross(Vector3.up, groundNormal), groundNormal).normalized;
        Debug.DrawRay(transform.position + cameraRotation * Vector3.forward, perpNormal, Color.blue, 0, false);*/

        Controller.enabled = true;
    }
}
