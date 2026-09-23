using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance;

    [System.Serializable]
    public class CursorData
    {
        public string id;

        public Sprite sprite;

        [Header("Appearance")]
        public Vector2 size = new Vector2(48f, 48f);

        public float rotation;

        public Vector2 offset;
    }

    [Header("Cursor Setup")]
    public Image cursorImage;

    [Header("Cursors")]
    public List<CursorData> cursors = new List<CursorData>();

    private Dictionary<string, CursorData> cursorLookup;

    private string currentCursor;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        cursorLookup = new Dictionary<string, CursorData>();

        foreach (CursorData cursor in cursors)
        {
            if (!cursorLookup.ContainsKey(cursor.id))
            {
                cursorLookup.Add(cursor.id, cursor);
            }
        }

        Cursor.visible = false;
    }

    void Start()
    {
        SetCursor("Default");
    }

    void Update()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        cursorImage.rectTransform.position =
            mousePos;
    }

    public void SetCursor(string id)
    {
        if (currentCursor == id)
            return;

        if (cursorLookup.TryGetValue(id, out CursorData data))
        {
            cursorImage.sprite = data.sprite;

            cursorImage.rectTransform.sizeDelta =
                data.size;

            cursorImage.rectTransform.rotation =
                Quaternion.Euler(0f, 0f, data.rotation);

            cursorImage.rectTransform.anchoredPosition +=
                data.offset;

            currentCursor = id;
        }
    }
}