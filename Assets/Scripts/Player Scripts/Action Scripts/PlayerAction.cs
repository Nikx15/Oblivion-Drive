using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerAction : MonoBehaviour
{
    [SerializeField] protected PlayerPhysics PlayerPhysics;

    protected Rigidbody rb => PlayerPhysics.rb;

    protected PlayerPhysics.GroundInfo groundInfo => PlayerPhysics.groundInfo;
}
