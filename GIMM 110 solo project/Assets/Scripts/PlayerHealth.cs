using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    private bool isDead = false;

    [Header("UI References")]
    public Slider healthSlider;
    public Image healthFillImage;

    public GameObject deathScreen;
    private CanvasGroup deathGroup;

    void Start()
    {
        currentHealth = maxHealth;

        if (healthSlider != null)
            healthSlider.maxValue = maxHealth;

        UpdateHealthUI();

        if (deathScreen != null)
        {
            deathGroup = deathScreen.GetComponent<CanvasGroup>();
            deathScreen.SetActive(false);
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
        UpdateHealthUI();

        Object.FindFirstObjectByType<StyleManager>()?.OnPlayerHit();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("Player died!");

        // Stop game time
        Time.timeScale = 0f;

        // Disable movement/shooting instead of destroying
        var move = GetComponent<Movement2D>();
        if (move != null) move.enabled = false;

        var shoot = GetComponent<Shoot>();
        if (shoot != null) shoot.enabled = false;

        var shootAlt = GetComponent<ShootAlternate>();
        if (shootAlt != null) shootAlt.enabled = false;

        // Show death screen
        if (deathScreen != null)
        {
            deathScreen.SetActive(true);
            StartCoroutine(FadeInDeathScreen());
        }

        // Start restart wait
        StartCoroutine(WaitForRestart());
    }

    private System.Collections.IEnumerator FadeInDeathScreen()
    {
        if (deathGroup == null)
            yield break;

        float t = 0f;
        deathGroup.alpha = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * 0.75f;
            deathGroup.alpha = Mathf.Lerp(0, 1, t);
            yield return null;
        }
        deathGroup.alpha = 1f;
    }

    private System.Collections.IEnumerator WaitForRestart()
    {
        Debug.Log("Waiting for restart...");
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Space pressed — restarting level");
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                yield break;
            }
            yield return null;
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (healthSlider != null)
            healthSlider.value = currentHealth;

        if (healthFillImage != null)
            healthFillImage.fillAmount = currentHealth / maxHealth;
    }
}


