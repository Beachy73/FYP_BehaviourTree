using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BounceEffect : MonoBehaviour
{
    public float rotateSpeed = 50f;
    private float turnSpeed;

    [SerializeField]
    public float bounceSpeed = 2f;
    [SerializeField]
    public float bounceHeight = 0.3f;
    private Vector3 pos;

    private void Awake()
    {
        pos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // Rotation
        turnSpeed = rotateSpeed * Time.deltaTime;
        transform.Rotate(0f, turnSpeed, 0f, Space.World);

        // Up and Down Movement
        float newY = Mathf.Sin(Time.time * bounceSpeed) * bounceHeight + pos.y;
        transform.position = new Vector3(pos.x, newY, pos.z);
    }
}
