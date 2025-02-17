using UnityEngine;

namespace WeatherAndFacts
{
    public class FactsView : MonoBehaviour
    {
        [SerializeField]
        private Canvas _factsCanvas;

        private void Awake()
        {
            Debug.Log("�������� FactsView!!!");
        }

        public void ActivateUI(bool isActive)
        {
            _factsCanvas.gameObject.SetActive(isActive);
        }
    }
}