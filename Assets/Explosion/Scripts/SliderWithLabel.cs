using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderWithLabel : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private TMP_Text _MinTxt;
    [SerializeField] private TMP_Text _MaxTxt;

    [Header("Range")]
    [SerializeField] private float min = 0f;
    [SerializeField] private float max = 1f;
    [SerializeField] private bool wholeNumbers = false;

    [Header("Step Settings")]
    [SerializeField] private bool useSteps = false;
    [SerializeField] private float stepSize = 0.1f;

    public System.Action<float> OnValueChanged;

    private bool _isUpdating;

    private void Awake()
    {
        slider.minValue = min;
        slider.maxValue = max;

        slider.wholeNumbers = wholeNumbers;

        _MinTxt.text = min.ToString();
        _MaxTxt.text = max.ToString();

        slider.onValueChanged.AddListener(HandleValueChanged);

        HandleValueChanged(slider.value);
    }

    private void HandleValueChanged(float value)
    {
        if (_isUpdating)
            return;

        // -------- STEP SNAP --------
        if (useSteps && stepSize > 0f)
        {
            float snapped = Mathf.Round(value / stepSize) * stepSize;
            snapped = Mathf.Clamp(snapped, min, max);

            if (!Mathf.Approximately(snapped, value))
            {
                _isUpdating = true;
                slider.value = snapped;
                _isUpdating = false;

                value = snapped;
            }
        }

        // -------- LABEL --------
        if (wholeNumbers)
            valueText.text = ((int)value).ToString();
        else
            valueText.text = value.ToString("0.00");

        OnValueChanged?.Invoke(value);
    }

    public float GetCurrentValue()
    {
        return slider.value;
    }
}