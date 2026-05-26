using TMPro;
using UnityEngine;

public class nameLobby : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI name1;
    [SerializeField] private TextMeshProUGUI name2;

    void Start()
    {
        name1.text= name2.text;        
    }

}
