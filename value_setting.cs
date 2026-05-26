using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class value_setting : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI value;
    [SerializeField] private Slider slider_temp;

    void Update()
    {
        value.text = ((int)slider_temp.value).ToString();
    }
}
