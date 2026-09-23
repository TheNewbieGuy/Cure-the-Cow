using UnityEngine;
using UnityEngine.UI;

// Shows/hides the single on-screen inspection image while zoomed into a
// ZoomTarget. One shared instance - individual zoom points don't need
// their own Image reference.
public class ZoomInspectionDisplay : MonoBehaviour
{
    public static ZoomInspectionDisplay Instance;

    [Header("References")]
    public GameObject displayPanel;
    public Image displayImage;

    void Awake()
    {
        Instance = this;

        if (displayPanel != null)
            displayPanel.SetActive(false);
    }

    public void Show(Sprite sprite)
    {
        if (displayPanel == null || displayImage == null)
            return;

        if (sprite == null)
        {
            Hide();
            return;
        }

        displayImage.sprite = sprite;
        displayPanel.SetActive(true);
    }

    public void Hide()
    {
        if (displayPanel != null)
            displayPanel.SetActive(false);
    }
}
