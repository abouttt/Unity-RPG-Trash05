using UnityEngine;

public class UIController : MonoBehaviour
{
    private void OnCursorToggle()
    {
        if (UIManager.Instance.IsShowedSelfishPopup)
        {
            return;
        }

        InputManager.Instance.CursorLocked = !InputManager.Instance.CursorLocked;
    }

    private void ShowOrClosePopup<T>() where T : UI_Popup
    {
        if (UIManager.Instance.IsShowedHelperPopup)
        {
            return;
        }

        UIManager.Instance.ShowOrClose<T>();
    }
}
