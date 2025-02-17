using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Runtime.Controllers
{
    public class GameManager : MonoBehaviour
    {
        #region Main

        [Foldout("Main Music"), SerializeField]
        private AudioClip winSound;

        #endregion

        #region Touches

        [Foldout("Touches"), SerializeField] private Touch[] _touches;
        [Foldout("Touches"), SerializeField] private GameObject[] vfxPrefabs, fingers;

        #endregion

        #region Countdown

        [Foldout("Countdown"), SerializeField] private GameObject[] countdownVfx;
        [Foldout("Countdown"), SerializeField] private float countdownTime = 3f;
        [Foldout("Countdown"), SerializeField] private bool countdownStarted;

        #endregion

        #region Selection

        [Foldout("Selection"), SerializeField] private bool selecting = false;
        [Foldout("Selection"), SerializeField] private AudioClip selectionMusic;
        [Foldout("Selection"), SerializeField] private AudioClip highlightSound, deepHorn;

        [Foldout("Selection"), SerializeField]
        private float selectionTime = 5f, startDelay = 0.1f, maxDelay = 1.5f, growthFactor = 1.5f;

        #endregion

        #region Explosion

        [Foldout("Explosion"), SerializeField] private GameObject explosionPrefab;
        [Foldout("Explosion"), SerializeField] private AudioClip explosionSound;

        #endregion

        #region Dictionary

        [Foldout("Dictionary")] public Dictionary<int, GameObject> FingerDictionary = new Dictionary<int, GameObject>();

        #endregion

        #region Private

        private Coroutine _countdownCoroutine;
        private AudioSource _audioSource;

        #endregion
        
        private void Start()
        {
            _audioSource = gameObject.GetComponent<AudioSource>();
        }

        private void Update()
        {
            TouchControler();
        }

        private void TouchControler()
        {
            int touchCount = Input.touchCount;

            if (touchCount > 0)
            {
                _touches = new Touch[touchCount];

                for (int i = 0; i < touchCount; i++)
                {
                    _touches[i] = Input.GetTouch(i);
                    Touch touch = _touches[i];
                    int fingerId = touch.fingerId;

                    switch (_touches[i].phase)
                    {
                        case TouchPhase.Began:
                            Vector2 touchPosition = Camera.main.ScreenToWorldPoint(_touches[i].position);
                            GameObject newVfx = Instantiate(vfxPrefabs[Random.Range(0, vfxPrefabs.Length)],
                                touchPosition, Quaternion.Euler(90, 0, 0));
                            ParticleSystem[] childerenParticles = newVfx.GetComponentsInChildren<ParticleSystem>();
                            foreach (ParticleSystem particle in childerenParticles)
                            {
                                var main = particle.main;
                                main.startColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f),
                                    Random.Range(0f, 1f));
                            }

                            if (!FingerDictionary.ContainsKey(fingerId))
                            {
                                FingerDictionary.Add(fingerId, newVfx);
                            }

                            break;
                        case TouchPhase.Ended:
                        case TouchPhase.Canceled:
                            if (FingerDictionary.TryGetValue(fingerId, out GameObject obj))
                            {
                                Destroy(obj);
                                FingerDictionary.Remove(fingerId);
                            }

                            // **Listeleri güncelle**
                            if (FingerDictionary.Count == 0)
                            {
                                ResetCountdown(); // Geri sayımı sıfırla
                            }

                            break;

                        case TouchPhase.Stationary:
                            if (AllTouchesStationary() && !countdownStarted)
                            {
                                countdownStarted = true;
                                _countdownCoroutine = StartCoroutine(CountdownAndSelect());
                            }

                            break;
                    }
                }
            }
            else
            {
                ResetCountdown(); // Tüm dokunuşlar bittiğinde geri sayımı durdur
                FingerDictionary.Clear();
            }
        }

        private bool AllTouchesStationary()
        {
            foreach (var touch in _touches)
            {
                if (touch.phase != TouchPhase.Stationary)
                {
                    return false;
                }
            }

            return true;
        }

        private IEnumerator CountdownAndSelect()
        {
            yield return new WaitForSeconds(2f); // Tüm parmaklar stationary durumunda 2 saniye bekleyin

            // Geri sayım başladı
            for (int i = 3; i > 0; i--)
            {
                var countdownElement = Instantiate(countdownVfx[i - 1], Vector3.zero, Quaternion.identity);
                _audioSource.PlayOneShot(highlightSound);
                yield return new WaitForSeconds(1f);
                Destroy(countdownElement);
            }

            // GO yazan particle'ı instantiate et
            var go = Instantiate(countdownVfx[3], Vector3.zero, Quaternion.identity);
            _audioSource.PlayOneShot(deepHorn);
            yield return new WaitForSeconds(1.5f);
            Destroy(go);


            // Rastgele seçim yap
            yield return StartCoroutine(RandomSelection());
        }

        private IEnumerator RandomSelection()
        {
            selecting = true;
            float elapsedTime = 0f;
            float currentDelay = startDelay;
            List<GameObject> activeVfxList = new List<GameObject>(FingerDictionary.Values);

            activeVfxList.RemoveAll(item => item == null);
            if (activeVfxList.Count == 0) yield break;

            if (selectionMusic != null)
            {
                _audioSource.clip = selectionMusic;
                _audioSource.loop = true;
                _audioSource.Play();
            }

            while (elapsedTime < selectionTime)
            {
                activeVfxList.RemoveAll(item => item == null);
                if (activeVfxList.Count == 0)
                {
                    Debug.LogWarning("All touches removed mid-selection! Cancelling.");
                    _audioSource.Stop();
                    yield break;
                }

                foreach (GameObject vfx in activeVfxList)
                {
                    if (vfx == null) continue;

                    yield return StartCoroutine(HighlightEffect(vfx, false));
                    yield return new WaitForSeconds(currentDelay);
                    elapsedTime += currentDelay;
                    currentDelay *= growthFactor; // Her tur süresi artırılıyor
                    if (currentDelay > maxDelay) currentDelay = maxDelay;
                    if (elapsedTime >= selectionTime) break;
                }
            }

            int finalIndex = Random.Range(0, activeVfxList.Count);
            GameObject finalSelectedVfx = activeVfxList[finalIndex];
            if (finalSelectedVfx == null) yield break;

            yield return StartCoroutine(HighlightEffect(finalSelectedVfx, true));

            if (winSound != null)
            {
                _audioSource.PlayOneShot(winSound);
            }

            yield return new WaitForSeconds(3f);
            InstantiateExplosion(finalSelectedVfx.transform.position);

            if (explosionSound != null)
            {
                _audioSource.PlayOneShot(explosionSound);
            }

            yield return new WaitForSeconds(2f);
            Destroy(finalSelectedVfx);

            _audioSource.Stop();
        }

        private void InstantiateExplosion(Vector3 position)
        {
            if (explosionPrefab != null)
            {
                var destructionPrefab = Instantiate(explosionPrefab, position, Quaternion.Euler(90, 0, 0));
                Destroy(destructionPrefab, 5f);
            }
        }

        private void ResetCountdown()
        {
            if (countdownStarted)
            {
                countdownStarted = false;
                if (_countdownCoroutine != null)
                {
                    StopCoroutine(_countdownCoroutine);
                    _countdownCoroutine = null;
                }
            }
        }

        private IEnumerator HighlightEffect(GameObject obj, bool isFinal)
        {
            if (obj == null) yield break; // 📌 **Eğer objemiz null olmuşsa animasyonu iptal et**

            float duration = isFinal ? 1f : 0.5f;
            float elapsedTime = 0f;
            Vector3 originalScale = obj.transform.localScale;
            Vector3 targetScale = isFinal ? originalScale * 1.8f : originalScale * 1.3f;

            Light light = obj.GetComponentInChildren<Light>();
            if (light == null)
            {
                GameObject lightObj = new GameObject("HighlightLight");
                light = lightObj.AddComponent<Light>();
                light.color = Color.yellow;
                light.intensity = 0;
                light.range = 4;
                lightObj.transform.SetParent(obj.transform);
                lightObj.transform.localPosition = Vector3.zero;
            }

            if (highlightSound != null)
            {
                _audioSource.PlayOneShot(highlightSound);
            }

            while (elapsedTime < duration)
            {
                if (obj == null) yield break; // 📌 **Eğer objemiz patlatılırsa animasyonu iptal et!**

                float t = elapsedTime / duration;
                obj.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
                light.intensity = Mathf.Lerp(0, isFinal ? 5 : 3, t);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            if (!isFinal)
            {
                yield return new WaitForSeconds(0.3f);
                elapsedTime = 0f;
                while (elapsedTime < duration)
                {
                    if (obj == null) yield break; // 📌 **Eğer nesne silinmişse büyütmeyi iptal et!**

                    float t = elapsedTime / duration;
                    obj.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
                    light.intensity = Mathf.Lerp(3, 0, t);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }

                Destroy(light.gameObject);
            }
        }
    }
}