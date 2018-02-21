using UnityEngine;

public class CheckpointTrigger : MonoBehaviour {
    private int IdCheckpoint;
    // [TODO] instead of ranking by checkpoints, rank by time to reach checkpoint.
    // so have the times stored in an array then check array1[i]>array2[i].
    private void OnTriggerEnter2D(Collider2D collision)
    {
        var carScript = collision.GetComponent<CarController>();
        if (!carScript.isCarDead() &&
            (carScript.getCheckpoint()< IdCheckpoint // Only count the trigger if the car is still active and the triggered point is new
                ))//|| (carScript.getCheckpoint()>IdCheckpoint && IdCheckpoint==0))) // Or only count the trigger if it is a lap. Note: It's possible that the car can alternate between the start and the end point for maximum points.
        {
            if (carScript.getCheckpoint() > IdCheckpoint && IdCheckpoint == 0) // car made a lap (or is going back and forth)
            {
                carScript.setAsFinishedLap();
                carScript.setCarAsDead(); // car finished a lap
            }
            else
            {
                carScript.setCheckpoint(IdCheckpoint);
                carScript.resetTimer();
            }
        }
    }

    public void setIdCheckpoint(int id)
    {
        this.IdCheckpoint = id;
    }

    public int getIdCheckpoint()
    {
        return (this.IdCheckpoint);
    }
}
