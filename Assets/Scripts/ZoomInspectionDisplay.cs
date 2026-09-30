using UnityEngine;
using UnityEngine.UI;

// Shows/hides inspection images on either the main panel or alternate panel while zoomed into a ZoomTarget.
public class ZoomInspectionDisplay : MonoBehaviour
{
    public static ZoomInspectionDisplay Instance;

    [Header("Main Panel References")]
    public GameObject mainDisplayPanel;
    public Image mainDisplayImage;

    [Header("Alternate Panel References")]
    public GameObject alternateDisplayPanel;
    public Image alternateDisplayImage;

    void Awake()
    {
        Instance = this;
        HideAll();
    }

    public void Show(Sprite sprite, bool useAlternatePanel)
    {
        HideAll();

        if (sprite == null)
            return;

        if (useAlternatePanel)
        {
            if (alternateDisplayPanel != null && alternateDisplayImage != null)
            {
                alternateDisplayImage.sprite = sprite;
                alternateDisplayPanel.SetActive(true);
            }
        }
        else
        {
            if (mainDisplayPanel != null && mainDisplayImage != null)
            {
                mainDisplayImage.sprite = sprite;
                mainDisplayPanel.SetActive(true);
            }
        }
    }

    public void Hide()
    {
        HideAll();
    }

    private void HideAll()
    {
        if (mainDisplayPanel != null)
            mainDisplayPanel.SetActive(false);

        if (alternateDisplayPanel != null)
            alternateDisplayPanel.SetActive(false);
    }
}