using UnityEngine;

/// <summary>
/// GAMEOBJECT ATAMASI: Bu scripti hedef (kuş) Prefab'ına ekleyin.
/// Prefab üzerinde ayrıca şunlar bulunmalıdır:
/// - SpriteRenderer (kuş görseli)
/// - Collider2D (örn. CircleCollider2D veya BoxCollider2D) — Physics2D.OverlapCircle
///   ile algılanabilmesi için ZORUNLUDUR.
/// - Layer: "Target" olarak ayarlayın ve ShootingController'daki Target Layer Mask
///   alanına bu layer'ı seçin.
/// 
/// Inspector'da doldurulması gerekenler:
/// - Min/Max Speed   -> Kuşun uçuş hızı aralığı
/// - Score Value     -> Vurulduğunda eklenecek puan
/// - Hit Clip        -> Vuruş/Çığlık sesi (Hit/Squawk)
/// - Boundary Margin -> Ekran dışına çıkınca yok olma toleransı (viewport birimi, örn 0.25)
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Target : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    [Tooltip("Minimum uçuş hızı")]
    [SerializeField] private float minSpeed = 2f;

    [Tooltip("Maksimum uçuş hızı")]
    [SerializeField] private float maxSpeed = 5f;

    [Header("Puan ve Ses")]
    [Tooltip("Bu hedef vurulduğunda eklenecek puan")]
    [SerializeField] private int scoreValue = 10;

    [Tooltip("Vurulduğunda çalınacak Hit/Squawk sesi")]
    [SerializeField] private AudioClip hitClip;

    [Range(0f, 1f)]
    [SerializeField] private float hitVolume = 1f;

    [Header("Sınır Kontrolü")]
    [Tooltip("Ekranın kenarından ne kadar uzaklaşınca hedefin otomatik yok edileceği (viewport birimi)")]
    [SerializeField] private float boundaryMargin = 0.25f;

    private Vector2 moveDirection = Vector2.right;
    private float moveSpeed;
    private Camera mainCamera;
    private bool isDestroyed = false;

    private void Start()
    {
        mainCamera = Camera.main;
        moveSpeed = Random.Range(minSpeed, maxSpeed);
    }

    /// <summary>
    /// TargetSpawner tarafından, hedef doğduğunda hareket yönünü belirlemek için çağrılır.
    /// </summary>
    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;
    }

    private void Update()
    {
        // Time.timeScale = 0 olduğunda Time.deltaTime de 0 olacağından
        // oyun durduğunda hareket otomatik olarak durur.
        transform.position += (Vector3)(moveDirection * moveSpeed * Time.deltaTime);
        CheckOutOfBounds();
    }

    private void CheckOutOfBounds()
    {
        if (mainCamera == null || isDestroyed) return;

        Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);

        if (viewportPos.x < -boundaryMargin || viewportPos.x > 1f + boundaryMargin ||
            viewportPos.y < -boundaryMargin || viewportPos.y > 1f + boundaryMargin)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// ShootingController tarafından bu hedef vurulduğunda çağrılır.
    /// </summary>
    public void GetHit()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        PlayHitSound();

        // NullReferenceException önlemi: GameManager sahnede yoksa çökmeyi engelle
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
        }

        Destroy(gameObject);
    }

    private void PlayHitSound()
    {
        if (hitClip != null)
        {
            // Obje yok edilse bile sesin duyulabilmesi için geçici bir AudioSource
            // oluşturan yardımcı metot kullanılıyor.
            AudioSource.PlayClipAtPoint(hitClip, transform.position, hitVolume);
        }
        else
        {
            Debug.LogWarning("Target: HitClip atanmamış, vuruş sesi çalınamadı.");
        }
    }
}
