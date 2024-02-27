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

    /* EXAMPLE USAGE
     *  Find or estimate the lowest point on your object
        Vector3 lowestPoint = new Vector3(controller.transform.position.x, controller.transform.position.y - 1f, controller.transform.position.z);
        
        Find the y distance relative to the lowest position
        float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;

        Add a slight buffer to the tip of the arc so it can be controlled
        float highestPointOnArc = grapplePointRelativeYPos + controller.overShootYAxis;
        
        If the determined arc point is anything less than a straight line confine it to just the buffer value so it does not bend backwards
        if (grapplePointRelativeYPos < 0) highestPointOnArc = controller.overShootYAxis;
        
        Set the velocity of the object
        rb.velocity = PhysicsExtensions.CalculateArcedVelocityToPoint(controller.transform.position, grapplePoint, highestPointOnArc);
     */
}

