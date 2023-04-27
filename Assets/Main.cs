using System;
using System.Collections;
using System.Collections.Generic;
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

    private Vector3 currentTransform;
    private Vector3 targetTransform;

    private Vector3 currentScale;
    private Vector3 targetScale;

    private Quaternion currentRotation;
    private Quaternion targetRotation;
    
    [SerializeField, HideInInspector]internal Matrix4x4 A;
    [SerializeField, HideInInspector]internal Matrix4x4 B;

    private void OnEnable()
    {
        vectors = GetComponent<VectorRenderer>();
    }
    
    void Update()
    {

        targetTransform = new Vector3(B.m03, B.m13, B.m23);
        currentTransform = (1.0f - Time) * transform.position + Time * targetTransform;
        
        
        using (vectors.Begin())
        {
            DrawCube(currentTransform, vectors);
        }
    }
    
    private void DrawCube(Vector3 position, VectorRenderer vectors)
    {
        //Parallel to X-axis arrows
        vectors.Draw(position, new Vector3(position.x + cubeSize, position.y, position.z),Color.red);
        vectors.Draw(position+new Vector3(0,cubeSize,0), new Vector3(position.x + cubeSize, position.y + cubeSize, position.z),Color.red);
        vectors.Draw(position+new Vector3(0,0,cubeSize), new Vector3(position.x + cubeSize, position.y, position.z + cubeSize),Color.red);
        vectors.Draw(position+new Vector3(0,cubeSize,cubeSize), new Vector3(position.x + cubeSize, position.y + cubeSize, position.z + cubeSize),Color.red);
        
        //Parallel to Y-axis arrows
        vectors.Draw(position, new Vector3(position.x, position.y + cubeSize, position.z),Color.green);
        vectors.Draw(position+new Vector3(cubeSize,0,0), new Vector3(position.x + cubeSize, position.y + cubeSize, position.z),Color.green);
        vectors.Draw(position+new Vector3(0,0,cubeSize), new Vector3(position.x, position.y + cubeSize, position.z + cubeSize),Color.green);
        vectors.Draw(position+new Vector3(cubeSize,0,cubeSize), new Vector3(position.x + cubeSize, position.y + cubeSize, position.z + cubeSize),Color.green);

        //Parallel to Z-axis arrows
        vectors.Draw(position, new Vector3(position.x, position.y, position.z + cubeSize),Color.blue);
        vectors.Draw(position+new Vector3(cubeSize,0,0), new Vector3(position.x + cubeSize, position.y, position.z + cubeSize),Color.blue);
        vectors.Draw(position+new Vector3(0,cubeSize,0), new Vector3(position.x, position.y + cubeSize, position.z + cubeSize),Color.blue);
        vectors.Draw(position+new Vector3(cubeSize,cubeSize,0), new Vector3(position.x + cubeSize, position.y + cubeSize, position.z + cubeSize),Color.blue);
    }
}

[CustomEditor(typeof(Main))]
public class MainEditor : Editor
{
    private void OnSceneGUI()
    {
        var main = target as Main;
        if (!main) return;
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
    }
}