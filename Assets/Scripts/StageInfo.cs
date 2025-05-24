using System.ComponentModel;
using UnityEngine;

public class StageInfo : MonoBehaviour
{
    [Header("스테이지 설정")]
    [SerializeField]
    private string stageName;
    public string StageName { get { return stageName; } }
    [SerializeField]
    private int bossThresholdScore;
    private int bossThresholdScoreInital;
    public int BossThresholdScore { get { return bossThresholdScore; } set { bossThresholdScore = value; } }

    [Header("스테이지 오브젝트 연결")]
    [Description("스테이지 오브젝트 연결")]
    [SerializeField]
    private Camera mainCamera;
    [SerializeField]
    private Player player;

    private async void Start()
    {
        await System.Threading.Tasks.Task.Delay(500);
        if (this.mainCamera != null)
        {
            if(GameManager.Instance.mainCamera == null) GameManager.Instance.mainCamera = this.mainCamera;
            if(CameraEffectManager.Instance.mainCamera == null) CameraEffectManager.Instance.AttachMainCamera(this.mainCamera);
        }
        if (this.player != null && GameManager.Instance.player == null)
        {
            GameManager.Instance.player = this.player;
            Debug.Log("Player is set to GameManager.");
        }
        GameManager.Instance.stageInfo = this;

        await System.Threading.Tasks.Task.Delay(1500);
        GameManager.Instance.ShowCaption(stageName);

        bossThresholdScoreInital = bossThresholdScore;
    }

    public void increaseBossThreshold()
    {
        bossThresholdScore += bossThresholdScoreInital + (bossThresholdScore / 2);
    }

    public void ResetBossThreshold()
    {
        bossThresholdScore = bossThresholdScoreInital;
    }
}
