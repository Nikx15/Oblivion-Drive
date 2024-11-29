using System;
using System.Collections;
using UnityEngine;

public class PlayerPhysics : MonoBehaviour
{
    public Rigidbody rb;

    public LayerMask layerMask;
    public Vector3 horizontalVelocity => Vector3.ProjectOnPlane(rb.velocity, rb.transform.up);
    public Vector3 verticalVelocity => Vector3.Project(rb.velocity, rb.transform.up);

    public float verticalSpeed => Vector3.Dot(rb.velocity, rb.transform.up);

    public Action onPlayerPhysicsUpdate;

    public float speed => horizontalVelocity.magnitude;

    private void FixedUpdate()
    {
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

    [SerializeField] float gravity;

    void Gravity()
    {
        rb.velocity -= Vector3.up * gravity * Time.deltaTime;
    }

    private void LateFixedUpdate()
    {
        Ground();

        Snap();

        if (groundInfo.ground)
            rb.velocity = horizontalVelocity;
    }

    [SerializeField] float groundDistance;

    public struct GroundInfo
    {
        public Vector3 point;

        public Vector3 normal;

        public bool ground;
    }

    [HideInInspector] public GroundInfo groundInfo;

    public Action onGroundEnter;

    public Action onGroundExit;

    void Ground()
    {
        float maxDistance = Mathf.Max(rb.centerOfMass.y, 0) + (rb.sleepThreshold * Time.fixedDeltaTime);

        if (groundInfo.ground && verticalSpeed < rb.sleepThreshold)
            maxDistance += groundDistance;

        bool ground = Physics.Raycast(rb.worldCenterOfMass, -rb.transform.up, out RaycastHit hit, maxDistance, layerMask, QueryTriggerInteraction.Ignore);

        Vector3 point = ground ? hit.point : rb.transform.position;

        Vector3 normal = ground ? hit.normal : Vector3.up;

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
        };
    }

    void Snap()
    {
        rb.transform.up = groundInfo.normal;

        Vector3 goal = groundInfo.point;

        Vector3 difference = goal - rb.transform.position;

        if (rb.SweepTest(difference, out _, difference.magnitude, QueryTriggerInteraction.Ignore)) return;

        rb.transform.position = goal;
    }
}
