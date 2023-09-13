using UnityEngine;
using TMPro;

namespace OmnicatLabs.DebugUtils
{
    public class FPSDebug : MonoBehaviour
    {
        private GameObject debugger;
        private float deltaTime;

        void Start()
        {
            debugger = Instantiate(Resources.Load("FPSCounter") as GameObject);
        }

        void Update()
        {
            deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
            float fps = 1.0f / deltaTime;
            debugger.GetComponentInChildren<TextMeshProUGUI>().SetText(Mathf.Ceil(fps).ToString());
        }
    }
}