using System;
using System.Collections;
using System.Drawing;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerPhysics : MonoBehaviour
{
    public Rigidbody rb;

    public LayerMask layerMask;
    public Vector3 horizontalVelocity => Vector3.ProjectOnPlane(rb.velocity, rb.transform.up);
    public Vector3 verticalVelocity => Vector3.Project(rb.velocity, rb.transform.up);

    public float verticalSpeed => Vector3.Dot(rb.velocity, rb.transform.up);

    public Action onPlayerPhysicsUpdate;

    public float speed => horizontalVelocity.magnitude;

    public float gravityFactor = 50f;
    public float angle;

    public bool inAir = false;

    public MovementState state;
    public enum MovementState
    {
        idle,
        move,
        air
    }

    private void Start()
    {
        state = MovementState.idle;
    }

    private void FixedUpdate()
    {
        StateHandler();

        onPlayerPhysicsUpdate?.Invoke();

        if (!groundInfo.ground)
            Gravity();

        if (groundInfo.ground && verticalSpeed < rb.sleepThreshold)
            rb.velocity = horizontalVelocity;

        StartCoroutine(LateFixedUpdateRoutine());

        IEnumerator LateFixedUpdateRoutine()
        {
            yield return new WaitForFixedUpdate();

            LateFixedUpdate();
        }
    }

    private void StateHandler()
    {
        if (speed == 0 && groundInfo.ground)
        {
            state = MovementState.idle;
            Debug.Log("Idle");
        }
            
        else if (speed >= 1 && groundInfo.ground)
        {
            state = MovementState.move;
            Debug.Log("Move");
        }

        else
        {
            state = MovementState.air;
            Debug.Log("Air");
        }
    }

    [SerializeField] float gravity;

    void Gravity()
    {
        rb.velocity -= Vector3.up * gravity * Time.deltaTime;
    }

    private void LateFixedUpdate()
    {
        inAir = false;
        if (!inAir)
        {
            Ground();
        }
        else if (inAir)
        {
            groundInfo.ground = false;
            Fall();
        }

        if (!inAir && groundInfo.ground)
        {
            Snap();
        }

        if (!groundInfo.ground)
        {
            //Debug.Log("in air!");
            inAir = true;
            // rb.transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, 0f, 0f), 2f * Time.deltaTime);
            rb.transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, 0f, 0f), 2f * speed / 10 * Time.deltaTime);
        }

        if (groundInfo.ground)
        {
            rb.velocity = horizontalVelocity;
        }
    }

    [SerializeField] float groundDistance;

    public struct GroundInfo
    {
        public Vector3 point;

        public Vector3 normal;

        public bool ground;

        public float angle;
    }

    [HideInInspector] public GroundInfo groundInfo;

    public Action onGroundEnter;

    public Action onGroundExit;

    RaycastHit Ground()
    {
        //Debug.Log("Yum, Ground");
        float maxDistance = Mathf.Max(rb.centerOfMass.y, 0) + (rb.sleepThreshold * Time.fixedDeltaTime);

        if (groundInfo.ground && verticalSpeed < rb.sleepThreshold)
            maxDistance += groundDistance;

        bool ground = Physics.Raycast(rb.worldCenterOfMass, -rb.transform.up, out RaycastHit hit, maxDistance, layerMask, QueryTriggerInteraction.Ignore);

        Vector3 point = ground ? hit.point : rb.transform.position;

        Vector3 normal = ground ? hit.normal : Vector3.up;

        angle = Vector3.Angle(point, normal);


        if (speed < 20 && groundInfo.ground == true)
        {
            if (angle < 45 && angle > 0 || angle < 180 && angle > 135)
            {
                ground = false;
                inAir = true;
                Debug.Log("I got kicked!");
            }
        }

        if (ground != groundInfo.ground)
        {
            if (ground)
                onGroundEnter?.Invoke();
            else
                onGroundExit?.Invoke();
        }

        groundInfo = new()
        {
            point = point,
            normal = normal,
            ground = ground,
            angle = angle,
        };

        return hit;
    }

    void Fall()
    {
        StartCoroutine(SelfRight());
        Debug.Log("IM FALLING");
        rb.AddRelativeForce(Vector3.down * gravityFactor);
        groundInfo.ground = false;
    }

    void Snap()
    {
        rb.transform.up = groundInfo.normal;

        Vector3 goal = groundInfo.point;

        Vector3 difference = goal - rb.transform.position;

        if (rb.SweepTest(difference, out _, difference.magnitude, QueryTriggerInteraction.Ignore)) return;

        rb.transform.position = goal;
    }

    IEnumerator SelfRight()
    {
        while (transform.up.y != Vector3.up.y)
        {
            Vector3 myUp = new Vector3(0, transform.up.y, 0);
            float delta = Vector3.SignedAngle(myUp, Vector3.up, Vector3.up);
            Vector3 upDelta = new Vector3(0, delta, 0);
            transform.Rotate(new Vector3(0, delta * .1f, 0));
            yield return new WaitForEndOfFrame();
        }
    }
}