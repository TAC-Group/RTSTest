using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

namespace RTSToolkitFree
{
	public class Unit : MonoBehaviour, IHealth
	{
		public Text Name;
		public int id;
		public int Id
		{
			get { return id; }
			set 
			{
				id = value;
				if (Name != null)
				{
					Name.text = id.ToString();
				}
			}
		}

		public bool isMovable = true;

		public bool isManual = false;

		/// <summary>
		/// Двигается ли юнит к цели
		/// </summary>
		public bool isMoving = false;
		public bool IsMoving
		{
			get { return isMoving; }
			set
			{
				isMoving = value;
				if (isMoving == true)
				{
					ChangeMaterial(Color.green);
				}
				else
				{
					ChangeMaterial(Color.yellow);
				}
			}
		}



		/// <summary>
		/// Атакует
		/// </summary>
		public bool isAttacking = false;

		public bool IsAttacking
		{
			get { return isAttacking; }
			set 
			{ 
				isAttacking = value;
				if (isAttacking == true)
				{
					ChangeMaterial(Color.red);
				}
				else
				{
					IsMoving = false;
					ChangeMaterial(Color.yellow);
				}
			}
		}


		/// <summary>
		/// Начал ли юнит процесс умирания
		/// </summary>
		private bool isDying = false;

		public int targetId = -1;

		public List<int> attackers = new List<int>();

		//public int noAttackers = 0;
		public int maxAttackers = 3;

		[HideInInspector] public float prevTargetD;
		[HideInInspector] public int failedR = 0;
		public int critFailedR = 100;


		public float health = 100.0f;
		/// <summary>
		/// Здоровье от 0 до 100 (0 - мертв)
		/// </summary>
		public float Health
		{
			get { return health; }
			set
			{
				health = value;

				if (health <= 0) { health = 0; isDying = true; }
				if (health > 100) { health = 100; }
				if (StatusBar != null)
				{
					StatusBar.SetHealth(health);
				}
				if (isDying == true)
				{
					agent.enabled = false;
					boxCollider.enabled = false;
					StartCoroutine(DelayDeath(12));
				}
			}
		}

		/// <summary>
		/// Мертв ли
		/// </summary>
		public bool IsDead
		{
			get { return Health == 0; }
		}

		/// <summary>
		/// Можно ли давать задание
		/// </summary>
		public bool IsApproachable
		{
			get { return IsDead == false; }
		}


		public float maxHealth = 100.0f;
		public float selfHealFactor = 10.0f;

		public float strength = 10.0f;
		public float defence = 10.0f;

		[HideInInspector] public bool changeMaterial = true;

		public int nation = 1;

		private NavMeshAgent agent;
		private BoxCollider boxCollider;
		private Renderer renderer;
		private StatusBar StatusBar;

		void Start()
		{
		}

		public void Init()
		{
			agent = GetComponent<NavMeshAgent>();
			boxCollider = GetComponent<BoxCollider>();
			renderer = GetComponent<Renderer>();
			StatusBar = GetComponentInChildren<StatusBar>();

			if (agent != null)
			{
				agent.enabled = true;
			}
		}


		public IEnumerator DelayDeath(float argTime)
		{
			isMovable = false;

			AttackEnd();
			ResetTarget();

			// unselecting deads	
			ManualControl manualControl = GetComponent<ManualControl>();
			if (manualControl != null)
			{
				manualControl.IsSelected = false;
			}

			GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false;


			ChangeMaterial(Color.blue);

			yield return new WaitForSeconds(argTime);
			StartCoroutine(DelaySink());
		}

		public float sinkUpdateFraction = 1f;


		public IEnumerator DelaySink()
		{
			ChangeMaterial(new Color((148.0f / 255.0f), (0.0f / 255.0f), (211.0f / 255.0f), 1.0f));

			// moving sinking object down into the ground	
			while (transform.position.y > -1.0f)
			{
				float sinkSpeed = -0.2f;
				//transform.position += new Vector3(0f, sinkSpeed, 0f);
				transform.position += new Vector3(0f, sinkSpeed * Time.deltaTime / sinkUpdateFraction, 0f);
				yield return new WaitForSeconds(0.1f);
			}

			Destroy(gameObject);
		}


		public void ResetSearching()
		{
			isManual = false;

			AttackEnd();
			ResetTarget();

			if (agent.enabled)
			{
				agent.SetDestination(transform.position);
			}

			ChangeMaterial(Color.yellow);
		}

