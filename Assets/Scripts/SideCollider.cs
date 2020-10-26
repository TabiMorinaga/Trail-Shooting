using UnityEngine;

//上下の体力ゲージ
public class SideCollider : MonoBehaviour {
    void Start() {
        //Colliderを画面の横幅に合わせる
        GetComponent<BoxCollider2D>().size = new Vector2(GameManager.upperRight.x * 2, 1);
        //位置を画面端に合わせる
        transform.position = new Vector2(0, GameManager.upperRight.y * Mathf.Sign(transform.position.y));
    }
    public GameManager thisSideGM;
    public GameManager anotherSideGM;
    void OnTriggerEnter2D(Collider2D collision) {
        if (collision.tag == "Tail") {
            var bullet = collision.GetComponentInParent<Bullet>();
            thisSideGM.TimeLimitAccessor -= bullet.lifeTime;
            anotherSideGM.TimeLimitAccessor += bullet.lifeTime;
            bullet.Initialize();
        }
    }
}