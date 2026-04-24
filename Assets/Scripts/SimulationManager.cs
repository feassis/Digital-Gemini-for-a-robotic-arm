using System.Collections.Generic;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    [SerializeField] private List<Transform> targetObject = new List<Transform>();

    [SerializeField] int populationSize;
    [SerializeField] int generations;

    [SerializeField] float mutationRate;
    [SerializeField] float mutationStrength;


    [SerializeField] float L0, L1, L2, L3, clawOffset, fitnessTarget;

    [SerializeField] RoboticArm roboticArm;

    InverseKinematicsSolver kinematicsSolver;

    int procedureIndex = 0;

    private void Awake()
    {
        kinematicsSolver = new InverseKinematicsSolver(populationSize, generations, mutationRate, mutationStrength,
             L0, L1, L2, L3, clawOffset, fitnessTarget);
    }

    private void Start()
    {
        roboticArm.OnTargetAchived += RoboticArm_OnTargetAchived;
        RoboticArm_OnTargetAchived();


    }

    private void RoboticArm_OnTargetAchived()
    {
        if (procedureIndex >= targetObject.Count)
            return;
        float duration = Time.realtimeSinceStartup;

        var angles = kinematicsSolver.RunEvolution(targetObject[procedureIndex].position);

        procedureIndex++;

        roboticArm.SetTargetAngles(new float[] { angles.theta0, angles.theta1, angles.theta2, angles.theta3 });

        duration = Time.realtimeSinceStartup - duration;

        Debug.Log($"Position {procedureIndex - 1}: {duration}");
    }
}
