using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IPointerExitHandler {

    void Awake() {
        useableTime = timeLimit = maxTime / 2;
        for (int i = 0; i < 50; i++) {
            Instantiate(bulletPrefab, bulletParent);
        }
        upperRight = Camera.main.ScreenToWorldPoint(Vector3.zero) * -1;
    }

    public GameObject bulletPrefab;
    public Transform bulletParent;
    static public Vector2 upperRight;//画面の右上端
    Bullet drawingBullet;
    bool drawing = false;
    float maxTime = 2;
    float timeLimit;
    float useableTime;
    public RectTransform limitSlider;
    public RectTransform useableSlider;

    public void OnPointerDown(PointerEventData data) {
        for (int i = 0; i < bulletParent.childCount; i++) {
            if (!bulletParent.GetChild(i).gameObject.activeSelf) {
                drawingBullet = bulletParent.GetChild(i).GetComponent<Bullet>();
                break;
            }
        }
        if (drawingBullet == null) {
            drawingBullet = Instantiate(bulletPrefab, bulletParent).GetComponent<Bullet>();
        }
        drawingBullet.StartDraw(peDataToWorldPoint(data));
    }

    public void OnDrag(PointerEventData data) {
        if (drawingBullet != null) {
            if (!drawingBullet.gameObject.activeSelf) { drawingBullet = null; return; }
            useableTime -= Time.deltaTime;
            ChangeSlider();
            if (useableTime == 0) { OnPointerUp(data); return; }
            drawingBullet.SetPosition(peDataToWorldPoint(data));
            drawing = true;
        }
    }

    public void OnPointerUp(PointerEventData data) {
        if (drawingBullet != null) {
            if (!drawingBullet.gameObject.activeSelf) { drawingBullet = null; return; }
            drawingBullet.SetPosition(peDataToWorldPoint(data));
            drawingBullet.StartCoroutine(drawingBullet.Repeat());
            drawingBullet = null;
            drawing = true;
        }
    }

    public void OnPointerExit(PointerEventData data) {
        OnPointerUp(data);
    }

    void Update() {
        if (!drawing && useableTime < timeLimit) {
            useableTime += Time.deltaTime / (timeLimit / maxTime * 5);
            ChangeSlider();
        }
        else {
            drawing = false;
        }
    }

    public Transform sideColl;
    public Text resultText;
    public float TimeLimitAccessor {
        set {
            timeLimit = Mathf.Clamp(value, 0, maxTime);
            if (timeLimit == maxTime) { resultText.text = "Win"; }
            if (timeLimit == 0) { resultText.text = "Lose"; }
            if (timeLimit == maxTime || timeLimit == 0) { StartCoroutine(ReloadNewGame()); }
            limitSlider.localScale = sideColl.localScale = new Vector2(timeLimit / maxTime, 1);
            ChangeSlider();
        }
        get { return timeLimit; }
    }

    IEnumerator ReloadNewGame() {
        GetComponent<Text>().raycastTarget = false;
        yield return new WaitForSeconds(3);
        resultText.text = "Continue";
        resultText.raycastTarget = true;
    }

    public void LoadScene() {
        SceneManager.LoadScene("GameScene");
    }

    void ChangeSlider() {
        useableTime = Mathf.Clamp(useableTime, 0, timeLimit);
        useableSlider.localScale = new Vector2(useableTime / maxTime, 1);
    }

    public RectTransform rectTransform;
    Vector2 peDataToWorldPoint(PointerEventData data) {
        Vector3 worldPoint;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, data.position, Camera.main, out worldPoint);
        return worldPoint;
    }
}