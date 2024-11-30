using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class sound : MonoBehaviour
{
    [SerializeField] Slider soundSlider;
    [SerializeField] private TMP_Text textMeshPro;

    private void Awake()
    {
        soundSlider.value = PlayerPrefs.GetFloat("Volume", 100f);
        AudioListener.volume = soundSlider.value * 10;
    }

    void Start()
    {
        soundSlider.value = PlayerPrefs.GetFloat("Volume", 100f);
        AudioListener.volume = soundSlider.value * 10;

        UpdateText(soundSlider.value);

        soundSlider.onValueChanged.AddListener(UpdateText);
        soundSlider.onValueChanged.AddListener(ChangeVolume);
    }

    private void UpdateText(float value)
    {
        textMeshPro.text = value.ToString("0");
    }

    private void OnDestroy()
    {
        soundSlider.onValueChanged.RemoveListener(UpdateText);
    }

    // Méthode pour régler le volume du jeu
    public void ChangeVolume(float value)
    {
        AudioListener.volume = value * 10;
        PlayerPrefs.SetFloat("Volume", value);
        PlayerPrefs.Save();
    }
}
