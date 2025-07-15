using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    public List<UnityEngine.Object> objectsToSave = new();
    public bool autoLoadOnStart = true;
    public bool useEncryption = true;

    private string path => Path.Combine(Application.persistentDataPath, "savefile.json");
    private const string encryptionKey = "MySecretKey";

    private void Awake()
    {
        if (Instance != null) Destroy(this);
        Instance = this;

        if (autoLoadOnStart) LoadData();
    }

    public void RegisterObject(UnityEngine.Object obj)
    {
        if (obj is ISaveFuncs)
        {
            if (!objectsToSave.Contains(obj))
            {
                objectsToSave.Add(obj);
            }
            else
            {
                Debug.LogWarning("Object already registered: " + obj.name);
            }
        }
        else
        {
            Debug.LogError("Object does not implement ISaveFuncs: " + obj.name);
        }
    }

    public void SaveData()
    {
        var saveFile = new SaveFile();

        foreach (var obj in objectsToSave)
        {
            if (obj is ISaveFuncs saveable)
            {
                var data = saveable.SaveData();
                var entry = new SaveEntry
                {
                    id = saveable.id,
                    type = data.GetType().AssemblyQualifiedName,
                    jsonData = JsonUtility.ToJson(data)
                };

                saveFile.entries.Add(entry);
            }
            else
            {
                Debug.LogWarning("Object does not implement ISaveFuncs: " + obj.name);
            }
        }

        string json = JsonUtility.ToJson(saveFile, true);
        if (useEncryption) json = Encrypt(json);
        File.WriteAllText(path, json);
        
        Debug.Log("Data Saved to: " + path);
    }

    public void LoadData()
    {
        if (!File.Exists(path))
        {
            Debug.LogError("No save file found at " + path);
            return;
        }

        string json = File.ReadAllText(path);
        if (useEncryption) json = Decrypt(json);
        var saveFile = JsonUtility.FromJson<SaveFile>(json);

        foreach (var entry in saveFile.entries)
        {
            foreach (var obj in objectsToSave)
            {
                if (obj is ISaveFuncs saveable && saveable.id == entry.id)
                {
                    var dataType = System.Type.GetType(entry.type);
                    var data = JsonUtility.FromJson(entry.jsonData, dataType);
                    saveable.LoadData(data);
                }
            }
        }

        Debug.Log("Data Loaded from: " + path);
    }

    public void LoadDataForObject(UnityEngine.Object checkObj)
    {
        if (!File.Exists(path))
        {
            Debug.LogError("No save file found at " + path);
            return;
        }

        string json = File.ReadAllText(path);
        if (useEncryption) json = Decrypt(json);
        var saveFile = JsonUtility.FromJson<SaveFile>(json);

        foreach (var entry in saveFile.entries)
        {
            foreach (var obj in objectsToSave)
            {
                if (obj is ISaveFuncs saveable && saveable.id == entry.id && obj == checkObj)
                {
                    var dataType = System.Type.GetType(entry.type);
                    var data = JsonUtility.FromJson(entry.jsonData, dataType);
                    saveable.LoadData(data);
                }
            }
        }

        Debug.Log("Data Loaded for: " + checkObj.name);
    }

    public static string Encrypt(string plainText)
    {
        byte[] key = Encoding.UTF8.GetBytes(encryptionKey.PadRight(32).Substring(0, 32)); // AES-256
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.GenerateIV(); // Random IV for security
            using var encryptor = aes.CreateEncryptor();

            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] encrypted = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            // Combine IV + encrypted data
            byte[] result = new byte[aes.IV.Length + encrypted.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(encrypted, 0, result, aes.IV.Length, encrypted.Length);

            return Convert.ToBase64String(result);
        }
    }

    public static string Decrypt(string encryptedText)
    {
        byte[] fullData = Convert.FromBase64String(encryptedText);
        byte[] key = Encoding.UTF8.GetBytes(encryptionKey.PadRight(32).Substring(0, 32)); // AES-256

        using (Aes aes = Aes.Create())
        {
            aes.Key = key;

            // Extract IV from beginning
            byte[] iv = new byte[aes.BlockSize / 8];
            byte[] cipherText = new byte[fullData.Length - iv.Length];

            Buffer.BlockCopy(fullData, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullData, iv.Length, cipherText, 0, cipherText.Length);

            aes.IV = iv;
            using var decryptor = aes.CreateDecryptor();

            byte[] decrypted = decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);
            return Encoding.UTF8.GetString(decrypted);
        }
    }
}

[System.Serializable]
public class SaveEntry
{
    public string id;
    public string jsonData;
    public string type;
}

[System.Serializable]
public class SaveFile
{
    public List<SaveEntry> entries = new();
}