using UnityEngine;
using TMPro;

public class MasterInfo : MonoBehaviour
{
    public static float score = 0f;
    public float pointsPerSecond = 1f;

    [SerializeField] GameObject scoreDisplay;
    TMP_Text scoreText;
    bool counting = true;

    void Awake()
    {
        score = 0f;
    }

    void OnEnable()
    {
        if (scoreDisplay != null)
            scoreText = scoreDisplay.GetComponent<TMP_Text>();
        if (scoreText == null)
            FindScoreText();

        var player = FindObjectOfType<PlayerHealth>();
        if (player != null) player.Died += OnPlayerDied;
    }

    void OnDisable()
    {
        var player = FindObjectOfType<PlayerHealth>();
        if (player != null) player.Died -= OnPlayerDied;
    }

    void OnPlayerDied()
    {
        counting = false;
        UpdateDisplay();
    }

    void Update()
    {
        if (scoreText == null)
        {
            FindScoreText();
        }
        if (counting)
        {
            score += pointsPerSecond * Time.deltaTime;
        }
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = "SCORE: " + Mathf.FloorToInt(score);
        }
    }

    public static int ScoreInt => Mathf.FloorToInt(score);

    void FindScoreText()
    {
        var go = GameObject.Find("ScoreText");
        if (go != null)
        {
            scoreText = go.GetComponent<TMP_Text>();
        }
    }

    public void SetScoreText(TMP_Text text)
    {
        scoreText = text;
        UpdateDisplay();
    }

    public bool HasScoreTextBound()
    {
        if (scoreText != null) return true;
        if (scoreDisplay != null)
        {
            var t = scoreDisplay.GetComponent<TMP_Text>();
            if (t != null)
            {
                scoreText = t;
                return true;
            }
        }
        return false;
    }

    public TMP_Text GetScoreText()
    {
        if (scoreText != null) return scoreText;
        if (scoreDisplay != null)
        {
            var t = scoreDisplay.GetComponent<TMP_Text>();
            if (t != null)
            {
                scoreText = t;
                return scoreText;
            }
        }
        return null;
    }
}
