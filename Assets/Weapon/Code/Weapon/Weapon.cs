using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

using Tac;

public class Weapon : MonoBehaviour
{
	/// <summary>
	/// Рабочая дальность применения оружия
	/// </summary>
	public int WorkDistance;

	/// <summary>
	/// Максимальная дальность применения оружия
	/// </summary>
	public float MaxDistance { get { return WorkDistance * 2; } }

	/// <summary>
	/// Мнимальный урон 
	/// </summary>
	public float MinDamage = 0;

	/// <summary>
	/// Максимальный урон (урон зависит от точности попадания (0-1, 0 - промах, 1 - идеальное попадание) 
	/// аппроксимируя линейно между минимальным и максимальным уроном)
	/// </summary>
	public float MaxDamage;


	/// <summary>
	/// Стоимость выстрела в очках
	/// </summary>
	public int Cost;

	public AudioSource AudioSource;

	/// <summary>
	/// Атаковать (наследники оружия должы переопределить этот метод)
	/// </summary>
	/// <param name="argAgent">Агет, которому принадлежит оружие</param>
	/// <param name="argEnemy">Враг, которого атакуют</param>
	public virtual void Attack(GameObject argAgent, GameObject argEnemy) { }

}
