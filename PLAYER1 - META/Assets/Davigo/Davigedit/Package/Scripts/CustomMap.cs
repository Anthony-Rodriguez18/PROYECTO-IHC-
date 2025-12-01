using System.IO;
using System.Text;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Davigo.Davigedit
{
    // Custom maps are defined as below, minus the newlines
    // GUID (constant size).
    // Metadata length (int, 4 bytes).
    // Metadata.
    // Image length, if there is an image.
    // Image, if there is an image.
    // Asset bundle until end.
    public class CustomMap : Map, IDisposable
    {
        public enum State { Unloaded, GUID, Metadata, Image, Complete }

        public string GUID { get; private set; }
        public byte[] AssetBundle { get; private set; }

        public override MapIdentifier MapIdentifier => new MapIdentifier() { identifier = GUID, isCustomMap = true };

        private State currentState;
        private FileStream stream;

        private readonly string path;        

        public const string MapDirectory = "Maps";
        public const string MapExtension = "davimap";
        public const int GuidByteCount = 36;

        public CustomMap(string path)
        {
            this.path = path;
        }

        ~CustomMap()
        {
            Dispose();
        }

        public void LoadGUID()
        {
            if (currentState == State.GUID)
                return;

            stream = new FileStream(path, FileMode.Open);

            byte[] GUIDbytes = new byte[GuidByteCount];
            stream.Read(GUIDbytes, 0, GUIDbytes.Length);

            GUID = Encoding.UTF8.GetString(GUIDbytes);

            currentState = State.GUID;
        }

        public void LoadMapSettings()
        {
            if (currentState == State.Metadata)
                return;

            if (currentState == State.Unloaded)
                LoadGUID();

            #region Legacy Support
            byte[] matchBytes = new byte[7];
            stream.Read(matchBytes, 0, matchBytes.Length);

            string matchString = Encoding.UTF8.GetString(matchBytes);

            bool legacy = matchString == "UnityFS";
            stream.Position -= 7;

            if (legacy)
            {
                mapSettings = new MapSettings()
                {
                    Name = Path.GetFileName(path).Replace(".davimap", ""),
                    Description = "Just another legacy Davimap."
                };

                currentState = State.Image;
                return;
            }
            #endregion

            byte[] headerBytes = new byte[sizeof(int)];
            stream.Read(headerBytes, 0, headerBytes.Length);

            int size = BitConverter.ToInt32(headerBytes, 0);

            byte[] mapSettingsBytes = new byte[size];
            stream.Read(mapSettingsBytes, 0, size);

            string json = Encoding.UTF8.GetString(mapSettingsBytes);

            mapSettings = JsonUtility.FromJson<MapSettings>(json);

            if (string.IsNullOrEmpty(mapSettings.Name))
            {
                mapSettings.Name = Path.GetFileName(path).Replace(".davimap", "");
            }

            currentState = State.Metadata;
        }

        public void LoadPreviewImage()
        {
            if (currentState == State.Image)
                return;

            if (currentState == State.Unloaded)
                LoadGUID();

            if (currentState == State.GUID)
                LoadMapSettings();

            if (MapSettings.HasPreviewImage)
            {
                // Debug.Log($"Begin read preview header at position {stream.Position}.");

                byte[] headerBytes = new byte[sizeof(int)];
                stream.Read(headerBytes, 0, headerBytes.Length);

                int size = BitConverter.ToInt32(headerBytes, 0);

                // Debug.Log($"Begin read preview for {size} at position {stream.Position}.");

                byte[] previewImageBytes = new byte[size];
                stream.Read(previewImageBytes, 0, size);

                Texture2D previewImage = new Texture2D(2, 2);
                previewImage.LoadImage(previewImageBytes);

                MapSettings.PreviewImage = previewImage;
            }

            currentState = State.Image;
        }

        public void LoadAssetBytes()
        {
            if (currentState == State.Complete)
                return;

            if (currentState == State.Unloaded)
                LoadGUID();

            if (currentState == State.GUID)
                LoadMapSettings();

            if (currentState == State.Metadata)
                LoadPreviewImage();

            List<byte> bytes = new List<byte>();

            while (true)
            {
                int b = stream.ReadByte();

                if (b == -1)
                    break;

                bytes.Add((byte)b);
            }

            AssetBundle = bytes.ToArray();

            currentState = State.Complete;
        }

        public void Dispose()
        {
            if (stream != null)
                stream.Dispose();
        }

        public static string GetGUIDForMapAtPath(string path)
        {
            string guid;

            using (var metadataReader = new FileStream(path, FileMode.Open))
            {
                byte[] metadataBytes = new byte[GuidByteCount];
                metadataReader.Read(metadataBytes, 0, metadataBytes.Length);

                guid = Encoding.UTF8.GetString(metadataBytes);
            }

            return guid;
        }

        public static bool SearchDirectoriesForMapByGUID(string guid, out string path, params string[] directories)
        {
            for (int i = 0; i < directories.Length; i++)
            {
                if (SearchDirectoryForMapByGUID(guid, out string p, directories[i]))
                {
                    path = p;

                    return true;
                }
            }

            path = "";
            return false;
        }

        public static bool SearchDirectoryForMapByGUID(string guid, out string path, string directory)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(directory);
            List<FileInfo> mapFiles = new List<FileInfo>(directoryInfo.GetFiles($"*.{MapExtension}"));

            foreach (FileInfo file in mapFiles)
            {
                string fileGuid = GetGUIDForMapAtPath(file.FullName);

                if (fileGuid == guid)
                {
                    path = file.FullName;
                    return true;
                }
            }

            path = "";
            return false;
        }

        public static void Write(string path, Guid guid, MapSettings mapSettings, byte[] assetBundle)
        {
            using (var fileStream = File.Create(path))
            {
                string guidString = guid.ToString();
                byte[] guidBytes = Encoding.UTF8.GetBytes(guidString);

                mapSettings.HasPreviewImage = mapSettings.PreviewImage != null;

                string mapSettingsJson = JsonUtility.ToJson(mapSettings, true);
                byte[] mapSettingsBytes = Encoding.UTF8.GetBytes(mapSettingsJson);
                byte[] mapSettingsHeaderBytes = BitConverter.GetBytes(mapSettingsBytes.Length);

                fileStream.Write(guidBytes, 0, guidBytes.Length);
                fileStream.Write(mapSettingsHeaderBytes, 0, mapSettingsHeaderBytes.Length);
                fileStream.Write(mapSettingsBytes, 0, mapSettingsBytes.Length);

                if (mapSettings.HasPreviewImage)
                {
                    byte[] imageBytes = mapSettings.PreviewImage.EncodeToPNG();
                    byte[] imageBytesHeader = BitConverter.GetBytes(imageBytes.Length);

                    // Debug.Log($"Write preview header at position {fileStream.Position}.");
                    fileStream.Write(imageBytesHeader, 0, imageBytesHeader.Length);

                    // Debug.Log($"Write preview for {imageBytes.Length} at position {fileStream.Position}.");
                    fileStream.Write(imageBytes, 0, imageBytes.Length);
                }

                fileStream.Write(assetBundle, 0, assetBundle.Length);

                fileStream.Close();
            }
        }
    }
}
