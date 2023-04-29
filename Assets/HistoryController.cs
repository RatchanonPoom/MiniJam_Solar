using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CreatureHistory
{
    public Vector3 bornPosition;
    public float bornTime;

    public AnimationCurve Xpos;
    public AnimationCurve Ypos;
    public AnimationCurve Zpos;

    public void SmoothRecordedCurve(float weight)
    {
        for (int i = 0; i < Xpos.length; i++)
        {
            Xpos.SmoothTangents(i, weight);
            Ypos.SmoothTangents(i, weight);
            Zpos.SmoothTangents(i, weight);
        }
    }

    public void RecordPosition(float time, Vector3 posRelatedToPlanet)
    {
        Xpos.AddKey(time, posRelatedToPlanet.x);
        Ypos.AddKey(time, posRelatedToPlanet.y);
        Zpos.AddKey(time, posRelatedToPlanet.z);
    }

    public Vector3 RecordedPosition(float time)
    {
        return new Vector3(Xpos.Evaluate(time), Ypos.Evaluate(time), Zpos.Evaluate(time));
    }
}

public class HistoryController : MonoBehaviour
{
    static HistoryController _historyController;
    public static HistoryController instance
    {
        get
        {
            return _historyController;
        }
    }

    public CreatureHistory tmp_currentCreature;
    [SerializeField] Creature controllableCreature;


    [SerializeField] float recordInterval = 1;

    //private CreatureHistory test_runningHistory;
    //private Transform test_ghost;
    //private float test_ghostCreatTime;

    [SerializeField] GameObject test_ghostTemplate;

    [SerializeField] Transform test_ghostPlanet;

    private void Awake()
    {
        if (_historyController == null)
        {
            _historyController = this;
            Debug.Log($"HistoryController is being assigned by this object \"{this.name}\"");
        }
        else
            Debug.LogWarning($"this gameobject \"{this.name}\" contains a duplicate instance of HistoryController");

        controllableCreature.InitializedCreature();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backslash))
        {
            tmp_currentCreature.SmoothRecordedCurve(0);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            CreateGhost(tmp_currentCreature);
        }

        //if (test_ghost != null)
        //{
        //    test_ghost.localPosition = test_runningHistory.RecordedPosition(Time.time - test_ghostCreatTime);
        //}
    }

    public void CreateGhost(CreatureHistory history)
    {
        //test_runningHistory = history;
        /*test_ghost = */
        Creature creature = GameObject.Instantiate(test_ghostTemplate, test_ghostPlanet).GetComponent<Creature>();

        creature.InitializedCreature(history);

        //test_ghost.localPosition = test_runningHistory.bornPosition;

        //test_ghostCreatTime = Time.time;
    }

    public void StartRecording(Creature creature)
    {
        controllableCreature = creature;

        StartCoroutine(RecordCurrentCreatureHistoryUntilExpire());
    }

    IEnumerator RecordCurrentCreatureHistoryUntilExpire()
    {
        while (true)
        {
            yield return new WaitForSeconds(recordInterval);
            tmp_currentCreature.RecordPosition(Time.time, controllableCreature.currentPositionInPlanetSpace);
        }
    }
}
