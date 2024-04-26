using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Оружие ближнего боя
/// </summary>
public class Melee : Weapon
{

	public WeaponMeleeType Type;

	/// <summary>
	/// Ударить
	/// </summary>
	public void Hit()
	{ 
	
	}

	/// <summary>
	/// Бросить
	/// </summary>
	public void Throwing()
	{ 
	
	}

	/// <summary>
	/// Можно ли бросить
	/// </summary>
	public bool AllowThrowing()
	{
		bool ret = false;
		switch (Type)
		{
			case WeaponMeleeType.GutHookKnife:
			case WeaponMeleeType.EndlessKnive:
				ret = true;
				break;
		}
		return ret;
	}


}
