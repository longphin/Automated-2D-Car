using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointTrigger : MonoBehaviour {
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log("triggered");
        //other.GetComponent<CarController>().AddCheckPoint();
        int checkpointId = CarsControllerHelper.GetCheckpointId(transform);

        collision.GetComponent<CarController>().setCheckpoint(checkpointId);
    }
}
