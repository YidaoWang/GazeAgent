using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tobii.Research.Unity
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class GazePlotter : MonoBehaviour
    {
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

        [Range(3.0f, 15.0f), Tooltip("Number of gaze points in point cloud.")]
        public int PointCloudSize = 10;
        [Tooltip("Sprite to use for gaze points in the point cloud.")]
        public Sprite PointSprite;
        [Range(0.0f, 1.0f), Tooltip("Scale to draw the point sprites in the point cloud.")]
        public float PointScale = 0.1f;
        [Tooltip("Distance from screen to visualization plane in the World.")]
        public float VisualizationDistance = 10f;
        [Range(0.1f, 1.0f), Tooltip("How heavy filtering to apply to gaze point bubble movements. 0.1f is most responsive, 1.0f is least responsive.")]
        public float FilterSmoothingFactor = 0.6f;

        [SerializeField]
        [Tooltip("Turn gaze trail on or off.")]
        public bool _on = true;

        [SerializeField]
        [Tooltip("Use filter or not.")]
        private bool _useFilter = true;

        /// <summary>
        /// Turn gaze trail on or off.
        /// </summary>
        public bool On
        {
            get
            {
                return _on;
            }

            set
            {
                _on = value;
                OnSwitch();
            }
        }

        private IGazeData _lastGazePoint = new GazeData();

        private EyeTracker _eyeTracker;
        private Calibration _calibrationObject;

        // Members used for the gaze point cloud:
        private const float MaxVisibleDurationInSeconds = 0.5f;
        private IGazeData[] _gazePoints;
        private int _last;
        private bool _lastOn;
        private GameObject[] _gazePointCloudSprites;

        // Members used for gaze bubble (filtered gaze visualization):
        private SpriteRenderer _gazeBubbleRenderer;      // the gaze bubble sprite is attached to the GazePlotter game object itself

  
        private bool _hasHistoricPoint;
        private Vector3 _historicPoint;

        public bool UseFilter
        {
            get { return _useFilter; }
            set { _useFilter = value; }
        }

        void Start()
        {
            _eyeTracker = EyeTracker.Instance;
            _calibrationObject = Calibration.Instance;

            InitializeGazePointBuffer();
            InitializeGazePointCloudSprites();

            _last = PointCloudSize - 1;

            _gazeBubbleRenderer = GetComponent<SpriteRenderer>();
            UpdateGazeBubbleVisibility();
        }

        void Update()
        {
            IGazeData gazePoint = _eyeTracker.LatestGazeData;

            if (gazePoint.CombinedGazeRayScreenValid
                && gazePoint.TimeStamp > (_lastGazePoint.TimeStamp + float.Epsilon))
            {
                if (UseFilter)
                {
                    UpdateGazeBubblePosition(gazePoint);
                }
                else
                {
                    UpdateGazePointCloud(gazePoint);
                }

                _lastGazePoint = gazePoint;
            }

            UpdateGazePointCloudVisibility();
            UpdateGazeBubbleVisibility();
        }

        private void InitializeGazePointBuffer()
        {
            _gazePoints = new IGazeData[PointCloudSize];
            for (int i = 0; i < PointCloudSize; i++)
            {
                _gazePoints[i] = new GazeData();
            }
        }

        private void InitializeGazePointCloudSprites()
        {
            _gazePointCloudSprites = new GameObject[PointCloudSize];
            for (int i = 0; i < PointCloudSize; i++)
            {
                var pointCloudSprite = new GameObject("PointCloudSprite" + i);
                pointCloudSprite.layer = gameObject.layer;

                var spriteRenderer = pointCloudSprite.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = PointSprite;

                var cloudPointVisualizer = pointCloudSprite.AddComponent<CloudPointVisualizer>();
                cloudPointVisualizer.Scale = PointScale;

                pointCloudSprite.SetActive(false);
                _gazePointCloudSprites[i] = pointCloudSprite;
            }
        }

        private void UpdateGazePointCloudVisibility()
        {
            bool isPointCloudVisible = !UseFilter;

            for (int i = 0; i < PointCloudSize; i++)
            {
                if (IsNotTooOld(_gazePoints[i]))
                {
                    _gazePointCloudSprites[i].SetActive(isPointCloudVisible);
                }
                else
                {
                    _gazePointCloudSprites[i].SetActive(false);
                }
            }
        }

        private bool IsNotTooOld(IGazeData gazePoint)
        {
            return (Time.unscaledTime - gazePoint.TimeStamp) < MaxVisibleDurationInSeconds;
        }

        private void UpdateGazeBubblePosition(IGazeData gazePoint)
        {
            Vector3 gazePointInWorld = ProjectToPlaneInWorld(gazePoint);
            transform.position = Smoothify(gazePointInWorld);
        }

        private void UpdateGazePointCloud(IGazeData gazePoint)
        {
            _last = Next();
            _gazePoints[_last] = gazePoint;
            var cloudPointVisualizer = _gazePointCloudSprites[_last].GetComponent<CloudPointVisualizer>();
            Vector3 gazePointInWorld = ProjectToPlaneInWorld(gazePoint);
            Debug.Log("plotter" + gazePointInWorld);
            cloudPointVisualizer.NewPosition(gazePoint.TimeStamp, gazePointInWorld);
        }

        private void UpdateGazeBubbleVisibility()
        {
            _gazeBubbleRenderer.enabled = UseFilter;
        }

        private int Next()
        {
            return ((_last + 1) % PointCloudSize);
        }

        private Vector3 Smoothify(Vector3 point)
        {
            if (!_hasHistoricPoint)
            {
                _historicPoint = point;
                _hasHistoricPoint = true;
            }

            var smoothedPoint = new Vector3(
                point.x * (1.0f - FilterSmoothingFactor) + _historicPoint.x * FilterSmoothingFactor,
                point.y * (1.0f - FilterSmoothingFactor) + _historicPoint.y * FilterSmoothingFactor,
                point.z * (1.0f - FilterSmoothingFactor) + _historicPoint.z * FilterSmoothingFactor);

            _historicPoint = smoothedPoint;

            return smoothedPoint;
        }

        private void OnSwitch()
        {
            if (_lastOn && !_on)
            {
                // Switch off.
                RemovePlotter();
                _lastOn = false;
            }
            else if (!_lastOn && _on)
            {
                // Switch on.
                _lastOn = true;
            }
        }

        private void RemovePlotter()
        {

        }
    }
}

