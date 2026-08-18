using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum TileType { Empty, Straight, Corner, Source, Target }

/// <summary>
/// A single grid cell. Draws its own pipe arms at runtime (no sprites needed),
/// tracks its rotation, and reports its current connector directions (N,E,S,W).
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class CircuitTile : MonoBehaviour, IPointerClickHandler
{
    public TileType Type { get; private set; }
    public int Col { get; private set; }
    public int Row { get; private set; }
    public int RotationSteps { get; private set; } // 0-3, each step = 90 deg clockwise
    public bool IsRotatable => Type == TileType.Straight || Type == TileType.Corner;

    // Base connection pattern at rotation 0. Index: 0=North,1=East,2=South,3=West
    private bool[] baseConnections;

    private Image background;
    private Image[] arms = new Image[4];
    private Image hub;
    private UnityEngine.UI.Outline outline;

    private CircuitGridManager manager;

    public void Init(CircuitGridManager mgr, TileType type, int col, int row, int startRotation)
    {
        manager = mgr;
        Type = type;
        Col = col;
        Row = row;
        RotationSteps = ((startRotation % 4) + 4) % 4;
        baseConnections = GetBasePattern(type);

        BuildVisuals();
        RefreshShape();
        SetPowered(false);
    }

    private bool[] GetBasePattern(TileType type)
    {
        switch (type)
        {
            case TileType.Straight: return new[] { true, false, true, false };  // N-S pipe
            case TileType.Corner: return new[] { true, true, false, false };    // N-E pipe
            case TileType.Source: return new[] { false, true, false, false };   // opens East
            case TileType.Target: return new[] { false, false, false, true };   // opens West
            default: return new[] { false, false, false, false };
        }
    }

    /// <summary>Current connector directions after rotation: [N,E,S,W]</summary>
    public bool[] GetConnections()
    {
        var result = new bool[4];
        for (int d = 0; d < 4; d++)
        {
            int src = ((d - RotationSteps) % 4 + 4) % 4;
            result[d] = baseConnections[src];
        }
        return result;
    }

    public bool ConnectsNorth => GetConnections()[0];
    public bool ConnectsEast => GetConnections()[1];
    public bool ConnectsSouth => GetConnections()[2];
    public bool ConnectsWest => GetConnections()[3];

    public void Rotate(int direction)
    {
        if (!IsRotatable) return;
        RotationSteps = ((RotationSteps + direction) % 4 + 4) % 4;
        RefreshShape();
        manager.OnTileRotated();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!IsRotatable) return;
        manager.SelectTile(this);
    }

    // ---------- Visual construction (fully procedural, no art needed) ----------

    private void BuildVisuals()
    {
        float cell = manager.CellSize;
        GetComponent<RectTransform>().sizeDelta = new Vector2(cell, cell);

        background = gameObject.AddComponent<Image>();
        background.raycastTarget = IsRotatable;
        background.color = manager.tileBackgroundColor;

        if (Type == TileType.Empty)
        {
            background.color = new Color(0f, 0f, 0f, 0f);
            return;
        }

        hub = CreateChild("Hub", new Vector2(cell * 0.28f, cell * 0.28f), Vector2.zero);

        float armLen = cell * 0.5f;
        float thick = cell * 0.22f;
        arms[0] = CreateChild("ArmN", new Vector2(thick, armLen), new Vector2(0, armLen * 0.5f));
        arms[1] = CreateChild("ArmE", new Vector2(armLen, thick), new Vector2(armLen * 0.5f, 0));
        arms[2] = CreateChild("ArmS", new Vector2(thick, armLen), new Vector2(0, -armLen * 0.5f));
        arms[3] = CreateChild("ArmW", new Vector2(armLen, thick), new Vector2(-armLen * 0.5f, 0));

        switch (Type)
        {
            case TileType.Source: background.color = manager.sourceColor; break;
            case TileType.Target: background.color = manager.targetColor; break;
        }
    }

    private Image CreateChild(string name, Vector2 size, Vector2 pos)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(transform, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        var img = go.GetComponent<Image>();
        img.raycastTarget = false;
        return img;
    }

    private void RefreshShape()
    {
        if (Type == TileType.Empty) return;
        bool[] conns = GetConnections();
        for (int d = 0; d < 4; d++)
            arms[d].gameObject.SetActive(conns[d]);
    }

    public void SetPowered(bool powered)
    {
        if (Type == TileType.Empty) return;

        Color c = powered ? manager.poweredColor : manager.unpoweredColor;
        foreach (var arm in arms) if (arm != null) arm.color = c;
        if (hub != null) hub.color = c;

        if (Type == TileType.Target)
            background.color = powered ? manager.targetPoweredColor : manager.targetColor;
    }

    public void SetSelected(bool selected)
    {
        if (outline == null) outline = gameObject.AddComponent<UnityEngine.UI.Outline>();
        outline.effectColor = manager.selectionColor;
        outline.effectDistance = new Vector2(4, -4);
        outline.enabled = selected;
    }
}