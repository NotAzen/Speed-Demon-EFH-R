using UnityEngine;

public class RoomGen : MonoBehaviour
{
    [Tooltip("Assign a RoomPrefabs asset here, or place an asset named 'RoomPrefabs' in a Resources folder for automatic loading.")]
    public RoomPrefabs roomPrefabs;
    public RoomsVarData RVData;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        RVData = FindFirstObjectByType<RoomsVarData>();
        if(RVData.RoomNum >= RVData.RoomCap)
        {
            return;
        }
        var options = roomPrefabs.RoomOptions;

        var choice = options[Random.Range(0, options.Count)];
        if (choice == null)
        {
            Debug.LogWarning("Selected RoomOptions entry is null.");
            return;
        }
        RVData.RoomNum++;
        // Instantiate as a child of this GameObject and reset local transform
        var instance = Instantiate(choice, transform);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;
    }

    // Update is called once per frame
    void Update()
    {
    }
}
