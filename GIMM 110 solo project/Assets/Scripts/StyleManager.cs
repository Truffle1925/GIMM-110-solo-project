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
        public string rankName; // e.g. "D", "C", "B", "A", "S", "SS", etc.
        [Tooltip("Value applied/used when this style rank is active")]
        public int value = 0;
    }

    [Header("UI Elements")]
    public TMP_Text scoreText;                   // Displays total score
    public Image styleIcon;                      // Displays current style rank icon
    public TMP_Text styleRankText;               // Displays current style rank name

    [Header("Style Ranks")]
    public StyleRank[] styleRanks;               // style ranks (sprite + name + value)

    [Header("Score Settings")]
    public float score;
    public float styleScore;
    public float styleDecayRate = 5f;
    public float constantScoreRate = 1f;

    [Header("Multipliers")]
    public float dodgeMultiplier = 1.5f;
    public float weaponSwitchMultiplier = 1.3f;
    public float hitPenalty = 0.7f;

    [Header("Timing Windows")]
    public float dodgeWindow = 2f;
    public float switchWindow = 2f;
    public float hitWindow = 0.5f;

    private bool dodgedRecently = false;
    private bool switchedRecently = false;
    private bool hitRecently = false;

    private float styleMultiplier = 1f;
    private int previousStyleIndex = -1;
    public int CurrentStyleValue { get; private set; } = 0;

    // Animation settings
    [Header("Rank Text Animation")]
    public float animationDuration = 0.4f;      // Total time of the pop animation
    public float popScale = 1.5f;               // How much larger it grows
    public Color flashColor = Color.yellow;     // Temporary flash color

    private Vector3 baseScale;
    private Color baseColor;
    private Coroutine rankAnimCoroutine;

    void Start()
    {
        if (styleRankText != null)
        {
            baseScale = styleRankText.transform.localScale;
            baseColor = styleRankText.color;
        }
    }

    void Update()
    {
        score += constantScoreRate * Time.deltaTime;

        if (!WaveManagerTMP.IsBetweenWaves)
        {
            if (styleScore > 0)
                styleScore -= styleDecayRate * Time.deltaTime;

            styleScore = Mathf.Max(styleScore, 0);
        }

        UpdateUI();
    }

    public void OnKill(float basePoints)
    {
        float multiplier = styleMultiplier;

        if (dodgedRecently) multiplier *= dodgeMultiplier;
        if (switchedRecently) multiplier *= weaponSwitchMultiplier;
        if (hitRecently) multiplier *= hitPenalty;

        float gained = basePoints * multiplier;
        styleScore += gained;
        score += gained * 0.5f;
    }

    public void OnDodge() => StartCoroutine(RecentActionWindow("dodge"));
    public void OnWeaponSwitch() => StartCoroutine(RecentActionWindow("switch"));
    public void OnPlayerHit() => StartCoroutine(RecentActionWindow("hit"));

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
        if (scoreText)
            scoreText.text = $"Score: {Mathf.FloorToInt(score)}";

        if (styleRanks != null && styleRanks.Length > 0 && styleIcon)
        {
            int index = GetStyleRankIndex();
            index = Mathf.Clamp(index, 0, styleRanks.Length - 1);

            if (index != previousStyleIndex)
            {
                previousStyleIndex = index;
                styleIcon.sprite = styleRanks[index].sprite;
                CurrentStyleValue = styleRanks[index].value;

                if (styleRankText != null)
                {
                    styleRankText.text = styleRanks[index].rankName;
                    StartRankTextAnimation(); // 🔥 trigger animation when rank changes
                }
            }
        }
    }

    private void StartRankTextAnimation()
    {
        if (rankAnimCoroutine != null)
            StopCoroutine(rankAnimCoroutine);

        rankAnimCoroutine = StartCoroutine(AnimateRankText());
    }

    private IEnumerator AnimateRankText()
    {
        if (styleRankText == null) yield break;

        float timer = 0f;
        Vector3 targetScale = baseScale * popScale;

        // Flash to highlight color
        styleRankText.color = flashColor;

        // Scale up
        while (timer < animationDuration / 2f)
        {
            float t = timer / (animationDuration / 2f);
            styleRankText.transform.localScale = Vector3.Lerp(baseScale, targetScale, t);
            timer += Time.deltaTime;
            yield return null;
        }

        // Scale down and fade color back
        timer = 0f;
        while (timer < animationDuration / 2f)
        {
            float t = timer / (animationDuration / 2f);
            styleRankText.transform.localScale = Vector3.Lerp(targetScale, baseScale, t);
            styleRankText.color = Color.Lerp(flashColor, baseColor, t);
            timer += Time.deltaTime;
            yield return null;
        }

        styleRankText.transform.localScale = baseScale;
        styleRankText.color = baseColor;
        rankAnimCoroutine = null;
    }

    private int GetStyleRankIndex()
    {
        if (styleRanks == null || styleRanks.Length == 0)
            return 0;

        float maxStyle = 1000f;
        int index = Mathf.FloorToInt((styleScore / maxStyle) * (styleRanks.Length - 1));
        return Mathf.Clamp(index, 0, styleRanks.Length - 1);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (styleRanks == null || styleRanks.Length == 0)
            return;

        int idx = Mathf.Clamp(GetStyleRankIndex(), 0, styleRanks.Length - 1);
        previousStyleIndex = -1;
        CurrentStyleValue = styleRanks[idx].value;
    }
#endif
}

