using UnityEngine;
using System;

[Serializable]
public struct ProducerItemCapacity
{
    public int level;
    public int count;
}

[Serializable]
public struct ProducerLevel
{
    public float rechargeTime;
    public ProducerItemCapacity[] itemCapacities;
    public bool canProduce;

    public int TotalCapacity
    {
        get
        {
            int total = 0;
            if (itemCapacities != null)
            {
                foreach (var capacity in itemCapacities)
                {
                    total += capacity.count;
                }
            }
            return total;
        }
    }
}