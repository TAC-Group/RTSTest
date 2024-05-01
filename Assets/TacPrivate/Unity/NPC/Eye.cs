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
		/// ћаска определ€юща€ опасности 
		/// </summary>
		public LayerMask DangerLayerMask;
		/// <summary>
		/// ћаска определ€юща€ преп€тстви€ на пути
		/// </summary>
		public LayerMask ObstacleLayerMask;

		/// <summary>
		/// ≈сть ли преп€тствие на пути взгл€да
		/// </summary>
		public int PathOfSight(Transform argTarget)
		{
			return PathOfSight(argTarget, Vector3.zero, Vector3.zero);
		}

		/// <summary>
		/// ≈сть ли преп€тствие на пути взгл€да
		/// </summary>
		public int PathOfSight(Transform argTarget, Vector3 argOffset)
		{
			return PathOfSight(argTarget, argOffset, Vector3.zero);
		}

		/// <summary>
		/// ≈сть ли преп€тствие на пути взгл€да
		/// </summary>
		public int PathOfSight(Transform argTarget, Vector3 argOffset, Vector3 argTargetOffset)
		{
			RaycastHit hit;
			// ≈сли на пути преп€тствие
			if (Physics.Linecast(transform.TransformPoint(argOffset), argTarget.TransformPoint(argTargetOffset),
				out hit, ObstacleLayerMask))
			{
				return 1; // на пути цели есть преп€тствие
			}
			// ƒолжен пересечьс€ с целью или еЄ частью, иначе есть преп€тствие
			if (Physics.Linecast(transform.TransformPoint(argOffset), argTarget.TransformPoint(argTargetOffset), out hit, DangerLayerMask))
			{
				if (ContainsTransform(hit.transform, argTarget))
				{
					return 0; // цель видна
				}
			}
			return -1; // цель не видна
		}

		// true - если target часть parent
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
