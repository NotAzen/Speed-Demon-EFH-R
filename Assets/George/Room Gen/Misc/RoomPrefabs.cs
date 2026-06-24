using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPrefabList", menuName = "Prefab Collection")]
public class RoomPrefabs : ScriptableObject
{
    public List<GameObject> RoomOptions;
    public List<GameObject> StartRoomOptions;
    public List<GameObject> EndRoomOptions;

}
