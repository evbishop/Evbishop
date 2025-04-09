#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Evbishop.Editor.Texture_Batch_Editor
{
    public partial class TextureBatchEditor : OdinEditorWindow
    {
        private const string WINDOW = "Tools/Texture Batch Editor";
        private const string COUNT_TEXTURES = "Count textures";
        private const string SET_CRUNCH_COMPRESSION = "Set crunch compression";
        private const string SET_MIP_STREAMING = "Set mip streaming";

        [SerializeField] private List<TextureImporterType> textureImporterTypes = new() { TextureImporterType.Default };

        private delegate void TextureAction(bool isOn, int value, TextureImporter texture, string path, ref int count);

        [MenuItem(WINDOW)]
        private static void OpenWindow()
        {
            GetWindow<TextureBatchEditor>().Show();
        }

        private void SetCrunchCompression(bool isOn, int quality, TextureImporter textureImporter,
            string path, ref int count)
        {
            if (isOn == textureImporter.crunchedCompression &&
                (!isOn || (isOn && quality == textureImporter.compressionQuality)))
                return;

            textureImporter.crunchedCompression = isOn;
            textureImporter.compressionQuality = quality;
            AssetDatabase.ImportAsset(path);
            count++;
        }

        private void SetMipStreaming(bool isOn, int priority, TextureImporter textureImporter,
            string path, ref int count)
        {
            if (isOn == textureImporter.streamingMipmaps &&
                (!isOn || (isOn && priority == textureImporter.streamingMipmapsPriority)))
                return;

            textureImporter.streamingMipmaps = isOn;
            textureImporter.streamingMipmapsPriority = priority;
            AssetDatabase.ImportAsset(path);
            count++;
        }
    }
}
#endif
