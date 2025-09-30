
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;
public class UIController : MonoBehaviour
{
    public TextMeshProUGUI scoreText; public TextMeshProUGUI comboText; public TextMeshProUGUI feedbackText;
    public GameObject resultsPanel; public TextMeshProUGUI resultsScoreText; public TextMeshProUGUI resultsMaxComboText;
    public TextMeshProUGUI[] laneFeedbackTexts;
    public SpriteRenderer[] laneWindowBands;    public Color bandPerfectColor = Color.yellow;
    public Color bandGoodLateColor = Color.blue;
    public Color bandGoodEarlyColor = new Color(0.7f, 0.3f, 1f); // purple
    public Color bandMissColor = Color.white;

    [SerializeField] private float lanePopInDuration = 0.12f;
    [SerializeField] private float lanePopHoldDuration = 0.25f;
    [SerializeField] private float lanePopOutDuration = 0.12f;
    [SerializeField] private float lanePopScale = 1.2f;
    [SerializeField] private float lanePopMoveUp = 20f;
    [SerializeField] private bool laneAutoHide = true;

    private Sequence[] lanePopSequences;
    private Vector3[] laneBaseScales;
    void Awake()
    {
        if (resultsPanel != null) resultsPanel.SetActive(false);

        UpdateScore(0); 
        UpdateCombo(0); 
        ClearFeedback();

        if (laneFeedbackTexts != null)
        {
            int count = laneFeedbackTexts.Length;
            lanePopSequences = new Sequence[count];
            laneBaseScales = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                var t = laneFeedbackTexts[i];
                if (t != null)
                {
                    laneBaseScales[i] = t.rectTransform.localScale;
                    t.gameObject.SetActive(false);
                }
                else
                {
                    laneBaseScales[i] = Vector3.one;
                }
            }
        }
    }
    public void UpdateScore(int score)
    {
        if (scoreText != null) scoreText.text = score.ToString();
    }
    public void UpdateCombo(int combo)
    {
        if (comboText != null) comboText.text = combo > 0 ? combo.ToString() + " Combo" : "0";
    }
    void ClearFeedback()
    {
        if (feedbackText != null) feedbackText.text = "";
    }
    public void ShowResults(int score, int maxCombo)
    {
        if (resultsPanel != null) resultsPanel.SetActive(true);
        if (resultsScoreText != null) resultsScoreText.text = score.ToString();
        if (resultsMaxComboText != null) resultsMaxComboText.text = maxCombo.ToString();
    }

    public void ShowLaneFeedback(int lane, string message, Color color)
    {
        if (laneFeedbackTexts == null) return;
        if (lane < 0 || lane >= laneFeedbackTexts.Length) return;
        var t = laneFeedbackTexts[lane];
        if (t == null) return;

        if (lanePopSequences != null && lane < lanePopSequences.Length)
        {
            var prev = lanePopSequences[lane];
            if (prev != null && prev.IsActive()) prev.Kill(true);
            lanePopSequences[lane] = null;
        }

        var rt = t.rectTransform;
        var go = t.gameObject;

        Vector3 baseScale = (laneBaseScales != null && lane < laneBaseScales.Length) ? laneBaseScales[lane] : Vector3.one;
        Vector2 startPos = rt.anchoredPosition;

        t.text = message;
        t.color = new Color(color.r, color.g, color.b, 0f);
        rt.localScale = baseScale * 0.7f;
        if (!go.activeSelf) go.SetActive(true);

        var seq = DOTween.Sequence().SetTarget(go);

        seq.Join(rt.DOScale(baseScale * lanePopScale, lanePopInDuration).SetEase(Ease.OutBack))
           .Join(t.DOFade(1f, lanePopInDuration))
           .Append(rt.DOScale(baseScale, 0.08f))
           .AppendInterval(lanePopHoldDuration);

        if (laneAutoHide)
        {
            seq.Append(t.DOFade(0f, lanePopOutDuration).SetEase(Ease.OutQuad))
               .Join(rt.DOAnchorPosY(startPos.y + lanePopMoveUp, lanePopOutDuration).SetEase(Ease.OutQuad))
               .OnComplete(() =>
               {
                   rt.anchoredPosition = startPos;
                   rt.localScale = baseScale;
                   go.SetActive(false);
               });
        }
        else
        {
            seq.OnComplete(() =>
            {
                rt.localScale = baseScale;
                t.color = new Color(color.r, color.g, color.b, 1f);
            });
        }

        if (lanePopSequences != null && lane < lanePopSequences.Length)
        {
            lanePopSequences[lane] = seq;
        }
    }
    public void HideLaneFeedback(int lane)
    {
        if (laneFeedbackTexts == null) return;
        if (lane < 0 || lane >= laneFeedbackTexts.Length) return;

        if (lanePopSequences != null && lane < lanePopSequences.Length)
        {
            var prev = lanePopSequences[lane];
            if (prev != null && prev.IsActive()) prev.Kill(true);
            lanePopSequences[lane] = null;
        }

        var t = laneFeedbackTexts[lane];
        if (t != null)
        {
            DOTween.Kill(t.gameObject);
            t.gameObject.SetActive(false);
        }
    }

    public void SetLaneWindowColor(int lane, Color color)
    {
        if (laneWindowBands == null) return;
        if (lane < 0 || lane >= laneWindowBands.Length) return;
        var sr = laneWindowBands[lane];
        if (sr == null) return;
        sr.color = color;
        var go = sr.gameObject;
        if (!go.activeSelf) go.SetActive(true);
    }

    void OnDestroy()
    {
        if (lanePopSequences == null) return;
        for (int i = 0; i < lanePopSequences.Length; i++)
        {
            var s = lanePopSequences[i];
            if (s != null && s.IsActive()) s.Kill(true);
        }
    }

    public void OnRetryButtonClicked()
    {
        SceneManager.LoadScene("GamePlay");
    }
}
