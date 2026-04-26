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
    public List<string> bombledSkulls = new List<string>();
    public List<string> permanentlyOpenedDoors = new List<string>();
    public bool hasBow;
    public bool hasBomb;
    public bool hasRemoteBomb;
    public bool hasGrapple;
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
        Debug.Log("SaveState called - checkpoint: " + checkpointIndex + " hasBow: " + GameManager.hasBow + " chests found: " + FindObjectsByType<Chest>(FindObjectsSortMode.InstanceID).Length);

        savedState = new CheckpointState();
        savedState.checkpointIndex = checkpointIndex;

        foreach (var door in FindObjectsByType<DoorHealth>(FindObjectsSortMode.InstanceID))
            if (door.isDestroyed)
                savedState.destroyedDoors.Add(door.persistentID);

        foreach (var door in FindObjectsByType<LockedDoor>(FindObjectsSortMode.InstanceID))
            if (door.isUnlocked)
                savedState.openedLockedDoors.Add(door.persistentID);

        foreach (var wall in FindObjectsByType<IceWall>(FindObjectsSortMode.InstanceID))
            if (wall.currentState == IceWall.IceWallState.Melted)
                savedState.meltedIceWalls.Add(wall.persistentID);

        foreach (var door in FindObjectsByType<OneWayDoorEntrance>(FindObjectsSortMode.InstanceID))
            if (door.hasBeenOpened)
                savedState.openedEntranceDoors.Add(door.persistentID);

        foreach (var door in FindObjectsByType<OneWayDoorExit>(FindObjectsSortMode.InstanceID))
            if (door.hasBeenOpened)
                savedState.openedExitDoors.Add(door.persistentID);

        foreach (var chest in FindObjectsByType<Chest>(FindObjectsSortMode.InstanceID))
            if (chest.isOpened)
                savedState.openedChests.Add(chest.persistentID);

        foreach (var enemy in FindObjectsByType<PermanentDeathEnemy>(FindObjectsSortMode.InstanceID))
            if (enemy.isDead)
                savedState.deadEnemies.Add(enemy.persistentID);

        foreach (var door in FindObjectsByType<EnemyDoor>(FindObjectsSortMode.InstanceID))
            if (door.isOpened)
                savedState.openedEnemyDoors.Add(door.persistentID);

        // save bombed skulls
        foreach (var skull in FindObjectsByType<BombableSkull>(FindObjectsSortMode.InstanceID))
            if (skull.hasBeenBombed && !string.IsNullOrEmpty(skull.persistentID))
                savedState.bombledSkulls.Add(skull.persistentID);

        // save permanently opened doors via skull manager
        foreach (var manager in FindObjectsByType<BombableSkullManager>(FindObjectsSortMode.InstanceID))
            if (manager.doorOpened && !string.IsNullOrEmpty(manager.persistentID))
                savedState.permanentlyOpenedDoors.Add(manager.persistentID);

        savedState.hasBow = GameManager.hasBow;
        savedState.hasBomb = GameManager.hasBomb;
        savedState.hasRemoteBomb = GameManager.hasRemoteBomb;
        savedState.hasGrapple = GameManager.hasGrapple;
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
                  " chests found: " + FindObjectsByType<Chest>(FindObjectsSortMode.InstanceID).Length);

        if (savedState == null) return;

        foreach (var door in FindObjectsByType<DoorHealth>(FindObjectsSortMode.InstanceID))
            if (savedState.destroyedDoors.Contains(door.persistentID))
                door.RestoreDestroyed();

        foreach (var door in FindObjectsByType<LockedDoor>(FindObjectsSortMode.InstanceID))
            if (savedState.openedLockedDoors.Contains(door.persistentID))
                door.RestoreUnlocked();

        foreach (var wall in FindObjectsByType<IceWall>(FindObjectsSortMode.InstanceID))
            if (savedState.meltedIceWalls.Contains(wall.persistentID))
                wall.RestoreMelted();

        foreach (var door in FindObjectsByType<OneWayDoorEntrance>(FindObjectsSortMode.InstanceID))
            if (savedState.openedEntranceDoors.Contains(door.persistentID))
                door.RestoreOpened();

        foreach (var door in FindObjectsByType<OneWayDoorExit>(FindObjectsSortMode.InstanceID))
            if (savedState.openedExitDoors.Contains(door.persistentID))
                door.RestoreOpened();

        foreach (var chest in FindObjectsByType<Chest>(FindObjectsSortMode.InstanceID))
            if (savedState.openedChests.Contains(chest.persistentID))
                chest.RestoreOpened();

        foreach (var enemy in FindObjectsByType<PermanentDeathEnemy>(FindObjectsSortMode.InstanceID))
            if (savedState.deadEnemies.Contains(enemy.persistentID))
                Destroy(enemy.gameObject);

        foreach (var door in FindObjectsByType<EnemyDoor>(FindObjectsSortMode.InstanceID))
            if (savedState.openedEnemyDoors.Contains(door.persistentID))
                door.RestoreOpened();

        // restore bombed skulls
        foreach (var skull in FindObjectsByType<BombableSkull>(FindObjectsSortMode.InstanceID))
            if (!string.IsNullOrEmpty(skull.persistentID) &&
                savedState.bombledSkulls.Contains(skull.persistentID))
                skull.RestoreBombed();

        // restore permanently opened doors
        foreach (var manager in FindObjectsByType<BombableSkullManager>(FindObjectsSortMode.InstanceID))
        {
            if (!string.IsNullOrEmpty(manager.persistentID) &&
                savedState.permanentlyOpenedDoors.Contains(manager.persistentID))
            {
                manager.doorOpened = true;
                if (manager.linkedDoor != null)
                    manager.linkedDoor.RestorePermanentlyOpened();
            }
        }

        GameManager.hasBow = savedState.hasBow;
        GameManager.hasBomb = savedState.hasBomb;
        GameManager.hasRemoteBomb = savedState.hasRemoteBomb;
        GameManager.hasGrapple = savedState.hasGrapple;
        if (savedState.hasFireArrows && ArrowTypeManager.Instance != null)
            ArrowTypeManager.Instance.UnlockFireArrows();
        if (savedState.hasIceArrows && ArrowTypeManager.Instance != null)
            ArrowTypeManager.Instance.UnlockIceArrows();
        if (KeyManager.Instance != null)
            KeyManager.Instance.RestoreKeys(savedState.keyCount);
    }
}