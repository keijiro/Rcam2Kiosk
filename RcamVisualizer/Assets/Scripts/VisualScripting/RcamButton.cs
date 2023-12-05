using Unity.VisualScripting;

namespace Rcam2 {

[UnitCategory("Rcam")]
public sealed class RcamButton : MachineEventUnit<EmptyEventArgs>
{
    protected override string hookName => EventHooks.Update;

    public new class Data : EventUnit<EmptyEventArgs>.Data { public bool? state; }

    public override IGraphElementData CreateData() => new Data();

    [DoNotSerialize, PortLabelHidden] public ValueInput Number { get; private set; }

    protected override void Definition()
    {
        base.Definition();
        Number = ValueInput(nameof(Number), 0);
    }

    bool GetButtonFromSystem(Flow flow)
      => Singletons.InputHandle.GetButton(flow.GetValue<int>(Number));

    protected override bool ShouldTrigger(Flow flow, EmptyEventArgs args)
    {
        var data = flow.stack.GetElementData<Data>(this);
        var state = GetButtonFromSystem(flow);
        if (data.state == state) return false;
        data.state = state;
        return state;
    }
}

} // namespace Rcam2
