using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    public float viewRadius;

    [Range(0,360)]
    public float viewAngle;

    public LayerMask targetMask;

    public LayerMask obstacleMask;

    [HideInInspector]
    public Queue<GameObject> visibleTargets = new Queue<GameObject>();

    [HideInInspector]
    public Dictionary<GameObject, float> orderedTargets = new Dictionary<GameObject, float>();

    // delete this later
    public Queue<GameObject> FindNearestVisibleTargets(AgentAction action, bool ascendingSelect)
    {
        visibleTargets.Clear();
        orderedTargets.Clear();

        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, action.targetMask);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            GameObject target = targetsInViewRadius[i].gameObject;
            Vector3 dirToTarget = (target.transform.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
                if (!Physics.Raycast(transform.position, dirToTarget, distanceToTarget, obstacleMask))
                {
                    if (target.gameObject.name != gameObject.name && 
                        target.tag != "Grenade")
                    {
                        orderedTargets.Add(target, distanceToTarget);
                    }
                }
            }
        }

        var sorted = ascendingSelect? from entry in orderedTargets orderby entry.Value ascending select entry :
            from entry in orderedTargets orderby entry.Value descending select entry;

        foreach (KeyValuePair<GameObject, float> t in sorted)
        {
            visibleTargets.Enqueue(t.Key);
        }

        return visibleTargets;
    }

    public Queue<GameObject> FindVisibleTargets(GoapAction action, bool ascendingSelect)
    {
        visibleTargets.Clear();
        orderedTargets.Clear();

        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, action.targetMask);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            GameObject target = targetsInViewRadius[i].gameObject;
            Vector3 dirToTarget = (target.transform.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
                if (!Physics.Raycast(transform.position, dirToTarget, distanceToTarget, obstacleMask))
                {
                    if (target.gameObject.name != gameObject.name &&
                        target.tag != "Grenade")
                    {
                        orderedTargets.Add(target, distanceToTarget);
                    }
                }
            }
        }

        var sorted = ascendingSelect ? from entry in orderedTargets orderby entry.Value ascending select entry :
            from entry in orderedTargets orderby entry.Value descending select entry;

        foreach (KeyValuePair<GameObject, float> t in sorted)
        {
            visibleTargets.Enqueue(t.Key);
        }

        return visibleTargets;
    }

    public Vector3 DirFromAngle(float angle, bool global)
    {
        if (!global)
        {
            angle += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle* Mathf.Deg2Rad));
    }

}
