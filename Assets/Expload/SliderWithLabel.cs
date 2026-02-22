using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderWithLabel : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private TMP_Text _MinTxt;
    [SerializeField] private TMP_Text _MaxTxt;

    [SerializeField] private float min = 0f;
    [SerializeField] private float max = 1f;
    [SerializeField] private bool wholeNumbers = false;

    public System.Action<float> OnValueChanged;

    private void Awake()
    {
        slider.minValue = min;
        slider.maxValue = max;
        _MinTxt.text = "" + min;
        _MaxTxt.text = "" + max;
        slider.wholeNumbers = wholeNumbers;

        slider.onValueChanged.AddListener(HandleValueChanged);

        HandleValueChanged(slider.value);
    }

    private void HandleValueChanged(float value)
    {
        if (wholeNumbers)
            valueText.text = ((int)value).ToString();
        else
            valueText.text = value.ToString("0.00");

        OnValueChanged?.Invoke(value);
    }
}