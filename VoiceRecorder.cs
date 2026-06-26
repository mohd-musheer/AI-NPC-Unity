//using System;
//using System.Collections;
//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.Networking;
//using TMPro;

//public class VoiceRecorder : MonoBehaviour
//{
//    [Header("UI")]
//    public Button startButton;
//    public Button stopButton;
//    public TMP_Text statusText;

//    [Header("Chat UI (reuse existing objects)")]
//    public TMP_InputField inputField;
//    public TMP_Text dialogueText;

//    [Header("NPC")]
//    public NpcChatClient npcChat;     // holds NPCMovement + receives PerformAction
//    public AudioSource npcAudioSource; // optional; auto-created on NPC if null

//    [Header("Server")]
//    public string speechUrl = "http://3.107.103.27:8000/speech";

//    AudioClip recordedClip;
//    string microphoneName;
//    bool recording = false;
//    bool busy = false;

//    [Serializable]
//    public class SpeechResponse
//    {
//        public string transcript;
//        public string dialogue;
//        public string action;
//        public string audio;
//    }

//    void Awake()
//    {
//        Debug.Log("VoiceRecorder Awake");
//    }

//    void Start()
//    {
//        Debug.Log("VoiceRecorder Started");

//        if (Microphone.devices.Length == 0)
//        {
//            statusText.text = "No microphone";
//            return;
//        }

//        microphoneName = Microphone.devices[0];
//        Debug.Log("Microphone: " + microphoneName);

//        stopButton.interactable = false;
//        statusText.text = "Microphone Ready";
//    }

//    public void StartRecording()
//    {
//        Debug.Log("START CLICK");

//        if (Microphone.devices.Length == 0)
//        {
//            Debug.Log("NO MIC");
//            return;
//        }

//        if (recording)
//            return;

//        recording = true;

//        recordedClip = Microphone.Start(
//            microphoneName,
//            false,
//            20,
//            44100
//        );

//        startButton.interactable = false;
//        stopButton.interactable = true;
//        statusText.text = "Recording...";

//        Debug.Log("Start recording func last line");
//    }

//    public void StopRecording()
//    {
//        Debug.Log("Stop BUTTON CLICKED");

//        if (!recording)
//            return;

//        recording = false;
//        Microphone.End(microphoneName);

//        startButton.interactable = true;
//        stopButton.interactable = false;
//        statusText.text = "Recording Finished";

//        string path = System.IO.Path.Combine(
//            Application.persistentDataPath,
//            "speech.wav"
//        );

//        WavUtility.Save(path, recordedClip);

//        Debug.Log("Saved WAV: " + path);
//        Debug.Log("Samples = " + recordedClip.samples);

//        // After saving, auto-upload.
//        StartCoroutine(UploadSpeech(path));
//    }

//    // ------------------------------------------------------------------
//    // 1. Upload speech.wav as multipart/form-data, field name "file".
//    // ------------------------------------------------------------------
//    IEnumerator UploadSpeech(string path)
//    {
//        if (busy)
//            yield break;

//        busy = true;

//        byte[] wavBytes;

//        try
//        {
//            wavBytes = System.IO.File.ReadAllBytes(path);
//        }
//        catch (Exception e)
//        {
//            ShowError("File read failed: " + e.Message);
//            busy = false;
//            yield break;
//        }

//        statusText.text = "Uploading...";

//        WWWForm form = new WWWForm();
//        form.AddBinaryData(
//            "file",
//            wavBytes,
//            "speech.wav",
//            "audio/wav"
//        );

//        using UnityWebRequest request =
//            UnityWebRequest.Post(speechUrl, form);

//        request.timeout = 120;

//        statusText.text = "Transcribing...";

//        yield return request.SendWebRequest();

//        if (request.result != UnityWebRequest.Result.Success)
//        {
//            ShowError("Upload error: " + request.error);
//            busy = false;
//            yield break;
//        }

//        statusText.text = "NPC Thinking...";

//        string raw = request.downloadHandler.text;
//        Debug.Log("SPEECH RAW JSON = " + raw);

//        SpeechResponse response;

//        try
//        {
//            response = JsonUtility.FromJson<SpeechResponse>(raw);
//        }
//        catch (Exception e)
//        {
//            ShowError("Bad JSON: " + e.Message);
//            busy = false;
//            yield break;
//        }

//        if (response == null)
//        {
//            ShowError("Empty response.");
//            busy = false;
//            yield break;
//        }

//        // Update text fields.
//        UpdateUI(response);

//        // Reuse existing NPC movement logic.
//        if (npcChat != null && !string.IsNullOrEmpty(response.action))
//            npcChat.PerformAction(response.action);
//        else if (npcChat == null)
//            Debug.LogWarning("npcChat not assigned; action skipped.");

//        // Download + play NPC voice.
//        if (!string.IsNullOrEmpty(response.audio))
//        {
//            yield return StartCoroutine(DownloadVoice(response.audio));
//        }
//        else
//        {
//            statusText.text = "Ready";
//        }

