using UnityEngine;

public class GenderPrefabManager : MonoBehaviour
{
    public static GenderPrefabManager instance;

    [SerializeField] private GameObject malePrefab;
    [SerializeField] private GameObject femalePrefab;

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public GameObject GetPrefabByGender(string gender)
    {
        Debug.Log($"[GENDER] Input: '{gender}'");
        Debug.Log($"[GENDER] Male prefab: {malePrefab?.name}, Female prefab: {femalePrefab?.name}");

        if (gender == "Female")
        {
            return femalePrefab;
        }
        return malePrefab;
    }
}