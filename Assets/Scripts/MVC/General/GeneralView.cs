using System;
using UnityEngine;
using UnityEngine.UI;

namespace WeatherAndFacts
{
    public class GeneralView : MonoBehaviour
    {
        [SerializeField]
        private Button _weatherButton;
        [SerializeField]
        private Button _factsButton;

        public void BindWeatherButton(Action onClickAction)
        {
            _weatherButton.onClick.AddListener(() =>
            {
                _weatherButton.interactable = false;
                _factsButton.interactable = true;

                onClickAction();
            });

        }

        public void BindFactsButton(Action onClickAction)
        {
            _factsButton.onClick.AddListener(() =>
            {
                _weatherButton.interactable = true;
                _factsButton.interactable = false;

                onClickAction();
            });

        }
    }
}