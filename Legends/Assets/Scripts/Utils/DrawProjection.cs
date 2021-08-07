using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawProjection : AimingHelper
{
    public WeaponController WeaponController;
    public int numPoints = 50;
    public float timeBetweenPoints = 0.1f;
    public LayerMask CollidableLayers;

    private LineRenderer lineRenderer;
    private Vector3 startingPosition;
    private Vector3 startingVelocity;

    private Vector3 lastPoint;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
    }

    private void Update()
    {
        //DrawPath();
    }

    public void DrawPath(PlayerWeapon launcher)
    {
        lineRenderer.positionCount = numPoints;
        List<Vector3> points = new List<Vector3>();
        startingPosition = launcher.FirePoint.position;
        startingVelocity = launcher.FirePoint.forward * launcher.GrenadeLaunchForce * launcher.FireButton.input.magnitude;
        CalculatePoint(points);
        lineRenderer.SetPositions(points.ToArray());
    }

    private void CalculatePoint(List<Vector3> points)
    {
        for (float t = 0; t < numPoints; t += timeBetweenPoints)
        {
            Vector3 newPoint = startingPosition + t * startingVelocity;
            newPoint.y = startingPosition.y + startingVelocity.y * t + Physics.gravity.y / 2f * t * t;
            points.Add(newPoint);

            if (Physics.OverlapSphere(newPoint, 2, CollidableLayers).Length > 0)
            {
                lineRenderer.positionCount = points.Count;
                break;
            }
            lastPoint = newPoint;
        }
    }

    public override void Enable()
    {
        lineRenderer.enabled = true;
        isEnabled = true;
    }

    public override void Disable()
    {
        lineRenderer.enabled = false;
        isEnabled = false;
    }
}
