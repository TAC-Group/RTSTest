using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
		private Dictionary<int, Unit> UnitIndex = new Dictionary<int, Unit>();

		public List<List<Unit>> targets = new List<List<Unit>>();
		public List<KDTree> targetKD = new List<KDTree>();

		public Dictionary<string, Link> AllLink = new Dictionary<string, Link>();


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

			//ClusterStepForAll();


			//UpdateRateUnit(RetargetPhase, "Retarget", 0.1f);
			UpdateRateUnit(ApproachPhase, "Approach", 0.1f);
			UpdateRateUnit(AttackPhase, "Attack", 0.1f);

            DeathPhase();
			UpdateRateLink(ClusterStepOp, "Retarget", 0.1f);
			//UpdateRateLink(ClusterStep, "Retarget", 0.1f);
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
                        if (up.nation != i && up.isMovable == true && up.IsApproachable == true /*&& up.attackers.Count < up.maxAttackers*/)
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

		private List<Link> tmpLink;

		public void UpdateRateLink(Run run, string phaseName, float rate)
		{
			tmpLink = AllLink.Values.ToList();
			UpdateRate<Link>(run, phaseName, rate, tmpLink);
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

		static System.Diagnostics.Stopwatch globalTime = new System.Diagnostics.Stopwatch();
		static int cube = 0;
		static int tetra = 0;

		private void DeathPhase()
		{
			cube = 0;
			tetra = 0;

			for (int i = 0; i < allUnits.Count; i++)
			{
				if (allUnits[i].nation == 0) cube++;
				if (allUnits[i].nation == 1) tetra++;

				if (allUnits[i].IsDead)
				{
					UnitIndex.Remove(allUnits[i].Id);

					allUnits.RemoveAt(i);
				}

			}

			DeadLinkUpdate();
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

		public float DistanceSum = 0;


		public void CalcDistanceSum()
		{
			DistanceSum = 0;
			foreach (Link link in AllLink.Values) 
			{
				link.Distance = Vector3.Distance(UnitIndex[link.UnitIdFrom].transform.position,
								UnitIndex[link.UnitIdTo].transform.position);
				DistanceSum += link.Distance;
			}
		}


		public void DeadLinkUpdate()
		{
			List<Link> tmpLink = AllLink.Values.ToList();
			for (int i = 0; i < tmpLink.Count; i++)
			{
				string k1 = tmpLink[i].UnitIdFrom.ToString() + "-" + tmpLink[i].UnitIdTo.ToString();

				Unit unitFrom = GetUnit(tmpLink[i].UnitIdFrom);
				Unit unitTo = GetUnit(tmpLink[i].UnitIdTo);

				if (unitFrom == null || unitTo == null || unitFrom.IsDead || unitTo.IsDead)
				{
					AllLink.Remove(k1);
				}
			}
			CalcDistanceSum();
		}


		int currentUnitIdFrom;
		int currentNation;

		public void ClusterStepOp(int i)
		{
			Unit unit1From = GetUnit(tmpLink[i].UnitIdFrom);
			Unit unit1To = GetUnit(tmpLink[i].UnitIdTo);

			currentUnitIdFrom = unit1From.Id;
			currentNation = unit1To.nation;

			Unit unit2From = FindNearestUnit(unit1To.nation, unit1From.transform.position, AllowRetarget);

			if (unit2From != null)
			{
				Unit unit2To = GetUnit(unit2From.targetId);
				if (unit2To != null)
				{

					string k2 = unit2From.Id.ToString() + "-" + unit2To.Id.ToString();

					//for (int j = i + 1; j < AllLink.Values.Count; j++)
					{
						//Unit unit2From = GetUnit(tmpLink[j].UnitIdFrom);
						//Unit unit2To = GetUnit(tmpLink[j].UnitIdTo);

						if (unit1From.nation == unit2From.nation && AllLink.ContainsKey(k2))
						{
							float oldSum = tmpLink[i].Distance + AllLink[k2].Distance;

							float d1 = Vector3.Distance(unit1From.transform.position, unit2To.transform.position);
							float d2 = Vector3.Distance(unit2From.transform.position, unit1To.transform.position);
							float newSum = d1 + d2;

							if (newSum < oldSum)
							{
								int tmpPointId1From = tmpLink[i].UnitIdFrom;
								int tmpPointId1To = tmpLink[i].UnitIdTo;

								int tmpPointId2From = AllLink[k2].UnitIdFrom;
								int tmpPointId2To = AllLink[k2].UnitIdTo;

								RemoveLink(tmpLink[i].UnitIdFrom, tmpLink[i].UnitIdTo);
								RemoveLink(AllLink[k2].UnitIdFrom, AllLink[k2].UnitIdTo);

								AddLink(tmpPointId1From, tmpPointId2To);
								AddLink(tmpPointId2From, tmpPointId1To);

								string kk1 = tmpPointId1From.ToString() + "-" + tmpPointId2To.ToString();
								string kk2 = tmpPointId2From.ToString() + "-" + tmpPointId1To.ToString();


								unit1From.ResetTarget();
								unit1From.SetTarget(tmpPointId2To);

								unit2From.ResetTarget();
								unit2From.SetTarget(tmpPointId1To);

								AllLink[kk1].Distance = d1;
								AllLink[kk2].Distance = d2;

							}
						}
					}
				}
			}
		}

		public bool AllowRetarget(int argIndex)
		{
			bool ret = false;
			if (argIndex >= 0)
			{
				Unit tmpTarget = BattleSystem.active.targets[currentNation][argIndex];

				if (tmpTarget.IsDead == false && currentUnitIdFrom != tmpTarget.Id)
				{
					ret = true;
				}
			}
			return ret;
		}


		public void ClusterStep(int i)
		{
			Unit unit1From = GetUnit(tmpLink[i].UnitIdFrom);
			Unit unit1To = GetUnit(tmpLink[i].UnitIdTo);

			for (int j = i + 1; j < AllLink.Values.Count; j++)
			{
				Unit unit2From = GetUnit(tmpLink[j].UnitIdFrom);
				Unit unit2To = GetUnit(tmpLink[j].UnitIdTo);

				if (unit1From.nation == unit2From.nation)
				{
					float oldSum = tmpLink[i].Distance + tmpLink[j].Distance;

					float d1 = Vector3.Distance(unit1From.transform.position, unit2To.transform.position);
					float d2 = Vector3.Distance(unit2From.transform.position, unit1To.transform.position);
					float newSum = d1 + d2;

					if (newSum < oldSum)
					{
						RemoveLink(tmpLink[i].UnitIdFrom, tmpLink[i].UnitIdTo);
						RemoveLink(tmpLink[j].UnitIdFrom, tmpLink[j].UnitIdTo);

						int tmpPointId = tmpLink[i].UnitIdTo;
						tmpLink[i].UnitIdTo = tmpLink[j].UnitIdTo;
						tmpLink[j].UnitIdTo = tmpPointId;


						unit1From.ResetTarget();
						unit1From.SetTarget(tmpLink[i].UnitIdTo);

						unit2From.ResetTarget();
						unit2From.SetTarget(tmpLink[j].UnitIdTo);

						tmpLink[i].Distance = d1;
						tmpLink[j].Distance = d2;

						AddLink(tmpLink[i].UnitIdFrom, tmpLink[i].UnitIdTo);
						AddLink(tmpLink[j].UnitIdFrom, tmpLink[j].UnitIdTo);
					}
				}
			}
		}

		public void ClusterStepForAll()
		{
			List<Link> tmpLink = AllLink.Values.ToList();
			for (int i = 0; i < tmpLink.Count; i++)
			{
				Unit unit1From = GetUnit(tmpLink[i].UnitIdFrom);
				Unit unit1To = GetUnit(tmpLink[i].UnitIdTo);

				for (int j = i + 1; j < AllLink.Values.Count; j++)
				{
					Unit unit2From = GetUnit(tmpLink[j].UnitIdFrom);
					Unit unit2To = GetUnit(tmpLink[j].UnitIdTo);

					if (unit1From.nation == unit2From.nation)
					{
						float oldSum = tmpLink[i].Distance + tmpLink[j].Distance;

						float d1 = Vector3.Distance(unit1From.transform.position, unit2To.transform.position);
						float d2 = Vector3.Distance(unit2From.transform.position, unit1To.transform.position);
						float newSum = d1 + d2;

						if (newSum < oldSum)
						{
							RemoveLink(tmpLink[i].UnitIdFrom, tmpLink[i].UnitIdTo);
							RemoveLink(tmpLink[j].UnitIdFrom, tmpLink[j].UnitIdTo);

							int tmpPointId = tmpLink[i].UnitIdTo;
							tmpLink[i].UnitIdTo = tmpLink[j].UnitIdTo;
							tmpLink[j].UnitIdTo = tmpPointId;
							

							unit1From.ResetTarget();
							unit1From.SetTarget(tmpLink[i].UnitIdTo);

							unit2From.ResetTarget();
							unit2From.SetTarget(tmpLink[j].UnitIdTo);

							tmpLink[i].Distance = d1;
							tmpLink[j].Distance = d2;

							AddLink(tmpLink[i].UnitIdFrom, tmpLink[i].UnitIdTo);
							AddLink(tmpLink[j].UnitIdFrom, tmpLink[j].UnitIdTo);
						}
					}
				}
			}
		}


		public void AddLink(int argUnitIdFrom, int argUnitIdTo)
		{
			string k1 = argUnitIdFrom.ToString() + "-" + argUnitIdTo.ToString();
			string k2 = argUnitIdTo.ToString() + "-" + argUnitIdFrom.ToString();

			if (AllLink.ContainsKey(k1) == false)
			{
				AllLink.Add(k1, new Link(argUnitIdFrom, argUnitIdTo));
			}

			if (AllLink.ContainsKey(k1) == true && AllLink.ContainsKey(k2) == true)
			{
				AllLink[k1].TwoDirection = true;
				AllLink[k2].TwoDirection = true;
			}
		}

		public void RemoveLink(int argUnitIdFrom, int argUnitIdTo)
		{
			string k1 = argUnitIdFrom.ToString() + "-" + argUnitIdTo.ToString();
			string k2 = argUnitIdTo.ToString() + "-" + argUnitIdFrom.ToString();

			if (AllLink.ContainsKey(k1) == true && AllLink.ContainsKey(k2) == true)
			{
				AllLink[k1].TwoDirection = false;
				AllLink[k2].TwoDirection = false;
			}

			if (AllLink.ContainsKey(k1) == true)
			{
				AllLink.Remove(k1);
			}
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

			//DrawLink();
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

		public void DrawLink()
		{
			Color color = Color.grey;

			Dictionary<string, int> l = new Dictionary<string, int>();
			for (int i = 0; i < AllLink.Values.Count; i++)

			foreach (Link link in AllLink.Values)
			{
				Unit unitFrom = GetUnit(link.UnitIdFrom);
				Unit unitTo = GetUnit(link.UnitIdTo);

				if (unitFrom != null & unitTo != null)
				{
					string d1 = unitFrom.Id.ToString() + "-" + unitTo.Id.ToString();

					if (AllLink.ContainsKey(d1) && AllLink[d1].TwoDirection == true)
					{
						color = Color.black;
					}
					else
					{
						if (unitFrom.nation == 0)
						{
							color = Color.red;
						}
						else if (unitFrom.nation == 1)
						{
							color = Color.blue;
						}
					}

					UnityEditor.Handles.color = color;

					UnityEditor.Handles.DrawLine(unitFrom.transform.position, unitTo.transform.position);
				}
			}
		}


#endif


	}



	public class Link
	{
		public bool TwoDirection = false;

		public int UnitIdFrom;
		public int UnitIdTo;
		public float Distance;

		public Link(int argUnitIdFrom, int argUnitIdTo)
		{
			UnitIdFrom = argUnitIdFrom;
			UnitIdTo = argUnitIdTo;
		}
	}

}
