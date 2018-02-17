using System.Collections.Generic;
using UnityEngine;

public class Checkpoint
{
    private int IdCheckpoint;
    private Transform transform;

    public Checkpoint(Transform transform, int id)
    {
        this.transform = transform;
        this.IdCheckpoint = id;
    }

    public int getId()
    {
        return (IdCheckpoint);
    }

    public Vector2 getPosition()
    {
        return (transform.position);
    }
}

public class InnerTrack : MonoBehaviour
{
    private List<Checkpoint> checkpoints = new List<Checkpoint>();

    // Use this for initialization
    void Start()
    {
        // Get a list of checkpoints
        foreach (var node in GetComponentsInChildren<Transform>())
        {
            if (node != transform)
            {
                int thisCheckpointId = checkpoints.Count;
                checkpoints.Add(new Checkpoint(node, thisCheckpointId));
                node.GetComponent<CheckpointTrigger>().setIdCheckpoint(thisCheckpointId);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    public float getDistanceToNextCheckpoint(Vector2 position, int currentIdCheckpoint)
    {
        if (currentIdCheckpoint + 1 < checkpoints.Count) // The next checkpoint is valid
        {
            return (Vector2.Distance(position, checkpoints[currentIdCheckpoint + 1].getPosition()));
        }
        // Else, the next checkpoint is the starting one
        return (Vector2.Distance(position, checkpoints[0].getPosition()));
    }

    public int getCheckpointCount()
    {
        return (checkpoints.Count);
    }
}
