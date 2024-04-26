using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

using Tac;

public class Weapon : MonoBehaviour
{
	/// <summary>
	/// ������� ��������� ���������� ������
	/// </summary>
	public int WorkDistance;

	/// <summary>
	/// ������������ ��������� ���������� ������
	/// </summary>
	public float MaxDistance { get { return WorkDistance * 2; } }

	/// <summary>
	/// ���������� ���� 
	/// </summary>
	public float MinDamage = 0;

	/// <summary>
	/// ������������ ���� (���� ������� �� �������� ��������� (0-1, 0 - ������, 1 - ��������� ���������) 
	/// ������������� ������� ����� ����������� � ������������ ������)
	/// </summary>
	public float MaxDamage;


	/// <summary>
	/// ��������� �������� � �����
	/// </summary>
	public int Cost;

	public AudioSource AudioSource;

	/// <summary>
	/// ��������� (���������� ������ ����� �������������� ���� �����)
	/// </summary>
	/// <param name="argAgent">����, �������� ����������� ������</param>
	/// <param name="argEnemy">����, �������� �������</param>
	public virtual void Attack(GameObject argAgent, GameObject argEnemy) { }

}
