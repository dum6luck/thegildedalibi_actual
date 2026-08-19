using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CircuitGridManager : MonoBehaviour
{
    // Global static flag accessible across scene loads
    public static bool BreakerPuzzleSolved = false;

    [Header("Scene References")]
    public RectTransform gridContainer;
    public TMP_Text statusText;

    [Header("Grid Settings")]
    public float CellSize = 100f;
    public float spacing = 8f;

    [Header("On Complete")]
    [Tooltip("Name of the scene to load once the circuit is solved (e.g. Main_Game). Must be added to Build Settings.")]
    public string sceneToLoadOnComplete = "Main_Game";
    [Tooltip("Seconds to wait after the win message appears before loading the next scene.")]
    public float delayBeforeLoad = 2f;

    [Header("Colors")]
    public Color tileBackgroundColor = new Color(0.16f, 0.16f, 0.20f);
    public Color unpoweredColor = new Color(0.40f, 0.40f, 0.46f);
    public Color poweredColor = new Color(1.00f, 0.82f, 0.20f);
    public Color sourceColor = new Color(0.20f, 0.60f, 0.30f);
    public Color targetColor = new Color(0.20f, 0.35f, 0.60f);
    public Color targetPoweredColor = new Color(0.30f, 0.80f, 1.00f);
    public Color selectionColor = Color.white;

    private const int Cols = 5;
    private const int Rows = 5;

    private CircuitTile[,] tiles;
    private CircuitTile selected;
    private bool solved;

    private void Start()
    {
        BuildLevel();
    }

    private void Update()
    {
        if (solved || selected == null) return;

        // WASD rotates the selected tile
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.D))
            selected.Rotate(1);
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.A))
            selected.Rotate(-1);

        // Arrow keys move selection to the next/previous tile
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.UpArrow))
            CycleSelection(1);
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            CycleSelection(-1);

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            bool backward = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            CycleSelection(backward ? -1 : 1);
        }
    }

    private void BuildLevel()
    {
        var layout = new TileType[Cols, Rows];
        for (int c = 0; c < Cols; c++)
            for (int r = 0; r < Rows; r++)
                layout[c, r] = TileType.Empty;

        layout[0, 2] = TileType.Source;
        layout[1, 2] = TileType.Straight;
        layout[2, 2] = TileType.Corner;
        layout[2, 1] = TileType.Corner;
        layout[3, 1] = TileType.Corner;
        layout[3, 2] = TileType.Corner;
        layout[4, 2] = TileType.Target;

        tiles = new CircuitTile[Cols, Rows];

        float totalW = Cols * CellSize + (Cols - 1) * spacing;
        float totalH = Rows * CellSize + (Rows - 1) * spacing;

        for (int c = 0; c < Cols; c++)
        {
            for (int r = 0; r < Rows; r++)
            {
                TileType type = layout[c, r];

                var go = new GameObject($"Tile_{c}_{r}", typeof(RectTransform));
                go.transform.SetParent(gridContainer, false);
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                float x = -totalW / 2f + CellSize / 2f + c * (CellSize + spacing);
                float y = totalH / 2f - CellSize / 2f - r * (CellSize + spacing);
                rt.anchoredPosition = new Vector2(x, y);

                var tile = go.AddComponent<CircuitTile>();

                int startRotation = 0;
                if (type == TileType.Straight || type == TileType.Corner)
                    startRotation = Random.Range(0, 4);

                tile.Init(this, type, c, r, startRotation);
                tiles[c, r] = tile;
            }
        }

        selected = null;
        solved = false;
        SelectFirstRotatable();
        SetStatus("Use WASD to rotate a piece, and Arrow Keys to move between tiles!");
        EvaluateFlow();
    }

    private void SelectFirstRotatable()
    {
        foreach (var t in tiles)
        {
            if (t != null && t.IsRotatable)
            {
                SelectTile(t);
                return;
            }
        }
    }

    public void SelectTile(CircuitTile tile)
    {
        if (selected != null) selected.SetSelected(false);
        selected = tile;
        if (selected != null) selected.SetSelected(true);
    }

    private void CycleSelection(int dir)
    {
        var rotatables = new List<CircuitTile>();
        foreach (var t in tiles) if (t != null && t.IsRotatable) rotatables.Add(t);
        if (rotatables.Count == 0) return;

        int idx = rotatables.IndexOf(selected);
        idx = ((idx + dir) % rotatables.Count + rotatables.Count) % rotatables.Count;
        SelectTile(rotatables[idx]);
    }

    public void OnTileRotated()
    {
        EvaluateFlow();
    }

    private void EvaluateFlow()
    {
        foreach (var t in tiles) if (t != null) t.SetPowered(false);

        CircuitTile source = null, target = null;
        foreach (var t in tiles)
        {
            if (t == null) continue;
            if (t.Type == TileType.Source) source = t;
            if (t.Type == TileType.Target) target = t;
        }
        if (source == null) return;

        var visited = new bool[Cols, Rows];
        var queue = new Queue<CircuitTile>();
        queue.Enqueue(source);
        visited[source.Col, source.Row] = true;

        while (queue.Count > 0)
        {
            var cur = queue.Dequeue();
            cur.SetPowered(true);

            if (cur.ConnectsNorth && cur.Row > 0)
                TryEnqueue(tiles[cur.Col, cur.Row - 1], n => n.ConnectsSouth, visited, queue);

            if (cur.ConnectsSouth && cur.Row < Rows - 1)
                TryEnqueue(tiles[cur.Col, cur.Row + 1], n => n.ConnectsNorth, visited, queue);

            if (cur.ConnectsEast && cur.Col < Cols - 1)
                TryEnqueue(tiles[cur.Col + 1, cur.Row], n => n.ConnectsWest, visited, queue);

            if (cur.ConnectsWest && cur.Col > 0)
                TryEnqueue(tiles[cur.Col - 1, cur.Row], n => n.ConnectsEast, visited, queue);
        }

        bool poweredTarget = target != null && visited[target.Col, target.Row];
        if (poweredTarget)
        {
            solved = true;
            SetStatus("Circuit complete! Energy is flowing.");

            // Set the static solve flag
            BreakerPuzzleSolved = true;

            // Unlock and show the cursor before returning to the main scene
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Trigger scene load via BreakerSceneLoader if present, or fallback to Coroutine
            if (BreakerSceneLoader.Instance != null)
            {
                BreakerSceneLoader.Instance.ReturnToMainGame(delayBeforeLoad);
            }
            else if (!string.IsNullOrEmpty(sceneToLoadOnComplete))
            {
                StartCoroutine(LoadSceneAfterDelay());
            }
        }
        else
        {
            SetStatus("Use WASD to rotate a piece, and Arrow Keys to move between tiles!");
        }
    }

    private IEnumerator LoadSceneAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeLoad);
        SceneManager.LoadScene(sceneToLoadOnComplete);
    }

    private void TryEnqueue(CircuitTile neighbor, System.Func<CircuitTile, bool> hasMatchingConnector,
        bool[,] visited, Queue<CircuitTile> queue)
    {
        if (neighbor == null || neighbor.Type == TileType.Empty) return;
        if (visited[neighbor.Col, neighbor.Row]) return;
        if (!hasMatchingConnector(neighbor)) return;

        visited[neighbor.Col, neighbor.Row] = true;
        queue.Enqueue(neighbor);
    }

    private void SetStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
    }
}