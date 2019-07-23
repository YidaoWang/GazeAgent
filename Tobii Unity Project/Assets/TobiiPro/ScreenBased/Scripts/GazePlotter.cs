using Assets.TobiiPro.ScreenBased.Scripts;
using Assets.UDP;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tobii.Research.Unity
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class GazePlotter : MonoBehaviour
    {
        
        [Tooltip("Distance from screen to visualization plane in the World.")]
        public float VisualizationDistance = 10f;
        [Range(0.1f, 1.0f), Tooltip("How heavy filtering to apply to gaze point bubble movements. 0.1f is most responsive, 1.0f is least responsive.")]
        public float FilterSmoothingFactor = 0.6f;

        private IGazeData _lastGazeData = new GazeData();
        private DataExchangeSystem dataExchangeSystem;

        private EyeTracker _eyeTracker;
        private Calibration _calibrationObject;

        private float screen_z;

        // Members used for gaze bubble (filtered gaze visualization):
        private SpriteRenderer GazeBubbleRenderer;      // the gaze bubble sprite is attached to the GazePlotter game object itself

        void Start()
        {
            screen_z = GameObject.Find("Cube").transform.position.z;

            dataExchangeSystem = GameObject.Find("DataExchangeSystem").GetComponent<DataExchangeSystem>();
          
            GazeBubbleRenderer = GetComponent<SpriteRenderer>();

            var conditionSettings = GameObject.Find("ConditionSettings").GetComponent<ConditionSettings>();

            conditionSettings.OnConditionChange += (media, cursor) =>
            {
                if (cursor == CursorCondition.C)
                {
                    GazeBubbleRenderer.color = new Color(GazeBubbleRenderer.color.r, GazeBubbleRenderer.color.g, GazeBubbleRenderer.color.b, 255);
                }
                else
                {
                    GazeBubbleRenderer.color = new Color(GazeBubbleRenderer.color.r, GazeBubbleRenderer.color.g, GazeBubbleRenderer.color.b, 0);
                }
            };

            dataExchangeSystem.OnReceive += data =>
            {
                if (data.MediaCondition == MediaCondition.N)
                {
                    var d = data as GazeMediaData;
                    transform.position = new Vector3(d.GazePoint.x, d.GazePoint.y, screen_z);
                }
            };

            _eyeTracker = EyeTracker.Instance;
            _calibrationObject = Calibration.Instance;
        }

        void Update()
        {     
        }


        public void UpdateGazePlotter(float texture_width, float texture_height)
        {
            IGazeData gazeData = _eyeTracker.LatestGazeData;

            if (gazeData.CombinedGazeRayScreenValid
                && gazeData.TimeStamp > (_lastGazeData.TimeStamp + float.Epsilon))
            {
                var gazepoint = GetGazePointdata(gazeData);
                //var screenPos = ToScreenPos(gazepoint, texture_width, texture_height); 
                dataExchangeSystem.Post(new GazeMediaData(gazepoint));

                _lastGazeData = gazeData;
            }
            GazeBubbleRenderer.enabled = true;
        }

        public Vector2 ToScreenPos(Vector3 gazePos, float texture_width, float texture_height)
        {
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, gazePos)
                - (new Vector2(Screen.width, Screen.height) - new Vector2(texture_width, texture_height)) / 2;

            return screenPos;
        }

        private Vector3 GetGazePointdata(IGazeData gazePoint)
        {
            Vector3 gazePointInWorld = ProjectToPlaneInWorld(gazePoint);
            return gazePointInWorld;
            //return Smoothify(gazePointInWorld);
        }

        public static Vector3 ProjectToPlaneInWorld(IGazeData gazePoint)
        {
            Ray ray = gazePoint.CombinedGazeRayScreen;
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Vector3 gazeOnScreen = hit.point;
                return gazeOnScreen;
            }
            return Vector3.zero;
        }

        //private Vector3 Smoothify(Vector3 point)
        //{
        //    if (!_hasHistoricPoint)
        //    {
        //        _historicPoint = point;
        //        _hasHistoricPoint = true;
        //    }

        //    var smoothedPoint = new Vector3(
        //        point.x * (1.0f - FilterSmoothingFactor) + _historicPoint.x * FilterSmoothingFactor,
        //        point.y * (1.0f - FilterSmoothingFactor) + _historicPoint.y * FilterSmoothingFactor,
        //        point.z * (1.0f - FilterSmoothingFactor) + _historicPoint.z * FilterSmoothingFactor);

        //    _historicPoint = smoothedPoint;

        //    return smoothedPoint;
        //}
    }
}

