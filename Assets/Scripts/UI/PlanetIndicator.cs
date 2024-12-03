using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlanetIndicator : MonoBehaviour
{
    public Camera mainCamera; // �������
    public Canvas worldSpaceCanvas; // ����ռ�� Canvas
    public RectTransform distanceTextPrefab; // �����ı�Ԥ�Ƽ�
    public RectTransform arrowPrefab; // ��ͷԤ�Ƽ�
    public RectTransform meteoriteDistanceTextPrefab; // ��ʯ�����ı�Ԥ�Ƽ�
    public RectTransform warningSignPrefab; // �����־Ԥ�Ƽ�
    public float edgePadding = 50f; // ��Ļ��Ե�����
    public Vector2 planetOffset = new Vector2(0, 50); // �����ı���ƫ��
    public Vector2 meteoriteOffset = new Vector2(0, 30); // ��ʯ�ı���ƫ��

    private List<TargetData> planetTargets = new List<TargetData>(); // ����Ŀ���б�
    private List<TargetData> meteoriteTargets = new List<TargetData>(); // ��ʯĿ���б�

    // �洢Ŀ��������ݵ���
    private class TargetData
    {
        public Transform targetTransform; // Ŀ��λ��
        public RectTransform distanceTextUI; // �����ı� UI
        public RectTransform arrowUI; // ��ͷ UI
        public Text distanceTextComponent; // �ı����
        public Image warningSign; // �����־

        public TargetData(Transform target, RectTransform distanceText, RectTransform arrow, Text distanceTextComp, Image warningSignImage)
        {
            targetTransform = target;
            distanceTextUI = distanceText;
            arrowUI = arrow;
            distanceTextComponent = distanceTextComp;
            warningSign = warningSignImage;
        }
    }

    void Start()
    {
        // ��ʼ�����Ǻ���ʯĿ��
        InitializeTargets("Ground", distanceTextPrefab, arrowPrefab, planetTargets);
        InitializeTargets("Meteorite", meteoriteDistanceTextPrefab, warningSignPrefab, meteoriteTargets);
    }

    void Update()
    {
        // ����Ŀ��� UI
        UpdateTargets(planetTargets, isMeteorite: false);
        UpdateTargets(meteoriteTargets, isMeteorite: true);

        // ����Ƿ����µ���ʯ����
        CheckForNewMeteorites();
    }

    private void InitializeTargets(string tag, RectTransform distancePrefab, RectTransform arrowPrefab, List<TargetData> targetList)
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject obj in targets)
        {
            if (obj != null)
            {
                AddTarget(obj.transform, distancePrefab, arrowPrefab, targetList, tag == "Meteorite");
            }
        }
    }

    private void AddTarget(Transform target, RectTransform distancePrefab, RectTransform arrowPrefab, List<TargetData> targetList, bool isMeteorite)
    {
        RectTransform distanceText = Instantiate(distancePrefab, worldSpaceCanvas.transform);
        distanceText.localScale = Vector3.one;

        RectTransform arrow = null;
        Image warningSign = null;

        if (isMeteorite)
        {
            warningSign = Instantiate(arrowPrefab, worldSpaceCanvas.transform).GetComponent<Image>();
            warningSign.rectTransform.localScale = Vector3.one;
        }
        else
        {
            arrow = Instantiate(arrowPrefab, worldSpaceCanvas.transform);
            arrow.localScale = Vector3.one;
        }

        Text distanceTextComp = distanceText.GetComponent<Text>();
        targetList.Add(new TargetData(target, distanceText, arrow, distanceTextComp, warningSign));
    }

    private void UpdateTargets(List<TargetData> targetList, bool isMeteorite)
    {
        for (int i = targetList.Count - 1; i >= 0; i--) // ��������԰�ȫ�Ƴ�Ԫ��
        {
            var targetData = targetList[i];

            if (targetData.targetTransform == null) // ���Ŀ���ѱ�����
            {
                RemoveTarget(targetData, targetList);
                continue;
            }

            UpdateTargetUI(targetData, isMeteorite);
        }
    }

    private void RemoveTarget(TargetData targetData, List<TargetData> targetList)
    {
        if (targetData.distanceTextUI != null)
            Destroy(targetData.distanceTextUI.gameObject);

        if (targetData.arrowUI != null)
            Destroy(targetData.arrowUI.gameObject);

        if (targetData.warningSign != null)
            Destroy(targetData.warningSign.gameObject);

        targetList.Remove(targetData);
    }

    private void UpdateTargetUI(TargetData targetData, bool isMeteorite)
    {
        if (mainCamera == null || worldSpaceCanvas == null)
        {
            return;
        }

        Vector3 screenPoint = mainCamera.WorldToScreenPoint(targetData.targetTransform.position);
        bool isTargetVisible = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < Screen.width && screenPoint.y > 0 && screenPoint.y < Screen.height;

        float distanceToTarget = Vector3.Distance(mainCamera.transform.position, targetData.targetTransform.position);

        if (isTargetVisible)
        {
            // ��ʾ�����ı������ؼ�ͷ�򾯸��־
            if (isMeteorite)
            {
                targetData.warningSign?.gameObject.SetActive(false);
                targetData.distanceTextUI?.gameObject.SetActive(true);
            }
            else
            {
                targetData.arrowUI?.gameObject.SetActive(false);
                targetData.distanceTextUI?.gameObject.SetActive(true);
            }

            RectTransform canvasRect = worldSpaceCanvas.GetComponent<RectTransform>();
            Vector2 viewportPoint = mainCamera.WorldToViewportPoint(targetData.targetTransform.position);
            Vector2 canvasPosition = new Vector2(
                (viewportPoint.x - 0.5f) * canvasRect.sizeDelta.x,
                (viewportPoint.y - 0.5f) * canvasRect.sizeDelta.y
            );

            // Ӧ��ƫ����
            canvasPosition += isMeteorite ? meteoriteOffset : planetOffset;

            targetData.distanceTextUI.anchoredPosition = canvasPosition;
            targetData.distanceTextComponent.text = $"{distanceToTarget:F1}m";
        }
        else
        {
            // ��ʾ��ͷ�򾯸��־
            if (isMeteorite)
            {
                targetData.distanceTextUI?.gameObject.SetActive(false);
                targetData.warningSign?.gameObject.SetActive(true);

                RectTransform canvasRect = worldSpaceCanvas.GetComponent<RectTransform>();
                Vector2 arrowPosition = CalculateOffScreenPosition(screenPoint, canvasRect);
                targetData.warningSign.rectTransform.anchoredPosition = arrowPosition;

                // �����ͷ��ת����
                Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
                Vector2 direction = (new Vector2(screenPoint.x, screenPoint.y) - screenCenter).normalized;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90f;
                targetData.warningSign.rectTransform.localRotation = Quaternion.Euler(0, 0, angle);
            }
            else
            {
                targetData.distanceTextUI?.gameObject.SetActive(false);
                targetData.arrowUI?.gameObject.SetActive(true);

                RectTransform canvasRect = worldSpaceCanvas.GetComponent<RectTransform>();
                Vector2 arrowPosition = CalculateOffScreenPosition(screenPoint, canvasRect);
                targetData.arrowUI.anchoredPosition = arrowPosition;

                // �����ͷ��ת����
                Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
                Vector2 direction = (new Vector2(screenPoint.x, screenPoint.y) - screenCenter).normalized;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90f;
                targetData.arrowUI.localRotation = Quaternion.Euler(0, 0, angle);
            }
        }
    }

    private Vector2 CalculateOffScreenPosition(Vector3 screenPoint, RectTransform canvasRect)
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        Vector3 direction = (screenPoint - screenCenter).normalized;

        float maxX = canvasRect.sizeDelta.x / 2f - edgePadding;
        float maxY = canvasRect.sizeDelta.y / 2f - edgePadding;

        return new Vector2(
            Mathf.Clamp(direction.x * maxX, -maxX, maxX),
            Mathf.Clamp(direction.y * maxY, -maxY, maxY)
        );
    }

    private void CheckForNewMeteorites()
    {
        GameObject[] newMeteorites = GameObject.FindGameObjectsWithTag("Meteorite");
        foreach (GameObject meteorite in newMeteorites)
        {
            if (!meteoriteTargets.Exists(t => t.targetTransform == meteorite.transform))
            {
                AddTarget(meteorite.transform, meteoriteDistanceTextPrefab, warningSignPrefab, meteoriteTargets, true);
            }
        }
    }
}