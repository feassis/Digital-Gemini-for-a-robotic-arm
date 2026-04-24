using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InverseKinematicsSolver
{
    int populationSize;
    int generations;

    float mutationRate;
    float mutationStrength;

    Vector3 targetPosition;

    float L0, L1, L2, L3, ClawOffset,fitnessTarget;

    List<Individual> population = new List<Individual>();

    public void SetTargetPosition(Vector3 targetPosition) => this.targetPosition = targetPosition;

    public InverseKinematicsSolver(int populationSize, int generations, float mutationRate, float mutationStrength, 
        float l0, float l1, float l2, float l3, float clawOffset, float fitnessTarget)
    {
        this.populationSize = populationSize;
        this.generations = generations;
        this.mutationRate = mutationRate;
        this.mutationStrength = mutationStrength;
        L0 = l0;
        L1 = l1;
        L2 = l2;
        L3 = l3;
        ClawOffset = clawOffset;
        this.fitnessTarget = fitnessTarget;
    }

    Individual CreateRandomIndividual()
    {
        return new Individual
        {
            theta1 = Random.Range(-90f, 90f),
            theta2 = Random.Range(-90f, 90f),
            theta3 = Random.Range(-90f, 90f)
        };
    }


    float EvaluateFitness(Individual ind)
    {
        float x = targetPosition.x;
        float y = targetPosition.y;
        float z = targetPosition.z;

        float dTarget = Mathf.Sqrt(x * x + z * z);

        float t1 = Mathf.Deg2Rad * ind.theta1;
        float t2 = Mathf.Deg2Rad * ind.theta2;
        float t3 = Mathf.Deg2Rad * ind.theta3;

        float d =
            -(L1 * Mathf.Sin(t1)
            + L2 * Mathf.Sin(t1 + t2)
            + (L3 + ClawOffset) * Mathf.Sin(t1 + t2 + t3));

        float yEst =
            L0
            + L1 * Mathf.Cos(t1)
            + L2 * Mathf.Cos(t1 + t2)
            + (L3+ ClawOffset) * Mathf.Cos(t1 + t2 + t3);

        float error =
            Mathf.Pow(dTarget - d, 2) +
            Mathf.Pow(y - yEst, 2);

        return error; 
    }


    List<Individual> SelectionByElitism()
    {
        population.Sort((a, b) => a.fitness.CompareTo(b.fitness));

        int survivors = populationSize / 2;
        return population.Take(survivors).ToList();
    }

    List<Individual> SelectionByTournament()
    {
        List<Individual> newPopulation = new List<Individual>(population);

        while (newPopulation.Count > populationSize / 2)
        {
            int i1 = Random.Range(0, newPopulation.Count);
            int i2 = Random.Range(0, newPopulation.Count);

            if (i1 == i2) continue;

            Individual a = newPopulation[i1];
            Individual b = newPopulation[i2];

            if (a.fitness > b.fitness)
                newPopulation.RemoveAt(i1);
            else
                newPopulation.RemoveAt(i2);
        }

        return newPopulation;
    }

    Individual ReproductionByCrossover(Individual a, Individual b)
    {
        var ind = new Individual
        {
            theta1 = Random.value < 0.5f ? a.theta1 : b.theta1,
            theta2 = Random.value < 0.5f ? a.theta2 : b.theta2,
            theta3 = Random.value < 0.5f ? a.theta3 : b.theta3
        };

        ind.fitness = EvaluateFitness(ind);
        return ind;
    }

    Individual ReproductionByAverage(Individual a, Individual b)
    {
        var ind = new Individual
        {
                theta1 = (a.theta1 + b.theta1) * 0.5f,
                theta2 = (a.theta2 + b.theta2) * 0.5f,
                theta3 = (a.theta3 + b.theta3) * 0.5f
        };

        ind.fitness = EvaluateFitness(ind);
        return ind;
    }

    void Mutate(Individual ind)
    {
        if (Random.value < mutationRate)
           ind.theta1 += Random.Range(-mutationStrength, mutationStrength);

        if (Random.value < mutationRate)
           ind.theta2 += Random.Range(-mutationStrength, mutationStrength);

        if (Random.value < mutationRate)
           ind.theta3 += Random.Range(-mutationStrength, mutationStrength);
    }

    public (float theta0, float theta1, float theta2, float theta3) RunEvolution(Vector3 targetPosition)
    {
        this.targetPosition = targetPosition;
        // população inicial
        population.Clear();
        for (int i = 0; i < populationSize; i++)
            population.Add(CreateRandomIndividual());

        for (int gen = 0; gen < generations; gen++)
        {
            // fitness
            foreach (var ind in population)
                ind.fitness = EvaluateFitness(ind);


            // parada
            if (population[0].fitness < fitnessTarget)
                break;

            // seleção
            population = SelectionByTournament();

            // reprodução
            while (population.Count < populationSize)
            {
                var p1 = population[Random.Range(0, population.Count)];
                var p2 = population[Random.Range(0, population.Count)];
                population.Add(ReproductionByAverage(p1, p2));
            }

            // mutação
            foreach (var ind in population)
                Mutate(ind);
        }

        population.Sort((a, b) => a.fitness.CompareTo(b.fitness));

        float theta0 = -Mathf.Atan2(targetPosition.z, targetPosition.x) * Mathf.Rad2Deg;

        return (theta0, population[0].theta1, population[0].theta2, population[0].theta3);
    }
}
