 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_StatsRadarChart : MonoBehaviour
{
    Stats stats;
    [SerializeField] Material radarMaterial;
    [SerializeField] Texture2D radarTexture2d;
    CanvasRenderer radarMeshCanvasRenderer;

    void Awake(){
        radarMeshCanvasRenderer = transform.Find("radarMesh").GetComponent<CanvasRenderer>();
    }

    public void zSetStats(Stats stats){
        this.stats = stats;
        stats.OnStatsChanged += yStats_OnStatsChanged;
        yUpdateStatsVisual();
    }

    void yStats_OnStatsChanged(object sender, System.EventArgs e){
        yUpdateStatsVisual();
    }

    void Update(){
        if (Input.GetKey(KeyCode.Alpha1)) stats.zIncreaseStatAmount(Stats.Type.Memory);
        if (Input.GetKey(KeyCode.Alpha2)) stats.zDecreaseStatAmount(Stats.Type.Memory);

        if (Input.GetKey(KeyCode.Alpha3)) stats.zIncreaseStatAmount(Stats.Type.Concentration);
        if (Input.GetKey(KeyCode.Alpha4)) stats.zDecreaseStatAmount(Stats.Type.Concentration);

        if (Input.GetKey(KeyCode.Alpha5)) stats.zIncreaseStatAmount(Stats.Type.Thought);
        if (Input.GetKey(KeyCode.Alpha6)) stats.zDecreaseStatAmount(Stats.Type.Thought);

        if (Input.GetKey(KeyCode.Alpha7)) stats.zIncreaseStatAmount(Stats.Type.Quickness);
        if (Input.GetKey(KeyCode.Alpha8)) stats.zDecreaseStatAmount(Stats.Type.Quickness);
    }

    void yUpdateStatsVisual(){
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[6];
        Vector2[] uv = new Vector2[6];
        int[] triangles = new int[3 * 5];

        float angleIncrement = 360f / 5;
        float radarChartSize = 135f;

        Vector3 memoryVertex = Quaternion.Euler(0, 0, angleIncrement * 0) * Vector3.up * radarChartSize * stats.zGetStatAmountNormalized(Stats.Type.Memory);
        int memoryVertexIndex = 1;
        Vector3 concentrationVertex = Quaternion.Euler(0, 0, -angleIncrement * 1) * Vector3.up * radarChartSize * stats.zGetStatAmountNormalized(Stats.Type.Concentration);
        int concentrationIndex = 2;
        Vector3 thoughtVertex = Quaternion.Euler(0, 0, -angleIncrement * 2) * Vector3.up * radarChartSize * stats.zGetStatAmountNormalized(Stats.Type.Thought);
        int thoughtIndex = 3;
        Vector3 quicknessVertex = Quaternion.Euler(0, 0, -angleIncrement * 3) * Vector3.up * radarChartSize * stats.zGetStatAmountNormalized(Stats.Type.Quickness);
        int quicknessIndex = 4;
        Vector3 problemSolvingVertex = Quaternion.Euler(0, 0, -angleIncrement * 4) * Vector3.up * radarChartSize * stats.zGetStatAmountNormalized(Stats.Type.ProblemSolving);
        int problemSolvingIndex = 5;

        vertices[0] = Vector3.zero;
        vertices[memoryVertexIndex] = memoryVertex;
        vertices[concentrationIndex] = concentrationVertex;
        vertices[thoughtIndex] = thoughtVertex;
        vertices[quicknessIndex] = quicknessVertex;
        vertices[problemSolvingIndex] = problemSolvingVertex;

        uv[0] = Vector2.zero;
        uv[memoryVertexIndex] = Vector2.one;
        uv[concentrationIndex] = Vector2.one;
        uv[thoughtIndex] = Vector2.one;
        uv[quicknessIndex] = Vector2.one;
        uv[problemSolvingIndex] = Vector2.one;

        triangles[0] = 0;
        triangles[1] = memoryVertexIndex;
        triangles[2] = concentrationIndex;

        triangles[3] = 0;
        triangles[4] = concentrationIndex;
        triangles[5] = thoughtIndex;

        triangles[6] = 0;
        triangles[7] = thoughtIndex;
        triangles[8] = quicknessIndex;

        triangles[9] = 0;
        triangles[10] = quicknessIndex;
        triangles[11] = problemSolvingIndex;

        triangles[12] = 0;
        triangles[13] = problemSolvingIndex;
        triangles[14] = memoryVertexIndex;

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        //radarMaterial.color = Color.green;

        radarMeshCanvasRenderer.SetMesh(mesh);
        radarMeshCanvasRenderer.SetMaterial(radarMaterial, radarTexture2d);
    }
}
