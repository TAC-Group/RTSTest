using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static RTSToolkitFree.KDTree;
using static UnityEngine.GraphicsBuffer;

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
		private Dictionary<int, Unit> UnitIndex = new Dictionary<int, Unit>();

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
			globalTime.Start();
		}

		void Update()
        {
            Calc();
        }

        void Calc()
        {
			UpdateRateUnit(SearchPhase, "Search", 0.1f);

			UpdateRateUnit(ApproachPhase, "Approach", 0.1f);
			UpdateRateUnit(AttackPhase, "Attack", 0.1f);
			DeathPhase();

			UpdateRateUnit(RetargetPhase, "Retarget", 0.1f);
		}



		public void AddUnit(Unit argUnit)
		{
			allUnits.Add(argUnit);
			UnitIndex.Add(argUnit.Id, argUnit);
		}

		public Unit GetUnit(int argId)
		{
			if (UnitIndex.ContainsKey(argId))
			{ 
				return UnitIndex[argId];
			}
			else { return null; }
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
                        if (up.EnemyNation != i && up.isMovable == true && up.IsApproachable == true /*&& up.attackers.Count < up.maxAttackers*/)
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


		public void UpdateRateUnit(Run run, string phaseName, float rate)
		{
			UpdateRate<Unit>(run, phaseName, rate, allUnits);
		}


		public void UpdateRate<T>(Run run, string phaseName, float rate, List<T> argList)
        {
			if (argList.Count == 0) return;

			DateTime begin = DateTime.Now;

			if (rIndex.ContainsKey(phaseName) == false)
            { 
                rIndex.Add(phaseName, 0);
            }

			int nToLoop = (int)(argList.Count * rate) + 1;
			for (int i = 0; i < nToLoop; i++)
			{
				rIndex[phaseName]++;
				if (rIndex[phaseName] >= argList.Count)
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
			allUnits[unitIndex].ClusterRetarget();
		}

		private void ApproachPhase(int unitIndex)
		{
			allUnits[unitIndex].Approach();
		}
		private void AttackPhase(int unitIndex)
		{
			allUnits[unitIndex].Attack();
		}

		static System.Diagnostics.Stopwatch globalTime = new System.Diagnostics.Stopwatch();
		static int cube = 0;
		static int tetra = 0;

		private void DeathPhase()
		{
			cube = 0;
			tetra = 0;

			for (int i = 0; i < allUnits.Count; i++)
			{
				if (allUnits[i].EnemyNation == 0) cube++;
				if (allUnits[i].EnemyNation == 1) tetra++;

				if (allUnits[i].IsDead)
				{
					UnitIndex.Remove(allUnits[i].Id);

					allUnits.RemoveAt(i);
				}

			}
		}

		public void OnGUI()
		{
			GUI.Label(GUIRect(0.05f), $"Time: " + (globalTime.ElapsedMilliseconds / 1000f).ToString("F2") + " | " + cube.ToString() + " / " + tetra.ToString());
		}

		Rect GUIRect(float height)
		{
			return new Rect(Screen.width * 0.05f, Screen.height * height, 500f, 20f);
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
			Color color = Color.grey;

			if (argUnit.EnemyNation == 0)
			{
				color = Color.red;
			}
			else if (argUnit.EnemyNation == 1)
			{
				color = Color.blue;
			}

			UnityEditor.Handles.color = color;

			for (int i = 0; i < argUnit.attackers.Count; i++)
			{
				if (UnitIndex.ContainsKey(argUnit.attackers[i]))
				{
					if (argUnit.targetId != -1 && UnitIndex[argUnit.attackers[i]].Id == argUnit.targetId)
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