		public void UnSetSearching()
		{
			if (isMovable)
			{
				isManual = true;

				AttackEnd();
				ResetTarget();

				if (agent.enabled)
				{
					agent.SetDestination(transform.position);
				}

				ChangeMaterial(Color.grey);
			}
		}


		public void ApplyDamage(float argDamage)
		{
			Health -= argDamage;
		}


		public void SetTarget(int argTargetId)
		{
			if (argTargetId != -1)
			{
				Unit target = BattleSystem.active.GetUnit(argTargetId);
				target.attackers.Add(Id);
			}

			targetId = argTargetId;
			IsMoving = true;
			failedR = 0;
		}

		public void ResetTarget()
		{
			if (targetId != -1)
			{
				Unit target = BattleSystem.active.GetUnit(targetId);
				if (target != null)
				{
					target.attackers.Remove(Id);
				}
			}
			targetId = -1;
			IsMoving = false;
		}

		/// <summary>
		/// Начать атаку, цель есть и находится близко
		/// </summary>
		public void AttackBegin()
		{
			IsMoving = false;
			IsAttacking = true;
		}

		/// <summary>
		/// Аттака закончена, цели нет
		/// </summary>
		public void AttackEnd()
		{
			IsAttacking = false;
			IsMoving = false;
		}

		/// <summary>
		/// Догнать, если нельзя атаковать, не меняя цели
		/// </summary>
		public void AttackCatchUp()
		{
			IsAttacking = false;
			IsMoving = true;
		}

		public void Search()
		{
			if (isMovable == true && isManual == false && IsMoving == false && targetId == -1)
			{
				Unit unit = BattleSystem.active.FindNearestUnit(nation, transform.position, AllowSearchFull);
				if (unit != null)
				{
					// Проверить, может есть атакующие, которые дальше
					if (unit.attackers.Count >= unit.maxAttackers)
					{
						float newTargetDistance = Vector3.Distance(unit.transform.position, transform.position);
						SwapAttackers(unit, newTargetDistance);
					}

					if (unit.attackers.Count < unit.maxAttackers)
					{
						SetTarget(unit.Id);
						BattleSystem.active.AddLink(Id, unit.Id);
					}
					else
					{
						Unit unit2 = BattleSystem.active.FindNearestUnit(nation, transform.position, AllowSearch);

						if (unit2 != null)
						{
							SetTarget(unit2.Id);
							BattleSystem.active.AddLink(Id, unit2.Id);
						}

					}
				}
			}
			// Удаление мертвых атакующих
			for (int i = 0; i < attackers.Count; i++)
			{ 
				if (BattleSystem.active.GetUnit(attackers[i]) == null)
				{ 
					attackers.Remove(attackers[i]);
				}
			}
		}

		public bool AllowSearch(int argIndex)
		{ 
			bool ret = false;
			if (argIndex >= 0)
			{
				Unit tmpTarget = BattleSystem.active.targets[nation][argIndex];
				if (tmpTarget.IsDead == false && tmpTarget.attackers.Count < tmpTarget.maxAttackers
						/*&& tmpTarget.attackers.Contains(Id) == false*/)
				{
					ret = true;
				}
			}
			return ret;
		}

		public bool AllowSearchFull(int argIndex)
		{
			bool ret = false;
			if (argIndex >= 0)
			{
				Unit tmpTarget = BattleSystem.active.targets[nation][argIndex];
				if (tmpTarget.IsDead == false /*&& tmpTarget.attackers.Count < tmpTarget.maxAttackers*/
						/*&& tmpTarget.attackers.Contains(Id) == false*/)
				{
					ret = true;
				}
			}
			return ret;
		}

		float oldTargetDistanceSq;


		public void SwapAttackers(Unit argUnit, float newTargetDistance)
		{
			int bestIndex = -1;
			int bestId = -1;
			float bestD = 0;
			for (int j = 0; j < argUnit.attackers.Count; j++)
			{
				Unit tmpUnit = BattleSystem.active.GetUnit(argUnit.attackers[j]);
				if (tmpUnit != null)
				{

					float d = Vector3.Distance(argUnit.transform.position, tmpUnit.transform.position);
					if (d > newTargetDistance && d > bestD)
					{
						bestD = d;
						bestId = tmpUnit.Id;
						bestIndex = j;
					}
				}
			}
			if (bestId != -1)
			{
				Unit removeUnit = BattleSystem.active.GetUnit(argUnit.attackers[bestIndex]);
				removeUnit.targetId = -1;
				removeUnit.IsMoving = false;
				argUnit.attackers.RemoveAt(bestIndex);
			}
		}


