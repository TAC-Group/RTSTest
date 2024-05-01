// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2018-24 Sergej Jakovlev
// You can use this code for educational purposes only;
// this code or its modifications cannot be used for commercial purposes
// or in proprietary libraries without permission from the author

using UnityEngine;

namespace Tac.Npc
{
	public class Eye : MonoBehaviour
	{
		/// <summary>
		/// ����� ������������ ��������� 
		/// </summary>
		public LayerMask DangerLayerMask;
		/// <summary>
		/// ����� ������������ ����������� �� ����
		/// </summary>
		public LayerMask ObstacleLayerMask;

		/// <summary>
		/// ���� �� ����������� �� ���� �������
		/// </summary>
		public int PathOfSight(Transform argTarget)
		{
			return PathOfSight(argTarget, Vector3.zero, Vector3.zero);
		}

		/// <summary>
		/// ���� �� ����������� �� ���� �������
		/// </summary>
		public int PathOfSight(Transform argTarget, Vector3 argOffset)
		{
			return PathOfSight(argTarget, argOffset, Vector3.zero);
		}

		/// <summary>
		/// ���� �� ����������� �� ���� �������
		/// </summary>
		public int PathOfSight(Transform argTarget, Vector3 argOffset, Vector3 argTargetOffset)
		{
			RaycastHit hit;
			// ���� �� ���� �����������
			if (Physics.Linecast(transform.TransformPoint(argOffset), argTarget.TransformPoint(argTargetOffset),
				out hit, ObstacleLayerMask))
			{
				return 1; // �� ���� ���� ���� �����������
			}
			// ������ ���������� � ����� ��� � ������, ����� ���� �����������
			if (Physics.Linecast(transform.TransformPoint(argOffset), argTarget.TransformPoint(argTargetOffset), out hit, DangerLayerMask))
			{
				if (ContainsTransform(hit.transform, argTarget))
				{
					return 0; // ���� �����
				}
			}
			return -1; // ���� �� �����
		}

		// true - ���� target ����� parent
		private bool ContainsTransform(Transform target, Transform parent)
		{
			if (target == null)
			{
				return false;
			}
			if (target.Equals(parent))
			{
				return true;
			}
			return ContainsTransform(target.parent, parent);
		}
	}
}
