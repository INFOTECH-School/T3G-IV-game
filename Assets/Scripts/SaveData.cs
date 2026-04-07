using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    public SerializableVector3 playerPosition;
    public string currentHeldItemId;
    public List<string> playedCutscenes;
    public List<ObjectiveData> objectives;
    public int levelProgress;
    public int currentLevel;
    public int level1DependencyScore;
    public int level2DependencyScore;
    public int truckDependencyScore;
    public Player.PlayerState playerState;
    public List<BasketData> basketsProgress;
    public List<BrokenWheelData> brokenWheelsProgress;
    public List<string> destroyedItems;
}

[Serializable]
public struct SerializableVector3
{
    public float x, y, z;

    public SerializableVector3(Vector3 vector3)
    {
        x = vector3.x;
        y = vector3.y;
        z = vector3.z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }
}

[Serializable]
public struct SerializableQuaternion
{
    public float x, y, z, w;

    public SerializableQuaternion(Quaternion quaternion)
    {
        x = quaternion.x;
        y = quaternion.y;
        z = quaternion.z;
        w = quaternion.w;
    }

    public Quaternion ToQuaternion()
    {
        return new Quaternion(x, y, z, w);
    }
}

[Serializable]
public class ObjectiveData
{
    public string id;
    public bool isCompleted;
    public SerializableVector3 position;
    public SerializableQuaternion rotation;
    
    public ObjectiveData(string id, bool isCompleted, SerializableVector3 position, SerializableQuaternion rotation)
    {
        this.id = id;
        this.isCompleted = isCompleted;
        this.position = position;
        this.rotation = rotation;
    }
}

[Serializable]
public class BasketData
{
    public string basketName;
    public int basketCounter;
    public bool holdingPoint1Active;
    public bool holdingPoint2Active;

    public BasketData(string basketName, int basketCounter, bool holdingPoint1Active, bool holdingPoint2Active)
    {
        this.basketName = basketName;
        this.basketCounter = basketCounter;
        this.holdingPoint1Active = holdingPoint1Active;
        this.holdingPoint2Active = holdingPoint2Active;
    }
}

[Serializable]
public class BrokenWheelData
{
    public string brokenWheelName;
    public bool brokenWheelFixed;

    public BrokenWheelData(string brokenWheelName, bool brokenWheelFixed)
    {
        this.brokenWheelName = brokenWheelName;
        this.brokenWheelFixed = brokenWheelFixed;
    }
}