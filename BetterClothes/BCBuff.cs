using System;
using Vintagestory.API.Config;

namespace BetterClothes;
public class BCBuff
{
    public BCBuff(string name, string treeKey, BuffType enchantType, float[] values, bool isPercent = true)
    {
        Name = name;
        TreeKey = treeKey;
        Type = enchantType;
        IsPercent = isPercent;
        Values = values;
    }

    public string Name { get; set; }

    public string GetName()
    {
        return BCMethods.Translate(Name.ToLower());
    }

    public string TreeKey { get; set; }
    public BuffType Type { get; set; }
    public bool IsPercent { get; set; } = true;
    public float[] Values { get; set; }

    private float lastValue = 0;

    public float Value { get => lastValue; }
    public float GetRandomValue()
    {
        var value = Get();
        lastValue = value;
        return value;

        float Get()
        {
            if (Values == null || Values.Length == 0)
                throw new Exception($"Values for Buff of type {Type} has not been set.");

            double totalWeight = 0;
            double[] weights = new double[Values.Length];

            for (int i = 0; i < Values.Length; i++)
            {
                float value = Values[i] > 0 ? Values[i] : 1;

                weights[i] = 1.0 / value;
                totalWeight += weights[i];
            }

            double randomValue = BCData.Random.NextDouble() * totalWeight;

            double currentWeight = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                currentWeight += weights[i];
                if (randomValue <= currentWeight)
                {
                    return Values[i];
                }
            }

            // Fallback
            return Values[0];
        }
    }
}
