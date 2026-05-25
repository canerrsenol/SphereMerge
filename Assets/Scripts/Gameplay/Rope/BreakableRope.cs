using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class BreakableRope : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RopeGenerator2D ropeGenerator;
    [SerializeField] private RopeLoadSensor2D loadSensor;
    [SerializeField] private RopeCapacityView capacityView;

    [Header("Capacity")]
    [Min(1)]
    [SerializeField] private int maxLoad = 6;
    [Min(0f)]
    [SerializeField] private float breakDelay = 1f;

    private Coroutine breakCountdown;
    private bool isBroken;

    private void Awake()
    {
        CacheReferences();
    }

    private void OnEnable()
    {
        CacheReferences();
        capacityView?.Show();

        if (ropeGenerator != null)
        {
            ropeGenerator.Broken += HandleRopeBroken;

            if (ropeGenerator.IsBroken)
            {
                HandleRopeBroken();
            }
        }

        if (loadSensor != null)
        {
            loadSensor.LoadChanged += HandleLoadChanged;
            HandleLoadChanged(loadSensor.LoadCount);
        }
        else
        {
            UpdateCapacityView(0);
        }
    }

    private void OnDisable()
    {
        if (loadSensor != null)
        {
            loadSensor.LoadChanged -= HandleLoadChanged;
        }

        if (ropeGenerator != null)
        {
            ropeGenerator.Broken -= HandleRopeBroken;
        }

        CancelBreakCountdown();
    }

    private void Reset()
    {
        CacheReferences();
    }

    private void OnValidate()
    {
        maxLoad = Mathf.Max(1, maxLoad);
        breakDelay = Mathf.Max(0f, breakDelay);
        CacheReferences();
        UpdateCapacityView(loadSensor != null ? loadSensor.LoadCount : 0);
    }

    private void HandleLoadChanged(int loadCount)
    {
        if (isBroken)
        {
            return;
        }

        UpdateCapacityView(loadCount);

        if (loadCount >= maxLoad)
        {
            BeginBreakCountdown();
        }
        else
        {
            CancelBreakCountdown();
        }
    }

    private void BeginBreakCountdown()
    {
        if (breakCountdown != null)
        {
            return;
        }

        if (breakDelay <= 0f)
        {
            BreakIfStillOverloaded();
            return;
        }

        breakCountdown = StartCoroutine(BreakAfterDelay());
    }

    private IEnumerator BreakAfterDelay()
    {
        yield return new WaitForSeconds(breakDelay);
        breakCountdown = null;
        BreakIfStillOverloaded();
    }

    private void BreakIfStillOverloaded()
    {
        if (isBroken || loadSensor == null || loadSensor.LoadCount < maxLoad || ropeGenerator == null)
        {
            return;
        }

        ropeGenerator.BreakFromMiddle();
    }

    private void HandleRopeBroken()
    {
        if (isBroken)
        {
            return;
        }

        isBroken = true;
        CancelBreakCountdown();
        capacityView?.Hide();
    }

    private void CancelBreakCountdown()
    {
        if (breakCountdown == null)
        {
            return;
        }

        StopCoroutine(breakCountdown);
        breakCountdown = null;
    }

    private void UpdateCapacityView(int loadCount)
    {
        capacityView?.SetRemainingCapacity(Mathf.Max(0, maxLoad - loadCount));
    }

    private void CacheReferences()
    {
        if (ropeGenerator == null)
        {
            ropeGenerator = GetComponent<RopeGenerator2D>();
        }

        if (loadSensor == null)
        {
            loadSensor = GetComponent<RopeLoadSensor2D>();
        }

        if (capacityView == null)
        {
            capacityView = GetComponentInChildren<RopeCapacityView>(true);
        }
    }
}
