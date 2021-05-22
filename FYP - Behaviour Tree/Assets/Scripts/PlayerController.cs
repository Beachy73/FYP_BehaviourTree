using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public CharacterController controller;

    private HealthManager healthManager;

    private float x;
    private float z;
    private Vector3 move;
    private Vector3 velocity;
    private bool isGrounded;

    public float speed = 12f;
    public float gravity = -9.81f;
    public float jumpHeight = 3f;

    //public Transform groundCheck;
    //public float groundDistance = 0.4f;
    //public LayerMask groundMask;

    public Camera camera;
    public float zoomedFoV = 40f;
    public float normalFoV = 60f;
    public float cameraLerpRate = 10f;

    private float mouseX;
    private float mouseY;
    private float xRotation = 0f;

    public float mouseSensitivity = 400f;

    public Transform playerBody;

    public HealthBar healthBar;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        healthManager = GetComponent<HealthManager>();
        healthBar.SetHealth(healthManager.maxHealth);
    }

    // Update is called once per frame
    void Update()
    {
        if (healthManager.GetCurrentHealth() <= 0)
        {
            camera.transform.parent = null;
            SceneManager.LoadScene("GameOverScene");
        }
        
        //isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        
        x = Input.GetAxis("Horizontal");
        z = Input.GetAxis("Vertical");

        move = transform.right * x + transform.forward * z;

        controller.Move(move * speed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);

        if (Input.GetMouseButton(1))
        {
            camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, zoomedFoV, cameraLerpRate * Time.deltaTime);
        }
        else
        {
            camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, normalFoV, cameraLerpRate * Time.deltaTime);
        }

        healthBar.SetHealth(healthManager.GetCurrentHealth());
    }
}