//        busy = false;
//    }

//    // ------------------------------------------------------------------
//    // 2. Download response.audio WAV.
//    // ------------------------------------------------------------------
//    IEnumerator DownloadVoice(string audioUrl)
//    {
//        statusText.text = "Downloading Voice...";

//        using UnityWebRequest request =
//            UnityWebRequestMultimedia.GetAudioClip(
//                audioUrl,
//                AudioType.WAV
//            );

//        request.timeout = 120;

//        yield return request.SendWebRequest();

//        if (request.result != UnityWebRequest.Result.Success)
//        {
//            ShowError("Voice download error: " + request.error);
//            yield break;
//        }

//        AudioClip clip = DownloadHandlerAudioClip.GetContent(request);

//        if (clip == null)
//        {
//            ShowError("Voice clip null.");
//            yield break;
//        }

//        PlayVoice(clip);
//    }

//    // ------------------------------------------------------------------
//    // 3. Play downloaded WAV on AudioSource attached to NPC.
//    // ------------------------------------------------------------------
//    void PlayVoice(AudioClip clip)
//    {
//        AudioSource source = ResolveAudioSource();

//        if (source == null)
//        {
//            ShowError("No AudioSource available.");
//            return;
//        }

//        source.Stop();
//        source.clip = clip;
//        source.Play();

//        statusText.text = "Playing Voice...";

//        // Return to Ready after clip finishes.
//        StartCoroutine(ReadyAfter(clip.length));
//    }

//    AudioSource ResolveAudioSource()
//    {
//        if (npcAudioSource != null)
//            return npcAudioSource;

//        // Attach to NPC GameObject if possible, else self.
//        GameObject host =
//            npcChat != null ? npcChat.gameObject : gameObject;

//        npcAudioSource = host.GetComponent<AudioSource>();

//        if (npcAudioSource == null)
//        {
//            npcAudioSource = host.AddComponent<AudioSource>();
//            npcAudioSource.playOnAwake = false;
//            Debug.Log("AudioSource auto-added to " + host.name);
//        }

//        return npcAudioSource;
//    }

//    IEnumerator ReadyAfter(float seconds)
//    {
//        yield return new WaitForSeconds(seconds);

//        if (!recording && !busy)
//            statusText.text = "Ready";
//    }

//    // ------------------------------------------------------------------
//    // 4. Update input field + dialogue text.
//    // ------------------------------------------------------------------
//    void UpdateUI(SpeechResponse response)
//    {
//        if (inputField != null)
//            inputField.text = response.transcript;

//        if (dialogueText != null)
//        {
//            dialogueText.text =
//                "You: " + response.transcript +
//                "\n\nNPC: " + response.dialogue;
//        }
//    }

//    // ------------------------------------------------------------------
//    // Error helper.
//    // ------------------------------------------------------------------
//    void ShowError(string message)
//    {
//        Debug.LogError(message);

//        if (statusText != null)
//            statusText.text = "Error: " + message;
//    }
//}


using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;

public class VoiceRecorder : MonoBehaviour
{
    [Header("UI")]
    public Button startButton;
    public Button stopButton;
    public TMP_Text statusText;

    [Header("Chat UI (reuse existing objects)")]
    public TMP_InputField inputField;
    public TMP_Text dialogueText;

    [Header("NPC")]
    public NpcChatClient npcChat;     // holds NPCMovement + receives PerformAction
    public AudioSource npcAudioSource; // optional; auto-created on NPC if null

    [Header("Server")]
    public string speechUrl = "http://3.107.103.27:8000/speech";

    AudioClip recordedClip;
    string microphoneName;
    bool recording = false;
    bool busy = false;

    [Serializable]
    public class SpeechResponse
    {
        public string transcript;
        public string dialogue;
        public string action;
        public string audio;
    }

    void Awake()
    {
        Debug.Log("VoiceRecorder Awake");
    }

    void Start()
    {
        Debug.Log("VoiceRecorder Started");

        if (Microphone.devices.Length == 0)
        {
            statusText.text = "No microphone";
            return;
        }

        microphoneName = Microphone.devices[0];
        Debug.Log("Microphone: " + microphoneName);

        stopButton.interactable = false;
        statusText.text = "Microphone Ready";
    }

    public void StartRecording()
    {
        Debug.Log("START CLICK");

        if (Microphone.devices.Length == 0)
        {
            Debug.Log("NO MIC");
            return;
        }

        if (recording)
            return;

        recording = true;

        recordedClip = Microphone.Start(
            microphoneName,
            false,
            20,
            44100
        );

        startButton.interactable = false;
        stopButton.interactable = true;
        statusText.text = "Recording...";

        Debug.Log("Start recording func last line");
    }

