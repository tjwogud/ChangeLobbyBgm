using System;
using System.Collections;
using UnityEngine;

namespace ChangeLobbyBgm
{
    public class StaticCoroutine : MonoBehaviour
    {
        private static StaticCoroutine instance;
        
        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        public static void Start(IEnumerator coroutine, Action onEnd = null)
        {
            if (!instance)
                instance = new GameObject("StaticCoroutine").AddComponent<StaticCoroutine>();
            instance.StartCoroutine(instance.StartCo(coroutine, onEnd));
        }

        private IEnumerator StartCo(IEnumerator coroutine, Action onEnd)
        {
            yield return coroutine;
            onEnd?.Invoke();
        }
    }
}
