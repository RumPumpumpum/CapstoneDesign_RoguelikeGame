using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public Vector3 playerPosition;
    public Vector3 mouseUpPosition;
    public Rigidbody rb;


    void OnMouseDown()
    {
            // 스크린의 마우스 위치로 부터 Ray를 생성한다.
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            // 충돌 할 경우 충돌 정보를 저장할 변수
            RaycastHit hitData;

            if (Physics.Raycast(ray, out hitData, Mathf.Infinity))
            {
                playerPosition = Input.mousePosition;
                Debug.Log("MouseDown");
            }

    }

    void OnMouseUp()
    {
        mouseUpPosition = Input.mousePosition;

        rb = GetComponent<Rigidbody>();
        rb.AddForce((playerPosition - mouseUpPosition) * 10);

        Debug.Log("MouseDown");

    }
}
