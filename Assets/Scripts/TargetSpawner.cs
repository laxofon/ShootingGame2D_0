using UnityEngine;
using System.Collections;

/// <summary>
/// GAMEOBJECT ATAMASI: Bu scripti boş bir GameObject'e ekleyin. Örn: "TargetSpawner"
/// 
/// Inspector'da doldurulması gerekenler:
/// - Target Prefabs      -> Bir veya birden fazla kuş Prefab'ı (Target.cs içeren)
/// - Min/Max Spawn Interval -> İki doğum arasındaki bekleme süresi aralığı (saniye)
/// - Spawn Margin         -> Kuşların ekran dışında ne kadar uzakta doğacağı (dünya birimi)
/// 
/// Not: Bu script Orthographic (2D) kamera varsayar. Kamera Main Camera olarak
/// tag'lenmiş ve Projection = Orthographic olmalıdır.
/// </summary>
public class TargetSpawner : MonoBehaviour
{
    [Header("Hedef Prefabları")]
    [Tooltip("Rastgele seçilecek kuş/hedef prefabları")]
    [SerializeField] private GameObject[] targetPrefabs;

    [Header("Spawn Ayarları")]
    [Tooltip("İki doğum arasındaki minimum süre")]
    [SerializeField] private float minSpawnInterval = 0.5f;

    [Tooltip("İki doğum arasındaki maksimum süre")]
    [SerializeField] private float maxSpawnInterval = 1.5f;

    [Tooltip("Hedeflerin ekran kenarının ne kadar dışında doğacağı (dünya birimi)")]
    [SerializeField] private float spawnMargin = 1f;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError("TargetSpawner: Sahnede 'MainCamera' tag'ine sahip bir kamera bulunamadı.");
            return;
        }

        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            float wait = Random.Range(minSpawnInterval, maxSpawnInterval);
            // WaitForSeconds, Time.timeScale'e duyarlıdır; oyun durunca (timeScale=0)
            // bu bekleme de otomatik olarak durur.
            yield return new WaitForSeconds(wait);

            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
                continue;

            SpawnTarget();
        }
    }

    private void SpawnTarget()
    {
        // NullReferenceException önlemi
        if (targetPrefabs == null || targetPrefabs.Length == 0 || mainCamera == null)
        {
            Debug.LogWarning("TargetSpawner: Target Prefabs listesi boş veya kamera bulunamadı.");
            return;
        }

        GameObject prefab = targetPrefabs[Random.Range(0, targetPrefabs.Length)];

        // Ortografik kameranın dünya-uzayı görünüm sınırlarını hesapla
        float camHeight = mainCamera.orthographicSize * 2f;
        float camWidth = camHeight * mainCamera.aspect;
        Vector3 camCenter = mainCamera.transform.position;

        int edge = Random.Range(0, 4); // 0: Sol, 1: Sağ, 2: Alt, 3: Üst
        Vector3 spawnPos;
        Vector2 direction;

        switch (edge)
        {
            case 0: // Soldan doğ, sağa doğru uç
                spawnPos = new Vector3(
                    camCenter.x - camWidth / 2f - spawnMargin,
                    Random.Range(camCenter.y - camHeight / 2f, camCenter.y + camHeight / 2f),
                    0f);
                direction = new Vector2(1f, Random.Range(-0.4f, 0.4f));
                break;

            case 1: // Sağdan doğ, sola doğru uç
                spawnPos = new Vector3(
                    camCenter.x + camWidth / 2f + spawnMargin,
                    Random.Range(camCenter.y - camHeight / 2f, camCenter.y + camHeight / 2f),
                    0f);
                direction = new Vector2(-1f, Random.Range(-0.4f, 0.4f));
                break;

            case 2: // Alttan doğ, yukarı doğru uç
                spawnPos = new Vector3(
                    Random.Range(camCenter.x - camWidth / 2f, camCenter.x + camWidth / 2f),
                    camCenter.y - camHeight / 2f - spawnMargin,
                    0f);
                direction = new Vector2(Random.Range(-0.4f, 0.4f), 1f);
                break;

            default: // Üstten doğ, aşağı doğru uç
                spawnPos = new Vector3(
                    Random.Range(camCenter.x - camWidth / 2f, camCenter.x + camWidth / 2f),
                    camCenter.y + camHeight / 2f + spawnMargin,
                    0f);
                direction = new Vector2(Random.Range(-0.4f, 0.4f), -1f);
                break;
        }

        GameObject targetObj = Instantiate(prefab, spawnPos, Quaternion.identity);

        Target targetScript = targetObj.GetComponent<Target>();
        if (targetScript != null)
        {
            targetScript.SetDirection(direction);
        }
        else
        {
            Debug.LogWarning("TargetSpawner: Doğan objede Target.cs bulunamadı.");
        }
    }
}
