using System;
using UnityEngine;

public class RoboticArm : MonoBehaviour
{
    public Transform[] joints; // 4 juntas

    public float[] currentAngles;
    public float[] targetAngles;

    public float speed = 2f;
    public float acceptance= 0.01f;
    private bool completedTarget = false;

    public event Action OnTargetAchived;
    void Start()
    {
        for (int i = 0; i < joints.Length; i++)
        {
            // pega ângulo inicial dependendo do eixo
            if (i == 0)
                currentAngles[i] = joints[i].localEulerAngles.y;
            else
                currentAngles[i] = joints[i].localEulerAngles.z;
        }
    }

    void Update()
    {
        bool reachedGoal = true;
        for (int i = 0; i < joints.Length; i++)
        {
            currentAngles[i] = Mathf.LerpAngle(
                currentAngles[i],
                targetAngles[i],
                Time.deltaTime * speed
            );

            reachedGoal &= Mathf.Abs( currentAngles[i]  - targetAngles[i]) < acceptance;

            ApplyRotation(i);
        }

        if(reachedGoal && !completedTarget)
        {
            completedTarget = true;

            OnTargetAchived?.Invoke();
        }
    }

    void ApplyRotation(int i)
    {
        if (i == 0)
        {
            // rotação no eixo Y
            joints[i].localRotation = Quaternion.Euler(0, currentAngles[i], 0);
        }
        else
        {
            // rotação no eixo Z
            joints[i].localRotation = Quaternion.Euler(0, 0, currentAngles[i]);
        }
    }

    public void SetTargetAngles(float[] newAngles)
    {
        completedTarget = false;
        for (int i = 0; i < targetAngles.Length; i++)
        {
            targetAngles[i] = newAngles[i];
        }
    }
}
