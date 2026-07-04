using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// GAMEOBJECT ATAMASI: Bu scripti boş bir GameObject'e ekleyin. Örn: "GameManager"
/// Sahnede yalnızca BİR adet olmalıdır (Singleton).
/// 
/// Inspector'da doldurulması gerekenler:
/// - Score Text        -> Canvas > ScoreText (TextMeshProUGUI, sol üst)
/// - Timer Text         -> Canvas > TimerText (TextMeshProUGUI, sağ üst)
/// - Game Over Panel    -> Canvas > GameOverPanel (başlangıçta inaktif)
/// - Final Score Text   -> Canvas > GameOverPanel > FinalScoreText (TextMeshProUGUI)
/// - Round Duration     -> 60 (saniye)
/// 
/// GameOverPanel içindeki "Yeniden Başlat" butonunun OnClick() event'ine bu script'in
/// RestartGame() metodunu Inspector üzerinden sürükleyip bağlayın.
/// </summary>
public class GameManager : MonoBehaviour
{
    // Diğer scriptlerin kolayca erişebilmesi için Singleton referansı
    public static GameManager Instance { get; private set; }

    [Header("UI Referansları")]
    [Tooltip("Sol üstte gösterilecek skor metni")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Tooltip("Sağ üstte gösterilecek geri sayım metni")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Tooltip("Süre dolduğunda aktif olacak Oyun Sonu paneli")]
    [SerializeField] private GameObject gameOverPanel;

    [Tooltip("Oyun sonu panelindeki 'Toplam Skorunuz: X' metni")]
    [SerializeField] private TextMeshProUGUI finalScoreText;

    [Header("Oyun Ayarları")]
    [Tooltip("Round süresi (saniye)")]
    [SerializeField] private float roundDuration = 60f;

    private int currentScore = 0;
    private float timeRemaining;
    private bool isGameOver = false;

    /// <summary>Diğer scriptlerin oyunun bitip bitmediğini kontrol etmesi için</summary>
    public bool IsGameOver => isGameOver;

    private void Awake()
    {
        // Basit Singleton kurulumu
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Oyun her başladığında (veya restart edildiğinde) değerleri sıfırla
        timeRemaining = roundDuration;
        currentScore = 0;
        isGameOver = false;
        Time.timeScale = 1f; // Önceki oyundan kalma pause durumunu temizle

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        UpdateScoreUI();
        UpdateTimerUI();
    }

    private void Update()
    {
        if (isGameOver) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            UpdateTimerUI();
            EndGame();
            return;
        }

        UpdateTimerUI();
    }

    /// <summary>
    /// Bir hedef vurulduğunda Target.cs tarafından çağrılır.
    /// </summary>
    public void AddScore(int amount)
    {
        if (isGameOver) return;

        currentScore += amount;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        // NullReferenceException önlemi: UI atanmamışsa sessizce geç
        if (scoreText != null)
            scoreText.text = "Score: " + currentScore;
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int seconds = Mathf.CeilToInt(timeRemaining);
            timerText.text = "Time: " + seconds;
        }
    }

    private void EndGame()
    {
        isGameOver = true;
        Time.timeScale = 0f; // Oyunu dondur

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (finalScoreText != null)
            finalScoreText.text = "Final Score: " + currentScore;
    }

    /// <summary>
    /// "Yeniden Başlat" butonunun OnClick() event'ine bağlanır.
    /// </summary>
    public void RestartGame()
    {
        // ÖNEMLİ: Yeni sahne yüklenmeden önce zaman ölçeğini normale döndür,
        // aksi halde yeni sahne de donuk başlar.
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
