using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panel : MonoBehaviour
{

    [SerializeField] private string id = ""; public string ID { get { return id; } }
    [SerializeField] private RectTransform container = null;

    private bool _initialized = false; public bool IsInitialized => _initialized;
    private bool _isOpen = false; public bool IsOpen => _isOpen;
    private Canvas _canvas = null; public Canvas Canvas { get { return _canvas; } set { _canvas = value; } }
    
    public virtual void Awake()
    {
        Initialize();
    }

    public virtual void Initialize()
    {
        if (_initialized) { return; }
        _initialized = true;
        Close();
    }

    public virtual void Open()
    {
        if (_initialized == false) { Initialize(); }
        transform.SetAsLastSibling();
        container.gameObject.SetActive(true);
        _isOpen = true;
    }

    public virtual void Close()
    {
        if (_initialized == false) { Initialize(); }
        container.gameObject.SetActive(false);
        _isOpen = false;
    }
    
}