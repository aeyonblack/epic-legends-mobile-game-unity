using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttacking : MonoBehaviour
{
    public LineRenderer LR;
    public Joystick Joystick;
    public Transform LookAtPoint;
    public float TrailDistance = 1;
    public Transform Player;

    private RaycastHit hit;

    private void FixedUpdate()
    {
        /*if (Mathf.Abs(Joystick.Horizontal) > 0.5f || Mathf.Abs(Joystick.Vertical) > 0.5f)
        {
            transform.position = new Vector3(Player.position.x, 1, Player.position.z - 2.57f);
            LookAtPoint.position = new Vector3(Joystick.Horizontal + Player.position.x, 1, Joystick.Vertical + Player.position.z);
            transform.LookAt(new Vector3(LookAtPoint.position.x, 0, LookAtPoint.position.z));
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
            LR.SetPosition(0, transform.position);
            if (Physics.Raycast(transform.position, transform.forward, out hit, TrailDistance))
            {
                LR.SetPosition(1, hit.point);
            }
        }*/
        MovePlayer();
    }

    private void MovePlayer()
    {
        float horizontalMovement = Joystick.Horizontal;
        float verticalMovement = Joystick.Vertical;

        Quaternion rotation = Quaternion.Euler(0f, LookAtPoint.eulerAngles.y, 0f);

        Vector3 horizontalMoveScreen = rotation * Vector3.right;
        Vector3 verticalMoveScreen = rotation * Vector3.forward;

        Vector3 moveDirection = horizontalMoveScreen * horizontalMovement + verticalMoveScreen * verticalMovement;

        if (moveDirection != Vector3.zero)
        {
            Quaternion characterRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation,
                characterRotation, 180f * Time.deltaTime);
        }
    }
}
