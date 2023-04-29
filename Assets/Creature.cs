using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Creature : MonoBehaviour
{
    [SerializeField] float surfaceVelocity;

    [SerializeField] Transform planet;

    [SerializeField] Vector3 sphericalCoord;

    Vector2 _input;

    private void OnMove(InputValue value)
    {
        _input = value.Get<Vector2>();
    }

    private void Update()
    {
        sphericalCoord += (Vector3)_input * Time.deltaTime * (surfaceVelocity);

        float x = Mathf.Sin(sphericalCoord.x) * Mathf.Sin(sphericalCoord.y) * sphericalCoord.z;
        float y = Mathf.Cos(sphericalCoord.y) * sphericalCoord.z;
        float z = Mathf.Cos(sphericalCoord.x) * Mathf.Sin(sphericalCoord.y) * sphericalCoord.z;

        this.transform.position = planet.TransformPoint(new Vector3(x, y, z));

        this.transform.LookAt(planet);
    }
}
