using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 员工AI
/// </summary>
public class EmpAI : MonoBehaviour
{
    Transform mesh;
    Transform agnet;

    private void Start()
    {
        mesh = transform.Find("Mesh");
        agnet = transform.Find("Agent");
    }

    private void Update()
    {
        mesh.position = agnet.transform.position + new Vector3(0, 4, 4);
    }
}
