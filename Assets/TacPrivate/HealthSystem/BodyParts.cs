// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2016 Sergej Jakovlev
// You can use this code for educational purposes only;
// this code or its modifications cannot be used for commercial purposes
// or in proprietary libraries without permission from the author

using System.Collections.Generic;
using System;

namespace Tac.HealthSystem
{
	/// <summary>
	/// ����� ����
	/// </summary>
	public enum BodyParts
	{
		/// <summary>
		///  ������
		/// </summary>
		Head = 1,
		/// <summary>
		///  �����
		/// </summary>
		Thorax = 2,
		/// <summary>
		///  �����
		/// </summary>
		Abdomen = 3,

		/// ����� ������
		/// </summary>
		ThighRight = 4,
		/// <summary>
		/// ����� �����
		/// </summary>
		ThighLeft = 5,
		/// <summary>
		/// ����� ������
		/// </summary>
		ShoulderRight = 6,
		/// <summary>
		/// <summary>
		/// ����� �����
		/// </summary>
		ShoulderLeft = 7,
		/// <summary>
		/// ������ ������
		/// </summary>
		ShinRight = 8,
		/// <summary>
		/// ������ �����
		/// </summary>
		ShinLeft = 9,
		/// <summary>
		/// ���������� ������
		/// </summary>
		ForearmRight = 10,
		/// <summary>
		/// ���������� �����
		/// </summary>
		ForearmLeft = 11
	}

	/// <summary>
	/// ��������� ����� ����
	/// </summary>
	public class BodyPartState
	{
		/// <summary>
		/// ��������� ����� ����
		/// </summary>
		List<Dependency> Dependency = new List<Dependency>();

		List<VitalSystemState> SystemDependency = new List<VitalSystemState>();

		/// <summary>
		/// ���������� ���������
		/// </summary>
		/// <remarks>
		/// �� 0 �� 100% (0 - ��������� �� �������������)
		/// </remarks>
		private float state;

		public float State
		{
			get { return state; }
			set
			{
				if (value < 0)
				{
					int locDegradationForce = (int)(value * -1);
					state = 0;

					for (int i = 0; i < SystemDependency.Count; i++)
					{
						int tmpMaxForceDegradation = SystemDependency[i].MaxForceDegradation;
						SystemDependency[i].MaxForceDegradation = locDegradationForce;
						SystemDependency[i].Degradation();
						SystemDependency[i].MaxForceDegradation = tmpMaxForceDegradation;
					}
				}
				else
				{
					state = value;
				}
			}

		}


		/// <summary>
		/// �������� ���������������� � ���������� ������������ �� ������ ������ ����
		/// </summary>
		public float DependencyState;

		public float ComplexState
		{
			get
			{
				float locComplexState = State - DependencyState;
				if (locComplexState < 0)
				{
					locComplexState = 0;
				}
				return locComplexState;
			}
		}

		/// <summary>
		/// ���������� ������� �� ��������
		/// </summary>
		/// <remarks>
		/// 50% �������� ���� ������ ���� �������� ��������� �� �����������
		/// </remarks>
		public float Koef;



		public BodyPartState(float argBeginState, float argKoef)
		{
			State = argBeginState;
			Koef = argKoef;
		}

		public void AddBodyDependency(BodyPartState argDependencyPart, float argDependencyKoef)
		{
			Dependency.Add(new Dependency(argDependencyPart, argDependencyKoef));
		}

		public void AddSystemDependency(VitalSystemState argVitalSystem)
		{
			SystemDependency.Add(argVitalSystem);
		}


		public void ClearDependency()
		{
			DependencyState = 0;
		}

		/// <summary>
		/// ���������� �������� �����������, �������� ����������� ������
		/// </summary>
		/// <returns></returns>
		public float Injury()
		{
			for (int i = 0; i < Dependency.Count; i++)
			{
				DependencyState += Dependency[i].GetDependency();
			}

			return Koef * ((100 - ComplexState) / 100);
		}


	}

	/// <summary>
	/// ����������� ����� ����� ���� �� ������
	/// </summary>
	public class Dependency
	{
		/// <summary>
		/// ������� ������� �� ����������
		/// </summary>
		/// <remarks>
		/// 1 - �������� �������
		/// ������ 1 - ������� �� ���������� ������ ��� ����������� ����� �������
		/// ������ 1 - ������� �� ���������� ������ ��� ����������� ����� �������
		/// </remarks>
		public float DependencyKoef = 1;


		/// <summary>
		/// ������ �� ��������� ��������� �����
		/// </summary>
		BodyPartState DependencyPart;

		public Dependency(BodyPartState argDependencyPart, float argDependencyKoef)
		{
			DependencyPart = argDependencyPart;
			DependencyKoef = argDependencyKoef;
		}

		/// <summary>
		/// ����������� �������� �����������
		/// </summary>
		public float GetDependency()
		{
			float retDependency = 100 - (float)(Math.Pow(DependencyPart.ComplexState, DependencyKoef) / Math.Pow(100, DependencyKoef - 1));
			return retDependency;
		}

	}
}