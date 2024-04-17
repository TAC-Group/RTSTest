using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace RTSToolkitFree
{
    public class SpawnPoint : MonoBehaviour
    {
        public GameObject objectToSpawn;
        public float TimeStep = 0.01f;
        public int numberOfObjects = 10000;
        public float size = 1.0f;

        public bool randomizeRotation = true;
        public Vector3 Offset;


        void Start()
        {
		 	StartCoroutine(Spawn());
		}


        IEnumerator Spawn()
        {
            while(numberOfObjects > 0)
            {

                Unit spawnPointUp = GetComponent<Unit>();
                if(spawnPointUp != null)
                {
                    if(spawnPointUp.IsDead)
                    {
                        numberOfObjects = 0;
                        break;
                    }
                }

                Quaternion rotation = transform.rotation;
                if (randomizeRotation)
                {
                    rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                }

                Vector2 randPos = Random.insideUnitCircle * size;

                Vector3 position = transform.position + new Vector3(randPos.x, 0f, randPos.y) + transform.rotation * Offset;
                position = World.GetTerrainPosition(position);

                GameObject instance = World.Create(objectToSpawn, position, rotation);

                Unit unit = instance.GetComponent<Unit>();
                if (unit != null)
                {
                    if(unit.EnemyNation >= BattleSystem.active.numberNations)
                    {
                        BattleSystem.active.AddNation();
                    }

                    unit.Init();

					unit.Id = World.GetId();


                    unit.ChangeMaterial(Color.white);

					BattleSystem.active.AddUnit(unit);
				}

				numberOfObjects--;
			    yield return new WaitForSeconds(TimeStep);
			}
		}

    }
}
