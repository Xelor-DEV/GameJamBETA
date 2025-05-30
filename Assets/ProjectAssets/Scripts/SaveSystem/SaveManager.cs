using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using UnityEngine;

public static class SaveManager
{
    private static readonly string audioPath = Application.persistentDataPath + "/audioConfig.dat";
    private static readonly string settingsPath = Application.persistentDataPath + "/gameSettings.dat";

    private static readonly byte[] Key = {
        0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
        0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10,
        0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18,
        0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F, 0x20
    };

    public static void SaveAudioConfig(AudioConfig config)
    {
        SaveData(audioPath, config.ToData());
    }

    public static AudioConfigData LoadAudioConfig()
    {
        return LoadData<AudioConfigData>(audioPath);
    }

    public static void SaveGameSettings(GameSettings settings)
    {
        SaveData(settingsPath, settings.ToData());
    }

    public static GameSettingsData LoadGameSettings()
    {
        return LoadData<GameSettingsData>(settingsPath);
    }

    public static void SaveAudioConfig(AudioConfigData configData)
    {
        SaveData(audioPath, configData);
    }

    public static void SaveGameSettings(GameSettingsData settingsData)
    {
        SaveData(settingsPath, settingsData);
    }

    private static void SaveData<T>(string path, T data)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Key;
            aes.GenerateIV();

            using (FileStream fileStream = new FileStream(path, FileMode.Create))
            using (CryptoStream cryptoStream = new CryptoStream(
                fileStream,
                aes.CreateEncryptor(),
                CryptoStreamMode.Write))
            {
                // Escribir IV al inicio del archivo
                fileStream.Write(aes.IV, 0, aes.IV.Length);

                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(cryptoStream, data);
            }
        }
    }

    private static T LoadData<T>(string path) where T : class
    {
        if (!File.Exists(path)) return null;

        try
        {
            using (FileStream fileStream = new FileStream(path, FileMode.Open))
            {
                byte[] iv = new byte[16];
                fileStream.Read(iv, 0, iv.Length);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = Key;
                    aes.IV = iv;

                    using (CryptoStream cryptoStream = new CryptoStream(
                        fileStream,
                        aes.CreateDecryptor(),
                        CryptoStreamMode.Read))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        return (T)formatter.Deserialize(cryptoStream);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading data: {e.Message}");
            return null;
        }
    }
}