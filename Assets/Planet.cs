using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    [SerializeField] float gravity;
    [SerializeField] Vector3 rotateAxis = Vector3.up;
    [SerializeField] float dayLength;

    Rigidbody[] affectedBodies;

    private void Awake()
    {
        affectedBodies = GetComponentsInChildren<Rigidbody>();
    }

    private void FixedUpdate()
    {
        transform.Rotate(rotateAxis, 2 * Mathf.PI / dayLength);

        foreach(Rigidbody body in affectedBodies)
        {
            Vector3 direction = body.transform.position - this.transform.position;

            body.AddForce(body.mass * gravity * direction * Time.fixedDeltaTime);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        float axisDrawLength = 100;
        Gizmos.DrawLine(this.transform.position + rotateAxis.normalized * axisDrawLength * .5f, this.transform.position - rotateAxis.normalized * axisDrawLength * .5f);
    }
}
