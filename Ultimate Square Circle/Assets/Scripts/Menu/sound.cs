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
        // Initialiser le volume avec la valeur sauvegard�e ou une valeur par d�faut
        soundSlider.value = PlayerPrefs.GetFloat("Volume", 100f);
        AudioListener.volume = soundSlider.value * 10;

        // Mettre � jour le texte initialement
        UpdateText(soundSlider.value);

        // Ajouter un listener pour mettre � jour le texte lorsque le Slider change de valeur
        soundSlider.onValueChanged.AddListener(UpdateText);
        soundSlider.onValueChanged.AddListener(ChangeVolume);
    }

    private void UpdateText(float value)
    {
        // Mettre � jour le texte avec la valeur du soundSlider
        textMeshPro.text = value.ToString("0");
    }

    private void OnDestroy()
    {
        // Supprimer le listener pour �viter des erreurs
        soundSlider.onValueChanged.RemoveListener(UpdateText);
    }

    // M�thode pour r�gler le volume du jeu
    public void ChangeVolume(float value)
    {
        AudioListener.volume = value * 10;
        PlayerPrefs.SetFloat("Volume", value);
        PlayerPrefs.Save();
    }
}
