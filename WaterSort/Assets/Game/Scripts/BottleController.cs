using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class BottleController : MonoBehaviour
{
    public Color[] bottleColors;
    public SpriteRenderer bottleMaskSR;

    public AnimationCurve ScaleAndRotationMultiplierCurve;
    public AnimationCurve FillAmountCurve;
    public AnimationCurve RotationSpeedMultiplier;

    public float[] fillAmounts;
    public float[] rotationValues;

    private int rotationIndex = 0;

    [Range(0, 4)]
    public int numberOfColorsInBottle = 4;

    public Color topColor;
    public int numberOfTopColorLayers = 1;

    public BottleController bottleControllerRef;
    public bool justThisBottle = false;
    private int numberOfColorsToTransfer = 0;

    public Transform leftRotationPoint;
    public Transform rightRotationPoint;
    private Transform chosenRotationPoint;

    private float directionMultiplier = 1.0f;

    private Vector3 originalPosition;
    private Vector3 startPosition;
    private Vector3 endPosition;

    public LineRenderer lineRenderer;

    public float timeToRotate = 1.0f;

    private void Start()
    {
        bottleMaskSR.material.SetFloat("_FillAmount", fillAmounts[numberOfColorsInBottle]);

        originalPosition = transform.position;

        UpdateColorsOnShader();
        UpdateTopColorValues();

        SetupLineRenderer();
    }

    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.pKey.wasPressedThisFrame &&
            justThisBottle == true)
        {
            UpdateTopColorValues();

            if (bottleControllerRef.FillBottleCheck(topColor))
            {
                ChoseRotationPointAndDirection();

                numberOfColorsToTransfer = Mathf.Min(
                    numberOfTopColorLayers,
                    4 - bottleControllerRef.numberOfColorsInBottle
                );

                for (int i = 0; i < numberOfColorsToTransfer; i++)
                {
                    bottleControllerRef.bottleColors[
                        bottleControllerRef.numberOfColorsInBottle + i
                    ] = topColor;
                }

                bottleControllerRef.UpdateColorsOnShader();
            }

            CalculateRotationIndex(4 - bottleControllerRef.numberOfColorsInBottle);
            StartCoroutine(RotateBottle());
        }
    }

    private void SetupLineRenderer()
    {
        if (lineRenderer == null)
        {
            Debug.LogError("LineRenderer atanmadı!", this);
            return;
        }

        lineRenderer.enabled = false;
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;

        lineRenderer.startWidth = 0.08f;
        lineRenderer.endWidth = 0.08f;

        lineRenderer.sortingLayerName = "Default";
        lineRenderer.sortingOrder = 999;

        Material lineMat = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.material = lineMat;
    }

    private void DrawPourLine()
    {
        if (lineRenderer == null || chosenRotationPoint == null)
            return;

        lineRenderer.enabled = true;
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;

        lineRenderer.startWidth = 0.3f;
        lineRenderer.endWidth = 0.3f;

        lineRenderer.sortingLayerName = "Default";
        lineRenderer.sortingOrder = 999;

        Color lineColor = topColor;
        lineColor.a = 1f;

        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;

        Vector3 start = chosenRotationPoint.position;
        Vector3 end = chosenRotationPoint.position - Vector3.up * 14.5f;

        start.z = 0f;
        end.z = 0f;

        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }

    private void HidePourLine()
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }

    public void StartColorTransfer()
    {
        if (bottleControllerRef == null)
        {
            Debug.LogError("There Is No Bottle Controller Ref", this);
            return;
        }

        UpdateTopColorValues();

        if (!bottleControllerRef.FillBottleCheck(topColor))
            return;

        ChoseRotationPointAndDirection();

        numberOfColorsToTransfer = Mathf.Min(
            numberOfTopColorLayers,
            4 - bottleControllerRef.numberOfColorsInBottle
        );

        for (int i = 0; i < numberOfColorsToTransfer; i++)
        {
            bottleControllerRef.bottleColors[
                bottleControllerRef.numberOfColorsInBottle + i
            ] = topColor;
        }

        bottleControllerRef.UpdateColorsOnShader();

        CalculateRotationIndex(4 - bottleControllerRef.numberOfColorsInBottle);

        GetComponent<SpriteRenderer>().sortingOrder += 2;
        bottleMaskSR.sortingOrder += 2;

        StartCoroutine(MoveBottle());
    }

    private IEnumerator MoveBottle()
    {
        startPosition = transform.position;

        if (chosenRotationPoint == leftRotationPoint)
        {
            endPosition = bottleControllerRef.rightRotationPoint.position;
        }
        else
        {
            endPosition = bottleControllerRef.leftRotationPoint.position;
        }

        float t = 0f;

        while (t <= 1f)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            t += Time.deltaTime * 2f;

            yield return new WaitForEndOfFrame();
        }

        transform.position = endPosition;
        StartCoroutine(RotateBottle());
    }

    private IEnumerator MoveBottleBack()
    {
        startPosition = transform.position;
        endPosition = originalPosition;

        float t = 0f;

        while (t <= 1f)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            t += Time.deltaTime * 2f;

            yield return new WaitForEndOfFrame();
        }

        transform.position = endPosition;

        GetComponent<SpriteRenderer>().sortingOrder -= 2;
        bottleMaskSR.sortingOrder -= 2;
    }

    private void UpdateColorsOnShader()
    {
        bottleMaskSR.material.SetColor("_C1", bottleColors[0]);
        bottleMaskSR.material.SetColor("_C2", bottleColors[1]);
        bottleMaskSR.material.SetColor("_C3", bottleColors[2]);
        bottleMaskSR.material.SetColor("_C4", bottleColors[3]);
    }

    private IEnumerator RotateBottle()
    {
        float t = 0f;
        float lerpValue;
        float angleValue;
        float lastAngleValue = 0f;

        while (t < timeToRotate)
        {
            lerpValue = t / timeToRotate;

            angleValue = Mathf.Lerp(
                0.0f,
                directionMultiplier * rotationValues[rotationIndex],
                lerpValue
            );

            transform.RotateAround(
                chosenRotationPoint.position,
                Vector3.forward,
                lastAngleValue - angleValue
            );

            float curveAngle = Mathf.Abs(angleValue);
            float lastCurveAngle = Mathf.Abs(lastAngleValue);

            bottleMaskSR.material.SetFloat(
                "_SARM",
                ScaleAndRotationMultiplierCurve.Evaluate(curveAngle)
            );

            float currentFill = fillAmounts[numberOfColorsInBottle];
            float curveFill = FillAmountCurve.Evaluate(curveAngle);

            if (currentFill > curveFill + 0.005f)
            {
                DrawPourLine();

                bottleMaskSR.material.SetFloat("_FillAmount", curveFill);

                float fillDifference =
                    FillAmountCurve.Evaluate(lastCurveAngle) -
                    FillAmountCurve.Evaluate(curveAngle);

                bottleControllerRef.FillUp(fillDifference);
            }

            t += Time.deltaTime * RotationSpeedMultiplier.Evaluate(curveAngle);
            lastAngleValue = angleValue;

            yield return new WaitForEndOfFrame();
        }

        angleValue = directionMultiplier * rotationValues[rotationIndex];
        float finalCurveAngle = Mathf.Abs(angleValue);

        bottleMaskSR.material.SetFloat(
            "_SARM",
            ScaleAndRotationMultiplierCurve.Evaluate(finalCurveAngle)
        );

        bottleMaskSR.material.SetFloat(
            "_FillAmount",
            FillAmountCurve.Evaluate(finalCurveAngle)
        );

        numberOfColorsInBottle -= numberOfColorsToTransfer;
        bottleControllerRef.numberOfColorsInBottle += numberOfColorsToTransfer;

        HidePourLine();

        StartCoroutine(RotateBottleBack());
    }

    private IEnumerator RotateBottleBack()
    {
        float t = 0f;
        float lerpValue;
        float angleValue;

        float lastAngleValue = directionMultiplier * rotationValues[rotationIndex];

        while (t < timeToRotate)
        {
            lerpValue = t / timeToRotate;

            angleValue = Mathf.Lerp(
                directionMultiplier * rotationValues[rotationIndex],
                0.0f,
                lerpValue
            );

            transform.RotateAround(
                chosenRotationPoint.position,
                Vector3.forward,
                lastAngleValue - angleValue
            );

            float curveAngle = Mathf.Abs(angleValue);

            bottleMaskSR.material.SetFloat(
                "_SARM",
                ScaleAndRotationMultiplierCurve.Evaluate(curveAngle)
            );

            lastAngleValue = angleValue;

            t += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        UpdateTopColorValues();

        angleValue = 0f;
        transform.eulerAngles = new Vector3(0f, 0f, angleValue);

        bottleMaskSR.material.SetFloat(
            "_SARM",
            ScaleAndRotationMultiplierCurve.Evaluate(0f)
        );

        StartCoroutine(MoveBottleBack());
    }

    public void UpdateTopColorValues()
    {
        if (numberOfColorsInBottle != 0)
        {
            numberOfTopColorLayers = 1;

            topColor = bottleColors[numberOfColorsInBottle - 1];
            topColor.a = 1f;

            if (numberOfColorsInBottle == 4)
            {
                if (bottleColors[3].Equals(bottleColors[2]))
                {
                    numberOfTopColorLayers = 2;

                    if (bottleColors[2].Equals(bottleColors[1]))
                    {
                        numberOfTopColorLayers = 3;

                        if (bottleColors[1].Equals(bottleColors[0]))
                        {
                            numberOfTopColorLayers = 4;
                        }
                    }
                }
            }
            else if (numberOfColorsInBottle == 3)
            {
                if (bottleColors[2].Equals(bottleColors[1]))
                {
                    numberOfTopColorLayers = 2;

                    if (bottleColors[1].Equals(bottleColors[0]))
                    {
                        numberOfTopColorLayers = 3;
                    }
                }
            }
            else if (numberOfColorsInBottle == 2)
            {
                if (bottleColors[1].Equals(bottleColors[0]))
                {
                    numberOfTopColorLayers = 2;
                }
            }

            rotationIndex = 3 - (numberOfColorsInBottle - numberOfTopColorLayers);
        }
    }

    public bool FillBottleCheck(Color colorToCheck)
    {
        if (numberOfColorsInBottle == 0)
        {
            return true;
        }

        if (numberOfColorsInBottle == 4)
        {
            return false;
        }

        return topColor.Equals(colorToCheck);
    }

    private void CalculateRotationIndex(int numberOfEmptySpacesInSecondBottle)
    {
        rotationIndex = 3 - (
            numberOfColorsInBottle -
            Mathf.Min(numberOfEmptySpacesInSecondBottle, numberOfTopColorLayers)
        );
    }

    private void FillUp(float fillAmountToAdd)
    {
        bottleMaskSR.material.SetFloat(
            "_FillAmount",
            bottleMaskSR.material.GetFloat("_FillAmount") + fillAmountToAdd
        );
    }

    private void ChoseRotationPointAndDirection()
    {
        if (transform.position.x > bottleControllerRef.transform.position.x)
        {
            chosenRotationPoint = leftRotationPoint;
            directionMultiplier = -1.0f;
        }
        else
        {
            chosenRotationPoint = rightRotationPoint;
            directionMultiplier = 1.0f;
        }
    }
}