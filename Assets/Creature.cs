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

    private void MoveAlongSphericalCoord()
    {
        sphericalCoord += (Vector3)_input * Time.deltaTime * (surfaceVelocity);

        float x = Mathf.Sin(sphericalCoord.x) * Mathf.Sin(sphericalCoord.y) * sphericalCoord.z;
        float y = Mathf.Cos(sphericalCoord.y) * sphericalCoord.z;
        float z = Mathf.Cos(sphericalCoord.x) * Mathf.Sin(sphericalCoord.y) * sphericalCoord.z;

        this.transform.position = planet.TransformPoint(new Vector3(x, y, z));
    }

    float radius;
    float angularSpeed;
    float angularDisplacement;

    Vector3 radiusAxis;
    Vector3 inputAxis;

    Vector3 supposedPosition;
    Vector3 currentPositionInPlanetSpace;

    Vector3 lookup;

    float mag;

    [SerializeField] bool look = true;
    
    private void MoveAlongCameraCoord()
    {
        radius = 10;
        angularSpeed = surfaceVelocity / radius;
        angularDisplacement = angularSpeed * _input.magnitude * Time.deltaTime;

        mag = _input.magnitude;

        radiusAxis = planet.InverseTransformDirection(this.transform.position - planet.position).normalized;
        inputAxis = planet.InverseTransformDirection(this.transform.TransformDirection(_input)).normalized; //always a tangent of the planet

        supposedPosition = (Mathf.Cos(angularDisplacement) * radius * radiusAxis) + (Mathf.Sin(angularDisplacement) * radius * inputAxis);

        lookup = Vector3.Cross(inputAxis, radiusAxis);

        //currentPositionInPlanetSpace = planet.InverseTransformPoint(this.transform.position);

        //this.transform.position = planet.TransformPoint(currentPositionInPlanetSpace + displacement);
        this.transform.position = planet.TransformPoint(supposedPosition);
    }

    private void Update()
    {
        MoveAlongCameraCoord();

        if (look)
            this.transform.LookAt(planet, this.transform.TransformDirection(Vector3.up));
    }

    private void OnDrawGizmos()
    {
        float len = 20;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(planet.position, planet.position + radiusAxis*len);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(planet.position, planet.position + inputAxis*len);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(planet.position, planet.position + lookup*len);
    }
}
