using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class PlayerHealth : NetworkBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 4f;
    public NetworkVariable<float> currentHealth = new NetworkVariable<float>(4f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Damage Overlay")]
    [SerializeField] private Image blood1;
    [SerializeField] private Image blood2;
    [SerializeField] private float fadeSpeed = 0.5f;
    [SerializeField] private Slider healthSlider;

    private PlayerControllers _controllers;

    public override void OnNetworkSpawn()
    {
        _controllers = GetComponent<PlayerControllers>();

        if (IsOwner && healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth.Value;

            if (blood1) SetAlpha(blood1, 0);
            if (blood2) SetAlpha(blood2, 0);
        }
        currentHealth.OnValueChanged += OnHealthChanged;
    }

    void Update()
    {
        if (IsOwner && Input.GetKeyDown(KeyCode.T))
        {
            healthSlider.value -= 1;
            Debug.Log("SAYA TEKAN T, SLIDER HARUSNYA KURANG: " + healthSlider.value);
            healthSlider.gameObject.name = "INI_SLIDER_YANG_LAGI_GANTI_" + healthSlider.value;
            Debug.Log("SAYA TEKAN T, NAMA OBJECT SLIDER SEKARANG: " + healthSlider.gameObject.name);
        }
    }

    public override void OnNetworkDespawn()
    {
        currentHealth.OnValueChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(float previousValue, float newValue)
    {
        UpdateUiAnimation(newValue);
    }

    private void SetAlpha(Image img, float alpha)
    {
        if (img == null) return;
        Color tempcolor = img.color;
        tempcolor.a = alpha;
        img.color = tempcolor;
    }

    public void TakeDamage(int amount = 1)
    {
        if (!IsServer)
        {
            TakeDamageServerRpc(amount);
            return;
        }

        currentHealth.Value -= amount;
        currentHealth.Value = Mathf.Clamp(currentHealth.Value, 0, maxHealth);

        if (currentHealth.Value <= 0)
        {
            Die();
        }
    }

    [ServerRpc]
    private void TakeDamageServerRpc(int amount)
    {
        TakeDamage(amount);
    }

    public void Heal(float amount)
    {
        if (currentHealth.Value <= 0) return;

        if (!IsServer)
        {
            HealServerRpc(amount);
            return;
        }
        currentHealth.Value += amount;
        currentHealth.Value = Mathf.Clamp(currentHealth.Value, 0, maxHealth);

        Debug.Log($"[HEAL] Player {OwnerClientId} healed. Current: {currentHealth.Value}");
    }

    [ServerRpc]
    private void HealServerRpc(float amount)
    {
        Heal(amount);
    }

    private void UpdateUiAnimation(float health)
    {
        if (_controllers != null)
        {
            _controllers.UpdateHealtAnimation((int)health);
        }

        if (IsOwner)
        {
            if (healthSlider != null) healthSlider.value = health;
            if (health < maxHealth)
            {
                TriggerBloodOverlay();
            }
        }
    }

    private void TriggerBloodOverlay()
    {
        if (blood1 != null) { blood1.gameObject.SetActive(true); StartCoroutine(FadeImage(blood1)); }
        if (blood2 != null) { blood2.gameObject.SetActive(true); StartCoroutine(FadeImage(blood2)); }
    }

    private IEnumerator FadeImage(Image img)
    {
        float currentAlpha = 1f;
        SetAlpha(img, currentAlpha);
        while (currentAlpha > 0)
        {
            currentAlpha -= Time.deltaTime * fadeSpeed;
            SetAlpha(img, currentAlpha);
            yield return null;
        }
        SetAlpha(img, 0f);
        img.gameObject.SetActive(false);
    }

    private void Die()
    {
        Debug.Log($"Player {OwnerClientId} Mati!");
    }
}