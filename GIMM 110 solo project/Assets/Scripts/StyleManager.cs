using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class StyleManager : MonoBehaviour
{
    [System.Serializable]
    public class StyleRank
    {
        public Sprite sprite;
        [Tooltip("Value applied/used when this style rank is active")]
        public int value = 0;
    }

    [Header("UI Elements")]
    public TMP_Text scoreText;                   // Displays total score
    public Image styleIcon;                      // Displays current style rank icon

    // Replace simple sprite array with paired sprite+value entries
    public StyleRank[] styleRanks;               // style ranks (sprite + value)

    [Header("Score Settings")]
    public float score;                      // Total score (increases over time)
    public float styleScore;                 // Temporary combo-style score
    public float styleDecayRate = 5f;        // How quickly style score decays per second
    public float constantScoreRate = 1f;     // Constant increase over time

    [Header("Multipliers")]
    public float dodgeMultiplier = 1.5f;     // Bonus multiplier after dodging
    public float weaponSwitchMultiplier = 1.3f; // Bonus after switching weapons
    public float hitPenalty = 0.7f;          // Penalty after getting hit

    [Header("Timing Windows")]
    public float dodgeWindow = 2f;
    public float switchWindow = 2f;
    public float hitWindow = 0.5f;

    private bool dodgedRecently = false;
    private bool switchedRecently = false;
    private bool hitRecently = false;

    private float styleMultiplier = 1f;

    // Tracks the currently applied style rank index and its value
    private int previousStyleIndex = -1;
    public int CurrentStyleValue { get; private set; } = 0;

    void Update()
    {
        // Add constant passive score
        score += constantScoreRate * Time.deltaTime;

        // Only decay styleScore when not in between-waves countdown
        if (!WaveManagerTMP.IsBetweenWaves)
        {
            if (styleScore > 0)
                styleScore -= styleDecayRate * Time.deltaTime;

            // Clamp values
            styleScore = Mathf.Max(styleScore, 0);
        }

        // Update UI (also updates CurrentStyleValue when rank changes)
        UpdateUI();
    }

    // Called when player kills an enemy
    public void OnKill(float basePoints)
    {
        float multiplier = styleMultiplier;

        // Apply temporary bonuses
        if (dodgedRecently) multiplier *= dodgeMultiplier;
        if (switchedRecently) multiplier *= weaponSwitchMultiplier;
        if (hitRecently) multiplier *= hitPenalty;

        float gained = basePoints * multiplier;
        styleScore += gained;
        score += gained * 0.5f; // add smaller portion to total score
    }

    public void OnDodge()
    {
        StartCoroutine(RecentActionWindow("dodge"));
    }

    public void OnWeaponSwitch()
    {
        StartCoroutine(RecentActionWindow("switch"));
    }

    public void OnPlayerHit()
    {
        StartCoroutine(RecentActionWindow("hit"));
    }

    private IEnumerator RecentActionWindow(string type)
    {
        switch (type)
        {
            case "dodge":
                dodgedRecently = true;
                yield return new WaitForSeconds(dodgeWindow);
                dodgedRecently = false;
                break;
            case "switch":
                switchedRecently = true;
                yield return new WaitForSeconds(switchWindow);
                switchedRecently = false;
                break;
            case "hit":
                hitRecently = true;
                yield return new WaitForSeconds(hitWindow);
                hitRecently = false;
                break;
        }
    }

    private void UpdateUI()
    {
        // Update score text
        if (scoreText)
            scoreText.text = $"Score: {Mathf.FloorToInt(score)}";

        // Update style rank icon and value
        if (styleRanks != null && styleRanks.Length > 0 && styleIcon)
        {
            int index = GetStyleRankIndex();

            // clamp index defensively
            index = Mathf.Clamp(index, 0, styleRanks.Length - 1);

            // only update when changed
            if (index != previousStyleIndex)
            {
                previousStyleIndex = index;
                styleIcon.sprite = styleRanks[index].sprite;
                CurrentStyleValue = styleRanks[index].value;
            }
        }
    }

    private int GetStyleRankIndex()
    {
        if (styleRanks == null || styleRanks.Length == 0)
            return 0;

        // Style ranks distributed across the configured number of slots
        float maxStyle = 1000f; // Adjust to scale difficulty
        int index = Mathf.FloorToInt((styleScore / maxStyle) * (styleRanks.Length - 1));
        return Mathf.Clamp(index, 0, styleRanks.Length - 1);
    }

    // Editor helper: keep previousStyleIndex in sync when values edited in inspector at edit-time
#if UNITY_EDITOR
    private void OnValidate()
    {
        // Ensure arrays are not null
        if (styleRanks == null || styleRanks.Length == 0)
            return;

        // Validate previous index and update CurrentStyleValue so inspector reflects value during edit
        int idx = Mathf.Clamp(GetStyleRankIndex(), 0, styleRanks.Length - 1);
        previousStyleIndex = -1; // force update on next UpdateUI call
        CurrentStyleValue = styleRanks[idx].value;
    }
#endif
}
