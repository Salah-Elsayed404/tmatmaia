namespace Tmatmaia.Core;

public enum Phase { Focus, ShortBreak, LongBreak }

public sealed class PomodoroEngine
{
    private DateTime _endUtc;
    private TimeSpan _remaining;

    public PomodoroEngine(Settings settings)
    {
        Settings = settings;
        _remaining = DurationOf(Phase.Focus);
    }

    public Settings Settings { get; set; }
    public Phase Phase { get; private set; } = Phase.Focus;
    public bool Running { get; private set; }
    public int Completed { get; private set; }

    public event Action? Changed;
    public event Action<Phase>? PhaseCompleted;

    public TimeSpan Total => DurationOf(Phase);

    public TimeSpan Remaining => Running ? Max(_endUtc - DateTime.UtcNow, TimeSpan.Zero) : _remaining;

    public double Progress => Total.TotalSeconds <= 0 ? 0 : 1 - Remaining.TotalSeconds / Total.TotalSeconds;

    public void Toggle()
    {
        if (Running)
        {
            _remaining = Remaining;
            Running = false;
        }
        else
        {
            _endUtc = DateTime.UtcNow + _remaining;
            Running = true;
        }
        Changed?.Invoke();
    }

    public void Reset()
    {
        Running = false;
        _remaining = Total;
        Changed?.Invoke();
    }

    public void Skip() => Advance(raiseCompleted: false);

    public void Tick()
    {
        if (Running && DateTime.UtcNow >= _endUtc) Advance(raiseCompleted: true);
        else Changed?.Invoke();
    }

    public void ApplySettings(Settings settings)
    {
        Settings = settings;
        if (!Running) _remaining = Total;
        Changed?.Invoke();
    }

    private void Advance(bool raiseCompleted)
    {
        var finished = Phase;
        if (finished == Phase.Focus)
        {
            Completed++;
            Phase = Completed % Math.Max(1, Settings.LongBreakEvery) == 0 ? Phase.LongBreak : Phase.ShortBreak;
        }
        else
        {
            Phase = Phase.Focus;
        }

        _remaining = Total;
        Running = raiseCompleted && Settings.AutoStartNext;
        if (Running) _endUtc = DateTime.UtcNow + _remaining;

        if (raiseCompleted) PhaseCompleted?.Invoke(finished);
        Changed?.Invoke();
    }

    private TimeSpan DurationOf(Phase phase) => TimeSpan.FromMinutes(phase switch
    {
        Phase.Focus => Settings.FocusMinutes,
        Phase.ShortBreak => Settings.ShortBreakMinutes,
        _ => Settings.LongBreakMinutes,
    });

    private static TimeSpan Max(TimeSpan a, TimeSpan b) => a > b ? a : b;
}
