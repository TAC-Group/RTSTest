using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SpawnWeapon : MonoBehaviour
{
	public int Nation;
	public List<WeaponAssortment> Assortment;


	public void AddWeapon(Transform argWeaponPoint)
	{
		for (int i = 0; i < 10; i++)
		{
			int index = World.rnd.Next(0, Assortment.Count);
			if (Assortment[index].LeftCount > 0)
			{
				Assortment[index].LeftCount--;

				GameObject weaponObj = World.Create(Assortment[index].Weapon, argWeaponPoint);
				break;
			}
		}
	}

}

[Serializable]
public class WeaponAssortment
{
	public GameObject Weapon;
	public int MaxCount;
	public int LeftCount;
}
