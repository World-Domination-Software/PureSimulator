using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TextFieldPutty : MonoBehaviour, IPointerDownHandler
{
    public bool copyOnSelect = true;

    private InputField field;
    private int lastAnchor;
    private int lastFocus;

    private void Start()
    {
        field = GetComponent<InputField>();

        lastAnchor = field.selectionAnchorPosition;
        lastFocus = field.selectionFocusPosition;

        if (!Application.isEditor)
            copyOnSelect = true;
    }

    private void Update()
    {
        if (!field.isFocused || !copyOnSelect)
            return;

        int anchor = field.selectionAnchorPosition, focus = field.selectionFocusPosition;

        if ((anchor != lastAnchor || focus != lastFocus) && anchor != focus) 
        {
            int start = Mathf.Min(anchor, focus);
            int length = Mathf.Abs(focus - anchor);
            GUIUtility.systemCopyBuffer = field.text.Substring(start, length);
            lastAnchor = anchor;
            lastFocus = focus;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right) //paste text from system clipboard! 
        {
            Paste();
        }
    }

    public void Paste() 
    {
        string pasteText = GUIUtility.systemCopyBuffer;
        int pos = field.caretPosition;
        field.text.Insert(pos, pasteText);
        field.caretPosition = pos + pasteText.Length;
    }
}
