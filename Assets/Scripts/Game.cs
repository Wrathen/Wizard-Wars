using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    public GameObject _player1GameObject;
    public GameObject _player2GameObject;
    public GameObject TouchArea1;
    public GameObject TouchArea2;
    public GameObject Entity_Fireball;
    public GameObject Entity_Firebolt;
    public GameObject Entity_IceLance;
    public GameObject Entity_IceShard;
    public GameObject Entity_IceShard2;
    public GameObject Entity_IceShard3;

    public GameObject _Player1;
    public GameObject _Player2;

    public bool iAmRed = true;
    public Player Me;
    public Player Enemy;

    public byte mySpell = 0;

    void CastSpell(byte spellID)
    {
        posUpdater = 0;
        Vector3 MousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        LoginScreen.CastSpell(spellID, Me.transform.localPosition.x, Me.transform.localPosition.y, MousePosition.x, MousePosition.y);
    }

    public string testingen = "";
    void Update()
    {
        GameObject.Find("fps").GetComponent<Text>().text = testingen;
    }

    private float posUpdater = 0f;
    void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) mySpell = 0;
        else if (Input.GetKeyDown(KeyCode.Alpha2)) mySpell = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha3)) mySpell = 2;
        else if (Input.GetKeyDown(KeyCode.Alpha4)) mySpell = 3;
        else if (Input.GetKeyDown(KeyCode.Alpha5)) mySpell = 4;
        else if (Input.GetKeyDown(KeyCode.Alpha6)) mySpell = 5;

        if (posUpdater < 0.4f) posUpdater += Time.deltaTime;
        if (Input.GetMouseButton(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform == TouchArea1.transform && iAmRed) CastSpell(mySpell);
                else if (hit.transform == TouchArea2.transform && !iAmRed) CastSpell(mySpell);
            }
        }

        float p1_Vx = 0;
        float p1_Vy = 0;

        if (Input.GetKey(KeyCode.A)) p1_Vx += -35.4f;
        if (Input.GetKey(KeyCode.D)) p1_Vx += 35.4f;
        if (Input.GetKey(KeyCode.W)) p1_Vy += 35.4f;
        if (Input.GetKey(KeyCode.S)) p1_Vy += -35.4f;

        if (!iAmRed) p1_Vx = -p1_Vx;

        Me.ChangeVelocity(new Vector2(p1_Vx, p1_Vy));
        Me.Move();
    }
}
