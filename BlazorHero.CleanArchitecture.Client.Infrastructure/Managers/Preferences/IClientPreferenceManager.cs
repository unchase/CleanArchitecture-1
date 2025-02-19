﻿using BlazorHero.CleanArchitecture.Infrastructure.Interfaces.Managers;
using MudBlazor;
using System.Threading.Tasks;

namespace BlazorHero.CleanArchitecture.Client.Infrastructure.Managers.Preferences
{
    public interface IClientPreferenceManager : IPreferenceManager
    {
        Task<MudTheme> GetCurrentThemeAsync();

        Task<bool> ToggleDarkModeAsync();

        Task ChangeLanguageAsync(string languageCode);
    }
}