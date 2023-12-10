using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _mouseSensitivity;
    [SerializeField] private float _movementSpeedMultiplier;
    [SerializeField] private float _sprintSpeedAdd = 1.5f;
    [SerializeField] private float _maxStamina = 100.0f;
    [SerializeField] private float _staminaRegenRate = 0.5f;
    [SerializeField] private float _staminaUseRate = 1.0f;
    [SerializeField] private float _throwStrength = 1.0f;
    [SerializeField] private float _boxThrowCooldown = 3.0f;
    [SerializeField] private float _footstepTimerMax = 0.4f;
    [SerializeField] private float _footstepTimerMaxSprint = 0.25f;

    [Header("References")]
    [SerializeField] CharacterController _characterController;
    [SerializeField] Camera _camera;
    [SerializeField] Slider _staminaSlider;
    [SerializeField] GameObject _captureBoxPrefab;
    [SerializeField] Transform _boxSpawnPoint;
    [SerializeField] Image _boxCooldownImage;
    [SerializeField] TMPro.TextMeshProUGUI _counter;
    [SerializeField] AudioSource _leftFootstep;
    [SerializeField] AudioSource _rightFootstep;

    private float _verticalRotation = 0.0f;

    private float _currentStamina = 100.0f;

    private bool _isHiding = false;

    private float _boxThrowCurrentCooldown = 0.0f;

    private Vector3 _lastPos;

    private float _footstepTimerCurrent;
    private bool _footstepFlipFlop = false;

    private bool Sprinting => Input.GetKey(KeyCode.LeftShift);
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        _currentStamina = _maxStamina;

        GameManager.instance.OnCaptured.AddListener((x) =>
        {
            _counter.text = $"{x}";
        });
    }

    private void Update()
    {
        UpdateBoxThrowCooldown();
        UpdateStaminaBar();
        TryThrowBox();
        HandleRotation();
    }

    private void UpdateBoxThrowCooldown()
    {
        _boxThrowCurrentCooldown -= Time.deltaTime;
        _boxCooldownImage.fillAmount = 1 - _boxThrowCurrentCooldown / _boxThrowCooldown;
    }

    private void UpdateStaminaBar()
    {
        _staminaSlider.value = _currentStamina / _maxStamina;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        HandleMovement();
    }

    private void PlayFootstepSounds()
    {
        _footstepTimerCurrent -= Time.fixedDeltaTime;
        if (_footstepTimerCurrent <= 0)
        {
            _footstepTimerCurrent = Sprinting ? _footstepTimerMaxSprint : _footstepTimerMax;
            if (_footstepFlipFlop)
                _leftFootstep.Play();
            else
                _rightFootstep.Play();
            _footstepFlipFlop = !_footstepFlipFlop;
        }
    }

    private void TryThrowBox()
    {
        if (!Input.GetKeyDown(KeyCode.E) || _boxThrowCurrentCooldown > 0.0f)
        {
            return;
        }
        _boxThrowCurrentCooldown = _boxThrowCooldown;
        var box = Instantiate(_captureBoxPrefab);
        box.transform.position = _boxSpawnPoint.position;
        box.transform.rotation = Quaternion.FromToRotation(box.transform.forward,transform.forward);
        box.transform.rotation = box.transform.rotation*Quaternion.AngleAxis(90,Vector3.forward);
        var physics = box.GetComponent<Rigidbody>();
        physics.AddForce(_camera.transform.forward * _throwStrength);
        var captureBox = box.GetComponent<CaptureBox>();
        captureBox.playerRef = this;
    }

    public void OnHitMonster(GameObject monster)
    {
        monster.GetComponent<Monster>()?.OnCapture();
    }

    private void HandleRotation()
    {
        if (_isHiding)
            return;

        float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity * Time.deltaTime;
        float mouseY = -Input.GetAxis("Mouse Y") * _mouseSensitivity * Time.deltaTime;

        _verticalRotation += mouseY;
        _verticalRotation = Mathf.Clamp(_verticalRotation, -90.0f, 90.0f);

        _camera.transform.localRotation = Quaternion.Euler(_verticalRotation, 0, 0);
        transform.rotation *= Quaternion.Euler(0, mouseX, 0);
    }

    private void HandleMovement()
    {
        if (_isHiding)
            return;

        var xDirection = 0;
        var zDirection = 0;

        if (Input.GetKey(KeyCode.W))
            zDirection = 1;
        else if (Input.GetKey(KeyCode.S))
            zDirection = -1;
        if (Input.GetKey(KeyCode.A))
            xDirection = -1;
        else if (Input.GetKey(KeyCode.D))
            xDirection = 1;

        var forward = transform.forward * zDirection;
        var right = _camera.transform.right * xDirection;

        var result = forward + right;
        result = result.normalized;

        if (result.magnitude != 0)
            PlayFootstepSounds();

        var multiplier = _movementSpeedMultiplier;
        if (result.magnitude != 0 && Input.GetKey(KeyCode.LeftShift) && _currentStamina > 0.0f)
        {
            multiplier += _sprintSpeedAdd;
            _currentStamina -= _staminaUseRate * Time.fixedDeltaTime;
        }
        else
        {
            _currentStamina += _staminaRegenRate * Time.fixedDeltaTime;
            if (_currentStamina >= _maxStamina)
            {
                _currentStamina = _maxStamina;
            }
        }

        result *= multiplier * Time.deltaTime;
        _characterController.Move(Physics.gravity * Time.deltaTime + result);
        _lastPos = transform.position;
    }
}
