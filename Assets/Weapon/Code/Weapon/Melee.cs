using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ������ �������� ���
/// </summary>
public class Melee : Weapon
{

	public WeaponMeleeType Type;

	/// <summary>
	/// �������
	/// </summary>
	public void Hit()
	{ 
	
	}

	/// <summary>
	/// �������
	/// </summary>
	public void Throwing()
	{ 
	
	}

	/// <summary>
	/// ����� �� �������
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
