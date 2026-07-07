using System.Collections;
using TMPro;
using UnityEngine;
using static System.Net.Mime.MediaTypeNames;

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

    [Tooltip("Sound clip played each time an individual bullet is reloaded")]
    [SerializeField] private AudioClip reloadClip;

    [Header("Nişangah Referansı")]
    [Tooltip("Ateşin hangi konumdan yapılacağını belirleyen nişangah Transform'u")]
    [SerializeField] private Transform crosshairTransform;

    [Header("Vuruş Ayarları")]
    [Tooltip("Sadece bu layer'daki objeler hedef olarak kabul edilir. Varsayılan: hepsi.")]
    [SerializeField] private LayerMask targetLayerMask = ~0;

    [Tooltip("Nişangah etrafındaki vuruş algılama yarıçapı")]
    [SerializeField] private float hitRadius = 0.3f;

    [Header("Ammunition & Reload Settings")]
    [Tooltip("Maximum number of bullets in the weapon")]
    public int maxAmmo = 6;

    [Tooltip("Total time in seconds to fully reload from 0 to max")]
    public float totalReloadTime = 2f;

    [Tooltip("UI Text element to display the current ammo and warning on screen")]
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI warningText;

    private int currentAmmo;
    private bool isReloading = false;
    private Coroutine warningTextCoroutine;

    private void Start()
    {
        // Referanslar atanmamışsa güvenli varsayılanlara düş
        if (crosshairTransform == null)
            crosshairTransform = transform;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // Initialize ammunition state and UI at the start of the game
        currentAmmo = maxAmmo;
        UpdateAmmoUI();
    }

    private void Update()
    {
        // Oyun bittiyse ateş etmeyi engelle
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        // Prevent firing or starting a new reload if we are already reloading
        if (isReloading)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Fire();
            }
            return;
        }

        // Right-click to trigger the reload routine
        if (Input.GetMouseButtonDown(1) && currentAmmo < maxAmmo)
        {
            StartCoroutine(ReloadRoutine());
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Fire();
        }
        return;
    }

    /// <summary>
    /// Ateş etme işlemini gerçekleştirir. Hem fare tıklaması hem de
    /// sesli komut ("Boom!") tarafından çağrılabilir.
    /// </summary>
    public void Fire()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        // Block firing if the gun is currently reloading
        if (isReloading)
        {
            DisplayOnScreenWarning("weapon is not loaded yet");
            return;
        }

        // Check if there is enough ammo to shoot
        if (currentAmmo <= 0)
        {
            Debug.Log("Out of ammo!");
            return;
        }

        currentAmmo--;
        UpdateAmmoUI();
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

    /// <summary>
    /// Coroutine that handles the progressive reload over time.
    /// It adds bullets one by one until the magazine is full.
    /// </summary>
    private IEnumerator ReloadRoutine()
    {
        isReloading = true;

        // Calculate the wait time between each individual bullet reload
        float timePerBullet = totalReloadTime / maxAmmo;

        // Determine exactly how many bullets are missing from the magazine
        int bulletsToReload = maxAmmo - currentAmmo;

        // Loop to add each missing bullet progressively
        for (int i = 0; i < bulletsToReload; i++)
        {
            // Wait for the calculated fraction of the reload time
            yield return new WaitForSeconds(timePerBullet);

            currentAmmo++;
            UpdateAmmoUI();

            if (audioSource != null && reloadClip != null)
            {
                audioSource.PlayOneShot(reloadClip);
            }
        }

        // Reload complete, allow shooting again
        isReloading = false;
    }

    /// <summary>
    /// Updates the UI Text element to reflect the current ammo state.
    /// </summary>
    private void UpdateAmmoUI()
    {
        if (ammoText != null)
        {
            ammoText.text = "Ammo: " + currentAmmo + "/" + maxAmmo;
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

    private void DisplayOnScreenWarning(string message)
    {
        if (warningText == null) return;

        // If a warning is already counting down, stop it so it resets the 2-second timer
        if (warningTextCoroutine != null)
        {
            StopCoroutine(warningTextCoroutine);
        }

        warningTextCoroutine = StartCoroutine(WarningTextRoutine(message));
    }

    private IEnumerator WarningTextRoutine(string message)
    {
        warningText.text = message;
        yield return new WaitForSeconds(2f);
        warningText.text = "";
    }
}
