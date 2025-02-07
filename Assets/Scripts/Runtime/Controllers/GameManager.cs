using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Runtime.Controllers
{
    public class GameManager : MonoBehaviour
    {
        [Foldout("Touches"), SerializeField] private Touch[] _touches;
        [Foldout("Touches"), SerializeField] private GameObject[] vfxPrefabs, fingers;
        
        [Foldout("Countdown"), SerializeField] private GameObject[] countdownVfx;
        [Foldout("Countdown"), SerializeField] private float countdownTime, currentCountdownTime;
        [Foldout("Countdown"), SerializeField] private bool countdownStarted;

        [Foldout("Selection"), SerializeField] private bool selecting = false;
        [Foldout("Selection"), SerializeField] private float selectionTime, speedDecreaseFactor, currentSpeed, scaleFactor;
        [Foldout("Selection"), SerializeField] private int selectionCycles;

        private void Update()
        {
            int touchCount = Input.touchCount;

            if (touchCount>0)
            {
                
                _touches = new Touch[touchCount];

                for (int i = 0; i < touchCount; i++)
                {
                    _touches[i] = Input.GetTouch(i);

                    switch (_touches[i].phase)
                    {
                        case TouchPhase.Began:
                            
                            Vector2 touchPosition = Camera.main.ScreenToWorldPoint(_touches[i].position);
                            GameObject newVfx = Instantiate(vfxPrefabs[Random.Range(0, vfxPrefabs.Length)], touchPosition, Quaternion.Euler(90, 0, 0));
                            ParticleSystem[] childerenParticles = newVfx.GetComponentsInChildren<ParticleSystem>();
                            foreach (ParticleSystem particle in childerenParticles)
                            {
                                var main = particle.main;
                                main.startColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
                            }
                            
                            if (fingers == null || fingers.Length < touchCount)
                            {
                                System.Array.Resize(ref fingers, touchCount);
                            }
                            
                            fingers[i] = newVfx;
                            break;
                        case TouchPhase.Ended:
                                Destroy(fingers[i]);
                                fingers[i] = null;
                            break;
                        case TouchPhase.Stationary:
                            break;
                    }
                    Debug.Log(_touches[i].phase);
                }
            }
            else
            {
                foreach (GameObject finger in fingers)
                {
                    Destroy(finger);
                }
            }
        }

        private IEnumerator StartCountdown()
        {
            
            
            yield return new WaitForSeconds(.5f);
            //StartCoroutine(RandomSelection());
        }

        private IEnumerator RandomSelection()
        {
            selecting = true;
            int index = 0;
            
            float elapsedTime = 0f;
            float cycleDuration = selectionTime / selectionCycles;

            while (selecting)
            {
                if (vfxPrefabs != null)
                {
                    index = Random.Range(0, vfxPrefabs.Length);

                    foreach (GameObject vfx in vfxPrefabs)
                    {
                        vfx.transform.localScale = Vector3.one;
                    }

                    if (vfxPrefabs[index] != null)
                    {
                        
                    }
                    
                }
                
            }

            yield return null;
        }
    }
}
