using TMPro;
using UnityEngine;

public class TheStack : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public Color32[] gameColors = new Color32[4];
    public Material stackMat;
    public GameObject endPanel;

    private const float BOUNDS_SIZE = 3.5f;
    private const float STACK_MOVING_SPEED = 5.0f;
    private const float ERROR_MARGIN = 0.1f;
    private const float STACK_BOUNDS_GAIN = 0.25f;
    private const int COMBO_START_GAIN = 5;

    private GameObject[] theStack;
    private Vector2 stackBounds = new Vector2(BOUNDS_SIZE, BOUNDS_SIZE);

    private int stackIndex;
    private int scoreCount = 0;
    private int combo = 0;

    private float tileTransition = 0.0f;
    private float tileSpeed = 2.0f;
    private float secondaryTilePosition;

    private Vector3 desirePosition;
    private Vector3 lastTilePosition;

    private bool isMovingOnX = true;
    private bool ganeOver = false;
    public bool gameStart = false;

    public AudioSource audioSource;
    public AudioClip stack;
    void Start()
    {
        theStack = new GameObject[transform.childCount];
        for (int i =0; i < transform.childCount; i++)
        {
            theStack[i] = transform.GetChild(i).gameObject;
            ColorMesh(theStack[i]);
        }
        stackIndex = transform.childCount - 1;
    }

    private void CreateRubble(Vector3 pos, Vector3 scale)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.localPosition = pos;
        go.transform.localScale = scale;
        go.AddComponent<Rigidbody>();

        

        MeshRenderer rend = go.GetComponent<MeshRenderer>();
        if (rend != null)
        {
            if (stackMat != null)
                rend.material = new Material(stackMat);
            else
                rend.material = new Material(Shader.Find("Shader Graphs/LitStack"));
        }

        ColorMesh(go);
    }


    // Update is called once per frame
    void Update()
    {
        if(ganeOver)
            return;

        if (Input.GetMouseButtonDown(0) && gameStart == true)
        {
            if (PlaceTile())
            {
                audioSource.PlayOneShot(stack);
                SpawnTile();
                scoreCount++;
                scoreText.text = scoreCount.ToString();
            }
            else
            {
                EndGame();
            }
        }

        MoveTile();
        transform.position = Vector3.Lerp(transform.position, desirePosition, Time.deltaTime * STACK_MOVING_SPEED);
    }

    private void MoveTile()
    {
        tileTransition += Time.deltaTime * tileSpeed;
        if (isMovingOnX)
            theStack[stackIndex].transform.localPosition = new Vector3(Mathf.Sin(tileTransition) * BOUNDS_SIZE, scoreCount, secondaryTilePosition);
        else
            theStack[stackIndex].transform.localPosition = new Vector3(secondaryTilePosition, scoreCount, Mathf.Sin(tileTransition) * BOUNDS_SIZE);
    }

    private void SpawnTile()
    {
        lastTilePosition = theStack[stackIndex].transform.localPosition;
        stackIndex--;
        if(stackIndex < 0)
        {
            stackIndex = transform.childCount - 1;
        }
        desirePosition = (Vector3.down) * scoreCount;
        theStack[stackIndex].transform.localPosition = new Vector3(0, scoreCount, 0);
        theStack[stackIndex].transform.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);

        ColorMesh(theStack[stackIndex]);
    }

    private void ColorMesh(GameObject obj)
    {
        // Lấy MeshRenderer và MeshFilter
        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
        MeshFilter filter = obj.GetComponent<MeshFilter>();

        // Clone mesh để tránh ghi đè màu lên các tile khác
        Mesh mesh = Instantiate(filter.sharedMesh);
        filter.mesh = mesh;

        // Tạo mảng màu mới theo vertex
        Vector3[] vertices = mesh.vertices;
        Color32[] colors = new Color32[vertices.Length];
        float f = Mathf.Sin(scoreCount * 0.25f);
        Color c = Lerp4(gameColors[0], gameColors[1], gameColors[2], gameColors[3], f);

        for (int i = 0; i < vertices.Length; i++)
            colors[i] = c;

        mesh.colors32 = colors;

        // Gán shader nếu chưa đúng
        if (renderer.sharedMaterial == null || renderer.sharedMaterial.shader.name != "Shader Graphs/LitStack")
        {
            renderer.sharedMaterial = new Material(Shader.Find("Shader Graphs/LitStack"));
        }

        // Gán lại màu Tint (nếu Shader Graph có TintColor)
        renderer.sharedMaterial.SetColor("_TintColor", c);
    }



    private bool PlaceTile()
    {
        Transform t = theStack[stackIndex].transform;

        if (isMovingOnX)
        {
            float deltaX = lastTilePosition.x - t.localPosition.x;
            if(Mathf.Abs(deltaX) > ERROR_MARGIN)
            {
                // Cut the tile
                combo = 0;
                stackBounds.x -= Mathf.Abs(deltaX);
                if (stackBounds.x <= 0)
                    return false;

                float middle = lastTilePosition.x + t.localPosition.x / 2;
                t.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);
                CreateRubble
                (
                    new Vector3((t.position.x > 0)
                    ? t.position.x + (t.localScale.x /2)
                    : t.position.x - (t.localScale.x /2)
                    , t.position.y
                    , t.position.z),
                    new Vector3(Mathf.Abs(deltaX), 1, t.localScale.z)
                );
                t.localPosition = new Vector3(middle - (lastTilePosition.x / 2), scoreCount, lastTilePosition.z);
            }
            else
            {
                if(combo > COMBO_START_GAIN)
                {
                    stackBounds.x += STACK_BOUNDS_GAIN;
                    if (stackBounds.x > BOUNDS_SIZE)
                            stackBounds.x = BOUNDS_SIZE;

                    float middle = lastTilePosition.x + t.localPosition.x / 2;
                    t.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);
                    t.localPosition = new Vector3(middle - (lastTilePosition.x / 2), scoreCount, lastTilePosition.z);
                }
                combo++;
                t.localPosition = new Vector3(lastTilePosition.x, scoreCount, lastTilePosition.z);
            }
        }
        else
        {
            float deltaZ = lastTilePosition.z - t.localPosition.z;
            if (Mathf.Abs(deltaZ) > ERROR_MARGIN)
            {
                // Cut the tile
                combo = 0;
                stackBounds.y -= Mathf.Abs(deltaZ);
                if (stackBounds.y <= 0)
                    return false;

                float middle = lastTilePosition.z + t.localPosition.z / 2;
                t.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);
                
                CreateRubble
                (
                    new Vector3 (t.position.x
                    , t.position.y
                    , (t.position.z > 0)
                    ? t.position.z + (t.localScale.z / 2)
                    : t.position.z - (t.localScale.z / 2)),
                    new Vector3(t.localScale.x, 1, Mathf.Abs(deltaZ))
                );

                    t.localPosition = new Vector3(lastTilePosition.x, scoreCount, middle - (lastTilePosition.z/2));
                }
            else
            {
                if (combo > COMBO_START_GAIN)
                {
                    stackBounds.y += STACK_BOUNDS_GAIN;
                    if (stackBounds.y > BOUNDS_SIZE)
                        stackBounds.y = BOUNDS_SIZE;

                    float middle = lastTilePosition.z + t.localPosition.z / 2;
                    t.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);
                    t.localPosition = new Vector3(lastTilePosition.x, scoreCount, middle - (lastTilePosition.z/2));
                }
                combo++;
                t.localPosition = new Vector3(lastTilePosition.x, scoreCount, lastTilePosition.z);
            }
    }

        secondaryTilePosition = (isMovingOnX) ? t.localPosition.x : t.localPosition.z;

        isMovingOnX = !isMovingOnX;
        return true;
    }

    private Color32 Lerp4(Color32 a, Color32 b, Color32 c, Color32 d, float t)
    {
        if (t < 0.33f)
        {
            return Color.Lerp(a, b, t / 0.33f);
        }
        else if (t < 0.66f)
        {
            return Color.Lerp(b, c, (t - 0.33f) / 0.33f);
        }
        else
        {
            return Color.Lerp(c, d, (t - 0.66f) / 0.34f);
        }
    }

    public GameObject bestScore;
    private void EndGame()
    {
        if(PlayerPrefs.GetInt("score") < scoreCount)
        {
            PlayerPrefs.SetInt("score", scoreCount);
        }

        ganeOver = true;
        gameStart = false;
        endPanel.SetActive(true);
        bestScore.SetActive(true);
        theStack[stackIndex].AddComponent<Rigidbody>();
    }

    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
