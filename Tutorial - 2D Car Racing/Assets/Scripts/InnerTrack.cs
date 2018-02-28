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
        var nodes = GetComponentsInChildren<Transform>();
        foreach (var node in nodes)
        {
            if (node != transform)
            {
                int thisCheckpointId = checkpoints.Count;
                checkpoints.Add(new Checkpoint(node, thisCheckpointId));
                var script = node.GetComponent<CheckpointTrigger>();
                script.setIdCheckpoint(thisCheckpointId);
                script.setMaxIdCheckpoint(nodes.Length - 2); // minus 1 because nodes needs to exclude itself, and another minus 1 to correct the array indexing to be 0-based
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
