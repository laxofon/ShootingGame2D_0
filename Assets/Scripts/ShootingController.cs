using UnityEngine;

/// <summary>
/// GAMEOBJECT ATAMASI: Bu scripti "Crosshair" GameObject'ine (CrosshairController ile aynı obje)
/// veya ayrı bir "Shooter" adlı boş GameObject'e ekleyin.
/// Aynı obje üzerinde bir AudioSource component'i bulunmalıdır (yoksa otomatik aranır,
/// yine de Inspector'dan manuel atamak en güvenlisidir).
/// 
/// Inspector'da doldurulması gerekenler:
/// - Audio Source        -> Bu objedeki (veya başka bir) AudioSource component'i
/// - Gun Shot Clip        -> Silah sesi / lazer ses dosyası (.wav/.mp3)
/// - Crosshair Transform  -> Nişangahın Transform'u (boş bırakılırsa kendi transform'u kullanılır)
/// - Target Layer Mask    -> Hedeflerin (kuşların) bulunduğu Layer (örn: "Target" layer'ı oluşturup seçin)
/// - Hit Radius           -> Vuruş toleransı (nişangah yarıçapı), örn. 0.3
/// 
/// Bu script hem Fare Tıklaması hem de MicrophoneVoiceDetector.cs tarafından
/// çağrılan ortak Fire() metodunu kullanır.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class ShootingController : MonoBehaviour
{
    [Header("Ses Ayarları")]
    [Tooltip("Silah sesi çalmak için kullanılacak AudioSource")]
    [SerializeField] private AudioSource audioSource;

    [Tooltip("Ateş edildiğinde çalınacak Silah/Lazer sesi")]
    [SerializeField] private AudioClip gunShotClip;

    [Header("Nişangah Referansı")]
    [Tooltip("Ateşin hangi konumdan yapılacağını belirleyen nişangah Transform'u")]
    [SerializeField] private Transform crosshairTransform;

    [Header("Vuruş Ayarları")]
    [Tooltip("Sadece bu layer'daki objeler hedef olarak kabul edilir. Varsayılan: hepsi.")]
    [SerializeField] private LayerMask targetLayerMask = ~0;

    [Tooltip("Nişangah etrafındaki vuruş algılama yarıçapı")]
    [SerializeField] private float hitRadius = 0.3f;

    private void Start()
    {
        // Referanslar atanmamışsa güvenli varsayılanlara düş
        if (crosshairTransform == null)
            crosshairTransform = transform;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // Oyun bittiyse ateş etmeyi engelle
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Fire();
        }
    }

    /// <summary>
    /// Ateş etme işlemini gerçekleştirir. Hem fare tıklaması hem de
    /// sesli komut ("Boom!") tarafından çağrılabilir.
    /// </summary>
    public void Fire()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        PlayGunShot();

        Vector2 firePoint = crosshairTransform.position;

        // Nişangah konumundaki en yakın hedefi bul
        Collider2D hitCollider = Physics2D.OverlapCircle(firePoint, hitRadius, targetLayerMask);

        if (hitCollider != null)
        {
            Target target = hitCollider.GetComponent<Target>();
            if (target != null)
            {
                target.GetHit();
            }
        }
    }

    private void PlayGunShot()
    {
        // NullReferenceException önlemi: ses kaynağı veya klip eksikse
        // oyunun çökmesini önle, sadece uyarı ver.
        if (audioSource != null && gunShotClip != null)
        {
            audioSource.PlayOneShot(gunShotClip);
        }
        else
        {
            Debug.LogWarning("ShootingController: AudioSource veya GunShotClip atanmamış, silah sesi çalınamadı.");
        }
    }

    // Sahne görünümünde vuruş yarıçapını görselleştirmek için (isteğe bağlı, sadece Editor'de görünür)
    private void OnDrawGizmosSelected()
    {
        if (crosshairTransform == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(crosshairTransform.position, hitRadius);
    }
}
