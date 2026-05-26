using TMPro;
using UnityEngine;

public class DataPlayerLoad : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nama;
    [SerializeField] private TextMeshProUGUI sens_game;
    [SerializeField] private TextMeshProUGUI fov_game;
    [SerializeField] private TextMeshProUGUI volume_game;
    [SerializeField] private PlayerControllers playerControllers;
    [SerializeField] private Camera main;
    [SerializeField] private TextMeshProUGUI gender_text;

    private void Start()
    {
        ChangeName(DataManager.instance.Nama);
        //playerControllers.lookSenseH = DataManager.instance.Sens;
        //playerControllers.lookSenveV = DataManager.instance.Sens;
        main.fieldOfView = DataManager.instance.Fov;
        
        
        
        sens_game.text = DataManager.instance.Sens.ToString();
        fov_game.text = DataManager.instance.Fov.ToString();
        volume_game.text= DataManager.instance.MasterVolume.ToString();
    }

    public void ChangeName(string names)
    {
        this.nama.text = names; 
    }
}
