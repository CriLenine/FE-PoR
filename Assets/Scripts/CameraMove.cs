using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    [SerializeField] private float _speed = 10f;
    [SerializeField] private float _scrollSpeed = 300f;


    private void Update()
    {
        if(Input.GetMouseButton(2) && Input.GetAxis("Mouse X") != 0)
        {
            transform.position -= new Vector3(Input.GetAxisRaw("Mouse X") * Time.deltaTime * _speed,
                                                0,
                                                Input.GetAxisRaw("Mouse Y") * Time.deltaTime * _speed);
        }
        Camera.main.fieldOfView -= Input.GetAxis("Mouse ScrollWheel") * _scrollSpeed;
    }
}
