using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class CheckpointController : NetworkBehaviour
{
    [SerializeField] GameObject[] checkpoints;
    GameObject startEndCheckpoint;
    GameObject lastCheckpoint;
    public int laps = 1;
    public int currLap = 1;
    public bool started = false;
    public bool ended = false;
    private HashSet<GameObject> CheckpointSet = new HashSet<GameObject>();

    const string START_END_TAG = "Checkpoint/Start-End";
    const string CHECKPOINT_TAG = "Checkpoint/Checkpoint";
    const string LAST_CHECKPOINT_TAG = "Checkpoint/Last";

    HashSet<string> checkpointTags = new HashSet<string>() { START_END_TAG, CHECKPOINT_TAG, LAST_CHECKPOINT_TAG };

    public override void OnNetworkSpawn() {
        startEndCheckpoint = GameObject.FindGameObjectWithTag(START_END_TAG);
        List<GameObject> checkpoints = GameObject.FindGameObjectsWithTag(CHECKPOINT_TAG).ToList();
        lastCheckpoint = GameObject.FindGameObjectWithTag(LAST_CHECKPOINT_TAG);
        checkpoints.Insert(0, startEndCheckpoint);
        checkpoints.Add(lastCheckpoint);
        this.checkpoints = checkpoints.ToArray();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (checkpointTags.Contains(other.tag))
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
