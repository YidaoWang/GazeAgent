using DlibFaceLandmarkDetector;
using DlibFaceLandmarkDetector.UnityUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DlibFaceLandmarkDetectorExample
{
    public class Agent
    {

        /// <summary>
        /// The dlib shape predictor file name.
        /// </summary>
        string dlibShapePredictorFileName = "sp_human_face_68.dat";

        /// <summary>
        /// The dlib shape predictor file path.
        /// </summary>
        string dlibShapePredictorFilePath;

        /// <summary>
        /// The face landmark detector.
        /// </summary>
        FaceLandmarkDetector faceLandmarkDetector;


        public int Width { get; }
        public int Height { get;}

        public Agent(int width, int height)
        {
            Width = width;
            Height = height;

            dlibShapePredictorFileName = DlibFaceLandmarkDetectorExample.dlibShapePredictorFileName;
#if UNITY_WEBGL && !UNITY_EDITOR
            getFilePath_Coroutine = Utils.getFilePathAsync (dlibShapePredictorFileName, (result) => {
                getFilePath_Coroutine = null;

                dlibShapePredictorFilePath = result;
                Run ();
            });
            StartCoroutine (getFilePath_Coroutine);
#else
            dlibShapePredictorFilePath = Utils.getFilePath(dlibShapePredictorFileName);
#endif
            if (string.IsNullOrEmpty(dlibShapePredictorFilePath))
            {
                Debug.LogError("shape predictor file does not exist. Please copy from “DlibFaceLandmarkDetector/StreamingAssets/” to “Assets/StreamingAssets/” folder. ");
            }

            faceLandmarkDetector = new FaceLandmarkDetector(dlibShapePredictorFilePath);

        }


        public Vector2[] GetLandmarkPoints(Color32[] colors)
        {
            faceLandmarkDetector.SetImage<Color32>(colors, Width, Height, 4, true);

            //detect face rects
            List<Rect> detectResult = faceLandmarkDetector.Detect();

            if (detectResult.Count > 0)
            {
                var rect = detectResult[0];
                var w = (int)rect.width;
                var h = (int)rect.height;

                //detect landmark points
                return faceLandmarkDetector.DetectLandmark(rect).ToArray();
            }

            return null;
        }

        

        public void DrawAgent(Texture2D texture, Vector2[] landmarkPoints, Vector2 gazePoint)
        {
            if (landmarkPoints == null || landmarkPoints.Length < 67) return;

            var colors = new Color32[Width * Height];
            //int thickness = 3;
            int pupil = 10;
            Vector2 flipedGazePoint = new Vector2(gazePoint.x, Height - gazePoint.y);
            Color32 color = new Color32(0, 0, 0, 255);

            // ランドマークを中心へ移動
            //AdjustVectors(landmarkPoints, width, height, rect);

            // ランドマーク点描
            //for (int i = 0; i < landmarkPoints.Count; i++)
            //{
            //    var pixels = GetPixels(landmarkPoints[i], thickness);
            //    foreach (var p in pixels)
            //    {
            //        var sn = PointToSequence(p, width, height, true);
            //        if (0 < sn && sn < colors.Length)
            //        {
            //            colors[sn] = color;
            //        }
            //    }
            //}
            //DrawCircle(colors, width, height, flipedGazePoint, 5, color, true);

            // 左目
            var lr = (landmarkPoints[39] - landmarkPoints[36]).magnitude / 2;
            var lcenter = landmarkPoints[36] + (landmarkPoints[39] - landmarkPoints[36]) / 2;

            DrawCircle(colors, Width, Height, lcenter, lr + 5, color, true);


            var ldel = (flipedGazePoint - lcenter) * (20.0f / 320);
            //ldel = new Vector2(Math.Min(ldel.x, 20), Math.Min(ldel.y, 20));
            DrawCircle(colors, Width, Height, lcenter + ldel, pupil, color, true, true);

            // 右目
            var rr = (landmarkPoints[42] - landmarkPoints[45]).magnitude / 2;
            var rcenter = landmarkPoints[42] + (landmarkPoints[45] - landmarkPoints[42]) / 2;

            DrawCircle(colors, Width, Height, rcenter, rr + 5, color, true);

            var rdel = (flipedGazePoint - rcenter) * (20.0f / 320);
            //rdel = new Vector2(Math.Min(rdel.x, 20), Math.Min(rdel.y, 20));
            DrawCircle(colors, Width, Height, rcenter + rdel, pupil, color, true, true);


            // 輪郭
            for (var i = 0; i < 16; i++)
            {
                DrawLine(colors, Width, Height, landmarkPoints[i], landmarkPoints[i + 1], color);
            }
            // 眉
            //for (var i = 17; i < 26; i++)
            //{
            //    DrawLine(colors, width, height, landmarkPoints[i], landmarkPoints[i + 1], color);
            //}
            // 鼻
            for (var i = 27; i < 35; i++)
            {
                DrawLine(colors, Width, Height, landmarkPoints[i], landmarkPoints[i + 1], color);
            }
            DrawLine(colors, Width, Height, landmarkPoints[35], landmarkPoints[30], color);

            // 口外形
            for (var i = 48; i < 59; i++)
            {
                DrawLine(colors, Width, Height, landmarkPoints[i], landmarkPoints[i + 1], color);
            }
            DrawLine(colors, Width, Height, landmarkPoints[59], landmarkPoints[48], color);

            // 口内形
            for (var i = 60; i < 67; i++)
            {
                DrawLine(colors, Width, Height, landmarkPoints[i], landmarkPoints[i + 1], color);
            }
            DrawLine(colors, Width, Height, landmarkPoints[67], landmarkPoints[60], color);

            texture.SetPixels32(colors);
            texture.Apply(false);
        }

        private int PointToSequence(Vector2 point, int width, int heigth, bool flip)
        {
            if (!flip)
            {
                return (int)point.x + width * (int)point.y;
            }
            else
            {
                return (int)point.x + width * (heigth - (int)point.y);
            }
        }

        private void DrawLine(Color32[] coloes, int width, int heigth, Vector2 start, Vector2 end, Color32 color)
        {
            var vec = end - start;

            var dx = Math.Max(Math.Abs(vec.x), Math.Abs(vec.y));
            var dy = Math.Min(Math.Abs(vec.x), Math.Abs(vec.y));

            for (var i = 0; i < dx; i++)
            {
                int s = PointToSequence(start + (vec * i / dx), width, heigth, true);
                if (0 <= s && s < coloes.Length)
                {
                    coloes[s] = color;
                }
            }
        }

        private void DrawCircle(Color32[] colors, int width, int heigth, Vector2 center, float radius, Color32 color, bool flip, bool fill = false)
        {
            if (fill)
            {
                if (flip)
                {
                    center = new Vector2(center.x, heigth - center.y);
                }
                for (int i = PointToSequence(center - new Vector2(0, radius), width, heigth, false); i <= PointToSequence(center + new Vector2(0, radius), width, heigth, false); i++)
                {
                    float x = i % width - center.x;
                    float y = i / width - center.y;
                    if (x * x + y * y <= radius * radius)
                    {
                        colors[i] = color;
                    }
                }
            }
            else
            {
                for (var i = 0; i < 2 * Math.PI * radius; i++)
                {
                    int s = PointToSequence(center + new Vector2((float)Math.Sin(i / radius), (float)Math.Cos(i / radius)) * radius, width, heigth, flip);
                    if (0 <= s && s < colors.Length)
                    {
                        colors[s] = color;
                    }
                }
            }
        }

        private void AdjustRect(Color32[] colors, int width, int height, Rect rect)
        {
            Color32[] res = new Color32[colors.Length];

            int adjx = -(int)rect.xMin + width / 2 - (int)rect.width / 2;
            int adjy = -(int)rect.yMin + height / 2 - (int)rect.height / 2;

            for (var i = 0; i < colors.Length; i++)
            {
                int j = i + adjx - adjy * width;
                if (j >= 0 && j < res.Length)
                {
                    res[j] = colors[i];
                }
            }
            for (var i = 0; i < colors.Length; i++)
            {
                colors[i] = res[i];
            }
        }

        /// <summary>
        /// Raises the destroy event.
        /// </summary>
        void OnDestroy()
        {

            if (faceLandmarkDetector != null)
                faceLandmarkDetector.Dispose();

#if UNITY_WEBGL && !UNITY_EDITOR
            if (getFilePath_Coroutine != null) {
                StopCoroutine (getFilePath_Coroutine);
                ((IDisposable)getFilePath_Coroutine).Dispose ();
            }
#endif
        }

    }
}
