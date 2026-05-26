using NUnit.Framework.Internal;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PassDataSource : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nama;
    [SerializeField] private TextMeshProUGUI level;
    [SerializeField] private TextMeshProUGUI sens;
    [SerializeField] private TextMeshProUGUI fov;
    [SerializeField] private TextMeshProUGUI item;
    [SerializeField] private TextMeshProUGUI gender;
    [SerializeField] private TextMeshProUGUI ghost;
    [SerializeField] private TextMeshProUGUI volumeSlider;
    [SerializeField] private TextMeshProUGUI oujia;

    public void SaveDataSebelumPindahScene()
    {
        DataManager.instance.Nama = nama.text;
        DataManager.instance.Gender = gender.text;
        DataManager.instance.Item = item.text;
        DataManager.instance.Ghost = ghost.text;
        DataManager.instance.Oujia = oujia.text;

        if (int.TryParse(level.text, out int levelVal)) DataManager.instance.Level = levelVal;
        if (int.TryParse(sens.text, out int sensVal)) DataManager.instance.Sens = sensVal;
        if (int.TryParse(fov.text, out int fovVal)) DataManager.instance.Fov = fovVal;
        if (int.TryParse(volumeSlider.text, out int volVal)) DataManager.instance.MasterVolume = volVal;

        Debug.Log($"[PASSDATA] Data tersimpan - Nama: {DataManager.instance.Nama}, Gender: {DataManager.instance.Gender}");
    }
}
