using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    [SerializeField] private float _speed = 10f;
    [SerializeField] private Vector3 _offset;
    
    private Vector3 _target;

    private void Awake()
    {
        Instance ??= this;
        _target = transform.position;
    }
    private void Update()
    {
        if((_target - transform.position).magnitude > .02f)
            transform.position = Vector3.MoveTowards(transform.position, _target, _speed * Time.deltaTime);
    }

    public void CameraFocus(Vector3 cellPosition)
    {
        _target = cellPosition + _offset;
    }
}
