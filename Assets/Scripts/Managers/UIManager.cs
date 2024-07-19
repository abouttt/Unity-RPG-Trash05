using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : SingletonBehaviour<UIManager>
{
    public int ActivePopupCount => _activePopups.Count;
    public bool IsShowedHelperPopup => _helperPopup != null;
    public bool IsShowedSelfishPopup => _selfishPopup != null;

    private readonly Dictionary<UIType, Transform> _uiRoots = new();
    private readonly Dictionary<Type, UI_Base> _uiObjects = new();
    private readonly LinkedList<UI_Popup> _activePopups = new();
    private UI_Popup _helperPopup;
    private UI_Popup _selfishPopup;

    protected override void Init()
    {
        base.Init();

        foreach (UIType type in Enum.GetValues(typeof(UIType)))
        {
            if (type is UIType.Subitem)
            {
                continue;
            }

            var root = new GameObject($"{type}_Root").transform;
            root.SetParent(transform);
            _uiRoots.Add(type, root);
        }
    }

    protected override void Dispose()
    {
        base.Dispose();

        Clear();
    }

    public T Get<T>() where T : UI_Base
    {
        if (_uiObjects.TryGetValue(typeof(T), out var ui))
        {
            return ui as T;
        }

        return null;
    }

    public void Register<T>(T ui) where T : UI_Base
    {
        if (ui == null)
        {
            Debug.LogWarning($"[UIManager/Register] {typeof(T)} object is null.");
            return;
        }

        if (ui.UIType == UIType.Subitem)
        {
            Debug.LogWarning($"[UIManager/Register] Subitem type can't register : {ui.name}");
            return;
        }

        if (_uiObjects.ContainsKey(typeof(T)))
        {
            Debug.LogWarning($"[UIManager/Register] {ui.name} is already registered.");
            return;
        }

        if (!ui.TryGetComponent<Canvas>(out var canvas))
        {
            Debug.LogWarning($"[UIManager/Register] {ui.name} is no exist Canvas component.");
            return;
        }
        else
        {
            canvas.sortingOrder = (int)ui.UIType;
        }

        if (ui.UIType == UIType.Popup)
        {
            InitPopup(ui as UI_Popup);
            ui.gameObject.SetActive(false);
        }

        ui.transform.SetParent(_uiRoots[ui.UIType]);
        _uiObjects.Add(typeof(T), ui);
    }

    public void Unregister<T>() where T : UI_Base
    {
        if (_uiObjects.TryGetValue(typeof(T), out var ui))
        {
            if (ui is UI_Popup popup)
            {
                popup.ClearEvents();

                if (popup.IsHelper && _helperPopup == popup)
                {
                    _helperPopup = null;
                }
                else if (popup.IsSelfish && _selfishPopup == popup)
                {
                    _selfishPopup = null;
                }

                _activePopups.Remove(popup);
            }

            _uiObjects.Remove(typeof(T));
        }
        else
        {
            Debug.LogWarning($"[UIManager/Unregister] {typeof(T)} is no registered.");
        }
    }

    public T Show<T>() where T : UI_Base
    {
        if (_uiObjects.TryGetValue(typeof(T), out var ui))
        {
            if (ui.gameObject.activeSelf)
            {
                return ui as T;
            }

            if (ui is UI_Popup popup)
            {
                if (IsShowedSelfishPopup && !popup.IgnoreSelfish)
                {
                    return null;
                }

                if (popup.IsHelper)
                {
                    if (IsShowedHelperPopup)
                    {
                        _activePopups.Remove(_helperPopup);
                        _helperPopup.gameObject.SetActive(false);
                    }

                    _helperPopup = popup;
                }
                else if (popup.IsSelfish)
                {
                    CloseAll(UIType.Popup);
                    _selfishPopup = popup;
                }

                _activePopups.AddFirst(popup);
                RefreshAllPopupDepth();
            }

            ui.gameObject.SetActive(true);

            return ui as T;
        }

        return null;
    }

    public bool IsShowed<T>() where T : UI_Base
    {
        if (_uiObjects.TryGetValue(typeof(T), out var ui))
        {
            return ui.gameObject.activeSelf;
        }

        return false;
    }

    public void Close<T>() where T : UI_Base
    {
        if (_uiObjects.TryGetValue(typeof(T), out var ui))
        {
            if (!ui.gameObject.activeSelf)
            {
                return;
            }

            if (ui.UIType == UIType.Popup)
            {
                var popup = ui as UI_Popup;

                if (popup.IsHelper)
                {
                    _helperPopup = null;
                }
                else if (popup.IsSelfish)
                {
                    _selfishPopup = null;
                }

                _activePopups.Remove(popup);
            }

            ui.gameObject.SetActive(false);
        }
    }

    public void CloseTopPopup()
    {
        if (ActivePopupCount > 0)
        {
            var popup = _activePopups.First.Value;

            if (popup.IsHelper)
            {
                _helperPopup = null;
            }
            else if (popup.IsSelfish)
            {
                _selfishPopup = null;
            }

            _activePopups.RemoveFirst();
            popup.gameObject.SetActive(false);
        }
    }

    public void CloseAll(UIType type)
    {
        if (type == UIType.Popup)
        {
            foreach (var popup in _activePopups)
            {
                popup.gameObject.SetActive(false);
            }

            _activePopups.Clear();
            _helperPopup = null;
            _selfishPopup = null;
        }
        else
        {
            foreach (Transform child in _uiRoots[type])
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    public void ShowOrClose<T>() where T : UI_Base
    {
        if (IsShowed<T>())
        {
            Close<T>();
        }
        else
        {
            Show<T>();
        }
    }

    public void Clear()
    {
        foreach (var kvp in _uiRoots)
        {
            foreach (Transform child in kvp.Value)
            {
                Destroy(child.gameObject);
            }
        }

        _uiObjects.Clear();
        _activePopups.Clear();
        _helperPopup = null;
        _selfishPopup = null;
    }

    private void InitPopup(UI_Popup popup)
    {
        popup.PopupRT.anchoredPosition = popup.DefaultPosition;

        popup.Focused += () =>
        {
            _activePopups.Remove(popup);
            _activePopups.AddFirst(popup);
            RefreshAllPopupDepth();
        };
    }

    private void RefreshAllPopupDepth()
    {
        int count = 1;
        foreach (var popup in _activePopups)
        {
            popup.Canvas.sortingOrder = (int)UIType.Top - count++;
        }
    }
}
