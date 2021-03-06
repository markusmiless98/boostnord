using UnityEngine;

public class CameraController : MonoBehaviour {
    [SerializeField] private float movementTime = 10, zoomAmount = 2, maxZoom = 10, minZoom = 5;
    private Camera _camera;
    private Vector3 _newPos, _newZoom, _dragStartPosition, _dragCurrentPosition, defaultNewPos, defaultNewZoom;

    public GameManager gm;


    private void Awake() {
        _camera = Camera.main;
        _newPos = transform.position;
        _newZoom = _camera.transform.localPosition;

        defaultNewPos = _newPos;
        defaultNewZoom = _newZoom;
    }

    public void ResetValues() {

        transform.position = defaultNewPos;
        _camera.transform.localPosition = defaultNewZoom;

        _newPos = transform.position;
        _newZoom = _camera.transform.localPosition;
    }

    private void Update() {
        if (!gm.paused) HandleMouseInput();
    }

    private void HandleMouseInput() {
        if (Input.mouseScrollDelta.y != 0) {
            _newZoom += Input.mouseScrollDelta.y * _camera.transform.forward * zoomAmount;
        }

        if (Input.GetMouseButtonDown(1)) {
            var plane = new Plane(Vector3.up, Vector3.zero);
            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (plane.Raycast(ray, out var entry)) {
                _dragStartPosition = ray.GetPoint(entry);
            }
        }
        if (Input.GetMouseButton(1)) {
            var plane = new Plane(Vector3.up, Vector3.zero);
            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (plane.Raycast(ray, out var entry)) {
                _dragCurrentPosition = ray.GetPoint(entry);

                _newPos = transform.position + _dragStartPosition - _dragCurrentPosition;
            }
        }

        _newZoom.y = Mathf.Clamp(_newZoom.y, minZoom, maxZoom);
        _camera.transform.localPosition =
            Vector3.Lerp(_camera.transform.localPosition, _newZoom, Time.deltaTime * movementTime);
        transform.position =
            Vector3.Lerp(transform.position, _newPos, Time.deltaTime * movementTime);
    }
}