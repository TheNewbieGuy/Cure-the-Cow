using UnityEngine;
using System.Collections;

public class CustomerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;

    [Header("Rotation")]
    public float turnSpeed = 180f;

    private Vector3 targetPosition;
    private bool moving;
    private bool turning;

    public void MoveTo(Vector3 position)
    {
        targetPosition = position;
        moving = true;
    }

    public void TurnAround()
    {
        if (!turning)
        {
            StartCoroutine(TurnAroundRoutine());
        }
    }

    private IEnumerator TurnAroundRoutine()
    {
        turning = true;

        float startY = transform.eulerAngles.y;
        float targetY = startY + 180f;

        while (Mathf.Abs(
                   Mathf.DeltaAngle(
                       transform.eulerAngles.y,
                       targetY
                   )) > 0.5f)
        {
            float newY = Mathf.MoveTowardsAngle(
                transform.eulerAngles.y,
                targetY,
                turnSpeed * Time.deltaTime
            );

            transform.rotation = Quaternion.Euler(
                0f,
                newY,
                0f
            );

            yield return null;
        }

        transform.rotation = Quaternion.Euler(
            0f,
            targetY,
            0f
        );

        turning = false;
    }

    void Update()
    {
        if (!moving || turning)
            return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(
                transform.position,
                targetPosition
            ) < 0.01f)
        {
            transform.position = targetPosition;
            moving = false;
        }
    }

    public bool IsMoving()
    {
        return moving || turning;
    }
}