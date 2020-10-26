using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Bullet : MonoBehaviour {
    //TrailRenderer Shadow;
    void Start() {
        trailRenderer = GetComponent<TrailRenderer>();
        edgeColl = transform.GetComponentInChildren<EdgeCollider2D>();
        //Shadow = transform.GetComponentInChildren<TrailRenderer>();
    }

    TrailRenderer trailRenderer;
    EdgeCollider2D edgeColl;
    List<Vector2> positions = new List<Vector2>();
    float drawnTime;
    bool used = false;

    public void StartDraw(Vector2 pos) {
        used = true;
        transform.position = pos;
        gameObject.SetActive(true);
    }

    public void SetPosition(Vector2 pos) {
        transform.position = pos;
        positions.Add(pos);
        drawnTime += Time.deltaTime;
        /*Shadow.time = */trailRenderer.time += Time.deltaTime;
        edgeColl.points = positions.Skip(Mathf.CeilToInt(positions.Count * (1 - trailRenderer.time / drawnTime))).Select(p => p - positions.Last()).ToArray();
    }

    public IEnumerator Repeat() {
        if (positions.Count < 2) { Initialize(); yield break; }
        sbyte inverted = 1;//画面端で反転させる
        int invertedCount = -1;
        float remainder = 0;
        for (int i = 0; i < 200; i++) {
            if (!used) { yield break; }
            var vec = (Vector3)(positions[1] - positions[0]);
            transform.position += Vector3.up * vec.y;//先にｙの値は足しておく
            if (Mathf.Abs(transform.position.y) > GameManager.upperRight.y * 2) { Initialize(); yield break; }
            var x = vec.x;
            if (i == invertedCount & invertedCount != -1) {
                x = x + remainder * Mathf.Sign(x);
                inverted *= -1; invertedCount = -1;
            }

            //左右の画面端で跳ね返す処理
            x *= inverted;
            var leadPos = transform.position.x;
            if (Mathf.Abs(leadPos + x) > GameManager.upperRight.x) {
                inverted *= -1;
                var outside = Mathf.Abs(leadPos + x) - GameManager.upperRight.x;
                remainder = Mathf.Abs(x) - outside;
                x = outside * Mathf.Sign(x) * inverted;
                invertedCount = i + positions.Count - 1;
            }
            transform.position += Vector3.right * x;
            positions.Add(transform.position);
            positions.RemoveAt(0);
            //                              描かれてない部分を飛ばして
            edgeColl.points = positions.Skip(Mathf.CeilToInt(positions.Count * (1 - trailRenderer.time / drawnTime)))
                .Select(p => p - positions.Last()).ToArray();
            //  ↑先頭からの相対位置に変換する
            yield return null;
        }
        Initialize();
    }

    //初期化
    public void Initialize() {
        edgeColl.points = null;
        positions.Clear();
        used = false;
        StartCoroutine(Shrink());
    }

    //100フレームで消滅させる
    IEnumerator Shrink() {
        var time = trailRenderer.time;
        var flame = 100;
        for (int i = flame; i > 0; i--) {
            /*Shadow.time = */trailRenderer.time = time * i / flame;
            yield return null;
        }
        drawnTime = /*Shadow.time = */trailRenderer.time = 0;
        gameObject.SetActive(false);
    }

    public float lifeTime {
        set {
            /*Shadow.time = */trailRenderer.time = value;
            if (trailRenderer.time <= 0) {
                Initialize();
            }
        }
        get { return trailRenderer.time; }
    }

    void OnTriggerEnter2D(Collider2D collision) {
        //頭でかち合ったら消滅
        if (collision.tag == "Head") { Initialize(); }
        //しっぽに当たったらぶつかった弾の長さ分短くなる
        if (collision.tag == "Tail" && collision.transform.parent.gameObject.activeSelf) {
            var bullet = collision.GetComponentInParent<Bullet>();
            var time = bullet.lifeTime;
            bullet.lifeTime -= lifeTime;
            lifeTime -= time;
        }
    }
}