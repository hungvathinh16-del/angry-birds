using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.UIElements;

public class ball : MonoBehaviour
{
    private bool isPressed = false;
    public Rigidbody2D rb;
    public float releasetime = .15f;
    public Rigidbody2D hook;
    public float maxdragdis = 3f;
    public float power;
    public float maxShoot;
    public LineRenderer line;
    Vector2 DragStartPos;
    Vector2 DragEndPos;
    readonly List<Vector3> points = new();
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        line = GetComponent<LineRenderer>();
        line.enabled = false;
        DragStartPos = (Vector3)hook.position;

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            DragEndPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        
        if (isPressed)
        {
            DragEndPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (Vector3.Distance(DragStartPos, DragEndPos) > maxdragdis)
                rb.position = DragStartPos + (DragEndPos - DragStartPos).normalized * maxdragdis;
            else
                rb.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    

    private void OnMouseDown()
    {
        Debug.Log("clicked");
        isPressed = true;
        rb.bodyType = RigidbodyType2D.Kinematic;
        line.enabled = true;
    }

    IEnumerator Release()
    {
        yield return new WaitForSeconds(releasetime);

        GetComponent<SpringJoint2D>().enabled = false;
    }
    //StartCoroutine(Release());

    private void OnMouseUp()
    {
        isPressed = false;
        rb.bodyType = RigidbodyType2D.Dynamic;

        GetComponent<SpringJoint2D>().enabled = false;
        Shoot();
        line.enabled = false;

    }

    void Shoot()
    { 
        Vector2 _velocity = (DragStartPos - DragEndPos) * power;
        if (_velocity.magnitude > maxShoot)
        {
            _velocity = _velocity.normalized * maxShoot;
        }

        rb.linearVelocity = _velocity;
        Debug.Log("speed " +  _velocity.magnitude);
    }

    private void FixedUpdate()
    {
        DrawTrajectory();
    }

    Vector3 getNextPosition(Vector3 currentp, Vector3 vel)
    {
        //vị trí hiện tại + v*t
        return (currentp + vel * 0.02f);
    }
    void DrawTrajectory()
    {
        var pos = transform.position;
        var v0 = (DragStartPos - DragEndPos) * power;
        if (v0.magnitude > maxShoot)
        {
            v0 = v0.normalized * maxShoot;
        }
        points.Capacity = 30;
        points.Clear();
        points.Add(pos);
        while(true)
        {
            pos = getNextPosition(pos, v0);
            pos.z = -1;//cái linerenderer bên em nếu ở z=0 thì không hiện
            points.Add(pos);
            if(points.Count >= 30 || !isPressed)
            {
                break;
            }
            //v = v0 x t x 1/2a (gravityScale = 1.5)
            v0 += Physics2D.gravity * rb.gravityScale * 0.02f; 
        }
        line.positionCount = points.Count;
        line.SetPositions(points.ToArray());
        line.Simplify(0.1f);
    }    
}

//các code trên dùng trong unity 6
//linearVelocity = Velocity
//code cũ , đường dự doán rất ngắn

//if (Input.GetMouseButton(0))
//{
//    Vector2 v = (DragStartPos - DragEndPos).normalized * maxShoot;
//    Vector2[] trajectory = Plot(rb, (Vector2)transform.position, v, 20);

//    line.positionCount = trajectory.Length;

//    Vector3[] positions = new Vector3[trajectory.Length];
//    for (int i = 0; i < trajectory.Length; i++)
//    {
//        positions[i] = trajectory[i];
//    }

//    line.SetPositions(positions);
//}


//public Vector2[] Plot(Rigidbody2D rigidbody2D, Vector2 position, Vector2 v, int steps)
//{
//    Vector2[] results = new Vector2[steps];

//    float timestep = Time.fixedDeltaTime / Physics2D.velocityIterations;
//    Vector2 gravity = Physics2D.gravity * rigidbody2D.gravityScale * timestep * timestep;

//    float drag = 1f - timestep * rigidbody2D.linearDamping;
//    Vector2 moveStep = v * timestep;

//    for (int i = 0; i < steps; i++) 
//    {
//        moveStep += gravity;
//        moveStep *= drag;
//        position += moveStep;
//        results[i] = position;
//    }

//    return results;
//}