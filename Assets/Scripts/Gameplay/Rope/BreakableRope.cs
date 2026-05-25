using System.Collections;
using UnityEngine;

// Breaks a rope when too many selected spheres stay on it.
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

    // Finds the rope components used by this behaviour.
    private void Awake()
    {
        CacheReferences();
    }

    // Starts tracking rope load and break events.
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

    // Stops event tracking and any pending break countdown.
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

    // Refreshes component references when this component is added.
    private void Reset()
    {
        CacheReferences();
    }

    // Keeps inspector values valid and refreshes the preview text.
    private void OnValidate()
    {
        maxLoad = Mathf.Max(1, maxLoad);
        breakDelay = Mathf.Max(0f, breakDelay);
        CacheReferences();
        UpdateCapacityView(loadSensor != null ? loadSensor.LoadCount : 0);
    }

    // Updates capacity feedback and starts breaking when overloaded.
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

    // Starts the delay before the overloaded rope breaks.
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

    // Waits before checking whether the rope should still break.
    private IEnumerator BreakAfterDelay()
    {
        yield return new WaitForSeconds(breakDelay);
        breakCountdown = null;
        BreakIfStillOverloaded();
    }

    // Breaks the rope only when its load is still over the limit.
    private void BreakIfStillOverloaded()
    {
        if (isBroken || loadSensor == null || loadSensor.LoadCount < maxLoad || ropeGenerator == null)
        {
            return;
        }

        ropeGenerator.BreakFromMiddle();
    }

    // Marks this object broken and hides its capacity view.
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

    // Cancels a pending rope break delay.
    private void CancelBreakCountdown()
    {
        if (breakCountdown == null)
        {
            return;
        }

        StopCoroutine(breakCountdown);
        breakCountdown = null;
    }

    // Displays how much more load the rope can carry.
    private void UpdateCapacityView(int loadCount)
    {
        capacityView?.SetRemainingCapacity(Mathf.Max(0, maxLoad - loadCount));
    }

    // Finds related rope components when they are not assigned.
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
