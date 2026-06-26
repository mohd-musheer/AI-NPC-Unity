//using System;
//using System.Collections;
//using System.Text;
//using TMPro;
//using UnityEngine;
//using UnityEngine.Networking;
//using UnityEngine.UI;

//public class NpcChatClient : MonoBehaviour
//{
//    [Header("API")]
//    public string apiUrl = "http://3.107.103.27:8000/chat";

//    [Header("UI")]
//    public TMP_Text dialogueText;
//    public TMP_InputField inputField;
//    public Button sendButton;

//    private NPCMovement npcMovement;
//    private bool waitingForResponse = false;

//    [Serializable]
//    class ChatRequest
//    {
//        public string message;
//    }

//    [Serializable]
//    class NPCResponse
//    {
//        public string action;
//        public string dialogue;
//    }

//    void Start()
//    {
//        npcMovement = GetComponent<NPCMovement>();

//        sendButton.onClick.AddListener(
//            SendButtonPressed
//        );
//    }

//    void SendButtonPressed()
//    {
//        if (waitingForResponse)
//            return;

//        string msg = inputField.text.Trim();

//        if (msg == "")
//            return;

//        StartCoroutine(
//            SendChat(msg)
//        );
//    }

//    IEnumerator SendChat(string message)
//    {
//        waitingForResponse = true;
//        Debug.Log("Sending to: " + apiUrl);
//        sendButton.interactable = false;

//        dialogueText.text =
//            "You: " + message +
//            "\n\nNPC is thinking...";

//        ChatRequest body =
//            new ChatRequest
//            {
//                message = message
//            };

//        string jsonBody =
//            JsonUtility.ToJson(body);

//        using UnityWebRequest request =
//            new UnityWebRequest(
//                apiUrl,
//                "POST"
//            );

//        request.uploadHandler =
//            new UploadHandlerRaw(
//                Encoding.UTF8.GetBytes(jsonBody)
//            );

//        request.downloadHandler =
//            new DownloadHandlerBuffer();

//        request.SetRequestHeader(
//            "Content-Type",
//            "application/json"
//        );

//        request.timeout = 60;

//        yield return request.SendWebRequest();

//        waitingForResponse = false;

//        sendButton.interactable = true;

//        if (request.result != UnityWebRequest.Result.Success)
//        {
//            dialogueText.text =
//                "Error: " + request.error;

//            Debug.LogError(request.error);

//            yield break;
//        }

//        Debug.Log(
//            "RAW JSON = " +
//            request.downloadHandler.text
//        );

//        NPCResponse response =
//            JsonUtility.FromJson<NPCResponse>(
//                request.downloadHandler.text
//            );

//        if (response == null)
//        {
//            dialogueText.text =
//                "Invalid response.";

//            yield break;
//        }

//        dialogueText.text =
//            "You: " + message +
//            "\n\nNPC: " +
//            response.dialogue;

//        Debug.Log(
//            "ACTION = " +
//            response.action
//        );

//        PerformAction(
//            response.action
//        );

//        inputField.text = "";

//        inputField.ActivateInputField();
//    }

//    void PerformAction(string action)
//    {
//        if (npcMovement == null)
//            return;

//        switch (action.ToLower())
//        {
//            case "wave":
//                npcMovement.Wave();
//                break;

//            case "guide_player":
//            case "follow_player":
//                npcMovement.StartFollowing();
//                break;

//            case "stop":
//            case "idle":
//                npcMovement.StopFollowing();
//                break;

//            case "jump":
//                npcMovement.Jump();
//                break;

//            case "move_forward":
//                npcMovement.MoveForward();
//                break;

//            case "move_backward":
//                npcMovement.MoveBackward();
//                break;

//            case "move_left":
//                npcMovement.MoveLeft();
//                break;

//            case "move_right":
//                npcMovement.MoveRight();
//                break;

//            case "turn_left":
//                npcMovement.RotateLeft();
//                break;

//            case "turn_right":
//                npcMovement.RotateRight();
//                break;

//            default:
//                Debug.Log(
//                    "Unknown Action : " +
//                    action
//                );
//                break;
//        }
//    }
//}


using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NpcChatClient : MonoBehaviour
{
    [Header("API")]
    public string apiUrl = "http://3.107.103.27:8000/chat";

    [Header("UI")]
    public TMP_Text dialogueText;
    public TMP_InputField inputField;
    public Button sendButton;

    private NPCMovement npcMovement;
    private bool waitingForResponse = false;

    [Serializable]
    class ChatRequest
    {
        public string message;
    }

    [Serializable]
    class NPCResponse
    {
        public string action;
        public string dialogue;
    }

    void Start()
    {
        npcMovement = GetComponent<NPCMovement>();

        sendButton.onClick.AddListener(
            SendButtonPressed
        );
    }

    void SendButtonPressed()
    {
        if (waitingForResponse)
            return;

        string msg = inputField.text.Trim();

        if (msg == "")
            return;

        StartCoroutine(
            SendChat(msg)
        );
    }

    IEnumerator SendChat(string message)
    {
        waitingForResponse = true;
        Debug.Log("Sending to: " + apiUrl);
        sendButton.interactable = false;

        dialogueText.text =
            "You: " + message +
            "\n\nNPC is thinking...";

        ChatRequest body =
            new ChatRequest
            {
                message = message
            };

        string jsonBody =
            JsonUtility.ToJson(body);

        using UnityWebRequest request =
            new UnityWebRequest(
                apiUrl,
                "POST"
            );

        request.uploadHandler =
            new UploadHandlerRaw(
                Encoding.UTF8.GetBytes(jsonBody)
            );

        request.downloadHandler =
            new DownloadHandlerBuffer();

        request.SetRequestHeader(
            "Content-Type",
            "application/json"
        );

        request.timeout = 60;

        yield return request.SendWebRequest();

        waitingForResponse = false;

        sendButton.interactable = true;

        if (request.result != UnityWebRequest.Result.Success)
        {
            dialogueText.text =
                "Error: " + request.error;

            Debug.LogError(request.error);

            yield break;
        }

        Debug.Log(
            "RAW JSON = " +
            request.downloadHandler.text
        );

        NPCResponse response =
            JsonUtility.FromJson<NPCResponse>(
                request.downloadHandler.text
            );

        if (response == null)
        {
            dialogueText.text =
                "Invalid response.";

            yield break;
        }

        dialogueText.text =
            "You: " + message +
            "\n\nNPC: " +
            response.dialogue;

        Debug.Log(
            "ACTION = " +
            response.action
        );

        PerformAction(
            response.action
        );

        inputField.text = "";

        inputField.ActivateInputField();
    }

    public void PerformAction(string action)
    {
        if (npcMovement == null)
            return;

        switch (action.ToLower())
        {
            case "wave":
                npcMovement.Wave();
                break;

            case "guide_player":
            case "follow_player":
                npcMovement.StartFollowing();
                break;

            case "stop":
            case "idle":
                npcMovement.StopFollowing();
                break;

            case "jump":
                npcMovement.Jump();
                break;

            case "move_forward":
                npcMovement.MoveForward();
                break;

            case "move_backward":
                npcMovement.MoveBackward();
                break;

            case "move_left":
                npcMovement.MoveLeft();
                break;

            case "move_right":
                npcMovement.MoveRight();
                break;

            case "turn_left":
                npcMovement.RotateLeft();
                break;

            case "turn_right":
                npcMovement.RotateRight();
                break;

            default:
                Debug.Log(
                    "Unknown Action : " +
                    action
                );
                break;
        }
    }
}