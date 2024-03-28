using Blayms.MEA.Utils;
using Blayms.MEA.Utils.ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Blayms.MEA
{
    internal static class Internal
    {
        internal static string AssetNameOf(ZipEntry zipEntry)
        {
            return $"{ModExtraAssets.ToolAcronym}_{Path.GetFileNameWithoutExtension(zipEntry.Name)}";
        }
        internal static Texture2D Texture2DFromZip(ZipEntry zipEntry, byte[] bytes)
        {
            Texture2D texture2D = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            texture2D.filterMode = FilterMode.Point;
            texture2D.LoadImage(bytes);
            texture2D.Apply();
            texture2D.name = AssetNameOf(zipEntry);

            return texture2D;
        }
        internal static Sprite SpriteFromZip(ZipEntry zipEntry, byte[] bytes)
        {
            string[] splitName = Path.GetFileNameWithoutExtension(zipEntry.Name).Split('!');
            int pixelsPerUnit = 100;
            if (splitName.Length != 2)
            {
                Debug.LogWarning("In order to create sprite properly, you must have !int at the end of your .png file name! The sprite creation proccess won't be terminated anyway. It's pixelsPerUnit will be set to defaults, which is 100");
            }
            else
            {
                pixelsPerUnit = int.Parse(splitName[1]);
            }
            Texture2D texture2D = Texture2DFromZip(zipEntry, bytes);

            Sprite sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.one / 2, pixelsPerUnit);
            sprite.name = AssetNameOf(zipEntry).Split('!')[0];
            return sprite;
        }
        internal static Cubemap CubemapFromZip(ZipEntry zipEntry, byte[] bytes)
        {
            Texture2D texture2D = Texture2DFromZip(zipEntry, bytes);
            int d = 0;
            List<Tuple<Color[], CubemapFace>> temp = new List<Tuple<Color[], CubemapFace>>();
            switch (AspectRatioOfResolution(texture2D.width, texture2D.height))
            {
                case 0.17f: // looks like a straight stick
                    d = texture2D.width;
                    temp.Add(new Tuple<Color[], CubemapFace>(texture2D.GetPixels(0, 0, d, d), CubemapFace.PositiveX));
                    temp.Add(new Tuple<Color[], CubemapFace>(texture2D.GetPixels(0, d, d, d), CubemapFace.NegativeX));
                    temp.Add(new Tuple<Color[], CubemapFace>(texture2D.GetPixels(0, d * 2, d, d), CubemapFace.PositiveY));
                    temp.Add(new Tuple<Color[], CubemapFace>(texture2D.GetPixels(0, d * 3, d, d), CubemapFace.NegativeY));
                    temp.Add(new Tuple<Color[], CubemapFace>(texture2D.GetPixels(0, d * 4, d, d), CubemapFace.PositiveZ));
                    temp.Add(new Tuple<Color[], CubemapFace>(texture2D.GetPixels(0, d * 5, d, d), CubemapFace.NegativeZ));
                    break;
                case 6: // looks like a laying stick
                    d = texture2D.width / 6;
                    temp.Add(new Tuple<Color[], CubemapFace>(texture2D.GetPixels(0, 0, d, d), CubemapFace.PositiveX));
                    temp.Add(new Tuple<Color[], CubemapFace>(texture2D.GetPixels(d, 0, d, d), CubemapFace.NegativeX));
                    temp.Add(new Tuple<Color[], CubemapFace>(texture2D.GetPixels(d * 2, 0, d, d), CubemapFace.PositiveY));
                    temp.Add(new Tuple<Color[], CubemapFace>(texture2D.GetPixels(d * 2, 0, d, d), CubemapFace.NegativeY));
                    temp.Add(new Tuple<Color[], CubemapFace>(texture2D.GetPixels(d * 4, 0, d, d), CubemapFace.PositiveZ));
                    temp.Add(new Tuple<Color[], CubemapFace>(texture2D.GetPixels(d * 5, 0, d, d), CubemapFace.NegativeZ));
                    break;
                case 0.75f: // looks like a normally rotated cross
                    d = texture2D.width / 3;
                    temp.Add(new Tuple<Color[], CubemapFace>(texture2D.GetPixels(d * 2, d, d, d), CubemapFace.PositiveX));
                    temp.Add(new Tuple<Color[], CubemapFace>(texture2D.GetPixels(0, d, d, d), CubemapFace.NegativeX));
                    temp.Add(new Tuple<Color[], CubemapFace>(texture2D.GetPixels(d, 0, d, d), CubemapFace.PositiveY));
                    temp.Add(new Tuple<Color[], CubemapFace>(texture2D.GetPixels(d, d * 2, d, d), CubemapFace.NegativeY));
                    temp.Add(new Tuple<Color[], CubemapFace>(texture2D.GetPixels(d, d, d, d), CubemapFace.PositiveZ));
                    temp.Add(new Tuple<Color[], CubemapFace>(texture2D.GetPixels(d, d * 3, d, d), CubemapFace.NegativeZ));
                    break;
                case 0.14f: // looks like a laying cross
                    d = texture2D.width / 4;
                    temp.Add(new Tuple<Color[], CubemapFace>(texture2D.GetPixels(d * 2, d, d, d), CubemapFace.PositiveX));
                    temp.Add(new Tuple<Color[], CubemapFace>(texture2D.GetPixels(0, d, d, d), CubemapFace.NegativeX));
                    temp.Add(new Tuple<Color[], CubemapFace>(texture2D.GetPixels(d, 0, d, d), CubemapFace.PositiveY));
                    temp.Add(new Tuple<Color[], CubemapFace>(texture2D.GetPixels(d, d * 2, d, d), CubemapFace.NegativeY));
                    temp.Add(new Tuple<Color[], CubemapFace>(texture2D.GetPixels(d, d, d, d), CubemapFace.PositiveZ));
                    temp.Add(new Tuple<Color[], CubemapFace>(texture2D.GetPixels(d * 3, d, d, d), CubemapFace.NegativeZ));
                    break;
            }

            Cubemap cubemap = new Cubemap(d, TextureFormat.ARGB32, false);
            cubemap.name = AssetNameOf(zipEntry).Split('!')[0];
            cubemap.filterMode = FilterMode.Point;

            for (int i = 0; i < temp.Count; i++)
            {
                cubemap.SetPixels(temp[i].Item1, temp[i].Item2);
            }

            cubemap.Apply();

            return cubemap;
        }
        internal static AudioClip AudioClipFromZip(ZipEntry zipEntry, byte[] bytes)
        {
            AudioClip audioClip = WavUtility.ToAudioClip(bytes);
            audioClip.name = AssetNameOf(zipEntry);
            return audioClip;
        }
        internal static Mesh MeshFromZip(ZipEntry zipEntry, string data)
        {
            Mesh mesh = new ObjUtility().Load(data);
            mesh.name = AssetNameOf(zipEntry);
            return mesh;
        }
        internal static object TryDeserializingJson(string path, MEAZipLoadingProcedure zipLoading, string json, Type jsonType, object[] args)
        {
            try
            {
                return zipLoading.JsonDeserializeFunction?.Invoke(json, jsonType, args);
            }
            catch(Exception ex)
            {
                throw new Exceptions.JsonDeserializationFailException(path, ex);
            }
        }
        internal static float AspectRatioOfResolution(int w, int h)
        {
            float aspectRatio = (float)w / (float)h;
            return (float)Math.Round(aspectRatio, 2);
        }
        internal static string MeshStringCleanUp(string str)
        {
            string rstr = str.Replace('\t', ' ');
            while (rstr.Contains("  "))
                rstr = rstr.Replace("  ", " ");
            return rstr.Trim();
        }
        internal static string GetFileSubDirectory(string filePath)
        {
            string dirPath = Path.GetDirectoryName(filePath);
            return dirPath.Split(Path.DirectorySeparatorChar).Last();
        }
        internal static Type GetTypeFromFileEnd(string filePath, bool mightContainExtra = true)
        {
            string ext = Path.GetExtension(filePath);
            switch (ext)
            {
                case ".png":
                    if (mightContainExtra)
                    {
                        string[] splitName = Path.GetFileNameWithoutExtension(filePath).Split('!');
                        if (splitName.Length == 2)
                        {
                            return splitName[1] == "c" ? typeof(Cubemap) : typeof(Sprite);
                        }
                    }
                    return typeof(Texture2D);
                case ".wav": return typeof(AudioClip);
                case ".obj": return typeof(Mesh);
            }
            return typeof(object);
        }
        internal static void UnityLog<T>(this IList<T> list)
        {
            string log = $"IList of type {typeof(T)}: [\n";
            for (int i = 0; i < list.Count; i++)
            {
                log += $"{i} = {list[i]} {(i == 0 ? "," : "")}\n";
            }
            log += "]";
            Debug.Log(log);
        }
    }
}
