using UnityEngine;
using UnityEngine.UI;

public class AutoFieldActivator : MonoBehaviour
{
    private InputField field;

    private void OnEnable()
    {
        if(field == null)
            field = GetComponent<InputField>();

        field.ActivateInputField();
    }
}
