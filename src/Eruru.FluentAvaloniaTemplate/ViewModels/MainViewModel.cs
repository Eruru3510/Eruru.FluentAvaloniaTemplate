using Microsoft.Extensions.DependencyInjection;
using Presentation;
using Presentation.Navigations;

namespace Eruru.FluentAvaloniaTemplate.ViewModels;

public partial class MainViewModel : ViewModelBase {

	public NavigationService NavigationService { get; } = PresentationApi.Services!.GetRequiredService<NavigationService> ();

}