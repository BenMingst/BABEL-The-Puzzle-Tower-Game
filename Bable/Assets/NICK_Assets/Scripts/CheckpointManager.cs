using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CheckpointState
{
    public int checkpointIndex;
    public List<string> destroyedDoors = new List<string>();
    public List<string> openedLockedDoors = new List<string>();
    public List<string> meltedIceWalls = new List<string>();
    public List<string> openedEntranceDoors = new List<string>();
    public List<string> openedExitDoors = new List<string>();
    public List<string> openedChests = new List<string>();
    public List<string> deadEnemies = new List<string>();
    public List<string> openedEnemyDoors = new List<string>();
    public bool hasBow;
    public bool hasBomb;
    public bool hasFireArrows;
    public bool hasIceArrows;
    public int keyCount;
}

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance;

    public CheckpointState savedState = new CheckpointState();

    void Awake()
    {
        Debug.Log("CheckpointManager Awake - existing instance: " + (Instance != null));
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SaveState(int checkpointIndex)
    {
        Debug.Log("SaveState called - checkpoint: " + checkpointIndex + " hasBow: " + GameManager.hasBow + " chests found: " + FindObjectsOfType<Chest>().Length);

        savedState = new CheckpointState();
        savedState.checkpointIndex = checkpointIndex;

        foreach (var door in FindObjectsOfType<DoorHealth>())
            if (door.isDestroyed)
                savedState.destroyedDoors.Add(door.persistentID);

        foreach (var door in FindObjectsOfType<LockedDoor>())
            if (door.isUnlocked)
                savedState.openedLockedDoors.Add(door.persistentID);

        foreach (var wall in FindObjectsOfType<IceWall>())
            if (wall.currentState == IceWall.IceWallState.Melted)
                savedState.meltedIceWalls.Add(wall.persistentID);

        foreach (var door in FindObjectsOfType<OneWayDoorEntrance>())
            if (door.hasBeenOpened)
                savedState.openedEntranceDoors.Add(door.persistentID);

        foreach (var door in FindObjectsOfType<OneWayDoorExit>())
            if (door.hasBeenOpened)
                savedState.openedExitDoors.Add(door.persistentID);

        foreach (var chest in FindObjectsOfType<Chest>())
            if (chest.isOpened)
                savedState.openedChests.Add(chest.persistentID);

        foreach (var enemy in FindObjectsOfType<PermanentDeathEnemy>())
            if (enemy.isDead)
                savedState.deadEnemies.Add(enemy.persistentID);

        foreach (var door in FindObjectsOfType<EnemyDoor>())
            if (door.isOpened)
                savedState.openedEnemyDoors.Add(door.persistentID);

        savedState.hasBow = GameManager.hasBow;
        savedState.hasBomb = GameManager.hasBomb;
        savedState.hasFireArrows = ArrowTypeManager.Instance != null && ArrowTypeManager.Instance.hasFireArrows;
        savedState.hasIceArrows = ArrowTypeManager.Instance != null && ArrowTypeManager.Instance.hasIceArrows;
        savedState.keyCount = KeyManager.Instance != null ? KeyManager.Instance.GetKeyCount() : 0;

        Debug.Log("SaveState complete - hasBow: " + savedState.hasBow + " openedChests: " + savedState.openedChests.Count);
    }

    public void RestoreState()
    {
        Debug.Log("RestoreState - Instance ID: " + GetInstanceID() +
                  " savedState null: " + (savedState == null) +
                  " checkpoint: " + savedState.checkpointIndex +
                  " hasBow: " + savedState.hasBow +
                  " openedChests: " + savedState.openedChests.Count +
                  " chests found: " + FindObjectsOfType<Chest>().Length);

        if (savedState == null) return;

        foreach (var door in FindObjectsOfType<DoorHealth>())
            if (savedState.destroyedDoors.Contains(door.persistentID))
                door.RestoreDestroyed();

        foreach (var door in FindObjectsOfType<LockedDoor>())
            if (savedState.openedLockedDoors.Contains(door.persistentID))
                door.RestoreUnlocked();

        foreach (var wall in FindObjectsOfType<IceWall>())
            if (savedState.meltedIceWalls.Contains(wall.persistentID))
                wall.RestoreMelted();

        foreach (var door in FindObjectsOfType<OneWayDoorEntrance>())
            if (savedState.openedEntranceDoors.Contains(door.persistentID))
                door.RestoreOpened();

        foreach (var door in FindObjectsOfType<OneWayDoorExit>())
            if (savedState.openedExitDoors.Contains(door.persistentID))
                door.RestoreOpened();

        foreach (var chest in FindObjectsOfType<Chest>())
            if (savedState.openedChests.Contains(chest.persistentID))
                chest.RestoreOpened();

        foreach (var enemy in FindObjectsOfType<PermanentDeathEnemy>())
            if (savedState.deadEnemies.Contains(enemy.persistentID))
                Destroy(enemy.gameObject);

        foreach (var door in FindObjectsOfType<EnemyDoor>())
            if (savedState.openedEnemyDoors.Contains(door.persistentID))
                door.RestoreOpened();

        GameManager.hasBow = savedState.hasBow;
        GameManager.hasBomb = savedState.hasBomb;
        if (savedState.hasFireArrows && ArrowTypeManager.Instance != null)
            ArrowTypeManager.Instance.UnlockFireArrows();
        if (savedState.hasIceArrows && ArrowTypeManager.Instance != null)
            ArrowTypeManager.Instance.UnlockIceArrows();
        if (KeyManager.Instance != null)
            KeyManager.Instance.RestoreKeys(savedState.keyCount);
    }
}