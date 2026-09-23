using UnityEngine;

public class CustomerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float rotationSpeed = 8f;

    private Vector3 targetPosition;

    private bool moving;

    public void MoveTo(Vector3 position)
    {
        targetPosition = position;
        moving = true;
    }

    void Update()
    {
        if (!moving)
            return;

        Vector3 direction =
            targetPosition - transform.position;

        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * rotationSpeed
            );
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(
                transform.position,
                targetPosition) < 0.01f)
        {
            moving = false;
        }
    }

    public bool IsMoving()
    {
        return moving;
    }
}