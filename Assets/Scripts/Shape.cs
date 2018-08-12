using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Shape : MonoBehaviour
{
    private const float AnimationWindow = 0.1f;
    public List<Pos> Positions;
    private static readonly Prefab ShapeSquarePrefab = new Prefab("ShapeSquare");
    private static readonly Prefab ShapePrefab = new Prefab("Shape");
    private List<GameObject> ShapeSquares = new List<GameObject>(10);
    public Pos ZeroPos;

    private static List<Shape> Shapes;

    public static void InitShapes(int cells)
    {
        Shapes = new List<Shape>();
        var shapes = new List<List<Pos>>();
        while (cells > 4)
        {
            var r = UnityEngine.Random.value;
            var count = 1;
            if (r < 0.3f)
            {
                count = 1;
            } else if (r < 0.65f)
            {
                count = 2;
            } else if (r < 0.85f)
            {
                count = 3;
            }
            else
            {
                count = 4;
            }
            shapes.Add(GenShape(count));
            cells -= count;
        }
        for (var i = 0; i < cells; i++)
        {
            shapes.Add(GenShape(1));
        }
        var orderedPoses = shapes.OrderBy(a => UnityEngine.Random.value);
        foreach (var poses in orderedPoses)
        {
            var s = Create(poses);
            Shapes.Add(s);
        }
    }

    private static List<Pos> GenShape(int cells)
    {
        var pos = new Pos(0, 0);
        var res = new List<Pos>{};
        for (var i = 0; i < cells; i++)
        {
            res.Add(pos);
            pos = pos.Copy();
            if (UnityEngine.Random.value > 0.5f)
            {
                pos.X++;
            }
            else
            {
                pos.Y++;
            }
        }
        return res;
    }

    public static Shape Create(List<Pos> poses)
    {
        var shape = ShapePrefab.Instantiate().GetComponent<Shape>();
        shape.Positions = poses;
        int maxY = 0, maxX = 0;
        foreach (var pos in shape.Positions)
        {
            maxY = Math.Max(pos.Y, maxY);
            maxX = Math.Max(pos.X, maxX);
        }
        shape.Height = maxY + 1;
        shape.Length = maxX + 1;
        var c = UnityEngine.Random.ColorHSV(0.5f, 1f, 0.5f, 1f, 0.5f, 1f);
        foreach (var position in shape.Positions)
        {
            var go = ShapeSquarePrefab.Instantiate();
            go.transform.SetParent(shape.transform); 
            go.transform.localPosition = new Vector3(position.X, position.Y);
            go.GetComponent<SpriteRenderer>().color = c;
            shape.ShapeSquares.Add(go);
        }
        shape.transform.localScale = Vector3.one * 0.25f;
        return shape;
    }

    public static void SortShapes()
    {
        var p = Container.Instance.Triangle.transform.position.y + 1;
        var i = 0;
        foreach (var shape in Shapes)
        {
            Utils.Animate(shape.transform.position, new Vector3(-4, p), AnimationWindow, (v) =>
            {
                shape.transform.position = v;
            }, shape, true);
            p -= 0.7f;
            if (i == 0)
            {
                p -= 1f;
            }
            i++;
        }
    }

    public static Shape GetNextShape()
    {
        if (Shapes.Count == 0) return null;
        var s = Shapes[0];
        Shapes.RemoveAt(0);
        s.transform.position = Container.Instance.Triangle.transform.position + Vector3.up;
        s.transform.localScale = Vector3.one;
        Container.Instance.Shapes.Add(s);
        SortShapes();
        if (Shapes.Count > 0) Shapes[0].transform.localScale = Vector3.one * 0.7f;
        return s;
    }

    public bool InitMove()
    {
        transform.position = Container.Instance.Triangle.transform.position;
        transform.SetParent(Container.Instance.transform);
        var r = Container.Instance.Right();
        var u = Container.Instance.Dir() * -1;
        foreach (var pos in Positions)
        {
            var v = Container.Instance.At() + u * (pos.Y + 1) + r * pos.X;
            pos.X = v.X;
            pos.Y = v.Y;
        }
        ZeroPos = Container.Instance.At() - Container.Instance.Dir();
        
        for (var i = 0; i < Height; i++)
        {
            if (!MoveDown(true))
            {
                UpdateAll();
                return false;
            }
        }
        UpdateAll();
        return true;
    }

    public static void UpdateAll()
    {
        var container = Container.Instance;
        foreach (var shape in container.Shapes)
        {
            if (GoodPos(shape.ZeroPos))
            {
                Utils.Animate(shape.transform.position,
                    container.Squares[shape.ZeroPos.X, shape.ZeroPos.Y].transform.position, AnimationWindow,
                    (v) =>
                    {
                        shape.transform.position += v;
                    }, null, false, 0f, InterpolationType.InvSquare);
            }
        }
    }

    public bool MoveDown(bool initing = false)
    {
        var dir = Container.Instance.Dir();
        var container = Container.Instance;
        foreach (var pos in Positions)
        {
            if (GoodPos(pos))
            {
                container.Squares[pos.X, pos.Y].Shape = null;
            }
        }
        foreach (var pos in Positions)
        {
            var newPos = pos + dir;
            if (!GoodPos(newPos))
            {
                continue; 
            }
            var s = container.Squares[newPos.X, newPos.Y].Shape;
            if (s != null && s != this)
            {
                if (!s.MoveDown())
                {
                    return false;
                }
            }
        }
        foreach (var pos in Positions)
        {
            var newPos = pos + dir;
            if (GoodPos(newPos))
            {
                container.Squares[newPos.X, newPos.Y].Shape = this;
            } else if (!initing)
            {
                return false;
            }
            pos.X = newPos.X;
            pos.Y = newPos.Y;
        }
        ZeroPos += container.Dir();
        return true;
    }
    
    private static bool GoodPos(Pos p)
    {
        var c = Container.Instance;
        return p.X >= 0 && p.Y >= 0 && p.X < c.Size && p.Y < c.Size;
    }

    public int Height, Length;
}