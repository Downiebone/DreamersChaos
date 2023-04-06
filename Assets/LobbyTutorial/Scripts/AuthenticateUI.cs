using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AuthenticateUI : MonoBehaviour {


    [SerializeField] private Button authenticateButton;


    private void Awake() {
        authenticateButton.onClick.AddListener(() => {
            if (EditPlayerName.Instance.GetPlayerName() == string.Empty)
            {
                //ui to say you need a name
                return;
            }
            LobbyManager.Instance.Authenticate(EditPlayerName.Instance.GetPlayerName());
            Hide();
        });
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

}