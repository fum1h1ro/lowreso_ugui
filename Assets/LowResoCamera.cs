using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[ExecuteInEditMode]
public class LowResoCamera : MonoBehaviour {
  //const HideFlags _hideFlags = HideFlags.HideAndDontSave;
  const HideFlags _hideFlags = HideFlags.DontSave;
  Transform _transform;
  Camera _subCamera, _mainCamera;
  RenderTexture _buffer;
  //Canvas _canvas;
  //RawImage _rawImage;
  Material _material;
  Vector2 _outputSize = Vector2.zero;
  int _oldWidth, _oldHeight;
  bool _oldDepthBuffering;
  public int _width = 320;
  public int _height = 568;
  public bool _depthBuffering = true;
  public int _sortOrder = 0;


  public float width {
    get { return _width; }
  }
  public float height {
    get { return _height; }
  }




  void Awake() {
    _transform = transform;
    create_camera();
    create_material();
  }
  void OnEnable() {
    alloc_buffer();
  }
  void OnDisable() {
    free_buffer();
  }
  void OnDestroy() {
    destroy_material();
    destroy_camera();
  }

  void Start() {
    _mainCamera = GetComponent<Camera>();

  }
  public void OnClick() {
    Debug.Log("HOGE");
  }

  void Update() {
    update_buffer();
  }
  void OnPreCull() {
    _subCamera.CopyFrom(_mainCamera);
    _subCamera.SetTargetBuffers(_buffer.colorBuffer, _buffer.depthBuffer);
    _subCamera.aspect = (float)_width / (float)_height;
    _subCamera.clearFlags = CameraClearFlags.SolidColor;
    _subCamera.backgroundColor = Color.clear;
    _subCamera.Render();
    _mainCamera.cullingMask = 0;
  }
  void OnPostRender() {
    _mainCamera.cullingMask = _subCamera.cullingMask;
    _material.SetPass(0);
    _material.mainTexture = _buffer;
    GL.PushMatrix();
    float shw = Screen.width * 0.5f;
    float shh = Screen.height * 0.5f;
    //GL.LoadOrtho();
    GL.LoadPixelMatrix(-shw, shw, -shh, shh);
    GL.Begin(GL.QUADS);
    GL.Color(new Color(1, 1, 1, 1));
    float hw = _outputSize.x * 0.5f;
    float hh = _outputSize.y * 0.5f;
    GL.TexCoord2(0, 0);
    GL.Vertex3(-hw, -hh, 0);
    GL.TexCoord2(1, 0);
    GL.Vertex3(hw, -hh, 0);
    GL.TexCoord2(1, 1);
    GL.Vertex3(hw, hh, 0);
    GL.TexCoord2(0, 1);
    GL.Vertex3(-hw, hh, 0);
    GL.End();
    GL.PopMatrix();
  }
  void create_material() {
    if (_material == null) {
      _material = new Material(Shader.Find("Sprites/Default"));
    }
  }
  void destroy_material() {
    if (_material != null) {
#if UNITY_EDITOR
      Material.DestroyImmediate(_material);
#else
      Material.Destroy(_material);
#endif
    }
  }
  void calc_size() {
    const int kLogBase = 2;
    float w = Screen.width;
    float h = Screen.height;
    float logWidth = Mathf.Log(w / (float)_width, kLogBase);
    float logHeight = Mathf.Log(h / (float)_height, kLogBase);
    float logWeightedAverage = Mathf.Lerp(logWidth, logHeight, 1.0f/*m_MatchWidthOrHeight*/);
    float scaleFactor = Mathf.Pow(kLogBase, logWeightedAverage);
    _outputSize.x = _width * scaleFactor;
    _outputSize.y = _height * scaleFactor;
  }
  void alloc_buffer() {
    int w = Mathf.Max(16, _width);
    int h = Mathf.Max(16, _height);
    _buffer = RenderTexture.GetTemporary(w, h, _depthBuffering ? 24 : 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 1);
    _buffer.filterMode = FilterMode.Point;
    _oldWidth = _width;
    _oldHeight = _height;
    _oldDepthBuffering = _depthBuffering;
    calc_size();
  }
  void update_buffer() {
    if (_oldWidth != _width || _oldHeight != _height || _oldDepthBuffering != _depthBuffering) {
      free_buffer();
      alloc_buffer();
    }
  }
  void free_buffer() {
    RenderTexture.ReleaseTemporary(_buffer);
    _buffer = null;
  }
  void create_camera() {
    GameObject obj = new GameObject(gameObject.name + "_sub");
    obj.hideFlags = _hideFlags;
    obj.transform.SetParent(_transform, false);
    Camera cam = obj.AddComponent<Camera>();
    cam.enabled = false;
    _subCamera = cam;
  }
  void destroy_camera() {
#if UNITY_EDITOR
    GameObject.DestroyImmediate(_subCamera.gameObject);
#else
    GameObject.Destroy(_subCamera.gameObject);
#endif
  }
  void OnGUI() {
    //GUILayout.Button("HOGE");
    //GUI.DrawTexture(new Rect(0, 0, Screen.width / 4, Screen.height / 4), _buffer);
  }
}