    public void StopRecording()
    {
        Debug.Log("Stop BUTTON CLICKED");

        if (!recording)
            return;

        recording = false;
        Microphone.End(microphoneName);

        startButton.interactable = true;
        stopButton.interactable = false;
        statusText.text = "Recording Finished";

        string path = System.IO.Path.Combine(
            Application.persistentDataPath,
            "speech.wav"
        );

        WavUtility.Save(path, recordedClip);

        Debug.Log("Saved WAV: " + path);
        Debug.Log("Samples = " + recordedClip.samples);

        // After saving, auto-upload.
        StartCoroutine(UploadSpeech(path));
    }

    // ------------------------------------------------------------------
    // 1. Upload speech.wav as multipart/form-data, field name "file".
    // ------------------------------------------------------------------
    IEnumerator UploadSpeech(string path)
    {
        if (busy)
            yield break;

        busy = true;

        byte[] wavBytes;

        try
        {
            wavBytes = System.IO.File.ReadAllBytes(path);
        }
        catch (Exception e)
        {
            ShowError("File read failed: " + e.Message);
            busy = false;
            yield break;
        }

        statusText.text = "Uploading...";

        WWWForm form = new WWWForm();
        form.AddBinaryData(
            "file",
            wavBytes,
            "speech.wav",
            "audio/wav"
        );

        using UnityWebRequest request =
            UnityWebRequest.Post(speechUrl, form);

        request.timeout = 120;

        statusText.text = "Transcribing...";

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            ShowError("Upload error: " + request.error);
            busy = false;
            yield break;
        }

        statusText.text = "NPC Thinking...";

        string raw = request.downloadHandler.text;
        Debug.Log("SPEECH RAW JSON = " + raw);

        SpeechResponse response;

        try
        {
            response = JsonUtility.FromJson<SpeechResponse>(raw);
        }
        catch (Exception e)
        {
            ShowError("Bad JSON: " + e.Message);
            busy = false;
            yield break;
        }

        if (response == null)
        {
            ShowError("Empty response.");
            busy = false;
            yield break;
        }

        // Update text fields.
        UpdateUI(response);

        // Reuse existing NPC movement logic.
        if (npcChat != null && !string.IsNullOrEmpty(response.action))
            npcChat.PerformAction(response.action);
        else if (npcChat == null)
            Debug.LogWarning("npcChat not assigned; action skipped.");

        // Download + play NPC voice.
        if (!string.IsNullOrEmpty(response.audio))
        {
            yield return StartCoroutine(DownloadVoice(response.audio));
        }
        else
        {
            statusText.text = "Ready";
        }

        busy = false;
    }

    // ------------------------------------------------------------------
    // 2. Download response.audio WAV.
    // ------------------------------------------------------------------
    IEnumerator DownloadVoice(string audioUrl)
    {
        statusText.text = "Downloading Voice...";

        using UnityWebRequest request =
            UnityWebRequestMultimedia.GetAudioClip(
                audioUrl,
                AudioType.WAV
            );

        request.timeout = 120;

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            ShowError("Voice download error: " + request.error);
            yield break;
        }

        AudioClip clip = DownloadHandlerAudioClip.GetContent(request);

        if (clip == null)
        {
            ShowError("Voice clip null.");
            yield break;
        }

        PlayVoice(clip);
    }

    // ------------------------------------------------------------------
    // 3. Play downloaded WAV on AudioSource attached to NPC.
    // ------------------------------------------------------------------
    void PlayVoice(AudioClip clip)
    {
        AudioSource source = ResolveAudioSource();

        if (source == null)
        {
            ShowError("No AudioSource available.");
            return;
        }

        source.Stop();
        source.clip = clip;
        source.Play();

        statusText.text = "Playing Voice...";

        // Return to Ready after clip finishes.
        StartCoroutine(ReadyAfter(clip.length));
    }

    AudioSource ResolveAudioSource()
    {
        if (npcAudioSource != null)
            return npcAudioSource;

        // Attach to NPC GameObject if possible, else self.
        GameObject host =
            npcChat != null ? npcChat.gameObject : gameObject;

        npcAudioSource = host.GetComponent<AudioSource>();

        if (npcAudioSource == null)
        {
            npcAudioSource = host.AddComponent<AudioSource>();
            npcAudioSource.playOnAwake = false;
            Debug.Log("AudioSource auto-added to " + host.name);
        }

        return npcAudioSource;
    }

    IEnumerator ReadyAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        if (!recording && !busy)
            statusText.text = "Ready";
    }

    // ------------------------------------------------------------------
    // 4. Update input field + dialogue text.
    // ------------------------------------------------------------------
    void UpdateUI(SpeechResponse response)
    {
        if (inputField != null)
            inputField.text = response.transcript;

        if (dialogueText != null)
        {
            dialogueText.text =
                "You: " + response.transcript +
                "\n\nNPC: " + response.dialogue;
        }
    }

    // ------------------------------------------------------------------
    // Error helper.
    // ------------------------------------------------------------------
    void ShowError(string message)
    {
        Debug.LogError(message);

        if (statusText != null)
            statusText.text = "Error: " + message;
    }
}
