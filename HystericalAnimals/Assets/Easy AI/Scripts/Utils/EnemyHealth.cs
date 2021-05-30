using UnityEngine;
using System.Collections;
using UnityEngine.AI;

namespace  AxlPlay{
public class EnemyHealth : MonoBehaviour {
	
	public int health = 100;
	
	private Animator _anim;
	private NavMeshAgent _agent;
	
	private bool isDead;
	// Use this for initialization
	void Start () {
		
		_anim = GetComponent<Animator>();
		_agent = GetComponent<NavMeshAgent>();
		
	}
	
	
	public void TakeDamage(int amount, Vector3 hitPoint){
		
		health = health - amount;
		
		var temp = ZombieGameManager.current.getBloodPooledObject();
		
		temp.transform.position = hitPoint;
		temp.SetActive(true);
		
		
		if (health <= 0 && !isDead){
			
			Die();
			
		}
	}
	void Die(){
		isDead = true;
		_anim.SetTrigger("Die");
        _agent.isStopped = true;
		StartCoroutine("Deactive");
	}
	IEnumerator Deactive (){
		
		
		yield return new WaitForSeconds(3f);
		
		//// re start the enemy
		
		Transform newPos = ZombieGameManager.current.getRandomPos();
		
		transform.position = newPos.position;
		
		_agent.SetDestination(newPos.position);
        _agent.isStopped = false;

		_anim.SetTrigger("Restart");
		health = 100;
		
		isDead = false;
		///
		 
		
	}
}
}
