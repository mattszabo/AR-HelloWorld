namespace GoogleARCore.HelloAR {
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Rendering;
    using GoogleARCore;

    public class HelloARController : MonoBehaviour {
        public Camera m_firstPersonCamera;
        public LayerMask touchInputMask;
        public GameObject m_trackedPlanePrefab;
        public GameObject m_cubeAndroidPrefab;
        public GameObject m_searchingForPlaneUI;

        private List<GameObject> m_allCubes = new List<GameObject>();
        private List<TrackedPlane> m_newPlanes = new List<TrackedPlane>();
        private List<TrackedPlane> m_allPlanes = new List<TrackedPlane>();

        private Color[] m_planeColors = new Color[] {
            new Color(1.0f, 1.0f, 1.0f),
            new Color(0.611f, 0.152f, 0.654f),
            new Color(0.403f, 0.227f, 0.717f),
            new Color(0.247f, 0.317f, 0.709f),
            new Color(0.129f, 0.588f, 0.952f),
            new Color(0.011f, 0.662f, 0.956f),
            new Color(0f, 0.737f, 0.831f),
            new Color(0f, 0.588f, 0.533f),
            new Color(0.298f, 0.686f, 0.313f),
            new Color(0.545f, 0.764f, 0.290f),
            new Color(0.803f, 0.862f, 0.223f),
            new Color(1.0f, 0.921f, 0.231f),
            new Color(1.0f, 0.756f, 0.027f)
        };

        public void Update() {
            _QuitOnConnectionErrors();

            // The tracking state must be FrameTrackingState.Tracking in order to access the Frame.
            if (Frame.TrackingState != FrameTrackingState.Tracking) {
                const int LOST_TRACKING_SLEEP_TIMEOUT = 15;
                Screen.sleepTimeout = LOST_TRACKING_SLEEP_TIMEOUT;
                return;
            }

            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Frame.GetNewPlanes(ref m_newPlanes);

            for (int i = 0; i < m_newPlanes.Count; i++) {
                GameObject planeObject = Instantiate(m_trackedPlanePrefab, Vector3.zero, Quaternion.identity,
                    transform);
                planeObject.GetComponent<TrackedPlaneVisualizer>().SetTrackedPlane(m_newPlanes[i]);

                planeObject.GetComponent<Renderer>().material.SetColor("_GridColor", m_planeColors[Random.Range(0,
                    m_planeColors.Length - 1)]);
                planeObject.GetComponent<Renderer>().material.SetFloat("_UvRotation", Random.Range(0.0f, 360.0f));
            }

            bool showSearchingUI = true;
            Frame.GetAllPlanes(ref m_allPlanes);
            for (int i = 0; i < m_allPlanes.Count; i++) {
                if (m_allPlanes[i].IsValid) {
                    showSearchingUI = false;
                    break;
                }
            }

            m_searchingForPlaneUI.SetActive(showSearchingUI);

            Touch touch;

            if (Input.touchCount > 0) {
                touch = Input.GetTouch(0);

                TrackableHit hit;
                TrackableHitFlag raycastFilter = TrackableHitFlag.PlaneWithinBounds | TrackableHitFlag.PlaneWithinPolygon;

                if (Session.Raycast(m_firstPersonCamera.ScreenPointToRay(touch.position), raycastFilter, out hit)) {
                    bool createNewCube = true;

                    for (int i = 0; i < m_allCubes.Count; i++) {
                        GameObject cube = m_allCubes[i].transform.GetChild(0).gameObject;
                        Renderer rend = cube.GetComponent<Renderer>();

                        if (rend != null && rend.bounds.Contains(hit.Point)) {
                            createNewCube = false;
                            
                            if (touch.phase == TouchPhase.Began) {
                                cube.SendMessage("OnTouchDown", SendMessageOptions.DontRequireReceiver);
                            } else if (touch.phase == TouchPhase.Stationary) {
                                cube.SendMessage("OnTouchStay", SendMessageOptions.DontRequireReceiver);
                            } else if (touch.phase == TouchPhase.Moved) {
                                cube.SendMessage("OnTouchMoved", hit.Point, SendMessageOptions.DontRequireReceiver);
                            } else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) {
                                cube.SendMessage("OnTouchUp", SendMessageOptions.DontRequireReceiver);
                            }

                            break;
                        }
                    }

                    if (createNewCube && touch.phase == TouchPhase.Began) {
                        GameObject newCube = createAnchoredCube(hit);
                        // Add to the list of Cube's
                        m_allCubes.Add(newCube);
                    }
                }
            }
        }

        private GameObject createAnchoredCube(TrackableHit hit) {
            var anchor = Session.CreateAnchor(hit.Point, Quaternion.identity);
            var cube = Instantiate(m_cubeAndroidPrefab, hit.Point, Quaternion.identity,
                anchor.transform);

            cube.transform.LookAt(m_firstPersonCamera.transform);
            cube.transform.rotation = Quaternion.Euler(0.0f,
                cube.transform.rotation.eulerAngles.y, cube.transform.rotation.z);

            // Use a plane attachment component to maintain Cube's y-offset from the plane
            cube.GetComponent<PlaneAttachment>().Attach(hit.Plane);

            return cube;
        }

        private void _QuitOnConnectionErrors() {
            if (Session.ConnectionState == SessionConnectionState.DeviceNotSupported) {
                _ShowAndroidToastMessage("This device does not support ARCore.");
                Application.Quit();
            } else if (Session.ConnectionState == SessionConnectionState.UserRejectedNeededPermission) {
                _ShowAndroidToastMessage("Camera permission is needed to run this application.");
                Application.Quit();
            } else if (Session.ConnectionState == SessionConnectionState.ConnectToServiceFailed) {
                _ShowAndroidToastMessage("ARCore encountered a problem connecting.  Please start the app again.");
                Application.Quit();
            }
        }

        private static void _ShowAndroidToastMessage(string message) {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            if (unityActivity != null) {
                AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
                unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                    AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity,
                        message, 0);
                    toastObject.Call("show");
                }));
            }
        }
    }
}
