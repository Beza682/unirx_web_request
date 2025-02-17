using TMPro;
using UnityEngine;

namespace WeatherAndFacts
{
    public class WeatherView : MonoBehaviour
    {
        [SerializeField]
        private Canvas _weatherCanvas;
        [SerializeField]
        private TMP_Text _temperature;

        private void Awake()
        {
            Debug.Log("Создание WeatherView!!!");
        }

        public void SetTemperature(string temperature)
        {
            _temperature.text = temperature;
        }

        public void ActivateUI(bool isActive)
        {
            _weatherCanvas.gameObject.SetActive(isActive);
        }
    }
}