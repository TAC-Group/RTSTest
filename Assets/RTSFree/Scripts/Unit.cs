﻿using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;
using Tac.HealthSystem;
using MathNet.Numerics;

namespace RTSToolkitFree
{
	public class Unit : MonoBehaviour, IAgentState
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

		public GameObject WeaponPoint;


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

		public GameObject Target;

		public List<int> attackers = new List<int>();

		//public int noAttackers = 0;
		public int maxAttackers = 3;

		[HideInInspector] public float prevTargetD;
		[HideInInspector] public int failedR = 0;
		public int critFailedR = 100;

		/*
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
					//boxCollider.enabled = false;
					StartCoroutine(DelayDeath(12));
				}
			}
		}*/

		public float Health
		{
			get { return HealthState.Health; }
		}
		private HealthState HealthState;

		/// <summary>
		/// Меткость
		/// </summary>
		private PhysicalSkill precision = new PhysicalSkill(1, 100);
		/// <summary>
		/// Меткость
		/// </summary>
		public PhysicalSkill Precision { get { return precision; } set { precision = value; } }

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
		//public float selfHealFactor = 10.0f;

		public float strength = 10.0f;
		public float defence = 10.0f;

		[HideInInspector] public bool changeMaterial = true;

		public int MyNation = 0;
		public int EnemyNation = 1;

		private NavMeshAgent agent;
		//private BoxCollider boxCollider;
		private Renderer renderer;
		private StatusBar StatusBar;

		private Weapon weapon;

		void Start()
		{
			Init();
		}

		public void Init()
		{
			HealthState = new HealthState(World.rnd);
			Precision.State = 70;

			agent = GetComponent<NavMeshAgent>();
			//boxCollider = GetComponent<BoxCollider>();
			renderer = GetComponent<Renderer>();
			StatusBar = GetComponentInChildren<StatusBar>();

			weapon = GetComponentInChildren<Weapon>();
			/*Firearm firearm = weapon as Firearm;
			if (firearm != null)
			{
				float p = firearm.GetProbability(gameObject, Target);
				float d = firearm.Fire(gameObject, Target);
			}*/


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
				float sinkSpeed = -0.05f;
				transform.position += new Vector3(0f, sinkSpeed, 0f);
				//transform.position += new Vector3(0f, sinkSpeed * Time.deltaTime / sinkUpdateFraction, 0f);
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
			BodyParts bodyPart = (BodyParts)World.rnd.Next(1, 11);
			ApplyDamage(bodyPart, argDamage);
		}

		public void ApplyDamage(BodyParts argBodyPart, float argDamage)
		{
			HealthState.Body[argBodyPart].State -= argDamage;
			//Health -= argDamage;

			HealthState.CalcHealth();

			if (HealthState.Health <= 0) { isDying = true; }
			if (StatusBar != null)
			{
				StatusBar.SetHealth(HealthState.Health);
			}
			if (isDying == true)
			{
				agent.enabled = false;
				//boxCollider.enabled = false;
				StartCoroutine(DelayDeath(12));
			}
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
				Unit unit = BattleSystem.active.FindNearestUnit(EnemyNation, transform.position, AllowSearchFull);
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
					}
					else
					{
						Unit unit2 = BattleSystem.active.FindNearestUnit(EnemyNation, transform.position, AllowSearch);

						if (unit2 != null)
						{
							SetTarget(unit2.Id);
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
				Unit tmpTarget = BattleSystem.active.targets[EnemyNation][argIndex];
				if (tmpTarget.IsDead == false && tmpTarget.attackers.Count < tmpTarget.maxAttackers)
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
				Unit tmpTarget = BattleSystem.active.targets[EnemyNation][argIndex];
				if (tmpTarget.IsDead == false)
				{
					ret = true;
				}
			}
			return ret;
		}

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


		public void ClusterRetarget()
		{
			Unit unit1To = BattleSystem.active.GetUnit(this.targetId);

			if (unit1To != null)
			{
				Unit unit2From = BattleSystem.active.FindNearestUnit(MyNation, transform.position, AllowRetarget);

				if (unit2From != null)
				{
					Unit unit2To = BattleSystem.active.GetUnit(unit2From.targetId);
					if (unit2To != null)
					{
						if (EnemyNation == unit2From.EnemyNation)
						{
							float d1Old = Vector3.Distance(transform.position, unit1To.transform.position);
							float d2Old = Vector3.Distance(unit2From.transform.position, unit2To.transform.position);

							float oldSum = d1Old + d2Old;

							float d1 = Vector3.Distance(transform.position, unit2To.transform.position);
							float d2 = Vector3.Distance(unit2From.transform.position, unit1To.transform.position);
							float newSum = d1 + d2;

							if (newSum < oldSum)
							{
								ResetTarget();
								SetTarget(unit2To.Id);

								unit2From.ResetTarget();
								unit2From.SetTarget(unit1To.Id);
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
				Unit tmpTarget = BattleSystem.active.targets[MyNation][argIndex];
				if (tmpTarget.IsDead == false)
				{
					ret = true;
				}
			}
			return ret;
		}



		public void Approach()
		{
			if (IsMoving == true && targetId != -1 && weapon != null)
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
						//agent.stoppingDistance = agent.radius / (transform.localScale.x) + target.agent.radius / (target.transform.localScale.x);
						//float stoppDistance = (2f + transform.localScale.x * target.transform.localScale.x * agent.stoppingDistance);

						agent.stoppingDistance = weapon.WorkDistance;

						// если приближающийся уже близок к своей цели
						if (newTargetD < weapon.WorkDistance)
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
						//agent.stoppingDistance = agent.radius / (transform.localScale.x) + target.agent.radius / (target.transform.localScale.x);
						agent.stoppingDistance = weapon.WorkDistance;

						// distance between attacker and target

						float rTarget = Vector3.Distance(transform.position, target.transform.position);
						//float stoppDistance = (2.5f + transform.localScale.x * target.transform.localScale.x * agent.stoppingDistance);

						// if target moves away, resetting back to approach target phase

						if (rTarget > weapon.WorkDistance)
						{
							AttackCatchUp();
						}
						// attacker starts attacking their target	
						else
						{
							// if attack passes target through target defence, cause damage to target
							/*if (UnityEngine.Random.value > (strength / (strength + defence)))
							{
								target.ApplyDamage(2.0f * strength * UnityEngine.Random.value);
							}*/
							weapon.Attack(gameObject, target.gameObject);
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

	public interface IAgentState
	{
		PhysicalSkill Precision { get; set; }
		void ApplyDamage(float argDamage);
		void ApplyDamage(BodyParts argBodyPart, float argDamage);
	}



}
