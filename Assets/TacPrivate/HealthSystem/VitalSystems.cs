// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2016 Sergej Jakovlev
// You can use this code for educational purposes only;
// this code or its modifications cannot be used for commercial purposes
// or in proprietary libraries without permission from the author

namespace Tac.HealthSystem
{

	/// <summary>
	/// �������� ������ �������
	/// </summary>
	public enum VitalSystems
	{
		/// <summary>
		/// �������� ������������
		/// </summary>
		Cerebration = 1,
		/// <summary>
		/// ����������� �������
		/// </summary>
		�irculatory = 2,
		/// <summary>
		/// ����������� �������
		/// </summary>
		Respiratory = 3,
		/// <summary>
		/// ������� �������
		/// </summary>
		Digestive = 4,
		/// <summary>
		/// �������� �������
		/// </summary>
		Immune = 5
	}

	/// <summary>
	/// ��������� �������� ������ �������
	/// </summary>
	public class VitalSystemState
	{
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
				state = value;
				if (state < 0)
				{
					state = 0;
				}
			}
		}

		/// <summary>
		/// �������� ���������� (���� ��� ����� ��� ���-�� ������)
		/// </summary>
		public int SpeedDegradation;
		/// <summary>
		/// �������� ����������� (���� ��� ����� ��� ���-�� ������)
		/// </summary>
		public int SpeedRegeneration;
		/// <summary>
		/// ������������ ���� (�������������) ����������
		/// </summary>
		public int MaxForceDegradation;
		/// <summary>
		/// ������������ ���� (�������������) ��������������
		/// </summary>
		public int MaxForceRegeneration;

		/// <summary>
		/// ������� ������������ (��� �������) ���������
		/// </summary>
		public int LevelIrreversibleChange;


		/// <summary>
		/// ��������� �����������
		/// </summary>
		private System.Random rnd;

		public VitalSystemState(int argState, int argSpeedDegradation, int argSpeedRegeneration,
			int argMaxForceDegradation, int argMaxForceRegeneration,
			int argLevelIrreversibleChange)
		{
			State = argState;
			SpeedDegradation = argSpeedDegradation;
			SpeedRegeneration = argSpeedRegeneration;
			MaxForceDegradation = argMaxForceDegradation;
			MaxForceRegeneration = argMaxForceRegeneration;
			LevelIrreversibleChange = argLevelIrreversibleChange;
			SetRandom(new System.Random());
		}

		public void SetRandom(System.Random argRandom)
		{
			rnd = argRandom;
		}

		public void Degradation()
		{
			float locChangeValue = (float)(rnd.NextDouble() * MaxForceDegradation);
			State -= locChangeValue;
		}

		public void AutoRegeneration()
		{
			if (State > LevelIrreversibleChange)
			{
				Regeneration();
			}
		}

		public void Regeneration()
		{
			float locChangeValue = (float)(rnd.NextDouble() * MaxForceDegradation);
			State += locChangeValue;
		}

	}
}