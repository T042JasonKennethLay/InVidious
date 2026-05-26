using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DataManager : MonoBehaviour
{

    private int level;
    public int Level { get => level; set => level = value; }

    private string nama;
    public string Nama { get => nama; set => nama = value; }

    private int sens;
    public int Sens { get => sens; set => sens=value; }

    private int fov;
    public int Fov { get => fov; set => fov = value; }

    private string item;
    public string  Item{ get => item; set => item = value; }

    private string gender;
    public string Gender { get => gender; set => gender = value; }

    private string ghost;
    public string Ghost { get => ghost; set => ghost = value; }

    private string oujia;
    public string Oujia { get=>oujia; set => oujia = value; }

    private float masterVolume;
    public float MasterVolume { get=>masterVolume; set => masterVolume = value; } 


  

    public static DataManager instance;


    [SerializeField] private GameObject _loaderCanvas;
    [SerializeField] private Image _progressBar;

    private List<ulong> ghostClientIds = new List<ulong>();
    public List<ulong> GhostClientIds { get => ghostClientIds; set => ghostClientIds = value; }

    public bool IsPlayerGhost(ulong clientId)
    {
        return ghostClientIds.Contains(clientId);
    }

    private void Awake()    
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(transform.root.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

    }

}
