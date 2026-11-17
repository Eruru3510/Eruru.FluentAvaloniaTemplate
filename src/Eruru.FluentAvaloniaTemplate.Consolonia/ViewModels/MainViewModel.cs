using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Presentation;
using Presentation.Navigations;

namespace Eruru.FluentAvaloniaTemplate.Consolonia.ViewModels;

public partial class MainViewModel : ObservableObject {

	public NavigationService NavigationService { get; set; } = PresentationApi.Services!.GetRequiredService<NavigationService> ();

}