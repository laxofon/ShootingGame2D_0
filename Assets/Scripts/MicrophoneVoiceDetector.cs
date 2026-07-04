using UnityEngine;
using System.Collections;

/// <summary>
/// GAMEOBJECT ATAMASI: Bu scripti boş bir GameObject'e ekleyin. Örn: "VoiceDetector"
/// 
/// ÖNEMLİ NOT: Unity'nin standart kütüphaneleri gerçek "kelime tanıma" (speech-to-text)
/// yapamaz. Bu script, mikrofon girişindeki ANİ SES YÜKSELMESİNİ (loudness/genlik) algılayarak
/// "Boom!" gibi yüksek ve ani bir sesin söylenmiş olabileceğini varsayar ve ateş tetikler.
/// Gerçek kelime bazlı tanıma için Windows Speech Recognition, Google Cloud Speech-to-Text
/// veya benzeri bir üçüncü parti eklenti/API entegrasyonu gereklidir.
/// 
/// Inspector'da doldurulması gerekenler:
/// - Shooting Controller     -> Sahnedeki ShootingController component'i (Crosshair objesi)
/// - Loudness Threshold       -> Mikrofonunuza göre test ederek ayarlayın (varsayılan 0.05)
/// - Detection Cooldown       -> Art arda tetiklenmeyi önlemek için bekleme süresi (saniye)
/// - Enable Microphone Detection -> Mikrofon özelliğini açıp kapatmak için
/// </summary>
public class MicrophoneVoiceDetector : MonoBehaviour
{
    [Header("Referanslar")]
    [Tooltip("Ateş etme mantığını içeren ShootingController")]
    [SerializeField] private ShootingController shootingController;

    [Header("Mikrofon Ayarları")]
    [Tooltip("Bu değerin üzerindeki ses şiddeti 'Boom!' komutu olarak algılanır. Ortam gürültüsüne göre ayarlayın.")]
    [SerializeField] private float loudnessThreshold = 0.05f;

    [Tooltip("Bir algılamadan sonra tekrar algılama yapılmadan önce beklenecek süre")]
    [SerializeField] private float detectionCooldown = 0.5f;

    [Tooltip("Ses örneklemesi için pencere boyutu")]
    [SerializeField] private int sampleWindow = 128;

    [Tooltip("Mikrofon algılama özelliğini tamamen açar/kapatır")]
    [SerializeField] private bool enableMicrophoneDetection = true;

    private AudioClip microphoneClip;
    private string microphoneDevice;
    private bool isCooldown = false;

    private void Start()
    {
        if (!enableMicrophoneDetection) return;

        // Sistemde mikrofon yoksa özelliği güvenli şekilde devre dışı bırak
        if (Microphone.devices.Length == 0)
        {
            Debug.LogWarning("MicrophoneVoiceDetector: Sistemde mikrofon bulunamadı. Sesli komut devre dışı bırakıldı.");
            enableMicrophoneDetection = false;
            return;
        }

        microphoneDevice = Microphone.devices[0];
        // Döngüsel (loop) 1 saniyelik kayıt başlat; sürekli üzerine yazılır
        microphoneClip = Microphone.Start(microphoneDevice, true, 1, AudioSettings.outputSampleRate);
    }

    private void Update()
    {
        if (!enableMicrophoneDetection || microphoneClip == null) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
        if (isCooldown) return;

        float loudness = GetMicrophoneLoudness();

        if (loudness > loudnessThreshold)
        {
            TriggerVoiceShot();
        }
    }

    /// <summary>
    /// Mikrofonun son örneklerinin RMS (Root Mean Square) genliğini hesaplar.
    /// </summary>
    private float GetMicrophoneLoudness()
    {
        int micPosition = Microphone.GetPosition(microphoneDevice) - sampleWindow;
        if (micPosition < 0) return 0f;

        float[] samples = new float[sampleWindow];
        microphoneClip.GetData(samples, micPosition);

        float sum = 0f;
        for (int i = 0; i < sampleWindow; i++)
        {
            sum += samples[i] * samples[i];
        }

        return Mathf.Sqrt(sum / sampleWindow);
    }

    private void TriggerVoiceShot()
    {
        // NullReferenceException önlemi
        if (shootingController != null)
        {
            shootingController.Fire();
        }
        else
        {
            Debug.LogWarning("MicrophoneVoiceDetector: ShootingController atanmamış.");
        }

        StartCoroutine(CooldownRoutine());
    }

    private IEnumerator CooldownRoutine()
    {
        isCooldown = true;
        // Realtime kullanıyoruz ki oyun duraklatılsa bile cooldown normal işlesin
        yield return new WaitForSecondsRealtime(detectionCooldown);
        isCooldown = false;
    }

    private void OnDestroy()
    {
        if (enableMicrophoneDetection && !string.IsNullOrEmpty(microphoneDevice) && Microphone.IsRecording(microphoneDevice))
        {
            Microphone.End(microphoneDevice);
        }
    }
}
