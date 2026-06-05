using Farmacontrol.Desktop.States;
using Microsoft.Extensions.DependencyInjection;

namespace Farmacontrol.Desktop.Views;

public class RootView() : ViewBase<RootState>(Program.ServiceProvider.GetRequiredService<RootState>())
{
    protected override object Build(RootState state)
    {
        return new ContentControl()
            .Content(state, s => s.CurrentContent, BindingMode.OneWay);
    }
}