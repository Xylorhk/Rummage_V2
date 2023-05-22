using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class WarningMessageUI : MonoBehaviour
{
    public string menuClick = string.Empty;
    public TextMeshProUGUI warningMessageTextMesh;
    public Button buttonYes;
    public Button buttonNo;

    public void SetWarning(string warningMessage, UnityEngine.Events.UnityAction yesDelegate, UnityEngine.Events.UnityAction noDelegate)
    {
        warningMessageTextMesh.text = warningMessage;
        AddWarningDelegate(yesDelegate, noDelegate);

        buttonYes.onClick.AddListener(delegate { ClearWarning(); });
        buttonNo.onClick.AddListener(delegate { ClearWarning(); });

        buttonYes.onClick.AddListener(delegate { if (menuClick != string.Empty) { AudioManager.Get().Play(menuClick); } });
        buttonNo.onClick.AddListener(delegate { if (menuClick != string.Empty) { AudioManager.Get().Play(menuClick); } });
    }

    public void AddWarningDelegate(UnityEngine.Events.UnityAction yesDelegate, UnityEngine.Events.UnityAction noDelegate)
    {
        buttonYes.onClick.AddListener(yesDelegate);
        buttonNo.onClick.AddListener(noDelegate);
    }

    public void ClearWarning()
    {
        buttonYes.onClick.RemoveAllListeners();
        buttonNo.onClick.RemoveAllListeners();
        this.gameObject.SetActive(false);
    }
}
