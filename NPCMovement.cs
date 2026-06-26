using System.Collections;
using UnityEngine;

public class NPCMovement : MonoBehaviour
{
    public Transform player;

    public float moveSpeed = 30f;
    public float followDistance = 10.5f;

    private bool followPlayer = false;

    private void Update()
    {
        if (followPlayer && player != null)
        {
            Vector3 direction = player.position - transform.position;
            direction.y = 0;

            if (direction.magnitude > followDistance)
            {
                transform.position +=
                    direction.normalized *
                    moveSpeed *
                    Time.deltaTime;
            }

            transform.LookAt(
                new Vector3(
                    player.position.x,
                    transform.position.y,
                    player.position.z
                )
            );
        }
    }

    public void StartFollowing()
    {
        followPlayer = true;
    }

    public void StopFollowing()
    {
        followPlayer = false;
    }

    public void MoveForward()
    {
        transform.position += transform.forward * 20f;
    }

    public void MoveBackward()
    {
        transform.position -= transform.forward * 20f;
    }

    public void MoveLeft()
    {
        transform.position -= transform.right * 20f;
    }

    public void MoveRight()
    {
        transform.position += transform.right * 20f;
    }

    public void RotateLeft()
    {
        transform.Rotate(0, -450, 0);
    }

    public void RotateRight()
    {
        transform.Rotate(0, 450, 0);
    }

    public void Wave()
    {
        transform.Rotate(0, 450, 0);
    }

    public void Jump()
    {
        StartCoroutine(JumpRoutine());
    }

    IEnumerator JumpRoutine()
    {
        Vector3 start = transform.position;
        Vector3 top = start + Vector3.up * 20;

        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * 2;

            transform.position =
                Vector3.Lerp(start, top, t);

            yield return null;
        }

        t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * 2;

            transform.position =
                Vector3.Lerp(top, start, t);

            yield return null;
        }
    }
}