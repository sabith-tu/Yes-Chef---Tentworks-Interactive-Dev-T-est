using UnityEngine;
using UnityEngine.UI;

public class PrepSlotUI : MonoBehaviour
{
    public Image fillBar;

    public GameObject uiContainer;

    public void UpdateProgress(float currentTime, float totalTime)
    {
        if (!uiContainer.activeSelf)
            uiContainer.SetActive(true);
        fillBar.fillAmount = currentTime / totalTime;
    }

    public void HideUI()
    {
        uiContainer.SetActive(false);
    }
}
