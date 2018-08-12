using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Pos
{
    public Pos(int x, int y)
    {
        X = x;
        Y = y;
    }
    public int X, Y;

    public static Pos operator +(Pos a, Pos b)
    {
        return new Pos(a.X + b.X, a.Y + b.Y);
    }

    public static Pos operator -(Pos a, Pos b)
    {
        return new Pos(a.X - b.X, a.Y - b.Y);
    }

    public static Pos operator *(Pos a, int v)
    {
        return new Pos(a.X * v, a.Y * v);
    }

    public Pos Copy()
    {
        return new Pos(X, Y);
    }
}

public class Container : MonoBehaviour
{
    public static Container Instance;
    public ContainerSquare[,] Squares;
    private static readonly Prefab SquarePrefab = new Prefab("ContainerSquare");
    public int Size = 3, AtX = 1, Rotation = 0;
    private const float AnimationTime = 0.1f;
    public GameObject Triangle;
    public Shape CurShape;
    private Transform _rotationPoint;
    public List<Shape> Shapes = new List<Shape>();
    public Text LoseText, WinText, SizeText;
    public GameObject MenuElements;

    private void Awake()
    {
        Instance = this;
    }

    public void StartLevel()
    {
        MenuElements.SetActive(false);
        Triangle.SetActive(true);
        Squares = new ContainerSquare[Size, Size];
        for (var x = 0; x < Size; x++)
        {
            for (var y = 0; y < Size; y++)
            {
                var go = SquarePrefab.Instantiate();
                go.transform.SetParent(transform);
                go.transform.position = new Vector3(x, y); 
                Squares[x, y] = go.GetComponent<ContainerSquare>();
                go.GetComponent<SpriteRenderer>().color = ContainerSquare.Gray;
            }
        }
        _rotationPoint = Squares[Size / 2, Size / 2].transform;
        var v = Squares[Size / 2, Size - 1].transform.position + Vector3.up;
        AtX = Size / 2;
        Triangle.transform.position = v;
        Camera.main.transform.position = new Vector3(v.x, v.y - 1 - Size / 3, Camera.main.transform.position.z);
        Shape.InitShapes(Size * Size);
        CurShape = Shape.GetNextShape();
        Shape.SortShapes();
        DisplayShadow();
        Camera.main.orthographicSize = Math.Max(Size, 5);
    }

    public Pos At()
    {
        switch (Rotation)
        {
            case 0:
                return new Pos(AtX, Size - 1);
            case 1:
                return new Pos(Size - 1, Size - 1 - AtX);
            case 2:
                return new Pos(Size - 1 - AtX, 0);
            case 3:
                return new Pos(0, AtX);
        }
        return new Pos(-1, -1); 
    }

    public Pos Dir()
    {
        switch (Rotation)
        {
            case 0:
                return new Pos(0, -1);
            case 1:
                return new Pos(-1, 0);
            case 2:
                return new Pos(0, 1);
            case 3:
                return new Pos(1, 0);
        }
        return new Pos(-1, -1);
    }

    public Pos Right()
    {
        switch (Rotation)
        {
            case 0:
                return new Pos(1, 0);
            case 1:
                return new Pos(0, -1);
            case 2:
                return new Pos(-1, 0);
            case 3:
                return new Pos(0, 1);
        }
        return new Pos(-1, -1);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            Rotate(1);
        } else if (Input.GetKeyDown(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            Rotate(-1);
        } else if (Input.GetKeyDown(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            if (CurShape == null) return;
            if (CurShape.InitMove())
            {
                CurShape = Shape.GetNextShape();
                if (CurShape == null)
                {
                    Utils.Animate(Color.white, Color.black, 2f, (v) =>
                    {
                        Camera.main.backgroundColor = v;
                    }, null, true);
                    WinText.gameObject.SetActive(true);
                    return;
                }
                while (Size - 1 - CurShape.Length + 1 < AtX)
                {
                    Rotate(-1, true); 
                }
                DisplayShadow();
            }
            else
            {
                LoseText.gameObject.SetActive(true);
                Utils.Animate(new Color(0.52f, 0f, 0.01f), Color.black, 2f, (v) =>
                {
                    Camera.main.backgroundColor = v;
                }, null, true);
                CurShape = null;
            }
        } else if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(0);
        }
    }

    private void Rotate(int dir, bool noDisplay = false)
    {
        float xChange;
        var maxX = Size - 1;
        if (CurShape != null)
        {
            maxX = Size - 1 - CurShape.Length + 1;
        }
        if (dir < 0)
        {
            if (AtX > 0)
            {
                AtX--;
                xChange = 1;
            }
            else
            {
                AtX = maxX;
                xChange = -maxX;
                Rotation = (Rotation + 3) % 4;
                Utils.Animate(0f, -90f, AnimationTime, (v) =>
                {
                    transform.RotateAround(_rotationPoint.position, Vector3.forward, v); 
                }, null, false, 0f, InterpolationType.InvSquare);
            }
        }
        else
        {
            if (AtX < maxX)
            {
                AtX++;
                xChange = -1;
            }
            else
            {
                AtX = 0;
                xChange = maxX;
                Rotation = (Rotation + 1) % 4;
                Utils.Animate(0f, 90f, AnimationTime, (v) =>
                {
                    transform.RotateAround(_rotationPoint.position, Vector3.forward, v);
                }, null, false, 0f, InterpolationType.InvSquare);
            }
        }
        Utils.Animate(0, xChange, AnimationTime, (v) =>
        {
            transform.position += Vector3.right * v;
        }, null, false, 0f, InterpolationType.InvSquare);
        if (noDisplay) return;
        DisplayShadow();
    }

    public void IncreaseSize()
    {
        Debug.Log("pressed");
        if (Size < 11)
        {
            Size += 2;
        }
        SizeText.text = "Size: " + Size;
    }

    public void DecreaseSize()
    {
        Debug.Log("pressed");
        if (Size > 3)
        {
            Size -= 2;
        }
        SizeText.text = "Size: " + Size;
    }

    public void DisplayShadow()
    {
        if (CurShape == null) return;
        foreach (var square in Squares)
        {
            square.GetComponent<SpriteRenderer>().color = ContainerSquare.Gray;
        }
        foreach (var pos in CurShape.Positions)
        {
            var v = At() + Dir() * (CurShape.Height - pos.Y - 1) + Right() * pos.X;
            Squares[v.X, v.Y].GetComponent<SpriteRenderer>().color = Color.white;
        }
    }
}
