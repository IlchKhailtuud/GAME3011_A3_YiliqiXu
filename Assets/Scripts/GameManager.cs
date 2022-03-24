using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    
    #region
    public enum SweetsType
    {
        EMPTY,
        NORMAL,
        BARRIER,
        ROW_CLEAR,
        COLUMN_CLEAR,
        RAINBOWCANDY,
        COUNT
    }
    
    public Dictionary<SweetsType, GameObject> sweetPrefabDict;

    [System.Serializable]
    public struct SweetPrefab
    {
        public SweetsType type;
        public GameObject prefab;
    }

    public SweetPrefab[] sweetPrefabs;

    public GameObject gridPrefab;
    
    private GameSweet[,] sweets;
    
    private GameSweet pressedSweet;
    private GameSweet enteredSweet;

    #endregion
    
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            return _instance;
        }

        set
        {
            _instance = value;
        }
    }
    
    public int xColumn;
    public int yRow;
    
    public float fillTime;
    
    public Text timeText;

    private float gameTime;

    private bool gameOver;

    public int playerScore;
    private int targetScore = 150;
    
    public Text playerScoreText;
    public Text targetScoreText;
    
    private float addScoreTime;

    private float currentScore;

    public GameObject gameOverPanel;
    public GameObject gameWinPanel;

    public Text finalLoseScoreText;
    public Text finalWinScoreText;

    private Difficulty difficulty;

    [SerializeField] 
    private DifficultySO difficultySo;
    
    private void Awake()
    {
        _instance = this;
    }
    
    // Use this for initialization
    void Start()
    {
        targetScoreText.text = targetScore.ToString();
        difficulty = difficultySo.GameDifficulty;
        
        switch (difficulty)
        {
            case Difficulty.EASY:
                gameTime = 100;
                break;
            case Difficulty.NORMAL:
                gameTime = 75;
                break;
            case Difficulty.HARD:
                gameTime = 60;
                break;
        }
        
        sweetPrefabDict = new Dictionary<SweetsType, GameObject>();
        for (int i = 0; i < sweetPrefabs.Length; i++)
        {
            if (!sweetPrefabDict.ContainsKey(sweetPrefabs[i].type))
            {
                sweetPrefabDict.Add(sweetPrefabs[i].type, sweetPrefabs[i].prefab);
            }
        }


        for (int x = 0; x < xColumn; x++)
        {
            for (int y = 0; y < yRow; y++)
            {
                GameObject chocolate = Instantiate(gridPrefab, CorrectPositon(x, y), Quaternion.identity);
                chocolate.transform.SetParent(transform);
            }
        }

        sweets = new GameSweet[xColumn, yRow];
        for (int x = 0; x < xColumn; x++)
        {
            for (int y = 0; y < yRow; y++)
            {
                CreateNewSweet(x, y, SweetsType.EMPTY);
            }
        }

        if (difficulty == Difficulty.NORMAL || difficulty == Difficulty.HARD)
        {
            Destroy(sweets[4, 4].gameObject);
            CreateNewSweet(4, 4, SweetsType.BARRIER);
            Destroy(sweets[4, 3].gameObject);
            CreateNewSweet(4, 3, SweetsType.BARRIER);
            Destroy(sweets[1, 1].gameObject);
            CreateNewSweet(1, 1, SweetsType.BARRIER);
            Destroy(sweets[1, 1].gameObject);
            CreateNewSweet(1, 1, SweetsType.BARRIER);
            Destroy(sweets[7, 1].gameObject);
            CreateNewSweet(7, 1, SweetsType.BARRIER);
            Destroy(sweets[1, 6].gameObject);
            CreateNewSweet(1, 6, SweetsType.BARRIER);
            Destroy(sweets[7, 6].gameObject);
            CreateNewSweet(7, 6, SweetsType.BARRIER);
        }

        StartCoroutine(AllFill());
    }

    // Update is called once per frame
    void Update()
    {
        gameTime -= Time.deltaTime;
        if (gameTime<=0)
        {
            gameTime = 0;

            if (playerScore >= targetScore)
            {
                finalWinScoreText.text = playerScore.ToString();
                gameWinPanel.SetActive(true);
            }
            else
            {
                finalLoseScoreText.text = playerScore.ToString();
                gameOverPanel.SetActive(true);    
            }
            
            gameOver = true;
        }
        timeText.text = gameTime.ToString("0");

        if (addScoreTime<=0.05f)
        {
            addScoreTime += Time.deltaTime;
        }
        else
        {
            if (currentScore < playerScore)
            {
                currentScore++;
                playerScoreText.text = currentScore.ToString();
                addScoreTime = 0;
            }
        }

        
    }

    public Vector3 CorrectPositon(int x, int y)
    {
        return new Vector3(transform.position.x - xColumn / 2f + x, transform.position.y + yRow / 2f - y);

    }
    
    public GameSweet CreateNewSweet(int x, int y, SweetsType type)
    {
        GameObject newSweet = Instantiate(sweetPrefabDict[type], CorrectPositon(x, y), Quaternion.identity);
        newSweet.transform.parent = transform;

        sweets[x, y] = newSweet.GetComponent<GameSweet>();
        sweets[x, y].Init(x, y, this, type);

        return sweets[x, y];
    }
    
    public IEnumerator AllFill()
    {
        bool needRefill = true;

        while (needRefill)
        {
            yield return new WaitForSeconds(fillTime);
            while (Fill())
            {
                yield return new WaitForSeconds(fillTime);
            }
            
            needRefill= ClearAllMatchedSweet();
        }

       
    }
    
    public bool Fill()
    {
        bool filledNotFinished = false;

        for (int y = yRow-2; y >=0; y--)
        {
            for (int x = 0; x < xColumn; x++)
            {
                GameSweet sweet = sweets[x, y];

                if (sweet.CanMove())
                {
                    GameSweet sweetBelow = sweets[x, y + 1];

                    if (sweetBelow.Type==SweetsType.EMPTY)
                    {
                        Destroy(sweetBelow.gameObject);
                        sweet.MovedComponent.Move(x, y + 1,fillTime);
                        sweets[x, y + 1] = sweet;
                        CreateNewSweet(x, y, SweetsType.EMPTY);
                        filledNotFinished = true;
                    }
                    else         
                    {
                        for (int down = -1; down <= 1; down++)
                        {
                            if (down != 0)
                            {
                                int downX = x + down;

                                if (downX >= 0 && downX < xColumn)
                                {
                                    GameSweet downSweet = sweets[downX, y + 1];

                                    if (downSweet.Type == SweetsType.EMPTY)
                                    {
                                        bool canfill = true;

                                        for (int aboveY = y; aboveY >= 0; aboveY--)
                                        {
                                            GameSweet sweetAbove = sweets[downX, aboveY];
                                            if (sweetAbove.CanMove())
                                            {
                                                break;
                                            }
                                            else if (!sweetAbove.CanMove() && sweetAbove.Type != SweetsType.EMPTY)
                                            {
                                                canfill = false;
                                                break;
                                            }
                                        }

                                        if (!canfill)
                                        {
                                            Destroy(downSweet.gameObject);
                                            sweet.MovedComponent.Move(downX, y + 1, fillTime);
                                            sweets[downX, y + 1] = sweet;
                                            CreateNewSweet(x, y, SweetsType.EMPTY);
                                            filledNotFinished = true;
                                            break;
                                        }
                                    }

                                }
                            }
                        }
                    }
                }
                
            }
        }
        
        for (int x = 0; x < xColumn; x++)
        {
            GameSweet sweet = sweets[x, 0];

            if (sweet.Type==SweetsType.EMPTY)
            {
                GameObject newSweet= Instantiate(sweetPrefabDict[SweetsType.NORMAL], CorrectPositon(x, -1), Quaternion.identity);
                newSweet.transform.parent = transform;

                sweets[x, 0] = newSweet.GetComponent<GameSweet>();
                sweets[x, 0].Init(x, -1, this, SweetsType.NORMAL);
                sweets[x, 0].MovedComponent.Move(x, 0,fillTime);
                sweets[x, 0].ColoredComponent.SetColor((ColorSweet.ColorType)Random.Range(0, sweets[x, 0].ColoredComponent.NumColors));
                filledNotFinished = true;
            }
        }

        return filledNotFinished;
    }
    
    private bool IsFriend(GameSweet sweet1,GameSweet sweet2)
    {
        return (sweet1.X == sweet2.X && Mathf.Abs(sweet1.Y - sweet2.Y) == 1) || (sweet1.Y == sweet2.Y && Mathf.Abs(sweet1.X - sweet2.X) == 1);
    }
    
    private void ExchangeSweets(GameSweet sweet1, GameSweet sweet2)
    {
        if (sweet1.CanMove()&&sweet2.CanMove())
        {
            sweets[sweet1.X, sweet1.Y] = sweet2;
            sweets[sweet2.X, sweet2.Y] = sweet1;

            if (MatchSweets(sweet1,sweet2.X,sweet2.Y)!=null||MatchSweets(sweet2,sweet1.X,sweet1.Y)!=null||sweet1.Type==SweetsType.RAINBOWCANDY||sweet2.Type==SweetsType.RAINBOWCANDY)
            {
                int tempX = sweet1.X;
                int tempY = sweet1.Y;


                sweet1.MovedComponent.Move(sweet2.X, sweet2.Y, fillTime);
                sweet2.MovedComponent.Move(tempX, tempY, fillTime);

                if (sweet1.Type==SweetsType.RAINBOWCANDY&&sweet1.CanClear()&&sweet2.CanClear())
                {
                    ClearColorSweet clearColor = sweet1.GetComponent<ClearColorSweet>();

                    if (clearColor!=null)
                    {
                        clearColor.ClearColor = sweet2.ColoredComponent.Color;
                    }

                    ClearSweet(sweet1.X, sweet1.Y);
                }

                if (sweet2.Type == SweetsType.RAINBOWCANDY && sweet2.CanClear() && sweet1.CanClear())
                {
                    ClearColorSweet clearColor = sweet2.GetComponent<ClearColorSweet>();

                    if (clearColor != null)
                    {
                        clearColor.ClearColor = sweet1.ColoredComponent.Color;
                    }

                    ClearSweet(sweet2.X, sweet2.Y);
                }


                ClearAllMatchedSweet();
                StartCoroutine(AllFill());

                pressedSweet = null;
                enteredSweet = null;
            }
            else
            {
                sweets[sweet1.X, sweet1.Y] = sweet1;
                sweets[sweet2.X, sweet2.Y] = sweet2;
            }
            
        }
    }
    
    #region
    public void PressSweet(GameSweet sweet)
    {
        if (gameOver)
        {
            return;
        }
        pressedSweet = sweet;
    }

    public void EnterSweet(GameSweet sweet)
    {
        if (gameOver)
        {
            return;
        }
        enteredSweet = sweet;
    }

    public void ReleaseSweet()
    {
        if (gameOver)
        {
            return;
        }
        if (IsFriend(pressedSweet,enteredSweet))
        {
            ExchangeSweets(pressedSweet, enteredSweet);
        }
        
    }
    #endregion
    
    #region
    public List<GameSweet> MatchSweets(GameSweet sweet,int newX,int newY)
    {
        if (sweet.CanColor())
        {
            ColorSweet.ColorType color = sweet.ColoredComponent.Color;
            List<GameSweet> matchRowSweets = new List<GameSweet>();
            List<GameSweet> matchLineSweets = new List<GameSweet>();
            List<GameSweet> finishedMatchingSweets = new List<GameSweet>();
            
            matchRowSweets.Add(sweet);
            
            for (int i = 0; i <=1; i++)
            {
                for (int xDistance = 1; xDistance < xColumn; xDistance++)
                {
                    int x;
                    if (i==0)
                    {
                        x = newX - xDistance;
                    }
                    else
                    {
                        x = newX + xDistance;
                    }
                    if (x<0||x>=xColumn)
                    {
                        break;
                    }

                    if (sweets[x,newY].CanColor()&&sweets[x,newY].ColoredComponent.Color==color)
                    {
                        matchRowSweets.Add(sweets[x, newY]);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (matchRowSweets.Count>=3)
            {
                for (int i = 0; i < matchRowSweets.Count; i++)
                {
                    finishedMatchingSweets.Add(matchRowSweets[i]);
                }
            }
            
            if (matchRowSweets.Count>=3)
            {
                for (int i = 0; i < matchRowSweets.Count; i++)
                {
                    for (int j = 0; j <=1; j++)
                    {
                        for (int yDistance = 1; yDistance < yRow; yDistance++)
                        {
                            int y;
                            if (j==0)
                            {
                                y = newY - yDistance;
                            }
                            else
                            {
                                y = newY + yDistance;
                            }
                            if (y<0||y>=yRow)
                            {
                                break;
                            }

                            if (sweets[matchRowSweets[i].X,y].CanColor()&&sweets[matchRowSweets[i].X,y].ColoredComponent.Color==color)
                            {
                                matchLineSweets.Add(sweets[matchRowSweets[i].X, y]);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    if (matchLineSweets.Count<2)
                    {
                        matchLineSweets.Clear();
                    }
                    else
                    {
                        for (int j = 0; j < matchLineSweets.Count; j++)
                        {
                            finishedMatchingSweets.Add(matchLineSweets[j]);
                        }
                        break;
                    }
                }
            }

            if (finishedMatchingSweets.Count>=3)
            {
                return finishedMatchingSweets;
            }

            matchRowSweets.Clear();
            matchLineSweets.Clear();

            matchLineSweets.Add(sweet);
            
            
            for (int i = 0; i <= 1; i++)
            {
                for (int yDistance = 1; yDistance < yRow; yDistance++)
                {
                    int y;
                    if (i == 0)
                    {
                        y = newY - yDistance;
                    }
                    else
                    {
                        y = newY + yDistance;
                    }
                    if (y < 0 || y >= yRow)
                    {
                        break;
                    }

                    if (sweets[newX, y].CanColor() && sweets[newX, y].ColoredComponent.Color == color)
                    {
                        matchLineSweets.Add(sweets[newX, y]);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (matchLineSweets.Count >= 3)
            {
                for (int i = 0; i < matchLineSweets.Count; i++)
                {
                    finishedMatchingSweets.Add(matchLineSweets[i]);
                }
            }
            
            if (matchLineSweets.Count >= 3)
            {
                for (int i = 0; i < matchLineSweets.Count; i++)
                {
                    for (int j = 0; j <= 1; j++)
                    {
                        for (int xDistance= 1; xDistance < xColumn; xDistance++)
                        {
                            int x;
                            if (j == 0)
                            {
                                x = newY - xDistance;
                            }
                            else
                            {
                                x = newY + xDistance;
                            }
                            if (x < 0 || x >= xColumn)
                            {
                                break;
                            }

                            if (sweets[x, matchLineSweets[i].Y].CanColor() && sweets[x, matchLineSweets[i].Y].ColoredComponent.Color == color)
                            {
                                matchRowSweets.Add(sweets[x, matchLineSweets[i].Y]);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    if (matchRowSweets.Count < 2)
                    {
                        matchRowSweets.Clear();
                    }
                    else
                    {
                        for (int j = 0; j < matchRowSweets.Count; j++)
                        {
                            finishedMatchingSweets.Add(matchRowSweets[j]);
                        }
                        break;
                    }
                }
            }

            if (finishedMatchingSweets.Count >= 3)
            {
                return finishedMatchingSweets;
            }
        }

        return null;
    }
    
    public bool ClearSweet(int x, int y)
    {
        if (sweets[x,y].CanClear()&&!sweets[x,y].ClearedComponent.IsClearing)
        {
            sweets[x, y].ClearedComponent.Clear();
            CreateNewSweet(x, y, SweetsType.EMPTY);

            ClearBarrier(x, y);
            return true;
        }

        return false;
    }
    
    private void ClearBarrier(int x,int y)
    {
        for (int friendX = x-1; friendX <= x+1; friendX++)
        {
            if (friendX!=x&&friendX>=0 && friendX<xColumn)
            {
                if (sweets[friendX,y].Type==SweetsType.BARRIER&&sweets[friendX,y].CanClear())
                {
                    sweets[friendX, y].ClearedComponent.Clear();
                    CreateNewSweet(friendX, y, SweetsType.EMPTY);
                }
            }
        }

        for (int friendY = y- 1; friendY <=y+ 1; friendY++)
        {
            if (friendY != y && friendY >= 0 && friendY < yRow)
            {
                if (sweets[x,friendY].Type == SweetsType.BARRIER && sweets[x,friendY].CanClear())
                {
                    sweets[x,friendY].ClearedComponent.Clear();
                    CreateNewSweet(x,friendY, SweetsType.EMPTY);
                }
            }
        }
    }
    
    private bool ClearAllMatchedSweet()
    {
        bool needRefill = false;

        for (int y = 0; y < yRow; y++)
        {
            for (int x = 0; x < xColumn; x++)
            {
                if (sweets[x,y].CanClear())
                {
                    List<GameSweet> matchList= MatchSweets(sweets[x, y], x, y);

                    if (matchList!=null)
                    {
                        SweetsType specialSweetsType = SweetsType.COUNT;

                        GameSweet randomSweet = matchList[Random.Range(0, matchList.Count)];
                        int specialSweetX = randomSweet.X;
                        int specialSweetY = randomSweet.Y;

                        if (matchList.Count==4)
                        {
                            if (difficulty > 0)
                                specialSweetsType =(SweetsType)Random.Range((int)SweetsType.ROW_CLEAR, (int)SweetsType.COLUMN_CLEAR);
                        }
                        
                        else if (matchList.Count>=5)
                        {
                            
                            if (difficulty == Difficulty.HARD)
                                specialSweetsType = SweetsType.RAINBOWCANDY;
                        }

                        for (int i = 0; i < matchList.Count; i++)
                        {
                            if (ClearSweet(matchList[i].X, matchList[i].Y))
                            {
                                needRefill = true;
                            }
                        }

                        if (specialSweetsType!=SweetsType.COUNT)
                        {
                            Destroy(sweets[specialSweetX, specialSweetY]);
                            GameSweet newSweet = CreateNewSweet(specialSweetX, specialSweetY, specialSweetsType);
                            if (specialSweetsType==SweetsType.ROW_CLEAR||specialSweetsType==SweetsType.COLUMN_CLEAR&&newSweet.CanColor()&&matchList[0].CanColor())
                            {
                                newSweet.ColoredComponent.SetColor(matchList[0].ColoredComponent.Color);
                            }
                            
                            else if (specialSweetsType==SweetsType.RAINBOWCANDY&&newSweet.CanColor())
                            {
                                newSweet.ColoredComponent.SetColor(ColorSweet.ColorType.ANY);
                            }
                        }
                    }
                }
            }
        }
        return needRefill;
    }
    #endregion


    public void ReturnToMain()
    {
        SceneManager.LoadScene(0);
    }

    public void Replay()
    {
        SceneManager.LoadScene(1);
    }
    
    public void ClearRow(int row)
    {
        for (int x = 0; x < xColumn; x++)
        {
            ClearSweet(x, row);
        }
    }
    
    public void ClearColumn(int column)
    {
        for (int y = 0; y < yRow; y++)
        {
            ClearSweet(column, y);
        }
    }
    
    public void ClearColor(ColorSweet.ColorType color)
    {
        for (int x = 0; x < xColumn; x++)
        {
            for (int y = 0; y < yRow; y++)
            {
                if (sweets[x,y].CanColor()&&(sweets[x,y].ColoredComponent.Color==color||color==ColorSweet.ColorType.ANY))
                {
                    ClearSweet(x, y);
                }
            }
        }
    }

}
