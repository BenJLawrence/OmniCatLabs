using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PhysicsExtensions
{
    /// <summary>
    /// Finds the velocity needed to move a rigidbody in an arc to an exact end point
    /// </summary>
    /// <param name="startPoint">The point that the rigid body is trying to move from</param>
    /// <param name="endPoint">The point that the rigid body is trying to move to</param>
    /// <param name="trajectoryHeight">The height of the arced movement which the rigid body will pass through</param>
    /// <returns></returns>
    public static Vector3 CalculateArcedVelocityToPoint(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity)
            + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

        return velocityXZ + velocityY;
    }
}

