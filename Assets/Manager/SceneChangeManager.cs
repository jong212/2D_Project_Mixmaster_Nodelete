// SceneChangeManager.cs
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class SceneChangeManager : Singleton<SceneChangeManager>
{

    //�ʱ�ȭ ���� 
    [SerializeField] private string sceneToLoad = "map1";
    [SerializeField] GameObject monsterPrefab;
    [SerializeField] private int MonLevel = 1;
    [SerializeField] public MonsterData getMonsterData;

    // ��� �� ���� ������ ��� ��ųʸ�
    public Dictionary<LinkedNode, int> nodeByLevel = new Dictionary<LinkedNode, int>();

    // ���� �߽� ��ġ ����Ʈ
    public List<Vector3Int> roomCenterList = new List<Vector3Int>();
    Coroutine monsterSpawnCoroutine;

    // SceneToLoad �Ӽ�
    public string SceneToLoad
    {
        get
        {
            return sceneToLoad;
        }
        set
        {
            sceneToLoad = value;
        }
    }

    //scene To Load �ʵ忡 ���� ������ �̵�
    public void StartButton()
    {
        // SceneToLoad�� ������ ���� �񵿱�� �ε�
        SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Single).completed += OnSceneLoaded;
    } 
    // �� �ε� �Ϸ� �� ȣ��Ǵ� �ݹ� �޼���
    private void OnSceneLoaded(AsyncOperation asyncOperation)
    {
        if (monsterSpawnCoroutine != null)
        {
            // ���� ���� �ڷ�ƾ ����
            StopCoroutine(monsterSpawnCoroutine);
          
        }
        // �ε尡 �Ϸ�Ǹ�
        if (asyncOperation.isDone)
            //Debug.Log("Test1");
        {
            // ���÷��̾� ������ �������� �߰��� ���� �����͵� ������
            PlayerData playerData = GameDB.Instance.playerDictionary.GetValueOrDefault("Player");
            if (playerData != null)
            {
                // �÷��̾� ������ �ε� �� ����
                GameObject playerPrefab = Resources.Load<GameObject>("Prefabs/Player");
                if (playerPrefab != null)
                {

                    GameObject playerInstance = Instantiate(playerPrefab);
                    GameManager.Instance.SetPlayer(playerInstance);

                    //���ᵥ���͵� ��������..                   
                    if (sceneToLoad != "map1")
                    {
                        //�� ��ųʸ����� Ű�� ������ 
                        foreach (string teamKey in GameDB.Instance.TeamDictionary.Keys)
                        {                            
                            //Ű�� �ش��ϴ� ���� ���� ���� �ڵ�
                            TeamData teamData = GameDB.Instance.TeamDictionary[teamKey];
                            
                            //������ state�� �����̸� ���� �ֳ��ϸ� �������� ���� ���̶� 
                            if (!teamData.state) continue;

                            //Ű�� ������ �̸��� �����ؼ� �����հ������� 
                            GameObject teamMemberPrefab = Resources.Load<GameObject>($"Prefabs/{teamKey}");
                            //�� ������Ʈ ����
                            
                            
                            if (teamKey == "team1") teamMemberPrefab.tag = "team1";
                            else if (teamKey == "team2") teamMemberPrefab.tag = "team2";
                            else if (teamKey == "team3") teamMemberPrefab.tag = "team3";
                            else Debug.LogError("NoTag");


                            if (teamMemberPrefab != null)
                            {   
                                //������ �����鿡�� monstercontrolloer�� �ʿ��� ����� ���ؼ� ������
                                MonsterController mcDelete = teamMemberPrefab.GetComponent<MonsterController>();

                                if (mcDelete != null)
                                {
                                    mcDelete.enabled = false;
                                }


                                //������Ʈ�� ���� 
                                GameObject teamMemberInstance = Instantiate(teamMemberPrefab);
                                if (teamMemberInstance.GetComponent<Playerteam>() == null)
                                {
                                    // If it doesn't have the component, add it
                                    teamMemberInstance.AddComponent<Playerteam>();
                                    Debug.Log("YourComponent added.");
                                }
                                else
                                {
                                    Debug.Log(teamKey);
                                }
                                //�÷��̾� �Ʒ��� ������
                                teamMemberInstance.GetComponent<Playerteam>().Init(teamData);
                                teamMemberInstance.transform.parent = playerInstance.transform;
                                
                            }
                            else
                            {
                                // Log an error if the team member prefab is not found
                                Debug.LogError("Team member prefab not found!");
                            }
                        }
                    }


                    if (sceneToLoad != "map1")
                    {
                        //������ ���� ��⼭ ������
                        ObjectPool.Instance.CreateInstance("mon1", null, 3);
                        ObjectPool.Instance.CreateInstance("mon2", null, 5);
                        ObjectPool.Instance.CreateInstance("mon3", null, 5);
                        ObjectPool.Instance.CreateInstance("mon4", null, 5);
                        ObjectPool.Instance.CreateInstance("mon5", null, 5);
                        ObjectPool.Instance.CreateInstance("mon6", null, 5);


                        //GameObject monster = ObjectPool.Instance.GetInactiveObject("mon1");
                        //Ȱ��ȭ
                        //monster.SetActive(true);
                        //��ȯ
                        //ObjectPool.Instance.AddInactiveObject(monster);

                    }
                    Vector3Int firstRoomCenter = Vector3Int.zero;
                    Tilemap tilemap = null;

                    // ���� map1�� �ƴ� ��� ���� ������ ���� �� Ȱ��ȭ
                    if (sceneToLoad != "map1")
                    {
                        // ����� �����Ϳ��� ù ��° ���� �߽��� �����ɴϴ�.
                        Vector3 vec = GameManager.Instance.linkData.Head.room.center;
                        firstRoomCenter = new Vector3Int((int)vec.x, (int)vec.y);//roomCenterList[0];

                        // ���� Scene�� Ÿ�ϸ��� �����ɴϴ�.
                        tilemap = FindObjectOfType<CreateMap>().GetTilemap();
                        GameManager.Instance.tilemap = tilemap;
                    }
                    // ī�޶� �÷��̾ ����ٴϵ��� ����
                    Camera_fix cameraFollowScript = Camera.main.GetComponent<Camera_fix>();
                    cameraFollowScript.player = playerInstance;

                    // Ÿ�ϸ��� null�� �ƴ� ���
                    if (tilemap != null)
                    {
                        // �׸��� ��ǥ�� ���� ��ǥ�� ��ȯ�մϴ�.
                        Vector3 worldPosition = tilemap.CellToWorld(firstRoomCenter);
                        // �÷��̾� ��ġ�� ���� ��ǥ�� �����մϴ�.
                        playerInstance.transform.position = worldPosition;
                        // �÷��̾� �ֺ��� ���͸� �����մϴ�.
                        monsterSpawnCoroutine = StartCoroutine(CheakOnRect());

                    }
                    // �÷��̾� �����͸� ������� �÷��̾��� ���¸� �����մϴ�.
                    Statusinfo statusComponent = playerInstance.GetComponent<Statusinfo>();
                    if (statusComponent != null)
                    {
                        statusComponent.hp = playerData.health;
                        statusComponent.str = playerData.str;
                        statusComponent.name = playerData.name;
                    }
                    else
                    {
                        // �÷��̾� ������ �ν��Ͻ����� Statusinfo ������Ʈ�� ã�� �� ���� ��� ������ ����մϴ�.
                        Debug.LogError("Statusinfo component not found on the player prefab instance");
                    }
                }
                else
                {
                    // �÷��̾� �������� �߰ߵ��� ���� ��� ������ ����մϴ�.
                    Debug.LogError("Player prefab not found!");
                }
            }
            else
            {
                // ���� �����ͺ��̽����� �÷��̾� �����͸� ã�� �� ���� ��� ��� ����մϴ�.
                Debug.LogWarning("Player data not found in GameDB");
            }
        }
    }
    // ��� ������ �����մϴ�.
    public void SetNodeLevel()
    {
        int level = 1;
        List<LinkedNode> currentNodes = new List<LinkedNode>();
        List<LinkedNode> recordNodes = new List<LinkedNode>();
        currentNodes.Add(GameManager.Instance.linkData.Head);

        while (true)
        {
            recordNodes.AddRange(currentNodes);
            foreach (LinkedNode node in currentNodes)
            {
                nodeByLevel.Add(node, level);
            }
            List<LinkedNode> temp = new List<LinkedNode>();
            currentNodes.ForEach(node => temp.AddRange(node.linkedNodeDic.Values.ToList()));

            currentNodes = temp.Where(node => !recordNodes.Contains(node)).Distinct().ToList();
            if (currentNodes.Count == 0)
                return;
            level++;
            
        }
        //nodeByLevel;
    }
    // ���͸� �ν��Ͻ�ȭ�մϴ�.
    List<GameObject> InstantiateMonster(int def = 0)
    {

        //�� �׷��� ���� ������
        MonsterData monsterData = GameDB.Instance.GetMonster($"mon{def}");

        //GameObject monster = ObjectPool.Instance.GetInactiveObject($"mon{def}");
        //monster.GetComponent<monsterinfo>().Init(monsterData);

        // Ű�� �ش��ϴ� ��� ���͸� Ȱ��ȭ�մϴ�.
        List<GameObject> allMonsters = ObjectPool.Instance.GetAllInactiveObjects($"mon{def}");

        // Ȱ��ȭ�� ��� ���Ϳ� ���� �����͸� �����մϴ�.
        allMonsters.ForEach(x => x.GetComponent<MonsterController>().Init(monsterData));

        return allMonsters;
    }

    // �簢�� ������ ��带 Ȯ���ϰ� ���͸� �����ϴ� �ڷ�ƾ�Դϴ�.
    IEnumerator CheakOnRect()
    {
        // ��� ������ �����մϴ�.
        SetNodeLevel();
        List<GameObject> monsterGroup = new List<GameObject>();
        while (true)
        {
            // �÷��̾� ��ġ�� ������� ��带 Ȯ���մϴ�.
            Vector3Int vec = GameManager.Instance.tilemap.WorldToCell(GameManager.Instance.player.transform.position);
            var linkedNode = GameManager.Instance.CheakOnNode(vec);
            if (linkedNode == null)
            {
                yield return new WaitForSeconds(0.1f);
                continue;
            }
            /*if (GameManager.Instance.nodes.Contains(linkedNode))
            {
                yield return new WaitForSeconds(0.1f);
                continue;
            }
            GameManager.Instance.nodes.Add(linkedNode);*/
            if (GameManager.Instance.currentNode == linkedNode)
            {
                yield return new WaitForSeconds(0.1f);
                continue;
            }
            GameManager.Instance.currentNode = linkedNode;
            //Debug.Log(MonLevel);

            if (monsterGroup.Count > 0)
                monsterGroup.ForEach(x => ObjectPool.Instance.AddInactiveObject(x));
            // �ش� ����� ������ ���� ���͸� �ν��Ͻ�ȭ�մϴ�.
            List<GameObject> monsters = InstantiateMonster(nodeByLevel[linkedNode]);

            monsterGroup.AddRange(monsters);

            /*MonLevel++;
            if (MonLevel > 5)
                MonLevel = 5;*/

            monsters.ForEach(x => x.transform.position = linkedNode.CellToPosition(GameManager.Instance.tilemap));


            yield return new WaitForSeconds(0.1f);
        }
    }
}