using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Enemy : MonoBehaviour
{
    public enum NPCState { Idle, Patrol, MoveToPlayer }
    public NPCState currentState = NPCState.Idle;

    public float moveSpeed = 2.0f;
    public Transform patrolPointA;
    public Transform patrolPointB;
    private Transform targetPatrolPoint;

    public Text stateText;
    public Text timerText;
    public LineRenderer lineOfSight;

    private Transform player;
    private float stateTimer = 5f;
    private int stateChangeCounter = 0;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        targetPatrolPoint = patrolPointA;
        StartCoroutine(StateSwitcher());
    }

    void Update()
    {
        UpdateLineOfSight();
        CheckForPlayerInSight();
        stateText.text = "State: " + currentState;

        switch (currentState)
        {
            case NPCState.Idle:
                break;

            case NPCState.Patrol:
                MoveToTarget(targetPatrolPoint);

                // If close to the patrol point, switch to the other point
                if (Vector3.Distance(transform.position, targetPatrolPoint.position) < 0.2f)
                {
                    targetPatrolPoint = (targetPatrolPoint == patrolPointA) ? patrolPointB : patrolPointA;
                }
                break;

            case NPCState.MoveToPlayer:
                MoveToTarget(player);
                break;
        }
    }

    void MoveToTarget(Transform target)
    {
        Vector3 direction = (target.position - transform.position).normalized;

        // Move NPC towards the target
        transform.position += direction * moveSpeed * Time.deltaTime;

        // Smoothly rotate NPC only on the Y-axis
        Vector3 lookDirection = new Vector3(direction.x, 0, direction.z);
        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    void ChangeState(NPCState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            Debug.Log("NPC State Changed to: " + newState);

            // Play stage change sound whenever the state changes
            AudioManager.Instance.PlayStageChangeSound();
        }
    }

    IEnumerator StateSwitcher()
    {
        while (true)
        {
            float remainingTime = stateTimer;

            // Countdown timer
            while (remainingTime > 0)
            {
                timerText.text = "Switching in: " + remainingTime.ToString("F1");
                yield return new WaitForSeconds(0.1f);
                remainingTime -= 0.1f;
            }

            if (currentState == NPCState.Idle || currentState == NPCState.Patrol)
            {
                if (stateChangeCounter == 2)
                {
                    ChangeState(currentState == NPCState.Idle ? NPCState.Patrol : NPCState.Idle);
                    stateChangeCounter = 0;
                }
                else
                {
                    ChangeState(Random.value > 0.5f ? NPCState.Idle : NPCState.Patrol);
                    stateChangeCounter++;
                }
            }
        }
    }

    void CheckForPlayerInSight()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float dotProduct = Vector3.Dot(transform.forward, directionToPlayer);

        // Check if player is in front of NPC (field of view)
        if (dotProduct > 0.7f) // Adjust 0.7 to narrow/widen the vision cone
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, directionToPlayer, out hit, 15f)) // 15f is detection range
            {
                if (hit.collider.CompareTag("Player"))
                {
                    Debug.Log("Player spotted! Moving towards player.");
                    ChangeState(NPCState.MoveToPlayer);
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(HandleGameEnd(currentState == NPCState.MoveToPlayer));
        }
    }

    private IEnumerator HandleGameEnd(bool isPlayerLost)
    {
        if (isPlayerLost)
        {
            Debug.Log("Game Over! Player Lost.");
            AudioManager.Instance.PlayLoseSound();
        }
        else
        {
            Debug.Log("Congratulations! Player Wins.");
            AudioManager.Instance.PlayWinSound();
        }

        // Wait for the sound to play
        yield return new WaitForSeconds(0.5f);

        // Load the appropriate scene
        SceneManager.LoadScene(isPlayerLost ? "DefeatScene" : "VictoryScene");
    }

    private void UpdateLineOfSight()
    {
        if (lineOfSight != null)
        {
            lineOfSight.SetPosition(0, transform.position);
            lineOfSight.SetPosition(1, transform.position + transform.forward * 15f); // 15f is detection range
        }
    }
}