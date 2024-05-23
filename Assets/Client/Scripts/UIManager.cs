using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Interactable impairedButton;
    [SerializeField] private Interactable observerButton;

    private void Start()
    {
        impairedButton.OnClick.AddListener(() => SetPlayerType(true));
        observerButton.OnClick.AddListener(() => SetPlayerType(false));
    }

    private void SetPlayerType(bool isImpaired)
    {
        NetworkManager.Singleton.SetPlayerType(isImpaired);
        gameObject.SetActive(false); // Hide the UI after selection
    }
}