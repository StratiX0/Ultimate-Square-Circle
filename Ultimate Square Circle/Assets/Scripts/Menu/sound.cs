using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class sound : MonoBehaviour
{
    [SerializeField] Slider soundSlider;
    [SerializeField] private TMP_Text textMeshPro;

    void Start()
    {
        // Initialiser le volume avec la valeur sauvegardée ou une valeur par défaut
        soundSlider.value = PlayerPrefs.GetFloat("Volume", 100f);
        AudioListener.volume = soundSlider.value * 10;

        // Mettre à jour le texte initialement
        UpdateText(soundSlider.value);

        // Ajouter un listener pour mettre à jour le texte lorsque le Slider change de valeur
        soundSlider.onValueChanged.AddListener(UpdateText);
        soundSlider.onValueChanged.AddListener(ChangeVolume);
    }

    private void UpdateText(float value)
    {
        // Mettre à jour le texte avec la valeur du soundSlider
        textMeshPro.text = value.ToString("0");
    }

    private void OnDestroy()
    {
        // Supprimer le listener pour éviter des erreurs
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
