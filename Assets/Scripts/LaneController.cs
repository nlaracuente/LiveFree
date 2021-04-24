using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneController : Singleton<LaneController>
{
    List<Transform> lanes;
    LaneId[] laneIds;
    Queue<LaneId> randomLaneIds;

    private void Start()
    { 
        lanes = new List<Transform>();
        for(var i = 0; i < transform.childCount; i++)
        {
            var lane = transform.GetChild(i);
            if (lane != null)
                lanes.Add(lane);
        }
        laneIds = Enum.GetValues(typeof(LaneId)).Cast<LaneId>().ToArray();
    }

    public LaneId ChangeLane(LaneId fromLaneId, int direction)
    {
        var index = (int)fromLaneId;
        index = Mathf.Clamp(index + direction, 0, lanes.Count - 1);
        return (LaneId)index;
    }

    public Transform GetLaneById(LaneId laneId)
    {
        var laneNumber = (int)laneId;
        var index = Mathf.Clamp(laneNumber, 0, lanes.Count - 1);
        return lanes[index];
    }

    public LaneId GetRandomLaneId()
    {
        if(randomLaneIds == null || randomLaneIds.Count < 1)
        {
            var randIds = Utility.RandomizeArray(laneIds);
            randomLaneIds = new Queue<LaneId>(randIds);
        }

        return randomLaneIds.Dequeue();
    }
}

public enum LaneId
{
    LeftLane,
    MiddleLane,
    RightLane
}
