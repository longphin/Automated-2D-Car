using UnityEngine;

public class CheckpointTrigger : MonoBehaviour {
    private int IdCheckpoint;
    private int maxIdCheckpoint;
    // [TODO] instead of ranking by checkpoints, rank by time to reach checkpoint.
    // so have the times stored in an array then check array1[i]>array2[i].
    private void OnTriggerEnter2D(Collider2D collision)
    {
        var carScript = collision.GetComponent<CarController>();
        if (!carScript.isCarDead()
            && carScript.getCheckpoint()< IdCheckpoint // Only count the trigger if the car is still active and the triggered point is new
            && IdCheckpoint-carScript.getCheckpoint() == 1)//|| (carScript.getCheckpoint()>IdCheckpoint && IdCheckpoint==0))) // Or only count the trigger if it is a lap. Note: It's possible that the car can alternate between the start and the end point for maximum points.
        {
                carScript.setCheckpoint(IdCheckpoint);
                carScript.resetTimer();
        }else
        if (!carScript.isCarDead() &&
            carScript.getCheckpoint() == maxIdCheckpoint && IdCheckpoint == 0) // car made a lap (or is going back and forth)
        {
            Debug.Log("car finished lap");
            carScript.setAsFinishedLap();
            carScript.setCarAsDead(); // car finished a lap
        }
    }

    public void setMaxIdCheckpoint(int max)
    {
        maxIdCheckpoint = max;
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
