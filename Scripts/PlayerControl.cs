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
            // ��ũ���� ���콺 ��ġ�� ���� Ray�� �����Ѵ�.
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            // �浹 �� ��� �浹 ������ ������ ����
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
