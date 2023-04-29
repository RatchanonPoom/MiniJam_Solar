using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Creature : MonoBehaviour
{
    [SerializeField] Transform planet;
    public Vector3 currentPositionInPlanetSpace;

    [SerializeField] bool isControllable;
    private float bornTime;
    private CreatureHistory history;

    //Auto Behaviour Parameter
    bool isAlive;
    [SerializeField] float surfaceVelocity;

    #region Input System Messages

    Vector2 _input;
    private void OnMove(InputValue value)
    {
        _input = value.Get<Vector2>();
    }
    #endregion

    #region Player Controlled Behaviour


    Vector3 sphericalCoord;
    private void MoveAlongSphericalCoord()
    {
        sphericalCoord += (Vector3)_input * Time.deltaTime * (surfaceVelocity);

        float x = Mathf.Sin(sphericalCoord.x) * Mathf.Sin(sphericalCoord.y) * sphericalCoord.z;
        float y = Mathf.Cos(sphericalCoord.y) * sphericalCoord.z;
        float z = Mathf.Cos(sphericalCoord.x) * Mathf.Sin(sphericalCoord.y) * sphericalCoord.z;

        this.transform.position = planet.TransformPoint(new Vector3(x, y, z));
    }

    Vector3 radiusAxis;
    Vector3 inputAxis;
    Vector3 lookup;

    /// <summary>
    /// move in orbit of the planet with an object space input
    /// </summary>
    private void MoveWithPlayerInput()
    {
        float radius = 40;
        float angularSpeed = surfaceVelocity / radius;
        float angularDisplacement = angularSpeed * _input.magnitude * Time.deltaTime;

        //determine coordinate system in relation of the planet with the current input
        radiusAxis = planet.InverseTransformDirection(this.transform.position - planet.position).normalized;
        inputAxis = planet.InverseTransformDirection(this.transform.TransformDirection(_input)).normalized; //always a tangent of the planet

        Vector3 supposedLocalPosition = (Mathf.Cos(angularDisplacement) * radius * radiusAxis) + (Mathf.Sin(angularDisplacement) * radius * inputAxis);

        lookup = Vector3.Cross(inputAxis, radiusAxis);


        //this.transform.position = planet.TransformPoint(currentPositionInPlanetSpace + displacement);
        this.transform.localPosition = supposedLocalPosition;

        currentPositionInPlanetSpace = this.transform.localPosition;
    }

    #endregion

    public void InitializedCreature()
    {
        isControllable = true;

        HistoryController.instance.StartRecording(this);

        BornCreature();
    }

    public void InitializedCreature(CreatureHistory history)
    {
        isControllable = false;

        this.history = history;

        this.transform.localPosition = history.bornPosition;

        BornCreature();
    }

    private void BornCreature()
    {
        bornTime = Time.time;
    }

    private void MoveWithRecording()
    {
        this.transform.localPosition = history.RecordedPosition(Time.time - bornTime);
    }

    private void Update()
    {
        if (isControllable)
            MoveWithPlayerInput();
        else
            MoveWithRecording();

        //align with planet surface
        this.transform.LookAt(planet, this.transform.TransformDirection(Vector3.up));
    }

    private void Expire()
    {

    }

    private void OnDrawGizmos()
    {
        float len = 20;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(planet.position, planet.position + radiusAxis * len);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(planet.position, planet.position + inputAxis * len);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(planet.position, planet.position + lookup * len);
    }
}
