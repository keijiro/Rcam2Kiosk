using Unity.VisualScripting;

namespace Rcam2 {

[UnitCategory("Rcam")]
public sealed class RcamToggle : MachineEventUnit<EmptyEventArgs>
{
    protected override string hookName => EventHooks.Update;

    public new class Data : EventUnit<EmptyEventArgs>.Data { public bool? state; }

    public override IGraphElementData CreateData() => new Data();

    [DoNotSerialize, PortLabelHidden] public ValueInput Number { get; private set; }
    [DoNotSerialize, PortLabelHidden] public ValueOutput Value { get; private set; }

    protected override void Definition()
    {
        base.Definition();
        Number = ValueInput(nameof(Number), 0);
        Value = ValueOutput<bool>(nameof(Value));
    }

    bool GetToggleFromSystem(Flow flow)
      => Singletons.InputHandle.GetToggle(flow.GetValue<int>(Number));

    protected override void AssignArguments(Flow flow, EmptyEventArgs args)
      => flow.SetValue(Value, GetToggleFromSystem(flow));

    protected override bool ShouldTrigger(Flow flow, EmptyEventArgs args)
    {
        var data = flow.stack.GetElementData<Data>(this);
        var state = GetToggleFromSystem(flow);
        if (data.state == state) return false;
        data.state = state;
        return true;
    }
}

} // namespace Rcam2
