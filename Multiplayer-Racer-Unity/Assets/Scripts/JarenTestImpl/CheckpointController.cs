using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointController : MonoBehaviour
{
    [SerializeField]  
    public GameObject startEndCheckpoint;
    [SerializeField]  
    public GameObject[] checkpoints;
    [SerializeField]
    public GameObject lastCheckpoint;
    public int laps;
    public int currLap;
    public bool started;
    public bool ended;
    private HashSet<GameObject> CheckpointSet = new HashSet<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        laps = 1;
        currLap = 1;
        ended = false;
    }

   

    private void OnTriggerEnter(Collider other)
    {
        

        if (other.CompareTag("checkpoint"))
        {
            UpdateCheckPointSet(other);
        }
    }
    /// <summary>
    ///  The UpdateCheckpointSet method verifies that each step of the race is valid by using a Set to track checkpoint inclusion,
    ///  as well as incrementing the lap and ending the race
    ///  This method assumes that only  bidirectional progression is possible on the track.
    /// </summary>
    /// <param name="other"> The collider of the checkpoint </param>
    private void UpdateCheckPointSet(Collider other)
    {
        GameObject checkpoint = other.gameObject;
        if (!ended)
        {

            if (CheckpointSet.Count == 0)
            {
                if (checkpoint == startEndCheckpoint)
                {
                    Debug.LogFormat("currlap: {0}", currLap);
                    CheckpointSet.Add(checkpoint);
                }
                else
                {
                    Debug.Log("You're going the wrong way!");
                }
            }
            else if (checkpoint == startEndCheckpoint)
            {
                if (CheckpointSet.Count == checkpoints.Length)
                {
                    currLap += 1;

                    CheckpointSet.Clear();
                    CheckpointSet.Add(checkpoint);
                }
                else
                {
                    Debug.Log("You're going the wrong way!");
                    CheckpointSet.Clear();

                }

            }
            else
            {
                if (checkpoint == lastCheckpoint)
                {
                    if (CheckpointSet.Count == checkpoints.Length - 1 || CheckpointSet.Count == checkpoints.Length)
                    {
                        CheckpointSet.Add(checkpoint);
                    }
                    else
                    {
                        Debug.Log("You're going the wrong way!");
                        CheckpointSet.Clear();

                    }
                }
                else
                {

                    if (CheckpointSet.Contains(lastCheckpoint))
                    {
                        CheckpointSet.Remove(lastCheckpoint);
                    }
                    CheckpointSet.Add(checkpoint);
                }
            }

            if (currLap > laps)
            {
                Debug.Log("Race is over!");
                ended = true;
                return;
            }
            Debug.Log($"checkpoints visited: {CheckpointSet.Count}");
        }

    }
}
