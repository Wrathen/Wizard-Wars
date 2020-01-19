using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public static class MonoBehaviourExtensions
{
    public static void Invoke(this MonoBehaviour me, Action myDelegate, float time = 0)
    {
        me.StartCoroutine(ExecuteAfterTime(myDelegate, time));
    }

    private static IEnumerator ExecuteAfterTime(Action myDelegate, float delay)
    {
        yield return new WaitForSeconds(delay);
        myDelegate();
    }
}
public class LoginScreen : MonoBehaviour
{
    UdpClient client;

    public Game game;
    public static LoginScreen _Instance;

    public static bool IAmRed = true;
    public bool gameStarted = false;
    public Queue<byte[]> ServerResponse;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        _Instance = this;
    }
    private void Start()
    {
        ServerResponse = new Queue<byte[]>();
        GameObject.Find("connecttoserver").GetComponent<Button>().onClick.AddListener(ConnectToServer);
        GameObject.Find("Button").GetComponent<Button>().onClick.AddListener(() => { Login(); });
    }
    private void Update()
    {
        if (gameStarted && SceneManager.GetActiveScene().name != "Game")
            SceneManager.LoadScene("Game", LoadSceneMode.Single);

        while (ServerResponse.Count > 0)
        {
            ParseServerMessage(ServerResponse.Dequeue());
            if (game != null) game.testingen = ServerResponse.Count.ToString();
        }
    }

    private void ConnectToServer()
    {
        try
        {
            client = new UdpClient(GameObject.Find("iptext").GetComponent<Text>().text, Int32.Parse(GameObject.Find("porttext").GetComponent<Text>().text));
            client.BeginReceive(OnRead, client);
        }
        catch { GameObject.Find("Text").GetComponent<Text>().text = "Server is Offline!"; return; }
    }
    private void OnRead(IAsyncResult ar)
    {
        UdpClient c = (UdpClient)ar.AsyncState;
        IPEndPoint receivedIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
        c.BeginReceive(OnRead, ar.AsyncState);

        ServerResponse.Enqueue(c.EndReceive(ar, ref receivedIpEndPoint));
    }

    private void ParseServerMessage(byte[] b)
    {
        Action action = (Action)b[0];
        bool playerColor = b[1] == 1;
        switch (action)
        {
            case Action.Login:
                if (playerColor) IAmRed = true;
                else IAmRed = false;
                gameStarted = true;
                break;
            case Action.Move:
                float enemyPosX = DisassembleFloat(new byte[4] { b[2], b[3], b[4], b[5] });
                float enemyPosY = DisassembleFloat(new byte[4] { b[6], b[7], b[8], b[9] });

                game.Enemy.gameObject.transform.position = new Vector3(enemyPosX, enemyPosY);
                break;
            case Action.Cast: // WHY THE *freak* THE SENDER DOESNT HAVE ANY LAG WHILST THE OTHER GUY IS EXPERIENCING HEAVY *lag*
                byte spellID = b[2];
                byte EntityID = b[19];
                float startPosX = DisassembleFloat(new byte[4] { b[3], b[4], b[5], b[6] });
                float startPosY = DisassembleFloat(new byte[4] { b[7], b[8], b[9], b[10] });
                float endPosX = DisassembleFloat(new byte[4] { b[11], b[12], b[13], b[14] });
                float endPosY = DisassembleFloat(new byte[4] { b[15], b[16], b[17], b[18] });
                Vector3 startPos = new Vector3(startPosX, startPosY);
                Vector3 endPos = new Vector3(endPosX, endPosY);

                GameObject Spell = null;
                if (spellID == 0) Spell = Instantiate(game.Entity_Fireball, startPos, Quaternion.identity);
                else if (spellID == 1) Spell = Instantiate(game.Entity_Firebolt, startPos, Quaternion.identity);
                else if (spellID == 2) Spell = Instantiate(game.Entity_IceLance, startPos, Quaternion.identity);
                else if (spellID == 3) Spell = Instantiate(game.Entity_IceShard, startPos, Quaternion.identity);
                else if (spellID == 4) Spell = Instantiate(game.Entity_IceShard2, startPos, Quaternion.identity);
                else if (spellID == 5) Spell = Instantiate(game.Entity_IceShard3, startPos, Quaternion.identity);

                Entity Behaviour = Spell.GetComponent<Entity>();

                Behaviour.id = EntityID;
                if (playerColor) Behaviour.SetCasterTo(1);
                else Behaviour.SetCasterTo(0);

                Behaviour.RotateTo(endPos);
                break;
            case Action.Hit:
                GameObject[] _AllEntities = GameObject.FindGameObjectsWithTag("Entity");
                EntityTypes spell = (EntityTypes)b[2];
                byte entityID = b[3];

                foreach (GameObject g in _AllEntities) if (g.GetComponent<Entity>().id == entityID) Destroy(g); // Destroy the occuring entity.

                if (IAmRed == playerColor && spell == EntityTypes.Fireball)
                    game.Me.TakeDamage(17);
                else
                    game.Enemy.TakeDamage(17);
                break;
            case Action.EntityExplosion:
                GameObject[] AllEntities = GameObject.FindGameObjectsWithTag("Entity");
                byte entity1 = b[2]; // Entity1's ID
                byte entity2 = b[3]; // Entity2's ID

                for (int i = 0; i < AllEntities.Length; i++)
                {
                    byte thisEntitysID = AllEntities[i].GetComponent<Entity>().id;
                    if (thisEntitysID == entity1 || thisEntitysID == entity2) Destroy(AllEntities[i]);
                }
                break;
            case Action.Die:
                StartCoroutine(RestartTheGame(playerColor));
                break;
        }
    }

    // When the Game Ends
    public IEnumerator RestartTheGame(bool loser)
    {
        if (loser == IAmRed) GameObject.Find("winlose").GetComponent<TextMeshProUGUI>().text = "LUL\nu loser";
        else GameObject.Find("winlose").GetComponent<TextMeshProUGUI>().text = "POG!\nYou Won";
        ServerResponse.Clear();
        gameStarted = false;
        game = null;
        yield return new WaitForSeconds(4f);
        IAmRed = true;
        ServerResponse = new Queue<byte[]>();
        SceneManager.LoadScene("Login", LoadSceneMode.Single);
    }
    // Sending Data to Server
    public static void Login()
    {
        SendServerMessage(new object[] { Action.Login });
    }
    public static void SendPosition(float x, float y)
    {
        SendServerMessage(new object[] {
            Action.Move, // Action Type
            IAmRed, // Player Color 1 = Red, 0 = Blue
            x, // Player Position X
            y // Player Position Y
        });
    }
    public static void CastSpell(byte id, float startPosX, float startPosY, float endPosX, float endPosY)
    {
        SendServerMessage(new object[] {
            Action.Cast, // Action Type
            IAmRed, // Player Color 1 = Red, 0 = Blue
            id, // ID of Spell
            startPosX, // Start Position X of Spell
            startPosY, // Start Position Y of Spell
            endPosX, // Target Position X of Spell
            endPosY // Target Position Y of Spell
        });
    }
    public static void GetHit(byte affectedPlayerColor, EntityTypes spell, byte entityID)
    {
        SendServerMessage(new object[] {
            Action.Hit, // Action Type
            affectedPlayerColor, // Player Color 1 = Red, 0 = Blue
            spell, // Spell type
            entityID // Entity ID of the Spell
        });
    }
    public static void EntitiesHit(byte entity1, byte entity2)
    {
        SendServerMessage(new object[] {
            Action.EntityExplosion,
            IAmRed,
            entity1,
            entity2
        });
    }
    public static void Die()
    {
        SendServerMessage(new object[] {
            Action.Die, // Action Type
            IAmRed // Player Color 1 = Red, 0 = Blue
        });
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Login")
        {
            game = GameObject.Find("Game Manager").GetComponent<Game>();
            game.iAmRed = IAmRed;
            game.Me = IAmRed ? game._Player1.GetComponent<Player>() : game._Player2.GetComponent<Player>();
            game.Enemy = IAmRed ? game._Player2.GetComponent<Player>() : game._Player1.GetComponent<Player>();
            game.Me.id = IAmRed ? (byte)1 : (byte)0;
            game.Enemy.id = IAmRed ? (byte)0 : (byte)1;
        }
    }

    // Server Stuff
    public enum Action
    {
        Login = 0,
        Move = 1,
        Cast = 2,
        Hit = 3,
        Die = 4,
        EntityExplosion = 5
    }
    public enum EntityTypes
    {
        Fireball = 0,
        Icelance = 1
    }
    public static void SendServerMessage(params object[] list)
    {
        List<byte> Message = new List<byte>();

        foreach (object o in list)
        {
            Type t = o.GetType();
            if (t.Equals(typeof(byte))) Message.AddRange(AssembleByte((byte)o));
            else if (t.Equals(typeof(bool))) Message.AddRange(AssembleBool((bool)o));
            else if (t.Equals(typeof(float))) Message.AddRange(AssembleFloat((float)o));
            else if (t.Equals(typeof(char))) Message.AddRange(AssembleChar((char)o));
            else if (t.Equals(typeof(string))) Message.AddRange(AssembleString((string)o));
            else if (t.Equals(typeof(int))) Message.AddRange(AssembleInt((int)o));
            else if (t.Equals(typeof(ushort))) Message.AddRange(AssembleUShort((ushort)o));
            else if (t.Equals(typeof(Action))) Message.AddRange(AssembleByte((byte)((Action)o)));
            else if (t.Equals(typeof(EntityTypes))) Message.AddRange(AssembleByte((byte)((EntityTypes)o)));
        }
        _Instance.client.Send(Message.ToArray(), Message.Count);
    }
    public static void SendServerMessage(string msg)
    {
        byte[] Message = AssembleString(msg);
        _Instance.client.Send(Message, Message.Length);
    }
    // Utilities
    public static byte[] AssembleByte(byte b)
    {
        return new byte[] { b };
    }
    public static byte[] AssembleBool(bool b)
    {
        return BitConverter.GetBytes(b);
    }
    public static byte[] AssembleFloat(float f)
    {
        return BitConverter.GetBytes(f);
    }
    public static byte[] AssembleChar(char c)
    {
        return Encoding.ASCII.GetBytes(new char[] { c });
    }
    public static byte[] AssembleString(string s)
    {
        return Encoding.ASCII.GetBytes(s);
    }
    public static byte[] AssembleInt(int i)
    {
        byte[] intBytes = BitConverter.GetBytes(i);
        if (BitConverter.IsLittleEndian) Array.Reverse(intBytes);
        return intBytes;
    }
    public static byte[] AssembleUShort(ushort u)
    {
        return BitConverter.GetBytes(u);
    }

    public static byte DisassembleByte(byte[] b, int i = 0)
    {
        return b[i];
    }
    public static bool DisassembleBool(byte[] b, int i = 0)
    {
        return BitConverter.ToBoolean(b, i);
    }
    public static float DisassembleFloat(byte[] b, int i = 0)
    {
        return BitConverter.ToSingle(b, i);
    }
    public static char DisassembleChar(byte[] b, int i = 0)
    {
        return BitConverter.ToChar(b, i);
    }
    public static string DisassembleString(byte[] b, int i = 0)
    {
        return BitConverter.ToString(b, i);
    }
    public static int DisassembleInt(byte[] b, int i = 0)
    {
        return BitConverter.ToInt32(b, i);
    }
    public static ushort DisassembleUShort(byte[] b, int i = 0)
    {
        return BitConverter.ToUInt16(b, i);
    }
}
