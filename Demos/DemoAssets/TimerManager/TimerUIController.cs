using UnityEngine;

namespace OmnicatLabs.Demos.Timers
{
    public class TimerUIController : MonoBehaviour
    {
        public GameObject timerUI;
        public static TimerUIController Instance;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        public void SpawnTimeUI()
        {
            GameObject instance = Instantiate(timerUI);
            instance.transform.SetParent(transform);
        }
    }
}
