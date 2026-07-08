using UnityEngine;

// Птица летает между точками маршрута и периодически садится на "насест" (ветку).
// Диалог запускается кликом (см. BirdInteraction.cs) и не зависит от текущего состояния.
public class BirdController : MonoBehaviour
{
    public enum State { Flying, Landed }

    [Header("Полёт")]
    public Transform[] waypoints;      // пустые GameObject-точки маршрута полёта
    public float flySpeed = 2f;
    public float rotationSpeed = 5f;

    [Header("Посадка")]
    public Transform perch;            // точка на ветке, куда садится птица
    public float minFlyTime = 5f;
    public float maxFlyTime = 12f;
    public float landedDuration = 4f;

    private State currentState = State.Flying;
    private int currentWaypoint;
    private float stateTimer;

    private void Start()
    {
        stateTimer = Random.Range(minFlyTime, maxFlyTime);
    }

    private void Update()
    {
        stateTimer -= Time.deltaTime;

        switch (currentState)
        {
            case State.Flying:
                FlyAlongPath();
                if (stateTimer <= 0f && perch != null)
                {
                    currentState = State.Landed;
                    stateTimer = landedDuration;
                }
                break;

            case State.Landed:
                transform.position = Vector3.MoveTowards(transform.position, perch.position, flySpeed * Time.deltaTime);
                if (stateTimer <= 0f)
                {
                    currentState = State.Flying;
                    stateTimer = Random.Range(minFlyTime, maxFlyTime);
                }
                break;
        }
    }

    private void FlyAlongPath()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform target = waypoints[currentWaypoint];
        transform.position = Vector3.MoveTowards(transform.position, target.position, flySpeed * Time.deltaTime);

        Vector3 dir = target.position - transform.position;
        if (dir.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Quaternion targetRot = Quaternion.Euler(0, 0, angle);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
        }
    }
}
