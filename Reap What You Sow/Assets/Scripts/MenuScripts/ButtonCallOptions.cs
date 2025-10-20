using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonCallsOptions : MonoBehaviour
{
    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(Call);
    }

    void Call()
    {
        OptionsMenu.Instance?.TogglePanel();
    }
}
