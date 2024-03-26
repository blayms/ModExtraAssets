/*
 * Copyright (c) 2019 Dummiesman
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
*/

using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;
using System.Globalization;

namespace Blayms.MEA.Utils
{
    internal enum SplitMode
    {
        None,
        Object,
        Material
    }

    internal class ObjUtility
    {

        public SplitMode SplitMode = SplitMode.Object;

        internal List<Vector3> Vertices = new List<Vector3>();
        internal List<Vector3> Normals = new List<Vector3>();
        internal List<Vector2> UVs = new List<Vector2>();

        internal Dictionary<string, Material> Materials;

        private FileInfo _objInfo;
        public Mesh Load(string data)
        {
            var reader = new StringReader(data);

            Dictionary<string, ObjMeshBuilder> builderDict = new Dictionary<string, ObjMeshBuilder>();
            ObjMeshBuilder currentBuilder = null;
            string currentMaterial = "default";

            //lists for face data
            //prevents excess GC
            List<int> vertexIndices = new List<int>();
            List<int> normalIndices = new List<int>();
            List<int> uvIndices = new List<int>();

            //helper func
            Action<string> setCurrentObjectFunc = (objectName) =>
            {
                if (!builderDict.TryGetValue(objectName, out currentBuilder))
                {
                    currentBuilder = new ObjMeshBuilder(objectName, this);
                    builderDict[objectName] = currentBuilder;
                }
            };

            //create default object
            setCurrentObjectFunc.Invoke("default");

            //do the reading
            for (string line = reader.ReadLine(); line != null; line = reader.ReadLine())
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string processedLine = Internal.MeshStringCleanUp(line);
                string[] splitLine = processedLine.Split(' ');

                //comment or blank
                if (processedLine[0] == '#' || splitLine.Length < 2)
                {
                    continue;
                }

                //vtx
                if (splitLine[0] == "v")
                {
                    Vertices.Add(VectorFromStrArray(splitLine));
                    continue;
                }

                //normal
                if (splitLine[0] == "vn")
                {
                    Normals.Add(VectorFromStrArray(splitLine));
                    continue;
                }

                //uv
                if (splitLine[0] == "vt")
                {
                    UVs.Add(VectorFromStrArray(splitLine));
                    continue;
                }

                //new material
                if (splitLine[0] == "usemtl")
                {
                    string materialName = processedLine.Substring(7);
                    currentMaterial = materialName;

                    if (SplitMode == SplitMode.Material)
                    {
                        setCurrentObjectFunc.Invoke(materialName);
                    }
                    continue;
                }

                //new object
                if ((splitLine[0] == "o" || splitLine[0] == "g") && SplitMode == SplitMode.Object)
                {
                    string objectName = processedLine.Substring(2);
                    setCurrentObjectFunc.Invoke(objectName);
                    continue;
                }

                //face data (the fun part)
                if (splitLine[0] == "f")
                {
                    //loop through indices
                    for (int i = 1; i < splitLine.Length; i++)
                    {
                        string faceLoop = splitLine[i];

                        int vertexIndex = int.MinValue;
                        int normalIndex = int.MinValue;
                        int uvIndex = int.MinValue;

                        //parse face loop
                        if (faceLoop.Contains("//"))
                        {
                            //vertex and normal
                            string[] slashSplits = faceLoop.Split('/');
                            vertexIndex = FastIntParse(slashSplits[0]);
                            normalIndex = FastIntParse(slashSplits[2]);
                        }
                        else if (faceLoop.Contains("/"))
                        {
                            //get slash splits
                            string[] slashSplits = faceLoop.Split('/');
                            if (slashSplits.Length > 2)
                            {
                                //vertex, uv, and normal
                                vertexIndex = FastIntParse(slashSplits[0]);
                                uvIndex = FastIntParse(slashSplits[1]);
                                normalIndex = FastIntParse(slashSplits[2]);
                            }
                            else
                            {
                                //vertex, and uv
                                vertexIndex = FastIntParse(slashSplits[0]);
                                uvIndex = FastIntParse(slashSplits[1]);
                            }
                        }
                        else
                        {
                            //just vertex index
                            vertexIndex = FastIntParse(faceLoop);
                        }

                        //"postprocess" indices
                        if (vertexIndex > int.MinValue)
                        {
                            if (vertexIndex < 0)
                                vertexIndex = Vertices.Count - vertexIndex;
                            vertexIndex--;
                        }
                        if (normalIndex > int.MinValue)
                        {
                            if (normalIndex < 0)
                                normalIndex = Normals.Count - normalIndex;
                            normalIndex--;
                        }
                        if (uvIndex > int.MinValue)
                        {
                            if (uvIndex < 0)
                                uvIndex = UVs.Count - uvIndex;
                            uvIndex--;
                        }

                        //set array values
                        vertexIndices.Add(vertexIndex);
                        normalIndices.Add(normalIndex);
                        uvIndices.Add(uvIndex);
                    }

                    //push to builder
                    currentBuilder.PushFace(currentMaterial, vertexIndices, normalIndices, uvIndices);

                    //clear lists
                    vertexIndices.Clear();
                    normalIndices.Clear();
                    uvIndices.Clear();
                }

            }

            Mesh mesh = new Mesh();
            List<Mesh> meshes = new List<Mesh>();
            foreach (var builder in builderDict)
            {
                if (builder.Value.PushedFaceCount == 0)
                    continue;

                meshes.Add(builder.Value.Build());
            }
            CombineInstance[] combine = new CombineInstance[meshes.Count];
            for (int i = 0; i < meshes.Count; i++)
            {
                combine[i].mesh = meshes[i];
                combine[i].transform = Matrix4x4.identity;
            }
            mesh.CombineMeshes(combine);
            return mesh;
        }

        #region Static

        /// <summary>
        /// Modified from https://codereview.stackexchange.com/a/76891. Faster than float.Parse
        /// </summary>
        public static float FastFloatParse(string input)
        {
            if (input.Contains("e") || input.Contains("E"))
                return float.Parse(input, CultureInfo.InvariantCulture);

            float result = 0;
            int pos = 0;
            int len = input.Length;

            if (len == 0) return float.NaN;
            char c = input[0];
            float sign = 1;
            if (c == '-')
            {
                sign = -1;
                ++pos;
                if (pos >= len) return float.NaN;
            }

            while (true) // breaks inside on pos >= len or non-digit character
            {
                if (pos >= len) return sign * result;
                c = input[pos++];
                if (c < '0' || c > '9') break;
                result = result * 10.0f + (c - '0');
            }

            if (c != '.' && c != ',') return float.NaN;
            float exp = 0.1f;
            while (pos < len)
            {
                c = input[pos++];
                if (c < '0' || c > '9') return float.NaN;
                result += (c - '0') * exp;
                exp *= 0.1f;
            }
            return sign * result;
        }

        /// <summary>
        /// Modified from http://cc.davelozinski.com/c-sharp/fastest-way-to-convert-a-string-to-an-int. Faster than int.Parse
        /// </summary>
        public static int FastIntParse(string input)
        {
            int result = 0;
            bool isNegative = input[0] == '-';

            for (int i = isNegative ? 1 : 0; i < input.Length; i++)
                result = result * 10 + (input[i] - '0');
            return isNegative ? -result : result;
        }

        public static Vector3 VectorFromStrArray(string[] cmps)
        {
            float x = FastFloatParse(cmps[1]);
            float y = FastFloatParse(cmps[2]);
            if (cmps.Length == 4)
            {
                float z = FastFloatParse(cmps[3]);
                return new Vector3(x, y, z);
            }
            return new Vector2(x, y);
        }
        #endregion
    }
}

