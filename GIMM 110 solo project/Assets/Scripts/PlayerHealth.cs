using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("UI References")]
    public Slider healthSlider;  // Drag your HealthBar slider here
    public Image healthFillImage; // Optional if you use Image-based bar

    void Start()
    {
        currentHealth = maxHealth;

        // Initialize UI
        if (healthSlider != null)
            healthSlider.maxValue = maxHealth;

        UpdateHealthUI();
    }

    public void TakeDamage(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Debug.Log("Player died!");
            // Handle death logic here
        }
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if (healthSlider != null)
            healthSlider.value = currentHealth;

        if (healthFillImage != null)
            healthFillImage.fillAmount = currentHealth / maxHealth;
    }
}