		public void Retarget()
		{
			if (IsMoving && targetId != -1)
			{
				Unit target = BattleSystem.active.GetUnit(targetId);
				if (target != null)
				{

					oldTargetDistanceSq = Vector3.Distance(target.transform.position, transform.position);
					Unit unit = BattleSystem.active.FindNearestUnit(nation, transform.position, AllowRetarget);
					if (unit != null)
					{
						// Проверить, может есть атакующие, которые дальше
						if (unit.attackers.Count >= unit.maxAttackers)
						{
							float newTargetDistance = Vector3.Distance(unit.transform.position, transform.position);
							SwapAttackers(unit, newTargetDistance);
						}

						if (unit.attackers.Count < unit.maxAttackers)
						{
							target.attackers.Remove(Id); // Удалить юнит из атакующих старой цели

							SetTarget(unit.Id);
						}
					}
				}
				else
				{
					ResetTarget();
				}
			}
		}

		public bool AllowRetarget(int argIndex)
		{
			bool ret = false;
			if (argIndex >= 0)
			{
				Unit tmpTarget = BattleSystem.active.targets[nation][argIndex];
				if (tmpTarget.IsDead == false && tmpTarget.attackers.Contains(Id) == false)
				{
					float newTargetDistanceSq = Vector3.Distance(tmpTarget.transform.position, transform.position);
					if (newTargetDistanceSq < oldTargetDistanceSq)
					{
						ret = true;
					}
				}
			}
			return ret;
		}


		public void Approach()
		{
			if (IsMoving == true && targetId != -1)
			{
				Unit target = BattleSystem.active.GetUnit(targetId);

				if (target != null && target.IsApproachable == true)
				{
					// дистанция между юнитом и целью
					float newTargetD = (transform.position - target.transform.position).magnitude;

					// Если атакующий не может подойти к своей цели, увеличивается счетчик failedR
					// и если счетчик становится больше critFailedR, то цель скидывается и запрашивается новая цель
					if (prevTargetD < newTargetD)
					{
						failedR = failedR + 1;
						if (failedR > critFailedR)
						{
							failedR = 0;
							ResetTarget();
						}
					}
					else
					{
						agent.stoppingDistance = agent.radius / (transform.localScale.x) + target.agent.radius / (target.transform.localScale.x);
						float stoppDistance = (2f + transform.localScale.x * target.transform.localScale.x * agent.stoppingDistance);

						// если приближающийся уже близок к своей цели
						if (newTargetD < stoppDistance)
						{
							agent.SetDestination(transform.position);

							// pre-setting for attacking
							AttackBegin();
						}
						else
						{
							//ChangeMaterial(Color.green);

							// начинаем двигаться
							if (isMovable)
							{
								if ((agent.destination - target.transform.position).sqrMagnitude > 1f)
								{
									agent.SetDestination(target.transform.position);
									agent.speed = 3.5f;
								}
							}
						}
					}

					// saving previous R
					prevTargetD = newTargetD;
				}
				// condition for non approachable targets	
				else
				{
					agent.SetDestination(transform.position);

					ResetTarget();
				}
			}

		}

		public void Attack()
		{
			if (isAttacking)
			{
				if (targetId != -1)
				{
					Unit target = BattleSystem.active.GetUnit(targetId);

					if (target != null)
					{
						agent.stoppingDistance = agent.radius / (transform.localScale.x) + target.agent.radius / (target.transform.localScale.x);

						// distance between attacker and target

						float rTarget = (transform.position - target.transform.position).magnitude;
						float stoppDistance = (2.5f + transform.localScale.x * target.transform.localScale.x * agent.stoppingDistance);

						// if target moves away, resetting back to approach target phase

						if (rTarget > stoppDistance)
						{
							AttackCatchUp();
						}
						// attacker starts attacking their target	
						else
						{
							// if attack passes target through target defence, cause damage to target
							if (UnityEngine.Random.value > (strength / (strength + defence)))
							{
								target.Health = target.Health - 2.0f * strength * UnityEngine.Random.value;
							}
						}
					}
					else
					{ 
						AttackEnd();
						ResetTarget();
					}
				}
				else
				{
					AttackEnd();
				}
			}
		}


		public void ChangeMaterial(Color argColor)
		{
			if (changeMaterial && renderer != null)
			{
				renderer.material.color = argColor;
			}
		}


	}

	public interface IHealth
	{
		void ApplyDamage(float argDamage);
	}



}
