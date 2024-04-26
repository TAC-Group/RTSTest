// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2016 Sergej Jakovlev
// You can use this code for educational purposes only;
// this code or its modifications cannot be used for commercial purposes
// or in proprietary libraries without permission from the author

namespace Tac.HealthSystem
{
	public class PhysicalSkill
	{
		/// <summary>
		/// �������� �������� ������
		/// </summary>
		private float state = 1;

		public float State
		{
			get { return state; }
			set
			{
				state = value;
				if (state < MinState)
				{
					state = MinState;
				}
				if (state > MaxState)
				{
					state = MaxState;
				}
			}
		}

		/// <summary>
		/// ��������� �� �������� ��������� ������
		/// </summary>
		public float DependencyState;

		/// <summary>
		/// �������� ������� ������, �������� ������
		/// </summary>
		public float ComplexState
		{
			get
			{
				float locComplexState = State - DependencyState;
				if (locComplexState < 1)
				{
					locComplexState = 1;
				}
				return locComplexState;
			}
		}

		/// <summary>
		/// ����������� ������� ��� ����� ������
		/// </summary>
		private float MinState;

		/// <summary>
		/// ������������ ������� ��� ����� ������
		/// </summary>
		private float MaxState;

		public PhysicalSkill(float argMinState, float argMaxState)
		{
			MinState = argMinState;
			MaxState = argMaxState;
		}

		public void Recalc(float argHealthState)
		{
			DependencyState = (State / (argHealthState / 100f)) - State;
		}

	}
}
