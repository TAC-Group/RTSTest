using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static RTSToolkitFree.KDTree;

// Управление сражениям проходит через 4 фазы: Search (поиск противника), Retarget (реакция на смену ближайшего противника)
// Approach (движение к противнику), Attack (атака противника) 
// А так же после смерти юнита происходят следующие процесы Die (смерть и труп остается какое-то время неизменным)
// и Sink to ground (медленное спускание под землю).

namespace RTSToolkitFree
{
    public class BattleSystem : MonoBehaviour
    {
        public static BattleSystem active;

		public int numberNations;
		public int playerNation = 0;

		public List<Unit> allUnits = new List<Unit>();
		public Dictionary<int, Unit> UnitIndex = new Dictionary<int, Unit>();

		public List<List<Unit>> targets = new List<List<Unit>>();
		public List<KDTree> targetKD = new List<KDTree>();

		public int randomSeed = 0;

        void Awake()
        {
            active = this;
			UnityEngine.Random.InitState(randomSeed);
        }

        void Start()
        {
            UnityEngine.AI.NavMesh.pathfindingIterationsPerFrame = 10000;
            StartCoroutine(RefreshDistanceTree());
        }

        void Update()
        {
            Calc();
        }

        void Calc()
        {
			UpdateRate(SearchPhase, "Search", 1.0f);
			UpdateRate(RetargetPhase, "Retarget", 1.0f);
			UpdateRate(ApproachPhase, "Approach", 1.0f);
			UpdateRate(AttackPhase, "Attack", 1.0f);

            DeathPhase();
        }


        public IEnumerator RefreshDistanceTree()
        {
            while (true)
            {
                // Разделение по нациям
                for (int i = 0; i < targetKD.Count; i++)
                {
                    List<Unit> nationTargets = new List<Unit>();
                    List<Vector3> nationTargetPositions = new List<Vector3>();

                    for (int j = 0; j < allUnits.Count; j++)
                    {
                        Unit up = allUnits[j];
                        if (up.nation != i && up.isMovable == true && up.IsApproachable == true)
                        {
                            nationTargets.Add(up);
                            nationTargetPositions.Add(up.transform.position);
                        }
                    }
                    targets[i] = nationTargets;
                    targetKD[i] = KDTree.MakeFromPoints(nationTargetPositions.ToArray());
                }

                yield return new WaitForSeconds(1.0f);
            }
		}


		public void AddNation()
		{
			numberNations++;
			targets.Add(new List<Unit>());
			targetKD.Add(null);
		}

        public delegate void Run(int unitIndex);
        public void UpdateRate(Run run, string phaseName, float rate)
        {
			DateTime begin = DateTime.Now;

			if (rIndex.ContainsKey(phaseName) == false)
            { 
                rIndex.Add(phaseName, 0);
            }

			int nToLoop = (int)(allUnits.Count * rate);
			for (int i = 0; i < nToLoop; i++)
			{
				rIndex[phaseName]++;
				if (rIndex[phaseName] >= allUnits.Count)
				{
					rIndex[phaseName] = 0;
				}
				run(rIndex[phaseName]);
			}

			double t = (DateTime.Now - begin).Milliseconds;
			if (t > 5)
			{
				Debug.Log(phaseName + ": " + t.ToString() + " ms");
			}
		}

		private Dictionary<string, int> rIndex = new Dictionary<string, int>();

		private void SearchPhase(int unitIndex)
		{
			allUnits[unitIndex].Search();
		}
		private void RetargetPhase(int unitIndex)
        {
            allUnits[unitIndex].Retarget();
        }
		private void ApproachPhase(int unitIndex)
		{
			allUnits[unitIndex].Approach();
		}
		private void AttackPhase(int unitIndex)
		{
			allUnits[unitIndex].Attack();
		}
		private void DeathPhase()
		{
			for (int i = 0; i < allUnits.Count; i++)
			{
				if (allUnits[i].IsDead)
				{
					UnitIndex.Remove(allUnits[i].Id);

					allUnits.RemoveAt(i);
				}
			}
		}

		public Unit FindNearestUnit(int nation, Vector3 argPosition)
		{
			if (nation < targetKD.Count && targetKD[nation] != null)
			{
				return FindNearestUnit(nation, argPosition, targetKD[nation].AllowEmpty);
			}
			else { return null; }
		}

		public Unit FindNearestUnit(int nation, Vector3 argPosition, Allow argAllowAction)
		{
			Unit ret = null;
			if (nation < targets.Count && targets[nation].Count > 0)
			{
				TreePath path = FindNearest(nation, argPosition, argAllowAction);
				if (path != null && path.Index.Count > 0)
				{
					ret = targets[nation][path.Index[path.Index.Count - 1]];
				}
			}
			return ret;
		}

		public TreePath FindNearest(int nation, Vector3 argPosition)
		{
			if (nation < targetKD.Count && targetKD[nation] != null)
			{
				return FindNearest(nation, argPosition, targetKD[nation].AllowEmpty);
			}
			else { return null; }
		}

		public TreePath FindNearest(int nation, Vector3 argPosition, Allow argAllowAction)
        {
			TreePath ret = null;
            if (nation < targets.Count && targets[nation].Count > 0)
            {
                ret = targetKD[nation].FindNearest(argPosition, argAllowAction);
            }
            return ret;
		}

		public Unit FindNearest2(Unit argUnit)
		{
			Unit best = null;
			float bestD = float.MaxValue;
            for (int i = 0; i < allUnits.Count; i++)
            {
				if (allUnits[i].nation != argUnit.nation &&
					argUnit.IsDead == false &&
					argUnit.attackers.Count < argUnit.maxAttackers)
				{
					float d = Vector3.Distance(argUnit.transform.position, allUnits[i].transform.position);
					if (d < bestD)
					{ 
						bestD = d;
						best = allUnits[i];
					}
				}
            }
			return best;
		}


#if UNITY_EDITOR
		public void OnDrawGizmos()
		{
			for (int i = 0; i < allUnits.Count; i++)
			{
				if (allUnits[i].IsDead == false)
				{
					DrawAttackers(allUnits[i]);
				}
			}
		}

		public void DrawAttackers(Unit argUnit)
		{
			var oldColor = UnityEditor.Handles.color;

			Color color = Color.grey;

			/*if (argUnit.nation == 1)
			{
				UnityEditor.Handles.color = Color.blue;
				if (argUnit.target != null)
				{
					UnityEditor.Handles.DrawLine(argUnit.transform.position, argUnit.target.transform.position);
				}

				return;
			}*/

			if (argUnit.nation == 0)
			{
				color = Color.red;
			}
			else if (argUnit.nation == 1)
			{
				color = Color.blue;
			}


				//color.a = 0.1f;



			UnityEditor.Handles.color = color;

			for (int i = 0; i < argUnit.attackers.Count; i++)
			{
				if (UnitIndex.ContainsKey(argUnit.attackers[i]))
				{
					if (argUnit.target != null && UnitIndex[argUnit.attackers[i]].Id == argUnit.target.Id)
					{
						UnityEditor.Handles.color = Color.black;
					}
					else
					{
						UnityEditor.Handles.color = color;
					}

					UnityEditor.Handles.DrawLine(argUnit.transform.position, UnitIndex[argUnit.attackers[i]].transform.position);
				}
			}
		}
#endif


	}
}
