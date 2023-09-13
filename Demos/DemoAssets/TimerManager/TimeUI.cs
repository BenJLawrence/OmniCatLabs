using UnityEngine;
using OmnicatLabs.Timers;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace OmnicatLabs.Demos.Timers
{
    public class TimeUI : MonoBehaviour, IPointerClickHandler
    {
        public float amountOfTime = 5f;
        public Image panel;

        private TextMeshProUGUI timeText;
        private Timer timer;

        private void Start()
        {
            timeText = GetComponentInChildren<TextMeshProUGUI>();
            StartTimer();
        }

        public void StartTimer()
        {
            TimerManager.Instance.CreateTimer(amountOfTime, ShowDone, UpdateTimeText, out timer, false);
        }

        public void ShowDone()
        {
            timeText.SetText("0");
            panel.color = Color.green;
        }

        public void Pause()
        {
            TimerManager.Instance.Pause(timer);
        }

        public void Resume()
        {
            TimerManager.Instance.Resume(timer);
        }

        public void Stop()
        {
            TimerManager.Instance.Stop(timer);
        }

        public void UpdateTimeText()
        {
            timeText.SetText(timer.timeRemaining.ToString());
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (timer.isPaused)
                {
                    Resume();
                }
                else Pause();
            }
            
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                Stop();
                Destroy(gameObject);
            }
        }
    }
}
