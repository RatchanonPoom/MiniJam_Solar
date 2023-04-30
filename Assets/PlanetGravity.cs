using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetGravity : MonoBehaviour
{
    [SerializeField] float gravity;

    Rigidbody[] affectedBodies;

    private void Awake()
    {
        affectedBodies = GetComponentsInChildren<Rigidbody>();
    }

    private void FixedUpdate()
    {
        foreach(Rigidbody body in affectedBodies)
        {
            Vector3 direction = body.transform.position - this.transform.position;

            body.AddForce(body.mass * gravity * direction * Time.fixedDeltaTime);
        }
    }
}
