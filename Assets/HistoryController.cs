using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
public struct CreatureHistory
{
    [System.Serializable]
    public struct CreatureActionLogEntry
    {
        public float actionTime;
        public int actionType;

        public CreatureActionLogEntry(float time, int type)
        {
            actionTime = time;
            actionType = type;
        }
    }

    public Vector3 bornPosition;
    public float bornTime;

    public AnimationCurve Xpos;
    public AnimationCurve Ypos;
    public AnimationCurve Zpos;

    public List<CreatureActionLogEntry> actionLog;

    public CreatureHistory(Creature origin)
    {
        this.bornPosition = origin.currentPositionInPlanetSpace;
        this.bornTime = Time.time;

        Xpos = new AnimationCurve();
        Ypos = new AnimationCurve();
        Zpos = new AnimationCurve();

        actionLog = new List<CreatureActionLogEntry>(); 
    }

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

    public void RecordAction(float time, int type)
    {
        actionLog.Add(new CreatureActionLogEntry(time, type));
    }

    public Queue<CreatureActionLogEntry> GetActionQueue()
    {
        Queue<CreatureActionLogEntry> queue = new Queue<CreatureActionLogEntry>();

        foreach (var action in actionLog)
        {
            queue.Enqueue(action);
        }

        return queue;
    }

    public Vector3 GetRecordedPosition(float time)
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

    private int _cycleCount;

    [SerializeField] private List<CreatureHistory> creaturesHistory;

    [SerializeField] float recordInterval = 1;

    [SerializeField] private CreatureHistory tmp_currentCreatureHistory;
    [SerializeField] private Creature controllableCreature;
    [SerializeField] Vector3 nextControllableBornPos;

    [SerializeField] private List<Creature> tmp_currentNonControllableCreatures;


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

        _cycleCount = 0;

        creaturesHistory = new List<CreatureHistory>();
    }

    private void Start()
    {
        BeginCycle();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backslash))
        {
            tmp_currentCreatureHistory.SmoothRecordedCurve(0);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            BeginCycle();
        }

        //if (test_ghost != null)
        //{
        //    test_ghost.localPosition = test_runningHistory.RecordedPosition(Time.time - test_ghostCreatTime);
        //}
    }

    private void BeginCycle()
    {
        Debug.Log("A NEW CYCLE BEGINS");

        BroadcastMessage("CycleStart", _cycleCount, SendMessageOptions.DontRequireReceiver);

        //Create ghosts
        tmp_currentNonControllableCreatures = new List<Creature>();
        foreach (var pastCreature in creaturesHistory)
        {
            tmp_currentNonControllableCreatures.Add(CreateGhost(pastCreature));
        }

        //Craete new controllable creature
        controllableCreature = CreateControllableCreature(nextControllableBornPos);
    }

    Coroutine routine_restartCycle;
    public void EndCycle()
    {
        StopRecording();

        foreach (var creature in tmp_currentNonControllableCreatures)
        {
            creature.ForceExpire();
        }

        //tmp_record last position
        //nextControllableBornPos = controllableCreature.currentPositionInPlanetSpace;

        controllableCreature = null;

        routine_restartCycle = StartCoroutine(RoutineRestartCycle());
    }

    private IEnumerator RoutineRestartCycle()
    {
        Debug.Log("THE CYCLE HAS COME TO A PREMATURE END");

        yield return new WaitForSeconds(3);

        BeginCycle();
    }

    private Creature CreateControllableCreature(Vector3 bornPos)
    {
        Creature creature = GameObject.Instantiate(test_ghostTemplate, test_ghostPlanet).GetComponent<Creature>();

        creature.transform.position = test_ghostPlanet.TransformPoint(bornPos);

        creature.InitializedCreature();

        creature.gameObject.SetActive(true);

        Debug.Log($"a controllable creature {creature.name} has been recreated");

        return creature;
    }

    private Creature CreateGhost(CreatureHistory history)
    {
        //test_runningHistory = history;
        /*test_ghost = */
        Creature creature = GameObject.Instantiate(test_ghostTemplate, test_ghostPlanet).GetComponent<Creature>();

        creature.InitializedCreature(history);

        creature.gameObject.SetActive(true);

        //test_ghost.localPosition = test_runningHistory.bornPosition;

        //test_ghostCreatTime = Time.time;
        Debug.Log($"creature {creature.name} has been recreated");

        return creature;
    }

    Coroutine routine_creatureHistoryRecording;

    /// <summary>
    /// Start position recording for the currently controlled creature
    /// </summary>
    /// <param name="creature"></param>
    public void StartRecording(Creature creature)
    {
        controllableCreature = creature;

        //record history
        tmp_currentCreatureHistory = new CreatureHistory(creature);
        routine_creatureHistoryRecording = StartCoroutine(RecordCurrentCreatureHistoryUntilExpire());
    }

    /// <summary>
    /// Record an action into the currently controlled creature history
    /// </summary>
    public void RecordCurrentCreatureAction(int type)
    {
        tmp_currentCreatureHistory.RecordAction(Time.time - controllableCreature.bornTime, type);
    }

    IEnumerator RecordCurrentCreatureHistoryUntilExpire()
    {
        float timeAtStartOfRecording = controllableCreature.bornTime;

        while (true)
        {
            yield return new WaitForSeconds(recordInterval);
            yield return new WaitForFixedUpdate();
            tmp_currentCreatureHistory.RecordPosition(Time.time - timeAtStartOfRecording, controllableCreature.currentPositionInPlanetSpace);
        }
    }

    private void StopRecording()
    {
        StopCoroutine(routine_creatureHistoryRecording);

        //add to world history
        creaturesHistory.Add(tmp_currentCreatureHistory);
    }
}
