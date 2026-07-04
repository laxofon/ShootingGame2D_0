using UnityEngine;

/// <summary>
/// GAMEOBJECT ATAMASI: Bu scripti "Crosshair" adlı GameObject'e ekleyin
/// (nişangah sprite'ını taşıyan obje; örn. bir SpriteRenderer içeren boş obje).
/// 
/// Inspector'da doldurulması gerekenler:
/// - Target Camera -> boş bırakılırsa otomatik olarak Camera.main kullanılır.
/// - Hide System Cursor -> true ise Windows/Mac imleci gizlenir, sadece nişangah görünür.
/// </summary>
public class CrosshairController : MonoBehaviour
{
    [Header("Ayarlar")]
    [Tooltip("Sistem fare imlecini gizle ve yalnızca nişangahı göster")]
    [SerializeField] private bool hideSystemCursor = true;

    [Tooltip("Ekran-dünya dönüşümü için kullanılacak kamera. Boş bırakılırsa Camera.main kullanılır.")]
    [SerializeField] private Camera targetCamera;

    private void Start()
    {
        // Kamera atanmamışsa sahnedeki ana kamerayı otomatik bul
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (hideSystemCursor)
            Cursor.visible = false;
    }

    private void Update()
    {
        if (targetCamera == null) return;

        // Fare pozisyonunu ekran koordinatından dünya koordinatına çevir.
        // Perspektif kameralarda doğru derinlik için kameranın z mesafesini kullanıyoruz.
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Mathf.Abs(targetCamera.transform.position.z);

        Vector3 worldPos = targetCamera.ScreenToWorldPoint(mouseScreenPos);
        worldPos.z = 0f; // 2D oyun olduğu için z ekseni sabit

        transform.position = worldPos;
    }

    private void OnDestroy()
    {
        // Sahne değişirken/obje yok edilirken imleci tekrar görünür yap
        Cursor.visible = true;
    }
}
