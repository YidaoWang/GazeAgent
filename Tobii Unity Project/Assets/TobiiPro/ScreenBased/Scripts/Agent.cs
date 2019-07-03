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

        GameObject gazePlotter;

        bool remoteFlag;

        Rect RemoteRect;
        List<Vector2> RemoteFaceLandmark;
        Vector2 RemoteGazePoint;


        public delegate void UpdateAgentEventHandler(Rect rect, List<Vector2> landmarkPoints, Vector2 gazePoint);
        public event UpdateAgentEventHandler OnUpdateAgent;


        public Agent(bool remoteFlag)
        {
            this.remoteFlag = remoteFlag;

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

            gazePlotter = GameObject.Find("[GazePlot]");

        }
        

        public void DrawAgent(Texture2D texture,Color32[] colors)
        {
            faceLandmarkDetector.SetImage<Color32>(colors, texture.width, texture.height, 4, true);

            //detect face rects
            List<Rect> detectResult = faceLandmarkDetector.Detect();

            if (detectResult.Count > 0)
            {
                var rect = detectResult[0];
                var w = (int)rect.width;
                var h = (int)rect.height;

                //detect landmark points
                var vectors = faceLandmarkDetector.DetectLandmark(rect);

                //faceLandmarkDetector.DrawDetectLandmarkResult()

                //draw landmark points
                //faceLandmarkDetector.DrawDetectLandmarkResult<Color32>(res, texture.width, texture.height, 4, true, 0, 255, 0, 255);

                //draw face rect
                //faceLandmarkDetector.DrawDetectResult<Color32> (res, texture.width, texture.height, 4, true, 255, 0, 0, 255, 2);

                //adjust face landmark
                //var adjusted = AdjustRect(res, texture.width, texture.height, rect);

                //OnUpdateAgent?.Invoke();

                //var gazeData = eyeTracker.LatestGazeData;
                //Vector3 transform;
                //if (gazeData.CombinedGazeRayScreenValid && gazeData.TimeStamp > (lastGazePoint.TimeStamp + float.Epsilon))
                //{
                //    lastGazePoint = gazeData;
                //    transform = GazePlotter.ProjectToPlaneInWorld(gazeData);
                //}
                //else
                //{
                //    transform = GazePlotter.ProjectToPlaneInWorld(lastGazePoint);
                //}

                Vector2 gazePos = gazePlotter.transform.localPosition;
                Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, gazePos)
                    - (new Vector2(Screen.width, Screen.height) - new Vector2(texture.width, texture.height)) / 2;

                Color32[] res;
                if (remoteFlag)
                {
                    OnUpdateAgent?.Invoke(rect, vectors, screenPos);
                    res = new Color32[texture.width * texture.height];
                    DrawAgent(res, texture.width, texture.height, RemoteRect, RemoteFaceLandmark, RemoteGazePoint);
                }
                else
                {
                    res = new Color32[colors.Length];
                    DrawAgent(res, texture.width, texture.height, rect, vectors, screenPos);
                }
                texture.SetPixels32(res);
                texture.Apply(false);
            }
        }

        public void DrawAgent(Color32[] colors, int width, int height, Rect rect, List<Vector2> landmarkPoints, Vector2 gazePoint)
        {
            if (landmarkPoints.Count < 67) return;
            //int thickness = 3;
            int pupil = 10;
            Vector2 flipedGazePoint = new Vector2(gazePoint.x, height - gazePoint.y);
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

            DrawCircle(colors, width, height, lcenter, lr + 5, color, true);


            var ldel = (flipedGazePoint - lcenter) * (20.0f / 320);
            //ldel = new Vector2(Math.Min(ldel.x, 20), Math.Min(ldel.y, 20));
            DrawCircle(colors, width, height, lcenter + ldel, pupil, color, true, true);

            // 右目
            var rr = (landmarkPoints[42] - landmarkPoints[45]).magnitude / 2;
            var rcenter = landmarkPoints[42] + (landmarkPoints[45] - landmarkPoints[42]) / 2;

            DrawCircle(colors, width, height, rcenter, rr + 5, color, true);

            var rdel = (flipedGazePoint - rcenter) * (20.0f / 320);
            //rdel = new Vector2(Math.Min(rdel.x, 20), Math.Min(rdel.y, 20));
            DrawCircle(colors, width, height, rcenter + rdel, pupil, color, true, true);


            // 輪郭
            for (var i = 0; i < 16; i++)
            {
                DrawLine(colors, width, height, landmarkPoints[i], landmarkPoints[i + 1], color);
            }
            // 眉
            //for (var i = 17; i < 26; i++)
            //{
            //    DrawLine(colors, width, height, landmarkPoints[i], landmarkPoints[i + 1], color);
            //}
            // 鼻
            for (var i = 27; i < 35; i++)
            {
                DrawLine(colors, width, height, landmarkPoints[i], landmarkPoints[i + 1], color);
            }
            DrawLine(colors, width, height, landmarkPoints[35], landmarkPoints[30], color);

            // 口外形
            for (var i = 48; i < 59; i++)
            {
                DrawLine(colors, width, height, landmarkPoints[i], landmarkPoints[i + 1], color);
            }
            DrawLine(colors, width, height, landmarkPoints[59], landmarkPoints[48], color);

            // 口内形
            for (var i = 60; i < 67; i++)
            {
                DrawLine(colors, width, height, landmarkPoints[i], landmarkPoints[i + 1], color);
            }
            DrawLine(colors, width, height, landmarkPoints[67], landmarkPoints[60], color);

            //AdjustRect(colors, width, height, rect);
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
