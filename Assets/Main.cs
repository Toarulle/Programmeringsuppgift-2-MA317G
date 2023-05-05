using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;
using UnityEngine.Subsystems;
using Vectors;


[ExecuteAlways]
[RequireComponent(typeof(VectorRenderer))]
public class Main : MonoBehaviour
{
    private VectorRenderer vectors;

    
    [Range(0,5f)]public float cubeSize = 1f;
    [Range(0,1f)]public float Time = 0.0f;

    public bool doATransforming;
    public bool doBTransforming;
    public bool doAScaling;
    public bool doBScaling;
    public bool doARotation;
    public bool doBRotation;
    
    public Vector3 aTargetTransform;
    public Vector3 bTargetTransform;
    public Vector3 cTransform;

    private Vector3 aTargetScale;
    private Vector3 bTargetScale;
    private Vector3 cScale;

    private Quaternion aTargetRotation;
    private Quaternion bTargetRotation;
    private Quaternion cRotation;

    private Vector3 baseXVector;
    private Vector3 baseYVector;
    private Vector3 baseZVector;
    
    
    [SerializeField, HideInInspector]internal Matrix4x4 A;
    [SerializeField, HideInInspector]internal Matrix4x4 B;
    
    [SerializeField, HideInInspector]internal Matrix4x4 C;

    private Vector3[] columnVectorAarray;
    private Vector3[] columnVectorBarray;

    private void OnEnable()
    {
        vectors = GetComponent<VectorRenderer>();
        baseXVector = new Vector3(1, 1, 1);
        baseYVector = new Vector3(0, 1, 0);
        baseZVector = new Vector3(0, 0, 1);
    }
    
    void Update()
    {
        columnVectorAarray = new Vector3[3];
        columnVectorAarray[0] = new Vector3(A[0, 0], A[1, 0], A[2, 0]);
        columnVectorAarray[1] = new Vector3(A[0, 1], A[1, 1], A[2, 1]);
        columnVectorAarray[2] = new Vector3(A[0, 2], A[1, 2], A[2, 2]);
        
        columnVectorBarray = new Vector3[3];
        columnVectorBarray[0] = new Vector3(B[0, 0], B[1, 0], B[2, 0]);
        columnVectorBarray[1] = new Vector3(B[0, 1], B[1, 1], B[2, 1]);
        columnVectorBarray[2] = new Vector3(B[0, 2], B[1, 2], B[2, 2]);
        
        aTargetTransform = GetTransformFromMatrix(A);
        bTargetTransform = GetTransformFromMatrix(B);

        Vector3 tempXA;
        tempXA.x = Vector3.Dot(columnVectorAarray[0], baseXVector);
        tempXA.y = Vector3.Dot(columnVectorAarray[1], baseXVector);
        tempXA.z = Vector3.Dot(columnVectorAarray[2], baseXVector);

        //Debug.Log(tempXA + " - " + new Vector3(A[2,0],A[2,1],A[2,2]) + " - " + baseXVector);
        Vector3 magnA;
        magnA.x = Vector3.Magnitude(columnVectorAarray[0]); 
        magnA.y = Vector3.Magnitude(columnVectorAarray[1]); 
        magnA.z = Vector3.Magnitude(columnVectorAarray[2]);

        Vector3 magnB;
        magnB.x = Vector3.Magnitude(columnVectorBarray[0]); 
        magnB.y = Vector3.Magnitude(columnVectorBarray[1]); 
        magnB.z = Vector3.Magnitude(columnVectorBarray[2]);
        
        
        ////lerp-transform the vectors and insert into Matrix C////
        SetTransformInMatrix(ref C, MyVectorLerp(aTargetTransform, bTargetTransform, Time));

        cScale = MyVectorLerp(magnA, magnB, Time);
        
        Debug.Log(cScale);
        
        cTransform = GetTransformFromMatrix(C);
        
        using (vectors.Begin())
        {
            vectors.Draw(Vector3.zero, tempXA, Color.magenta);
            
            /*vectors.Draw(Vector3.zero, new Vector3(A[0,0],A[0,1],A[0,2]),Color.blue);
            vectors.Draw(Vector3.zero, new Vector3(A[1,0],A[1,1],A[1,2]),Color.blue);
            vectors.Draw(Vector3.zero, new Vector3(A[2,0],A[2,1],A[2,2]),Color.blue);
            
            vectors.Draw(Vector3.zero, new Vector3(A[0,0],A[1,0],A[2,0]),Color.magenta);
            vectors.Draw(Vector3.zero, new Vector3(A[0,1],A[1,1],A[2,1]),Color.magenta);
            vectors.Draw(Vector3.zero, new Vector3(A[0,2],A[1,2],A[2,2]),Color.magenta);*/
            DrawCube(cTransform, cScale, vectors);
        }
    }

    public void SetMagnitudeInMatrix(ref Matrix4x4 x, Vector3 m)
    {
        
    }
    public void SetTransformInMatrix(ref Matrix4x4 x, Vector3 v)
    {
        Matrix4x4 temp = x;
        temp[0,3] = v.x;
        //Debug.Log(C.m03 + " - " + x.m03 + " - " + v.x);
        temp[1,3] = v.y;
        temp[2,3] = v.z;
        x = temp;
    }
    public Vector3 GetTransformFromMatrix(Matrix4x4 m)
    {
        return new Vector3(m.m03, m.m13, m.m23);
    }
    
    //Egenbyggd Lerpfunktion för vektorer
    private static Vector3 MyVectorLerp(Vector3 a, Vector3 b, float t)
    {
        Vector3 c;
        c = (1.0f - t) * a + t * b;
        return c;
    }
    
    private void DrawCube(Vector3 position, Vector3 size, VectorRenderer vectors)
    {
        //Parallel to X-axis arrows
        vectors.Draw(position, 
            new Vector3(position.x + cubeSize * size.x, position.y, position.z),Color.red);
        vectors.Draw(position+new Vector3(0,cubeSize * size.y,0), 
            new Vector3(position.x + cubeSize * size.x, position.y + cubeSize * size.y, position.z),Color.red);
        vectors.Draw(position+new Vector3(0,0,cubeSize * size.z), 
            new Vector3(position.x + cubeSize * size.x, position.y, position.z + cubeSize * size.z),Color.red);
        vectors.Draw(position+new Vector3(0,cubeSize * size.y,cubeSize * size.z), 
            new Vector3(position.x + cubeSize * size.x, position.y + cubeSize * size.y, position.z + cubeSize * size.z),Color.red);
        
        //Parallel to Y-axis arrows
        vectors.Draw(position, 
            new Vector3(position.x, position.y + cubeSize * size.y, position.z),Color.green);
        vectors.Draw(position+new Vector3(cubeSize * size.x,0,0), 
            new Vector3(position.x + cubeSize * size.x, position.y + cubeSize * size.y, position.z),Color.green);
        vectors.Draw(position+new Vector3(0,0,cubeSize * size.z), 
            new Vector3(position.x, position.y + cubeSize * size.y, position.z + cubeSize * size.z),Color.green);
        vectors.Draw(position+new Vector3(cubeSize * size.x,0,cubeSize * size.z), 
            new Vector3(position.x + cubeSize * size.x, position.y + cubeSize * size.y, position.z + cubeSize * size.z),Color.green);

        //Parallel to Z-axis arrows
        vectors.Draw(position, 
            new Vector3(position.x, position.y, position.z + cubeSize * size.z),Color.blue);
        vectors.Draw(position+new Vector3(cubeSize * size.x,0,0), 
            new Vector3(position.x + cubeSize * size.x, position.y, position.z + cubeSize * size.z),Color.blue);
        vectors.Draw(position+new Vector3(0,cubeSize * size.y,0), 
            new Vector3(position.x, position.y + cubeSize * size.y, position.z + cubeSize * size.z),Color.blue);
        vectors.Draw(position+new Vector3(cubeSize * size.x,cubeSize * size.y,0), 
            new Vector3(position.x + cubeSize * size.x, position.y + cubeSize * size.y, position.z + cubeSize * size.z),Color.blue);
    }
}

[CustomEditor(typeof(Main))]
public class MainEditor : Editor
{
    private void OnSceneGUI()
    {
        var main = target as Main;
        if (!main) return;
        
        EditorGUI.BeginChangeCheck();

        Vector3 bTransform = main.GetTransformFromMatrix(main.B); 
        
        var newBTarget = Handles.PositionHandle(bTransform, main.transform.rotation);
        
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(main, "Moved Target");
            var copy = main.B;
            copy.m03 = newBTarget.x;
            copy.m13 = newBTarget.y;
            copy.m23 = newBTarget.z;
            main.B = copy;
            EditorUtility.SetDirty(main);
        }
        
        
        
        EditorGUI.BeginChangeCheck();
        
        Vector3 aTransform = main.GetTransformFromMatrix(main.A);

        var newATarget = Handles.PositionHandle(aTransform, main.transform.rotation);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(main, "Moved Target");
            var copy = main.A;
            copy.m03 = newATarget.x;
            copy.m13 = newATarget.y;
            copy.m23 = newATarget.z;
            main.A = copy;
            EditorUtility.SetDirty(main);
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var main = target as Main;
        if (!main) return;
        
        EditorGUI.BeginChangeCheck();
        
        /////////////////////////////////
        //Produce A Matrix in Inspector//
        /////////////////////////////////
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Matrix A");
        EditorGUILayout.BeginVertical();

        var resultA = Matrix4x4.identity;
        for (int i = 0; i < 4; i++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int j = 0; j < 4; j++)
            {
                resultA[i, j] = EditorGUILayout.FloatField(main.A[i,j]);
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(main, "Change Matrix A");
            main.A = resultA;
            EditorUtility.SetDirty(main);
        }
        
        EditorGUILayout.Space(10);
        EditorGUI.BeginChangeCheck();
        /////////////////////////////////
        //Produce B Matrix in Inspector//
        /////////////////////////////////
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Matrix B");
        EditorGUILayout.BeginVertical();

        var resultB = Matrix4x4.identity;
        for (int i = 0; i < 4; i++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int j = 0; j < 4; j++)
            {
                resultB[i, j] = EditorGUILayout.FloatField(main.B[i,j]);
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(main, "Change Matrix B");
            main.B = resultB;
            EditorUtility.SetDirty(main);
        }        
        
        EditorGUILayout.Space(10);
        EditorGUI.BeginChangeCheck();
        /////////////////////////////////
        //Produce C Matrix in Inspector//
        /////////////////////////////////
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Matrix C (result)");
        EditorGUILayout.BeginVertical();

        var resultC = Matrix4x4.identity;
        for (int i = 0; i < 4; i++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int j = 0; j < 4; j++)
            {
                resultC[i, j] = EditorGUILayout.FloatField(main.C[i,j]);
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(main, "Change Matrix C");
            main.C = resultC;
            EditorUtility.SetDirty(main);
        }
        
        
    }
}